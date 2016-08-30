using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ContactRoute.Recording.Config;
using System.Threading;
using System.Data;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;
using Common.Tools.IO;
using Nixxis.Client.Admin;
using System.Collections.Specialized;
using Nixxis.Client.Controls;
using System.Reflection;

namespace Nixxis.Client.Recording
{
    /// <summary>
    /// Interaction logic for CallCenterLoadingContacts.xaml
    /// </summary>
    public partial class CallCenterLoadingContacts : Window
    {
        public static TranslationContext TranslationContext = new TranslationContext("CallCenterLoadingContacts");
        #region Class Data
        private System.Timers.Timer m_Timer = new System.Timers.Timer();
        private DateTime m_TimerStart = DateTime.MinValue;
        private SearchParameters m_SearchParameters = new SearchParameters();
        private ProfileData m_ConfigData = null;
        private IDataReader m_DataReader = null;
        private FileList m_FileList = null;
        private bool m_Error = false;
        private ContactList m_Contacts = new ContactList();
        private CallCenterControl m_ccControl;
        private int _MaxRecord = -1;
        private bool m_StopLoading = false;
        private BackgroundWorker m_BkgWorker = null;
        private AdminLight m_AdminLight = null;
        #endregion
        
        #region Properties
        public SearchParameters SearchParameters
        {
            get { return m_SearchParameters; }
            set { m_SearchParameters = value; }
        }
        public AdminLight AdminCore
        {
            set { m_AdminLight = value; }
        }
        public ProfileData ConfigData
        {
            get { return m_ConfigData; }
            set { m_ConfigData = value; }
        }
        public ContactList ContactList
        {
            get { return m_Contacts; }
            set { m_Contacts = value; }
        }
        public CallCenterControl ccControl
        {
            get { return m_ccControl; }
            set { m_ccControl = value; }
        }
        #endregion

        #region Constructor
        public CallCenterLoadingContacts()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();

            m_Timer.Interval = 500;
            m_Timer.Elapsed += new System.Timers.ElapsedEventHandler(ShowExecuteTime);
        }

        static CallCenterLoadingContacts()
        {
            string strErr = string.Empty;
        }
        #endregion

