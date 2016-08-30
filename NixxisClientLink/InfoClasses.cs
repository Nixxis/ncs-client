using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Threading;
using System.Xml;
using System.Diagnostics;

namespace Nixxis.Client
{


    public class BaseQualificationItem : IComparer<BaseQualificationItem>
	{
		private string m_Id;
		private string m_Parent;
		private string m_Description;
		private int m_DisplayOrder;
		private bool m_Argued;
		private int m_Positive;
		private bool m_PositiveUpdatable;
		private int m_Action;

		protected BaseQualificationItem()
		{
		}

		internal BaseQualificationItem(string id, string description, string parent, int displayOrder, bool argued, int positive, bool positiveUpdatable, int action)
		{
			m_Id = id;
			m_Description = description;
			m_Parent = parent;
			m_DisplayOrder = displayOrder;
			m_Argued = argued;
			m_Positive = positive;
			m_PositiveUpdatable = positiveUpdatable;
			m_Action = action;
		}

        internal BaseQualificationItem(BaseQualificationItem bqi)
        {
            m_Id = bqi.m_Id;
            m_Description = bqi.m_Description;
            m_Parent = bqi.Parent;
            m_DisplayOrder = bqi.m_DisplayOrder;
            m_Argued = bqi.m_Argued;
            m_Positive = bqi.m_Positive;
            m_PositiveUpdatable = bqi.m_PositiveUpdatable;
            m_Action = bqi.m_Action;
        }

		public string Id
		{
			get
			{
				return m_Id;
			}
		}

		internal string Parent
		{
			get
			{
				return m_Parent;
			}
		}

		public string Description
		{
			get
			{
				return m_Description;
			}
		}

		public bool Argued
		{
			get
			{
				return m_Argued;
			}
		}

		public int Positive
		{
			get
			{
				return m_Positive;
			}
		}

		public bool PositiveUpdatable
		{
			get
			{
				return m_PositiveUpdatable;
			}
		}

		public int Action
		{
			get
			{
				return m_Action;
			}
		}

        #region IComparer<BaseQualificationItem> Members

        public int Compare(BaseQualificationItem x, BaseQualificationItem y)
        {
            return x.m_DisplayOrder.CompareTo(y.m_DisplayOrder);
        }

        #endregion
    }

	public class QualificationInfo : BaseQualificationItem
	{
		private class ActivityInfo
		{
			public string Id;
			public int Revision;
			public string QualificationId;
			public DateTime LastUpdate = DateTime.Now;

			internal QualificationInfo m_Q;
			internal SortedDictionary<string, BaseQualificationItem> m_BaseQualifications = new SortedDictionary<string, BaseQualificationItem>();
		}

        private BaseQualificationItem m_bqi;

        public BaseQualificationItem Qualification
        {
            get { return m_bqi; }
            set { m_bqi = value; }
        }


		private static SortedDictionary<string, ActivityInfo> m_ActivityReferences = new SortedDictionary<string, ActivityInfo>();

		public static QualificationInfo FromActivityId(HttpLink link, string activityId)
		{
			ActivityInfo A = null;
			string ActivityKey = activityId;
			int ActivityRevision = 0;
			string QualificationId = string.Empty;

			int Sep = activityId.IndexOf('.');

			if (Sep > 0)
			{
				ActivityKey = activityId.Substring(0, Sep);
				Int32.TryParse(activityId.Substring(Sep + 1), out ActivityRevision);
			}

            if (m_ActivityReferences.TryGetValue(activityId, out A))
            {
				m_ActivityReferences.Remove(activityId);
                A = null;
            }

			if(A == null)
			{
				if (link != null)
				{
					ResponseData Data = link.GetResponseData(link.BuildRequest("~getinfo", null, string.Concat(((int)InfoCodes.Qualifications).ToString(), "\r\n", activityId)));

					if (Data != null && Data.Valid)
					{
						A = new ActivityInfo();

						foreach (KeyValuePair<string, string> Pair in Data)
						{
							if (Pair.Key.Equals("ActivityQualification"))
							{
								string[] Details = Pair.Value.Split(new char[] { ';' });

								A.Id = Details[0];

								if (Details.Length > 1)
									A.QualificationId = Details[1];
								else
									A.QualificationId = string.Empty;

								if (Details.Length > 2)
									A.Revision = Convert.ToInt32(Details[2]);
								else
									A.Revision = ActivityRevision;

								if (m_ActivityReferences.ContainsKey(A.Id))
									m_ActivityReferences.Remove(A.Id);

								m_ActivityReferences.Add(A.Id, A);
							}
							else if(Pair.Key.StartsWith("_"))
							{
								string[] Details = Pair.Value.Split(new char[] { ';' }, 10);

								BaseQualificationItem QItem = new BaseQualificationItem(Pair.Key.Substring(1), Microsoft.JScript.GlobalObject.unescape(Details[9]), Details[1], Convert.ToInt32(Details[0]), (Convert.ToInt32(Details[2]) > 0), Convert.ToInt32(Details[3]), (Convert.ToInt32(Details[4]) > 0), Convert.ToInt32(Details[5]));

								if (A.m_BaseQualifications.ContainsKey(QItem.Id))
									A.m_BaseQualifications.Remove(QItem.Id);

								A.m_BaseQualifications.Add(QItem.Id, QItem);
							}
						}

						A.m_Q = new QualificationInfo(A.m_BaseQualifications[A.QualificationId]);
						FillChildren(A, A.m_Q);
					}
				}
			}

			if (A == null)
				return null;

			return A.m_Q;
		}

