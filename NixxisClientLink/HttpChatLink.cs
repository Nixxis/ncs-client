using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;
using ContactRoute.Client;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Specialized; 

namespace Nixxis.Client
{
    public delegate void ChatLinkEventHandler(string eventType, int lineType, string from, string to, string message);

    internal class HttpChatLink : IDisposable
    {
        internal class RequestData
        {
            private HttpChatLink m_Link;
            private Uri m_Uri;
            private byte[] m_PostData;

            public RequestData(HttpChatLink link, Uri uri)
            {
                m_Link = link;
                m_Uri = uri;
                m_PostData = null;
            }

            public RequestData(HttpChatLink link, Uri uri, byte[] postData)
            {
                m_Link = link;
                m_Uri = uri;
                m_PostData = postData;
            }

            public byte[] PostData
            {
                get
                {
                    return m_PostData;
                }
                set
                {
                    m_PostData = value;
                }
            }

            public void Exec(ContactRoute.Client.IHttpNetworkClient clientSocket)
            {
                if (PostData == null)
                {
                    clientSocket.Get(m_Uri.PathAndQuery);
                }
                else
                {
                    clientSocket.Post(m_Uri.PathAndQuery, m_PostData, 0, m_PostData.Length, "application/octet-stream");
                }
            }

        }

        private IHttpNetworkClient m_NetworkClient;
        private bool m_Disposed;
        private Uri m_BaseUri;
        private string m_UserAgent;
        private ICredentials m_Credentials;
        private CookieContainer m_CookieContainer;
        private string m_SessionId = null;
        private SynchronizationContext m_SyncContext;
        private IPAddress[] m_DnsServers;
        private string m_AgentId;
        private bool m_EventThreadRunning;
        private Thread m_EventThread;
        private List<ChatConversation> m_Conversations = new List<ChatConversation>();

        public event ChatLinkEventHandler EventHandler;

		public HttpChatLink(string baseUriString, string agentId)
		{
			Initialize(typeof(HttpNetworkClient), baseUriString, agentId, null, null);
		} 

        public HttpChatLink(Type linkNetworkType, string baseUriString, string agentId, SynchronizationContext syncContext, System.Collections.Specialized.StringCollection forceDnsServers)
		{
            string[] DnsServers = null;

            if (forceDnsServers != null)
            {
                DnsServers = new string[forceDnsServers.Count];
                forceDnsServers.CopyTo(DnsServers, 0);
            }

            Initialize(linkNetworkType, baseUriString, agentId, syncContext, DnsServers);
		}

		public HttpChatLink(string baseUriString, string agentId, System.Collections.Specialized.StringCollection forceDnsServers)
		{
			string[] DnsServers = new string[forceDnsServers.Count];

			forceDnsServers.CopyTo(DnsServers, 0);

            Initialize(typeof(HttpNetworkClient), baseUriString, agentId, null, DnsServers);
		}

        public HttpChatLink(string baseUriString, string agentId, SynchronizationContext syncContext, string[] forceDnsServers)
		{
            Initialize(typeof(HttpNetworkClient), baseUriString, agentId, syncContext, forceDnsServers);
		}

        public HttpChatLink(string baseUriString, string agentId, string[] forceDnsServers)
		{
            Initialize(typeof(HttpNetworkClient), baseUriString, agentId, null, forceDnsServers);
		}

        public HttpChatLink(string baseUriString, string agentId, SynchronizationContext syncContext)
		{
            Initialize(typeof(HttpNetworkClient), baseUriString, agentId, syncContext, null);
		}

        ~HttpChatLink()
        {
            Dispose();
        }

        private void Initialize(Type linkNetworkType, string baseUriString, string agentId, SynchronizationContext syncContext, string[] forceDnsServers)
		{
            string RootUri = baseUriString.Substring(0, baseUriString.IndexOf('?'));

			m_SyncContext = syncContext;
            m_AgentId = agentId;

			if (forceDnsServers != null && forceDnsServers.Length > 0)
			{
				m_DnsServers = new IPAddress[forceDnsServers.Length];

				for (int i = 0; i < forceDnsServers.Length; i++)
					m_DnsServers[i] = IPAddress.Parse(forceDnsServers[i]);
			}
			else
			{
				m_DnsServers = null;
			}

			m_BaseUri = new Uri(RootUri);
			m_CookieContainer = new CookieContainer(32, 32, 512);
			m_UserAgent = string.Join("; ", new string[] { string.Format("Nixxis/HttpLink.{0}", Assembly.GetExecutingAssembly().FullName.Substring(Assembly.GetExecutingAssembly().FullName.IndexOf("Version=", StringComparison.OrdinalIgnoreCase) + 8, 3)), Assembly.GetExecutingAssembly().FullName, Assembly.GetEntryAssembly().FullName });

            m_NetworkClient = Activator.CreateInstance(linkNetworkType, m_BaseUri) as IHttpNetworkClient;

            m_EventThread = new Thread(new ThreadStart(EventLoop));
            m_EventThread.IsBackground = true;
            m_EventThread.Name = "Chat event polling";
            m_EventThread.Priority = ThreadPriority.BelowNormal;

            m_EventThread.Start();

            for (int i = 0; i < 30; i++)
            {
                if (m_EventThreadRunning)
                    break;

                Thread.Sleep(100);
            }
		}

