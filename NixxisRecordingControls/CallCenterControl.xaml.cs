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
using Nixxis.Client.Controls;
using System.Windows.Threading;
using Nixxis.Client.Admin;
using System.Net;
using System.Data;
using System.IO;
using System.ComponentModel;
using Microsoft.Win32;

namespace Nixxis.Client.Recording
{
    /// <summary>
    /// Interaction logic for CallCenterControl.xaml
    /// </summary>
    public partial class CallCenterControl : UserControl
    {
        #region Enums
        public enum PlayerStates
        {
            Stopped,
            Playing,
            Pause,
        }
        #endregion

        #region Class data
        private ContactData m_CurrentItemData = null;
        private RecordingFileInformation m_CurrentItemFileInfo = null;
        private DispatcherTimer m_MediaPlayerTimer = new DispatcherTimer();
        private CustomerDataList m_CustomerDataList = new CustomerDataList();
        #endregion


        #region Properties XAML
        public static readonly DependencyProperty ActivityListProperty = DependencyProperty.Register("ActivityList", typeof(DisplayList), typeof(CallCenterControl));
        public DisplayList ActivityList
        {
            get { return (DisplayList)GetValue(ActivityListProperty); }
            set { SetValue(ActivityListProperty, value); }
        }

        public static readonly DependencyProperty CampaignListProperty = DependencyProperty.Register("CampaignList", typeof(DisplayList), typeof(CallCenterControl));
        public DisplayList CampaignList
        {
            get { return (DisplayList)GetValue(CampaignListProperty); }
            set { SetValue(CampaignListProperty, value); }
        }

        public static readonly DependencyProperty TeamListProperty = DependencyProperty.Register("TeamList", typeof(DisplayList), typeof(CallCenterControl));
        public DisplayList TeamList
        {
            get { return (DisplayList)GetValue(TeamListProperty); }
            set { SetValue(TeamListProperty, value); }
        }

        public static readonly DependencyProperty AgentListProperty = DependencyProperty.Register("AgentList", typeof(DisplayList), typeof(CallCenterControl));
        public DisplayList AgentList
        {
            get { return (DisplayList)GetValue(AgentListProperty); }
            set { SetValue(AgentListProperty, value); }
        }

        public static readonly DependencyProperty ContactsProperty = DependencyProperty.Register("Contacts", typeof(ContactList), typeof(CallCenterControl));
        public ContactList Contacts
        {
            get { return (ContactList)GetValue(ContactsProperty); }
            set { SetValue(ContactsProperty, value); }
        }

        public static readonly DependencyProperty IsAllowedSearchDateTimeProperty = DependencyProperty.Register("IsAllowedSearchDateTime", typeof(bool), typeof(CallCenterControl), new PropertyMetadata(true));
        public bool IsAllowedSearchDateTime
        {
            get { return (bool)GetValue(IsAllowedSearchDateTimeProperty); }
            set { SetValue(IsAllowedSearchDateTimeProperty, value); }
        }

        public static readonly DependencyProperty IsAllowedSearchContactProperty = DependencyProperty.Register("IsAllowedSearchContact", typeof(bool), typeof(CallCenterControl), new PropertyMetadata(true));
        public bool IsAllowedSearchContact
        {
            get { return (bool)GetValue(IsAllowedSearchContactProperty); }
            set { SetValue(IsAllowedSearchContactProperty, value); }
        }

        public static readonly DependencyProperty IsAllowedSearchCampaignProperty = DependencyProperty.Register("IsAllowedSearchCampaign", typeof(bool), typeof(CallCenterControl), new PropertyMetadata(true));
        public bool IsAllowedSearchCampaign
        {
            get { return (bool)GetValue(IsAllowedSearchCampaignProperty); }
            set { SetValue(IsAllowedSearchCampaignProperty, value); }
        }

        public static readonly DependencyProperty IsAllowedMediaTypeInboundProperty = DependencyProperty.Register("IsAllowedMediaTypeInbound", typeof(bool), typeof(CallCenterControl), new PropertyMetadata(true));
        public bool IsAllowedMediaTypeInbound
        {
            get { return (bool)GetValue(IsAllowedMediaTypeInboundProperty); }
            set { SetValue(IsAllowedMediaTypeInboundProperty, value); }
        }

        public static readonly DependencyProperty IsAllowedMediaTypeOutboundProperty = DependencyProperty.Register("IsAllowedMediaTypeOutbound", typeof(bool), typeof(CallCenterControl), new PropertyMetadata(true));
        public bool IsAllowedMediaTypeOutbound
        {
            get { return (bool)GetValue(IsAllowedMediaTypeOutboundProperty); }
            set { SetValue(IsAllowedMediaTypeOutboundProperty, value); }
        }