        private static void FillChildren(ActivityInfo A, QualificationInfo parent)
        {
            parent.m_QualiChilds.Clear();

            foreach (KeyValuePair<string, BaseQualificationItem> kvp in A.m_BaseQualifications)
            {

                if (kvp.Value.Parent == parent.Id)
                {
                    QualificationInfo NewInfo = new QualificationInfo(kvp.Value);

                    parent.m_QualiChilds.Add(NewInfo);
                    FillChildren(A, NewInfo);
                }
            }

            parent.m_QualiChilds.Sort(parent);
        }

        private List<BaseQualificationItem> m_QualiChilds = new List<BaseQualificationItem>();

        public List<BaseQualificationItem> ListQualification
        {
            get { return m_QualiChilds; }
          }

		public static QualificationInfo FromActivityId(string activityId)
		{
			return FromActivityId(null, activityId);
		}

        private QualificationInfo(BaseQualificationItem bqi) : base(bqi)
        {
        }

        public QualificationInfo()
        {
        }
	}

	public class ActivityInfo
	{
		private string m_ActivityId;
		private string m_CampaignId;
		private bool m_IsSearchMode;
		private string m_Description;
		private string m_CampaignDescription;

		public ActivityInfo(string id, bool isSearchMode, string campaignId, string campaignDescription, string description)
		{
			m_ActivityId = id;
			m_IsSearchMode = isSearchMode;
			m_Description = description;
			m_CampaignId = campaignId;
			m_CampaignDescription = campaignDescription;
		}

		public string ActivityId
		{
			get
			{
				return m_ActivityId;
			}
		}

		public bool IsSearchMode
		{
			get
			{
				return m_IsSearchMode;
			}
		}

		public string CampaignId
		{
			get
			{
				return m_CampaignId;
			}
		}

		public string Description
		{
			get
			{
				return m_Description;
			}
		}

		public string CampaignDescription
		{
			get
			{
				return m_CampaignDescription;
			}
		}

		public void InternalRefresh(bool isSearchMode, string description)
		{
			m_IsSearchMode = isSearchMode;
			m_Description = description;
		}
	}

	public class ActivityCollection : KeyedCollection<string, ActivityInfo>
	{
		protected override string GetKeyForItem(ActivityInfo item)
		{
			return item.ActivityId;
		}
	}

	public class ExternalAgentInfo
	{
		public string AgentId { get; private set; }
		public string Description { get; private set; }
		public string Account { get; private set; }
		public string State { get; private set; }

		public ExternalAgentInfo(string agentId, string account, string description, string state)
		{
			AgentId = agentId;
			Account = account;
			Description = description;
			State = state;
		}
	}

	public class ExternalAgentCollection : KeyedCollection<string, ExternalAgentInfo>
	{
		protected override string GetKeyForItem(ExternalAgentInfo item)
		{
			return item.AgentId;
		}
	}

	public class PauseCodeInfo
	{
		private string m_PauseCodeId;
		private string m_Description;

		public PauseCodeInfo(string pauseCodeId, string description)
		{
			m_PauseCodeId = pauseCodeId;
			m_Description = description;
		}

		public string PauseCodeId
		{
			get
			{
				return m_PauseCodeId;
			}
		}

		public string Description
		{
			get
			{
				return m_Description;
			}
		}

		public void InternalRefresh(string description)
		{
			m_Description = description;
		}

        public override string ToString()
        {
            return Description;
        }
	}

	public class PauseCodeCollection : KeyedCollection<string, PauseCodeInfo>
	{
		protected override string GetKeyForItem(PauseCodeInfo item)
		{
			return item.PauseCodeId;
		}
	}

    public class ContactInfo : IContactInfo, IScriptControl2, INotifyPropertyChanged, IDisposable//, ITypedList
	{
		private string m_Id;
        protected HttpLink m_ClientLink;
        private bool m_RequestAgentAction = false;

		private char m_MediaCode;
		private char m_State;
		private DateTime m_Appearance = DateTime.Now;
		private DateTime m_LastStateChange;
        private string m_ContactDuration, m_StateDuration;

		private string m_From;
		private string m_To;
		private string m_UUI;
        private string m_Language;
		private string m_Activity;
		private string m_Queue;
		private string m_Direction;
		private string m_Context;
		private string m_Script;
		private string m_CustomerId;
		private string m_CustomerDescription;
        private string m_ContactListId;
        private string m_ContentLink;
		private int m_ContentType;
        private string m_ContentHandler;
        private string m_Campaign;
        private ContactHistory m_History;

        private SendOrPostCallback m_SendOrPostPropertyChanged;

        public bool RequestAgentAction
        {
            get
            {
                return m_RequestAgentAction;
            }
            set
            {
                m_RequestAgentAction = value; OnPropertyChanged(new PropertyChangedEventArgs("RequestAgentAction"));
            }
        }

        public string ContentHandler
        {
            get
            {
                return m_ContentHandler;
            }
            set
            {
                m_ContentHandler = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ContentHandler"));
            }
        }

        public string ContentLink
        {
            get 
            { 
                return m_ContentLink; 
            }
            set 
            { 
                m_ContentLink = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ContentLink"));
            }
        }

		//	0 = Default
		//	1 = Chat		| 64 = SMS alphabet & size
		//					| 128 = SMS x 3
		//
		public int ContentType
		{
			get
			{
				return m_ContentType;
			}
			set
			{
				m_ContentType = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ContentType"));
            }
		}

        public string ContactListId
        {
            get
            {
                return m_ContactListId;
            }
            internal set
            {
                m_ContactListId = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ContactListId"));
            }
        }

        public string Campaign
        {
            get 
            {
                return m_Campaign;
            }
            internal set 
            {
                m_Campaign = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Campaign"));
            }
        }

		private object m_UserData;

		private ContactInfo()
		{
		}

