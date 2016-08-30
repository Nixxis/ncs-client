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
using System.Data.SqlClient;
using System.Xml;
using Nixxis.Client.Admin;
using System.Windows.Controls.Primitives;
using Nixxis.Client.Controls;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Globalization;
using System.Threading;
using System.IO;
using ContactRoute;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Interaction logic for AdminFrameSet.xaml
    /// </summary>
    public partial class AdminFrameSet : UserControl, INotifyPropertyChanged
    {
        public static RoutedUICommand PauseActivity { get; private set; }
        public static RoutedUICommand UnpauseActivity { get; private set; }

        public static RoutedUICommand PauseChatActivities { get; private set; }
        public static RoutedUICommand UnpauseChatActivities { get; private set; }
        public static RoutedUICommand PauseMailActivities { get; private set; }
        public static RoutedUICommand UnpauseMailActivities { get; private set; }

        public static RoutedUICommand PauseInboundActivities { get; private set; }
        public static RoutedUICommand UnpauseInboundActivities { get; private set; }
        public static RoutedUICommand PauseOutboundActivities { get; private set; }
        public static RoutedUICommand UnpauseOutboundActivities { get; private set; }


        public static RoutedUICommand AffectToCampaign { get; private set; }
        public static RoutedUICommand AffectToCore { get; private set; }
        public static RoutedUICommand AffectToActivities{ get; private set; }
        public static RoutedUICommand ShowObject { get; private set; }
        public static PersistentRoutedUICommand ShowCategory { get; private set; }
        public static RoutedUICommand SwitchCategory { get; private set; }
        public static RoutedUICommand AdminObjectAddOperation { get; private set; }
        public static RoutedUICommand AdminObjectDuplicateOperation { get; private set; }
        public static RoutedUICommand AdminObjectPrintOperation { get; private set; }
        public static RoutedUICommand AdminObjectDeleteOperation { get; private set; }

        public static RoutedUICommand AdminObjectMoveUpOperation { get; private set; }
        public static RoutedUICommand AdminObjectMoveDownOperation { get; private set; }
        public static RoutedUICommand DataManagement { get; private set; }

        public static RoutedUICommand CommitChanges { get; private set; }

        private string m_MainWindowTitle = null;

        private static TranslationContext TranslationContext = new TranslationContext("AdminFrameSet");
        private static string Translate(string text)
        {
            return TranslationContext.Translate(text);
        }


        static AdminFrameSet()
        {
            PauseActivity = new RoutedUICommand(string.Empty, "PauseActivity", typeof(AdminFrameSet));
            UnpauseActivity = new RoutedUICommand(string.Empty, "UnpauseActivity", typeof(AdminFrameSet));

            PauseInboundActivities = new RoutedUICommand(string.Empty, "PauseInboundActivities", typeof(AdminFrameSet));
            UnpauseInboundActivities = new RoutedUICommand(string.Empty, "UnpauseInboundActivities", typeof(AdminFrameSet));
            PauseOutboundActivities = new RoutedUICommand(string.Empty, "PauseOutboundActivities", typeof(AdminFrameSet));
            UnpauseOutboundActivities = new RoutedUICommand(string.Empty, "UnpauseOutboundActivities", typeof(AdminFrameSet));

            PauseMailActivities = new RoutedUICommand(string.Empty, "PauseMailActivities", typeof(AdminFrameSet));
            UnpauseMailActivities = new RoutedUICommand(string.Empty, "UnpauseMailActivities", typeof(AdminFrameSet));
            PauseChatActivities = new RoutedUICommand(string.Empty, "PauseChatActivities", typeof(AdminFrameSet));
            UnpauseChatActivities = new RoutedUICommand(string.Empty, "UnpauseChatActivities", typeof(AdminFrameSet));


            ShowObject = new RoutedUICommand(string.Empty, "ShowObject", typeof(AdminFrameSet));
            AffectToCampaign = new RoutedUICommand(string.Empty, "AffectToCampaign", typeof(AdminFrameSet));
            AffectToCore = new RoutedUICommand(string.Empty, "AffectToCore", typeof(AdminFrameSet));
            AffectToActivities = new RoutedUICommand(string.Empty, "AffectToActivities", typeof(AdminFrameSet));
            ShowCategory = new PersistentRoutedUICommand(string.Empty, "ShowCategory", typeof(AdminFrameSet));
            SwitchCategory = new RoutedUICommand(string.Empty, "SwitchCategory", typeof(AdminFrameSet));
            AdminObjectAddOperation = new RoutedUICommand(string.Empty, "AdminObjectAddOperation", typeof(AdminFrameSet));
            AdminObjectDuplicateOperation = new RoutedUICommand(string.Empty, "AdminObjectDuplicateOperation", typeof(AdminFrameSet));
            AdminObjectPrintOperation = new RoutedUICommand(string.Empty, "AdminObjectPrintOperation", typeof(AdminFrameSet));
            AdminObjectDeleteOperation = new RoutedUICommand(string.Empty, "AdminObjectDeleteOperation", typeof(AdminFrameSet));
            AdminObjectMoveUpOperation = new RoutedUICommand(string.Empty, "AdminObjectMoveUpOperation", typeof(AdminFrameSet));
            AdminObjectMoveDownOperation = new RoutedUICommand(string.Empty, "AdminObjectMoveDownOperation", typeof(AdminFrameSet));
            DataManagement = new RoutedUICommand(string.Empty, "DataManagement", typeof(AdminFrameSet));

            CommitChanges = new RoutedUICommand(string.Empty, "CommitChanges", typeof(AdminFrameSet));

        }

        private static GeneralSettings m_Settings = new GeneralSettings((IMainWindow)Application.Current.MainWindow);

        public static GeneralSettings Settings
        {
            get
            {
                return m_Settings;
            }
            set
            {
                m_Settings = value;
            }
        }

        public AdminCore TheCore { get; set; }

        public AdminFrameSet(AdminCore core)
        {

            if (ConfigurationManager.AppSettings["PathUpload"] == null)
            {
                try
                {

                    HttpWebRequest wr = WebRequest.Create(string.Format("{0}?action=startupInfo", ((ISession)AppDomain.CurrentDomain.GetData("SessionInfo"))["admin"].Location)) as HttpWebRequest;
                    wr.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                    wr.Method = WebRequestMethods.Http.Get;
                    using (HttpWebResponse response = wr.GetResponse() as HttpWebResponse)
                    {
                        XmlDocument doc = new XmlDocument();
                        using (System.IO.Stream s = response.GetResponseStream())
                            doc.Load(s);
                        AdminCore.PathUpload = doc.SelectSingleNode("AdminInfo/Upload").InnerText;
                        AdminCore.PathSounds = doc.SelectSingleNode("AdminInfo/Sounds").InnerText;
                        AdminCore.PathAttachments = doc.SelectSingleNode("AdminInfo/Attachments").InnerText;
                        AdminCore.PathImportExports = doc.SelectSingleNode("AdminInfo/ImportExports").InnerText;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }
            }
            else
                AdminCore.PathUpload = ConfigurationManager.AppSettings["PathUpload"];

            if (ConfigurationManager.AppSettings["PathImportExports"] != null)
                AdminCore.PathImportExports = ConfigurationManager.AppSettings["PathImportExports"];

            if (ConfigurationManager.AppSettings["PathAttachments"] != null)
                AdminCore.PathAttachments = ConfigurationManager.AppSettings["PathAttachments"];

            if (ConfigurationManager.AppSettings["PathSounds"] != null)
                AdminCore.PathSounds = ConfigurationManager.AppSettings["PathSounds"];

            if (AdminCore.PathImportExports == null)
                AdminCore.PathImportExports = "ImportExports";

            if (AdminCore.PathSounds == null)
                AdminCore.PathSounds = "HomeSounds";

            if (AdminCore.PathAttachments == null)
                AdminCore.PathAttachments = "Attachments";

            TheCore = core;

            IsVisibleChanged += new DependencyPropertyChangedEventHandler(AdminFrameSet_IsVisibleChanged);

            Helpers.StartLongRunningTask((DoWorkEventHandler)((a, b) =>
            {
                Init();
            }));

            if (TheCore == null || !TheCore.HasBeenLoaded)
            {
                ConfirmationDialog dlg = new ConfirmationDialog();
                dlg.IsInfoDialog = true;
                dlg.MessageText = Translate("Administration cannot be loaded.\nPlease verify http connectivity and server version.\nThe application will be closed.");
                dlg.Owner = Application.Current.MainWindow;
                dlg.ShowDialog();
                Application.Current.Shutdown();
                return;
            }

            TheCore.ClearReservations(TheCore.GetSessionId((Application.Current.MainWindow as IMainWindow).LoggedIn));

            InitializeComponent();

        }
        
        public AdminFrameSet(): this(null)
        {

        }

        void AdminFrameSet_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as AdminFrameSet).IsVisible)
            {
                AdminFrameSet_Loaded(sender, null);
            }
            else
            {
                AdminFrameSet_Unloaded(sender, null);
            }

        }

        void AdminFrameSet_Loaded(object sender, RoutedEventArgs e)
        {
            m_MainWindowTitle = Application.Current.MainWindow.Title;

            Focus();

            NixxisBaseExpandPanel nep = FindResource("AdminPanel") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.SetPanelCommand.CanExecute(nep))
                    NixxisGrid.SetPanelCommand.Execute(nep);
            }

            nep = FindResource("DetailsPanel2") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.SetPanelCommand.CanExecute(nep))
                    NixxisGrid.SetPanelCommand.Execute(nep);
            }


        }

        void AdminFrameSet_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Title = m_MainWindowTitle;

            NixxisBaseExpandPanel nep = FindResource("AdminPanel") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                    NixxisGrid.RemovePanelCommand.Execute(nep);
            }

            nep = FindResource("DetailsPanel2") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                    NixxisGrid.RemovePanelCommand.Execute(nep);
            }

        }

        private void Init()
        {
            if (TheCore == null)
            {
                TheCore = new AdminCore(((IMainWindow)Application.Current.MainWindow).LoggedIn);

                List<XmlNode> corrections = new List<XmlNode>();

                for (int i = 0; i < 2; i++)
                {
                    corrections.Clear();

                    try
                    {
                        TheCore.Load();
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
                                correctionDlg.MessageText = Translate("Validation errors have been detected in current administration database. Do you want to correct them?");
                                if (correctionDlg.ShowDialog().GetValueOrDefault())
                                {
                                    XmlDocument saveResult = TheCore.Save(corrections);

                                    if (saveResult.DocumentElement.Name == "Success")
                                    {
                                        continue;
                                    }
                                    else
                                        Application.Current.Shutdown();
                                }
                                else
                                {
                                    dlg = new ConfirmationDialog();
                                    dlg.MessageText = string.Join("\n", lstMsg);

                                }                                                              
                            }
                            else
                            {
                                dlg = new ConfirmationDialog();
                                dlg.MessageText = string.Join("\n", lstMsg);
                            }
                        }

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
                }
            }
        }


        static internal AdminObject GetContextMenuObject(ContextMenu cm)
        {
            AdminObject toShow = null;
            if (cm != null)
            {
                object obj = NixxisDataGrid.GetClickedData(cm);

                if (obj == null)
                {
                    // It can be that we did not right click on a datagrid...
                    if (cm.PlacementTarget is ComboBox)
                    {
                        obj = ((ComboBox)(cm.PlacementTarget)).SelectedItem;
                    }
                    else if (cm.PlacementTarget is TreeView)
                    {
                        obj = NixxisTreeView.GetClickedData(cm);
                    }
                }

                if (obj is AdminObject)
                    toShow = (AdminObject)obj;
                else if (obj is BaseAdminCheckedLinkItem)
                    toShow = ((BaseAdminCheckedLinkItem)obj).Item;                
            }
            return toShow;
        }

        internal void ShowObjectWithContext(AdminObject obj, AdminObject context)
        {
            ShowObjectWithContext(obj, context, null);
        }

        internal bool CanShowObject(AdminObject obj)
        {
            if (obj == null)
                return false;

            while (obj.IsSystemWithValidOwner)
            {
                obj = obj.Owner;
            }

            if (obj is Agent)
            {
                return canShowCategory("agents");
            }
            else if (obj is Skill)
            {
                return canShowCategory("skills");
            }
            else if (obj is Language)
            {
                return canShowCategory("languages");
            }
            else if (obj is Team)
            {
                if (Settings.IsFullVersion)
                {
                    return canShowCategory("teams");
                }
            }
            else if (obj is Campaign)
            {
                return canShowCategory("campaigns");
            }
            else if (obj is Activity)
            {
                return canShowCategory("campaigns");
            }
            else if (obj is ObjectSecurity || obj is ObjectSecurityHelper)
            {
                return canShowCategory("objectsecurity");
            }
            else if (obj is AmdSettings)
            {
                if (Settings.IsFullVersion)
                {
                    return canShowCategory("amdsettings");
                }
                else
                {
                    return canShowCategory("globalsettings");
                }
            }
            else if (obj is AppointmentsContext)
            {
                return canShowCategory("appointments");
            }
            else if (obj is CallbackRuleset)
            {
                return canShowCategory("callbackrulesets");
            }
            else if (obj is Location)
            {
                if (Settings.IsFullVersion)
                {
                    return canShowCategory("locations");
                }
                else
                {
                    return canShowCategory("globalsettings");
                }
            }
            else if (obj is Pause)
            {
                return canShowCategory("pauses");
            }
            else if (obj is Role)
            {
                return canShowCategory("roles");
            }
            else if (obj is SecurityContext)
            {
                return canShowCategory("securitycontexts");
            }
            else if (obj is NumberingPlanEntry)
            {
                return canShowCategory("carriers");
            }
            else if (obj is Carrier)
            {
                return canShowCategory("carriers");
            }
            else if (obj is Phone)
            {
                return canShowCategory("phones");
            }
            else if (obj is Planning)
            {
                return canShowCategory("plannings");
            }
            else if (obj is SpecialDay)
            {
                return canShowCategory("plannings");
            }
            else if (obj is Preprocessor)
            {
                if (Settings.IsFullVersion)
                {
                    return canShowCategory("preprocessors");
                }
            }
            else if (obj is Prompt)
            {
                if (((Prompt)obj).Links[0].Link is Campaign)
                {
                    return canShowCategory("campaigns");
                }
                else if (((Prompt)obj).Links[0].Link is PromptRepository)
                {
                    return canShowCategory("prompts");
                }
                else if (((Prompt)obj).Links[0].Link is Activity)
                {
                    return canShowCategory("campaigns");
                }
                else if (((Prompt)obj).Links[0].Link is Queue)
                {
                    return canShowCategory("queues");
                }
            }
            else if (obj is Qualification)
            {
                return canShowCategory("campaigns");
            }
            else if (obj is FilterPart)
            {
                return canShowCategory("campaigns");
            }
            else if (obj is SortField)
            {
                return canShowCategory("campaigns");
            }
            else if (obj is Queue)
            {
                return canShowCategory("queues");
            }
            else if (obj is Phone)
            {
                return canShowCategory("phones");
            }
            else if (obj is Resource)
            {
                if (Settings.IsFullVersion)
                {
                    return canShowCategory("resources");
                }
                else
                {
                    return canShowCategory("globalsettings");
                }
            }
            return false;
        }

        internal void ShowObjectWithContext(AdminObject obj, AdminObject context, object ExtraInformation)
        {
            try
            {
                while (obj.IsSystemWithValidOwner)
                {
                    obj = obj.Owner;
                }

                if (obj is Agent)
                {
                    showCategory("agents");
                    Helpers.WaitForPriority();
                    agents.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is Skill)
                {
                    showCategory("skills");
                    Helpers.WaitForPriority();
                    skills.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is Language)
                {
                    showCategory("languages");
                    Helpers.WaitForPriority();
                    languages.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is Team)
                {
                    if (Settings.IsFullVersion)
                    {
                        showCategory("teams");
                        Helpers.WaitForPriority();
                        teams.SetSelectedWithContext(obj, context, ExtraInformation);
                    }
                }
                else if (obj is Campaign)
                {
                    showCategory("campaigns");
                    Helpers.WaitForPriority();
                    campaigns.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is Activity)
                {
                    showCategory("campaigns");
                    Helpers.WaitForPriority();
                    campaigns.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is AmdSettings)
                {
                    if (Settings.IsFullVersion)
                    {
                        showCategory("amdsettings");
                        Helpers.WaitForPriority();
                        amdsettings.SetSelectedWithContext(obj, context, ExtraInformation);
                    }
                    else
                    {
                        showCategory("globalsettings");
                        Helpers.WaitForPriority();
                        globalsettings.SetSelectedWithContext(obj, context, ExtraInformation);
                    }
                }
                else if (obj is AppointmentsContext)
                {
                    showCategory("appointments");
                    Helpers.WaitForPriority();
                    appointments.SetSelectedWithContext(obj, context, ExtraInformation);

                }
                else if (obj is CallbackRuleset)
                {
                    showCategory("callbackrulesets");
                    Helpers.WaitForPriority();
                    callbackrulesets.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is NumberingPlanEntry)
                {
                    if (context is Carrier)
                    {
                        //////
                        showCategory("campaigns");
                        Helpers.WaitForPriority();
                        campaigns.SetSelectedWithContext(((NumberingPlanEntry)obj).Activity.Target, context, ExtraInformation);

                    }
                    else
                    {
                        showCategory("carriers");
                        Helpers.WaitForPriority();
                        carriers.SetSelectedWithContext(obj, context, ExtraInformation);
                    }
                }
                else if (obj is Location)
                {
                    if (Settings.IsFullVersion)
                    {
                        showCategory("locations");
                        Helpers.WaitForPriority();
                        locations.SetSelectedWithContext(obj, context, ExtraInformation);
                    }
                    else
                    {
                        showCategory("globalsettings");
                        Helpers.WaitForPriority();
                        globalsettings.SetSelectedWithContext(obj, context, ExtraInformation);
                    }
                }
                else if (obj is Resource)
                {
                    if (Settings.IsFullVersion)
                    {
                        showCategory("resources");
                        Helpers.WaitForPriority();

                        resources.SetSelected(obj);
                    }
                    else
                    {
                        showCategory("globalsettings");
                        Helpers.WaitForPriority();
                        globalsettings.SetSelectedWithContext(obj, context, ExtraInformation);
                    }
                }
                else if (obj is Pause)
                {
                    showCategory("pauses");
                    Helpers.WaitForPriority();
                    pauses.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is Role)
                {
                    showCategory("roles");
                    Helpers.WaitForPriority();
                    roles.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is SecurityContext)
                {
                    showCategory("securitycontexts");
                    Helpers.WaitForPriority();
                    securitycontexts.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is ObjectSecurity || obj is ObjectSecurityHelper)
                {
                    if (context is Role)
                    {
                        AdminObject ao = null;
                        if (obj is ObjectSecurity)
                        {
                            ao = obj.Core.GetAdminObject(((ObjectSecurity)obj).SecuredAdminObjectId);
                        }
                        else
                        {
                            ao = obj.Core.GetAdminObject(((ObjectSecurityHelper)obj).SecuredAdminObjectId);
                        }
                        if (ao is Queue)
                        {
                            showCategory("queues");
                            Helpers.WaitForPriority();
                            queues.SetSelectedWithContext(ao, context, ExtraInformation);
                        }
                        else if (ao is Team)
                        {
                            showCategory("teams");
                            Helpers.WaitForPriority();
                            teams.SetSelectedWithContext(ao, context, ExtraInformation);
                        }
                        else if (ao is InboundActivity)
                        {
                            showCategory("campaigns");
                            Helpers.WaitForPriority();
                            campaigns.SetSelectedWithContext(ao, context, ExtraInformation);
                        }
                        else if (ao is OutboundActivity)
                        {
                            showCategory("campaigns");
                            Helpers.WaitForPriority();
                            campaigns.SetSelectedWithContext(ao, context, ExtraInformation);
                        }
                        else if (ao is Campaign)
                        {
                            showCategory("campaigns");
                            Helpers.WaitForPriority();
                            campaigns.SetSelectedWithContext(ao, context, ExtraInformation);
                        }
                        else if (ao is SecurityContext)
                        {
                            showCategory("securitycontexts");
                            Helpers.WaitForPriority();
                            securitycontexts.SetSelectedWithContext(ao, context, ExtraInformation);
                        }
                    }
                    else
                    {
                        showCategory("roles");
                        Helpers.WaitForPriority();
                        roles.SetSelectedWithContext(obj, context, ExtraInformation);
                    }
                }
                else if (obj is Carrier)
                {
                    showCategory("carriers");
                    Helpers.WaitForPriority();
                    carriers.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is Phone)
                {
                    showCategory("phones");
                    Helpers.WaitForPriority();
                    phones.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is Planning)
                {
                    showCategory("plannings");
                    Helpers.WaitForPriority();
                    plannings.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is SpecialDay)
                {
                    showCategory("plannings");
                    Helpers.WaitForPriority();
                    plannings.SetSelectedWithContext(obj, context, ExtraInformation);
                }
                else if (obj is Preprocessor)
                {
                    if (Settings.IsFullVersion)
                    {
                        showCategory("preprocessors");
                        Helpers.WaitForPriority();
                        preprocessors.SetSelectedWithContext(obj, context, ExtraInformation);
                    }
                }
                else if (obj is Prompt)
                {
                    if (((Prompt)obj).Links[0].Link is Campaign)
                    {
                        showCategory("campaigns");
                        Helpers.WaitForPriority();
                        campaigns.SetSelectedWithContext(((Prompt)obj).Links[0].Link, obj, ExtraInformation);
                    }
                    else if (((Prompt)obj).Links[0].Link is PromptRepository)
                    {
                        showCategory("prompts");
                        Helpers.WaitForPriority();
                        prompts.SetSelectedWithContext(obj, context, ExtraInformation);
                    }
                    else if (((Prompt)obj).Links[0].Link is Activity)
                    {
                        showCategory("campaigns");
                        Helpers.WaitForPriority();
                        campaigns.SetSelectedWithContext(((Prompt)obj).Links[0].Link, obj, ExtraInformation);
                    }
                    else if (((Prompt)obj).Links[0].Link is Queue)
                    {
                        showCategory("queues");
                        Helpers.WaitForPriority();
                        queues.SetSelectedWithContext(((Prompt)obj).Links[0].Link, obj, ExtraInformation);
                    }
                }
                else if (obj is Qualification)
                {
                    showCategory("campaigns");
                    Helpers.WaitForPriority();

                    campaigns.SetSelectedWithContext(((Activity)context).Campaign.Target, obj, ExtraInformation);
                }
                else if (obj is FilterPart)
                {
                    showCategory("campaigns");
                    Helpers.WaitForPriority();

                    campaigns.SetSelectedWithContext(((FilterPart)obj).Activity.Target.Campaign.Target, ((FilterPart)obj).Field.Target, ExtraInformation);
                }
                else if (obj is SortField)
                {
                    showCategory("campaigns");
                    Helpers.WaitForPriority();

                    campaigns.SetSelectedWithContext(((SortField)obj).Activity.Campaign.Target, ((SortField)obj).Field, ExtraInformation);

                }
                else if (obj is Queue)
                {
                    showCategory("queues");
                    Helpers.WaitForPriority();

                    queues.SetSelectedWithContext(obj, context, ExtraInformation);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        private bool canShowCategory(string category)
        {
            FrameworkElement fe = null;
            switch (category)
            {
                case "agents":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryAgents", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "skills":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategorySkills", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "languages":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryLanguages", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "teams":
                    fe = (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryTeams", true);                    

                    if (fe.Visibility != System.Windows.Visibility.Visible || (VisualTreeHelper.GetChild(fe,0) as FrameworkElement).Visibility != System.Windows.Visibility.Visible )
                        return false;
                    break;
                case "queues":
                    fe = (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryQueues", true);                    

                    if (fe.Visibility != System.Windows.Visibility.Visible || (VisualTreeHelper.GetChild(fe,0) as FrameworkElement).Visibility != System.Windows.Visibility.Visible )
                        return false;
                    break;
                case "campaigns":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryCampaigns", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "preprocessors":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPreprocessors", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "plannings":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPlannings", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "prompts":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPrompts", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "phones":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPhones", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "pauses":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPauses", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "roles":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryRoles", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "securitycontexts":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategorySecurityContexts", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "objectsecurity":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryRoles", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "carriers":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryCarriers", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "locations":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryLocations", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "resources":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryResources", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "callbackrulesets":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryCallbackRules", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "globalsettings":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryGlobalSettings", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "appointments":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryAppointments", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    break;
                case "amdsettings":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryAmdSettings", true).Visibility != System.Windows.Visibility.Visible)
                        return false;

                    break;
            }
            return true;
        }

        private bool showCategory(string category)
        {
            FrameworkElement fe = null;
            switch (category)
            {
                case "agents":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryAgents", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Users - "), m_MainWindowTitle);
                    break;
                case "skills":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategorySkills", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Skills - "), m_MainWindowTitle);
                    break;
                case "languages":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryLanguages", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Languages - "), m_MainWindowTitle);
                    break;
                case "teams":
                    fe = (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryTeams", true);                    

                    if (fe.Visibility != System.Windows.Visibility.Visible || (VisualTreeHelper.GetChild(fe,0) as FrameworkElement).Visibility != System.Windows.Visibility.Visible )
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Teams - "), m_MainWindowTitle);
                    break;
                case "queues":
                    fe = (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryQueues", true);                    

                    if (fe.Visibility != System.Windows.Visibility.Visible || (VisualTreeHelper.GetChild(fe,0) as FrameworkElement).Visibility != System.Windows.Visibility.Visible )
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Queues - "), m_MainWindowTitle);
                    break;
                case "campaigns":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryCampaigns", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Campaigns - "), m_MainWindowTitle);
                    break;
                case "preprocessors":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPreprocessors", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Preprocessors - "), m_MainWindowTitle);
                    break;
                case "plannings":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPlannings", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Plannings - "), m_MainWindowTitle);
                    break;
                case "prompts":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPrompts", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Prompts - "), m_MainWindowTitle);
                    break;
                case "phones":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPhones", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Phones - "), m_MainWindowTitle);
                    break;
                case "pauses":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPauses", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Pauses - "), m_MainWindowTitle);
                    break;
                case "roles":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryRoles", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Roles - "), m_MainWindowTitle);
                    break;
                case "securitycontexts":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategorySecurityContexts", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Security context - "), m_MainWindowTitle);
                    break;
                case "carriers":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryCarriers", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Carriers - "), m_MainWindowTitle);
                    break;
                case "locations":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryLocations", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Locations - "), m_MainWindowTitle);
                    break;
                case "resources":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryResources", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Resources - "), m_MainWindowTitle);
                    break;
                case "callbackrulesets":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryCallbackRules", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Callback rules - "), m_MainWindowTitle);
                    break;
                case "globalsettings":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryGlobalSettings", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Global settings - "), m_MainWindowTitle);
                    break;
                case "appointments":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryAppointments", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Appointments contexts - "), m_MainWindowTitle);
                    break;
                case "amdsettings":
                    if ((Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryAmdSettings", true).Visibility != System.Windows.Visibility.Visible)
                        return false;
                    Application.Current.MainWindow.Title = String.Concat(Translate("Answering machine detection settings - "), m_MainWindowTitle);
                    break;
            }

            AdminFrameSet.ShowCategory.State = category;
            Helpers.ApplyToChildren<UserControl>(this, VisibilityProperty, System.Windows.Visibility.Collapsed, (elm) => (!elm.Name.Equals(category) && "adminctrl".Equals(elm.Tag)));
            Helpers.ApplyToChildren<UserControl>(this, VisibilityProperty, System.Windows.Visibility.Visible, (elm) => (elm.Name.Equals(category) && "adminctrl".Equals(elm.Tag)));
            return true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == AdminFrameSet.ShowCategory)
            {
                showCategory(e.Parameter as string);
            }
            else if (e.Command == AdminFrameSet.SwitchCategory)
            {
                // TODO
                if (agents.Visibility == System.Windows.Visibility.Visible)
                {
                    agents.Visibility = System.Windows.Visibility.Collapsed;
                    skills.Visibility = System.Windows.Visibility.Visible;
                }
                else if (skills.Visibility == System.Windows.Visibility.Visible)
                {
                    agents.Visibility = System.Windows.Visibility.Visible;
                    skills.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    agents.Visibility = System.Windows.Visibility.Visible;
                    skills.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            else if (e.Command == AdminFrameSet.AffectToCampaign)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    object obj = NixxisDataGrid.GetClickedData(cm);

                    if (obj == null)
                    {
                        // It can be that we did not right click on a datagrid...
                        if (cm.PlacementTarget is ComboBox)
                        {
                            obj = ((ComboBox)(cm.PlacementTarget)).SelectedItem;
                        }
                    }

                    if (obj is PromptLink)
                    {
                        PromptLink pl = (PromptLink)obj;
                        if (pl.Link is Activity)
                        {
                            Activity act = ((Activity)(pl.Link));
                            Campaign camp = act.Campaign.Target;
                            camp.Prompts.Add(pl.Prompt);
                            act.Prompts.Remove(pl.Prompt);
                        }
                        else if (pl.Link is PromptRepository )
                        {
                            AdminObject[] references = pl.Prompt.GetReferences();
                            foreach (AdminObject o in references)
                            {
                                if (o is Activity)
                                {
                                    ((Activity)o).Campaign.Target.Prompts.Add(pl.Prompt);
                                }
                                else if (o is Campaign)
                                {
                                    ((Campaign)o).Prompts.Add(pl.Prompt);
                                }
                            }
                            pl.Core.GlobalPrompts.Prompts.Remove(pl.Prompt);
                        }
                    }
                }
            }
            else if (e.Command == AdminFrameSet.AffectToCore)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    object obj = NixxisDataGrid.GetClickedData(cm);

                    if (obj == null)
                    {
                        // It can be that we did not right click on a datagrid...
                        if (cm.PlacementTarget is ComboBox)
                        {
                            obj = ((ComboBox)(cm.PlacementTarget)).SelectedItem;
                        }
                    }


                    if (obj is PromptLink)
                    {
                        PromptLink pl = (PromptLink)obj;
                        if (pl.Link is Activity)
                        {
                            Activity act = ((Activity)(pl.Link));
                            act.Core.GlobalPrompts.Prompts.Add(pl.Prompt);
                            act.Prompts.Remove(pl.Prompt);
                        }
                        else if (pl.Link is Campaign)
                        {
                            Campaign camp = ((Campaign)(pl.Link));
                            camp.Core.GlobalPrompts.Prompts.Add(pl.Prompt);
                            camp.Prompts.Remove(pl.Prompt);
                        }
                        else if (pl.Link is Queue)
                        {
                            Queue queue = ((Queue)(pl.Link));
                            queue.Core.GlobalPrompts.Prompts.Add(pl.Prompt);
                            queue.Prompts.Remove(pl.Prompt);
                        }
                    }
                }
            }
            else if (e.Command == AdminFrameSet.AffectToActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    object obj = NixxisDataGrid.GetClickedData(cm);

                    if (obj == null)
                    {
                        // It can be that we did not right click on a datagrid...
                        if (cm.PlacementTarget is ComboBox)
                        {
                            obj = ((ComboBox)(cm.PlacementTarget)).SelectedItem;
                        }
                    }


                    if (obj is PromptLink)
                    {
                        PromptLink pl = (PromptLink)obj;
                        Campaign camp = ((Campaign)(pl.Link));

                        AdminObject[] references = pl.Prompt.GetReferences();
                        foreach (AdminObject o in references)
                        {
                            if (o is Activity)
                            {
                                (o as Activity).Prompts.Add(pl.Prompt);
                            }
                        }

                        camp.Prompts.Remove(pl.Prompt);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.PauseActivity)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Activity obj = NixxisDataGrid.GetClickedData(cm) as Activity;
                    if (obj != null)
                    {
                        obj.Paused = true;
                    }
                }
            }
            else if (e.Command == AdminFrameSet.UnpauseActivity)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Activity obj = NixxisDataGrid.GetClickedData(cm) as Activity;
                    if (obj != null)
                    {
                        obj.Paused = false;
                    }
                }
            }


            else if (e.Command == AdminFrameSet.PauseInboundActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        obj.PauseInbound(true);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.UnpauseInboundActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        obj.PauseInbound(false);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.PauseOutboundActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        obj.PauseOutbound(true);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.UnpauseOutboundActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        obj.PauseOutbound(false);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.PauseChatActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        obj.PauseChat(true);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.UnpauseChatActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        obj.PauseChat(false);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.PauseMailActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        obj.PauseMail(true);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.UnpauseMailActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        obj.PauseMail(false);
                    }
                }
            }
            else if (e.Command == AdminFrameSet.CommitChanges)
            {
                WaitScreen ws = new WaitScreen();
                ws.Owner = Application.Current.MainWindow;


                EnsureLastChangeIsApplied();

                agents.BackupSelection();
                skills.BackupSelection();
                languages.BackupSelection();
                teams.BackupSelection();
                queues.BackupSelection();
                campaigns.BackupSelection();
                preprocessors.BackupSelection();
                plannings.BackupSelection();
                prompts.BackupSelection();
                phones.BackupSelection();
                pauses.BackupSelection();
                carriers.BackupSelection();
                locations.BackupSelection();
                resources.BackupSelection();
                callbackrulesets.BackupSelection();
                globalsettings.BackupSelection();
                appointments.BackupSelection();
                amdsettings.BackupSelection();

                this.Visibility = System.Windows.Visibility.Hidden;

                ws.Show();


                try
                {
                    List<Tuple<string, string, string, string>> numberFormatUpdates = new List<Tuple<string, string, string, string>>();

                    foreach (Campaign c in TheCore.Campaigns)
                    {
                        if (c.NumberFormat != null && c.NumberFormat.OriginalTargetId!=null && c.NumberFormat.TargetId != c.NumberFormat.OriginalTargetId)
                        {
                            numberFormatUpdates.Add(new Tuple<string, string, string, string>(c.Id, c.SystemTable, c.NumberFormat.OriginalTargetId, c.NumberFormat.TargetId));
                        }
                    }


                    List<XmlNode> corrections = new List<XmlNode>();

                    for (int i = 0; i < 2; i++ )
                    {

                        XmlDocument saveResult = TheCore.Save(corrections);

                        if (saveResult.DocumentElement.Name == "Success")
                        {
                            TheCore.ClearDeleted();
                            TheCore.SaveAppliedSuccessfully();

                            foreach (Tuple<string, string, string, string> t in numberFormatUpdates)
                            {
                                ConfirmationDialog dlg = new ConfirmationDialog();
                                dlg.MessageText = string.Format(TranslationContext.Translate("Number format associated to campaign {0} has been changed from {1} to {2}.\nDo you want to convert the phone numbers in the database?"), TheCore.Campaigns[t.Item1].Description, TheCore.NumberFormats[t.Item3].Description, TheCore.NumberFormats[t.Item4].Description);
                                dlg.Owner = Application.Current.MainWindow;

                                if (dlg.ShowDialog().GetValueOrDefault())
                                {
                                    long affected = TheCore.UpdateNumbersFormat(
                                        t.Item1, t.Item2, t.Item3, t.Item4,
                                    ((a, b, c) => { ws.Progress = a; ws.Text = b; ws.ProgressDescription = c; Helpers.WaitForPriority(); })
                                    );

                                    ws.Progress = -1;
                                    ws.Text = string.Format(TranslationContext.Translate("{0} records affected"), affected);
                                    Helpers.WaitForPriority();
                                }
                                try
                                {
                                    dlg.Close();
                                }
                                catch
                                {
                                }

                            }

                            break;
                        }
                        else
                        {

                            ConfirmationDialog dlg = null; 

                            XmlNode nde = saveResult.SelectSingleNode(@"/Failure/Description");
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
                                foreach (XmlNode n in saveResult.SelectNodes(@"/Failure/ValidationErrors/ValidationError"))
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

                                if (allvalidationErrorsHaveCorrection && i<1)
                                {
                                    continue;
                                }
                                else
                                {
                                    dlg = new ConfirmationDialog();
                                    dlg.MessageText = string.Join("\n", lstMsg);
                                }
                            }

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

                            break;


                        }
                    }

                }
                catch (WebException webex)
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();

                    if(webex.Response!=null)
                    { 
                        using (Stream stream = webex.Response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(stream))
                            {
                                dlg.MessageText = sr.ReadToEnd();
                            }
                        }
                        // TODO: handle statuscode and display warning...
                        webex.Response.Close();
                    }
                    else
                    {
                        dlg.MessageText = webex.Message;
                    }


                    dlg.IsInfoDialog = true;
                    dlg.Owner = Application.Current.MainWindow;
                    dlg.ShowDialog();
                }

                try
                {
                    TheCore.Load();
                }
                catch (AdminValidationException ave)
                {
                    ConfirmationDialog dlg = new ConfirmationDialog();
                    dlg.IsInfoDialog = true;

                    XmlNode nde = ave.ValidationErrors.SelectSingleNode(@"/Failure/Description");
                    if (nde != null)
                    {
                        dlg.MessageText = nde.InnerText;
                    }
                    else
                    {
                        List<string> lstMsg = new List<string>();
                        foreach (XmlNode n in ave.ValidationErrors.SelectNodes(@"/Failure/ValidationErrors/ValidationError"))
                        {
                            if (lstMsg.Count() < 10)
                                lstMsg.Add(n.InnerText);
                        }
                        dlg.MessageText = string.Join("\n", lstMsg);
                    }

                    dlg.Owner = Application.Current.MainWindow;
                    dlg.ShowDialog();
                }


                    this.Visibility = System.Windows.Visibility.Visible;

                    ws.Close();

            }

        }


        private void EnsureLastChangeIsApplied()
        {
            foreach (UIElement element in containerGrid.Children)
            {
                if (element.IsVisible)
                {
                    element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    return;
                }
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == AdminFrameSet.ShowCategory)
            {
                FrameworkElement fe = null;
                switch (e.Parameter as string)
                {
                    case "agents":
                        e.CanExecute = (agents.Visibility != System.Windows.Visibility.Visible && 
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryAgents", true).Visibility== System.Windows.Visibility.Visible);
                        break;
                    case "skills":
                        e.CanExecute = (skills.Visibility != System.Windows.Visibility.Visible && 
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategorySkills", true).Visibility == System.Windows.Visibility.Visible);
                        break;
                    case "languages":
                        e.CanExecute = (languages.Visibility != System.Windows.Visibility.Visible && 
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryLanguages", true).Visibility == System.Windows.Visibility.Visible);
                        break;
                    case "teams":
                        fe = (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryTeams", true);                    

                        e.CanExecute = (teams.Visibility != System.Windows.Visibility.Visible) && Settings.IsFullVersion && 
                            fe.Visibility == System.Windows.Visibility.Visible && (VisualTreeHelper.GetChild(fe,0) as FrameworkElement).Visibility == System.Windows.Visibility.Visible ;
                        break;
                    case "queues":
                        fe = (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryQueues", true);                    

                        e.CanExecute = (queues.Visibility != System.Windows.Visibility.Visible) && Settings.IsFullVersion &&
                            fe.Visibility == System.Windows.Visibility.Visible && (VisualTreeHelper.GetChild(fe, 0) as FrameworkElement).Visibility == System.Windows.Visibility.Visible;
                        break;
                    case "campaigns":
                        e.CanExecute = (campaigns.Visibility != System.Windows.Visibility.Visible && 
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryCampaigns", true).Visibility == System.Windows.Visibility.Visible);
                        break;
                    case "preprocessors":
                        e.CanExecute = (preprocessors.Visibility != System.Windows.Visibility.Visible) && Settings.IsFullVersion && 
                            ( (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPreprocessors", true).Visibility== System.Windows.Visibility.Visible);;
                        break;
                    case "plannings":
                        e.CanExecute = (plannings.Visibility != System.Windows.Visibility.Visible && 
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPlannings", true).Visibility == System.Windows.Visibility.Visible);
                        break;
                    case "appointments":
                        e.CanExecute = (appointments.Visibility != System.Windows.Visibility.Visible && 
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryAppointments", true).Visibility == System.Windows.Visibility.Visible);
                        break;
                    case "phones":
                        e.CanExecute = (phones.Visibility != System.Windows.Visibility.Visible && 
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPhones", true).Visibility == System.Windows.Visibility.Visible);
                        break;
                    case "prompts":
                        e.CanExecute = (prompts.Visibility != System.Windows.Visibility.Visible && 
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPrompts", true).Visibility == System.Windows.Visibility.Visible);
                        break;
                    case "pauses":
                        e.CanExecute = (pauses.Visibility != System.Windows.Visibility.Visible && 
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryPauses", true).Visibility == System.Windows.Visibility.Visible);
                        break;
                    case "roles":
                        e.CanExecute = (roles.Visibility != System.Windows.Visibility.Visible &&
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryRoles", true).Visibility == System.Windows.Visibility.Visible);
                        break;
                    case "securitycontexts":
                        e.CanExecute = (securitycontexts.Visibility != System.Windows.Visibility.Visible &&
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategorySecurityContexts", true).Visibility == System.Windows.Visibility.Visible);
                        break;
                    case "carriers":
                        e.CanExecute = (carriers.Visibility != System.Windows.Visibility.Visible &&
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryCarriers", true).Visibility == System.Windows.Visibility.Visible);
                        break;
                    case "resources":
                        e.CanExecute = (resources.Visibility != System.Windows.Visibility.Visible) && Settings.IsFullVersion && 
                            ( (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryResources", true).Visibility== System.Windows.Visibility.Visible);
                        break;
                    case "callbackrulesets":
                        e.CanExecute = (callbackrulesets.Visibility != System.Windows.Visibility.Visible) && Settings.IsFullVersion && 
                            ( (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryCallbackRules", true).Visibility== System.Windows.Visibility.Visible);
                        break;
                    case "locations":
                        e.CanExecute = (locations.Visibility != System.Windows.Visibility.Visible) && Settings.IsFullVersion && 
                            ( (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryLocations", true).Visibility== System.Windows.Visibility.Visible);
                        break;
                    case "amdsettings":
                        e.CanExecute = (amdsettings.Visibility != System.Windows.Visibility.Visible) && Settings.IsFullVersion && 
                            ( (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryAmdSettings", true).Visibility== System.Windows.Visibility.Visible);
                        break;
                    case "globalsettings":
                        e.CanExecute = (globalsettings.Visibility != System.Windows.Visibility.Visible &&
                            (Resources["AdminPanel"] as Nixxis.Client.Controls.NixxisExpandPanel).FindByName("CategoryGlobalSettings", true).Visibility == System.Windows.Visibility.Visible);
                        break;

                    default:
                        e.CanExecute = false;
                        break;
                }

                e.Handled = true;
            }
            else if (e.Command == AdminFrameSet.SwitchCategory)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
            else if (e.Command == AdminFrameSet.AffectToCampaign)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    object obj = NixxisDataGrid.GetClickedData(cm);

                    if (obj == null)
                    {
                        // It can be that we did not right click on a datagrid...
                        if (cm.PlacementTarget is ComboBox)
                        {
                            obj = ((ComboBox)(cm.PlacementTarget)).SelectedItem;
                        }
                    }


                    if (obj is PromptLink)
                        e.CanExecute = true;
                }
                else
                    e.CanExecute = false;

                e.Handled = true;
            }
            else if (e.Command == AdminFrameSet.AffectToCore)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    object obj = NixxisDataGrid.GetClickedData(cm);

                    if (obj == null)
                    {
                        // It can be that we did not right click on a datagrid...
                        if (cm.PlacementTarget is ComboBox)
                        {
                            obj = ((ComboBox)(cm.PlacementTarget)).SelectedItem;
                        }
                    }


                    if (obj is PromptLink)
                        e.CanExecute = true;
                }
                else
                    e.CanExecute = false;

                e.Handled = true;
            }
            else if (e.Command == AdminFrameSet.AffectToActivities)
            {

                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    object obj = NixxisDataGrid.GetClickedData(cm);

                    if (obj == null)
                    {
                        // It can be that we did not right click on a datagrid...
                        if (cm.PlacementTarget is ComboBox)
                        {
                            obj = ((ComboBox)(cm.PlacementTarget)).SelectedItem;
                        }
                    }


                    if (obj is PromptLink)
                        e.CanExecute = true;
                }
                else
                    e.CanExecute = false;
                e.Handled = true;
            }
            else if (e.Command == AdminFrameSet.PauseActivity)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Activity obj = NixxisDataGrid.GetClickedData(cm) as Activity;
                    if (obj != null)
                    {
                        e.CanExecute = !obj.Paused && !obj.IsReadOnly;
                        e.Handled = true;
                    }
                }
            }
            else if (e.Command == AdminFrameSet.UnpauseActivity)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Activity obj = NixxisDataGrid.GetClickedData(cm) as Activity;
                    if (obj != null)
                    {
                        e.CanExecute = obj.Paused && !obj.IsReadOnly;
                        e.Handled = true;
                    }
                }
            }


            else if (e.Command == AdminFrameSet.PauseInboundActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        e.CanExecute = (obj.InboundStatus != CampaignStatus.Paused && obj.InboundStatus != CampaignStatus.NotActive );
                        e.Handled = true;
                    }
                }
            }
            else if (e.Command == AdminFrameSet.UnpauseInboundActivities    )
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        e.CanExecute = (obj.InboundStatus != CampaignStatus.Running && obj.InboundStatus != CampaignStatus.NotActive);
                        e.Handled = true;
                    }
                }
            }
            else if (e.Command == AdminFrameSet.PauseOutboundActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        e.CanExecute = (obj.OutboundStatus != CampaignStatus.Paused && obj.OutboundStatus != CampaignStatus.NotActive );
                        e.Handled = true;
                    }
                }
            }
            else if (e.Command == AdminFrameSet.UnpauseOutboundActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        e.CanExecute = (obj.OutboundStatus != CampaignStatus.Running && obj.OutboundStatus != CampaignStatus.NotActive);
                        e.Handled = true;
                    }
                }
            }

            else if (e.Command == AdminFrameSet.PauseChatActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        e.CanExecute = (obj.ChatStatus != CampaignStatus.Paused && obj.ChatStatus !=  CampaignStatus.NotActive);
                        e.Handled = true;
                    }
                }
            }
            else if (e.Command == AdminFrameSet.UnpauseChatActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        e.CanExecute = (obj.ChatStatus != CampaignStatus.Running && obj.ChatStatus !=  CampaignStatus.NotActive);
                        e.Handled = true;
                    }
                }
            }
            else if (e.Command == AdminFrameSet.PauseMailActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        e.CanExecute = (obj.MailStatus != CampaignStatus.Paused && obj.MailStatus != CampaignStatus.NotActive );
                        e.Handled = true;
                    }
                }
            }
            else if (e.Command == AdminFrameSet.UnpauseMailActivities)
            {
                ContextMenu cm = e.Parameter as ContextMenu;

                if (cm != null)
                {
                    Campaign obj = NixxisDataGrid.GetClickedData(cm) as Campaign;
                    if (obj != null)
                    {
                        e.CanExecute = (obj.MailStatus != CampaignStatus.Running && obj.MailStatus != CampaignStatus.NotActive );
                        e.Handled = true;
                    }
                }
            }
            else if (e.Command == AdminFrameSet.CommitChanges)
            {
                e.CanExecute = true;
                e.Handled = true;
            }

        }

        private void FirePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NixxisButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                Settings.IsDebug = !Settings.IsDebug;
            }
        }

        public bool NeedSave()
        {
            EnsureLastChangeIsApplied();
            XmlDocument tempDoc = new XmlDocument();            
            TheCore.Save(tempDoc);
            return (tempDoc.DocumentElement.ChildNodes.Count > 0);
        }

        public void ClearReservations()
        {
            TheCore.ClearReservations(TheCore.GetSessionId((Application.Current.MainWindow as IMainWindow).LoggedIn));
        }

        private void NixxisButton_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            NixxisPriorityPanel npp = Helpers.FindVisualParent<NixxisPriorityPanel>(sender as UIElement);
            npp.InvalidateArrange();
        }


        public static readonly DependencyProperty ProgressDescriptionProperty = DependencyProperty.Register("ProgressDescription", typeof(string), typeof(AdminFrameSet));

        public string ProgressDescription
        {
            get
            {
                return (string)GetValue(ProgressDescriptionProperty);
            }
            set
            {
                SetValue(ProgressDescriptionProperty, value);
            }
        }

        public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register("ProgressValue", typeof(int), typeof(AdminFrameSet));

        public int ProgressValue
        {
            get
            {
                return (int)GetValue(ProgressValueProperty);
            }
            set
            {
                SetValue(ProgressValueProperty, value);
            }
        }

        public static readonly DependencyProperty ProgressVisibilityProperty = DependencyProperty.Register("ProgressVisibility", typeof(Visibility), typeof(AdminFrameSet), new PropertyMetadata(Visibility.Collapsed));

        public Visibility ProgressVisibility
        {
            get
            {
                return (Visibility)GetValue(ProgressVisibilityProperty);
            }
            set
            {
                SetValue(ProgressVisibilityProperty, value);
            }
        }

        private void SaveDemo()
        {
            if (Nixxis.Client.Admin.AdminFrameSet.CommitChanges.CanExecute(null, ((IMainWindow)Application.Current.MainWindow).AdminFrameSet))
            {
                Nixxis.Client.Admin.AdminFrameSet.CommitChanges.Execute(null, ((IMainWindow)Application.Current.MainWindow).AdminFrameSet);
            }
        }

        private void StartButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                DlgAddDemo dlg = new DlgAddDemo();
                dlg.DataContext = TheCore;
                dlg.Owner = Application.Current.MainWindow;
                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    string Indest = null;
                    if (dlg.cboDestination.SelectedIndex > -1)
                        TheCore.CreateDemoEntries(dlg.txtDescription.Text, dlg.cboDestination.SelectedItem as NumberingPlanEntry, dlg.txtOutbound.Text, SaveDemo, DisplayWarning);
                    else
                        TheCore.CreateDemoEntries(dlg.txtDescription.Text, dlg.cboDestination.Text, dlg.txtOutbound.Text, SaveDemo, DisplayWarning);                  
                }
            }
        }

        private void DisplayWarning(string text)
        {
            ConfirmationDialog dlg = new ConfirmationDialog();
            dlg.MessageText = text;
            dlg.Owner = Application.Current.MainWindow;
            dlg.IsInfoDialog = true;
            dlg.ShowDialog();
        }

    }


    public class AdminObjectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            if (((AdminObject)value).IsSystemWithValidOwner && !(value is Activity) )
            {
                return Convert(((AdminObject)value).Owner, targetType, parameter, culture);
            }

            if (value is Agent)
            {
                return @"Images\AdminSmall_Agent.png";
            }
            if (value is Carrier)
            {
                return @"Images\AdminSmall_Carrier.png";
            }
            else if (value is Skill)
            {
                return @"Images\AdminSmall_Skill.png";
            }
            else if (value is Queue)
            {
                return @"Images\AdminSmall_Queue.png";
            }
            else if (value is Team)
            {
                return @"Images\AdminSmall_Team.png";
            }
            else if (value is Preprocessor)
            {
                return @"Images\AdminSmall_Preprocessor.png";
            }
            else if (value is Campaign)
            {
                return @"Images\AdminSmall_Campaign.png";
            }
            else if (value is Phone)
            {
                return @"Images\AdminSmall_Phone.png";
            }
            else if (value is Location)
            {
                return @"Images\AdminSmall_Location.png";
            }
            else if (value is CallbackRuleset)
            {
                return @"Images\AdminSmall_CallbackRule.png";
            }
            else if (value is Resource)
            {
                return @"Images\AdminSmall_Resource.png";
            }
            else if (value is AmdSettings)
            {
                return @"Images\AdminSmall_AmdSettings.png";
            }
            else if (value is Language)
            {
                return @"Images\AdminSmall_Language.png";
            }
            else if (value is Right || value is Role || value is ObjectSecurity )
            {
                return @"Images\AdminSmall_Rights.png";
            }
            else if (value is SecurityContext || value is AdminObjectSecurityContext)
            {
                return @"Images\AdminSmall_Security.png";
            }


            else if (value is InboundActivity)
            {
                InboundActivity ia = (InboundActivity)value;
                if (ia.MediaType == MediaType.Voice)
                {
                    return @"Images\Activities_Inbound.png";
                }
                else if (ia.MediaType == MediaType.Chat)
                {
                    return @"Images\Activities_Chat.png";
                }
                else if (ia.MediaType == MediaType.Mail)
                {
                    return @"Images\Activities_Mail.png";
                }
            }
            else if (value is OutboundActivity)
            {
                OutboundActivity oa = (OutboundActivity)value;
                if (oa.OutboundMode == DialingMode.Search)
                {
                    return @"Images\Activities_Search.png";
                }
                else
                {
                    return @"Images\Activities_Outbound.png";
                }
            }
            else if (value is Field)
            {
                return @"Images\AdminSmall_Field.png";
            }
            else if (value is FilterPart)
            {
                FilterPart fp = value as FilterPart;
                if (fp.Aggregator != Aggregator.None)
                    return @"Images\AdminSmall_AggregateFilter.png";
                else
                    return @"Images\AdminSmall_Filter.png";
            }
            else if (value is SortField)
            {
                SortField sf = value as SortField;
                if (sf.Aggregator != Aggregator.None)
                    return @"Images\AdminSmall_AggregateSortOrder.png";
                else
                    return @"Images\AdminSmall_SortOrder.png";
            }
            else if (value is SecurityContext)
            {
                return @"Images\AdminSmall_Security.png";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RightsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AdminObject aobj = value as AdminObject;

            if (targetType == typeof(Visibility))
            {
                if (aobj != null)
                {
                    if (aobj.CheckEnabled(parameter as string))
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
                return Visibility.Collapsed;
            }
            else
            {
                if (aobj != null)
                {
                    return aobj.CheckEnabled(parameter as string);
                }
                return false;

            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class InvertedRightsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AdminObject aobj = value as AdminObject;

            if (targetType==typeof(Visibility))
            {
                if (aobj != null)
                {
                    if (aobj.CheckEnabled(parameter as string))
                        return Visibility.Collapsed;
                    else
                        return Visibility.Visible;
                }
                return Visibility.Visible;
            }
            else
            {
                if (aobj != null)
                {
                    return !aobj.CheckEnabled(parameter as string);
                }
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
