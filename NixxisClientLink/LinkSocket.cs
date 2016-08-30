using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Specialized;

namespace ContactRoute.Client
{
    internal class HttpNetworkClient : ContactRoute.Client.IHttpNetworkClient
    {
        private ServicePoint m_ServicePoint;

        protected static char[] WhiteSpace = new char[] { ' ', '\t' };
        protected static char[] HeaderSeparator = new char[] { ':' };
        protected static char[] ListSeparator = new char[] { ';' };
        protected static char[] ValueSeparator = new char[] { '=' };

        public static int PreferedBlockSize = 1400;
        public static int DefaultTimeout = 30000;

        private static System.Reflection.AssemblyName LinkAssemblyName;
        private static string DefaultUserAgent;

        private IPAddress[] m_DnsServers;
        private string m_Host;
        private string m_ServerName;
        private int m_ServerPort;
        private Uri m_ServerUri;
        private IPEndPoint m_ServerEndPoint;
        private IPAddress[] m_ServerAddresses;
        private CookieContainer m_Cookies = new CookieContainer(5);
        private string m_UserAgent = DefaultUserAgent;
        private int m_Timeout = DefaultTimeout;
        private byte[] m_Buffer = new byte[PreferedBlockSize];
        private int m_BufferPos = 0;
        private NameValueCollection m_RequestHeaders = new NameValueCollection(10);
        private NameValueCollection m_ResponseHeaders = new NameValueCollection(50);
        private int m_ContentLength;
        private string m_ContentType;
        private string m_ContentEncoding;
        private int m_StatusCode = -1;
        private bool m_Aborted = false;

        public bool TraceNetwork { get; set; }

        public IPEndPoint ClientEndpoint { get; private set; }

        static HttpNetworkClient()
        {
            LinkAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            DefaultUserAgent = string.Format("Nixxis/{0} {1}.{2}.{3}", LinkAssemblyName.Name, LinkAssemblyName.Version.Major, LinkAssemblyName.Version.Minor, LinkAssemblyName.Version.Build);
        }

        public HttpNetworkClient(Uri uri)
        {
            m_ServerPort = uri.Port;

            if (m_ServerPort == 0)
                m_ServerPort = 80;

            if (m_ServerPort == 80)
            {
                m_Host = uri.Host;
            }
            else
            {
                m_Host = string.Concat(uri.Host, ':', uri.Port.ToString());
            }

            m_ServerName = uri.Host;
            m_ServerPort = uri.Port;

            m_ServerUri = uri;
        }

        public CookieContainer Cookies
        {
            get
            {
                return m_Cookies;
            }
        }

        public string UserAgent
        {
            get
            {
                return m_UserAgent;
            }
            set
            {
                m_UserAgent = value;
            }
        }

        public int Timeout
        {
            get
            {
                return m_Timeout;
            }
            set
            {
                m_Timeout = value;
            }
        }

        public int ContentLength
        {
            get
            {
                return m_ContentLength;
            }
        }

        public string ContentType
        {
            get
            {
                return m_ContentType;
            }
        }

        public int StatusCode
        {
            get
            {
                return m_StatusCode;
            }
        }

        public byte[] RawData
        {
            get
            {
                return m_Buffer;
            }
        }

        public bool Connect()
        {
            //TODO: manage servicepoint here
            return true;
        }

