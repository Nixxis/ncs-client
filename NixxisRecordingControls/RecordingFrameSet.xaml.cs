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
using Nixxis.Client.Controls;
using ContactRoute.Recording.Config;
using Nixxis.Client.Admin;
using ContactRoute;

namespace Nixxis.Client.Recording
{
    /// <summary>
    /// Interaction logic for RecordingFrameSet.xaml
    /// </summary>
    public partial class RecordingFrameSet : UserControl
    {
        #region Static
        public static PersistentRoutedUICommand ShowCategory { get; private set; }
        public static RoutedUICommand AddToDownloadManager { get; private set; }

        static RecordingFrameSet()
        {
            ShowCategory = new PersistentRoutedUICommand(string.Empty, "ShowCategory", typeof(RecordingFrameSet));
            AddToDownloadManager = new RoutedUICommand(string.Empty, "AddToDownloadManager", typeof(RecordingFrameSet));
        }
        #endregion

        #region Class data
        private string m_MainWindowTitle = string.Empty;
        private string m_CfgLocation = string.Empty;
        private System.Net.NetworkCredential m_User;
        private Profile m_Profile;
        private string m_ConfigfileName = "RecordingProfile.config";
        #endregion

        #region Constructors
        public RecordingFrameSet(ISession session, AdminLight adminLight)
        {
            m_CfgLocation = new Uri(new Uri(session["provisioning"].Location), "Settings").ToString();
            m_User = session.Credential;
            AdminLight = adminLight;
            CurrentUser = m_User.UserName;
            m_MainWindowTitle = Application.Current.MainWindow.Title;

            Tools.Log("--RecordingFrameSet-- InitializeComponent.", Tools.LogType.Info);
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(RecordingFrameSet_IsVisibleChanged);
            InitializeComponent();

            Tools.Log("--RecordingFrameSet-- Load configuration.", Tools.LogType.Info);
            m_Profile = new Profile();
            m_Profile.Location = m_CfgLocation;
            m_Profile.ConfigFile = m_ConfigfileName;
            m_Profile.UserModeId = m_User.UserName;
            m_Profile.Load();

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Try to load useful default values when no config is available

            if (string.IsNullOrEmpty(m_Profile.Data.ServiceDataRoot) || m_Profile.Data.ServiceDataRoot.StartsWith("http://appserver:8088"))
            {
                if (session["reporting"] != null)
                {
                    m_Profile.Data.ServiceDataRoot = session["reporting"].Location;
                    m_Profile.Data.ServiceDataAvailable = !string.IsNullOrEmpty(m_Profile.Data.ServiceDataRoot);

                    Tools.Log("--RecordingFrameSet-- Using default ServiceDataRoot: " + m_Profile.Data.ServiceDataRoot ?? "", Tools.LogType.Info);
                }
            }

            if (string.IsNullOrEmpty(m_Profile.Data.ServiceRelayRoot) || m_Profile.Data.ServiceRelayRoot.StartsWith("http://appserver:8088"))
            {
                if (session["recording"] != null)
                {
                    m_Profile.Data.ServiceRelayRoot = session["recording"].Location;
                    m_Profile.Data.ServiceRelayAvailable = !string.IsNullOrEmpty(m_Profile.Data.ServiceRelayRoot);

                    Tools.Log("--RecordingFrameSet-- Using default ServiceRelayRoot: " + m_Profile.Data.ServiceRelayRoot ?? "", Tools.LogType.Info);
                }
            }

            if (string.IsNullOrEmpty(m_Profile.Data.UserXmlUrl) || m_Profile.Data.UserXmlUrl.StartsWith("http://appserver:8088"))
            {
                if (session["agent"] != null)
                {
                    m_Profile.Data.UserXmlUrl = session["agent"].Location;
                    if (!m_Profile.Data.UserXmlUrl.EndsWith("/"))
                        m_Profile.Data.UserXmlUrl += "/";
                    m_Profile.Data.UserXmlUrl += "?fmt=uri&action={0}";

                    Tools.Log("--RecordingFrameSet-- Using default UserXmlUrl: " + m_Profile.Data.UserXmlUrl ?? "", Tools.LogType.Info);
                }
            }

            if (string.IsNullOrEmpty(m_Profile.Data.ServiceAdminRoot) || m_Profile.Data.ServiceAdminRoot.StartsWith("tcp://appserver:7654"))
            {
                if (session["admin"] != null)
                {
                    m_Profile.Data.ServiceAdminRoot = session["admin"].Location;
                    m_Profile.Data.ServiceAdminAvailable = !string.IsNullOrEmpty(m_Profile.Data.ServiceAdminRoot);

                    Tools.Log("--RecordingFrameSet-- Using default ServiceAdminRoot: " + m_Profile.Data.ServiceAdminRoot ?? "", Tools.LogType.Info);
                }
            }

            if (m_Profile.Data.VoiceServerList == null || m_Profile.Data.VoiceServerList.Length == 0)
            {
                TransferProfileData DefaultVoiceServer = new TransferProfileData(TransferProfileData.Medias.Voice);

                DefaultVoiceServer.Type = TransferProfileData.Types.AppServer;
                DefaultVoiceServer.Host = session["recording"].Location.TrimEnd('/');
                DefaultVoiceServer.Extension = new string[] { "mp3", "wav" };

                m_Profile.Data.VoiceServerList = new TransferProfileData[] { DefaultVoiceServer };

                Tools.Log("--RecordingFrameSet-- Using default TransferProfileData: " + DefaultVoiceServer.Host ?? "", Tools.LogType.Info);
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Tools.DebugAllActive = m_Profile.Data.DebugAllMsg;

            Tools.Log("--RecordingFrameSet-- Apply default settings.", Tools.LogType.Info);

            Application.Current.MainWindow.Title = m_Profile.Data.Description + " - " + m_MainWindowTitle;
            callcenter.Config = m_Profile;
            callcenter.IsAllowedSearchDateTime = m_Profile.Data.SearchOptionsArray[ProfileData.SearchOptionsList.DateTime];
            callcenter.IsAllowedSearchContact = m_Profile.Data.SearchOptionsArray[ProfileData.SearchOptionsList.Contact];
            callcenter.IsAllowedSearchCampaign = m_Profile.Data.SearchOptionsArray[ProfileData.SearchOptionsList.Campaign];

            callcenter.IsAllowedMediaTypeInbound = m_Profile.Data.SearchOptionsArray[ProfileData.SearchOptionsList.Inbound];
            callcenter.IsAllowedMediaTypeManuel = m_Profile.Data.SearchOptionsArray[ProfileData.SearchOptionsList.ManuelCall];
            callcenter.IsAllowedMediaTypeOutbound = m_Profile.Data.SearchOptionsArray[ProfileData.SearchOptionsList.Outbound];
            callcenter.IsAllowedMediaTypeDirect = m_Profile.Data.SearchOptionsArray[ProfileData.SearchOptionsList.DirectCall];
            callcenter.IsAllowedMediaTypeChat = m_Profile.Data.SearchOptionsArray[ProfileData.SearchOptionsList.ChatContacts];

            if (!callcenter.IsAllowedMediaTypeDirect)
            {
                callcenter.SearchParameters.IncludeDirect = false;
            }

            if (!callcenter.IsAllowedMediaTypeInbound)
            {
                callcenter.SearchParameters.IncludeInbound = false;
            }
            if (!callcenter.IsAllowedMediaTypeManuel)
            {
                callcenter.SearchParameters.IncludeManual = false;
            }
            if (!callcenter.IsAllowedMediaTypeOutbound)
            {
                callcenter.SearchParameters.IncludeOutbound = false;
            }
            if (!callcenter.IsAllowedMediaTypeChat)
            {
                callcenter.SearchParameters.IncludeChat = false;
            }

            downloadmanager.Config = m_Profile;
        }
        #endregion

        #region Properties
        public AdminLight AdminLight { get; set; }
        public string CurrentUser { get; set; }
        #endregion

        void RecordingFrameSet_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as RecordingFrameSet).IsVisible)
            {
                RecordingFrameSet_Loaded(sender, null);
            }
            else
            {
                RecordingFrameSet_Unloaded(sender, null);
            }
        }

        void RecordingFrameSet_Loaded(object sender, RoutedEventArgs e)
        {

            Focus();
            NixxisBaseExpandPanel nep = null;

            nep = FindResource("MainMenuPanel") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.SetPanelCommand.CanExecute(nep))
                {
                    NixxisGrid.SetPanelCommand.Execute(nep);
                }
            }
        }

        void RecordingFrameSet_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Title = m_MainWindowTitle;

            NixxisBaseExpandPanel nep = null;

            nep = FindResource("MainMenuPanel") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                    NixxisGrid.RemovePanelCommand.Execute(nep);
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == RecordingFrameSet.ShowCategory)
            {
                RecordingFrameSet.ShowCategory.State = e.Parameter;
                Helpers.ApplyToChildren<UserControl>(this, VisibilityProperty, System.Windows.Visibility.Collapsed, (elm) => (!elm.Name.Equals(e.Parameter as string) && "recordingctrl".Equals(elm.Tag)));
                Helpers.ApplyToChildren<UserControl>(this, VisibilityProperty, System.Windows.Visibility.Visible, (elm) => (elm.Name.Equals(e.Parameter as string) && "recordingctrl".Equals(elm.Tag)));
            }
            else if (e.Command == RecordingFrameSet.AddToDownloadManager)
            {
                downloadmanager.AddDownloadItem((DownloadManagerItem)e.Parameter);
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == RecordingFrameSet.ShowCategory)
            {
                switch (e.Parameter as string)
                {
                    case "callcenter":
                        e.CanExecute = (callcenter.Visibility != System.Windows.Visibility.Visible);
                        break;
                    case "office":
                        e.CanExecute = (office.Visibility != System.Windows.Visibility.Visible);
                        break;
                    case "downloadmanager":
                        e.CanExecute = (downloadmanager.Visibility != System.Windows.Visibility.Visible);
                        break;
                }
            }
            else if (e.Command == RecordingFrameSet.AddToDownloadManager)
            {
                e.CanExecute = true;
            }
            e.Handled = true;
        }
    }
}
