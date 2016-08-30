using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using System.Collections.Generic;

namespace Nixxis.Client
{
	internal enum DnsRR : byte
	{
		Question = 0x00,
		Answer = 0x80
	}

	internal enum DnsOpcode : ushort
	{
		Standard = 0x00,
		Inverted = 0x10,
		Status = 0x20
	}

	internal enum DnsError : ushort
	{
		Ok = 0x00,
		FormatError = 0x01,
		ServerFailure = 0x02,
		NameError = 0x03,
		NotImplemented = 0x04,
		Refused = 0x05
	}

	internal class DnsHeader
	{
		byte[] m_Header = new byte[12];

		public DnsHeader(ushort id, DnsOpcode opCode)
			: this(id, opCode, true)
		{
		}

		public DnsHeader(ushort id, DnsOpcode opCode, bool recursive)
		{
			m_Header[0] = (byte)((id >> 8) & 0xff);
			m_Header[1] = (byte)(id & 0xff);
			m_Header[2] &= 0x9f;
			m_Header[2] |= (byte)((byte)opCode << 6);

			this.IsRecursive = recursive;
		}

		public DnsHeader(byte[] buffer, ref int offset)
		{
			Buffer.BlockCopy(buffer, offset, m_Header, 0, 12);

			offset = offset + 12;
		}

		public bool IsRequest
		{
			get
			{
				return (m_Header[2] & 0x80) == 0;
			}
			set
			{
				if (value)
					m_Header[2] &= 0x7f;
				else
					m_Header[2] |= 0x80;
			}
		}

		public bool IsResponse
		{
			get
			{
				return (m_Header[2] & 0x80) != 0;
			}
			set
			{
				if (value)
					m_Header[2] |= 0x80;
				else
					m_Header[2] &= 0x7f;
			}
		}

		public bool IsStandard
		{
			get
			{
				return (m_Header[2] & 0x78) == (byte)DnsOpcode.Standard;
			}
		}

		public bool IsInverted
		{
			get
			{
				return (m_Header[2] & 0x78) == (byte)DnsOpcode.Inverted;
			}
		}

		public bool IsStatus
		{
			get
			{
				return (m_Header[2] & 0x78) == (byte)DnsOpcode.Status;
			}
		}

		public bool IsAuthoritativeAnswer
		{
			get
			{
				return (m_Header[2] & 0x04) != 0;
			}
		}

		public bool IsMessageTruncated
		{
			get
			{
				return (m_Header[2] & 0x02) != 0;
			}
		}

		public bool IsRecursive
		{
			get
			{
				return (m_Header[2] & 0x01) != 0;
			}
			set
			{
				if (value)
					m_Header[2] |= 0x01;
				else
					m_Header[2] &= 0xfe;
			}
		}

		public int RequestCount
		{
			get
			{
				return (int)((ushort)m_Header[4] << 8) | (ushort)m_Header[5];
			}
			set
			{
				m_Header[4] = (byte)(((uint)value >> 8) & 0xFF);
				m_Header[5] = (byte)((uint)value & 0xFF);
			}
		}

		public int ResponseCount
		{
			get
			{
				return (int)((ushort)m_Header[6] << 8) | (ushort)m_Header[7];
			}
			set
			{
				m_Header[6] = (byte)(((uint)value >> 8) & 0xFF);
				m_Header[7] = (byte)((uint)value & 0xFF);
			}
		}

		public int NSCount
		{
			get
			{
				return (int)((ushort)m_Header[8] << 8) | (ushort)m_Header[9];
			}
			set
			{
				m_Header[8] = (byte)(((uint)value >> 8) & 0xFF);
				m_Header[9] = (byte)((uint)value & 0xFF);
			}
		}

		public int AdditionalCount
		{
			get
			{
				return (int)((ushort)m_Header[10] << 8) | (ushort)m_Header[11];
			}
			set
			{
				m_Header[10] = (byte)(((uint)value >> 8) & 0xFF);
				m_Header[11] = (byte)((uint)value & 0xFF);
			}
		}