        #region Members
        private void LoadRecord()
        {
            Tools.Log(string.Format("Connection type {0}", m_ConfigData.ReportConnectionType.ToString()), Tools.LogType.Info);

            if (m_ConfigData.ReportConnectionType == ProfileData.ServiceTypes.AppServer)
            {
                lblHeading.Content = TranslationContext.Translate("Searching on App Server ...");

                m_DataReader = null;

                m_BkgWorker = new BackgroundWorker();
                m_BkgWorker.DoWork += new DoWorkEventHandler((DoWorkEventHandler)((a, b) => 
                {
                    Tools.Log(string.Format("BackgroundWorker started."), Tools.LogType.Info);
                    
                    LoadFromApplicationServer(a, b);
                    
                    if (m_DataReader != null)
                    {
                        CreateContactList(m_DataReader, a, b);
                    }
                    Tools.Log(string.Format("BackgroundWorker Stopped."), Tools.LogType.Info);
                }));
                m_BkgWorker.ProgressChanged += new ProgressChangedEventHandler(m_LoadContactsWorker_ProgressChanged);
                m_BkgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loadContactsWorker_RunWorkerCompleted);
                m_BkgWorker.RunWorkerAsync();
            }
            else
            {
                lblHeading.Content = TranslationContext.Translate("Searching on Sql Server ...");
                //TO DO: Load directly from the database
            }
        }
 

        private void LoadFromApplicationServer(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = ((BackgroundWorker)sender);

            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;

            m_Timer.Start();

            //
            //Parameters
            //
            string parameterFormat = "&{0}={1},{2}";
            string parameterFormatNull = "&{0}={1}";
            StringBuilder str = new StringBuilder();

            //Date/Time selection
            str.Append(string.Format(parameterFormat, "@fromDate", DataTypeCode.DateTimeDataType, m_SearchParameters.FromDate.Date.Add(m_SearchParameters.FromTime).ToString("yyyyMMddHHmmss")));
            str.Append(string.Format(parameterFormat, "@toDate", DataTypeCode.DateTimeDataType, m_SearchParameters.ToDate.Date.Add(m_SearchParameters.ToTime).ToString("yyyyMMddHHmmss")));

            
            str.Append(string.Format(parameterFormat, "@user", DataTypeCode.StringDataType, Uri.EscapeDataString(m_AdminLight.UserId)));

            if (m_SearchParameters.TeamItem == null || m_SearchParameters.TeamItem.Id == CreateDisplayLists.NoSelection)
            {
                if (m_SearchParameters.AgentItem == null || m_SearchParameters.AgentItem.Id == CreateDisplayLists.NoSelection)
                    str.Append(string.Format(parameterFormatNull, "@agentId", DataTypeCode.NullValue));
                else
                    str.Append(string.Format(parameterFormat, "@agentId", DataTypeCode.StringDataType, Uri.EscapeUriString(m_SearchParameters.AgentItem.Id)));
            }
            else
            {
                if (m_SearchParameters.AgentItem == null || m_SearchParameters.AgentItem.Id == CreateDisplayLists.NoSelection)
                {
                    string agentsByTeam = string.Empty;


                    foreach (AdminObjectLight rfo in m_AdminLight.Agents.Where((a) => (a.Related.Contains(m_SearchParameters.TeamItem.Id))))
                    {
                        if (string.IsNullOrEmpty(agentsByTeam))
                            agentsByTeam = rfo.Id;
                        else
                            agentsByTeam = agentsByTeam + "," + rfo.Id;

                    }

                    str.Append(string.Format(parameterFormat, "@agentId", DataTypeCode.StringDataType, Uri.EscapeUriString(agentsByTeam)));
                }
                else
                    str.Append(string.Format(parameterFormat, "@agentId", DataTypeCode.StringDataType, Uri.EscapeUriString(m_SearchParameters.AgentItem.Id)));
            }

            if (m_SearchParameters.CampaignItem == null || m_SearchParameters.CampaignItem.Id == CreateDisplayLists.NoSelection)
                str.Append(string.Format(parameterFormatNull, "@campaignId", DataTypeCode.NullValue));
            else
                str.Append(string.Format(parameterFormat, "@campaignId", DataTypeCode.StringDataType, Uri.EscapeUriString(m_SearchParameters.CampaignItem.Id)));

            if (string.IsNullOrEmpty(m_SearchParameters.Originator.Trim()))
                str.Append(string.Format(parameterFormatNull, "@originator", DataTypeCode.NullValue));
            else
                str.Append(string.Format(parameterFormat, "@originator", DataTypeCode.StringDataType, Uri.EscapeUriString(m_SearchParameters.Originator)));

            if (string.IsNullOrEmpty(m_SearchParameters.Destination.Trim()))
                str.Append(string.Format(parameterFormatNull, "@destination", DataTypeCode.NullValue));
            else
                str.Append(string.Format(parameterFormat, "@destination", DataTypeCode.StringDataType, Uri.EscapeUriString(m_SearchParameters.Destination)));

            if (string.IsNullOrEmpty(m_SearchParameters.Extension.Trim()))
                str.Append(string.Format(parameterFormatNull, "@Extension", DataTypeCode.NullValue));
            else
                str.Append(string.Format(parameterFormat, "@Extension", DataTypeCode.StringDataType, Uri.EscapeUriString(m_SearchParameters.Extension)));

            str.Append(string.Format(parameterFormat, "@includeWithoutRecording", DataTypeCode.BoolDataType, false.ToString()));

            if (m_SearchParameters.ActivityItem == null || m_SearchParameters.ActivityItem.Id == CreateDisplayLists.NoSelection)
                str.Append(string.Format(parameterFormatNull, "@activityId", DataTypeCode.NullValue));
            else
                str.Append(string.Format(parameterFormat, "@activityId", DataTypeCode.StringDataType, Uri.EscapeUriString(m_SearchParameters.ActivityItem.Id)));
                
            if (m_SearchParameters.IncludeQualification && !string.IsNullOrEmpty(m_SearchParameters.Qualification))
                str.Append(string.Format(parameterFormat, "@QualificationId", DataTypeCode.StringDataType, Uri.EscapeUriString(m_SearchParameters.Qualification)));
            else
                str.Append(string.Format(parameterFormatNull, "@QualificationId", DataTypeCode.NullValue));

            str.Append(string.Format(parameterFormat, "@IncludeInbound", DataTypeCode.BoolDataType, m_SearchParameters.IncludeInbound.ToString()));

            str.Append(string.Format(parameterFormat, "@IncludeManual", DataTypeCode.BoolDataType, m_SearchParameters.IncludeManual.ToString()));

            str.Append(string.Format(parameterFormat, "@IncludeOutbound", DataTypeCode.BoolDataType, m_SearchParameters.IncludeOutbound.ToString()));

            str.Append(string.Format(parameterFormat, "@IncludeDirect", DataTypeCode.BoolDataType, m_SearchParameters.IncludeDirect.ToString()));

            str.Append(string.Format(parameterFormat, "@IncludeChat", DataTypeCode.BoolDataType, m_SearchParameters.IncludeChat.ToString()));

            if (m_SearchParameters.PositiveCheck)
                str.Append(string.Format(parameterFormat, "@FilterPositive", DataTypeCode.IntDataType, m_SearchParameters.Positive.ToString()));
            else
                str.Append(string.Format(parameterFormatNull, "@FilterPositive", DataTypeCode.NullValue));

            if (m_SearchParameters.ArguedCheck)
            {
                if (m_SearchParameters.Argued)
                    str.Append(string.Format(parameterFormat, "@FilterArgued", DataTypeCode.IntDataType, 1));
                else
                    str.Append(string.Format(parameterFormat, "@FilterArgued", DataTypeCode.IntDataType, 0));
            }
            else
                str.Append(string.Format(parameterFormatNull, "@FilterArgued", DataTypeCode.NullValue));

            //
            //Getting request
            //

            HttpWebResponse response = null;
            DataSet ds = new DataSet();
            StreamReader reader = null;
            try
            {
                string requestStr = m_ConfigData.ServiceDataRoot + AppServerRequest.DataAction.RemoteData + "&source=" + m_ConfigData.ReportConnectionString + "&exec=SearchRecordings&timeout=" + m_ConfigData.ReportConnectionTimeout + str.ToString();
                if (m_ConfigData.ReportingConnectionAppUseV1)
                    requestStr += "&v1=1";
                if (m_ConfigData.ReportingConnectionAppUseNoNull)
                    requestStr += "&nonull=true";
                if (m_ConfigData.ReportingConnectionAppUseSkip)
                    requestStr += "&skip=0";

                requestStr += m_ConfigData.ReportingConnectionAppUrlExt;

                Tools.Log("HTTP Request search list: " + requestStr, Tools.LogType.Info);

                WebRequest request = HttpWebRequest.Create(requestStr);

                // These lines are needed, else exception is thrown one time over two:
                // The server committed a protocol violation. Section=ResponseStatusLine
                ((HttpWebRequest)request).KeepAlive = false;
                ((HttpWebRequest)request).ServicePoint.Expect100Continue = false;

                
                request.Timeout = (m_ConfigData.ReportConnectionTimeout * 1000) + 10;
                
                
                
                IAsyncResult ar = request.BeginGetResponse(new AsyncCallback((a) => { response = request.EndGetResponse(a) as HttpWebResponse;  }), null);

                
                while (!ar.IsCompleted)
                {
                    if (worker.CancellationPending)
                    {
                        request.Abort();
                        return;
                    }
                    System.Threading.Thread.Sleep(50);
                }
                Tools.Log("-- GET --> IAsyncResult.IsCompleted: " + ar.IsCompleted, Tools.LogType.Debug);
                if(response == null)
                    Tools.Log("-- GET --> Response length: null", Tools.LogType.Debug);
                else
                    Tools.Log("-- GET --> Response length: " + response.ContentLength.ToString(), Tools.LogType.Debug);

                if (response == null)
                    return;

                using (MemoryStream MS = new MemoryStream())
                {
                    byte[] Tmp = new byte[1500];
                    int Read;

                    BackgroundWorkerProgress progressState = new BackgroundWorkerProgress();
                    progressState.Maximum = -1;
                    progressState.InPrecent = false;
                    progressState.ProgressLabelFormat = TranslationContext.Translate("Downloading {4}");
                    progressState.Description = TranslationContext.Translate("Downloading results");
                    m_Timer.Stop();

                    using (Stream ResponseStream = response.GetResponseStream())
                    {
                        worker.ReportProgress(-2, progressState);

                        int Length = (int)response.ContentLength;
                        int Loop = 0;

                        Tools.Log("-- GET --> Worker progress. ContentLength: " + Length, Tools.LogType.Debug);

                        while ((Read = ResponseStream.Read(Tmp, 0, Tmp.Length)) > 0)
                        {
                            Loop++;
                            progressState.CurrentProgress = Tmp.Length * Loop;
                            worker.ReportProgress(-2, progressState);

                            MS.Write(Tmp, 0, Read);
                            
                            if (worker.CancellationPending)
                            {
                                return;
                            }
                        }
                    }


                    Tools.Log(string.Format("-- GET --> Worker progress. ContactData size is {0}.", Format.ToDisplayByte(progressState.CurrentProgress)), Tools.LogType.Debug);
                    MS.Seek(0, SeekOrigin.Begin);

                    ds.ReadXml(MS, XmlReadMode.ReadSchema);

                    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count == 0)
                    {
                        MS.Seek(0, SeekOrigin.Begin);

                        ds.ReadXml(MS, XmlReadMode.IgnoreSchema);
                    }
                }

                if (worker.CancellationPending)
                {
                    return;
                }

                m_DataReader = ds.CreateDataReader();
            }
            catch (Exception error)
            {
                m_Error = true;
                Tools.Log("HTTP error while getting search list. Error: " + error.ToString(), Tools.LogType.Error);
            }
            finally
            {
                m_Timer.Stop();
                ShowExecuteTime(null, null);
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
            }
        }

        private void CreateContactList(IDataReader dataReader, object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = ((BackgroundWorker)sender);

            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;

            try
            {
                m_Error = false;
                BackgroundWorkerProgress progressState = new BackgroundWorkerProgress();
                int maxRecords = 0;
                if (dataReader != null)
                {
                    dataReader.Read();

                    try { maxRecords = int.Parse(dataReader[0].ToString()); }
                    catch { }
                }

                progressState.Maximum = maxRecords;
                progressState.ProgressLabelFormat = TranslationContext.Translate("Record {0} of {1}.");
                progressState.Description = TranslationContext.Translate("Loading records");

                worker.ReportProgress(-2, progressState);

                this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {                    
                    if (m_ccControl.Contacts == null)
                        m_ccControl.Contacts = new Recording.ContactList();

                    m_ccControl.Contacts.Clear();
                }));
                dataReader.NextResult();
                
                if (dataReader == null) return;
                int count = 0;
                while (dataReader.Read() && (_MaxRecord == -1 || count < _MaxRecord) && !m_StopLoading)
                {
                    count++;
                    progressState.CurrentProgress = count;
                    worker.ReportProgress(-2, progressState);

                    //
                    //Filling data
                    //
                    int iConvert;
                    string sConvert;
                    bool bConvert;
                    DateTime dConvert;

                    ContactData data = new ContactData();

                    #region Set data
                    //
                    //Id
                    //
                    data.ContactId = dataReader["Id"].ToString();

                    if (dataReader["CampIsWritable"] != null && dataReader["CampIsWritable"] != DBNull.Value)
                        data.CampIsWritable = (bool) dataReader["CampIsWritable"];
                    if (dataReader["CampIsPower"] != null && dataReader["CampIsPower"] != DBNull.Value)
                        data.CampIsPower = (bool)dataReader["CampIsPower"];
                    if (dataReader["CampIsFull"] != null && dataReader["CampIsFull"] != DBNull.Value)
                        data.CampIsFull = (bool)dataReader["CampIsFull"];
                    if (dataReader["ActIsWritable"] != null && dataReader["ActIsWritable"] != DBNull.Value)
                        data.ActIsWritable = (bool)dataReader["ActIsWritable"];
                    if (dataReader["ActIsPower"] != null && dataReader["ActIsPower"] != DBNull.Value)
                        data.ActIsPower = (bool)dataReader["ActIsPower"];
                    if (dataReader["ActIsFull"] != null && dataReader["ActIsFull"] != DBNull.Value)
                        data.ActIsFull = (bool)dataReader["ActIsFull"];
                    if (dataReader["AgtIsWritable"] != null && dataReader["AgtIsWritable"] != DBNull.Value)
                        data.AgtIsWritable = (bool)dataReader["AgtIsWritable"];
                    if (dataReader["AgtIsPower"] != null && dataReader["AgtIsPower"] != DBNull.Value)
                        data.AgtIsPower = (bool)dataReader["AgtIsPower"];
                    if (dataReader["AgtIsFull"] != null && dataReader["AgtIsFull"] != DBNull.Value)
                        data.AgtIsFull = (bool)dataReader["AgtIsFull"];
                    
                    
                    //
                    //Localdatetime
                    //
                    dConvert = DateTime.MinValue;
                    DateTime.TryParse(dataReader["LocalDateTime"].ToString(), out dConvert);
                    data.LocalDateTime = dConvert; 
                    //
                    //Originator
                    //
                    sConvert = "";
                    try
                    {
                        string[] list = dataReader["Originator"].ToString().Split(new char[] { '@' });
                        if (list.Length > 0)
                        {
                            string[] list2 = list[0].Split(new char[] { ':' });
                            if (list2.Length > 1)
                                sConvert = list2[1];
                            else
                                sConvert = dataReader["Originator"].ToString();
                        }
                        else
                            sConvert = dataReader["Originator"].ToString();
                    }
                    catch { sConvert = dataReader["Originator"].ToString(); } 
                    data.Originator = sConvert;
                    //
                    //Destination
                    //
                    sConvert = "";
                    try
                    {
                        string[] list = dataReader["Destination"].ToString().Split(new char[] { '@' });
                        if (list.Length > 0)
                        {
                            string[] list2 = list[0].Split(new char[] { ':' });
                            if (list2.Length > 1)
                                sConvert = list2[1];
                            else
                                sConvert = dataReader["Destination"].ToString();
                        }
                        else
                            sConvert = dataReader["Destination"].ToString();
                    }
                    catch { sConvert = dataReader["Destination"].ToString(); }
                    data.Destination = sConvert;
                    //
                    //RecordingId
                    //
                    data.RecordingId = dataReader["RecordingId"].ToString();
                    //
                    //ContactStateId
                    //
                    iConvert = 0;
                    int.TryParse(dataReader["ContactStateId"].ToString(), out iConvert);
                    data.ContactStateId = iConvert;
                    //
                    //ActivityId
                    //
                    data.ActivityId = dataReader["ActivityId"].ToString();
                    //
                    //Act Description
                    //
                    data.Description = dataReader["Description"].ToString();
                    //
                    //CampaignId
                    //
                    data.CampaignId = dataReader["CampaignId"].ToString();
                    //
                    //Camp Description
                    //
                    data.CampDescription = dataReader["CampDescription"].ToString();
                    //
                    //ContactTypeId
                    //
                    iConvert = 0;
                    int.TryParse(dataReader["ContactTypeId"].ToString(), out iConvert);
                    data.ContactTypeId = iConvert;
                    //
                    //Duration
                    //
                    iConvert = 0;
                    int.TryParse(dataReader["Duration"].ToString(), out iConvert);
                    data.Duration = iConvert;
                    //
                    //SetupDuration
                    //
                    iConvert = 0;
                    int.TryParse(dataReader["SetupDuration"].ToString(), out iConvert);
                    iConvert = iConvert / 1000;
                    data.SetupDuration = iConvert;
                    //
                    //ComDuration
                    //
                    iConvert = 0;
                    int.TryParse(dataReader["ComDuration"].ToString(), out iConvert);
                    iConvert = iConvert / 1000;
                    data.ComDuration = iConvert;
                    //
                    //CustomerId
                    //
                    data.CustomerId = dataReader["CustomerId"].ToString();
                    //
                    //Memo (This was used by the V0 comment and score system.)
                    //
                    data.Memo = dataReader["Memo"].ToString().Trim();
                    //
                    //Positive
                    //
                    iConvert = 0;
                    int.TryParse(dataReader["Positive"].ToString(), out iConvert);
                    data.Positive = iConvert;
                    //
                    //Argued
                    //
                    bConvert = false;
                    bool.TryParse(dataReader["Argued"].ToString(), out bConvert);
                    data.Argued = bConvert;
                    //
                    //EndReason
                    //
                    data.EndReason = dataReader["EndReason"].ToString();
                    //
                    //ContactListId
                    //
                    data.ContactListId = dataReader["ContactListId"].ToString();
                    //
                    //Extension
                    //
                    data.Extension = dataReader["Extension"].ToString();
                    //
                    //QualificationId   
                    //
                    data.QualificationId = dataReader["ContactQualificationId"].ToString();
                    //
                    //QualificationOriginatorId
                    //
                    data.QualificationOriginatorId = dataReader["QualOriginatorId"].ToString();
                    //
                    //QualificationOriginatorTypeId  
                    // 
                    iConvert = 0;
                    int.TryParse(dataReader["QualOriginatorTypeId"].ToString(), out iConvert);
                    data.QualificationOriginatorTypeId = iConvert;


                    if (!string.IsNullOrEmpty(data.QualificationId))
                    {

                        QualificationLight seekQual = m_AdminLight.FlatQualifications.SingleOrDefault((q) => (q.Id == data.QualificationId));

                        if (seekQual != null)
                        {
                            data.QualificationDescription = seekQual.Description;

                            string fullName = string.Empty;

                            while (seekQual != null)
                            {
                                fullName = "/" + seekQual.Description + fullName;
                                seekQual = m_AdminLight.FlatQualifications.SingleOrDefault((q) => (seekQual.Related.Contains(q.Id)));
                            }
                            data.QualificationFullDescription = fullName;
                        }
                    }

                    //
                    //AgtQualName 
                    //
                    data.AgtQualName = dataReader["AgtQualName"].ToString();
                    //
                    //AgtQualAccount 
                    //
                    data.AgtQualAccount = dataReader["AgtQualAccount"].ToString();
                    //
                    //OrigContactQualificationId
                    //
                    data.OrigQualificationId = dataReader["OrigContactQualificationId"].ToString();
                    //
                    //OrigQualificationOriginatorId
                    //
                    data.OrigQualificationOriginatorId = dataReader["OrigQualOriginatorId"].ToString();
                    //
                    //OrigQualificationOriginatorTypeId
                    //
                    iConvert = 0;
                    int.TryParse(dataReader["OrigQualOriginatorTypeId"].ToString(), out iConvert);
                    data.OrigQualificationOriginatorTypeId = iConvert;                   
                    //
                    //AgtOrigQualName
                    //
                    data.AgtOrigQualName = dataReader["AgtOrigQualName"].ToString();
                    //
                    //AgtOrigQualAccount
                    //
                    data.AgtOrigQualName = dataReader["AgtOrigQualName"].ToString();
        //
        //Support version 0 of score and comment is stopped 
        //
                    //
                    //RecScoreOriginator
                    //
                    data.ScoreOriginator = dataReader["RecScoreOriginator"].ToString();
                    //
                    //AgtScoreName
                    //
                    data.ScoreAgentName = dataReader["AgtScoreName"].ToString();
                    //
                    //AgtScoreAccount
                    //
                    data.ScoreAgentAccount = dataReader["AgtScoreAccount"].ToString();
                    //
                    //RecScoreDateTimeUtc
                    //
                    dConvert = DateTime.MinValue;
                    DateTime.TryParse(dataReader["RecScoreDateTimeUtc"].ToString(), out dConvert);
                    data.ScoreDateTimeUtc = dConvert; 
                    //
                    //RecScoreTimeZone
                    //
                    iConvert = 0;
                    int.TryParse(dataReader["RecScoreTimeZone"].ToString(), out iConvert);
                    data.ScoreTimeZone = iConvert;
                    //
                    //RecScore
                    //
                    data.Score = dataReader["RecScore"].ToString();
                    data.Score.Trim();
                    //
                    //RecCommentOriginator
                    //
                    data.CommentOriginator = dataReader["RecCommentOriginator"].ToString();
                    //
                    //AgtCommentName
                    //
                    data.CommentAgentName = dataReader["AgtCommentName"].ToString();
                    //
                    //AgtCommentAccount
                    //
                    data.CommentAgentAccount = dataReader["AgtCommentAccount"].ToString();
                    //
                    //RecCommentDateTimeUtc
                    //
                    dConvert = DateTime.MinValue;
                    DateTime.TryParse(dataReader["RecCommentDateTimeUtc"].ToString(), out dConvert);
                    data.CommentDateTimeUtc = dConvert; 
                    //
                    //RecCommentTimeZone
                    //
                    iConvert = 0;
                    int.TryParse(dataReader["RecCommentTimeZone"].ToString(), out iConvert);
                    data.CommentTimeZone = iConvert;
                    //
                    //RecComment
                    //
                    data.Comment = dataReader["RecComment"].ToString();
                    //
                    //UserMemo
                    //
                    data.UserMemo = dataReader["UserMemo"].ToString();
                    //
                    //KeepRecording
                    //
                    if (System.DBNull.Value == dataReader["KeepRecording"])
                        data.KeepRecording = ContactData.KeepRecordingStates.Default;
                    else
                    {
                        bool keep = true;
                        if (bool.TryParse(dataReader["KeepRecording"].ToString(), out keep))
                        {
                            if (keep)
                                data.KeepRecording = ContactData.KeepRecordingStates.Keep;
                            else
                                data.KeepRecording = ContactData.KeepRecordingStates.NotKeep;
                        }
                    }
                    //
                    //FirstName
                    //
                    data.FirstName = dataReader["FirstName"].ToString();
                    //
                    //LastName
                    //
                    data.LastName = dataReader["LastName"].ToString();
                    //
                    //Account
                    //
                    data.Account = dataReader["Account"].ToString();
                    //
                    //RecordingMarker
                    //
                    data.RecordingMarker = 0;
                    //
                    //NbOfFiles
                    //---------
                    //This is used when preload of file names is active. To be check (TO DO)
                    //This has to be reviewed.
                    //
                    data.NbOfFiles = 0;
                    FileList _fl = new FileList();
                    if (ConfigData.PreLoadFileNames)
                    {
                        _fl = m_FileList.FindRecordings(dataReader["RecordingId"].ToString());
                        if (_fl.Count == 0)
                            data.RecodingState = ContactData.RecordingStates.Found;
                        else
                            data.RecodingState = ContactData.RecordingStates.NotFound;

                        data.NbOfFiles = _fl.Count;
                    }
                    //
                    //MediaType
                    //
                    data.MediaSubType = GetMediaTypeDescription(data.ContactTypeId, data.ActivityId);
                    #endregion

                    data.UpdateOriginalValue();

                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        m_ccControl.Contacts.Add(data);
                    }));
   
                }
                progressState.Description = string.Format(TranslationContext.Translate("Found a total of {0} record(s)."), count);
                progressState.CurrentProgress = count;
                worker.ReportProgress(-2, progressState);
            }
            catch (Exception error)
            {
                m_Error = true;
                Tools.Log("Error reading db: " + error.ToString(), Tools.LogType.Error);
            }
        }

        #region Memnbers - Events
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            m_BkgWorker.CancelAsync();
            ((Button)sender).Content = TranslationContext.Translate("Cancelling...");
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRecord();
        }

        private void ShowExecuteTime(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                if (m_TimerStart == DateTime.MinValue)
                    m_TimerStart = DateTime.Now;

                TimeSpan time = DateTime.Now.Subtract(m_TimerStart);
                lblFound.Content = string.Format(TranslationContext.Translate("{0:D2}:{1:D2}:{2:D2}"), time.Hours, time.Minutes, time.Seconds);
            }));
        }

        private void loadContactsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Arrow;

            if (m_Error)
                this.DialogResult = false;
            else
                this.DialogResult = true;
        }

        private void m_LoadContactsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorkerProgress progressState = null;
            if (e.UserState.GetType() == typeof(BackgroundWorkerProgress))
                progressState = (BackgroundWorkerProgress)e.UserState;


            if (e.ProgressPercentage == -1) //running progressbar
            {
                pbMain.IsIndeterminate = true;
            }
            else if (e.ProgressPercentage < -1) //Custom progressbar
            {
                if (progressState != null)
                {
                    if (progressState.Maximum == -1)
                    {
                        pbMain.IsIndeterminate = true;
                    }
                    else
                    {
                        pbMain.IsIndeterminate = false;
                        pbMain.Minimum = progressState.Minimum;
                        pbMain.Maximum = progressState.Maximum;

                        pbMain.Value = progressState.CurrentProgress;
                    }
                    lblFound.Content = progressState.GetProgressLabel();
                    lblStatus.Content = progressState.Description;
                }

            }
            else if (e.ProgressPercentage == 0) //Resetting progressbar
            {
                pbMain.IsIndeterminate = false;
                pbMain.Minimum = 0;
                pbMain.Maximum = 0;
                pbMain.Value = 0;
            }
            else if (e.ProgressPercentage > 0) //Progress
            {
                pbMain.Value = e.ProgressPercentage;
            }
        }
        #endregion
        #endregion

        #region Helpers
        private MediaSubTypes GetMediaTypeDescription(int type, string activity)
        {
            switch (type)
            {
                case 1: //Inbound
                    if (activity.Trim() == "" || activity == null)
                        return MediaSubTypes.DirectCall;
                    else
                        return MediaSubTypes.Inbound;
                case 2: //Outbound
                    if (activity.Trim() == "" || activity == null)
                        return MediaSubTypes.ManualCall;
                    else
                        return MediaSubTypes.Outbound;
                case 6: //Chat
                    return MediaSubTypes.Chat;
                default:
                    return MediaSubTypes.Undefined;
            }
        }
        #endregion
        
    }
}