        protected bool Send(string method, string query, byte[] postData, int offset, int count)
        {
            int Retry = 5;

            m_Aborted = false;

            lock (this)
            {
                StringBuilder SB = null;

                if (TraceNetwork)
                {
                    SB = new StringBuilder();

                    SB.Append(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"));
                    SB.Append(" ").Append(method).Append(" ");
                    SB.Append(query);

                    SB.AppendLine();

                    if (count > 0)
                        SB.Append("\t").AppendLine(Encoding.UTF8.GetString(postData, offset, count).Replace("\r", "").Replace("\n", "\r\n\t"));
                }

                while (Retry-- > 0)
                {
                    HttpWebRequest Request = WebRequest.Create(new Uri(m_ServerUri, query)) as HttpWebRequest;

                    Request.Method = method;
                    Request.ProtocolVersion = new Version(1, 1);
                    Request.UserAgent = m_UserAgent;

                    Request.Headers.Add(m_RequestHeaders);
                    Request.CookieContainer = m_Cookies;
                   
                    try
                    {
                        if (count > 0)
                        {
                            Request.ContentType = m_ContentType;
                            Request.ContentLength = count;

                            using (Stream RS = Request.GetRequestStream())
                            {
                                RS.Write(postData, offset, count);
                            }
                        }

                        using (WebResponse Response = Request.GetResponse())
                        {
                            HttpWebResponse HttpResponse = Response as HttpWebResponse;

                            m_ContentLength = (int)Response.ContentLength;
                            m_ContentType = Response.ContentType;

                            if(HttpResponse != null)
                            {
                                m_StatusCode = (int)HttpResponse.StatusCode;
                            }

                            if(m_ContentLength > 0)
                            {
                                if(m_Buffer.Length < m_ContentLength)
                                    m_Buffer = new byte[m_ContentLength];

                                m_BufferPos = 0;

                                using(Stream ResponseStream = Response.GetResponseStream())
                                {
                                    while(m_BufferPos < m_ContentLength)
                                        m_BufferPos += ResponseStream.Read(m_Buffer, m_BufferPos, m_ContentLength - m_BufferPos);
                                }
                            }

                            if (TraceNetwork)
                            {
                                SB.Append(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff")).Append(" HTTP ").Append(m_StatusCode).Append(" ").Append(m_ContentLength);

                                SB.AppendLine();

                                if (m_ContentLength > 0)
                                {
                                    SB.Append("\t").AppendLine(Encoding.UTF8.GetString(m_Buffer, 0, m_ContentLength).Replace("\r", "").Replace("\n", "\r\n\t"));
                                }

                                Trace.WriteLine(SB.ToString());
                            }

                            return true;
                        }
                    }
                    catch (Exception Ex)
                    {
                        Trace.WriteLine(string.Concat(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), " ", Ex.ToString()), "HttpClientSocket");
                    }
                }

                return false;
            }
        }

        public bool Get(string query)
        {
            m_ContentType = string.Empty;

            return Send("GET", query, null, 0, 0);
        }

        public bool Post(string query, byte[] postData)
        {
            m_ContentType = "application/octet-stream";

            return Send("POST", query, postData, 0, postData.Length);
        }

        public bool Post(string query, byte[] postData, string contentType)
        {
            m_ContentType = contentType;

            return Send("POST", query, postData, 0, postData.Length);
        }

        public bool Post(string query, byte[] postData, int offset, int count)
        {
            m_ContentType = "application/octet-stream";

            return Send("POST", query, postData, offset, count);
        }

        public bool Post(string query, byte[] postData, int offset, int count, string contentType)
        {
            m_ContentType = contentType;

            return Send("POST", query, postData, offset, count);
        }

        public void Abort()
        {
            m_Aborted = true;
        }

        public void ClearCookies()
        {
            m_Cookies = new CookieContainer(10);
        }
    }

	internal class HttpNetworkSocket : IHttpNetworkClient
	{
		protected static char[] WhiteSpace = new char[] { ' ', '\t' };
		protected static char[] HeaderSeparator = new char[] { ':' };
		protected static char[] ListSeparator = new char[] { ';' };
		protected static char[] ValueSeparator = new char[] { '=' };

		public static int PreferedBlockSize = 1400;
		public static int DefaultTimeout = 30000;
		public static string DefaultUserAgent = "Nixxis/HttpLink.2.0";