		internal void ToBuffer(byte[] buffer, ref int offset)
		{
			Buffer.BlockCopy(m_Header, 0, buffer, offset, 12);
			offset += 12;
		}
	}

	internal class DnsName
	{
		string[] m_NameParts;

		public DnsName(string name)
		{
			m_NameParts = name.TrimEnd('.').Split('.');
		}

		public DnsName(byte[] buffer, ref int offset)
		{
			ArrayList List = new ArrayList();

			offset = LoadNameParts(buffer, offset, List);
			m_NameParts = (string[])List.ToArray(typeof(string));
		}

		private int LoadNameParts(byte[] buffer, int offset, ArrayList list)
		{
			byte LengthTag;

			while ((LengthTag = buffer[offset++]) != 0)
			{
				if ((LengthTag & 0xc0) != 0)
				{
					int PointerOffset = ((LengthTag & 0x3f) << 8) | buffer[offset++];

					LoadNameParts(buffer, PointerOffset, list);
					break;
				}
				else
				{
					list.Add(Encoding.ASCII.GetString(buffer, offset, LengthTag));
					offset += LengthTag;
				}
			}

			return offset;
		}

		public override string ToString()
		{
			return string.Join(".", m_NameParts);
		}

		internal void ToBuffer(byte[] buffer, ref int offset)
		{
			for (int i = 0; i < m_NameParts.Length; i++)
			{
				int LengthTag = Encoding.ASCII.GetByteCount(m_NameParts[i]);

				if (LengthTag > 63)
					throw new IndexOutOfRangeException();

				buffer[offset++] = (byte)LengthTag;

				offset += Encoding.ASCII.GetBytes(m_NameParts[i], 0, m_NameParts[i].Length, buffer, offset);
			}

			buffer[offset++] = 0;
		}
	}

	public enum DnsClass
	{
		IN = 0x01,
		CS = 0x02,
		CH = 0x03,
		HS = 0x04,
		All = 0xFF
	}

	public enum DnsResourceType
	{
		A = 0x01,
		NS = 0x02,
		SOA = 0x06,
		SRV = 0x21
	}

	internal class DnsRequestRecord
	{
		protected DnsName m_Name;
		protected DnsResourceType m_Type;
		protected DnsClass m_Class;

		public DnsRequestRecord(byte[] buffer, ref int offset)
		{
			m_Name = new DnsName(buffer, ref offset);
			m_Type = (DnsResourceType)(((ushort)buffer[offset++] << 8) | (ushort)buffer[offset++]);
			m_Class = (DnsClass)(((ushort)buffer[offset++] << 8) | (ushort)buffer[offset++]);
		}

		public DnsRequestRecord(DnsName name, DnsResourceType type, DnsClass @class)
		{
			m_Name = name;
			m_Type = type;
			m_Class = @class;
		}

		public DnsName Name
		{
			get
			{
				return m_Name;
			}
		}

		public DnsResourceType Type
		{
			get
			{
				return m_Type;
			}
		}

		public DnsClass Class
		{
			get
			{
				return m_Class;
			}
		}

		internal virtual void ToBuffer(byte[] buffer, ref int offset)
		{
			m_Name.ToBuffer(buffer, ref offset);

			buffer[offset++] = (byte)((((ushort)m_Type) >> 8) & 0xff);
			buffer[offset++] = (byte)(((ushort)m_Type) & 0xff);

			buffer[offset++] = (byte)((((ushort)m_Class) >> 8) & 0xff);
			buffer[offset++] = (byte)(((ushort)m_Class) & 0xff);
		}
	}

	internal class DnsResourceRecord : DnsRequestRecord
	{
		uint m_TTL;
		internal byte[] m_DataBuffer;
		internal ushort m_DataOffset;
		internal ushort m_DataLength;
		DateTime m_Expires;

