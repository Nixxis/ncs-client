using System;
using System.Collections.Generic;
using System.Text;
using ContactRoute.Config;
using System.IO;
using System.Reflection;

namespace ContactRoute.Recording.Config
{
    public class TransferProfileData
    {
        public enum Medias
        {
            Voice = 0,
            Email = 1,
            Chat = 2,
            Office = 3,
        }
        public enum Types
        {
            Ftp = 0,
            Http = 1,
            AppServer = 2,
            StrictHttp = 3,
        }

        #region Class data

        private Medias m_MediaType = Medias.Voice;
        private Types m_Type = Types.AppServer;
        private string m_User = "recording";
        private string m_Password = "recording";
        private string m_Root = "{0}/{1}.{2}";
        private string m_Host = "";
        private string[] m_Extension = new string[] { "wav" , "mp3" };
        private bool m_UseActiveFtp = false;
        private int m_RequestTimeOut = 360;

        #endregion

        #region Constructors
        public TransferProfileData()
        {
        }
        public TransferProfileData(Medias mediaType)
        {
            this.m_MediaType = mediaType;
        }
        #endregion

        #region Properties

        [ConfigFileValueField]
        public Medias MediaType
        {
            get { return m_MediaType; }
            set { m_MediaType = value; }
        }
        [ConfigFileValueField]
        public Types Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }
        [ConfigFileValueField]
        public string Password
        {
            get { return m_Password; }
            set { m_Password = value; }
        }
        [ConfigFileValueField]
        public string Root
        {
            get { return m_Root; }
            set { m_Root = value; }
        }
        [ConfigFileValueField]
        public string User
        {
            get { return m_User; }
            set { m_User = value; }
        }
        [ConfigFileValueField]
        public string Host
        {
            get { return m_Host; }
            set { m_Host = value; }
        }
        [ConfigFileValueField]
        public string[] Extension
        {
            get { return returnArrayNotEmpty(m_Extension); }
            set { m_Extension = value; }
        }
        [ConfigFileValueField]
        public bool UseActiveFtp
        {
            get { return m_UseActiveFtp; }
            set { m_UseActiveFtp = value; }
        }
        [ConfigFileValueField]
        public int RequestTimeOut
        {
            get { return m_RequestTimeOut; }
            set { m_RequestTimeOut = value; }
        }

        #endregion

