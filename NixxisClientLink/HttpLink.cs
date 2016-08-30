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
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections.Specialized;
using ContactRoute; 

namespace Nixxis.Client
{
	[Flags] public enum SupervisionElements
	{
		None = 0,
		Agent = 1,
		Queue = 2,
		Team = 4,
		Activity = 8,
		Campaign = 16,
		System = 32,

		RealTime = 0xFF0001,
		History = 0xFF0002,
		PeakValues = 0xFF0004,
		Production = 0xFF0008,
		PeriodProduction = 0xFF0010,

		All = 0xFFFFFF
	}

    [Flags]
    public enum MessageDestinations
    {
        None = 0,
        Supervisors = 1,
        Team = 2,
        Agent = 4,
        Queue = 8,
        Campaign = 16
    }

    public enum MessageType
    {
        Default,
        HelpRequest,
        Warning,
        Alert,
        Chat,
        ChatEnd
    }


	public struct ServerEventArgs
	{
		public HttpLink Link;
		public EventCodes EventCode;
		public string Parameters;
		public bool Cancel;

		internal ServerEventArgs(HttpLink link, EventCodes eventCode)
		{
			Link = link;
			EventCode = eventCode;
			Parameters = null;
			Cancel = false;
		}
	}

	public struct ContactEventArgs
	{
		public HttpLink Link;
		public EventCodes EventCode;
		public string ContactId;
		public ContactInfo Contact;

		internal ContactEventArgs(HttpLink link, EventCodes eventCode, ContactInfo contact)
		{
			Link = link;
			EventCode = eventCode;
			ContactId = contact.Id;
			Contact = contact;
		}
	}
	
	public class ContactsList : IList<ContactInfo>, INotifyCollectionChanged, INotifyPropertyChanged
	{
		private HttpLink m_Link;
		private string m_ActiveContactId;
        private List<ContactInfo> m_List = new List<ContactInfo>();

        private ContactsList()
		{
		}

		internal ContactsList(HttpLink link)
		{
			m_Link = link;
        }

		public ContactInfo this[string id]
		{
			get
			{
				ContactInfo CInfo = null;

				if (Monitor.TryEnter(this, 5000))
				{
					try
					{
						for (int i = 0; i < this.Count; i++)
						{
							if (this[i].Id == id)
							{
								CInfo = this[i];
								break;
							}
						}
					}
					finally
					{
						Monitor.Exit(this);
					}
				}
				else
				{
					if (Debugger.IsAttached)
                        ContactRoute.DiagnosticHelpers.DebugIfPossible();

				}
			
				return CInfo;
			}
		}

		public string ActiveContactId
		{
			get
			{
				return m_ActiveContactId;
			}
			set
			{
				string Value = string.Empty;

				if (value != null)
					Value = value;

				if (m_ActiveContactId != Value)
				{
					m_ActiveContactId = Value;
					m_Link.SetActiveContact(m_ActiveContactId);

                    OnPropertyChanged(new PropertyChangedEventArgs("ActiveContactId"));
                    OnPropertyChanged(new PropertyChangedEventArgs("ActiveContact"));
                }
			}
		}

		public ContactInfo ActiveContact
		{
			get
			{
				lock (this)
				{
					for (int i = 0; i < this.Count; i++)
					{
						if (this[i].Id == m_ActiveContactId)
							return this[i];
					}
				}

				return null;
			}
			set
			{
				string Value = string.Empty;

				if (value != null)
					Value = value.Id;

				if (m_ActiveContactId != Value)
				{
					m_ActiveContactId = Value;
					m_Link.SetActiveContact(m_ActiveContactId);

                    OnPropertyChanged(new PropertyChangedEventArgs("ActiveContactId"));
                    OnPropertyChanged(new PropertyChangedEventArgs("ActiveContact"));
                  
                }
			}
		}

        public void ForceActiveContact(ContactInfo contact)
        {
            string Value = string.Empty;

            if (contact != null)
                Value = contact.Id;

            m_ActiveContactId = Value;
            m_Link.SetActiveContact(m_ActiveContactId);

            OnPropertyChanged(new PropertyChangedEventArgs("ActiveContactId"));
            OnPropertyChanged(new PropertyChangedEventArgs("ActiveContact"));
        }

        public int IndexOf(ContactInfo item)
        {
            return m_List.IndexOf(item);
        }

        public void Insert(int index, ContactInfo item)
        {
            m_List.Insert(index, item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        }

        public void RemoveAt(int index)
        {
            ContactInfo OldItem = m_List[index];
            m_List.RemoveAt(index);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, OldItem));
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        }