		private IPAddress[] m_DnsServers;
		private string m_Host;
		private string m_ServerName;
        private Uri m_Uri;
		private int m_ServerPort;
		private IPEndPoint m_ServerEndPoint;
		private IPAddress[] m_ServerAddresses;
        private CookieContainer m_Cookies = new CookieContainer(5);
        private Socket m_Socket;
		private string m_UserAgent = DefaultUserAgent;
		private int m_Timeout = DefaultTimeout;
		private byte[] m_Buffer = new byte[PreferedBlockSize];
		private int m_BufferPos = 0;
		private List<string> m_RequestHeaders = new List<string>();
		private SortedDictionary<string, string> m_ResponseHeaders = new SortedDictionary<string, string>();
		private int m_ContentLength;
		private string m_ContentType;
		private string m_ContentEncoding;
		private int m_StatusCode = -1;
		private bool m_Aborted = false;

        public bool TraceNetwork { get; set; }

		public IPEndPoint ClientEndpoint { get; private set; }

		public HttpNetworkSocket(string host) : this(host, null)
		{
		}

		public HttpNetworkSocket(string host, IPAddress[] dnsServers)
		{
			m_Host = host;
			m_DnsServers = dnsServers;

            m_Uri = new Uri("http://" + m_Host);

			int Sep = m_Host.IndexOf(':');

			if (Sep >= 0)
			{
				m_ServerName = m_Host.Substring(0, Sep);
				m_ServerPort = Convert.ToInt32(m_Host.Substring(Sep + 1));
			}
			else
			{
				m_ServerName = m_Host;
				m_ServerPort = 80;
			}

			AddDefaultHeaders();
			ResolveServerName();
		}

		public HttpNetworkSocket(Uri uri) : this(uri, null)
		{
		}

        public HttpNetworkSocket(Uri uri, IPAddress[] dnsServers)
		{
            m_Uri = uri;
			m_ServerPort = uri.Port;

			if (m_ServerPort == 0)
				m_ServerPort = 80;

			if (m_ServerPort == 80)
			{
				m_Host = uri.Host;
			}
			else
			{
				m_Host = string.Concat(uri.Host, ':', uri.Port.ToString());
			}

			m_DnsServers = dnsServers;

			m_ServerName = uri.Host;
			m_ServerPort = uri.Port;

			AddDefaultHeaders();
			ResolveServerName();
		}

		private void AddDefaultHeaders()
		{
			m_RequestHeaders.Add(string.Concat("host: ", m_Host, "\r\n"));
			m_RequestHeaders.Add(string.Concat("user-agent: ", m_UserAgent, "\r\n"));
		}

		private void ResolveServerName()
		{
			IPAddress Addr;

			if (IPAddress.TryParse(m_ServerName, out Addr))
			{
				m_ServerAddresses = new IPAddress[] { Addr };
			}
			else
			{
				if (m_DnsServers != null && m_DnsServers.Length > 0)
				{
					Nixxis.Client.DnsClient Client = new Nixxis.Client.DnsClient(m_DnsServers);

					Nixxis.Client.DnsBase[] Results = Client.ExecuteRequest(new Nixxis.Client.DnsRequest(m_ServerName, Nixxis.Client.DnsResourceType.A, true), 10000);

					if (Results.Length > 0)
					{
						List<IPAddress> Adresses = new List<IPAddress>();

						for (int i = 0; i < Results.Length; i++)
						{
							if (Results[0] is Nixxis.Client.DnsA)
							{
								Adresses.Add(((Nixxis.Client.DnsA)Results[0]).Address);
							}
						}

						m_ServerAddresses = Adresses.ToArray();
					}
				}
				else
				{
					m_ServerAddresses = Dns.GetHostAddresses(m_ServerName);
				}
			}
		}

		public CookieContainer Cookies
		{
			get
			{
				return m_Cookies;
			}
		}

		public string UserAgent
		{
			get
			{
				return m_UserAgent;
			}
			set
			{
				lock (this)
				{
					m_UserAgent = value;

					for (int i = 0; i < m_RequestHeaders.Count; i++)
					{
						if (m_RequestHeaders[i].StartsWith("user-agent:", StringComparison.OrdinalIgnoreCase))
						{
							m_RequestHeaders[i] = string.Concat("user-agent: ", m_UserAgent, "\r\n");
							break;
						}
					}
				}
			}
		}