		public DnsResourceRecord(byte[] buffer, ref int offset)
			: base(buffer, ref offset)
		{
			m_TTL = (uint)((uint)buffer[offset++] << 24) | ((uint)buffer[offset++] << 16) | ((uint)buffer[offset++] << 8) | (uint)buffer[offset++];
			m_Expires = DateTime.Now.AddSeconds(m_TTL);
			m_DataLength = (ushort)(((ushort)buffer[offset++] << 8) | (ushort)buffer[offset++]);
			m_DataOffset = (ushort)offset;
			m_DataBuffer = buffer;

			offset += m_DataLength;
		}

		public DnsResourceRecord(DnsName name, DnsResourceType type, DnsClass @class, byte[] dataBuffer)
			: this(name, type, @class, dataBuffer, 0, dataBuffer.Length)
		{
		}

		public DnsResourceRecord(DnsName name, DnsResourceType type, DnsClass @class, byte[] dataBuffer, int offset, int count)
			: base(name, type, @class)
		{
			if (dataBuffer != null && count > 0)
			{
				m_DataLength = (ushort)count;
				m_DataBuffer = new byte[m_DataLength];

				Buffer.BlockCopy(dataBuffer, offset, m_DataBuffer, 0, m_DataLength);
			}
		}

		public DateTime Expires
		{
			get
			{
				return m_Expires;
			}
		}

		internal override void ToBuffer(byte[] buffer, ref int offset)
		{
			base.ToBuffer(buffer, ref offset);

			buffer[offset++] = (byte)((m_TTL >> 24) & 0xff);
			buffer[offset++] = (byte)((m_TTL >> 16) & 0xff);
			buffer[offset++] = (byte)((m_TTL >> 8) & 0xff);
			buffer[offset++] = (byte)(m_TTL & 0xff);

			buffer[offset++] = (byte)((m_DataLength >> 8) & 0xff);
			buffer[offset++] = (byte)(m_DataLength & 0xff);
		}
	}

	public class DnsBase
	{
		internal DnsResourceRecord m_Resource;

		private DnsBase()
		{
		}

		internal DnsBase(DnsResourceRecord resource)
		{
			m_Resource = resource;
		}

		public string Name
		{
			get
			{
				return m_Resource.Name.ToString();
			}
		}

		public DnsResourceType Type
		{
			get
			{
				return m_Resource.Type;
			}
		}

		public DnsClass Class
		{
			get
			{
				return m_Resource.Class;
			}
		}

		public DateTime Expires
		{
			get
			{
				return m_Resource.Expires;
			}
		}
	}

	public class DnsUnknown : DnsBase
	{
		internal DnsUnknown(DnsResourceRecord resource)
			: base(resource)
		{
		}

		public int DataOffset
		{
			get
			{
				return m_Resource.m_DataOffset;
			}
		}

		public int DataLength
		{
			get
			{
				return m_Resource.m_DataLength;
			}
		}

		public byte[] DataBuffer
		{
			get
			{
				return m_Resource.m_DataBuffer;
			}
		}
	}

	public class DnsA : DnsBase
	{
		IPAddress m_Address;

		internal DnsA(DnsResourceRecord resource)
			: base(resource)
		{
			if (m_Resource.m_DataLength != 4)
				throw new InvalidOperationException();

			byte[] RawAddress = new byte[4];

			Buffer.BlockCopy(m_Resource.m_DataBuffer, m_Resource.m_DataOffset, RawAddress, 0, 4);

			m_Address = new IPAddress(RawAddress);
		}

		public IPAddress Address
		{
			get
			{
				return m_Address;
			}
		}
	}

	public class DnsNS : DnsBase
	{
		DnsName m_ServiceName;

		internal DnsNS(DnsResourceRecord resource)
			: base(resource)
		{
			if (m_Resource.m_DataLength < 2)
				throw new InvalidOperationException();

			int Offset = m_Resource.m_DataOffset;

			m_ServiceName = new DnsName(m_Resource.m_DataBuffer, ref Offset);
		}

		public string Server
		{
			get
			{
				return m_ServiceName.ToString();
			}
		}
	}