        public ContactInfo this[int index]
        {
            get
            {
                return m_List[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(ContactInfo item)
        {
            m_List.Add(item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        }

        public void Clear()
        {
            if(m_List.Count > 0)
            {
                m_List.Clear();

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            }
        }

        public bool Contains(ContactInfo item)
        {
            return m_List.Contains(item);
        }

        public void CopyTo(ContactInfo[] array, int arrayIndex)
        {
            m_List.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return m_List.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(ContactInfo item)
        {
            bool Result = m_List.Remove(item);

            if (Result)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            }

            return Result;
        }

        public IEnumerator<ContactInfo> GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if(CollectionChanged != null)
                m_Link.Invoke(CollectionChanged, this, args);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
                m_Link.Invoke(PropertyChanged, this, args);
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }

	public class ClientState : INotifyPropertyChanged
	{
		public class ClientMediaState
		{
			public static ClientMediaState operator & (ClientMediaState x, ClientMediaState y)
			{
				return new ClientMediaState(x.m_Available && y.m_Available);
			}

			public static ClientMediaState operator | (ClientMediaState x, ClientMediaState y)
			{
				return new ClientMediaState(x.m_Available || y.m_Available);
			}

			private bool m_Available;

			internal ClientMediaState(bool available)
			{
				m_Available = available;
			}

			internal ClientMediaState(ClientMediaState mediaState)
			{
				m_Available = mediaState.m_Available;
			}

			public bool Available
			{
				get
				{
					return m_Available;
				}
			}
		}

        private HttpLink m_Link;

		private ClientMediaState m_VoiceState;
		private ClientMediaState m_MailState;
		private ClientMediaState m_ChatState;

        private string m_LastAgentState = "";
        private DateTime m_LastAgentStateChange = DateTime.Now;
        private string m_LastAgentStateElapsed = "";

        SendOrPostCallback m_SendOrPostPropertyChanged;
        
        private ClientState()
        {
        }

        public void SetLastAgentState(string description, DateTime startTime)
        {
            if (m_LastAgentState != description)
            {
                m_LastAgentState = description;
                OnPropertyChanged(new PropertyChangedEventArgs("LastAgentState"));
            }

            if (m_LastAgentStateChange != startTime)
            {
                m_LastAgentStateChange = startTime;
                OnPropertyChanged(new PropertyChangedEventArgs("LastAgentStateChange"));
                OnPropertyChanged(new PropertyChangedEventArgs("LastAgentStateElapsed"));
            }
        }

        internal void TimerCallbackHandler(object state)
        {
            string Elapsed = DateTime.Now.Subtract(m_LastAgentStateChange).ToString("hh\\:mm\\:ss");

            if (m_LastAgentStateElapsed != Elapsed)
            {
                m_LastAgentStateElapsed = Elapsed;
                OnPropertyChanged(new PropertyChangedEventArgs("LastAgentStateElapsed"));
            }

        }

        public string LastAgentState
        {
            get
            {
                return m_LastAgentState;
            }
        }

        public DateTime LastAgentStateChange
        {
            get
            {
                return m_LastAgentStateChange;
            }
        }

        public string LastAgentStateElapsed
        {
            get
            {
                return m_LastAgentStateElapsed;
            }
        }

        internal ClientState(HttpLink link)
        {
            m_Link = link;
        }

		public ClientMediaState this[MediaTypes mediaType]
		{
			get
			{
				ClientMediaState State = new ClientMediaState(true);

				if ((mediaType & MediaTypes.Voice) != MediaTypes.None)
				{
					if (State == null)
						State = new ClientMediaState(m_VoiceState);
					else
						State &= m_VoiceState;
				}
				if ((mediaType & MediaTypes.Mail) != MediaTypes.None)
				{
					if (State == null)
						State = new ClientMediaState(m_MailState);
					else
						State &= m_MailState;
				}
				if ((mediaType & MediaTypes.Chat) != MediaTypes.None)
				{
					if (State == null)
						State = new ClientMediaState(m_ChatState);
					else
						State &= m_ChatState;
				}

				return State;
			}
		}

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
                m_Link.Invoke(PropertyChanged, this, args);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

	public class QueueInfo
	{
		private string m_QueueId;
		private int m_WaitingContacts;

		private QueueInfo()
		{
		}

		internal QueueInfo(string queueId)
		{
			m_QueueId = queueId;
		}

		internal bool Update(int waitingContacts)
		{
			bool Updated = false;

			if (m_WaitingContacts != waitingContacts)
			{
				m_WaitingContacts = waitingContacts;
				Updated = true;
			}

			return Updated;
		}

		public int WaitingContacts
		{
			get
			{
				return m_WaitingContacts;
			}
		}
	}

    public class QueueList : SortedList<string, QueueInfo>, INotifyPropertyChanged
	{
		private HttpLink m_Link;
		private int m_HighPriorityContacts;
		private int m_WaitingContacts;
        private string m_DialInfo;
        private bool m_IsTargetedCallback;
        private string m_DialInfoActivity;

		private QueueList()
		{
		}

		internal QueueList(HttpLink link)
		{
			m_Link = link;
		}

		internal bool UpdateHighPriorityContacts(int highPriorityContacts)
		{
			if (m_HighPriorityContacts == highPriorityContacts)
				return false;

			m_HighPriorityContacts = highPriorityContacts;
            FirePropertyChanged("HighPriorityContacts");

			return true;
		}

        internal void UpdateAgentDialInfo(string strNumber, string strActivity, string strDescription, bool isTargetedCallback)
        {
            m_DialInfoActivity = strActivity;
            if (string.IsNullOrEmpty(strDescription))
            {
                m_DialInfo = strNumber;
            }
            else
            {
                m_DialInfo = string.Format("{0}({1})", strDescription, strNumber);

            }
            m_IsTargetedCallback = isTargetedCallback;
            FirePropertyChanged("DialInfo");
            FirePropertyChanged("DialInfoIsTargetedCallback");
            FirePropertyChanged("DialInfoActivity");
        }

		internal bool UpdateWaitingContacts(int waitingContacts)
		{
			if (m_WaitingContacts == waitingContacts)
				return false;

			m_WaitingContacts = waitingContacts;
            FirePropertyChanged("WaitingContacts");

			return true;
		}

        public string DialInfo
        {
            get
            {
                return m_DialInfo;
            }
        }
        public bool DialInfoIsTargetedCallback
        {
            get
            {
                return m_IsTargetedCallback;
            }
        }
        public string DialInfoActivity
        {
            get
            {
                return m_DialInfoActivity;
            }
        }

		public int HighPriorityContacts
		{
			get
			{
				return m_HighPriorityContacts;
			}
		}

		public int WaitingContacts
		{
			get
			{
				return m_WaitingContacts;
			}
		}

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
	}

	[Flags] public enum MediaTypes : byte
	{
		None = 0,
		Voice = 1,
		Mail = 2,
		Chat = 4,
		Any = 255
	}

	public class ResponseData : SortedDictionary<string, string>
	{
		private static ResponseData m_Forbidden = new ResponseData((int)HttpStatusCode.Forbidden);

		public static ResponseData Forbidden
		{
			get
			{
				return m_Forbidden;
			}
		}

		private class ResponseKeyComparer : IComparer<string>
		{
			#region IComparer<string> Members

			public int Compare(string x, string y)
			{
				return string.CompareOrdinal(x, y);
			}

			#endregion
		}

		private static ResponseKeyComparer m_Comparer = new ResponseKeyComparer();

		protected int m_StatusCode;
		protected string m_RawResponse;

		private ResponseData()
		{
		}

		internal ResponseData(int statusCode)
			: this(statusCode, null)
		{
		}

		internal ResponseData(int statusCode, string rawResponse)
			: this(statusCode, rawResponse, false)
		{
		}

        internal ResponseData(int statusCode, string rawResponse, bool rawOnly)
            : base(m_Comparer)
        {
            m_StatusCode = statusCode;
            m_RawResponse = rawResponse;

            if (!rawOnly && !string.IsNullOrEmpty(rawResponse))
            {
                string[] ResponseLines = rawResponse.Split(ProtocolOptions.LineSeparator, StringSplitOptions.RemoveEmptyEntries);

                foreach (string ResponseLine in ResponseLines)
                {
                    string[] LineParts = ResponseLine.Split(ProtocolOptions.ValueSeparator, 2);

                    Add(LineParts[0], (LineParts.Length > 1) ? LineParts[1] : null);
                }
            }
        }

		public int StatusCode
		{
			get
			{
				return m_StatusCode;
			}
		}

		public string RawResponse
		{
			get
			{
				return m_RawResponse;
			}
		}

		public bool Valid
		{
			get
			{
				return (m_StatusCode == (int)HttpStatusCode.OK || m_StatusCode == (int)HttpStatusCode.NoContent);
			}
		}

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> keyval in this)
            {
                if (keyval.Value == null)
                {
                    sb.AppendLine(Microsoft.JScript.GlobalObject.unescape(keyval.Key));
                }
                else
                {
                    sb.AppendFormat("{0}={1}\n", Microsoft.JScript.GlobalObject.unescape(keyval.Key), Microsoft.JScript.GlobalObject.unescape(keyval.Value)); 
                }
            }
            return sb.ToString();
        }
	}


	public delegate void HttpLinkServerResponseDelegate(ResponseData data);
	public delegate void HttpLinkEventDelegate();
	public delegate void HttpLinkServerEventDelegate(ServerEventArgs eventArgs);
	public delegate void HttpLinkContactEventDelegate(ContactEventArgs eventArgs);

	public class ActivitySupervision
	{
		public string ActivityId;
		public string Description;
		public string RealTimeData;

		public ActivitySupervision(string activityId)
		{
			ActivityId = activityId;
		}
	}

	public class ActivitySupervisionCollection : KeyedCollection<string, ActivitySupervision>
	{
		protected override string GetKeyForItem(ActivitySupervision item)
		{
			return item.ActivityId;
		}
	}

	public class AgentRealtimeState
	{
		private HttpLink m_ClientLink;

		public string AgentId;
		public string Account;
		public string Description;
		public int State;
		public string StateDescription;
		public string VoiceStateDescription;
		public string ChatStateDescription;
		public string MailStateDescription;
		public bool VoiceAvailable;
		public bool ChatAvailable;
		public bool MailAvailable;

		public AgentRealtimeState(HttpLink clientLink, string agentId)
		{
			m_ClientLink = clientLink;
			AgentId = agentId;
		}

		public string GetPrivateInfo(string key)
		{
			ResponseData Data = m_ClientLink.GetResponseData(m_ClientLink.BuildRequest("~getinfo&fmt=uri", null, string.Concat(((int)InfoCodes.AgentProperty).ToString(), "\r\n", AgentId, "\r\n", key)));

			if (Data != null && Data.Valid)
			{
			}

			return null;
		}

		public void SetPrivateInfo(string key, string value)
		{
			ResponseData Data = m_ClientLink.GetResponseData(m_ClientLink.BuildRequest("~setinfo&fmt=uri", null, string.Concat(((int)InfoCodes.AgentProperty).ToString(), "\r\n", AgentId, "\r\n", key, "\r\n", Microsoft.JScript.GlobalObject.escape(value))));
		}
	}

	public class AgentRealtimeStateCollection : KeyedCollection<string, AgentRealtimeState>
	{
		protected override string GetKeyForItem(AgentRealtimeState item)
		{
			return item.AgentId;
		}
	}

	public class HttpLink : IDisposable, IAgentInfo
	{
        public static string UrlInsertSessionId(string url, string sessionId)
        {
            if (string.IsNullOrEmpty(url))
                return url;
            else
            {
                try
                {
                    return url.Substring(0, url.IndexOf("/", url.IndexOf("//") + 2) + 1) + '~' + sessionId + url.Substring(url.IndexOf("/", url.IndexOf("//") + 2));
                }
                catch { return string.Empty; }
            }
        }

		internal class RequestData
		{
			private HttpLink m_Link;
			private Uri m_Uri;
			private byte[] m_PostData;

			public RequestData(HttpLink link, Uri uri)
			{
				m_Link = link;
				m_Uri = uri;
				m_PostData = null;
			}

			public RequestData(HttpLink link, Uri uri, byte[] postData)
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
				Exec(clientSocket, false);
			}

			public void Exec(ContactRoute.Client.IHttpNetworkClient clientSocket, bool trace)
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

        private class InvokeData
        {
            public Delegate Method;
            public object[] Args;
        }

        private void DelayedInvoke(object state)
        {
            InvokeData ID = (InvokeData)state;

            ID.Method.DynamicInvoke(ID.Args);
        }

        public bool TraceInvokes { get; set; }

        internal void Invoke(Delegate method, params object[] args)
        {
            if (method != null)
            {
                if (TraceInvokes)
                {
                    try
                    {
                        Debug.WriteLine("Invoke " + ((method != null && method.Method != null) ? method.Method.Name : "")
                            + " on " + ((method != null && method.Target != null) ? method.Target.GetType().Name : "(null)") + " using "
                            + ((SyncContext == null || SyncContext.Equals(SynchronizationContext.Current)) ? ((m_EventControl != null && m_EventControl.InvokeRequired) ? "Control invoke" : "Dynamic invoke") : "SyncContext Post"),
                            "TraceInvokes");
                    }
                    catch
                    {
                    }
                }

                if (SyncContext == null || SyncContext.Equals(SynchronizationContext.Current))
                {
                    if (m_EventControl != null && m_EventControl.InvokeRequired)
                    {
                        m_EventControl.Invoke(method, args);
                    }
                    else
                    {
                        method.DynamicInvoke(args);
                    }
                }
                else
                {
                    SyncContext.Post(new System.Threading.SendOrPostCallback(DelayedInvoke), new InvokeData { Method = method, Args = args });
                }
            }
        }

        internal void InvokeDirect(Delegate method, params object[] args)
        {
            if (method != null)
            {
                if (TraceInvokes)
                {
                    try
                    {
                        Debug.WriteLine("Invoke " + ((method != null && method.Method != null) ? method.Method.Name : "")
                            + " on " + ((method != null && method.Target != null) ? method.Target.GetType().Name : "(null)") + " using "
                            + ((SyncContext == null || SyncContext.Equals(SynchronizationContext.Current)) ? ((m_EventControl != null && m_EventControl.InvokeRequired) ? "Control invoke" : "Dynamic invoke") : "SyncContext Send"),
                            "TraceInvokes");
                    }
                    catch
                    {
                    }
                }

                if (SyncContext == null || SyncContext.Equals(SynchronizationContext.Current))
                {
                    if (m_EventControl != null && m_EventControl.InvokeRequired)
                    {
                        m_EventControl.Invoke(method, args);
                    }
                    else
                    {
                        method.DynamicInvoke(args);
                    }
                }
                else
                {
                    SyncContext.Send(new System.Threading.SendOrPostCallback(DelayedInvoke), new InvokeData { Method = method, Args = args });
                }
            }
        }

        public bool TraceNetwork
        {
            get { return m_TraceNetwork; }
            set
            {
                m_TraceNetwork = value;
                if (m_ClientSocket != null)
                    m_ClientSocket.TraceNetwork = m_TraceNetwork;
                if (m_EventSocket != null)
                    m_EventSocket.TraceNetwork = m_TraceNetwork;
            }
        }

        private bool m_ServerConnectionBroken = false;
		private bool m_Disposed;
		private Uri m_BaseUri;
		private Uri m_EventUri;
		private string m_UserAgent;
		private Thread m_EventThread;
		private bool m_TraceNetwork = false;
		private ICredentials m_Credentials;
		private HttpWebRequest m_EventRequest;
		private CookieContainer m_CookieContainer;
		private string m_SessionId = null;
		private string m_Account = string.Empty;
		private string m_AgentId = null;
		private string m_Name = string.Empty;
		private string m_Description = string.Empty;
		private ClientRoles m_ClientRoles;
		private int m_AutoReady = -1;
		private bool m_AllowDialOnBehalf = false;
		private Control m_EventControl;
		private string[] m_ManualCallList = new string[0];
        private string[] m_WellknownChatDestinations = new string[0];
        private string[] m_WellknownMailDestinations = new string[0];
		private string[] m_StateSelection = new string[0];
		private TeamCollection m_Teams = new TeamCollection();
		private PauseCodeCollection m_PauseCodes = new PauseCodeCollection();
		private ActivitySupervisionCollection m_ActivitiesSupervision = new ActivitySupervisionCollection();
		private AgentRealtimeStateCollection m_AgentsRealtimeState = new AgentRealtimeStateCollection();
		private System.Collections.Specialized.NameValueCollection m_ClientSettings = new System.Collections.Specialized.NameValueCollection();
		private ActivityCollection m_Activities = new ActivityCollection();
        private DateTimeFormatInfo m_DateTimeFormat = null;

		private ContactsList m_Contacts;
		private QueueList m_Queues;
        private ClientState m_ClientState;

		private ContactRoute.Client.IHttpNetworkClient m_ClientSocket;
		private ContactRoute.Client.IHttpNetworkClient m_EventSocket;
		private IPAddress[] m_DnsServers;

        internal SynchronizationContext SyncContext { get; private set; }
        internal Type LinkNetworkType = typeof(HttpNetworkSocket);

		private HttpLinkCommands m_Commands;

		public event HttpLinkServerResponseDelegate ServerResponse;
		public event HttpLinkServerResponseDelegate SessionExpired;
		public event HttpLinkServerEventDelegate ServerEvent;
		public event HttpLinkServerEventDelegate AgentStateChanged;
		public event HttpLinkContactEventDelegate ContactAdded;
		public event HttpLinkContactEventDelegate ContactChanged;
		public event HttpLinkContactEventDelegate ContactRemoved;
		public event HttpLinkContactEventDelegate ContactStateChanged;
		public event HttpLinkServerEventDelegate AgentQueueStateChanged;
        public event HttpLinkServerEventDelegate ViewServerOperation;
        public event HttpLinkServerEventDelegate Watch;
        public event HttpLinkServerEventDelegate SelectionRelatedCommands;
		public event HttpLinkServerEventDelegate AgentWarning;
        public event HttpLinkServerEventDelegate OOBInfo;
        public event HttpLinkServerEventDelegate PauseForced;
		public event HttpLinkServerEventDelegate AgentStatistics;
		public event HttpLinkServerEventDelegate InboundStatistics;
		public event HttpLinkServerEventDelegate AgentsRealtimeStateChanged;
		public event HttpLinkEventDelegate AgentTeamsChanged;
		public event HttpLinkEventDelegate PauseCodesChanged;
		public event HttpLinkEventDelegate SearchActivitiesChanged;
		public event HttpLinkServerEventDelegate AgentMessage;
		public event HttpLinkServerEventDelegate ChatSpyMessage;

		public HttpLink(ISession session)
		{
			Initialize(session["agent"], null, null, null);
		}

        public HttpLink(ISession session, Control invokeControl)
        {
            Initialize(session["agent"], invokeControl, null, null);
        }

        public HttpLink(ISession session, SynchronizationContext syncContext)
        {
            Initialize(session["agent"], null, syncContext, null);
        }

        public void TraceComment(string comment)
		{
			Trace.WriteLine(string.Concat(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), "\t", comment));
		}

        public bool ServerConnectionBroken
        {
            get
            {
                return m_ServerConnectionBroken;
            }
            private set
            {
                m_ServerConnectionBroken = value;
            }
        }

		private void Initialize(IServiceDescription service, Control invokeControl, SynchronizationContext syncContext, string[] forceDnsServers)
		{
			m_EventControl = invokeControl;



            if (m_EventControl == null)
            {
                if (syncContext != null)
                    SyncContext = syncContext;
                else
                    SyncContext = SynchronizationContext.Current as SynchronizationContext;
            }



            m_Contacts = new ContactsList(this);
			m_Queues = new QueueList(this);
			m_Commands = new HttpLinkCommands(this);

            m_ClientState = new ClientState(this);

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

            string BaseUriString = service.Location;

			if (!Uri.IsWellFormedUriString(BaseUriString, UriKind.Absolute))
			{
				string Service = string.Concat("_NixxisClient._tcp.", BaseUriString.Replace('@', '.'));
				DnsClient Client;

				if (m_DnsServers != null)
				{
					Client = new DnsClient(m_DnsServers);
				}
				else
				{
					Client = new DnsClient();
				}

				DnsBase[] Locations = Client.ExecuteRequest(new DnsRequest(Service, DnsResourceType.SRV, true), 10000);

				if (Locations.Length > 0)
				{
					for (int i = 0; i < Locations.Length; i++)
					{
						if (Locations[i].Type == DnsResourceType.SRV)
						{
							DnsSRV SRV = (DnsSRV)Locations[i];

							if (SRV.Port != 80)
							{
								m_BaseUri = new Uri(string.Concat("http://", SRV.Server, ":", SRV.Port.ToString(), "/agent/"));
							}
							else
							{
								m_BaseUri = new Uri(string.Concat("http://", SRV.Server));
							}

#if DEBUG
							Trace.TraceInformation("Service {0} resolved as {1}", BaseUriString, m_BaseUri.ToString(), "/agent/");
#endif
							break;
						}
					}
				}

				if (m_BaseUri == null)
					throw new ArgumentException();
			}
			else
			{
				m_BaseUri = new Uri(BaseUriString);
			}

			m_EventUri = new Uri(m_BaseUri, "__events/");

			m_CookieContainer = new CookieContainer(32, 32, 512);

			m_UserAgent = string.Join("; ", new string[] { string.Format("Nixxis/HttpLink.{0}", Assembly.GetExecutingAssembly().FullName.Substring(Assembly.GetExecutingAssembly().FullName.IndexOf("Version=", StringComparison.OrdinalIgnoreCase) + 8, 3)), Assembly.GetExecutingAssembly().FullName, Assembly.GetEntryAssembly()==null? "":Assembly.GetEntryAssembly().FullName });

            try
            {
                string strLinkNetType = service.Settings["LinkNetworkType"];
                if(!string.IsNullOrEmpty(strLinkNetType))
                    LinkNetworkType = Type.GetType(strLinkNetType);
            }
            catch { }

            if (LinkNetworkType == null)
                LinkNetworkType = typeof(HttpNetworkSocket);

            m_ClientSocket = Activator.CreateInstance(LinkNetworkType, m_BaseUri) as IHttpNetworkClient;

            m_GlobalTimer = new System.Threading.Timer(this.TimerCallbackHandler, this, 1000, 100);
			TraceComment("Init Done");
		}

        System.Threading.Timer m_GlobalTimer;

        internal void TimerCallbackHandler(object state)
        {
            try
            {
                m_ClientState.TimerCallbackHandler(state);

                lock (m_Contacts)
                {
                    foreach (ContactInfo CInfo in m_Contacts)
                        CInfo.TimerCallbackHandler(state);
                }
            }
            catch
            {
            }
        }

		internal Control EventControl
		{
			get
			{
				return m_EventControl;
			}
		}


		public HttpLinkCommands Commands
		{
			get
			{
				return m_Commands;
			}
		}

		public Uri BaseUri
		{
			get
			{
				return m_BaseUri;
			}
		}

		public ContactsList Contacts
		{
			get
			{
				return m_Contacts;
			}
		}

		public QueueList Queues
		{
			get
			{
				return m_Queues;
			}
		}

		public TeamCollection Teams
		{
			get
			{
				return m_Teams;
			}
		}

        public IList<TimeZoneInfo> TimeZones
        {
            get
            {
                List<TimeZoneInfo> TZ = new List<TimeZoneInfo>();

                try
                {
                    string TZOffsets = ClientSettings["CallbackZones"];

                    if (!string.IsNullOrEmpty(TZOffsets))
                    {
                        string[] Parts = TZOffsets.Split(';');

                        foreach (string Offset in Parts)
                        {
                            string[] Subs = Offset.Split(new char[] { ',' }, 2);

                            if (Subs.Length == 2)
                            {
                                TZ.Add(TimeZoneInfo.CreateCustomTimeZone(Offset, new TimeSpan(0, int.Parse(Subs[0]), 0), Subs[1], Subs[1]));
                            }
                        }
                    }
                }
                catch
                {
                }

                return TZ;
            }
        }

        public ClientState ClientState
        {
            get
            {
                return m_ClientState;
            }
        }

		public PauseCodeCollection PauseCodes
		{
			get
			{
				return m_PauseCodes;
			}
		}

		public ActivitySupervisionCollection ActivitiesSupervision
		{
			get
			{
				return m_ActivitiesSupervision;
			}
		}

		public AgentRealtimeStateCollection AgentsRealtimeState
		{
			get
			{
				return m_AgentsRealtimeState;
			}
		}

		public AgentRealtimeState[] AgentsRealtimeStateList
		{
			get
			{
				AgentRealtimeState[] List;

				lock (m_AgentsRealtimeState)
				{
					List = new AgentRealtimeState[m_AgentsRealtimeState.Count];
					m_AgentsRealtimeState.CopyTo(List, 0);
				}

				if (List.Length > 10000)
					Trace.WriteLine(string.Format("Warning: long agent realtime stat list ({0})", List.Length));

				Array.Sort<AgentRealtimeState>(List, (x, y) => x.Account.CompareTo(y.Account));

				return List;
			}
		}

		public AgentRealtimeState[] AgentsActiveRealtimeStateList
		{
			get
			{
				ArrayList TmpList = new ArrayList();

				lock (m_AgentsRealtimeState)
				{
					foreach (AgentRealtimeState St in m_AgentsRealtimeState)
					{
						if (St.State > 0)
							TmpList.Add(St);
					}
				}

				if (TmpList.Count > 1000)
					Trace.WriteLine(string.Format("Warning: long agent realtime active stat list ({0})", TmpList.Count));

				AgentRealtimeState[] List = (AgentRealtimeState[])TmpList.ToArray(typeof(AgentRealtimeState));
				Array.Sort<AgentRealtimeState>(List, (x, y) => x.Account.CompareTo(y.Account));

				return List;
			}
		}

		public ICredentials Credentials
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_Credentials;
			}
		}