        private string[] returnArrayNotEmpty(string[] value)
        {
            if (value == null)
                return new string[0];
            else
                return value;
        }
    }

    public class ProfileData
    {
        #region Enums
        private int m_NumSearchItem = 9;
        public enum SearchOptionsList
        {
            SearchType = 0,
            DateTime = 1,
            Contact = 2,
            Campaign = 3,
            Inbound = 4,
            Outbound = 5,
            ManuelCall = 6,
            DirectCall = 7,
            ChatContacts = 8,
        }
        public enum TransferTypes
        {
            Ftp = 0,
            Http = 1,
        }
        public enum ServiceTypes
        {
            AppServer = 0,
            Direct = 1,
        }

        #endregion

        #region Class data
        private string m_Description = "Nixxis recording";
        //General
        private bool m_IsOfficeVisible = false;
        private bool m_IsCallCenterVisible = true;
        private bool m_IsOptionVisible = false;
        private bool m_ScoreAvailable = true;
        private bool m_KeepRecordingAvailable = false;
        private bool m_PreLoadFileNames = false;
        private string m_SearchOptions = "111111111";
        private SortedList<SearchOptionsList, bool> m_SearchOptionArray = new SortedList<SearchOptionsList, bool>();
        private string m_UserXmlKeyFormat = "becaf88d506c4f9bb6f8c691f4643c9f_{0}";
        private bool m_UseCommentVersion0 = false;
        private bool m_UseStreamForHttp = true;
        //Score
        private int m_ScoreLevel = 5;
        private bool m_MultiScore = false;
        private string[] m_ScoreList = { "Politeness", "Assertivity", "Proposal", "Business flow" };
        //Adv
        private bool m_DeleteMainConfig = true;
        private bool m_DebugAllMsg = false;
        private bool m_RecordingIdVisible = false;
        private bool m_ContactListIdVisible = false;
        //Service
        private bool m_ServiceAdminAvailable = true;
        private string m_ServiceAdminRoot = "tcp://appserver:7654";
        private bool m_ServiceDataAvailable = true;
        private string m_ServiceDataRoot = "http://appserver:8088/data";
        private bool m_ServiceUserAvailable = true;
        private string m_UserXmlUrl = "http://appserver:8088/agent/?fmt=uri&action={0}";
        private bool m_ServiceRelayAvailable = true;
        private string m_ServiceRelayRoot = "http://appserver:8088/relay/";
        //Database
        private int m_ReportConnectionType = 0;
        private string m_ReportConnectionString = "Recording";
        private int m_ReportingConnectionTimeout = 180;

        private bool m_ReportingConnectionAppUseV1 = false;
        private bool m_ReportingConnectionAppUseNoNull = true;
        private bool m_ReportingConnectionAppUseSkip = true;
        private string m_ReportingConnectionAppUrlExt = "";

        private int m_AdminConnectionType = 0;
        private string m_AdminConnectionString = "";
        private int m_AdminConnectionTimeout = 180;
        //Transfer
        private TransferProfileData[] m_VoiceServerList = null;
        private TransferProfileData[] m_ChatServerList = null;
        private TransferProfileData[] m_MailServerList = null;
        private TransferProfileData[] m_OfficeServerList = null;

        private int m_TransferCcType = 0;
        private string m_TransferCcPass = "";
        private string m_TransferCcRoot = "";
        private string m_TransferCcUser = "";
        private string[] m_TransferCcIp;
        private string[] m_TransferCcExtension;
        private bool m_TransferCcUseActiveFtp = false;
        private int m_TransferCcRequestTimeOut = 360;

        private int m_TransferOfficeType = 0;
        private string m_TransferOfficePass = "";
        private string m_TransferOfficeRoot = "";
        private string m_TransferOfficeUser = "";
        private string[] m_TransferOfficeIp;
        private string[] m_TransferOfficeExtension;
        private bool m_TransferOfficeUseActiveFtp = false;
        private int m_TransferOfficeRequestTimeOut = 360;

        //User options
        private string m_FileSaveFormatVoice = "{0} {1} - {3} {4}";
        private string m_FileMultiSaveFormatVoice = "{0} {1} - {3} {4}";

        private string m_FileSaveFormatChat = "{0} {1} - {3} {4}";
        private string m_FileMultiSaveFormatChat = "{0} {1} - {3} {4}";

        private string m_FileSaveFormatOffice = "{0} {1} - {3} {4}";
        private string m_FileMultiSaveFormatOffice = "{0} {1} - {3} {4}";

        private string m_FileLastSaveLocation = "";

        private bool m_IncludeInActiveAgents = true;
        private bool m_IncludeInActiveCampaigns = true;
        private bool m_IncludeInActiveActivities = true;
        #endregion

        #region Constructor

        public ProfileData()
            : base()
        {
            SetSearchOptions(m_SearchOptions);
        }

        #endregion

        #region Properties

        //General
        [ConfigFileValueField(UserValue=true)]
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }
        [ConfigFileValueField]
        public bool IsOfficeVisible
        {
            get { return m_IsOfficeVisible; }
            set { m_IsOfficeVisible = value; }
        }
        [ConfigFileValueField]
        public bool IsCallCenterVisible
        {
            get { return m_IsCallCenterVisible; }
            set { m_IsCallCenterVisible = value; }
        }
        [ConfigFileValueField]
        public bool IsOptionVisible
        {
            get { return m_IsOptionVisible; }
            set { m_IsOptionVisible = value; }
        }
        [ConfigFileValueField]
        public bool ScoreAvailable
        {
            get { return m_ScoreAvailable; }
            set { m_ScoreAvailable = value; }
        }
        [ConfigFileValueField]
        public bool KeepRecordingAvailable
        {
            get { return m_KeepRecordingAvailable; }
            set { m_KeepRecordingAvailable = value; }
        }
        [ConfigFileValueField]
        public bool RecordingIdVisible
        {
            get { return m_RecordingIdVisible; }
            set { m_RecordingIdVisible = value; }
        }
        [ConfigFileValueField(UserValue = true)]
        public bool UseStreamForHttp
        {
            get { return m_UseStreamForHttp; }
            set { m_UseStreamForHttp = value; }
        }
        [ConfigFileValueField]
        public bool PreLoadFileNames
        {
            get { return m_PreLoadFileNames; }
            set { m_PreLoadFileNames = value; }
        }
        [ConfigFileValueField(ReadOnly=true)]
        public string UserXmlKeyFormat
        {
            get { return m_UserXmlKeyFormat; }
            set { m_UserXmlKeyFormat = value; }
        }
        [ConfigFileValueField]
        public string SearchOptions
        {
            get { m_SearchOptions = GetSearchOptions(); return m_SearchOptions; }
            set { m_SearchOptions = value; SetSearchOptions(value); }
        }
        public SortedList<SearchOptionsList, bool> SearchOptionsArray
        {
            get
            { //SetSearchOptions(m_SearchOptions); 
                return m_SearchOptionArray;
            }
            set { m_SearchOptionArray = value; }
        }
        [ConfigFileValueField]
        public bool UseCommentVersion0
        {
            get { return m_UseCommentVersion0; }
            set { m_UseCommentVersion0 = value; }
        }
        //Score
        [ConfigFileValueField]
        public int ScoreLevel
        {
            get { return m_ScoreLevel; }
            set { m_ScoreLevel = value; }
        }
        [ConfigFileValueField]
        public bool MultiScore
        {
            get { return m_MultiScore; }
            set { m_MultiScore = value; }
        }
        [ConfigFileValueField]
        public string[] ScoreList
        {
            get
            {
                return returnArrayNotEmpty(m_ScoreList);
            }
            set { m_ScoreList = value; }
        }
        //Adv
        [ConfigFileValueField]
        public bool DeleteMainConfig
        {
            get { return m_DeleteMainConfig; }
            set { m_DeleteMainConfig = value; }
        }
        [ConfigFileValueField]
        public bool DebugAllMsg
        {
            get { return m_DebugAllMsg; }
            set { m_DebugAllMsg = value; }
        }
        [ConfigFileValueField]
        public bool ContactListIdVisible
        {
            get { return m_ContactListIdVisible; }
            set { m_ContactListIdVisible = value; }
        }
        //Service
        [ConfigFileValueField]
        public bool ServiceAdminAvailable
        {
            get { return m_ServiceAdminAvailable; }
            set { m_ServiceAdminAvailable = value; }
        }
        [ConfigFileValueField]
        public string ServiceAdminRoot
        {
            get { return m_ServiceAdminRoot; }
            set { m_ServiceAdminRoot = value; }
        }
        [ConfigFileValueField]
        public bool ServiceDataAvailable
        {
            get { return m_ServiceDataAvailable; }
            set { m_ServiceDataAvailable = value; }
        }
        [ConfigFileValueField]
        public string ServiceDataRoot
        {
            get { return m_ServiceDataRoot; }
            set { m_ServiceDataRoot = value; }
        }
        [ConfigFileValueField]
        public bool ServiceUserAvailable
        {
            get { return m_ServiceUserAvailable; }
            set { m_ServiceUserAvailable = value; }
        }
        [ConfigFileValueField]
        public string UserXmlUrl
        {
            get { return m_UserXmlUrl; }
            set { m_UserXmlUrl = value; }
        }
        [ConfigFileValueField]
        public bool ServiceRelayAvailable
        {
            get { return m_ServiceRelayAvailable; }
            set { m_ServiceRelayAvailable = value; }
        }
        [ConfigFileValueField]
        public string ServiceRelayRoot
        {
            get { return m_ServiceRelayRoot; }
            set { m_ServiceRelayRoot = value; }
        }
        //Database
        [ConfigFileValueField]
        public ServiceTypes ReportConnectionType
        {
            get { return (ServiceTypes)m_ReportConnectionType; }
            set { m_ReportConnectionType = value.GetHashCode(); }
        }
        [ConfigFileValueField]
        public string ReportConnectionString
        {
            get { return m_ReportConnectionString; }
            set { m_ReportConnectionString = value; }
        }
        [ConfigFileValueField]
        public int ReportConnectionTimeout
        {
            get { return m_ReportingConnectionTimeout; }
            set { m_ReportingConnectionTimeout = value; }
        }
        [ConfigFileValueField]
        public bool ReportingConnectionAppUseV1 
        {
            get { return m_ReportingConnectionAppUseV1; }
            set { m_ReportingConnectionAppUseV1 = value; }
        }
        [ConfigFileValueField]
        public bool ReportingConnectionAppUseNoNull 
        {
            get { return m_ReportingConnectionAppUseNoNull; }
            set { m_ReportingConnectionAppUseNoNull = value; }
        }
        [ConfigFileValueField]
        public bool ReportingConnectionAppUseSkip 
        {
            get { return m_ReportingConnectionAppUseSkip; }
            set { m_ReportingConnectionAppUseSkip = value; }
        }
        [ConfigFileValueField]
        public string ReportingConnectionAppUrlExt 
        {
            get { return m_ReportingConnectionAppUrlExt; }
            set { m_ReportingConnectionAppUrlExt = value; }
        }
        [ConfigFileValueField]
        public ServiceTypes AdminConnectionType
        {
            get { return (ServiceTypes)m_AdminConnectionType; }
            set { m_AdminConnectionType = value.GetHashCode(); }
        }
        [ConfigFileValueField]
        public string AdminConnectionString
        {
            get { return m_AdminConnectionString; }
            set { m_AdminConnectionString = value; }
        }
        [ConfigFileValueField]
        public int AdminConnectionTimeout
        {
            get { return m_AdminConnectionTimeout; }
            set { m_AdminConnectionTimeout = value; }
        }
        //Transefer
        [ConfigFileValueField]
        public TransferProfileData[] VoiceServerList
        {
            get { return m_VoiceServerList; }
            set { m_VoiceServerList = value; }
        }
        [ConfigFileValueField]
        public TransferProfileData[] ChatServerList
        {
            get { return m_ChatServerList; }
            set { m_ChatServerList = value; }
        }
        [ConfigFileValueField]
        public TransferProfileData[] OfficeServerList
        {
            get { return m_OfficeServerList; }
            set { m_OfficeServerList = value; }
        }
        [ConfigFileValueField]
        public TransferProfileData[] MailServerList
        {
            get { return m_MailServerList; }
            set { m_MailServerList = value; }
        }

        [ConfigFileValueField(ReadOnly=true)]
        public TransferTypes TransferCcType
        {
            get { return (TransferTypes)m_TransferCcType; }
            set { m_TransferCcType = value.GetHashCode(); }
        }
        [ConfigFileValueField(ReadOnly = true)]
        public string TransferCcPass
        {
            get { return m_TransferCcPass; }
            set { m_TransferCcPass = value; }
        }
        [ConfigFileValueField(ReadOnly = true)]
        public string TransferCcRoot
        {
            get { return m_TransferCcRoot; }
            set { m_TransferCcRoot = value; }
        }
        [ConfigFileValueField(ReadOnly = true)]
        public string TransferCcUser
        {
            get { return m_TransferCcUser; }
            set { m_TransferCcUser = value; }
        }
        [ConfigFileValueField]
        public string[] TransferCcIp
        {
            get 
            { 
                return returnArrayNotEmpty(m_TransferCcIp); 
            }
            set { m_TransferCcIp = value; }
        }
        [ConfigFileValueField(ReadOnly = true)]
        public string[] TransferCcExtension
        {
            get { return returnArrayNotEmpty(m_TransferCcExtension); }
            set { m_TransferCcExtension = value; }
        }
        [ConfigFileValueField(ReadOnly = true)]
        public bool TransferCcUseActiveFtp
        {
            get { return m_TransferCcUseActiveFtp; }
            set { m_TransferCcUseActiveFtp = value; }
        }
        [ConfigFileValueField(ReadOnly = true)]
        public int TransferCcRequestTimeOut
        {
            get { return m_TransferCcRequestTimeOut; }
            set { m_TransferCcRequestTimeOut = value; }
        }

        [ConfigFileValueField(ReadOnly = true)]
        public TransferTypes TransferOfficeType
        {
            get { return (TransferTypes)m_TransferOfficeType; }
            set { m_TransferOfficeType = value.GetHashCode(); }
        }
        [ConfigFileValueField(ReadOnly = true)]
        public string TransferOfficePass
        {
            get { return m_TransferOfficePass; }
            set { m_TransferOfficePass = value; }
        }
        [ConfigFileValueField(ReadOnly = true)]
        public string TransferOfficeRoot
        {
            get { return m_TransferOfficeRoot; }
            set { m_TransferOfficeRoot = value; }
        }
        [ConfigFileValueField(ReadOnly = true)]
        public string TransferOfficeUser
        {
            get { return m_TransferOfficeUser; }
            set { m_TransferOfficeUser = value; }
        }
        [ConfigFileValueField]
        public string[] TransferOfficeIp
        {
            get { return returnArrayNotEmpty(m_TransferOfficeIp); }
            set { m_TransferOfficeIp = value; }
        }
        [ConfigFileValueField(ReadOnly = true)]
        public string[] TransferOfficeExtension
        {
            get { return returnArrayNotEmpty(m_TransferOfficeExtension); }
            set { m_TransferOfficeExtension = value; }
        }
        [ConfigFileValueField(ReadOnly = true)]
        public bool TransferOfficeUseActiveFtp
        {
            get { return m_TransferOfficeUseActiveFtp; }
            set { m_TransferOfficeUseActiveFtp = value; }
        }
        [ConfigFileValueField(ReadOnly = true)]
        public int TransferOfficeRequestTimeOut
        {
            get { return m_TransferOfficeRequestTimeOut; }
            set { m_TransferOfficeRequestTimeOut = value; }
        }

        //User Option
        [ConfigFileValueField(UserValue = true)]
        public string FileSaveFormatVoice
        {
            get { return m_FileSaveFormatVoice; }
            set { m_FileSaveFormatVoice = value; }
        }
        [ConfigFileValueField(UserValue = true)]
        public string FileMultiSaveFormatVoice
        {
            get { return m_FileMultiSaveFormatVoice; }
            set { m_FileMultiSaveFormatVoice = value; }
        }
        [ConfigFileValueField(UserValue = true)]
        public string FileLastSaveLocation
        {
            get { return m_FileLastSaveLocation; }
            set { m_FileLastSaveLocation = value; }
        }

        [ConfigFileValueField(UserValue = true)]
        public string FileSaveFormatChat
        {
            get { return m_FileSaveFormatChat; }
            set { m_FileSaveFormatChat = value; }
        }
        [ConfigFileValueField(UserValue = true)]
        public string FileMultiSaveFormatChat
        {
            get { return m_FileMultiSaveFormatChat; }
            set { m_FileMultiSaveFormatChat = value; }
        }

        [ConfigFileValueField(UserValue = true)]
        public string FileSaveFormatOffice
        {
            get { return m_FileSaveFormatOffice; }
            set { m_FileSaveFormatOffice = value; }
        }
        [ConfigFileValueField(UserValue = true)]
        public string FileMultiSaveFormatOffice
        {
            get { return m_FileMultiSaveFormatOffice; }
            set { m_FileMultiSaveFormatOffice = value; }
        }

        [ConfigFileValueField(UserValue = true)]
        public bool IncludeInActiveAgents
        {
            get { return m_IncludeInActiveAgents; }
            set { m_IncludeInActiveAgents = value; }
        }
        [ConfigFileValueField(UserValue = true)]
        public bool IncludeInActiveCampaigns
        {
            get { return m_IncludeInActiveCampaigns; }
            set { m_IncludeInActiveCampaigns = value; }
        }
        [ConfigFileValueField(UserValue = true)]
        public bool IncludeInActiveActivities
        {
            get { return m_IncludeInActiveActivities; }
            set { m_IncludeInActiveActivities = value; }
        }
        #endregion

        #region Members

        private void SetSearchOptions(string value)
        {
            m_SearchOptionArray.Clear();
            for (int i = 0; i < value.Length; i++)
            {
                if (value.Substring(i, 1) == "1")
                    m_SearchOptionArray.Add((SearchOptionsList)i, true);
                else
                    m_SearchOptionArray.Add((SearchOptionsList)i, false);
            }
            if (m_SearchOptionArray.Count < m_NumSearchItem)
            {
                for (int i = m_SearchOptionArray.Count; i < m_NumSearchItem; i++)
                {
                    m_SearchOptionArray.Add((SearchOptionsList)i, true);
                }
            }
        }
        private string GetSearchOptions()
        {
            string returnStr = "";
            string[] tmpStr = new string[m_SearchOptionArray.Count];

            foreach (SearchOptionsList key in m_SearchOptionArray.Keys)
            {
                if (m_SearchOptionArray[key])
                    tmpStr[(int)key] = "1";
                else
                    tmpStr[(int)key] = "0";
            }

            foreach (string item in tmpStr)
                returnStr += item;

            if (returnStr.Length < m_NumSearchItem)
            {
                for (int i = returnStr.Length; i < m_NumSearchItem; i++)
                {
                    returnStr += 1;
                }
            }
            return returnStr;
        }

        private string[] returnArrayNotEmpty(string[] value)
        {
            if (value == null)
                return new string[0];
            else
                return value;
        }
        #endregion
    }

    public class Profiles
    {
        private string m_Folder;
        private List<Profile> m_Profiles = new List<Profile>();
        private string m_Type;
        private string m_UserModeId = "";

        public Profiles()
        { }
        public Profiles(string folder)
        {
            m_Folder = folder;
        }
        public Profiles(string folder, bool load)
        {
            m_Folder = folder;
            if (load) Load();
        }
        public Profiles(string folder, bool load, string useridmode)
        {
            m_Folder = folder;
            m_UserModeId = useridmode;
            if (load) Load();
        }

        public string Folder
        {
            get { return m_Folder; }
            set { m_Folder = value; }
        }
        public string Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }
        public string UserModeId
        {
            get { return m_UserModeId; }
            set { m_UserModeId = value; }
        }
        public List<Profile> Values
        {
            get { return m_Profiles; }
            set { m_Profiles = value; }
        }

        public int Load()
        {
            string[] files = Directory.GetFiles(m_Folder, "*.config");
            m_Profiles.Clear();

            foreach (string item in files)
            {
                Profile profile = new Profile();

                profile.ConfigFile = item;
                profile.UserModeId = m_UserModeId;
                string type = profile.GetConfigType();
                if (type != null)
                {
                    if (type == "crrecording")
                    {
                        profile.Load();
                        m_Profiles.Add(profile);
                    }
                    if (type == string.Empty)
                    {
                        profile.Load();
                        m_Profiles.Add(profile);
                    }
                }
            }

            return m_Profiles.Count;
        }
    }

    //
    //Display list
    //

    public class DisplayEnumItem
    {
        private int m_Id;
        private string m_Description;

        public DisplayEnumItem(int id, string description)
        {
            m_Id = id;
            m_Description = description;
        }

        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }
    }

    public class Profile : BaseProfile
    {
        #region Constructor

        public Profile()
            : base(typeof(ProfileData))
        {
            m_DataObj = new ProfileData();
        }

        #endregion

        #region Properties

        public ProfileData Data
        {
            get { return (ProfileData)m_DataObj; }
            set { m_DataObj = value; }
        }

        public override bool Load()
        {
            base.Load();

            //
            //For compatibility
            //
            if (this.Data.TransferCcIp.Length > 0)
            {
                foreach (string ip in this.Data.TransferCcIp)
                {
                    TransferProfileData newItem = new TransferProfileData(TransferProfileData.Medias.Voice);
                    newItem.Host = ip;
                    newItem.Extension = this.Data.TransferCcExtension;
                    newItem.Password = this.Data.TransferCcPass;
                    newItem.RequestTimeOut = this.Data.TransferCcRequestTimeOut;
                    newItem.Root = this.Data.TransferCcRoot;
                    newItem.Type = this.Data.TransferCcType == ProfileData.TransferTypes.Http ? TransferProfileData.Types.AppServer : TransferProfileData.Types.Ftp;
                    newItem.UseActiveFtp = this.Data.TransferCcUseActiveFtp;
                    newItem.User = this.Data.TransferCcUser;

                    if (this.Data.VoiceServerList == null) this.Data.VoiceServerList = new TransferProfileData[0];
                    TransferProfileData[] newArray = new TransferProfileData[this.Data.VoiceServerList.Length + 1];
                    Array.Copy(this.Data.VoiceServerList, newArray, this.Data.VoiceServerList.Length);
                    newArray[newArray.Length - 1] = newItem;
                    this.Data.VoiceServerList = newArray;
                }
                this.Data.TransferCcIp = new string[0];
            }
            if (this.Data.TransferOfficeIp.Length > 0)
            {
                foreach (string ip in this.Data.TransferOfficeIp)
                {
                    TransferProfileData newItem = new TransferProfileData(TransferProfileData.Medias.Office);
                    newItem.Host = ip;
                    newItem.Extension = this.Data.TransferOfficeExtension;
                    newItem.Password = this.Data.TransferOfficePass;
                    newItem.RequestTimeOut = this.Data.TransferOfficeRequestTimeOut;
                    newItem.Root = this.Data.TransferOfficeRoot;
                    newItem.Type = this.Data.TransferOfficeType == ProfileData.TransferTypes.Http ? TransferProfileData.Types.AppServer : TransferProfileData.Types.Ftp;
                    newItem.UseActiveFtp = this.Data.TransferOfficeUseActiveFtp;
                    newItem.User = this.Data.TransferOfficeUser;

                    if (this.Data.OfficeServerList == null) this.Data.OfficeServerList = new TransferProfileData[0];
                    TransferProfileData[] newArray = new TransferProfileData[this.Data.OfficeServerList.Length + 1];
                    Array.Copy(this.Data.OfficeServerList, newArray, this.Data.OfficeServerList.Length);
                    newArray[newArray.Length - 1] = newItem;
                    this.Data.OfficeServerList = newArray;
                }
                this.Data.TransferOfficeIp = new string[0];
            }

            return true;
        }

        #endregion
    }
}