	public class DnsSOA : DnsBase
	{
		DnsName m_ServiceName;
		DnsName m_Responsible;
		uint m_Serial;
		uint m_Refresh;
		uint m_Retry;
		uint m_Expire;
		uint m_Minimum;

		internal DnsSOA(DnsResourceRecord resource)
			: base(resource)
		{
			if (m_Resource.m_DataLength < 2)
				throw new InvalidOperationException();

			int Offset = m_Resource.m_DataOffset;

			m_ServiceName = new DnsName(m_Resource.m_DataBuffer, ref Offset);
			m_Responsible = new DnsName(m_Resource.m_DataBuffer, ref Offset);
			m_Serial = (uint)((uint)m_Resource.m_DataBuffer[Offset++] << 24) | ((uint)m_Resource.m_DataBuffer[Offset++] << 16) | ((uint)m_Resource.m_DataBuffer[Offset++] << 8) | (uint)m_Resource.m_DataBuffer[Offset++];
			m_Refresh = (uint)((uint)m_Resource.m_DataBuffer[Offset++] << 24) | ((uint)m_Resource.m_DataBuffer[Offset++] << 16) | ((uint)m_Resource.m_DataBuffer[Offset++] << 8) | (uint)m_Resource.m_DataBuffer[Offset++];
			m_Retry = (uint)((uint)m_Resource.m_DataBuffer[Offset++] << 24) | ((uint)m_Resource.m_DataBuffer[Offset++] << 16) | ((uint)m_Resource.m_DataBuffer[Offset++] << 8) | (uint)m_Resource.m_DataBuffer[Offset++];
			m_Expire = (uint)((uint)m_Resource.m_DataBuffer[Offset++] << 24) | ((uint)m_Resource.m_DataBuffer[Offset++] << 16) | ((uint)m_Resource.m_DataBuffer[Offset++] << 8) | (uint)m_Resource.m_DataBuffer[Offset++];
			m_Minimum = (uint)((uint)m_Resource.m_DataBuffer[Offset++] << 24) | ((uint)m_Resource.m_DataBuffer[Offset++] << 16) | ((uint)m_Resource.m_DataBuffer[Offset++] << 8) | (uint)m_Resource.m_DataBuffer[Offset++];
		}

		public string Server
		{
			get
			{
				return m_ServiceName.ToString();
			}
		}
	}

	public class DnsSRV : DnsBase
	{
		DnsName m_ServiceName;
		ushort m_Port;
		ushort m_Weight;
		ushort m_Priority;

		internal DnsSRV(DnsResourceRecord resource)
			: base(resource)
		{
			if (m_Resource.m_DataLength < 8)
				throw new InvalidOperationException();

			int Offset = m_Resource.m_DataOffset;

			m_Priority = (ushort)(((ushort)m_Resource.m_DataBuffer[Offset++] << 8) | (ushort)m_Resource.m_DataBuffer[Offset++]);
			m_Weight = (ushort)(((ushort)m_Resource.m_DataBuffer[Offset++] << 8) | (ushort)m_Resource.m_DataBuffer[Offset++]);
			m_Port = (ushort)(((ushort)m_Resource.m_DataBuffer[Offset++] << 8) | (ushort)m_Resource.m_DataBuffer[Offset++]);
			m_ServiceName = new DnsName(m_Resource.m_DataBuffer, ref Offset);
		}

		public int Priority
		{
			get
			{
				return m_Priority;
			}
		}

		public int Weigth
		{
			get
			{
				return m_Weight;
			}
		}

		public int Port
		{
			get
			{
				return m_Port;
			}
		}

		public string Server
		{
			get
			{
				return m_ServiceName.ToString();
			}
		}
	}

	internal class DnsMessage
	{
		private static ushort m_LastId = 0;

		internal static ushort NextMessageId()
		{
			lock (typeof(DnsMessage))
			{
				return ++m_LastId;
			}
		}

		DnsHeader m_Header;
		List<DnsRequestRecord> m_RequestRecords;
		List<DnsBase> m_ResponseRecords;
		List<DnsBase> m_NameServerRecords;
		List<DnsBase> m_AdditionalRecords;