        internal RequestData BuildRequest(string action, string conversationId)
        {
            return BuildRequest(action, conversationId, null, null);
        }

        internal RequestData BuildRequest(string action, string conversationId, string[] parameters)
        {
            return BuildRequest(action, conversationId, parameters, null);
        }

        internal RequestData BuildRequest(string action, string conversationId, string[] parameters, string postData)
        {
            RequestData Request;

            if (parameters != null && parameters.Length > 0)
            {
                StringBuilder SB = new StringBuilder();

                SB.Append("?conversationid=").Append(conversationId).Append("&action=").Append(action);

                for (int i = 0; i < parameters.Length; i++)
                {
                    SB.Append("&p").Append(i + 1).Append(parameters[i]);
                }

                Request = new RequestData(this, new Uri(m_BaseUri, SB.ToString()));
            }
            else
            {
                Request = new RequestData(this, new Uri(m_BaseUri, string.Format("?action={0}", action)));
            }

            if (!string.IsNullOrEmpty(postData))
            {
                Request.PostData = Encoding.UTF8.GetBytes(postData);
            }

            return Request;
        }

        private void FireEvents(object state)
        {
            ChatConversation Conversation = (ChatConversation)((object[])state)[0];
            string[] ResponseLines = (string[])((object[])state)[1];

            foreach (string ResponseLine in ResponseLines)
            {
                string[] LineParts = ResponseLine.Split(new char[] { ':' }, 2);
                string[] Params;
                int ReceiveId = -1;

                if (LineParts.Length > 1)
                {
                    Params = LineParts[1].Split(ProtocolOptions.EscapeSeparator);

                    for (int i = 0; i < Params.Length; i++)
                        Params[i] = Microsoft.JScript.GlobalObject.unescape(Params[i]);

                    if (!int.TryParse(Params[0], out ReceiveId))
                        ReceiveId = -1;
                }
                else
                {
                    Params = new string[0];
                }

                if (ReceiveId >= 0 && ReceiveId > Conversation.LastReceive)
                {
                    try
                    {
                        if (Conversation != null)
                            Conversation.LinkEventHandler(LineParts[0], int.Parse(Params[1]), Params[2], Params[3], Params[4]);

                        if(EventHandler != null)
                            EventHandler(LineParts[0], int.Parse(Params[1]), Params[2], Params[3], Params[4]);
                    }
                    catch
                    {
                    }

                    Conversation.LastReceive = ReceiveId;
                }
            }
        }

        internal ResponseData GetResponseData(RequestData request, string conversationId)
        {
            ResponseData Data = null;
            string ResponseBody;
            int Retry = 3;
            ChatConversation Conversation = null;

            if (!string.IsNullOrEmpty(conversationId))
            {
                lock (m_EventThread)
                {
                    Conversation = m_Conversations.Find(Conv => Conv.Id == conversationId);
                }
            }

            while (Data == null)
            {
                ResponseBody = string.Empty;

                try
                {
                    lock (m_NetworkClient)
                    {
                        request.Exec(m_NetworkClient);

                        Data = new ResponseData(m_NetworkClient.StatusCode);

                        if (m_NetworkClient.ContentLength > 0)
                        {
                            ResponseBody = Encoding.UTF8.GetString(m_NetworkClient.RawData, 0, m_NetworkClient.ContentLength);
                        }
                    }

                    if (Data != null)
                    {
                        if (Data.StatusCode == (int)HttpStatusCode.Forbidden)
                        {
                            m_SessionId = null;
                        }
                        else
                        {
                            if(m_SessionId == null)
                            {
                                try
                                {
                                    m_SessionId = m_NetworkClient.Cookies.GetCookies(m_BaseUri)["contactroutehttpserversessionid"].Value;
                                }
                                catch(Exception Ex)
                                {
                                    System.Diagnostics.Trace.WriteLine(Ex.ToString());
                                }
                            }

                            if (Conversation != null && (Data.Valid || Data.StatusCode == 301))
                            {
                                string[] ResponseLines = ResponseBody.Split(ProtocolOptions.LineSeparator, StringSplitOptions.RemoveEmptyEntries);

                                if (m_SyncContext != null)
                                {
                                    m_SyncContext.Send(FireEvents, new object[] { Conversation, ResponseLines });
                                }
                                else
                                {
                                    FireEvents(new object[] { Conversation, ResponseLines });
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(ResponseBody))
                                    Data.Add("~error", ResponseBody);
                            }
                        }
#if DEBUG
                        Debug.WriteLine(ResponseBody, Data.StatusCode.ToString());
#endif
                    }

                    break;
                }
                catch (Exception Ex)
                {
                    Debug.WriteLine(Ex.ToString());

                    if (Retry-- > 0)
                        continue;

                    break;
                }
            }

            if (Data != null)
            {
                if (Data.StatusCode == (int)HttpStatusCode.Forbidden)
                {
                    m_SessionId = null;
                }
                else if (Data.Valid)
                {
                }
            }

            return Data;
        }