		public string Account
		{
			get
			{
				return m_Account;
			}
		}

		public ClientRoles ClientRoles
		{
			get
			{
				if (m_Disposed)
					throw new ObjectDisposedException(this.GetType().Name);

				return m_ClientRoles;
			}
		}

		public int AutoReady
		{
			get
			{
				return m_AutoReady;
			}
		}

		public bool AllowDialOnBehalf
		{
			get
			{
				return m_AllowDialOnBehalf;
			}
		}

		public string[] WellKnownCallDestinations
		{
			get
			{
				return m_ManualCallList;
			}
		}

        public string[] WellknownChatDestinations
        {
            get
            {
                return m_WellknownChatDestinations;
            }
        }

        public string[] WellknownMailDestinations
        {
            get
            {
                return m_WellknownMailDestinations;
            }
        }


		public string[] AgentStateSelection
		{
			get
			{
				return m_StateSelection;
			}
		}

        public string AgentId
        {
            get
            {
                return m_AgentId;
            }
        }

		public string SessionId
		{
			get
			{
				return m_SessionId;
			}
		}

        public DateTimeFormatInfo DateTimeFormat
        {
            get
            {
                return m_DateTimeFormat;
            }
        }

        public bool HasClientRole(ClientRoles clientRole)
		{
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().Name);