		public DnsBase[] Responses
		{
			get
			{
				return (m_ResponseRecords == null) ? new DnsBase[0] : m_ResponseRecords.ToArray();
			}
		}

		public DnsBase[] NameServers
		{
			get
			{
				return (m_NameServerRecords == null) ? new DnsBase[0] : m_NameServerRecords.ToArray();
			}
		}

		public DnsBase[] Additionals
		{
			get
			{
				return (m_AdditionalRecords == null) ? new DnsBase[0] : m_AdditionalRecords.ToArray();
			}
		}

		private DnsBase NewSpecificRecord(DnsResourceRecord baseRecord)
		{
			DnsBase SpecificResponse = null;

			switch (baseRecord.Type)
			{
				case DnsResourceType.A:
					SpecificResponse = new DnsA(baseRecord);
					break;

				case DnsResourceType.NS:
					SpecificResponse = new DnsNS(baseRecord);
					break;

				case DnsResourceType.SOA:
					SpecificResponse = new DnsSOA(baseRecord);
					break;

				case DnsResourceType.SRV:
					SpecificResponse = new DnsSRV(baseRecord);
					break;

				default:
					SpecificResponse = new DnsUnknown(baseRecord);
					break;
			}

			return SpecificResponse;
		}

		public DnsMessage(byte[] buffer)
		{
			int Count, Offset = 0;

			m_Header = new DnsHeader(buffer, ref Offset);

			if (m_Header.RequestCount > 0)
			{
				m_RequestRecords = new List<DnsRequestRecord>();

				for (Count = 0; Count < m_Header.RequestCount; Count++)
				{
					m_RequestRecords.Add(new DnsRequestRecord(buffer, ref Offset));
				}
			}

			if (m_Header.ResponseCount > 0)
			{
				m_ResponseRecords = new List<DnsBase>();

				for (Count = 0; Count < m_Header.ResponseCount; Count++)
				{
					m_ResponseRecords.Add(NewSpecificRecord(new DnsResourceRecord(buffer, ref Offset)));
				}
			}

			if (m_Header.NSCount > 0)
			{
				m_NameServerRecords = new List<DnsBase>();

				for (Count = 0; Count < m_Header.NSCount; Count++)
				{
					m_NameServerRecords.Add(NewSpecificRecord(new DnsResourceRecord(buffer, ref Offset)));
				}
			}

			if (m_Header.AdditionalCount > 0)
			{
				m_AdditionalRecords = new List<DnsBase>();

				for (Count = 0; Count < m_Header.AdditionalCount; Count++)
				{
					m_AdditionalRecords.Add(NewSpecificRecord(new DnsResourceRecord(buffer, ref Offset)));
				}
			}
		}

		public DnsMessage(DnsHeader header)
		{
			m_Header = header;
		}

		public void AddRequestRecord(DnsRequestRecord requestRecord)
		{
			if (m_RequestRecords == null)
				m_RequestRecords = new List<DnsRequestRecord>();

			m_Header.RequestCount = m_Header.RequestCount + 1;
			m_RequestRecords.Add(requestRecord);
		}

		public int FillBuffer(byte[] buffer)
		{
			int i, Offset = 0;

			m_Header.ToBuffer(buffer, ref Offset);

			if (m_RequestRecords != null)
			{
				for (i = 0; i < m_RequestRecords.Count; i++)
					m_RequestRecords[i].ToBuffer(buffer, ref Offset);
			}

			return Offset;
		}
	}

	public class DnsRequest
	{
		DnsRequestRecord m_RequestRecord;
		DnsMessage m_Message;

		public DnsRequest(string name, DnsResourceType type, bool recursive)
			: this(name, type, DnsClass.IN, recursive)
		{
		}