        public ContactInfo(HttpLink clientLink, char media, string id, string script = null)
		{
            m_ClientLink = clientLink;
			m_MediaCode = media;
			m_State = ' ';
			m_Id = id;
			m_LastStateChange = DateTime.Now;
            m_Script = script;

            m_SendOrPostPropertyChanged = new SendOrPostCallback(SendOrPostPropertyChanged);
		}

        protected ContactInfo(ContactInfo cinfo) : this(cinfo.m_ClientLink, cinfo.MediaCode, cinfo.Id)
        {
            this.Update(cinfo);
        }

		public string Id
		{
			get
			{
				return m_Id;
			}
			internal set
			{
				m_Id = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Id"));
            }
		}

		public char MediaCode
		{
			get
			{
				return m_MediaCode;
			}
			internal set
			{
				m_MediaCode = value;
                OnPropertyChanged(new PropertyChangedEventArgs("MediaCode"));
                OnPropertyChanged(new PropertyChangedEventArgs("Media"));
            }
		}

		public ContactMedia Media
		{
			get
			{
				switch (m_MediaCode)
				{
					case 'V':
						return ContactMedia.Voice;
					case 'M':
						return ContactMedia.Mail;
					case 'C':
						return ContactMedia.Chat;
				}

				return ContactMedia.Unknown;
			}
		}

		public char State
		{
			get
			{
				return m_State;
			}
			internal set
			{
				if (m_State != value)
				{
					m_State = value;
					m_LastStateChange = DateTime.Now;
                    OnPropertyChanged(new PropertyChangedEventArgs("State"));
                    OnPropertyChanged(new PropertyChangedEventArgs("StateDescription"));
                    OnPropertyChanged(new PropertyChangedEventArgs("LastStateChange"));
                    OnPropertyChanged(new PropertyChangedEventArgs("StateDuration"));
                }
			}
		}

        public string StateDescription
        {
            get
            {
                switch (m_State)
                {
                    case 'A':
                        return Strings.ContactState_Alerting;
                    case 'P':
                        return Strings.ContactState_Preview;
                    case 'C':
                        return Strings.ContactState_Connected;
                    case 'D':
                        return Strings.ContactState_Disconnected;
                    case 'X':
                        return Strings.ContactState_Closing;
                    case 'H':
                        return Strings.ContactState_OnHold;
                }

                return m_State.ToString();
            }
        }

        internal void TimerCallbackHandler(object state)
        {
            string Elapsed = DateTime.Now.Subtract(m_Appearance).ToString("hh\\:mm\\:ss");

            if (m_ContactDuration != Elapsed)
            {
                m_ContactDuration = Elapsed;
                OnPropertyChanged(new PropertyChangedEventArgs("ContactDuration"));
            }

            Elapsed = DateTime.Now.Subtract(m_LastStateChange).ToString("hh\\:mm\\:ss");

            if (m_StateDuration != Elapsed)
            {
                m_StateDuration = Elapsed;
                OnPropertyChanged(new PropertyChangedEventArgs("StateDuration"));
            }
        }

        public string ContactDuration
        {
            get
            {
                return m_ContactDuration;
            }
        }

        public string StateDuration
        {
            get
            {
                return m_StateDuration;
            }
        }

		public DateTime LastStateChange
		{
			get
			{
				return m_LastStateChange;
			}
		}

		public DateTime Appearance
		{
			get
			{
				return m_Appearance;
			}
		}

		public string From
		{
			get
			{
				return m_From;
			}
			internal set
			{
				m_From = value;
                OnPropertyChanged(new PropertyChangedEventArgs("From"));
            }
		}

		public string To
		{
			get
			{
				return m_To;
			}
			internal set
			{
				m_To = value;
                OnPropertyChanged(new PropertyChangedEventArgs("To"));
            }
		}

		public string UUI
		{
			get
			{
				return m_UUI;
			}
			internal set
			{
				m_UUI = value;
                OnPropertyChanged(new PropertyChangedEventArgs("UUI"));
            }
		}
        public string Language
        {
            get
            {
                return m_Language;
            }
            internal set
            {
                m_Language = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Language"));
            }
        }

		public string Activity
		{
			get
			{
				return m_Activity;
			}
			internal set
			{
				m_Activity = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Activity"));
            }
		}

		public string Queue
		{
			get
			{
				return m_Queue;
			}
			internal set
			{
				m_Queue = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Queue"));
            }
		}

		public string Direction
		{
			get
			{
				return m_Direction;
			}
			internal set
			{
				m_Direction = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Direction"));
            }
		}

		public string Context
		{
			get
			{
				return m_Context;
			}
			internal set
			{
				m_Context = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Context"));
            }
		}

		public string Script
		{
			get
			{
				return m_Script;
			}
			internal set
			{
				m_Script = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Script"));
            }
		}

		public string CustomerId
		{
			get
			{
				return m_CustomerId;
			}
			internal set
			{
				m_CustomerId = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CustomerId"));
            }
		}

		public string CustomerDescription
		{
			get
			{
				return m_CustomerDescription;
			}
			internal set
			{
				m_CustomerDescription = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CustomerDescription"));
            }
		}

		public void SetCustomerId(string customerId)
		{
			m_CustomerId = customerId;
			m_ClientLink.SetCustomerId(m_Id, customerId);
            OnPropertyChanged(new PropertyChangedEventArgs("CustomerId"));

			if (ContactContentChanged != null)
			{
				try
				{
					ContactContentChanged(this);
				}
				catch
				{
				}
			}
		}

		public void SetCustomerDescription(string customerDescription)
		{
			m_CustomerDescription = customerDescription;
			m_ClientLink.SetCustomerDescription(m_Id, customerDescription);
            OnPropertyChanged(new PropertyChangedEventArgs("CustomerDescription"));

			if (ContactContentChanged != null)
			{
				try
				{
					ContactContentChanged(this);
				}
				catch
				{
				}
			}
		}