        public bool Join(ChatConversation conversation)
        {
            lock (m_EventThread)
            {
                RequestData Request = new RequestData(this, new Uri(m_BaseUri, string.Concat("?conversationid=", conversation.Id, "&action=join&history=0&agent=", m_AgentId)));

                ResponseData Data = GetResponseData(Request, conversation.Id);

                if (Data != null && (Data.Valid || Data.StatusCode == 301))
                {
                    m_Conversations.Add(conversation);
                    return true;
                }

                return false;
            }
        }

        public bool Leave(ChatConversation conversation, string message)
        {
            lock (m_EventThread)
            {
                if (m_Conversations.Remove(conversation))
                {
                    RequestData Request = new RequestData(this, new Uri(m_BaseUri, string.Concat("?conversationid=", conversation.Id, "&action=leave&fmt=uri&history=", conversation.LastReceive.ToString())), Encoding.UTF8.GetBytes(string.Concat("say:", conversation.LastTransmit.ToString(), ":", Microsoft.JScript.GlobalObject.escape(message))));
                    ResponseData Data = GetResponseData(Request, conversation.Id);

                    if (Data != null && (Data.Valid || Data.StatusCode == 301))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void Close(ChatConversation conversation)
        {
            lock (m_EventThread)
            {
                m_Conversations.Remove(conversation);
            }
        }

        public bool Say(ChatConversation conversation, string message)
        {
            lock (m_EventThread)
            {
                conversation.LastTransmit++;

                RequestData Request = new RequestData(this, new Uri(m_BaseUri, string.Concat("?conversationid=", conversation.Id, "&action=say&fmt=uri&history=", conversation.LastReceive.ToString())), Encoding.UTF8.GetBytes(string.Concat("say:", conversation.LastTransmit.ToString(), ":", Microsoft.JScript.GlobalObject.escape(message))));

                GetResponseData(Request, conversation.Id);
                return false;
            }
        }

        public bool Reset(ChatConversation conversation)
        {
            lock (m_EventThread)
            {
                conversation.LastTransmit++;

                RequestData Request = new RequestData(this, new Uri(m_BaseUri, string.Concat("?conversationid=", conversation.Id, "&action=say&history=-1")), Encoding.UTF8.GetBytes(string.Concat("reset:", conversation.LastTransmit.ToString(), ":")));

                GetResponseData(Request, conversation.Id);
                return false;
            }
        }

        private void EventLoop()
        {
            int LastConversation = -1;
            int LoopCount = 0;

            m_EventThreadRunning = true;

            while (m_EventThreadRunning)
            {
                Thread.Sleep(2000);

                LoopCount++;

                ChatConversation Conversation = null;

                lock (m_EventThread)
                {
                    if (m_Conversations.Count > 0)
                    {
                        LastConversation++;

                        if (LastConversation >= m_Conversations.Count)
                        {
                            LastConversation = -1;
                            continue;
                        }

                        Conversation = m_Conversations[LastConversation];
                    }
                    else
                    {
                        m_SessionId = null;
                        m_NetworkClient.ClearCookies();
                    }

                }


                if (Conversation != null)
                {
                    if (Conversation.LastTransmit == Conversation.LastKnownTransmit)
                    {
                        try
                        {
                            //TODO: send list of convIds

                            string ConversationId = m_Conversations[LastConversation].Id;
                            RequestData Request = new RequestData(this, new Uri(m_BaseUri, string.Concat("?conversationid=", ConversationId, "&action=say&history=", Conversation.LastReceive.ToString())));

                            GetResponseData(Request, ConversationId);
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        Conversation.LastKnownTransmit = Conversation.LastTransmit;
                    }
                }
            }

            m_EventThread = null;
        }


        #region IDisposable Members

        public void Dispose()
        {
            if (!m_Disposed)
            {
                m_Disposed = true;

                if (m_EventThread != null)
                {
                    lock (m_EventThread)
                    {
                        m_EventThreadRunning = false;

                        for (int i = 0; i < 30; i++)
                        {
                            if (m_EventThread == null)
                                break;

                            Thread.Sleep(100);
                        }
                    }
                }

                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }

	public enum ChatEventType
	{
		Enter,
		Leave,
		Say,
		Whisper,
		Activate,
		Wait,
		Hold,
		Retrieve
	}

    public class ChatLine
    {
        int m_LineType;
        int m_Flags;
        int m_EventType;

        public string From { get; internal set; }
        public string To { get; internal set; }
        public DateTime Time { get; internal set; }
        public string Message { get; internal set; }

        public int LineType 
        { 
            get
            {
                return m_LineType;
            }
            internal set
            {
                m_LineType = value;

                m_EventType = (m_LineType & 0xFF);
                m_Flags = m_LineType >> 8;
            }
        }

        public ChatEventType EventType 
        { 
            get
            {
                return (ChatEventType)m_EventType;
            }
        }

        public int Flags
        {
            get
            {
                return m_Flags;
            }
        }

        public bool FromMe 
        {
            get
            {
                return (m_Flags & 0x01) != 0;
            }
        }

        public bool FromAgent
        {
            get
            {
                return (m_Flags & 0x02) != 0;
            }
        }

        public bool ToMe
        {
            get
            {
                return (m_Flags & 0x04) != 0;
            }
        }

        public bool ToAgent
        {
            get
            {
                return (m_Flags & 0x08) != 0;
            }
        }
    }

    public class ChatConversation : IEnumerable<ChatLine>, INotifyCollectionChanged
    {
        static SortedList<string, HttpChatLink> m_Links = new SortedList<string,HttpChatLink>();

        HttpChatLink m_Link;
        string m_ConversationId;

        List<ChatLine> m_History = new List<ChatLine>();
        
        internal int LastTransmit = 0;
        internal int LastKnownTransmit = 0;
        internal int LastReceive = -1;

        public event ChatLinkEventHandler EventHandler;

		public ChatConversation(string baseUriString, string conversationId, string agentId)
		{
            Initialize(null, baseUriString, conversationId, agentId, SynchronizationContext.Current);
		}

        public ChatConversation(string baseUriString, string conversationId, string agentId, SynchronizationContext syncContext)
		{
            Initialize(null, baseUriString, conversationId, agentId, syncContext);
		}

        public ChatConversation(Type linkNetworkType, string baseUriString, string conversationId, string agentId)
        {
            Initialize(linkNetworkType, baseUriString, conversationId, agentId, SynchronizationContext.Current);
        }

        public ChatConversation(Type linkNetworkType, string baseUriString, string conversationId, string agentId, SynchronizationContext syncContext)
        {
            Initialize(linkNetworkType, baseUriString, conversationId, agentId, syncContext);
        }

        private void Initialize(Type linkNetworkType, string baseUriString, string conversationId, string agentId, SynchronizationContext syncContext)
        {
            m_ConversationId = conversationId;

            lock (typeof(ChatConversation))
            {
                if (m_Links.ContainsKey(baseUriString))
                {
                    m_Link = m_Links[baseUriString];
                }
                else
                {
                    m_Link = new HttpChatLink(linkNetworkType, baseUriString, agentId, syncContext, null);
                    m_Links.Add(baseUriString, m_Link);
                }
            }
        }

        internal void LinkEventHandler(string eventType, int lineType, string from, string to, string message)
        {
            ChatLine Line = new ChatLine { Time = DateTime.Now, LineType = lineType, From = from, To = to, Message = message };

            if (Line.EventType != ChatEventType.Activate && Line.EventType != ChatEventType.Wait)
            {
                m_History.Add(Line);

                if (CollectionChanged != null)
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Line));

                if (EventHandler != null)
                    EventHandler(eventType, lineType, from, to, message);
            }
        }

        public string Id
        {
            get
            {
                return m_ConversationId;
            }
        }

        public bool Join()
        {
            return m_Link.Join(this);
        }

        public bool Leave(string message)
        {
            return m_Link.Leave(this, message);
        }

        public bool Say(string message)
        {
            return m_Link.Say(this, message);
        }

        public bool Reset()
        {
            return m_Link.Reset(this);
        }

        public void Close()
        {
            m_Link.Close(this);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<ChatLine> GetEnumerator()
        {
            return m_History.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_History.GetEnumerator();
        }
    }
}