		public DnsRequest(string name, DnsResourceType type, DnsClass @class, bool recursive)
		{
			DnsHeader Header = new DnsHeader(DnsMessage.NextMessageId(), DnsOpcode.Standard, recursive);

			Header.IsRequest = true;

			m_Message = new DnsMessage(Header);
			m_RequestRecord = new DnsRequestRecord(new DnsName(name), type, @class);

			m_Message.AddRequestRecord(m_RequestRecord);
		}

		public int FillBuffer(byte[] buffer)
		{
			return m_Message.FillBuffer(buffer);
		}

		public DnsResourceType Type
		{
			get
			{
				return m_RequestRecord.Type;
			}
		}

		public DnsClass Class
		{
			get
			{
				return m_RequestRecord.Class;
			}
		}

		public string Name
		{
			get
			{
				return m_RequestRecord.Name.ToString();
			}
		}
	}

	public class DnsClient
	{
		private List<IPAddress> m_DnsServers = new List<IPAddress>();
		private bool m_DiscoverMode;
		private NetworkInterface[] m_DiscoverInterfaces;
		private Socket m_ClientSocket;
		private int m_MaxSize = 1024;
		private SortedDictionary<string, DnsA> m_AddressCache = new SortedDictionary<string,DnsA>();

		public DnsClient()
		{
			m_DiscoverMode = true;
			m_DiscoverInterfaces = NetworkInterface.GetAllNetworkInterfaces();

			Initialize();
		}

		public DnsClient(NetworkInterface adapter)
		{
			m_DiscoverMode = true;
			m_DiscoverInterfaces = new NetworkInterface[] { adapter };

			Initialize();
		}

		public DnsClient(IPAddress dnsServer)
		{
			m_DiscoverMode = false;
			m_DnsServers.Add(dnsServer);

			Initialize();
		}

		public DnsClient(IPAddress[] dnsServers)
		{
			m_DiscoverMode = false;

			for(int i = 0; i < dnsServers.Length; i++)
				m_DnsServers.Add(dnsServers[i]);

			Initialize();
		}

		private void Initialize()
		{
			if (m_DiscoverMode)
			{
				NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(OnNetworkAddressChanged);

				OnNetworkAddressChanged(null, null);
			}
		}

		private void OnNetworkAddressChanged(object sender, EventArgs e)
		{
			lock (this)
			{
				m_DnsServers.Clear();
				m_MaxSize = 0;

				foreach (NetworkInterface Adapter in m_DiscoverInterfaces)
				{
					LoadDnsServers(Adapter);
				}

				if (m_MaxSize == 0)
					m_MaxSize = 1024;
			}
		}

		private void LoadDnsServers(NetworkInterface adapter)
		{
			IPInterfaceProperties AdapterProperties = adapter.GetIPProperties();
			IPAddressCollection DnsServers = AdapterProperties.DnsAddresses;

			if (DnsServers.Count > 0)
			{
				IPv4InterfaceProperties IPProperties = AdapterProperties.GetIPv4Properties();

				if (m_MaxSize == 0 || IPProperties.Mtu < m_MaxSize)
					m_MaxSize = IPProperties.Mtu;

				foreach (IPAddress DnsServer in DnsServers)
				{
					if (!m_DnsServers.Contains(DnsServer))
						m_DnsServers.Add(DnsServer);
				}
			}
		}