		public int Timeout
		{
			get
			{
				return m_Timeout;
			}
			set
			{
				m_Timeout = value;
			}
		}

		public int ContentLength
		{
			get
			{
				return m_ContentLength;
			}
		}

		public string ContentType
		{
			get
			{
				return m_ContentType;
			}
		}

		public int StatusCode
		{
			get
			{
				return m_StatusCode;
			}
		}

		public byte[] RawData
		{
			get
			{
				return m_Buffer;
			}
		}

		public bool Connect()
		{
			lock (this)
			{
				if (TraceNetwork)
					Trace.WriteLine(string.Concat(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), " Opening socket connection"));

				if (m_Socket != null)
				{
					if (TraceNetwork)
						Trace.WriteLine("\t Closing existing socket");

					try
					{
						m_Socket.Close();
					}
					catch
					{
					}
				}

				ClientEndpoint = null;
				m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				try
				{
					m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
					m_Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.TypeOfService, 0x03);
					m_Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
				}
				catch
				{
				}

				try
				{
					if (TraceNetwork)
						Trace.WriteLine(string.Concat("\t Connecting ", m_ServerAddresses[0].ToString(), ":", m_ServerPort.ToString()));

					m_Socket.Connect(m_ServerAddresses, m_ServerPort);
				}
				catch
				{
					m_Socket = null;
				}

				if (m_Socket != null)
				{
					if (TraceNetwork)
						Trace.WriteLine("\t Connected");

					ClientEndpoint = m_Socket.LocalEndPoint as IPEndPoint;

					return true;
				}

				if (TraceNetwork)
					Trace.WriteLine(" Failed");

				return false;
			}
		}

		private void Add(string text, Encoding encoding)
		{
			int Size = encoding.GetByteCount(text);

			if (m_BufferPos + Size > m_Buffer.Length)
			{
				byte[] NewBuffer = new byte[(((m_BufferPos + Size) / 256) + 1) * 256];

				if (m_BufferPos > 0)
					Buffer.BlockCopy(m_Buffer, 0, NewBuffer, 0, m_BufferPos);

				m_Buffer = NewBuffer;
			}

			m_BufferPos += encoding.GetBytes(text, 0, text.Length, m_Buffer, m_BufferPos);
		}

		private void Add(string text)
		{
			Add(text, Encoding.UTF8);
		}

		private void AddHeaders(string path)
		{
			for (int i = 0; i < m_RequestHeaders.Count; i++)
			{
				Add(m_RequestHeaders[i], Encoding.UTF8);
			}

			Add("accept-encoding: deflate, gzip\r\n");

			if (m_Cookies.Count > 0)
			{
				bool First = true;
                CookieCollection Cookies = m_Cookies.GetCookies(m_Uri);

				for (int i = 0; i < Cookies.Count; i++)
				{
					Cookie C = Cookies[i];

					if (path.StartsWith(C.Path, StringComparison.OrdinalIgnoreCase))
					{
						if (!First)
						{
							Add(";", Encoding.UTF8);
						}
						else
						{
							Add("cookie: ", Encoding.UTF8);
							First = false;
						}

						Add(C.Name, Encoding.UTF8);
						Add("=", Encoding.UTF8);
						Add(C.Value, Encoding.UTF8);
					}
				}

				if (!First)
					Add("\r\n", Encoding.UTF8);
			}
		}