		public void SetContactListId(string id)
		{
			m_ContactListId = id;
			m_ClientLink.SetContactListId(m_Id, id);
            OnPropertyChanged(new PropertyChangedEventArgs("ContactListId"));

			if (ContactContentChanged != null)
			{
				try
				{
					ContactContentChanged(this);
				}
				catch
				{
				}
			}
		}

		public string GetPrivateInfo(string key)
		{
			ResponseData Data = m_ClientLink.GetResponseData(m_ClientLink.BuildRequest("~getinfo&fmt=uri", null, string.Concat(((int)InfoCodes.ContactProperty).ToString(), "\r\n", m_Id, "\r\n", key)));

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
			ResponseData Data = m_ClientLink.GetResponseData(m_ClientLink.BuildRequest("~setinfo&fmt=uri", null, string.Concat(((int)InfoCodes.ContactProperty).ToString(), "\r\n", m_Id, "\r\n", key, "\r\n", Microsoft.JScript.GlobalObject.escape(value))));
		}

		public System.Xml.XmlDocument GetContextData()
		{
			return GetContextData(0);
		}

		public System.Xml.XmlDocument GetContextData(int historyLength)
		{
			ResponseData Data = m_ClientLink.GetResponseData(m_ClientLink.BuildRequest("~getinfo&fmt=uri", null, string.Concat(((int)InfoCodes.ContextData).ToString(), "\r\n", m_Id, "\r\n", historyLength.ToString())), true);

			if (Data != null && Data.Valid && !string.IsNullOrEmpty(Data.RawResponse))
			{
				System.Xml.XmlDocument Doc = new System.Xml.XmlDocument();

				try
				{
					Doc.LoadXml(Data.RawResponse);
				}
				catch
				{
					Doc = null;
				}

				return Doc;
			}

			return null;
		}

		public System.Xml.XmlDocument UpdateContextData(System.Xml.XmlDocument data)
		{
			return UpdateContextData(data.OuterXml);
		}

		public System.Xml.XmlDocument UpdateContextData(string data)
		{
			ResponseData Data = m_ClientLink.GetResponseData(m_ClientLink.BuildRequest("~setinfo&fmt=uri", null, string.Concat(((int)InfoCodes.ContextData).ToString(), "\r\n", m_Id, "\r\n", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data)))), true);

            if (Data != null && Data.Valid && !string.IsNullOrEmpty(Data.RawResponse))
            {
                System.Xml.XmlDocument Doc = new System.Xml.XmlDocument();

                try
                {
                    Doc.LoadXml(Data.RawResponse);
                }
                catch
                {
                    Doc = null;
                }

                return Doc;
            }