		private DnsBase[] InternalExecuteRequest(Socket Sock, DnsRequest request, int timeout)
		{
			byte[] Buffer = new byte[m_MaxSize];
			byte[] ReceiveBuffer = new byte[m_MaxSize];
			int Length = request.FillBuffer(Buffer);
			int Received;

			IPAddress[] Targets = m_DnsServers.ToArray();
			int CurrentTarget = 0;

			DnsBase[] Responses = new DnsBase[0];

			if (request.Type == DnsResourceType.A)
			{
				lock (m_AddressCache)
				{
					DnsA A;

					if (m_AddressCache.TryGetValue(request.Name, out A))
					{
						if (A.Expires < DateTime.Now)
						{
							m_AddressCache.Remove(request.Name);
						}
						else
						{
							Responses = new DnsBase[] { A };

							return Responses;
						}
					}
				}
			}

			while (CurrentTarget < Targets.Length)
			{
				try
				{
					Sock.SendTo(Buffer, Length, SocketFlags.None, new IPEndPoint(Targets[CurrentTarget], 53));

					if (Sock.Poll(timeout * 1000, SelectMode.SelectRead))
					{
						Received = Sock.Receive(ReceiveBuffer);

						if (Received > 0)
						{
							DnsMessage Message = new DnsMessage(ReceiveBuffer);
							Responses = Message.Responses;

							if (Responses.Length > 0)
							{
								for (int i = 0; i < Responses.Length; i++)
								{
									if (Responses[i].Type == DnsResourceType.A)
									{
										DnsA A = (DnsA)Responses[i];

										lock (m_AddressCache)
										{
											if (m_AddressCache.ContainsKey(A.Name))
												m_AddressCache.Remove(A.Name);

											m_AddressCache.Add(A.Name, A);
										}
									}
								}

								break;
							}
							else
							{
								DnsBase[] NameServers = Message.NameServers;
								DnsBase[] Additionals = Message.Additionals;

								if (Additionals.Length > 0)
								{
									for (int i = 0; i < Additionals.Length; i++)
									{
										if (Additionals[i].Type == DnsResourceType.A)
										{
											DnsA A = (DnsA)Additionals[i];

											lock (m_AddressCache)
											{
												if (m_AddressCache.ContainsKey(A.Name))
													m_AddressCache.Remove(A.Name);

												m_AddressCache.Add(A.Name, A);
											}
										}
									}
								}

								if (NameServers != null)
								{
									List<IPAddress> NewTargets = new List<IPAddress>();

									for (int i = 0; i < NameServers.Length; i++)
									{
										string TargetName = null;
										IPAddress TargetAddress = null;

										if (NameServers[i] is DnsSOA)
										{
											DnsBase[] MainNS = InternalExecuteRequest(Sock, new DnsRequest(NameServers[i].Name, DnsResourceType.NS, true), timeout);

											if (MainNS.Length > 0)
											{
												NameServers = MainNS;
												i = -1;
												continue;
											}

											TargetName = ((DnsSOA)NameServers[i]).Server;
										}
										else if (NameServers[i] is DnsNS)
										{
											TargetName = ((DnsNS)NameServers[i]).Server;
										}

										lock (m_AddressCache)
										{
											if (m_AddressCache.ContainsKey(TargetName))
											{
												DnsA A = m_AddressCache[TargetName];

												if (A.Expires < DateTime.Now)
													m_AddressCache.Remove(TargetName);
												else
													TargetAddress = m_AddressCache[TargetName].Address;
											}
										}

										if (TargetAddress == null)
										{
											DnsBase[] Resolved = InternalExecuteRequest(Sock, new DnsRequest(TargetName, DnsResourceType.A, true), timeout);

											for (int j = 0; j < Resolved.Length; j++)
											{
												if (Resolved[j] is DnsA)
												{
													DnsA A = (DnsA)Resolved[j];

													if (TargetName == A.Name)
													{
														TargetAddress = A.Address;
														break;
													}
												}
											}
										}

										if (TargetAddress != null)
										{
											bool AlreadyIn = false;

											for (int j = 0; j < Targets.Length; j++)
											{
												if (Targets[j].Equals(TargetAddress))
												{
													AlreadyIn = true;
													break;
												}
											}

											if (!AlreadyIn)
											{
												NewTargets.Add(TargetAddress);
											}
										}
									}

									if (NewTargets.Count > 0)
									{
										Targets = NewTargets.ToArray();
										CurrentTarget = 0;

										continue;
									}
								}
							}
						}
					}
				}
				catch
				{
				}

				CurrentTarget++;
			}

			return Responses;
		}

		public DnsBase[] ExecuteRequest(DnsRequest request, int timeout)
		{
			Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			DnsBase[] Responses = InternalExecuteRequest(Sock, request, timeout);


			Sock.Close();

			return Responses;
		}
	}
}