        public static readonly DependencyProperty IsAllowedMediaTypeManuelProperty = DependencyProperty.Register("IsAllowedMediaTypeManuel", typeof(bool), typeof(CallCenterControl), new PropertyMetadata(true));
        public bool IsAllowedMediaTypeManuel
        {
            get { return (bool)GetValue(IsAllowedMediaTypeManuelProperty); }
            set { SetValue(IsAllowedMediaTypeManuelProperty, value); }
        }

        public static readonly DependencyProperty IsAllowedMediaTypeDirectProperty = DependencyProperty.Register("IsAllowedMediaTypeDirect", typeof(bool), typeof(CallCenterControl), new PropertyMetadata(true));
        public bool IsAllowedMediaTypeDirect
        {
            get { return (bool)GetValue(IsAllowedMediaTypeDirectProperty); }
            set { SetValue(IsAllowedMediaTypeDirectProperty, value); }
        }

        public static readonly DependencyProperty IsAllowedMediaTypeChatProperty = DependencyProperty.Register("IsAllowedMediaTypeChat", typeof(bool), typeof(CallCenterControl), new PropertyMetadata(true));
        public bool IsAllowedMediaTypeChat
        {
            get { return (bool)GetValue(IsAllowedMediaTypeChatProperty); }
            set { SetValue(IsAllowedMediaTypeChatProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(CallCenterControl));
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty PlayerItemProperty = DependencyProperty.Register("PlayerItem", typeof(object), typeof(CallCenterControl));
        public object PlayerItem
        {
            get { return GetValue(PlayerItemProperty); }
            set { SetValue(PlayerItemProperty, value); }
        }

        public static readonly DependencyProperty PlayerStateProperty = DependencyProperty.Register("PlayerState", typeof(PlayerStates), typeof(CallCenterControl), new PropertyMetadata(PlayerStates.Stopped));
        public PlayerStates PlayerState
        {
            get { return (PlayerStates)GetValue(PlayerStateProperty); }
            set { SetValue(PlayerStateProperty, value); }
        }        

        public static readonly DependencyProperty SearchParametersProperty = DependencyProperty.Register("SearchParameters", typeof(SearchParameters), typeof(CallCenterControl));
        public SearchParameters SearchParameters
        {
            get { return (SearchParameters)GetValue(SearchParametersProperty); }
            set { SetValue(SearchParametersProperty, value); }
        }

        public static readonly DependencyProperty UserInfoProperty = DependencyProperty.Register("UserInfo", typeof(string), typeof(CallCenterControl));
        public string UserInfo
        {
            get { return (string)GetValue(UserInfoProperty); }
            set { SetValue(UserInfoProperty, value); }
        }
        //
        //Config values
        //
        public static readonly DependencyProperty ConfigProperty = DependencyProperty.Register("Config", typeof(Profile), typeof(CallCenterControl));
        public Profile Config
        {
            get { return (Profile)GetValue(ConfigProperty); }
            set { SetValue(ConfigProperty, value); }
        }
        #endregion

        #region static
        static CallCenterControl() 
        {
            DependencyProperty dp = FrameworkElement.DataContextProperty.AddOwner(typeof(DataGridColumn)); 
            FrameworkElement.DataContextProperty.OverrideMetadata ( typeof(DataGrid), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDataContextChanged)));
        }
        public static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGrid grid = d as DataGrid;
            if (grid != null)
            {
                foreach (DataGridColumn col in grid.Columns)
                {
                    col.SetValue(FrameworkElement.DataContextProperty, e.NewValue);
                }
            }
        }
        #endregion

        #region Constructors
        public CallCenterControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Members override
        protected override void OnInitialized(EventArgs e)
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(CallCenterControl_IsVisibleChanged);
            
            base.OnInitialized(e);

            this.SearchParameters = new SearchParameters();