            return null;
		}

        public ContactHistory GetHistory()
        {

            if (m_History != null && m_History.Count > 0)
                return m_History;

            XmlDocument doc = GetContextData(100);
            m_History = new ContactHistory();

            if (doc != null)
            {
                XmlNode mainNode = doc.SelectSingleNode(@"contextdata/history");

                if (mainNode != null)
                {
                    foreach (XmlNode historyItem in mainNode.ChildNodes)
                    {
                        if (historyItem.Name.Equals("contact", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ContactHistoryItem history = new ContactHistoryItem();
                            history.CurrentContactId = this.Id;
                            history.ContactId = historyItem.Attributes["id"] == null ? string.Empty : historyItem.Attributes["id"].Value;

                            if (!string.IsNullOrEmpty(history.ContactId))
                            {
                                foreach (XmlNode item in historyItem.ChildNodes)
                                {
                                    #region XML Info
                                    try
                                    {
                                        if (item.Name.Equals("ContactId", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.ContactId = item.InnerXml;
                                        }
                                        else if (item.Name.Equals("Id", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.ContactId = item.InnerXml;
                                        }
                                        else if (item.Name.Equals("LocalDateTime", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.LocalDateTime = DateTime.Parse(item.InnerXml);
                                        }
                                        else if (item.Name.Equals("ContactTime", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.ContactTime = item.InnerXml;
                                        }
                                        else if (item.Name.Equals("Media", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.Media = item.InnerXml;
                                        }
                                        else if (item.Name.Equals("Direction", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.Direction = item.InnerXml;
                                        }
                                        else if (item.Name.Equals("SetupTime", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.SetupTime = new TimeSpan(0, 0, int.Parse(item.InnerXml));
                                        }
                                        else if (item.Name.Equals("ComTime", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.ComTime = new TimeSpan(0, 0, int.Parse(item.InnerXml));
                                        }
                                        else if (item.Name.Equals("QueueTime", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.QueueTime = new TimeSpan(0, 0, int.Parse(item.InnerXml));
                                        }
                                        else if (item.Name.Equals("TalkTime", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.TalkTime = new TimeSpan(0, 0, int.Parse(item.InnerXml));
                                        }
                                        else if (item.Name.Equals("Activity", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.Activity = item.InnerXml;
                                            history.ActivityId = item.Attributes["id"].Value;
                                        }
                                        else if (item.Name.Equals("Qualification", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.Qualification = item.InnerXml;
                                            history.QualificationId = item.Attributes["id"] == null ? string.Empty : item.Attributes["id"].Value;
                                        }
                                        else if (item.Name.Equals("QualifiedBy", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.QualifiedBy = item.InnerXml;
                                            history.QualifiedById = item.Attributes["id"] == null ? string.Empty : item.Attributes["id"].Value;
                                        }
                                        else if (item.Name.Equals("Recording", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            history.RecordingId = item.Attributes["id"] == null ? string.Empty : item.Attributes["id"].Value;
                                            history.RecordingMarker = new TimeSpan(0, 0, 0, 0, int.Parse(item.Attributes["marker"] == null ? "0" : item.Attributes["marker"].Value));
                                        }
                                    }
                                    catch (Exception error)
                                    {
                                        System.Diagnostics.Trace.WriteLine("Error processing XML for history of contact: " + error.ToString());
                                    }
                                    #endregion
                                }

                                m_History.Add(history);
                            }
                        }
                    }
                }
            }
            return m_History;
        }

		public object UserData
		{
			get
			{
				return m_UserData;
			}
			set
			{
				m_UserData = value;
                OnPropertyChanged(new PropertyChangedEventArgs("UserData"));
            }
		}

		public virtual bool Update(ContactInfo newInfo)
		{
			bool Updated = false;
            bool IsPreview = (m_State == 'P') ? true : false; 

			if (newInfo.From != null && newInfo.From != this.From)
			{
				m_From = newInfo.From;
				Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("From"));
			}

			if (newInfo.To != null && newInfo.To != this.To)
			{
				m_To = newInfo.To;
				Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("To"));
            }

			if (newInfo.UUI != null && newInfo.UUI != this.UUI)
			{
				m_UUI = newInfo.UUI;
				Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("UUI"));
            }

            if (newInfo.Language != null && newInfo.Language != this.Language)
            {
                m_Language = newInfo.Language;
                Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("Language"));
            }

			if (newInfo.Activity != null && newInfo.Activity != this.Activity)
			{
                if (!IsPreview)
                {
                    m_Activity = newInfo.Activity;
                    Updated = true;
                    OnPropertyChanged(new PropertyChangedEventArgs("Activity"));
                }
			}

			if (newInfo.Queue != null && newInfo.Queue != this.Queue)
			{
				m_Queue = newInfo.Queue;
				Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("Queue"));
            }

			if (newInfo.Direction != null && newInfo.Direction != this.Direction)
			{
				m_Direction = newInfo.Direction;
				Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("Direction"));
            }

			if (newInfo.Context != null && newInfo.Context != this.Context)
			{
				m_Context = newInfo.Context;
				Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("Context"));
            }

			if (newInfo.Script != null && newInfo.Script != this.Script)
			{
				m_Script = newInfo.Script;
				Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("Script"));
            }

			if (newInfo.ContentLink != null && newInfo.ContentLink != this.ContentLink)
			{
				m_ContentLink = newInfo.ContentLink;
				Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("ContentLink"));
            }

            if (newInfo.ContentHandler != null && newInfo.ContentHandler != this.ContentHandler)
            {
                m_ContentHandler = newInfo.ContentHandler;
                Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("ContentHandler"));
            }

			if (newInfo.ContentType != 0 && newInfo.ContentType != this.ContentType)
			{
				m_ContentType = newInfo.ContentType;
				Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("ContentType"));
            }

			if (newInfo.ContactListId != null && newInfo.ContactListId != this.ContactListId)
            {
                m_ContactListId = newInfo.ContactListId;
                Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("ContactListId"));
            }

            if (newInfo.CustomerId != null && newInfo.CustomerId != this.CustomerId)
            {
                m_CustomerId = newInfo.CustomerId;
                Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("CustomerId"));
            }

            if (newInfo.CustomerDescription != null && newInfo.CustomerDescription != this.CustomerDescription)
            {
                m_CustomerDescription = newInfo.CustomerDescription;
                Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("CustomerDescription"));
            }

            if (string.IsNullOrEmpty(m_CustomerDescription) && !string.IsNullOrEmpty(m_CustomerId))
            {
                m_CustomerDescription = m_CustomerId;
                Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("CustomerDescription"));
            }

            if (newInfo.Campaign != null && newInfo.Campaign != this.Campaign)
            {
                if (!IsPreview)
                {
                    m_Campaign = newInfo.Campaign;
                    Updated = true;
                    OnPropertyChanged(new PropertyChangedEventArgs("Campaign"));
                }
            }
            
            if (string.IsNullOrWhiteSpace(m_Script) && m_Direction == "O" && (string.IsNullOrWhiteSpace(m_Activity) || m_Activity.Length < 32) && !string.IsNullOrWhiteSpace(m_ClientLink.ClientSettings["manualCallScript"]))
            {
                m_Script = m_ClientLink.ClientSettings["manualCallScript"];
                Updated = true;
                OnPropertyChanged(new PropertyChangedEventArgs("Script"));
            }

			if (Updated)
			{
				if (ContactContentChanged != null)
				{
					try
					{
						ContactContentChanged(this);
					}
					catch
					{
					}
				}
			}

			return Updated;
		}

		#region IScriptControl2 Members

		public bool NewCall(string destination, ContactMedia media)
		{
			return NewCall(destination, media, null, null, null, null, null);
		}

		public bool NewCall(string destination, ContactMedia media, string contactId, string activity, string customerId, string contactListId, string onBehalfOf)
		{
			switch (media)
			{
				case ContactMedia.Voice:
                    System.Diagnostics.Debug.WriteLine(string.Format("New call to {0} for contact {1}", destination, contactId), "Scripting");

					m_ClientLink.Commands.VoiceNewCall.Execute(
						destination,
						contactId,
						(string.IsNullOrEmpty(activity)) ? null : string.Concat("AcId=", activity), 
						(string.IsNullOrEmpty(customerId)) ? null : string.Concat("CuId=", customerId),
						(string.IsNullOrEmpty(contactListId)) ? null : string.Concat("LiId=", contactListId), 
						(string.IsNullOrEmpty(onBehalfOf)) ? null : string.Concat("OnBehalf=", onBehalfOf));
					return true;
			}

			return false;
		}

        public bool Redial(string destination)
        {
            return NewCall(destination, this.Media, this.Id, this.Activity, this.CustomerId, this.ContactListId, null);
        }

		public bool SetTextContent(string content)
		{
			if (Media == ContactMedia.Mail)
			{
				throw new NotImplementedException();
			}

			return false;
		}

		public string GetTextContent()
		{
			if (Media == ContactMedia.Mail)
			{
				throw new NotImplementedException();
			}

			return null;
		}

		public bool Close(bool sendTextContent)
		{
			if (Media == ContactMedia.Mail && sendTextContent)
			{
				throw new NotImplementedException();
			}

			m_ClientLink.Commands.TerminateContact.Execute(m_Id);

			return true;
		}

		public bool Close(bool sendTextContent, TerminateBehavior terminateBehavior)
		{
			if (terminateBehavior == TerminateBehavior.ForcePause)
				m_ClientLink.Commands.Pause.Execute();

			m_ClientLink.Commands.TerminateContact.Execute(m_Id);

			if (terminateBehavior == TerminateBehavior.ForceReady && m_ClientLink.AutoReady < 0)
			{
				m_ClientLink.Commands.WaitForCall.Execute();
				m_ClientLink.Commands.WaitForChat.Execute();
				m_ClientLink.Commands.WaitForMail.Execute();
			}

			return true;
		}

		public IAgentInfo AgentInfo
		{
			get
			{
				return m_ClientLink;
			}
		}

		public IScriptContainer ScriptContainer
		{
			get
			{
				return m_ClientLink.EventControl as IScriptContainer;
			}
		}

		public System.Collections.Specialized.NameValueCollection ClientSettings
		{
			get
			{
				return m_ClientLink.ClientSettings;
			}
		}

		public event ContactContentChangedDelegate ContactContentChanged;

		#endregion

		#region IScriptControl Members

		public bool NewCall(string destination)
		{
			return NewCall(destination, ContactMedia.Voice, null, null, null, null, null);
		}

		public bool Hold()
		{
			switch (Media)
			{
				case ContactMedia.Voice:
					m_ClientLink.Commands.VoiceHold.Execute(Id);
					return true;

				case ContactMedia.Chat:
					m_ClientLink.Commands.ChatHold.Execute(Id);
					return true;
			}

			return false;
		}

		public bool Retrieve()
		{
			switch (Media)
			{
				case ContactMedia.Voice:
					m_ClientLink.Commands.VoiceRetrieve.Execute(Id);
					return true;

				case ContactMedia.Chat:
					m_ClientLink.Commands.ChatRetrieve.Execute(Id);
					return true;
			}

			return false;
		}

        public bool Hangup()
        {
            switch (Media)
            {
                case ContactMedia.Voice:
                    m_ClientLink.Commands.VoiceHangup.Execute(Id);
                    return true;

                case ContactMedia.Chat:
                    m_ClientLink.Commands.ChatHangup.Execute(Id);
                    return true;
            }

            return false;
        }

		public bool Transfer()
		{
			switch (Media)
			{
				case ContactMedia.Voice:
					m_ClientLink.Commands.VoiceTransfer.Execute(Id);
					return true;
			}

			return false;
		}

		public bool Transfer(TransferFailureBehavior failureBehavior)
		{
			return Transfer();
		}

		public bool Forward(string destination)
		{
			switch (Media)
			{
				case ContactMedia.Voice:
					m_ClientLink.Commands.VoiceForward.Execute(destination, Id);
					return true;

				case ContactMedia.Chat:
					m_ClientLink.Commands.ChatForward.Execute(destination, Id);
					return true;

				case ContactMedia.Mail:
					m_ClientLink.Commands.MailForward.Execute(destination, Id);
					return true;
			}

			return false;
		}

		public bool Forward(string destination, TransferFailureBehavior failureBehavior)
		{
			return Forward(destination);
		}

		public bool Qualify(QualificationInfo qualification)
		{
			m_ClientLink.SetQualification(Id, qualification.Id, null, null);

			return true;
		}

		public bool Qualify(string qualificationId)
		{
			m_ClientLink.SetQualification(Id, qualificationId, null, null);

			return true;
		}

		public bool Qualify(string qualificationId, DateTime callbackTime, string callbackNumber)
		{
			m_ClientLink.SetQualification(Id, qualificationId, callbackTime.ToString("yyyyMMddHHmm"), callbackNumber);

			return true;
		}

		public bool Close()
		{
			return Close(false);
		}

		public bool Close(TerminateBehavior terminateBehavior)
		{
			return Close(false, terminateBehavior);
		}

		public event ContactStateChangedDelegate ContactStateChanged;

		#endregion

        protected void SendOrPostPropertyChanged(object state)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, (PropertyChangedEventArgs)state);
            }
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
                m_ClientLink.Invoke(PropertyChanged, this, args);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void Dispose()
        {
        }
    }

    public class ContactInfoText : ContactInfo
    {
       protected PredefinedTextCollection m_PredefinedTextCollection;

       public ContactInfoText(ContactInfo cinfo): base(cinfo)
       {
       }

       public PredefinedTextCollection PredefinedTexts
       {
           get
           {
               if (m_PredefinedTextCollection == null)
               {
                   m_PredefinedTextCollection = m_ClientLink.GetPredefinedTexts(Activity);
               }
               return m_PredefinedTextCollection;
           }
       }
    }

    public class ContactInfoVoice : ContactInfo
    {
        public ContactInfoVoice(ContactInfo cinfo): base(cinfo)
        {
            this.Update(cinfo);
        } 
    }

        public class ContactInfoMail : MailMessage
        {
            public ContactInfoMail(ContactInfo cinfo)
                : base(cinfo)
            { }
        }

    public class MailMessage : ContactInfoText
    {
        private string m_MailReceivedText;
        private string m_MailToStartReplyText;
        private string m_MailCreationTime;
		private System.Collections.Specialized.NameValueCollection m_ResponseData = new System.Collections.Specialized.NameValueCollection();

        private List<ReceivedAttachmentItem> m_Attachments=null;

        public List<ReceivedAttachmentItem> InfosAttachments { get { return m_Attachments; } }
		public bool IsFakeMail { get; internal set; }

        public MailMessage Message
        {
            get
            {
                return this;
            }
        }

        public MailMessage(ContactInfo cinfo): base(cinfo)
        {
			if ((cinfo.ContentType & 1) == 1)
			{
				IsFakeMail = true;
			}
			else
			{
                this.Subject = this.UUI;

				string[] urlsplit = m_ClientLink.BaseUri.AbsoluteUri.Split('/');
				string url = urlsplit[0] + "//" + urlsplit[2] + ContentLink;
				HttpWebRequest wr;

				wr = WebRequest.Create(url + "responsedata") as HttpWebRequest;
				wr.Method = "GET";

				try
				{
					using (HttpWebResponse response = (HttpWebResponse)wr.GetResponse())
					{
						using (Stream objStream = response.GetResponseStream())
						{
							string[] DataParts = new StreamReader(objStream).ReadToEnd().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

							foreach (string DataPart in DataParts)
							{
								string[] ValueParts = DataPart.Split(new char[] { '=' }, 2);

								if (ValueParts.Length == 2)
								{
									m_ResponseData.Add(ValueParts[0], ValueParts[1]);
								}
							}
						}
					}
				}
				catch (Exception e)
				{
				}

				wr = WebRequest.Create(url + "draft2") as HttpWebRequest;
				wr.Method = "GET";

				try
				{
					using (HttpWebResponse response = (HttpWebResponse)wr.GetResponse())
					{
						using (Stream objStream = response.GetResponseStream())
						{
							string[] DraftParts = new StreamReader(objStream).ReadToEnd().Split(new char[] { '/' }, 2);

							if (!string.IsNullOrEmpty(DraftParts[0]))
							{
								string[] AttList = DraftParts[0].Split(',');

								foreach (string AttId in AttList)
								{
									int Pos = AttId.IndexOf(':');

									if (Pos > 0)
									{
										AttachmentItem Item = new AttachmentItem();

										Item.Id = AttId.Substring(0, Pos);
										Item.DescriptionAttachment = AttId.Substring(Pos + 1);
										Item.InitialChecked = true;

										AttachCollection.Insert(0, Item);
									}
									else
									{
										foreach (AttachmentItem Item in AttachCollection)
										{
                                            if (Item.Id == AttId)
                                                Item.InitialChecked = true;
										}
									}
								}
							}

							m_MailToStartReplyText = DraftParts[1];
						}
					}
				}
				catch (Exception e)
				{
				}

				CreateInfosAttachments();
			}

            this.Update(cinfo);
        }

		public System.Collections.Specialized.NameValueCollection ResponseData
		{
			get
			{
				return m_ResponseData;
			}
		}

        private void CreateInfosAttachments()
        {
            if (m_Attachments == null)
            {
                m_Attachments = new List<ReceivedAttachmentItem>();
                string[] urlsplit = m_ClientLink.BaseUri.AbsoluteUri.Split('/');
                string UrlMailData = urlsplit[0] + "//" + urlsplit[2] + ContentLink + "data";

                HttpWebRequest wr = WebRequest.Create(UrlMailData) as HttpWebRequest;
                wr.Method = "GET";

                try
                {
                    HttpWebResponse response = (HttpWebResponse)wr.GetResponse();

                    try
                    {
                        using (Stream objStream = response.GetResponseStream())
                        {
                            StreamReader sr = new StreamReader(objStream);

                            while (!sr.EndOfStream)
                            {
                                string[] data = sr.ReadLine().Split('=');

                                if (data[0] == "attachment")
                                {
                                    ReceivedAttachmentItem attItem = new ReceivedAttachmentItem();
                                    string[] TabAtt = data[1].Split('&');
                                    attItem.Id = TabAtt[0];
                                    attItem.FileName = Microsoft.JScript.GlobalObject.unescape(TabAtt[1]);
                                    m_Attachments.Add(attItem);                      

                                }
                            }
                        }
                    }
                    finally
                    {
                        response.Close();
                    }
                }
                catch (Exception e)
                {
                }
            }
        }
 
        private AttachmentCollection m_AttachmentCollection;

        public string Subject { get; internal set; }

        public string MailCreationTime
        {
            get
            {
				if (IsFakeMail)
					return "";

                if (m_MailCreationTime == null)
                {
                    string[] urlsplit = m_ClientLink.BaseUri.AbsoluteUri.Split('/');
                    string UrlMailData = urlsplit[0] + "//" + urlsplit[2] + ContentLink + "data";

                    HttpWebRequest wr = WebRequest.Create(UrlMailData) as HttpWebRequest;
                    wr.Method = "GET";

                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)wr.GetResponse();

                        try
                        {
                            using (Stream objStream = response.GetResponseStream())
                            {
                                StreamReader sr = new StreamReader(objStream);

                                while (!sr.EndOfStream)
                                {
                                    string[] data = sr.ReadLine().Split('=');

                                    if (data[0] == "CreationTime")
                                    {
                                        m_MailCreationTime = Microsoft.JScript.GlobalObject.unescape(data[1]);
                                    }
                                }                           
                            }
                        }
                        finally
                        {
                            response.Close();
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }

                return m_MailCreationTime;
            }

        }

        public string MailToStartReplyText 
        {
            get
            {
				if (IsFakeMail)
					return "";

				if (m_MailToStartReplyText == null)
                {
                    //TO BE DONE ON THE SERVER SIDE BECAUSE NOW THE URL RETURN A BAD FORMAT
                }
                return m_MailToStartReplyText;
            }
        }   

        public string MailReceivedText
        {
            get
            {
				if (IsFakeMail)
					return "";

				if (m_MailReceivedText == null)
                {
                  string[] urlsplit = m_ClientLink.BaseUri.AbsoluteUri.Split('/');
                  string url=  urlsplit[0]+"//"+urlsplit[2]+ ContentLink + "htmlbody";        

                    HttpWebRequest wr = WebRequest.Create(url) as HttpWebRequest;
                    wr.Method = "GET";
      
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)wr.GetResponse();

                        try
                        {
                            using (Stream objStream = response.GetResponseStream())
                            {
                                m_MailReceivedText = new StreamReader(objStream).ReadToEnd();
                            }
                        }
                        finally
                        {
                            response.Close();
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
                return m_MailReceivedText;
            }
        }

        public AttachmentCollection AttachCollection
        {
            get
            {
				if (IsFakeMail)
					return null;

				if (m_AttachmentCollection == null)
                {
                    m_AttachmentCollection = m_ClientLink.GetAttachments(Activity);
                }
                return m_AttachmentCollection;
            }
        }

        #region Added by Tom
        public string OriginalMessageUrl 
        { 
            get
            {
                string[] urlsplit = m_ClientLink.BaseUri.AbsoluteUri.Split('/');
                return urlsplit[0] + "//" + urlsplit[2] + ContentLink + "htmlbody";
            }
        }
        
        public string ReplyTo
        {
            get
            {
                return this.ResponseData["To"] ?? this.From;
            }
            set
            {
                if (this.ResponseData["To"] == null)
                    this.ResponseData.Add("To", value);
                else
                    this.ResponseData["To"] = value;
            }
        }
        public string ReplyFrom
        {
            get
            {
                return this.ResponseData["From"] ?? this.To;
            }
            set
            {
                if (this.ResponseData["From"] == null)
                    this.ResponseData.Add("From", value);
                else
                    this.ResponseData["From"] = value;
            }
        }
        public string ReplySubject
        {
            get
            {
                return this.ResponseData["Subject"] ?? "RE: " + this.Subject;
            }
            set
            {
                if (this.ResponseData["Subject"] == null)
                    this.ResponseData.Add("Subject", value);
                else
                    this.ResponseData["Subject"] = value;
            }
        }
        #endregion
    }

    public class ContactInfoChat : ContactInfoText
    {
        private ChatConversation m_Conversation;
        public ChatConversation Conversation
        {
            get { return m_Conversation; }
        }
        
 
        public ContactInfoChat(ContactInfo cinfo): base(cinfo)
        {


        }

        public override bool Update(ContactInfo newInfo)
        {
            bool Result = base.Update(newInfo);

            if (m_Conversation == null && !string.IsNullOrEmpty(this.ContentLink))
            {
                string ConversationId = string.Empty;

                int ConvIndex = this.ContentLink.IndexOf("conversationid=", StringComparison.OrdinalIgnoreCase);

                if (ConvIndex >= 0)
                {
                    int ConvEnd = this.ContentLink.IndexOf('&', ConvIndex);

                    if (ConvEnd >= 0)
                        ConversationId = this.ContentLink.Substring(ConvIndex + 15, ConvEnd - (ConvIndex + 15));
                    else
                        ConversationId = this.ContentLink.Substring(ConvIndex + 15);
                }

                m_Conversation = new ChatConversation(m_ClientLink.LinkNetworkType, new Uri(m_ClientLink.BaseUri, this.ContentLink).ToString(), ConversationId, m_ClientLink.AgentId, m_ClientLink.SyncContext);

                m_Conversation.Join();
            }
            else if (m_Conversation == null) ContactRoute.DiagnosticHelpers.DebugIfPossible();


            return Result;
        }

        public override void Dispose()
        {
            if (State == 'X' && m_Conversation != null)
            {
                m_Conversation.Close();
                m_Conversation = null;
            }

            base.Dispose();
        }

        public void Say(string message)
        {
            m_Conversation.Say(message);
        }

        public void Leave()
        {
            m_Conversation.Leave(null);
        }
    }

    public delegate void AddChatMessageHandler(int lineType, string from, string to, string message);
   
	
    public class PredefinedTextItem
    {
        private string m_Id;

        public string Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }
        private string m_Text;

        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }
        private string m_FullText;

        public string FullText
        {
            get { return m_FullText; }
            set { m_FullText = value; }
        }

        public PredefinedTextItem()
        {
        }
    }

    public class AttachmentItem
    {
        public string Id { get; internal set; }
        public string DescriptionAttachment { get; internal set; }
        public string CampaignId { get; internal set; }
        public int CompatibleMedias { get; internal set; }
        public string Language { get; internal set; }
        public string Location { get; internal set; }
        public bool LocationIsLocal { get; internal set; }
        public bool InlineDisposition { get; internal set; }
        public string Target { get; internal set; }
		public bool InitialChecked { get; internal set; }

		public AttachmentItem()
		{
		}

		public AttachmentItem(string localFile)
		{
			Id = localFile.ToLower();
			DescriptionAttachment = Path.GetFileName(localFile);
			LocationIsLocal = true;
			Location = localFile;
		}
    }

    public class PredefinedTextCollection : System.Collections.ObjectModel.KeyedCollection<string, PredefinedTextItem>
    {
        protected override string GetKeyForItem(PredefinedTextItem item)
        {
            return item.Id;
        }
    }

    public class AttachmentCollection : System.Collections.ObjectModel.KeyedCollection<string, AttachmentItem>
    {
        protected override string GetKeyForItem(AttachmentItem item)
        {
            return item.Id;
        }
    }

    public class ReceivedAttachmentItem
    {
        public string Id { get; internal set; }
        public string FileName { get; internal set; }
        public string PhysicalFile { get; set; }
    }

    public class ChatMessageLine
    {
        public int LineType { get; internal set; }
        public string From { get; internal set; }
        public string To { get; internal set; }
        public string Message { get; internal set; }
    }

}