		private void OnResponseHeader(string name, string value)
		{
			m_ResponseHeaders.Add(name, value);

			try
			{
				if (name.Equals("content-length", StringComparison.OrdinalIgnoreCase))
				{
					m_ContentLength = Convert.ToInt32(value);
				}
				else if (name.Equals("set-cookie", StringComparison.OrdinalIgnoreCase))
				{
					string[] Parts = value.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries);
					string[] NameValue = Parts[0].Split(ValueSeparator, 2);

					string CookieName = NameValue[0].Trim();
					string CookieValue = NameValue[1].Trim();
					string CookiePath = "/";

					for (int i = 1; i < Parts.Length; i++)
					{
						string[] PartValue = Parts[i].Split(ValueSeparator, 2);

						if (PartValue[0].Equals("path", StringComparison.OrdinalIgnoreCase))
						{
							CookiePath = PartValue[1];
						}
					}

                    m_Cookies.SetCookies(m_Uri, value);

				}
				else if (name.Equals("content-type", StringComparison.OrdinalIgnoreCase))
				{
					m_ContentType = value;
				}
				else if (name.Equals("content-encoding", StringComparison.OrdinalIgnoreCase))
				{
					m_ContentEncoding = value;
				}
			}
			catch
			{
			}
		}

		private bool GetResponse()
		{
			int Retry = 5;
			int Available;
			bool Complete = false;
			int GetRawData = -1;
			string CurHeader = null;
			string CurValue = null;

			m_StatusCode = -1;
			m_BufferPos = 0;
			m_ResponseHeaders.Clear();
			m_ContentLength = 0;
			m_ContentType = string.Empty;
			m_ContentEncoding = null;

			while (!Complete)
			{
				for(int i = 0; i < 100; i++)
				{
					if (m_Socket.Poll(m_Timeout * 10, SelectMode.SelectRead) || m_Aborted)
						break;
				}

				if (m_Aborted || (Available = m_Socket.Available) <= 0)
				{
					try
					{
						m_Socket.Close();
					}
					catch
					{
					}

					m_Socket = null;

					return false;
				}

				if (m_BufferPos + Available > m_Buffer.Length)
				{
					byte[] NewBuffer = new byte[(((m_BufferPos + Available) / 256) + 1) * 256];

					if (m_BufferPos > 0)
						Buffer.BlockCopy(m_Buffer, 0, NewBuffer, 0, m_BufferPos);

					m_Buffer = NewBuffer;
				}

				int Received = m_Socket.Receive(m_Buffer, m_BufferPos, Available, SocketFlags.None);

				if (Received == 0)
				{
					try
					{
						m_Socket.Close();
					}
					catch
					{
					}

					m_Socket = null;

					return false;
				}

				m_BufferPos += Received;

				if (GetRawData < 0)
				{
					byte LastSep = 0;
					int StartPos = 0;

					for (int i = 0; i < m_BufferPos; i++)
					{
						if (m_Buffer[i] == '\r' || m_Buffer[i] == '\n')
						{
							if (LastSep != 0 && m_Buffer[i] != LastSep)
							{
								LastSep = 0;
								StartPos++;

								continue;
							}

							LastSep = m_Buffer[i];

							string TextLine = Encoding.UTF8.GetString(m_Buffer, StartPos, i - StartPos);

							i++;

							if (i < m_BufferPos && (m_Buffer[i] == '\r' || m_Buffer[i] == '\n') && m_Buffer[i] != LastSep)
							{
								i++;
								LastSep = 0;
							}

							if (m_BufferPos > i)
							{
								m_BufferPos -= i;

								Buffer.BlockCopy(m_Buffer, i, m_Buffer, 0, m_BufferPos);

								i = -1;
							}
							else
							{
								m_BufferPos = 0;
							}

							if (TextLine.Length == 0)
							{
								if (CurHeader != null)
								{
									OnResponseHeader(CurHeader, CurValue);

									CurHeader = null;
									CurValue = null;
								}

								GetRawData = m_ContentLength;

								break;
							}
							else
							{
								if (StatusCode >= 200)
								{
									if (TextLine[0] == ' ' || TextLine[0] == '\t')
									{
										if (CurValue != null)
											CurValue = string.Concat(CurValue, TextLine.TrimStart(WhiteSpace));
										else
											CurValue = TextLine.TrimStart(WhiteSpace);
									}
									else
									{
										if (CurHeader != null)
										{
											OnResponseHeader(CurHeader, CurValue);
										}

										string[] Header = TextLine.Split(HeaderSeparator, 2);

										CurHeader = Header[0];
										CurValue = (Header.Length > 1) ? Header[1].TrimStart(WhiteSpace) : string.Empty;
									}
								}
								else
								{
									if (TextLine.StartsWith("HTTP/1.1", StringComparison.OrdinalIgnoreCase))
									{
										string[] Parts = TextLine.Split(WhiteSpace, StringSplitOptions.RemoveEmptyEntries);

										if (Parts.Length >= 2)
										{
											m_StatusCode = Convert.ToInt32(Parts[1]);
										}
									}
									else
									{
										return false;
									}
								}
							}
						}
					}
				}

				if (GetRawData >= 0)
				{
					Complete = (m_BufferPos >= GetRawData);
				}
			}

			if (StatusCode >= 300)
			{
				if (m_Socket != null)
				{
					try
					{
						m_Socket.Close();
					}
					catch
					{
					}

					m_Socket = null;
				}
			}

			if (GetRawData > 0)
			{
				if (!string.IsNullOrEmpty(m_ContentEncoding))
				{
					Stream RawStream = null;
					Stream EncodingStream = null;

					if (m_ContentEncoding.Equals("deflate", StringComparison.OrdinalIgnoreCase))
					{
						byte[] RawBuffer = new byte[GetRawData];

						Buffer.BlockCopy(m_Buffer, 0, RawBuffer, 0, GetRawData);

						RawStream = new MemoryStream(RawBuffer, 0, GetRawData, false);
						RawStream.Seek(0, SeekOrigin.Begin);

						EncodingStream = new System.IO.Compression.DeflateStream(RawStream, System.IO.Compression.CompressionMode.Decompress);
					}
					else if (m_ContentEncoding.Equals("gzip", StringComparison.OrdinalIgnoreCase))
					{
						byte[] RawBuffer = new byte[GetRawData];

						Buffer.BlockCopy(m_Buffer, 0, RawBuffer, 0, GetRawData);

						RawStream = new MemoryStream(RawBuffer, 0, GetRawData, false);
						RawStream.Seek(0, SeekOrigin.Begin);

						EncodingStream = new System.IO.Compression.GZipStream(RawStream, System.IO.Compression.CompressionMode.Decompress);
					}

					if (EncodingStream != null)
					{
						int Read;

						m_BufferPos = 0;

						while ((Read = EncodingStream.Read(m_Buffer, m_BufferPos, m_Buffer.Length - m_BufferPos)) > 0)
						{
							m_BufferPos += Read;

							if (m_BufferPos == m_Buffer.Length)
							{
								byte[] NewBuffer = new byte[m_BufferPos + 512];

								Buffer.BlockCopy(m_Buffer, 0, NewBuffer, 0, m_BufferPos);
								m_Buffer = NewBuffer;
							}
						}

						m_ContentLength = m_BufferPos;
						
						EncodingStream.Close();
					}
				}
			}

			return Complete;
		}

		protected bool Send(string method, string query, byte[] postData, int offset, int count)
		{
			int Retry = 5;

			m_Aborted = false;

			lock (this)
			{
				StringBuilder SB = null;

				if (TraceNetwork)
				{
					SB = new StringBuilder();

					SB.Append(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"));
					SB.Append(" ").Append(method).Append(" ");
					SB.Append(query);

					SB.AppendLine();

					if (count > 0)
						SB.Append("\t").AppendLine(Encoding.UTF8.GetString(postData, offset, count).Replace("\r", "").Replace("\n", "\r\n\t"));
				}

				while (Retry-- > 0)
				{
					m_BufferPos = 0;

					Add(method, Encoding.UTF8);
					Add(" ", Encoding.UTF8);
					Add(query, Encoding.UTF8);
					Add(" HTTP/1.1\r\n", Encoding.UTF8);

					AddHeaders(query);

					if (count > 0)
					{
						Add("content-type: ");
						Add(m_ContentType);
						Add(string.Format("\r\ncontent-length: {0}\r\n", count));
					}

					Add("\r\n", Encoding.UTF8);

					int SameSend = 0;

					if (count > 0 && m_BufferPos < PreferedBlockSize)
					{
						SameSend = PreferedBlockSize - m_BufferPos;

						if (SameSend > count)
							SameSend = count;

						Buffer.BlockCopy(postData, offset, m_Buffer, m_BufferPos, SameSend);

						m_BufferPos += SameSend;
					}

					int BufferSent = 0;
					int PostSent = SameSend;

					if (m_Socket == null)
					{
						if (!Connect())
							continue;
					}

					while (BufferSent < m_BufferPos)
					{
						int Sent = 0;

						try
						{
							Sent = m_Socket.Send(m_Buffer, BufferSent, m_BufferPos - BufferSent, SocketFlags.None);
						}
						catch (SocketException SockEx)
						{
							Trace.WriteLine(string.Concat(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), " ", SockEx.ToString()), "HttpClientSocket");
							Sent = 0;
						}
						catch(Exception Ex)
						{
							Trace.WriteLine(string.Concat(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), " ", Ex.ToString()), "HttpClientSocket");
						}

						if (m_Aborted)
						{
							try
							{
								m_Socket.Close();
							}
							catch
							{
							}

							m_Socket = null;

							return false;
						}

						if (Sent == 0)
						{
							try
							{
								m_Socket.Close();
							}
							catch
							{
							}

							m_Socket = null;

							break;
						}
						else
						{
							BufferSent += Sent;
						}
					}

					if (BufferSent < m_BufferPos)
						continue;

					while (PostSent < count)
					{
						int Sent = 0;
						int ToSend = count - PostSent;

						if (ToSend > PreferedBlockSize)
							ToSend = PreferedBlockSize;

						try
						{
							Sent = m_Socket.Send(postData, offset + PostSent, ToSend, SocketFlags.None);
						}
						catch (SocketException SockEx)
						{
							Trace.WriteLine(string.Concat(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff"), " ", SockEx.ToString()), "HttpClientSocket");
							Sent = 0;
						}
						catch
						{
						}

						if (m_Aborted)
						{
							try
							{
								m_Socket.Close();
							}
							catch
							{
							}

							m_Socket = null;

							return false;
						}

						if (Sent == 0)
						{
							try
							{
								m_Socket.Close();
							}
							catch
							{
							}

							m_Socket = null;

							break;
						}
						else
						{
							PostSent += Sent;
						}
					}

					if (PostSent < count)
						continue;

					if (GetResponse())
					{

						if (TraceNetwork)
						{
							SB.Append(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff")).Append(" HTTP ").Append(m_StatusCode).Append(" ").Append(m_ContentLength);

							SB.AppendLine();

							if (m_ContentLength > 0)
							{
								SB.Append("\t").AppendLine(Encoding.UTF8.GetString(m_Buffer, 0, m_ContentLength).Replace("\r", "").Replace("\n", "\r\n\t"));
							}

							Trace.WriteLine(SB.ToString());
						}

						return true;
					}

					if (m_Socket != null)
					{
						try
						{
							m_Socket.Close();
						}
						catch
						{
						}

						m_Socket = null;
					}
				}

				return false;
			}
		}

		public bool Get(string query)
		{
			m_ContentType = string.Empty;

			return Send("GET", query, null, 0, 0);
		}

		public bool Post(string query, byte[] postData)
		{
			m_ContentType = "application/octet-stream";

			return Send("POST", query, postData, 0, postData.Length);
		}

		public bool Post(string query, byte[] postData, string contentType)
		{
			m_ContentType = contentType;

			return Send("POST", query, postData, 0, postData.Length);
		}

		public bool Post(string query, byte[] postData, int offset, int count)
		{
			m_ContentType = "application/octet-stream";

			return Send("POST", query, postData, offset, count);
		}

		public bool Post(string query, byte[] postData, int offset, int count, string contentType)
		{
			m_ContentType = contentType;

			return Send("POST", query, postData, offset, count);
		}

		public void Abort()
		{
			m_Aborted = true;
		}

        public void ClearCookies()
        {
            m_Cookies = new CookieContainer(5);
        }
	}
}