            m_MediaPlayerTimer = new DispatcherTimer();
            m_MediaPlayerTimer.Interval = TimeSpan.FromSeconds(1);
            m_MediaPlayerTimer.Tick += new EventHandler(timer_Tick);
        }

        public override void EndInit()
        {
            base.EndInit();
            this.SetInitSearchControls();
        }

        #endregion

        #region Members
        #region GUI
        public void SetToolbarPanel()
        {
            NixxisBaseExpandPanel nep = FindResource("RecCallCenterToolbarPanel") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.SetPanelCommand.CanExecute(nep))
                    NixxisGrid.SetPanelCommand.Execute(nep);
            }
        }
        
        public void RemoveToolbarPanel()
        {
            NixxisBaseExpandPanel nep = FindResource("RecCallCenterToolbarPanel") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                    NixxisGrid.RemovePanelCommand.Execute(nep);
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NixxisDataGrid ndg = MainGrid;

            if (e.Parameter != null)
            {
                ndg = e.Parameter as NixxisDataGrid;
            }

        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            NixxisDataGrid ndg = MainGrid;

            if (e.Parameter != null)
            {
                ndg = e.Parameter as NixxisDataGrid;
            }


            e.Handled = true;
        }
        #endregion

        #region Search
        private void SetInitSearchControls()
        {
            AdminLight adminLight = (AdminLight)this.DataContext;
            if (adminLight == null)
                return;
            //
            //Agent
            this.AgentList = CreateDisplayLists.CreateAgentList(adminLight, true);
            //
            //Team
            this.TeamList= CreateDisplayLists.CreateTeamList(adminLight, true);
            //
            //Campaign
            this.CampaignList = CreateDisplayLists.CreateCampaignList(adminLight, true);
        }

        private void SearchContacts()
        {
            CallCenterLoadingContacts frm = new CallCenterLoadingContacts();
            frm.ccControl = this;
            frm.ConfigData = Config.Data;
            frm.AdminCore = (AdminLight)this.DataContext;
            frm.SearchParameters = this.SearchParameters;
            frm.Height = 170;
            frm.Width = 300;
            if (Application.Current.MainWindow.IsLoaded)
                frm.Owner = Application.Current.MainWindow;
            frm.ShowDialog();


            frm = null;
        }

        private void SetAgentList(string teamId)
        {
            AdminLight adminLight = (AdminLight)this.DataContext;
            if (adminLight == null)
                return;

            //
            //Agent
            if (teamId == null)
                return;

            if (teamId == CreateDisplayLists.NoSelection)
                this.AgentList = CreateDisplayLists.CreateAgentList(adminLight, true);
            else
                this.AgentList = CreateDisplayLists.CreateAgentList(adminLight, true, teamId);

            this.SearchParameters.AgentItem = new DisplayItem(CreateDisplayLists.NoSelection, CreateDisplayLists.NoSelection);
        }

        private void SetActivityList(string campaignId)
        {
            AdminLight adminLight = (AdminLight)this.DataContext;
            if (adminLight == null)
                return;

            if (campaignId == CreateDisplayLists.NoSelection)
            {
                this.ActivityList = new DisplayList();
                this.ActivityList.Add(new DisplayItem(CreateDisplayLists.NoSelection, CreateDisplayLists.NoSelection));
                this.SearchParameters.ActivityItem = new DisplayItem(CreateDisplayLists.NoSelection, CreateDisplayLists.NoSelection);
            }
            else
            {

                this.ActivityList = CreateDisplayLists.CreateActivitiesList(adminLight, true, campaignId);
                this.SearchParameters.ActivityItem = new DisplayItem(CreateDisplayLists.NoSelection, CreateDisplayLists.NoSelection);
            }
        }
        #endregion

        #region Playback
        private void RequestRecordingToPlay()
        {
            if (this.SelectedItem == null)
            {
                MessageBox.Show("Please select a contact!");
                return;
            }

            ContactData selectedItem = (ContactData)this.SelectedItem;

            switch (selectedItem.MediaType)
            {
                case MediaTypes.Voice:
                    RequestVoiceRecordingToPlay(selectedItem);
                    break;
                case MediaTypes.Chat:
                    RequestChatRecordingToPlay(selectedItem);
                    break;
                case MediaTypes.Mail:
                    //TO DO
                    break;
            }
        }

        private void RequestVoiceRecordingToPlay(ContactData contactData)
        {
            RecordingFileInformation recFileInfo = null;
            try
            {
                Tools.Log(string.Format("Check recording {0} to download", TransferFile.GetInternalFileName(contactData)), Tools.LogType.StatusInfo);
                //
                //Get File info
                //
                CallCenterWavFileSelector frm = new CallCenterWavFileSelector();
                frm.ConfigData= Config.Data;
                frm.ContactData = contactData;
                if (Application.Current.MainWindow.IsLoaded)
                    frm.Owner = Application.Current.MainWindow;
                frm.GetFileName(TransferFile.GetInternalFileName(contactData));
                recFileInfo = frm.SelectedFileInformation;
                frm = null;

                //
                //Download and play file
                //

                if (m_CurrentItemData != null)
                    m_CurrentItemData.RecodingState = ContactData.RecordingStates.Found;

                if (recFileInfo == null)
                {
                    contactData.RecodingState = ContactData.RecordingStates.NotFound;
                    Tools.Log(string.Format("Recording {0} isn't found on the server(s).", contactData.RecordingId), Tools.LogType.StatusInfo);
                }
                else
                {
                    Tools.Log(string.Format("Loading file uri: {0}", recFileInfo.DownloadUrl), Tools.LogType.Info);
                    contactData.RecodingState = ContactData.RecordingStates.Loading;
                    try
                    {
                        if (m_CurrentItemFileInfo == null || m_CurrentItemFileInfo.DownloadUrl != recFileInfo.DownloadUrl)
                        {
                            if (recFileInfo.DownloadServer.Type == TransferProfileData.Types.Http && this.Config.Data.UseStreamForHttp)
                            {
                                m_CurrentItemFileInfo = recFileInfo;
                                m_CurrentItemData = contactData;
                                PlayerItem = contactData;
                                //TO DO PLAYER: axWindowsMediaPlayer1.URL = recFileInfo.StreamingUrl;
                            }
                            else
                            {
                                string tmpFileName = null;

                                CallCenterDownloadingFile frmDownLoad = new CallCenterDownloadingFile();
                                frmDownLoad.ConfigData = Config.Data;
                                frmDownLoad.ContactData = contactData;
                                frmDownLoad.RecordingFileInfo = recFileInfo;
                                frmDownLoad.Height = 170;
                                frmDownLoad.Width = 300;
                                if (Application.Current.MainWindow.IsLoaded)
                                    frmDownLoad.Owner = Application.Current.MainWindow;
                                frmDownLoad.ShowDialog();
                                
                                tmpFileName = frmDownLoad.OutputfileName;

                                if (string.IsNullOrEmpty(tmpFileName) && contactData != null)
                                {
                                    contactData.RecodingState = ContactData.RecordingStates.NotFound;
                                    Tools.Log(string.Format(string.Format("Recording {0} isn't found on the server(s).", recFileInfo.DownloadUrl)), Tools.LogType.StatusInfo);
                                    return;
                                }
                                m_CurrentItemFileInfo = recFileInfo;
                                m_CurrentItemData = contactData;
                                PlayerItem = contactData;
                                contactData.RecodingState = ContactData.RecordingStates.Playing;
                                Tools.Log(string.Format(string.Format("Recording {0} is ready", recFileInfo.DownloadUrl)), Tools.LogType.StatusInfo);

                                mediaPlayer.Source = new Uri(tmpFileName);
                                recFileInfo.TempFilename = tmpFileName;
                            }

                            pnlGeneralInfoVoice.DataContext = recFileInfo;
                            MediaPlayer_Play();
                        }
                        else
                        {
                            if (contactData != null)
                            {
                                if (recFileInfo == null)
                                {
                                    contactData.RecodingState = ContactData.RecordingStates.NotFound;
                                    Tools.Log(string.Format(string.Format("Recording {0} isn't found on the server(s).", recFileInfo.DownloadUrl)), Tools.LogType.StatusInfo);
                                }
                                else
                                {
                                    contactData.RecodingState = ContactData.RecordingStates.Playing;
                                }
                            }
                            else
                            {
                                if (contactData != null)
                                {
                                    contactData.RecodingState = ContactData.RecordingStates.NotFound;
                                    Tools.Log(string.Format(string.Format("Recording {0} isn't found on the server(s).", recFileInfo.DownloadUrl)), Tools.LogType.StatusInfo);
                                }
                            }
                            MediaPlayer_Play();
                        }
                    }
                    catch (Exception error)
                    {
                        contactData.RecodingState = ContactData.RecordingStates.NotFound;
                        Tools.Log(string.Format("Recording {0} isn't found on the server(s). Due to an error: {1}", contactData.RecordingId, error.ToString()), Tools.LogType.Info);
                    }
                }
            }
            catch (Exception error)
            {
                Tools.Log(string.Format("RequestVoiceRecordingToPlay --> {0}", error.ToString()), Tools.LogType.Error);
            }
        }
        
        private void RequestChatRecordingToPlay(ContactData contactData)
        {
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Tools.Log(string.Format("timer_Tick mediaPlayer.Position: {0}, mediaPlayer.NaturalDuration: {1}", mediaPlayer.Position.ToString(), mediaPlayer.NaturalDuration.ToString()), Tools.LogType.Debug);
            
            slTimeProgress.Value = mediaPlayer.Position.TotalMilliseconds;
            lblCurrentTime.Text = Format.ToDisplayTimeSpan(mediaPlayer.Position);
        }

        private void MediaPlayer_Play()
        {
            this.PlayerState = PlayerStates.Playing;
            m_MediaPlayerTimer.Start();
            mediaPlayer.Play();
        }
        #endregion

        #region ContactData


        private void RequestUpdateContactData()
        {
            if (this.SelectedItem == null)
            {
                MessageBox.Show("Please select a contact!");
                return;
            }

            ContactData selectedItem = (ContactData)this.SelectedItem;
            
            if (Config.Data.ReportConnectionType == ProfileData.ServiceTypes.AppServer)
                UpdateContactDataApplicationServer(selectedItem);
            else
                UpdateContactDataDirectConnection(selectedItem);
        }
        private void UpdateContactDataApplicationServer(ContactData contactData)
        {
            string parameterFormat = "&{0}={1},{2}";
            string parameterFormatNull = "&{0}={1}";
            StringBuilder str = new StringBuilder();
            try
            {
                //
                //Parameters
                //
                //In Nixxis V2 comment and score version 0 can't be used anymore
                //
                str.Append(string.Format(parameterFormat, "@ContactType", DataTypeCode.IntDataType, contactData.ContactTypeId));
                str.Append(string.Format(parameterFormat, "@UseCommentV0", DataTypeCode.BoolDataType, Config.Data.UseCommentVersion0));

                //--> Score
                if (contactData.Score != contactData.ScoreOriginal || contactData.ForcedSaveCommentAndScore)
                {
                    str.Append(string.Format(parameterFormat, "@UpdateScore", DataTypeCode.BoolDataType, true));
                    str.Append(string.Format(parameterFormat, "@ScoreOriginator", DataTypeCode.StringDataType, UserInfo));
                    str.Append(string.Format(parameterFormat, "@ScoreDateTimeUtc", DataTypeCode.DateTimeDataType, DateTime.UtcNow.ToString("yyyyMMddHHmmss")));
                    str.Append(string.Format(parameterFormat, "@ScoreTimeZone", DataTypeCode.IntDataType, TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes));
                    str.Append(string.Format(parameterFormat, "@Score", DataTypeCode.StringDataType, contactData.Score));
                }
                else
                {
                    str.Append(string.Format(parameterFormat, "@UpdateScore", DataTypeCode.BoolDataType, false));
                    str.Append(string.Format(parameterFormatNull, "@ScoreOriginator", DataTypeCode.NullValue));
                    str.Append(string.Format(parameterFormatNull, "@ScoreDateTimeUtc", DataTypeCode.NullValue));
                    str.Append(string.Format(parameterFormatNull, "@ScoreTimeZone", DataTypeCode.NullValue));
                    str.Append(string.Format(parameterFormatNull, "@Score", DataTypeCode.NullValue));
                }
                //--> Comment
                if (contactData.Comment != contactData.CommentOriginal || contactData.ForcedSaveCommentAndScore)
                {
                    str.Append(string.Format(parameterFormat, "@UpdateComment", DataTypeCode.BoolDataType, true));
                    str.Append(string.Format(parameterFormat, "@CommentOriginator", DataTypeCode.StringDataType, UserInfo));
                    str.Append(string.Format(parameterFormat, "@CommentDateTimeUtc", DataTypeCode.DateTimeDataType, DateTime.UtcNow.ToString("yyyyMMddHHmmss")));
                    str.Append(string.Format(parameterFormat, "@CommentTimeZone", DataTypeCode.IntDataType, TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes));
                    str.Append(string.Format(parameterFormat, "@Comment", DataTypeCode.StringDataType, contactData.Comment));
                }
                else
                {
                    str.Append(string.Format(parameterFormat, "@UpdateComment", DataTypeCode.BoolDataType, false));
                    str.Append(string.Format(parameterFormatNull, "@CommentOriginator", DataTypeCode.NullValue));
                    str.Append(string.Format(parameterFormatNull, "@CommentDateTimeUtc", DataTypeCode.NullValue));
                    str.Append(string.Format(parameterFormatNull, "@CommentTimeZone", DataTypeCode.NullValue));
                    str.Append(string.Format(parameterFormatNull, "@Comment", DataTypeCode.NullValue));
                }

                //--> Qualification
                bool updateQual = QualificationTree.SelectedItem != null;

                if (updateQual)
                {
                    str.Append(string.Format(parameterFormat, "@UpdateQual", DataTypeCode.BoolDataType, true));
                    str.Append(string.Format(parameterFormat, "@ContactQualificationId", DataTypeCode.StringDataType, ((QualificationLight)QualificationTree.SelectedItem).Id));
                    str.Append(string.Format(parameterFormat, "@Positive", DataTypeCode.IntDataType, ((QualificationLight)QualificationTree.SelectedItem).Positive));
                    str.Append(string.Format(parameterFormat, "@Argued", DataTypeCode.BoolDataType, ((QualificationLight)QualificationTree.SelectedItem).Argued));
                    str.Append(string.Format(parameterFormat, "@QualOriginatorTypeId", DataTypeCode.IntDataType, 6));
                    str.Append(string.Format(parameterFormat, "@QualOriginatorId", DataTypeCode.StringDataType, UserInfo));
                }
                else
                {
                    str.Append(string.Format(parameterFormat, "@UpdateQual", DataTypeCode.BoolDataType, false));
                    str.Append(string.Format(parameterFormatNull, "@ContactQualificationId", DataTypeCode.NullValue));
                    str.Append(string.Format(parameterFormatNull, "@Positive", DataTypeCode.NullValue));
                    str.Append(string.Format(parameterFormatNull, "@Argued", DataTypeCode.NullValue));
                    str.Append(string.Format(parameterFormatNull, "@QualOriginatorTypeId", DataTypeCode.NullValue));
                    str.Append(string.Format(parameterFormatNull, "@QualOriginatorId", DataTypeCode.NullValue));
                }

                str.Append(string.Format(parameterFormat, "@ContactId", DataTypeCode.StringDataType, contactData.ContactId));
                str.Append(string.Format(parameterFormat, "@UpdateKeepRecording", DataTypeCode.BoolDataType, false));
                str.Append(string.Format(parameterFormatNull, "@KeepRecording", DataTypeCode.NullValue));

                //
                //Saving
                //
                try
                {
                    string requestStr = Config.Data.ServiceDataRoot + AppServerRequest.DataAction.RemoteData + "&source=" + Config.Data.ReportConnectionString + "&exec=UpdateContactData" + str.ToString();

                    Tools.Log("HTTP UpdateContactData. Request saved contact info: " + requestStr, Tools.LogType.Info);

                    WebRequest request = HttpWebRequest.Create(requestStr);
                    using(HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        if (response != null)
                        {
                            string strResponse = null;
                            using (Stream respStream = response.GetResponseStream())
                            {
                                using (StreamReader sr = new StreamReader(respStream))
                                {
                                    strResponse = sr.ReadToEnd();
                                }
                            }
                            response.Close();
                        }
                    }

                    contactData.UpdateOriginalValue();
                    MessageBox.Show("Contact information saved. Please reload to see changes.", "Information", MessageBoxButton.OK);
                }
                catch (Exception error)
                {
                    Tools.Log("HTTP UpdateContactData. Error on database. " + error.ToString(), Tools.LogType.Error);
                    MessageBox.Show("Can't update contact.\n\r\n\rError on http request.\n\r" + error.ToString());
                }
            }
            catch (Exception error)
            {
                Tools.Log("HTTP UpdateContactData. Error processing: " + error.ToString(), Tools.LogType.Error);
                MessageBox.Show("Can't update contact.\n\r\n\rError on http request.\n\r" + error.ToString());
            }
        }
        private void UpdateContactDataDirectConnection(ContactData contactData)
        {

        }
        private void GetCustomerData(ContactData contactData)
        {
            string msg = string.Empty;

            m_CustomerDataList.Clear();

            HttpWebResponse response = null;
            DataSet ds = new DataSet();
            bool err = false;
            StreamReader reader = null;

            if (string.IsNullOrEmpty(contactData.ContactListId))
            {
                //TO DO No data found message
                return;
            }

            try
            {
                string requestStr = "";
                try
                {
                    requestStr = Config.Data.ServiceDataRoot + AppServerRequest.DataAction.GetContextData + "&context=" + contactData.CampaignId + "&__Internal__id__=" + contactData.ContactListId;
                }
                catch
                {
                    Tools.Log("HTTP GetCustomerData. No correct item selected", Tools.LogType.Info);
                    //TO DO No data found message
                    err = true;
                }
                if (err) 
                    return;

                Tools.Log("HTTP GetCustomerData. Request customer data: " + requestStr, Tools.LogType.Info);
                WebRequest request = HttpWebRequest.Create(requestStr);
                response = request.GetResponse() as HttpWebResponse;
                ds.ReadXml(response.GetResponseStream(), XmlReadMode.Auto);
            }
            catch (Exception error)
            {
                Tools.Log("HTTP GetCustomerData. error while loading customer data. Wrn: " + error.ToString(), Tools.LogType.Warning);
                msg = "HTTP message while loading customer data. Msg: " + error.ToString();
                //TO DO No data found message
                err = true;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (response != null)
                    response.Close();
            }

            if (err) 
                return;

            // reading data types
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            try
            {                
                WebRequest request = HttpWebRequest.Create(string.Format("{0}{1}&context={2}", Config.Data.ServiceDataRoot, AppServerRequest.DataAction.ListContextFields, contactData.CampaignId));

                using (WebResponse res = request.GetResponse() as HttpWebResponse)
                {
                    doc.Load(res.GetResponseStream());
                }
            }
            catch
            {
            }
            // end reading data types

            try
            {
                int i = ds.Tables["campaigndata"].Columns.Count;
            }
            catch (Exception error)
            {
                Tools.Log("HTTP GetCustomerData. No customer data found", Tools.LogType.Info);
                msg = "HTTP GetCustomerData. message while showing customer data. Msg: " + error.ToString();
                //TO DO No data found message
                err = true;
            }

            if (err) 
                return;

            try
            {
                for (int i = 0; i < ds.Tables["campaigndata"].Columns.Count; i++)
                {
                    if (!ds.Tables["campaigndata"].Columns[i].Caption.StartsWith("_"))
                    {
                        // use previously read data type
                        Type tpe = ds.Tables["campaigndata"].Columns[i].DataType;
                        try
                        {
                            string dbType = doc.SelectSingleNode(@"contextfields/FieldsConfig/FieldConfig[NewFieldName=""" + ds.Tables["campaigndata"].Columns[i].Caption + @"""]/DBType").InnerText;
                            switch (dbType)
                            {
                                case "1":
                                    tpe = typeof(bool);
                                    break;
                                case "2":
                                    tpe = typeof(int);
                                    break;
                                case "3":
                                    tpe = typeof(DateTime);
                                    break;
                                case "4":
                                    tpe = typeof(double);
                                    break;
                                default:
                                    tpe = typeof(string);
                                    break;
                            }
                        }
                        catch
                        {
                        }
                        // end use previously read data type
                        m_CustomerDataList.Add(
                            new CustomerData(
                                ds.Tables["campaigndata"].Columns[i].Caption,
                                tpe,
                                ds.Tables["campaigndata"].Rows[0].ItemArray[i].ToString()));
                    }
                }
                lstCustomerData.ItemsSource = m_CustomerDataList;
            }
            catch (Exception error)
            {
                Tools.Log("HTTP GetCustomerData. error while showing customer data. Wrn: " + error.ToString(), Tools.LogType.Debug);
                msg = "HTTP message while showing customer data. Msg: " + error.ToString();
                //TO DO No data found message
            }

        }
        #endregion

        #region Saving recordings
        private void RequestSaveContact()
        {
            if (this.SelectedItem == null)
            {
                MessageBox.Show("Please select a contact!");
                return;
            }

            ContactData selectedItem = (ContactData)this.SelectedItem;

            PutContactInDownloadQueue(selectedItem);
        }

        private void RequestSaveAllContacts()
        {
            if (MainGrid.Items == null)
            {
                MessageBox.Show("Please select a contact!");
                return;
            }

            //Get folder
            string folder = Config.Data.FileLastSaveLocation;

            if (string.IsNullOrEmpty(folder))
            {
                folder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic).ToString();
                if (string.IsNullOrEmpty(folder))
                {
                    folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString();
                }
            }

            NixxisFolderBrowseDialog fbd = new NixxisFolderBrowseDialog();
            fbd.SelectedPath = Config.Data.FileLastSaveLocation;

            //Add to download manager                    
            if (Application.Current.MainWindow.IsLoaded)
            {
                if (fbd.ShowDialog() == true)
                {
                    foreach (ContactData item in MainGrid.Items)
                    {
                        DownloadManagerItem dItem = new DownloadManagerItem();
                        dItem.ContactItem = item;
                        dItem.FileName =System.IO.Path.Combine(fbd.SelectedPath, TransferFile.GetExportFileName(Config.Data.FileMultiSaveFormatVoice, item));


                        if (RecordingFrameSet.AddToDownloadManager.CanExecute(dItem, this))
                            RecordingFrameSet.AddToDownloadManager.Execute(dItem, this);
                        else
                            Tools.Log("Can't execute AddToDownloadManager!", Tools.LogType.Warning);
                    }
                }
            }
        }

        private void PutContactInDownloadQueue(ContactData contact)
        {
            DownloadManagerItem item = new DownloadManagerItem();
            item.ContactItem = contact;

            
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = TransferFile.GetExportFileName(Config.Data.FileSaveFormatVoice, contact);
            if (Application.Current.MainWindow.IsLoaded)
            {
                if (sfd.ShowDialog(Application.Current.MainWindow) == true)
                {
                    item.FileName = sfd.FileName;

                    if (RecordingFrameSet.AddToDownloadManager.CanExecute(item, this))
                        RecordingFrameSet.AddToDownloadManager.Execute(item, this);
                    else
                        Tools.Log("Can't execute AddToDownloadManager!", Tools.LogType.Warning);
                }
            }
        }
        #endregion
        #endregion

        #region Handlers
        private void btnSearchRecordings_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            SearchContacts();
            this.Cursor = Cursors.Arrow;
        }

        private void btnPlayRecording_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            if (this.PlayerState == PlayerStates.Pause)
            {
                mediaPlayer.Play();
                this.PlayerState = PlayerStates.Playing;
            }
            else
                RequestRecordingToPlay();
            this.Cursor = Cursors.Arrow;
        }

        private void btnPauseRecording_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            this.PlayerState = PlayerStates.Pause;
        }

        private void btnStopRecording_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            mediaPlayer_MediaEnded(null, null);
        }

        private void mediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            slTimeProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            slTimeProgress.Value = mediaPlayer.Position.TotalMilliseconds;
            lblCurrentTime.Text = Format.ToDisplayTimeSpan(mediaPlayer.Position);
            lblTotalTime.Text = Format.ToDisplayDuration(mediaPlayer.NaturalDuration);
        }

        private void mediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            timer_Tick(null, null);
            m_MediaPlayerTimer.Stop();
            mediaPlayer.Stop();
            slTimeProgress.Value = 0;
            lblCurrentTime.Text = Format.ToDisplayTimeSpan(mediaPlayer.Position);
            this.PlayerState = PlayerStates.Stopped;
        }

        private void CallCenterControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((FrameworkElement)sender).IsVisible)
            {
                SetToolbarPanel();
            }
            else
            {
                RemoveToolbarPanel();
            }
        }

        private void MainGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Debug
            //
            ContactData newItem = new ContactData();
            if (e.AddedItems != null && e.AddedItems.Count > 0)
                newItem = (ContactData)e.AddedItems[0];
            
            ContactData oldItem = new ContactData();
            
            if(e.RemovedItems != null && e.RemovedItems.Count > 0)
                oldItem = (ContactData)e.RemovedItems[0];

            Tools.Log(string.Format("SelectedItemChanged.ScoreList: {2}. New Item Score: {0}. Old Item Score: {1}", newItem.Score, oldItem.Score, nixxisScoreList1.Score), Tools.LogType.Debug);

            if (newItem == null)
                return;
            //
            //Set playback file info
            if(newItem == m_CurrentItemData)
                pnlGeneralInfoVoice.DataContext = m_CurrentItemFileInfo;
            else
                pnlGeneralInfoVoice.DataContext = null;
            //
            //Get customer data
            m_CustomerDataList.Clear();
            lstCustomerData.ItemsSource = m_CustomerDataList;
            GetCustomerData(newItem);

        } 

        private void ComboBoxTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbo = (ComboBox)sender;

            if (cbo.SelectedValue != null && cbo.SelectedValue.GetType() == typeof(DisplayItem))
                SetAgentList(((DisplayItem)cbo.SelectedValue).Id);
        }

        private void ComboBoxCampaign_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbo = (ComboBox)sender;

            if (cbo.SelectedValue != null && cbo.SelectedValue.GetType() == typeof(DisplayItem))
                SetActivityList(((DisplayItem)cbo.SelectedValue).Id);
        }

        private void ComboBoxActivity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        #endregion

        private void ParameterQualifsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try { this.SearchParameters.Qualification = ((DisplayItem)e.NewValue).Id; }
            catch { }
        }

        private void NixxisDetailedCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnSaveRecording_Click(object sender, RoutedEventArgs e)
        {
            RequestSaveContact();
        }

        private void btnSaveAllRecordings_Click(object sender, RoutedEventArgs e)
        {
            RequestSaveAllContacts();
        }

        private void slTimeProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //TO DO: avoid executing this when timer is updating the slider value.

            Slider sl = (Slider)sender;

            int SliderValue = (int)sl.Value;

            TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
            mediaPlayer.Position = ts;

        }

        private void btnUpdateContactInfo_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            RequestUpdateContactData();
            this.Cursor = Cursors.Arrow;
        }

        private void btnKeepCurrentQualification_Click(object sender, RoutedEventArgs e)
        {
            QualificationTree.UnSelectItem(QualificationTree.SelectedItem);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer == null)
                return;

            if (mediaPlayer.Volume == 0)
                mediaPlayer.Volume = (double)slVolume.Value;
            else
                mediaPlayer.Volume = 0;

        }


    }
}