			return ((m_ClientRoles & clientRole) == clientRole);
		}

		public System.Collections.Specialized.NameValueCollection ClientSettings
		{
			get
			{
				return m_ClientSettings;
			}
		}

		public ActivityCollection SearchModeActivities
		{
			get
			{
				return m_Activities;
			}
		}

		protected void OnResponse(object state)
		{
		}

		internal RequestData BuildRequest(string action)
		{
			return BuildRequest(action, null, (string)null);
		}

		internal RequestData BuildRequest(string action, string[] parameters)
		{
			return BuildRequest(action, parameters, (string)null);
		}

		internal RequestData BuildRequest(string action, string[] parameters, System.Collections.Specialized.NameValueCollection postData)
		{
			StringBuilder SB = new StringBuilder();

			foreach(string Key in postData.Keys)
			{
				SB.Append(Key).Append('=').Append(postData[Key] ?? "").Append('\n');
			}

			return BuildRequest(action, parameters, SB.ToString());
		}

		internal RequestData BuildRequest(string action, string[] parameters, string postData)
		{
			RequestData Request;

			if(parameters != null && parameters.Length > 0)
			{
				StringBuilder SB = new StringBuilder();

				SB.Append("?action=").Append(action);

				for(int i = 0; i < parameters.Length; i++)
				{
					SB.Append("&__p").Append(i + 1).Append('=').Append(parameters[i]);
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

		internal ResponseData GetResponseData(RequestData request)
		{
			return GetResponseData(request, false);
		}

		internal ResponseData GetResponseData(RequestData request, bool rawMode)
		{
			ResponseData Data = null;
			string ResponseBody;
			int Retry = 3;

			while (Data == null)
			{
				ResponseBody = string.Empty;

				try
				{
					
					lock (m_ClientSocket)
					{
						request.Exec(m_ClientSocket, m_TraceNetwork);

						if (m_ClientSocket.ContentLength > 0)
						{
							ResponseBody = Encoding.UTF8.GetString(m_ClientSocket.RawData, 0, m_ClientSocket.ContentLength);
						}

                        if (rawMode)
                        {
                            Data = new ResponseData(m_ClientSocket.StatusCode, ResponseBody, true);
                            return Data;
                        }
                        else
                        {
                            Data = new ResponseData(m_ClientSocket.StatusCode);
                        }
					}


					if(Data != null)
					{
						if (Data.StatusCode == (int)HttpStatusCode.Forbidden)
						{
							m_SessionId = null;
						}
						else if (Data.Valid)
						{
							string[] ResponseLines = ResponseBody.Split(ProtocolOptions.LineSeparator, StringSplitOptions.RemoveEmptyEntries);

							foreach (string ResponseLine in ResponseLines)
							{
								string[] LineParts = ResponseLine.Split(ProtocolOptions.ValueSeparator, 2);

								if (LineParts[0].Equals("event", StringComparison.OrdinalIgnoreCase))
								{
									string[] EventParts = LineParts[1].Split(ProtocolOptions.EscapeSeparator);

									if (EventParts.Length > 1)
									{
										try
										{
											ServerEventArgs EventArgs = new ServerEventArgs(this, (EventCodes)Enum.Parse(typeof(EventCodes), (string)EventParts[1], true));

											if (EventParts.Length > 2)
												EventArgs.Parameters = Uri.UnescapeDataString(EventParts[2]);

											ProcessEvent(EventArgs);
										}
										catch
										{
										}
									}
								}
								else
								{
									try
									{
										if (LineParts.Length>1)
											Data.Add(LineParts[0], LineParts[1]);
										else
											Data.Add(LineParts[0], null);
									}
									catch
									{
									}

								}
							}
						}
						else
						{
							if(!string.IsNullOrEmpty(ResponseBody))
								Data.Add("~error", ResponseBody);
						}
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
					Invoke(SessionExpired, Data);
				}
				else if (Data.Valid)
				{
					Invoke(ServerResponse, Data);
				}
			}

			return Data;
		}

		internal void SetActiveContact(string contactId)
		{
			GetResponseData(BuildRequest("~activecontact", null, contactId));
		}

		public void SetCustomerId(string contactId, string customerId)
		{
			ContactInfo CInfo = m_Contacts[contactId];

			if (CInfo != null)
			{
				string[] parameters = new string[4];

				parameters[0] = ((int)InfoCodes.ContactProperty).ToString();
				parameters[1] = contactId;
				parameters[2] = "@@CustomerId";
				parameters[3] = customerId;

				GetResponseData(BuildRequest("~setinfo", null, string.Join("\r\n", parameters)));

				Invoke(ContactChanged, new ContactEventArgs(this, EventCodes.ContactData, CInfo));
			}
		}

		public void SetCustomerDescription(string contactId, string customerDescription)
		{
			ContactInfo CInfo = m_Contacts[contactId];

			if (CInfo != null)
			{
				string[] parameters = new string[4];

				parameters[0] = ((int)InfoCodes.ContactProperty).ToString();
				parameters[1] = contactId;
				parameters[2] = "@@CustomerDescription";
				parameters[3] = customerDescription;

				GetResponseData(BuildRequest("~setinfo", null, string.Join("\r\n", parameters)));

				Invoke(ContactChanged, new ContactEventArgs(this, EventCodes.ContactData, CInfo));
			}
		}

		public void SetAgentState(string state)
		{
			string[] parameters = new string[2];

			parameters[0] = ((int)InfoCodes.AgentSubState).ToString();
			parameters[1] = state;

			GetResponseData(BuildRequest("~setinfo", null, string.Join("\r\n", parameters)));
		}

		public void SetContactListId(string contactId, string id)
		{
			ContactInfo CInfo = m_Contacts[contactId];

			if (CInfo != null)
			{
				string[] parameters = new string[4];

				parameters[0] = ((int)InfoCodes.ContactProperty).ToString();
				parameters[1] = contactId;
				parameters[2] = "@@ContactListId";
				parameters[3] = id;

				GetResponseData(BuildRequest("~setinfo", null, string.Join("\r\n", parameters)));

				Invoke(ContactChanged, new ContactEventArgs(this, EventCodes.ContactData, CInfo));
			}
		}

        public void SendDtmf(char digit)
        {
            SendDtmf(null, digit);
        }

        public void SendDtmf(string contactId, char digit)
        {
            string[] Parameters;

            if (!string.IsNullOrEmpty(contactId))
            {
                Parameters = new string[2];
                Parameters[1] = contactId;
            }
            else
            {
                Parameters = new string[1];
            }

            Parameters[0] = new string(digit, 1);

            GetResponseData(BuildRequest("~senddtmf", null, string.Join("\r\n", Parameters)));
        }

        public void WatchMe(string[] Parameters)
        {
            GetResponseData(BuildRequest("~watchme", null, string.Join("\r\n", Parameters)));
        }

        public DateTime GetServerTime()
        {
            DateTime StartTime = DateTime.Now;
            ResponseData RespData = GetResponseData(BuildRequest("~servertime"), true);
            DateTime EndTime = DateTime.Now;
            DateTime ServerTime = StartTime;

            if (RespData.Valid)
            {
                string[] Lines = RespData.RawResponse.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (Lines.Length > 1)
                {
                    ServerTime = DateTime.Parse(Lines[0]).Subtract(new TimeSpan((EndTime.Ticks - StartTime.Ticks) / 2));

                    if (Lines.Length >= 6 && m_DateTimeFormat == null)
                    {
                        try
                        {
                            m_DateTimeFormat = CultureInfo.GetCultureInfo(Lines[4]).DateTimeFormat.Clone() as DateTimeFormatInfo;
                        }
                        catch
                        {
                            m_DateTimeFormat = new DateTimeFormatInfo();
                        }

                        m_DateTimeFormat.SetAllDateTimePatterns(Lines[6].Split('|'), 'd');
                        m_DateTimeFormat.SetAllDateTimePatterns(Lines[7].Split('|'), 'T');
                    }
                }
            }

            return ServerTime;
        }

        public long GetServerTimeOffset()
        {
            DateTime StartTime = DateTime.Now;
            ResponseData RespData = GetResponseData(BuildRequest("~servertime"), true);
            DateTime EndTime = DateTime.Now;
            long ServerTimeOffset = 0;

            if (RespData.Valid)
            {
                string[] Lines = RespData.RawResponse.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (Lines.Length > 1)
                {
                    ServerTimeOffset = StartTime.Subtract(DateTime.Parse(Lines[0]).Subtract(new TimeSpan((EndTime.Ticks - StartTime.Ticks) / 2))).Ticks;

                    if (Lines.Length >= 6 && m_DateTimeFormat == null)
                    {
                        try
                        {
                            m_DateTimeFormat = CultureInfo.GetCultureInfo(Lines[4]).DateTimeFormat.Clone() as DateTimeFormatInfo;
                        }
                        catch
                        {
                            m_DateTimeFormat = new DateTimeFormatInfo();
                        }

                        m_DateTimeFormat.SetAllDateTimePatterns(Lines[6].Split('|'), 'd');
                        m_DateTimeFormat.SetAllDateTimePatterns(Lines[7].Split('|'), 'T');
                    }
                }
            }

            return ServerTimeOffset;
        }

		public bool Connect(out string connectionInfo)
		{
			string Value;

			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().Name);

			connectionInfo = null;

			if(m_SessionId != null)
				Disconnect();

			System.Collections.Specialized.NameValueCollection ConnectParams = new System.Collections.Specialized.NameValueCollection();

			try
			{
				foreach(System.Net.NetworkInformation.NetworkInterface Nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
				{
					if(Nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && Nic.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
					{
						foreach(System.Net.NetworkInformation.UnicastIPAddressInformation Addr in Nic.GetIPProperties().UnicastAddresses)
						{
							if(Addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
							{
								ConnectParams.Add("ipaddr", string.Concat(Addr.Address.ToString(), "/", Addr.IPv4Mask.ToString()));
							}
						}
					}
				}
			}
			catch
			{
			}

			try
			{
				if (m_ClientSocket.ClientEndpoint == null)
					m_ClientSocket.Connect();

				ConnectParams.Add("ipendpoint", m_ClientSocket.ClientEndpoint.ToString());
			}
			catch
			{
			}

			ResponseData Data = GetResponseData(BuildRequest("~connect", null, ConnectParams));

			if (Data == null || !Data.Valid || !Data.TryGetValue("session", out m_SessionId))
			{
				if(Data != null)
					Data.TryGetValue("~error", out connectionInfo);

				return false;
			}

            string DateTimeRef;

            if (Data.TryGetValue("culture", out DateTimeRef))
            {
                FindDateTimeFormat(DateTimeRef);
            }

			m_CookieContainer = new CookieContainer(32, 32, 512);
			m_CookieContainer.Add(new Uri(m_BaseUri, ".."), new Cookie("contactroutehttpserversessionid", m_SessionId));

            m_ClientSocket.Cookies.Add(m_BaseUri, new Cookie("contactroutehttpserversessionid", m_SessionId));

			if (Data.TryGetValue("actions", out Value))
			{
				Commands.BuildCommands(Value);
			}

			Data.TryGetValue("server", out connectionInfo);

			string Tmp;

			if (Data.TryGetValue("autoready", out Tmp))
				int.TryParse(Tmp, out m_AutoReady);

			if (Data.TryGetValue("allowdialonbehalf", out Tmp))
				bool.TryParse(Tmp, out m_AllowDialOnBehalf);

			foreach (string Key in Data.Keys)
			{
				if (Key.StartsWith("client_", StringComparison.OrdinalIgnoreCase))
					m_ClientSettings.Set(Key.Substring(7), Data[Key]);
			}

			m_EventThread = new Thread(new ThreadStart(HttpEventLoop));
			m_EventThread.Name = "Nixxis Client HttpLink";
			m_EventThread.IsBackground = true;

			m_EventThread.Start();
			return true;
		}

		public bool Disconnect()
		{
			m_SessionId = null;

			try
			{
				if (m_EventSocket != null)
				{
					m_EventSocket.Abort();
				}
			}
			catch
			{
			}

			for (int i = 0; i < 100; i++)
			{
				if (m_EventThread == null)
					break;

				Thread.Sleep(100);
			}

			try
			{
				if (m_EventThread != null)
				{
					m_EventThread.Abort();
					m_EventThread = null;
				}
			}
			catch
			{
			}

			Commands.ResetCommands();

			m_Account = string.Empty;
			m_ClientRoles = ClientRoles.None;
			m_ManualCallList = new string[0];
            m_WellknownChatDestinations = new string[0];
            m_WellknownMailDestinations = new string[0];
			m_StateSelection = new string[0];

			GetResponseData(BuildRequest("~disconnect"));

			m_CookieContainer = new CookieContainer(32, 32, 512);

			return true;
		}

		public bool Login(ICredentials credentials, string Extension, out string loginInfo, out string agentId)
		{
			return Login(credentials, Extension, ClientRoles.Agent, out loginInfo, out agentId);
		}

		public bool Login(ICredentials credentials, string Extension, ClientRoles roles, out string loginInfo, out string agentId)
		{
			string Value;

			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().Name);

			loginInfo = null;
            agentId = null;

			m_Credentials = credentials;

			NetworkCredential Cred = null;

			if (m_Credentials != null)
				Cred = m_Credentials.GetCredential(m_BaseUri, "basic");

			if (Cred == null)
				Cred = new NetworkCredential("", "", "");

			ResponseData Data = GetResponseData(BuildRequest("~login", null, string.Format("{0}\\{1}:{2}\r\n{3}\r\nroles={4}\r\nactiverole={5}", 
				Cred.Domain, Cred.UserName, Cred.Password, Extension, roles.ToString(), roles.ToString())));

			if (Data == null || !Data.Valid || !Data.TryGetValue("account", out m_Account))
			{
				if (Data != null)
					Data.TryGetValue("~error", out loginInfo);

				return false;
			}

			if (Data.TryGetValue("roles", out Value))
			{
				string[] Roles = Value.Split(ProtocolOptions.ListSeparator, StringSplitOptions.RemoveEmptyEntries);

				m_ClientRoles = ClientRoles.None;

				foreach (string Role in Roles)
				{
					try
					{
						m_ClientRoles |= (ClientRoles)Enum.Parse(typeof(ClientRoles), Role, true);
					}
					catch
					{
					}
				}
			}

			if (Data.TryGetValue("manualcalllist", out Value))
			{
				m_ManualCallList = Value.Split(ProtocolOptions.ListSeparator, StringSplitOptions.RemoveEmptyEntries);
			}

            if (Data.TryGetValue("wellknownchatlist", out Value))
            {
                m_WellknownChatDestinations = Value.Split(ProtocolOptions.ListSeparator, StringSplitOptions.RemoveEmptyEntries);
            }

            if (Data.TryGetValue("wellknownmaillist", out Value))
            {
                m_WellknownMailDestinations = Value.Split(ProtocolOptions.ListSeparator, StringSplitOptions.RemoveEmptyEntries);
            }

			if (Data.TryGetValue("stateselection", out Value))
            {
				m_StateSelection = Value.Split(ProtocolOptions.ListSeparator, StringSplitOptions.RemoveEmptyEntries);
			}

            string DateTimeRef;

			Data.TryGetValue("name", out m_Name);
			Data.TryGetValue("description", out m_Description);
			Data.TryGetValue("agentid", out agentId);

			loginInfo = m_Name;

			string Tmp;

			if (Data.TryGetValue("autoready", out Tmp))
				int.TryParse(Tmp, out m_AutoReady);

			if (Data.TryGetValue("allowdialonbehalf", out Tmp))
				bool.TryParse(Tmp, out m_AllowDialOnBehalf);

			m_AgentId = agentId;

						m_PauseCodes.Clear();

			foreach(KeyValuePair<string, string> Pair in Data)
			{
				if (Pair.Key.StartsWith("pausecode_", StringComparison.OrdinalIgnoreCase))
				{
					m_PauseCodes.Add(new PauseCodeInfo(Pair.Key.Substring(10), Pair.Value));
				}
				else if (Pair.Key.StartsWith("searchmode_", StringComparison.OrdinalIgnoreCase))
				{
					string[] SubParts = Pair.Value.Split(',');

					m_Activities.Add(new ActivityInfo(SubParts[2], true, SubParts[0], Microsoft.JScript.GlobalObject.unescape(SubParts[1]), Microsoft.JScript.GlobalObject.unescape(SubParts[3])));
				}
			}

			Invoke(PauseCodesChanged, null);
            Invoke(SearchActivitiesChanged, null);

			return true;
		}

        private void FindDateTimeFormat(string DateTimeRef)
        {
            string[] Parts = DateTimeRef.Split('|');

            if (m_DateTimeFormat == null)
            {
                try
                {
                    m_DateTimeFormat = CultureInfo.GetCultureInfo(Parts[0]).DateTimeFormat.Clone() as DateTimeFormatInfo;
                }
                catch
                {
                    m_DateTimeFormat = new DateTimeFormatInfo();
                }

                m_DateTimeFormat.SetAllDateTimePatterns(new string[] { Parts[2] }, 'd');
                m_DateTimeFormat.SetAllDateTimePatterns(new string[] { Parts[3] }, 'T');
            }
        }

        public bool ChangeRole(ClientRoles role)
        {
            string Value;

            if (m_Disposed)
                throw new ObjectDisposedException(this.GetType().Name);

            ResponseData Data = GetResponseData(BuildRequest("~changesettings", null, string.Format("roles={0}\r\nactiverole={1}",
                role.ToString(), role.ToString())));

            if (Data == null || !Data.Valid)
            {
                return false;
            }

            if (Data.TryGetValue("roles", out Value))
            {
                string[] Roles = Value.Split(ProtocolOptions.ListSeparator, StringSplitOptions.RemoveEmptyEntries);

                m_ClientRoles = ClientRoles.None;

                foreach (string Role in Roles)
                {
                    try
                    {
                        m_ClientRoles |= (ClientRoles)Enum.Parse(typeof(ClientRoles), Role, true);
                    }
                    catch
                    {
                    }
                }
            }

            return true;
        }

		public bool KeepAlive()
		{
			if (m_Disposed)
				throw new ObjectDisposedException(this.GetType().Name);

			if (m_SessionId == null)
				return false;

			ResponseData Data = GetResponseData(BuildRequest("idle"));

			if (Data == null || !Data.Valid)
			{
				return false;
			}

			return true;
		}

		public void ActivateTeam(string teamId, bool active)
		{
			GetResponseData(BuildRequest("~activateteam", null, string.Concat(teamId, "\n", (active) ? "1" : "0")));
		}

		public void SendMessage(MessageType messageType, string destination, string message)
		{
			GetResponseData(BuildRequest("~sendmessage", null, string.Concat("~", ((int)messageType).ToString(), "~0~", destination, "\n", Microsoft.JScript.GlobalObject.escape(message))));
		}

        public void SendMessage(MessageType messageType, MessageDestinations destination, string destinationMask, string message)
        {
            GetResponseData(BuildRequest("~sendmessage", null, string.Concat("~", ((int)messageType).ToString(), "~", ((int)destination).ToString(), "~", destinationMask ?? "", "\n", Microsoft.JScript.GlobalObject.escape(message))));
        }

        public void SetQualification(string contactId, string qualificationId, string callbackDateTime, string callbackPhone)
        {
            SetQualification(contactId, qualificationId, callbackDateTime, callbackPhone, string.Empty);
        }

        public void SetQualification(string contactId, string qualificationId, string callbackDateTime, string callbackPhone, string callbackBy)
        {
            string[] parameters = new string[7];

            parameters[0] = ((int)InfoCodes.Qualifications).ToString();
            parameters[1] = contactId;
            parameters[2] = qualificationId;
            parameters[3] = callbackDateTime;
            parameters[4] = callbackPhone;
            parameters[5] = "*";

            if(!string.IsNullOrEmpty(callbackBy))
                parameters[6] = callbackBy;

            GetResponseData(BuildRequest("~setinfo", null, string.Join("\r\n", parameters)));
        }

        public ResponseData LoadData(string key)
        {
            string[] parameters = new string[1];

            parameters[0] = key;

            return GetResponseData(BuildRequest("~loaddata", null, string.Join("\r\n", parameters)));
        }

        public void SaveData(string key, string value)
        {
            string[] parameters = new string[2];

            parameters[0] = key;
            parameters[1] = Microsoft.JScript.GlobalObject.escape(value);

            GetResponseData(BuildRequest("~savedata", null, string.Join("\r\n", parameters)));
        }

        public string ExecuteCommand(string CommandName,params string[]parameters)
        {
            ResponseData Data = GetResponseData(BuildRequest(CommandName,null,string.Join("\r\n", parameters)));
            if(Data.Valid == true)
            {
                return Data.ToString();
            }
            else
            {
                return null;
            }
        }

		public void MailAddAttachment(ContactInfo contact, string attId, string description, string fileName)
		{
			try
			{
				string[] urlsplit = this.BaseUri.AbsoluteUri.Split('/');
				string cmd = urlsplit[0] + "//" + this.BaseUri.Authority + contact.ContentLink + "addattachment&attid=" + attId + "&description=" + Microsoft.JScript.GlobalObject.escape(description);

				byte[] DataBytes = File.ReadAllBytes(fileName);

				HttpWebRequest tempReq = (HttpWebRequest)WebRequest.Create(cmd);
				tempReq.KeepAlive = false;
				tempReq.ContentLength = DataBytes.Length;

				CookieContainer cc = new CookieContainer();
				Cookie co = new Cookie();

				co.Value = this.SessionId;
				co.Name = "contactroutehttpserversessionid";
				co.Domain = this.BaseUri.Authority;

				tempReq.CookieContainer = cc;

				tempReq.Method = WebRequestMethods.Http.Post;

				using (Stream PostData = tempReq.GetRequestStream())
				{
					PostData.Write(DataBytes, 0, DataBytes.Length);
				}

				using (HttpWebResponse Response = tempReq.GetResponse() as HttpWebResponse)
				{
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Trace.WriteLine(e.ToString());
			}
		}
		
		public void MailReply(ContactInfo contact, string text, string[] attIds)
		{
			MailReply(contact, null, null, null, null, text, attIds);
		}
		
        public void MailReply(ContactInfo contact, string from, string to, string cc, string subject, string text, string[] attIds)
        {
            HttpWebResponse response = null;

            string[] urlsplit = this.BaseUri.AbsoluteUri.Split('/');
            StringBuilder SB = new StringBuilder();
			
			string CmdUri = urlsplit[0] + "//" + this.BaseUri.Authority + contact.ContentLink;
            
			if(!string.IsNullOrEmpty(from))
				SB.Append("From=").AppendLine(from);

			if(!string.IsNullOrEmpty(to))
				SB.Append("To=").AppendLine(to);

			if(!string.IsNullOrEmpty(cc))
				SB.Append("Cc=").Append(cc);

			if(!string.IsNullOrEmpty(subject))
				SB.Append("Subject=").Append(subject);

            try
            {
                byte[] DataBytes;
				HttpWebRequest tempReq;
				System.IO.Stream PostData;

				CookieContainer CC = new CookieContainer();
				Cookie co = new Cookie();

				co.Value = this.SessionId;
				co.Name = "contactroutehttpserversessionid";
				co.Domain = this.BaseUri.Authority;
				//CC.Add(co); Can't be use because of bug in framework 

				DataBytes = System.Text.Encoding.UTF8.GetBytes(SB.ToString());

				tempReq = (HttpWebRequest)WebRequest.Create(CmdUri + "responsedata");
				tempReq.KeepAlive = false;
				tempReq.ContentLength = DataBytes.Length;

                tempReq.CookieContainer = CC;

				tempReq.Method = WebRequestMethods.Http.Post;

				PostData = tempReq.GetRequestStream();
				PostData.Write(DataBytes, 0, DataBytes.Length);
				PostData.Close();

				response = tempReq.GetResponse() as HttpWebResponse;
				response.Close();
				
				DataBytes = System.Text.Encoding.UTF8.GetBytes(string.Concat(string.Join(",", attIds ?? new string[0]), "/", text));

				tempReq = (HttpWebRequest)WebRequest.Create(CmdUri + "response2");
                tempReq.KeepAlive = false;
                tempReq.ContentLength = DataBytes.Length;

                tempReq.CookieContainer = CC;

                tempReq.Method = WebRequestMethods.Http.Post;

                PostData = tempReq.GetRequestStream();
                PostData.Write(DataBytes, 0, DataBytes.Length);
				PostData.Close();

                response = tempReq.GetResponse() as HttpWebResponse;
				response.Close();
			}
            catch (Exception e)
            {
               System.Diagnostics.Trace.WriteLine(e.ToString());
            }
        }

        private string GetPredTextDesc(string text)
        {
            return text.Substring(0, text.IndexOf('&'));
        }

        private string GetPredTextContent(string text)
        {
            return text.Substring(text.IndexOf('&') + 1);
        }

        public Calendar GetCallbacks(string contactId, DateTime? startTime, DateTime? endTime)
        {
            Nixxis.Client.Calendar cal = new Calendar(cal_GetDays, contactId);

            return cal;
        }

        void cal_GetDays(Nixxis.Client.Calendar cal, string contactId, string qualificationId, DateTime? startTime, DateTime? endTime)
        {
            string[] parameters;

            if (startTime.HasValue && endTime.HasValue)
            {
                parameters = new string[5];
            }
            else if (startTime.HasValue)
            {
                parameters = new string[4];
            }
            else
            {
                parameters = new string[3];
            }

            parameters[0] = ((int)InfoCodes.CallbacksAgenda).ToString();
            parameters[1] = contactId;
            parameters[2] = qualificationId;

            if (startTime.HasValue)
            {
                parameters[3] = startTime.HasValue ? System.Xml.XmlConvert.ToString(startTime.Value, System.Xml.XmlDateTimeSerializationMode.Unspecified) : string.Empty;
                if (endTime.HasValue)
                {
                    parameters[4] = endTime.HasValue ? System.Xml.XmlConvert.ToString(endTime.Value, System.Xml.XmlDateTimeSerializationMode.Unspecified) : string.Empty;
                }
            }

            ResponseData RespData = GetResponseData(BuildRequest("~getinfo", null, string.Join("\r\n", parameters)));

            string[] strValues = new string[RespData.Count];
            int counter = 0;
            foreach (KeyValuePair<string, string> kvp in RespData)
            {
                strValues[counter++] = kvp.Key;
            }

            cal.Load(strValues);

        }

        public PredefinedTextCollection GetPredefinedTexts(string ActivityId)
        {
            string[] parameters = new string[3];

            parameters[0] = ((int)InfoCodes.PredefinedTexts).ToString();
            parameters[1] = ActivityId;
            parameters[2] = "false";    // true=plain text  false=html

            ResponseData RespData = GetResponseData(BuildRequest("~getinfo", null, string.Join("\r\n", parameters)));
            PredefinedTextCollection predefinedTextCol = new PredefinedTextCollection();

            foreach (KeyValuePair<string, string> kvp in RespData)
            {
                PredefinedTextItem pti = new PredefinedTextItem();

                pti.Id = kvp.Key;
                pti.Text = Microsoft.JScript.GlobalObject.unescape(GetPredTextDesc(kvp.Value));
                pti.FullText = Microsoft.JScript.GlobalObject.unescape(GetPredTextContent(kvp.Value));

                predefinedTextCol.Add(pti);
            }

            return predefinedTextCol;
        }

        public AttachmentCollection GetAttachments(string ActivityId)
        {
            string[] parameters = new string[3];

            parameters[0] = ((int)InfoCodes.Attachments).ToString();
            parameters[1] = ActivityId;

            ResponseData RespData = GetResponseData(BuildRequest("~getinfo", null, string.Join("\r\n", parameters)));
            AttachmentCollection AttachCollect = new AttachmentCollection();

            foreach (KeyValuePair<string, string> kvp in RespData)
            {
                AttachmentItem attachItem = new AttachmentItem ();

                attachItem.Id = kvp.Key;
                attachItem.DescriptionAttachment = Microsoft.JScript.GlobalObject.unescape(kvp.Value); 

                AttachCollect.Add(attachItem);
            }

            return AttachCollect;
        }

		private bool SuspendedWaitForCall;
		private bool SuspendedWaitForMail;
		private bool SuspendedWaitForChat;

		public void SuspendForModalDialog()
		{
			SuspendedWaitForCall = Commands.WaitForCall.Active;
			SuspendedWaitForMail = Commands.WaitForMail.Active;
			SuspendedWaitForChat = Commands.WaitForChat.Active;

			if(Commands.WaitForCall.Active)
				Commands.WaitForCall.Execute("0", "1");
			if (Commands.WaitForMail.Active)
				Commands.WaitForMail.Execute("0", "1");
			if (Commands.WaitForChat.Active)
				Commands.WaitForChat.Execute("0", "1");
		}

		public void ResumeAfterModalDialog()
		{
			if (SuspendedWaitForCall != Commands.WaitForCall.Active)
				Commands.WaitForCall.Execute(SuspendedWaitForCall);
			if (SuspendedWaitForMail != Commands.WaitForMail.Active)
				Commands.WaitForMail.Execute(SuspendedWaitForMail);
			if (SuspendedWaitForChat != Commands.WaitForChat.Active)
				Commands.WaitForChat.Execute(SuspendedWaitForChat);
		}

        private void HttpEventLoop()
		{
			ulong LastKnownSequence = 0;
			int Timeout = 30;
			ContactRoute.ApplicationServer.Formatting.DataFormatter Formatter = new ContactRoute.ApplicationServer.Formatting.DataFormatter();

            m_EventSocket = Activator.CreateInstance(LinkNetworkType, m_EventUri) as IHttpNetworkClient;

			m_EventSocket.Timeout = (Timeout + 10) * 1000;
            m_EventSocket.Cookies.Add(m_EventUri, new Cookie("contactroutehttpserversessionid", m_SessionId));
            bool connectionOK = true;
			while (!m_Disposed && m_SessionId != null)
			{
				Uri ThisUri = new Uri(m_EventUri, string.Format("?to={0}&fmt=0&seq={1}", Timeout, LastKnownSequence));

                try
				{

                    if (m_EventSocket.Get(ThisUri.PathAndQuery))
                    {
                        if (m_Disposed || m_SessionId == null)
                            break;

                        if (m_EventSocket.StatusCode == (int)HttpStatusCode.NoContent)
                        {
                        }
                        else if (m_EventSocket.StatusCode == (int)HttpStatusCode.OK)
                        {
                            if (m_EventSocket.ContentLength > 2)
                            {
                                try
                                {
                                    //									Formatter.Initialize(ResponseBuffer, 0, ResponseBuffer.Length, false);
                                    Formatter.Initialize(m_EventSocket.RawData, 0, m_EventSocket.ContentLength, false);
                                    uint EvtCount = ((((uint)Formatter.ReadByte()) << 8) | ((uint)Formatter.ReadByte()));

                                    for (uint EvtPos = 0; EvtPos < EvtCount; EvtPos++)
                                    {
                                        ulong EvtSeq = 0;
                                        object[] EventData = null;

                                        for (int i = 0; i < 8; i++)
                                        {
                                            EvtSeq >>= 8;
                                            EvtSeq |= (((ulong)Formatter.ReadByte()) << 56);
                                        }

                                        int DataCount = Formatter.ReadByte();

                                        if (DataCount != 0xFF)
                                        {
                                            EventData = new object[DataCount];

                                            for (int i = 0; i < DataCount; i++)
                                            {
                                                EventData[i] = Formatter.Deserialize();
                                            }
                                        }

                                        if (m_TraceNetwork)
                                        {
                                            Trace.WriteLine(string.Concat(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), " SEQ ", EvtSeq.ToString(), " EVENT ", (string)EventData[0], " ", (EventData.Length > 1) ? ((string)EventData[1]) : ""));
                                        }

                                        try
                                        {
                                            ServerEventArgs EventArgs = new ServerEventArgs(this, (EventCodes)Enum.Parse(typeof(EventCodes), (string)EventData[0], true));

                                            if (EventData.Length > 1)
                                                EventArgs.Parameters = (string)EventData[1];

                                            ProcessEvent(EventArgs);
                                        }
                                        catch
                                        {
                                        }

                                        if (EvtSeq > LastKnownSequence)
                                        {
                                            LastKnownSequence = EvtSeq;
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        if (connectionOK)
                        {
                            connectionOK = false;
                            ServerEventArgs args = new ServerEventArgs(this, EventCodes.AgentWarning);
                            args.Parameters = "Server connection is broken!";
                            this.ServerConnectionBroken = true;
                            Invoke(AgentWarning, args);
                        }
                    }
				}
				catch (WebException WEx)
				{
					if (WEx.Status != WebExceptionStatus.Timeout)
						Thread.Sleep(100);
				}
				catch
				{
					Thread.Sleep(100);
				}
			}

			m_EventSocket = null;
			m_EventThread = null;
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (!m_Disposed)
			{
				m_Disposed = true;

				Disconnect();
				Commands.Dispose();
			}
		}

		#endregion

		private void ProcessEvent(ServerEventArgs eventArgs)
		{

			InvokeDirect(ServerEvent, eventArgs);

			if (!eventArgs.Cancel)
			{
				if (eventArgs.EventCode == EventCodes.ClientState)
				{
					Invoke(AgentStateChanged, eventArgs);
				}
				else if (eventArgs.EventCode == EventCodes.CommandState)
				{
					string[] Params = eventArgs.Parameters.Split(ProtocolOptions.ListSeparator);

					if (Params.Length >= 2)
					{
						ICommand Command = Commands.FindCommand(Params[0]);

						if(Command != null)
						{
							Command.LockState();

							try
							{
								if(char.IsDigit(Params[1][0]))
									((BaseCommand)Command).SetAuthorized(Params[1][0] != '0');
								else
									((BaseCommand)Command).SetAuthorized(bool.Parse(Params[1]));

								if (Params.Length > 2)
								{
									if (Params[2][0] == '-')
										((BaseCommand)Command).SetActive(null);
                                    else if (char.IsDigit(Params[2][0]))
                                        ((BaseCommand)Command).SetActive(Params[2][0] != '0');
                                    else
										((BaseCommand)Command).SetActive(bool.Parse(Params[2]));
								}
							}
							catch
							{
							}

							Command.ReleaseState();
						}
					}
				}
				else if (eventArgs.EventCode == EventCodes.ContactData)
				{
					string[] Params = eventArgs.Parameters.Split(ProtocolOptions.EscapeSeparator, StringSplitOptions.RemoveEmptyEntries);
                    
					ContactInfo NewInfo = new ContactInfo(this, 'V', "");

					foreach (string Param in Params)
					{
						int Sep = Param.IndexOf('=');

						if (Sep > 0)
						{
							string Key = Param.Substring(0, Sep);
                            string Value = Param.Substring(Sep + 1);


							if (string.Equals(Key, "Id", StringComparison.OrdinalIgnoreCase))
							{
                                NewInfo.Id = Microsoft.JScript.GlobalObject.unescape(Value);
							}
							else if (string.Equals(Key, "From", StringComparison.OrdinalIgnoreCase))
							{
                                NewInfo.From = Microsoft.JScript.GlobalObject.unescape(Value);
							}
							else if (string.Equals(Key, "To", StringComparison.OrdinalIgnoreCase))
							{
                                NewInfo.To = Microsoft.JScript.GlobalObject.unescape(Value);
							}
							else if (string.Equals(Key, "Queue", StringComparison.OrdinalIgnoreCase))
							{
                                NewInfo.Queue = Microsoft.JScript.GlobalObject.unescape(Value);
							}
							else if (string.Equals(Key, "Direction", StringComparison.OrdinalIgnoreCase))
							{
                                NewInfo.Direction = Microsoft.JScript.GlobalObject.unescape(Value);
							}
                            else if (string.Equals(Key, "Media", StringComparison.OrdinalIgnoreCase))
                            {
                                NewInfo.MediaCode = Convert.ToChar(Value);
                            }
							else if (string.Equals(Key, "Context", StringComparison.OrdinalIgnoreCase))
							{
                                NewInfo.Context = Microsoft.JScript.GlobalObject.unescape(Value);
							}
							else if (string.Equals(Key, "Script", StringComparison.OrdinalIgnoreCase))
							{
                                NewInfo.Script = Microsoft.JScript.GlobalObject.unescape(Value);
							}
							else if (string.Equals(Key, "UUI", StringComparison.OrdinalIgnoreCase))
							{
                                NewInfo.UUI = Microsoft.JScript.GlobalObject.unescape(Value);
							}
                            else if (string.Equals(Key, "Language", StringComparison.OrdinalIgnoreCase))
                            {
                                NewInfo.Language = Microsoft.JScript.GlobalObject.unescape(Value);
                            }
                            else if (string.Equals(Key, "ContentLink", StringComparison.OrdinalIgnoreCase))
                            {
                                NewInfo.ContentLink = Microsoft.JScript.GlobalObject.unescape(Value);
                            }
                            else if (string.Equals(Key, "ContentHandler", StringComparison.OrdinalIgnoreCase))
                            {
                                NewInfo.ContentHandler = Microsoft.JScript.GlobalObject.unescape(Value);
                            }
                            else if (string.Equals(Key, "ContentType", StringComparison.OrdinalIgnoreCase))
							{
								int ContentType = 0;

								int.TryParse(Value, out ContentType);
								NewInfo.ContentType = ContentType;
							}
							else if (string.Equals(Key, "Activity", StringComparison.OrdinalIgnoreCase))
                            {
                                // remove the version if I can find one...
                                
                                string[] strArray = Value.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                                if(strArray.Length>0)
                                    NewInfo.Activity = Microsoft.JScript.GlobalObject.unescape(strArray[0]);
                            }
							else if (string.Equals(Key, "Customer", StringComparison.OrdinalIgnoreCase))
							{
								NewInfo.CustomerId = Microsoft.JScript.GlobalObject.unescape(Value);
							}
							else if (string.Equals(Key, "CustomerDescription", StringComparison.OrdinalIgnoreCase))
							{
								NewInfo.CustomerDescription = Microsoft.JScript.GlobalObject.unescape(Value);
							}
							else if (string.Equals(Key, "ContactListId", StringComparison.OrdinalIgnoreCase))
							{
                                NewInfo.ContactListId = Microsoft.JScript.GlobalObject.unescape(Value);
							}
                            else if (string.Equals(Key, "Campaign", StringComparison.OrdinalIgnoreCase))
                            {
                                // remove the version if I can find one...

                                string[] strArray = Value.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                                if (strArray.Length > 0)
                                    NewInfo.Campaign = Microsoft.JScript.GlobalObject.unescape(strArray[0]);
                            }
						}
					}

					ContactInfo CInfo;
                    ContactInfoMail CInfoM;
                    ContactInfoChat CInfoC;
                    ContactInfoVoice CInfoV;
                    
					bool IsNew = false;
					bool IsUpdated = false;

					lock (m_Contacts)
					{
						CInfo = m_Contacts[NewInfo.Id];

						if (CInfo == null)
						{
                            switch (NewInfo.MediaCode)
                            {
                                case 'V':
                                    CInfoV = new ContactInfoVoice(NewInfo);
                                    CInfo = CInfoV;
                                    m_Contacts.Add(CInfoV);
                                    break;

                                case 'M':
                                    CInfoM = new ContactInfoMail(NewInfo);
                                    CInfo = CInfoM;
                                    m_Contacts.Add(CInfoM);
                                    break;

                                case 'C':
                                    CInfoC = new ContactInfoChat(NewInfo);
                                    CInfo = CInfoC;
                                    m_Contacts.Add(CInfoC);
                                    break;
                            }

							IsNew = true;
						}
						else
						{
							IsUpdated = CInfo.Update(NewInfo);
						}
					}

					ContactEventArgs ContactArgs = new ContactEventArgs(this, eventArgs.EventCode, CInfo);

					if (IsNew)
					{
						Invoke(ContactAdded, ContactArgs);
					}
					else if (IsUpdated)
					{
						Invoke(ContactChanged, ContactArgs);
					}
				}
				else if (eventArgs.EventCode == EventCodes.ContactState)
				{
					string[] Params = eventArgs.Parameters.Split(ProtocolOptions.ListSeparator);

					if (Params.Length >= 1)
					{
						string ContactId = Params[0];
						char ContactState = Params[1][0];
						ContactInfo CInfo;

						lock (m_Contacts)
						{
							CInfo = m_Contacts[ContactId];
						}

						if (CInfo != null)
						{
							if (CInfo.State != ContactState)
							{
								CInfo.State = ContactState;

								ContactEventArgs ContactArgs = new ContactEventArgs(this, eventArgs.EventCode, CInfo);

								if (CInfo.State != 'X')
								{
									Invoke(ContactStateChanged, ContactArgs);
								}
								else
								{
									lock (m_Contacts)
									{
										m_Contacts.Remove(CInfo);

										if (m_Contacts.ActiveContactId == CInfo.Id)
											m_Contacts.ActiveContactId = null;
									}

									Invoke(ContactRemoved, ContactArgs);

                                    CInfo.Dispose();
								}
							}
						}
					}
				}
                else if(eventArgs.EventCode == EventCodes.ViewServerOperation)
                {
                    if(ViewServerOperation!=null)
                        Invoke(ViewServerOperation, eventArgs);
                }
                else if (eventArgs.EventCode == EventCodes.Watch)
                {
                    if (Watch != null)
                        Invoke(Watch, eventArgs);
                }
                else if(eventArgs.EventCode == EventCodes.SelectionRelatedCommands)
                {
                    if (SelectionRelatedCommands != null)
                        Invoke(SelectionRelatedCommands, eventArgs);
                }
                else if (eventArgs.EventCode == EventCodes.AgentQueueState)
				{
					string[] Parts = eventArgs.Parameters.Split(ProtocolOptions.ListSeparator);
					bool Changed = false;
					if (Monitor.TryEnter(m_Queues, 1000))
					{
						try
						{
							if (m_Queues.UpdateHighPriorityContacts(Convert.ToInt32(Parts[0])))
								Changed = true;

                            if (m_Queues.UpdateWaitingContacts(Convert.ToInt32(Parts[1])))
								Changed = true;

						}
						finally
						{
							Monitor.Exit(m_Queues);
						}
					}

					if (Changed)
					{
						Invoke(AgentQueueStateChanged, eventArgs);
					}
				}
                else if (eventArgs.EventCode == EventCodes.AgentDialInfoState)
                {
                    string[] Parts = eventArgs.Parameters.Split(ProtocolOptions.ListSeparator);

                    string strNumber = Microsoft.JScript.GlobalObject.unescape(Parts[0]);
                    string strActivity = Microsoft.JScript.GlobalObject.unescape(Parts[1]);
                    string strDescription = Microsoft.JScript.GlobalObject.unescape(Parts[2]);
                    string strIsTargetedCallback = Microsoft.JScript.GlobalObject.unescape(Parts[3]);

                    if (Monitor.TryEnter(m_Queues, 1000))
					{
                        try
                        {
                            m_Queues.UpdateAgentDialInfo(strNumber, strActivity, strDescription, Boolean.Parse(strIsTargetedCallback));
                        }
                        finally
                        {
                            Monitor.Exit(m_Queues);
                        }
                    }
                }
				else if (eventArgs.EventCode == EventCodes.AgentWarning)
				{
					Invoke(AgentWarning, eventArgs);
				}
                else if (eventArgs.EventCode == EventCodes.OOBInfo)
                {
                    Invoke(OOBInfo, eventArgs);
                }
                else if (eventArgs.EventCode == EventCodes.ForcePause)
                {
                    Invoke(PauseForced, eventArgs);
                }
                else if (eventArgs.EventCode == EventCodes.SupervisionItem)
				{
					string[] Parts = eventArgs.Parameters.Split(',');

					if (Parts[0].StartsWith("Inbound_"))
					{
						string ActivityId = Parts[0];
						ActivitySupervision Info;

						if (m_ActivitiesSupervision.Contains(ActivityId))
							Info = m_ActivitiesSupervision[ActivityId];
						else
							m_ActivitiesSupervision.Add(Info = new ActivitySupervision(ActivityId));

						Info.Description = Parts[2];
					}
					else if (Parts[0].StartsWith("Agent_"))
					{
						string AgentId = Parts[0];
						AgentRealtimeState Info;

						lock (m_AgentsRealtimeState)
						{
							if (m_AgentsRealtimeState.Contains(AgentId))
								Info = m_AgentsRealtimeState[AgentId];
							else
								m_AgentsRealtimeState.Add(Info = new AgentRealtimeState(this, AgentId));

							Info.Account = Parts[2];
							Info.Description = Parts[3];
						}
					}
				}
				else if (eventArgs.EventCode == EventCodes.SupervisionData)
				{
					string[] Parts = eventArgs.Parameters.Split(',');

					if (Parts[0].StartsWith("Agent_"))
					{
						if (AgentStatistics != null)
						{
							try
							{
								if (Parts[0].Substring(6).Equals(m_AgentId))
								{
									Invoke(AgentStatistics, eventArgs);
								}
							}
							catch
							{
							}
						}

						string AgentId = Parts[0];
						AgentRealtimeState Info = null;

						lock(m_AgentsRealtimeState)
						{
							if (m_AgentsRealtimeState.Contains(AgentId))
							{
								Info = m_AgentsRealtimeState[AgentId];
							}
						}

						if(Info != null)
						{
							if (Parts[1].Equals("@@RealTime", StringComparison.OrdinalIgnoreCase))
							{
								int.TryParse(Parts[2], out Info.State);
								Info.StateDescription = Parts[3];

								Info.VoiceStateDescription = Parts[5] ?? "";
								Info.ChatStateDescription = Parts[7] ?? "";
								Info.MailStateDescription = Parts[9] ?? "";

								bool.TryParse(Parts[6], out Info.VoiceAvailable);
								bool.TryParse(Parts[8], out Info.ChatAvailable);
								bool.TryParse(Parts[10], out Info.MailAvailable);

								//Carefull, eventagrs changed from here
								eventArgs.Parameters = AgentId;

								Invoke(AgentsRealtimeStateChanged, eventArgs);
							}
						}
					}
					else if (Parts[0].StartsWith("Inbound_"))
					{
						string ActivityId = Parts[0];
						ActivitySupervision Info;

						if (m_ActivitiesSupervision.Contains(ActivityId))
						{
							Info = m_ActivitiesSupervision[ActivityId];

							if (Parts[1].Equals("@@RealTime", StringComparison.OrdinalIgnoreCase))
							{
								Info.RealTimeData = eventArgs.Parameters;
							}
						}

						Invoke(InboundStatistics, eventArgs);
					}
				}
				else if (eventArgs.EventCode == EventCodes.TeamsChanged)
				{
					string[] Parts = eventArgs.Parameters.Split(ProtocolOptions.ListSeparator);

					m_Teams.Clear();

					for (int i = 0; i < Parts.Length - 2; i++)
					{
						try
						{
                            TeamInfo Team = new TeamInfo(Parts[i], Convert.ToInt32(Parts[++i]) != 0, Microsoft.JScript.GlobalObject.unescape(Parts[++i]));

							m_Teams.Add(Team);
						}
						catch
						{
						}
					}

					Invoke(AgentTeamsChanged);
				}
				else if (eventArgs.EventCode == EventCodes.AgentMessage)
				{
					string[] Parts = eventArgs.Parameters.Split(ProtocolOptions.ListSeparator);

					Invoke(AgentMessage, eventArgs);
				}
				else if (eventArgs.EventCode == EventCodes.ChatSpyMessage)
				{
					string[] Parts = eventArgs.Parameters.Split(ProtocolOptions.ListSeparator);

					Invoke(ChatSpyMessage, eventArgs);
				}
			}
		}

		#region IAgentInfo Members


		public string Name
		{
			get
			{
				return m_Name;
			}
		}

		public string Description
		{
			get
			{
				return m_Description;
			}
		}

		public ClientRoles Roles
		{
			get
			{
				return m_ClientRoles;
			}
		}

		public string GetPrivateInfo(string key)
		{
			ResponseData Data = GetResponseData(BuildRequest("~getinfo&fmt=uri", null, string.Concat(((int)InfoCodes.AgentProperty).ToString(), "\r\n", m_AgentId, "\r\n", key)));

			if (Data != null && Data.Valid)
			{
				foreach (string Key in Data.Keys)
					return Key;

				return "";
			}

			return null;
		}

		public void SetPrivateInfo(string key, string value)
		{
			ResponseData Data = GetResponseData(BuildRequest("~setinfo&fmt=uri", null, string.Concat(((int)InfoCodes.AgentProperty).ToString(), "\r\n", m_AgentId, "\r\n", key, "\r\n", Microsoft.JScript.GlobalObject.escape(value))));
		}

		#endregion

		public ExternalAgentCollection GetTeamMembers(string teamId)
		{
			return GetTeamMembers(teamId, true);
		}

		public ExternalAgentCollection GetTeamMembers(string teamId, bool onlyActive)
		{
			ResponseData Data = GetResponseData(BuildRequest("~getinfo&fmt=uri", null, string.Concat(((int)InfoCodes.AgentsInfo).ToString(), "\r\n", teamId, "\r\n", onlyActive.ToString())));

			if (Data != null && Data.Valid)
			{
				ExternalAgentCollection Coll = new ExternalAgentCollection();

				foreach (string Key in Data.Keys)
				{
					string[] Parts = Key.Split(';');
					
					if(Parts.Length > 2)
					{
						string State = "";

						if(Parts.Length > 5 && Parts[3] != "0")
						{
							State = Parts[4];
						}

						ExternalAgentInfo Info = new ExternalAgentInfo(Parts[0], Parts[1], Microsoft.JScript.GlobalObject.unescape(Parts[2]), State);
						Coll.Add(Info);
					}
				}

				return Coll;
			}

			return null;
		}
	}
}
