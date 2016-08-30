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
using System.ComponentModel;
using System.Timers;
using System.Windows.Threading;
using Nixxis.Client.Controls;
using Nixxis.Client.Admin;
using Nixxis.Client.Supervisor;
using Nixxis.Client.Agent;
using System.Threading;
using System.Windows.Controls.Primitives;
using Nixxis.Client.Recording;
using System.Configuration;
using Nixxis.Client.Reporting;
using System.Xml;
using System.Globalization;
using System.Net;
using System.IO;
using ContactRoute;

namespace Nixxis.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow, IAgentLoginInfoContainer
    {


        public static TranslationContext TranslationContext = new TranslationContext("MainWindow");

        internal AgentFrameSet m_AgentFrameSet;
        internal SupFrameSet m_SupFrameSet;
        internal AdminFrameSet m_AdminFrameSet;
        internal RecordingFrameSet m_RecordingFramSet;
        internal ReportingFrameSet m_ReportingFrameSet;
        internal StartPageControl m_StartPage;

        internal SessionInfo m_Session;
        internal AgentLoginInfo m_AgentLoginData;
        internal HttpLink m_ClientLink;
        internal string m_AgentConnectionInfo;
        internal Nixxis.ClientV2.Supervision m_SupLink;
        internal string m_AgentLoginInfo;
        internal string m_AgentId;

        private WaitScreen m_ws = null;
        private AdminCore m_AdminCore = null;
        private AdminLight m_RecAdminCore = null;

        private void LoadTranslations()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=loadTranslations", ((ISession)AppDomain.CurrentDomain.GetData("SessionInfo"))["admin"].Location));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Get;
            using (WebResponse response = webRequest.GetResponse())
            {
                using (Stream str = response.GetResponseStream())
                {
                    TranslationContext.LoadTranslations(str);
                }
            }

        }



        public MainWindow(XmlDocument servicesDoc, XmlDocument authentication)
        {
            if (ConfigurationManager.AppSettings["Language"] != null)
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(ConfigurationManager.AppSettings["Language"]);
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;


            m_Session = new SessionInfo(servicesDoc, authentication);

            AppDomain.CurrentDomain.SetData("SessionInfo", m_Session);

            if(authentication != null)
                m_AgentLoginData = new AgentLoginInfo(authentication.SelectSingleNode("AgentLogin/agent"));

            m_AgentLoginData.LoadFromNode(authentication.SelectSingleNode("AgentLogin/reporting"));

            try
            {
                if (!string.IsNullOrEmpty(m_AgentLoginData.GuiLanguage))
                {
                    System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(m_AgentLoginData.GuiLanguage);
                    System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(m_AgentLoginData.GuiLanguage);
                }

            }
            catch
            {
            }

            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            LoadTranslations();


            NixxisGrid.SetPanelCommand.Action = NixxisCommandHandler;
            NixxisGrid.RemovePanelCommand.Action = NixxisCommandHandler;

            InitializeComponent();

            try
            {
                XmlNode node = authentication.SelectSingleNode("AgentLogin");
                if (node != null)
                {
                    (FindResource("VSP") as VSPHelper).IsVirtualizing = System.Xml.XmlConvert.ToBoolean(node.Attributes["ClientVirtualization"].Value);
                }
            }
            catch
            {
            }



            if (IsNCS || IsDemo)
                this.Title = TranslationContext.Translate("Nixxis Contact Suite");
            else
                this.Title = TranslationContext.Translate("Nixxis Contact Express");

            if (!string.IsNullOrEmpty(m_Session.DomainName))
                this.Title = m_Session.DomainName + " - " + this.Title;

            this.Closing += new CancelEventHandler(MainWindow_Closing);
            this.Closed += new EventHandler(MainWindow_Closed);

            m_StartPage = new StartPageControl();
            ApplicationContent.Content = m_StartPage;


            if (!SkipLogin)
            {
                m_ClientLink = new HttpLink(m_Session);
                m_SupLink = new ClientV2.Supervision(m_ClientLink); 

                m_ClientLink.AgentTeamsChanged += new HttpLinkEventDelegate(m_ClientLink_AgentTeamsChanged);
                
                m_ClientLink.Connect(out m_AgentConnectionInfo);
                m_SupLink.Connected();
            }
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            m_AgentFrameSet = null;

            if (m_ClientLink != null)
                m_ClientLink.Dispose();
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (m_ClientLink != null && m_ClientLink.Contacts.Count > 0 && m_ClientLink.SessionId != null && !m_ClientLink.ServerConnectionBroken)
            {
                MessageBox.Show(TranslationContext.Translate("Please close contacts first"));
                e.Cancel = true;
                return;
            }
            else if (m_AdminFrameSet != null && m_AdminFrameSet.NeedSave())
            {
                ConfirmationDialog dlg = new ConfirmationDialog();
                dlg.MessageText = TranslationContext.Translate("You made some unsaved changes to administration data.\nAre you sure you want to quit the application without saving?");
                dlg.Owner = Application.Current.MainWindow;

                if (!dlg.ShowDialog().GetValueOrDefault())
                {
                    e.Cancel = true;
                    return;
                }
            }

            // here it means that we are quiting...
            if(m_AdminFrameSet!=null)
                m_AdminFrameSet.ClearReservations();
            
        }

        private void ClientLogin(ClientRoles roles)
        {
            //TODO: do only if really needed (sup right, sup asked)
            m_ClientLink.Login(m_Session.Credential, m_Session.Extension, roles, out m_AgentLoginInfo, out m_AgentId);
        }

        private void ClientChangeRole(ClientRoles roles)
        {
            m_ClientLink.ChangeRole(roles);
        }

        void m_ClientLink_AgentTeamsChanged()
        {
            m_AgentLoginData.AllowTeamSelection = true;
        }

        private void NixxisButton_Click_0(object sender, RoutedEventArgs e)
        {
            Resources[typeof(NixxisButton)] = FindResource("NixxisButtonStyle1");
            Resources["BackgroundBrush"] = FindResource("BackgroundBrush1");
        }

        private void NixxisButton_Click_1(object sender, RoutedEventArgs e)
        {
            Resources[typeof(NixxisButton)] = FindResource("NixxisButtonStyle2");
            Resources["BackgroundBrush"] = FindResource("BackgroundBrush2");
        }

        private void NixxisButton_Click_2(object sender, RoutedEventArgs e)
        {
            Resources[typeof(NixxisButton)] = FindResource("NixxisButtonStyle3");
            Resources["BackgroundBrush"] = FindResource("BackgroundBrush2");
        }

        private void NixxisCommandHandler(NixxisCommand command, object parameter)
        {
            if (command == NixxisGrid.SetPanelCommand)
            {
                UIElement uie = (UIElement)(parameter);
                if (!BottomGrid.Children.Contains(uie))
                    BottomGrid.Children.Add(uie);
                string strPanel = NixxisGrid.GetPanel(uie);
                uie.Visibility = System.Windows.Visibility.Visible;
                NixxisGrid.SetPanel(uie, string.Empty);
                NixxisGrid.SetPanel(uie, strPanel);
            }
            else if (command == NixxisGrid.RemovePanelCommand)
            {
                UIElement uie = (UIElement)(parameter);
                if (BottomGrid.Children.Contains(uie))
                {
                    string strPanel = NixxisGrid.GetPanel(uie);
                    BottomGrid.Children.Remove(uie);
                    // now, reactivate the previous panel...
                    BottomGrid.EnsurePanelVisible(strPanel);
                }
            }
        }

        public void ReloadCore(AdminCore core)
        {
            m_AdminFrameSet = new AdminFrameSet(core);

            ApplicationContent.Content = m_AdminFrameSet;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            if (e.Command == NixxisGrid.SetPanelCommand)
            {
                UIElement uie = (UIElement)(e.Parameter);
                if (!BottomGrid.Children.Contains(uie))
                    BottomGrid.Children.Add(uie);
                string strPanel = NixxisGrid.GetPanel(uie);
                uie.Visibility = System.Windows.Visibility.Visible;
                NixxisGrid.SetPanel(uie, string.Empty);
                NixxisGrid.SetPanel(uie, strPanel);
            }
            else if (e.Command == NixxisGrid.RemovePanelCommand)
            {
                UIElement uie = (UIElement)(e.Parameter);
                if (BottomGrid.Children.Contains(uie))
                {
                    string strPanel = NixxisGrid.GetPanel(uie);
                    BottomGrid.Children.Remove(uie);
                    // now, reactivate the previous panel...
                    BottomGrid.EnsurePanelVisible(strPanel);
                }
            }
            else
            {
                object oldValue = ApplicationContent.Content;

                // Bug workaround
                // Ensure no datagrid inside is in Full row select
                Helpers.ApplyToVisualChildren<DataGrid>((ApplicationContent.Content as DependencyObject), DataGrid.SelectionUnitProperty, DataGridSelectionUnit.CellOrRowHeader);

                switch (e.Parameter.ToString())
                {
                    case "adm":
                        // TODO: to improve.... this forces a reload when accessing the admin
                        m_RecAdminCore = null;

                        AdminFrameSet.ShowCategory.State = null;

                        WaitScreen ws = new WaitScreen();
                        ws.Owner = Application.Current.MainWindow;
                        ws.Show();
                        if (m_AdminCore == null)
                        {
                            m_AdminCore = new AdminCore(LoggedIn);

                            LoadProgressDelegate lpd = new LoadProgressDelegate(ws.LoadProgressEvent);
                            AdminCore.LoadProgressEvent += lpd;

                            DetailedLoadProgressDelegate dlpd = new DetailedLoadProgressDelegate(ws.DetailedLoadProgressEvent);
                            AdminCore.DetailedLoadProgressEvent += dlpd;


                            List<XmlNode> corrections = new List<XmlNode>();

                            for (int i = 0; i < 2; i++)
                            {
                                corrections.Clear();

                                try
                                {
                                    m_AdminCore.Load();
                                    break;
                                }
                                catch (AdminValidationException ave)
                                {
                                    ConfirmationDialog dlg = null;

                                    XmlNode nde = ave.ValidationErrors.SelectSingleNode(@"/Failure/Description");
                                    if (nde != null)
                                    {
                                        dlg = new ConfirmationDialog();
                                        dlg.MessageText = nde.InnerText;
                                    }
                                    else
                                    {
                                        List<string> lstMsg = new List<string>();
                                        XmlNode correction = null;
                                        bool allvalidationErrorsHaveCorrection = true;
                                        foreach (XmlNode n in ave.ValidationErrors.SelectNodes(@"/Failure/ValidationErrors/ValidationError"))
                                        {
                                            if (lstMsg.Count() < 10)
                                                lstMsg.Add(n.InnerText);

                                            correction = n.SelectSingleNode("PossibleCorrection");
                                            if (correction == null)
                                            {
                                                allvalidationErrorsHaveCorrection = false;
                                            }
                                            else
                                            {
                                                foreach (XmlNode child in correction.ChildNodes)
                                                    corrections.Add(child);
                                            }

                                        }

                                        if (allvalidationErrorsHaveCorrection && i==0)
                                        {
                                            ConfirmationDialog correctionDlg = new ConfirmationDialog();
                                            correctionDlg.Owner = Application.Current.MainWindow;
                                            correctionDlg.IsInfoDialog = false;
                                            correctionDlg.MessageText = TranslationContext.Translate("Validation errors have been detected in current administration database. Do you want to correct them?");
                                            if (correctionDlg.ShowDialog().GetValueOrDefault())
                                            {
                                                XmlDocument saveResult = m_AdminCore.Save(corrections);

                                                if (saveResult.DocumentElement.Name == "Success")
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    dlg = new ConfirmationDialog();
                                                    dlg.MessageText = string.Join("\n", lstMsg);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            dlg = new ConfirmationDialog();
                                            dlg.MessageText = string.Join("\n", lstMsg);
                                        }
                                    }
                                    if (dlg != null)
                                    {
                                        dlg.Owner = Application.Current.MainWindow;
                                        dlg.IsInfoDialog = true;
                                        dlg.ShowDialog();
                                        try
                                        {
                                            dlg.Close();
                                        }
                                        catch
                                        {
                                        }
                                    }

                                    ws.Close();
                                    Application.Current.Shutdown();
                                    return;

                                }
                            }


                            AdminCore.LoadProgressEvent -= lpd;

                            AdminCore.DetailedLoadProgressEvent -= dlpd;



                            if (m_AdminCore == null || !m_AdminCore.HasBeenLoaded)
                            {
                                ConfirmationDialog dlg = new ConfirmationDialog();
                                dlg.IsInfoDialog = true;
                                dlg.MessageText = TranslationContext.Translate("Administration cannot be loaded.\nPlease verify http connectivity and server version.\nThe application will be closed.");
                                dlg.Owner = Application.Current.MainWindow;
                                dlg.ShowDialog();
                                ws.Close();
                                Application.Current.Shutdown();
                                return;
                            }

                            ws.Progress = -1;
                            ws.Text = TranslationContext.Translate("Preparing to display data...");
                            Helpers.WaitForPriority();

                        }

                        if (m_AdminFrameSet == null)
                            m_AdminFrameSet = new AdminFrameSet(m_AdminCore);

                        ApplicationContent.Content = m_AdminFrameSet;

                        Helpers.ApplyToVisualChildren<DataGrid>((ApplicationContent.Content as DependencyObject), DataGrid.SelectionUnitProperty, DataGridSelectionUnit.FullRow);

                        ws.Close();

                        break;

                    case "sup":
                        if (string.IsNullOrEmpty(m_ClientLink.AgentId))
                            ClientLogin(ClientRoles.Supervisor);
                        else
                            ClientChangeRole(ClientRoles.Supervisor);

                        if (m_SupFrameSet == null)
                            m_SupFrameSet = new SupFrameSet(m_Session, m_SupLink, m_AgentLoginData.Id);

                        ApplicationContent.Content = m_SupFrameSet;
                        break;

                    case "agt":
                        if (string.IsNullOrEmpty(m_ClientLink.AgentId))
                            ClientLogin(ClientRoles.Agent);
                        else
                            ClientChangeRole(ClientRoles.Agent);

                        if (m_AgentFrameSet == null)
                        {
                            string cfgLocation = new Uri(new Uri(m_Session["provisioning"].Location), "Settings").ToString();
                            m_AgentFrameSet = new AgentFrameSet(m_Session, m_ClientLink, m_AgentLoginData.AllowTeamSelection, this.Version, cfgLocation);
                        }


                        ApplicationContent.Content = m_AgentFrameSet;
                        break;

                    case "rec":
                        if (m_RecAdminCore == null)
                        {
                            m_RecAdminCore = new AdminLight(LoggedIn);

                            Helpers.StartLongRunningTask((DoWorkEventHandler)((a, b) =>
                            {
                                m_RecAdminCore.Load();
                            }));
                            if (m_RecAdminCore == null || !m_RecAdminCore.HasBeenLoaded)
                            {
                                ConfirmationDialog dlg = new ConfirmationDialog();
                                dlg.IsInfoDialog = true;
                                dlg.MessageText = TranslationContext.Translate("Administration cannot be loaded.\nPlease verify http connectivity and server version.\nThe application will be closed.");
                                dlg.Owner = Application.Current.MainWindow;
                                dlg.ShowDialog();
                                Application.Current.Shutdown();
                                return;
                            }

                        }

                        if (m_RecordingFramSet == null)
                        {
                            m_RecordingFramSet = new RecordingFrameSet(m_Session, m_RecAdminCore);
                        }

                        ApplicationContent.Content = m_RecordingFramSet;
                        break;
                    case "rpt":
                        if (m_ReportingFrameSet == null)
                            m_ReportingFrameSet = new ReportingFrameSet(m_Session["reporting"].Location);

                        ApplicationContent.Content = m_ReportingFrameSet;
                        break;

                }

                if (oldValue is DependencyObject)
                    BindingOperations.ClearAllBindings((DependencyObject)oldValue);
            }
        }


        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (e.Command == NixxisGrid.RemovePanelCommand)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;

                if (!SkipLogin)
                {
                    switch (e.Parameter.ToString())
                    {
                        case "adm":
                            e.CanExecute = m_AgentLoginData != null && m_AgentLoginData.IsAdmin  && (ApplicationContent.Content == null || !(ApplicationContent.Content is Nixxis.Client.Admin.AdminFrameSet));
                            break;

                        case "sup":
                            e.CanExecute = m_AgentLoginData != null && m_AgentLoginData.IsSupervisor && (ApplicationContent.Content == null || !(ApplicationContent.Content is SupFrameSet));
                            break;

                        case "agt":
                            e.CanExecute = m_AgentLoginData != null && m_AgentLoginData.IsAgent && (ApplicationContent.Content == null || !(ApplicationContent.Content is AgentFrameSet));
                            break;

                        case "rec":
                            e.CanExecute = m_AgentLoginData != null && m_AgentLoginData.IsRecorder && (ApplicationContent.Content == null || !(ApplicationContent.Content is RecordingFrameSet));
                            break;

                        case "rpt":
                            e.CanExecute = m_AgentLoginData != null && m_AgentLoginData.IsReporter && (ApplicationContent.Content == null || !(ApplicationContent.Content is ReportingFrameSet));
                            break;
                    }
                }
                else
                {
                    e.CanExecute = e.Parameter.ToString().Equals("adm");
                }

            }
            e.Handled = true;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke
            (
            DispatcherPriority.Background,
            (SendOrPostCallback)delegate(object arg)
            {
                var f = arg as DispatcherFrame;
                f.Continue = false;
            },
            frame
            );
            Dispatcher.PushFrame(frame);
        }

        public AdminFrameSet AdminFrameSet
        {
            get { return m_AdminFrameSet; }
        }

        public AdminCore Core
        {
            get { return m_AdminCore; }
            set { m_AdminCore = value; }
        }

        private void TheWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_AgentLoginData != null)
            {
                if (    m_AgentLoginData.IsAgent
                    && !m_AgentLoginData.IsRecorder
                    && !m_AgentLoginData.IsReporter
                    && !m_AgentLoginData.IsSupervisor
                    && !m_AgentLoginData.IsAdmin)
                {
                    GeneralCommands.ShowApplication.Execute("agt", this);
                }
                else if ( !m_AgentLoginData.IsAgent
                    && m_AgentLoginData.IsRecorder
                    && !m_AgentLoginData.IsReporter
                    && !m_AgentLoginData.IsSupervisor
                    && !m_AgentLoginData.IsAdmin)
                {
                    GeneralCommands.ShowApplication.Execute("rec", this);
                }
                else if ( !m_AgentLoginData.IsAgent
                    && !m_AgentLoginData.IsRecorder
                    && m_AgentLoginData.IsReporter
                    && !m_AgentLoginData.IsSupervisor
                    && !m_AgentLoginData.IsAdmin)
                {
                    GeneralCommands.ShowApplication.Execute("rep", this);
                }
                else if (    !m_AgentLoginData.IsAgent
                    && !m_AgentLoginData.IsRecorder
                    && !m_AgentLoginData.IsReporter
                    && m_AgentLoginData.IsSupervisor
                    && !m_AgentLoginData.IsAdmin)
                {
                    GeneralCommands.ShowApplication.Execute("sup", this);
                }
                else if (    !m_AgentLoginData.IsAgent
                    && !m_AgentLoginData.IsRecorder
                    && !m_AgentLoginData.IsReporter
                    && !m_AgentLoginData.IsSupervisor
                    && m_AgentLoginData.IsAdmin)
                {
                    GeneralCommands.ShowApplication.Execute("adm", this);
                }

            }

            if (m_AgentLoginData != null
                && m_AgentLoginData.IsAgent
                && !m_AgentLoginData.IsRecorder
                && !m_AgentLoginData.IsReporter                
                && !m_AgentLoginData.IsSupervisor
                && !m_AgentLoginData.IsAdmin)
            {
                GeneralCommands.ShowApplication.Execute("agt", this);
            }

            if (SkipLogin)
            {
                GeneralCommands.ShowApplication.Execute("adm", this);
            }
                
        }

        public AgentLoginInfo AgentLoginInfo
        {
            get
            {
                return m_AgentLoginData;
            }
        }

        public string LoggedIn
        {
            get
            {
                if (m_AgentLoginData == null)
                    return null;
                return m_AgentLoginData.Id;
            }
        }

        public bool IsDemo
        {
            get
            {
                string strTemp = ConfigurationManager.AppSettings["Version"];
                if (strTemp!=null)
                {
                    return (strTemp=="Demo");
                }
                object obj = AppDomain.CurrentDomain.GetData("service_demo");
                if (obj == null)
                    return false;
                else
                    return (bool)obj;
            }
        }

        public bool IsNCS
        {
            get
            {
                string strTemp = ConfigurationManager.AppSettings["Version"];
                if (strTemp != null)
                {
                    return (strTemp == "NCS");
                }
                object obj = AppDomain.CurrentDomain.GetData("service_demo");
                if (obj != null && (bool)obj)
                {
                    return false;
                }
                else
                {
                    obj = AppDomain.CurrentDomain.GetData("service_flags");
                    return obj==null || obj is int && ((int)obj > 0) || obj is string && Int32.Parse((string)obj)>0;
                }                
            }
        }

        public bool IsExpress
        {
            get
            {
                string strTemp = ConfigurationManager.AppSettings["Version"];
                if (strTemp != null)
                {
                    return (strTemp == "Express" || strTemp == "NCE");
                }
                object obj = AppDomain.CurrentDomain.GetData("service_demo");
                if (obj != null && (bool)obj)
                {
                    return false;
                }
                else
                {
                    obj = AppDomain.CurrentDomain.GetData("service_flags");
                    return obj is int && ((int)obj == 0) || obj is string && Int32.Parse((string)obj) == 0;
                }
            }
        }

        // temp
        public string Version
        {
            get
            {
                if (IsDemo)
                    return "Demo";
                else if (IsNCS)
                    return "NCS";
                else 
                    return "Express";

            }
        }

        internal bool SkipLogin
        {
            get
            {
                return (m_AgentLoginData == null);
            }
        }

    }

}
