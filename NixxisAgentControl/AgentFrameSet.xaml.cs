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
using System.Collections.ObjectModel;
using ContactRoute.Recording.Config;
using System.Windows.Threading;
using ContactRoute;
using System.ComponentModel;
using System.Threading;

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for AgentFrameSet.xaml
    /// </summary>
    public partial class AgentFrameSet : UserControl
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);


        private static TranslationContext m_TranslationContext = new TranslationContext("AgentFrameSet");

        private static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        #region Static members
        public static TranslationContext TranslationContext = new TranslationContext("AgentFrameSet");
        public static RoutedUICommand DoNewCall { get; private set; }
        public static RoutedUICommand SetQualification { get; private set; }
        public static RoutedUICommand SearchMode { get; private set; }
        public static RoutedUICommand SendMessage { get; private set; }
        public static RoutedCommand CloseContact { get; private set; }
        public static RoutedCommand TeamSelection { get; private set; }
        public static RoutedCommand DisplayContactHistory { get; private set; }

        static AgentFrameSet()
        {
            DoNewCall = new RoutedUICommand(string.Empty, "DoNewCall", typeof(AgentFrameSet));
            SetQualification = new RoutedUICommand(string.Empty, "SetQualification", typeof(AgentFrameSet), new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.Q, ModifierKeys.Control) }));
            SearchMode = new RoutedUICommand(string.Empty, "SearchMode", typeof(AgentFrameSet), new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.S, ModifierKeys.Control) }));
            SendMessage = new RoutedUICommand(string.Empty, "SendMessage", typeof(AgentFrameSet), new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.M, ModifierKeys.Control) }));
            CloseContact = new RoutedCommand("CloseContact", typeof(AgentFrameSet), new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.X, ModifierKeys.Control) }));
            TeamSelection = new RoutedCommand("TeamSelection", typeof(AgentFrameSet), new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.T, ModifierKeys.Control) }));
            DisplayContactHistory = new RoutedCommand("DisplayContactHistory", typeof(AgentFrameSet), new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.H, ModifierKeys.Control) }));
        }
        #endregion

        #region Class data
        private string m_Version = string.Empty;
        private Control m_WaitPanel;
        private Control m_PausePanel;
        private Control m_LoginPanel;
        private bool m_HandleTeamSelection = false;
        ISession m_Session;
        HttpLink m_ClientLink;       
        InternalCommand CloseContactCommand;
        private AgentWarningMessageCollection m_AgtWarningMsg = new AgentWarningMessageCollection();
        private NixxisUserChatCollection m_UserChatCollection = new NixxisUserChatCollection();
        private NixxisUserChatWindow m_UserChatWindow = null;
        private Profile m_Profile;
        private string m_ConfigfileName = "RecordingProfile.config";
        private ContactHistoryItem m_SelectedHistory = null;
        private string m_LastAgentState = Translate(Properties.Resources.AgentStatePause);
        private DateTime m_LastAgentStateChange = DateTime.Now;

        #endregion



        public AgentFrameSet(ISession session, HttpLink clientLink, bool allowTeamSelection, string version, string cfgLocation)
        {
            this.Version = version;
            m_HandleTeamSelection = allowTeamSelection;
            m_Session = session;
            m_ClientLink = clientLink;

            m_Profile = new Profile();
            m_Profile.Location = cfgLocation;
            m_Profile.ConfigFile = m_ConfigfileName;
            m_Profile.ConfigMode = ContactRoute.Config.ConfigMode.System;
            m_Profile.Load();

            IsVisibleChanged += new DependencyPropertyChangedEventHandler(AgentFrameSet_IsVisibleChanged);

            this.Loaded += new RoutedEventHandler(AgentFrameSet_Loaded);
            Unloaded += new RoutedEventHandler(AgtFrameSet_Unloaded);

            m_ClientLink.ClientState.SetLastAgentState(m_LastAgentState, m_LastAgentStateChange);

            m_ClientLink.ContactAdded += new HttpLinkContactEventDelegate(ClientLink_ContactAdded);
            m_ClientLink.ContactRemoved += new HttpLinkContactEventDelegate(ClientLink_ContactRemoved);
            m_ClientLink.ContactStateChanged += new HttpLinkContactEventDelegate(ClientLink_ContactStateChanged);
            m_ClientLink.AgentTeamsChanged += new HttpLinkEventDelegate(ClientLink_AgentTeamsChanged);
            m_ClientLink.AgentWarning += new HttpLinkServerEventDelegate(m_ClientLink_AgentWarning);
            m_ClientLink.PauseForced += new HttpLinkServerEventDelegate(m_ClientLink_PauseForced);
            m_ClientLink.AgentMessage += new HttpLinkServerEventDelegate(m_ClientLink_AgentMessage);
            m_ClientLink.ViewServerOperation += m_ClientLink_ViewServerOperation;
            m_ClientLink.Contacts.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Contacts_PropertyChanged);
            m_ClientLink.Commands.WaitForCall.StateChanged += new CommandStateChangedDelegate(Wait_StateChanged);
            m_ClientLink.Commands.WaitForChat.StateChanged += new CommandStateChangedDelegate(Wait_StateChanged);
            m_ClientLink.Commands.WaitForMail.StateChanged += new CommandStateChangedDelegate(Wait_StateChanged);

            CloseContactCommand = new InternalCommand(m_ClientLink, (cmd, param) => cmd.Link.Commands.TerminateContact.Execute(cmd.Link.Contacts.ActiveContactId));
            CloseContactCommand.LinkedCommand = m_ClientLink.Commands.TerminateContact;

            InitializeComponent();

            ShowNoContactPanel();
        }

        private IViewServerAddon m_ViewServer = null;
        private Thread m_worker = null;
        private int m_StartCount = 0;

        void m_ClientLink_ViewServerOperation(ServerEventArgs eventArgs)
        {
            string param = eventArgs.Parameters;
            
            bool start = (param[0]=='1');
            string[] parts = param.Substring(2).Split(new string[] { ",[", "],[", "]" }, 4, StringSplitOptions.RemoveEmptyEntries);
            string strSupervisor = parts[0];
            
            
            
            if(start)            
            {
                // start vnnc server
                if (m_StartCount == 0)
                {
                    string[] ipAddresses = parts[1].Split(';').Select((a) => Microsoft.JScript.GlobalObject.unescape(a)).ToArray();
                    string[] addons = parts[2].Split(';').Select((a) => Microsoft.JScript.GlobalObject.unescape(a)).ToArray();


                    foreach(string strAddon in addons)
                    {
                        try
                        {
                            Type tpe = Type.GetType(strAddon);
                            m_ViewServer = Activator.CreateInstance(tpe) as IViewServerAddon;
                            break;
                        }
                        catch(Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine(ex);
                        }

                    }

                    if (m_ViewServer != null)
                    {
                        m_worker = new Thread(Start);

                        m_worker.IsBackground = true;

                        m_worker.Name = "ViewServer";

                        m_worker.Start(m_ViewServer);
                    }
                    else
                    {
                        return;
                    }
                }
                

                m_StartCount++;

                List<string> parameters = new List<string>();
                foreach (System.Net.NetworkInformation.NetworkInterface Nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (Nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && Nic.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                    {
                        foreach (System.Net.NetworkInformation.UnicastIPAddressInformation Addr in Nic.GetIPProperties().UnicastAddresses)
                        {
                            if (Addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                parameters.Add( string.Concat(Addr.Address.ToString(), "/", Addr.IPv4Mask.ToString()));
                            }
                        }
                    }
                }

                parameters.Insert(0, m_ViewServer.GetType().AssemblyQualifiedName);
                parameters.Insert(0, strSupervisor);

                m_ClientLink.WatchMe(parameters.ToArray());

            }
            else
            {
                if (m_StartCount > 0)
                {
                    m_StartCount--;
                    // stop server
                    if (m_StartCount == 0)
                    {

                        if (m_ViewServer != null)
                        {
                            try
                            {
                                m_ViewServer.Stop();
                            }
                            catch
                            {

                            }

                            m_ViewServer = null;
                        }

                        if (m_worker != null)
                        {
                            m_worker.Abort();

                            m_worker = null;
                        }
                    }
                }
            }

        }

        void Start(object obj)
        {
            IViewServerAddon v = obj as IViewServerAddon;

            v.Start();
        }




        #region Properties
        public string Version { get; set; }
        public AgentWarningMessageCollection AgtWarningMsg
        {
            get
            {
                return m_AgtWarningMsg;
            }
        }
        public Profile RecordingConfig
        {
            get
            {
                return m_Profile;
            }
        }
        
        public static readonly DependencyProperty SelectedContactHistoryProperty = DependencyProperty.Register("SelectedContactHistory", typeof(ContactHistoryItem), typeof(AgentFrameSet));
        public ContactHistoryItem SelectedContactHistory
        {
            get { return (ContactHistoryItem)GetValue(SelectedContactHistoryProperty); }
            set { SetValue(SelectedContactHistoryProperty, value); }
        }
    
        #endregion

        #region Messages and chat
        private void m_ClientLink_AgentMessage(ServerEventArgs eventArgs)
        {
            string[] list = eventArgs.Parameters.Split(new char[] { ',' });

            if ((MessageType)Enum.Parse(typeof(MessageType), list[0]) == MessageType.Chat)
            {
                //Chat message
                m_UserChatCollection.AddMessage(Microsoft.JScript.GlobalObject.unescape(list[5]), list[1], Microsoft.JScript.GlobalObject.unescape(list[2]), NixxisUserChatLine.ChatLineOriginators.Customer);
                ShowUserChatWindow();
            }
            else if ((MessageType)Enum.Parse(typeof(MessageType), list[0]) == MessageType.ChatEnd)
            {
                //Chat system message
                m_UserChatCollection.AddMessage(Microsoft.JScript.GlobalObject.unescape(list[5]), list[1], Microsoft.JScript.GlobalObject.unescape(list[2]), NixxisUserChatLine.ChatLineOriginators.System, NixxisUserChatLine.ChatEventType.Leave);
            }
            else
            {
                //Other message: will be processed as it where agtWarnings
                m_AgtWarningMsg.AddMessage("[" + Microsoft.JScript.GlobalObject.unescape(list[2]) + "] " + Microsoft.JScript.GlobalObject.unescape(list[5]));
            }
        }
        private void m_ClientLink_PauseForced(ServerEventArgs eventArgs)
        {
            string description = string.Empty;
            string failureCode = string.Empty;

            try
            {
                string[] prms = eventArgs.Parameters.Split(',');
                description = Microsoft.JScript.GlobalObject.unescape(prms[0]);
                failureCode = prms[1];
            }
            catch
            {
            }

            
            string[] failureReasons = null;

            if(m_ClientLink.ClientSettings["ForcedPauseDialog"]!=null)
                failureReasons = m_ClientLink.ClientSettings["ForcedPauseDialog"].Split(',');

            if(failureReasons!=null && failureReasons.Contains(failureCode))
            {
                ConfirmationDialog dlg = new ConfirmationDialog();

                string strMessage = string.Concat("ForcePauseDialog", failureCode);
                string strTranslatedMessage = Translate(strMessage);
                if(strTranslatedMessage==strMessage)
                    dlg.MessageText = Translate("You were forced to pause because your phone was unavailable.\nWould you like to go back in ready?");
                else
                    dlg.MessageText = strTranslatedMessage;
                dlg.Owner = Application.Current.MainWindow;
                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    m_ClientLink.Commands.WaitForCall.Execute();
                }
            }
            else
                m_AgtWarningMsg.AddMessage(description);
        }

        private void m_ClientLink_AgentWarning(ServerEventArgs eventArgs)
        {
            m_AgtWarningMsg.AddMessage(eventArgs.Parameters);
        }
        public void UserChatWindow_SendMessage(string msg, string userid)
        {
            ClientLink.SendMessage(MessageType.Chat, MessageDestinations.Agent, userid, msg);
        }
        public void UserChatWindow_EndConversation(string msg, string userid)
        {
            ClientLink.SendMessage(MessageType.ChatEnd, MessageDestinations.Agent, userid, msg);
        }
        private void ShowUserChatWindow()
        {
            if (m_UserChatWindow == null)
            {
                m_UserChatWindow = CreateWindow();
                m_UserChatWindow.Show();
            }
            else if (m_UserChatWindow.IsLoaded && !m_UserChatWindow.IsActive)
            {
                m_UserChatWindow.Activate();
            }
            else if (!m_UserChatWindow.IsLoaded)
            {
                m_UserChatWindow.SendMessage -= new NixxisUserChatSendMsgHandler(UserChatWindow_SendMessage);
                m_UserChatWindow.EndConversation -= new NixxisUserChatSendMsgHandler(UserChatWindow_EndConversation);
                m_UserChatWindow = null;

                m_UserChatWindow = CreateWindow();
                m_UserChatWindow.Show();
            }
        }
        private NixxisUserChatWindow CreateWindow()
        {
            NixxisUserChatWindow userChatWindow = new NixxisUserChatWindow();

            userChatWindow.AgentImageSource = new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/Chat_Agent.png"));
            userChatWindow.CustomerImageSource = new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/Chat_customer.png"));
            userChatWindow.RemoveImageSource = new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/Remove.png"));
            userChatWindow.ItemSource = m_UserChatCollection;
            userChatWindow.Mode = NixxisUserChatWindow.Modes.Agent;
            userChatWindow.SendMessage += new NixxisUserChatSendMsgHandler(UserChatWindow_SendMessage);
            userChatWindow.EndConversation += new NixxisUserChatSendMsgHandler(UserChatWindow_EndConversation);

            return userChatWindow;
        }
        #endregion


        void ClientLink_AgentTeamsChanged()
        {
            m_HandleTeamSelection = true;
        }

        void RefreshLastAgentState()
        {
            string NewAgentState = m_LastAgentState;

            if (ClientLink.Contacts.Count > 0)
            {
                NewAgentState =  Translate(Properties.Resources.AgentStateOnline);
            }
            else
            {
                if (ClientLink.Commands.WaitForCall.Active || ClientLink.Commands.WaitForChat.Active || ClientLink.Commands.WaitForMail.Active)
                {
                    NewAgentState = Translate(Properties.Resources.AgentStateWaiting) + " (";

                    if (ClientLink.Commands.WaitForCall.Active)
                        NewAgentState = NewAgentState + "V";
                    if (ClientLink.Commands.WaitForChat.Active)
                        NewAgentState = NewAgentState + "C";
                    if (ClientLink.Commands.WaitForMail.Active)
                        NewAgentState = NewAgentState + "M";

                    NewAgentState = NewAgentState + ")";
                }
                else
                {
                    NewAgentState = Translate(Properties.Resources.AgentStatePause);
                }
            }

            if (NewAgentState != m_LastAgentState)
            {
                if(!m_LastAgentState.StartsWith(NewAgentState))
                    m_LastAgentState = NewAgentState;

                m_LastAgentStateChange = DateTime.Now;

                ClientLink.ClientState.SetLastAgentState(m_LastAgentState, m_LastAgentStateChange);
            }
        }

        void Wait_StateChanged(ICommand command)
        {
            RefreshLastAgentState();
            ShowNoContactPanel();
        }

        void ClientLink_ContactStateChanged(ContactEventArgs eventArgs)
        {
            if (eventArgs.Contact.Media == ContactMedia.Mail && eventArgs.Contact.State == 'D')
            {
                ((IMediaPanel)eventArgs.Contact.UserData).IsReadOnly = true;
            }
        }

        void ClientLink_ContactAdded(ContactEventArgs eventArgs)
        {
            m_ClientLink.Commands.StartScript.Execute(eventArgs.ContactId);

            //When there is no current active contact or this is a voice contact, put this contact as active contact
            if (string.IsNullOrEmpty(m_ClientLink.Contacts.ActiveContactId) || eventArgs.Contact.Media == ContactMedia.Voice)
                m_ClientLink.Contacts.ActiveContact = eventArgs.Contact;

            UIElement contactPanel;
            switch (eventArgs.Contact.Media)
            {
                case ContactMedia.Chat:
                    contactPanel = new ChatPanelControl(ClientLink, (ContactInfoChat)eventArgs.Contact);
                    break;
                case ContactMedia.Mail:
                    if (string.IsNullOrEmpty(eventArgs.Contact.ContentHandler))
                        contactPanel = new MailPanelControl(ClientLink, (ContactInfoMail)eventArgs.Contact);
                    else
                        contactPanel = (UserControl)(Activator.CreateInstance(Type.GetType(eventArgs.Contact.ContentHandler), new object[] { ClientLink, (ContactInfoMail)eventArgs.Contact }));
                    break;
                case ContactMedia.Voice:
                case ContactMedia.Unknown:
                default:
                    contactPanel = new VoicePanelControl(ClientLink, eventArgs.Contact);
                    break;
            }

            eventArgs.Contact.UserData = contactPanel;

            RefreshLastAgentState();
            ShowContactPanel();

            try
            {
                if (m_ClientLink.ClientSettings["OnContactAdded"].Equals("foreground", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Application.Current.MainWindow.IsVisible)
                    {
                        Application.Current.MainWindow.Show();
                    }

                    if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                    {
                        Application.Current.MainWindow.WindowState = WindowState.Normal;
                    }

                    Application.Current.MainWindow.Activate();

                    if (!Application.Current.MainWindow.Topmost)
                    {
                        Application.Current.MainWindow.Topmost = true;
                        Application.Current.MainWindow.Topmost = false;
                    }

                    Application.Current.MainWindow.Focus();                
                }
                else if (m_ClientLink.ClientSettings["OnContactAdded"].Equals("flash", StringComparison.OrdinalIgnoreCase))
                {
                    FlashWindow(new System.Windows.Interop.WindowInteropHelper(Application.Current.MainWindow).Handle, false);
                }
            }
            catch
            {
            }
        }

        void ClientLink_ContactRemoved(ContactEventArgs eventArgs)
        {
            RefreshLastAgentState();

            ShowContactPanel();

            if (SelectedContactHistory != null && eventArgs.ContactId == SelectedContactHistory.CurrentContactId)
                SelectedContactHistory = null;
        }

        void Contacts_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("ActiveContact", StringComparison.OrdinalIgnoreCase))
            {
                ShowContactPanel();
            }
        }

        public HttpLink ClientLink
        {
            get
            {
                return m_ClientLink;
            }
        }

        void ShowContactPanel()
        {
            NixxisBaseExpandPanel ContactCommands = null;
            ContactInfo CInfo = m_ClientLink.Contacts.ActiveContact;

            if (CInfo != null)
            {
                switch (CInfo.Media)
                {
                    case ContactMedia.Voice:
                        ContactCommands = FindResource("VoiceCommands") as NixxisBaseExpandPanel;
                        break;
                    case ContactMedia.Mail:
                        ContactCommands = FindResource("MailCommands") as NixxisBaseExpandPanel;
                        break;
                    case ContactMedia.Chat:
                        ContactCommands = FindResource("ChatCommands") as NixxisBaseExpandPanel;
                        break;
                    default:
                        break;
                }

                UIElement NewScript = CInfo.UserData as UIElement;

                if (ScriptContainer.Content == null || !ScriptContainer.Content.Equals(NewScript))
                {
                    ScriptContainer.Content = NewScript;
                }
            }
            else
            {
                if (ClientLink.Contacts.Count > 0)
                {
                    m_ClientLink.Contacts.ActiveContact = ClientLink.Contacts[0];
                }
                else
                {
                    ShowNoContactPanel();
                }
            }
            if (m_ClientLink.Contacts.ActiveContact != null)
            {
                m_ClientLink.Contacts.ActiveContact.RequestAgentAction = false;
            }

            if (ContactCommands == null)
            {
                ContactCommands = FindResource("EmptyCommands") as NixxisBaseExpandPanel;
            }

            if (ContactCommands != null)
            {
                DependencyObject xxx = FocusManager.GetFocusScope(this);

                NixxisGrid.SetPanelCommand.Execute(ContactCommands);
            }
        }
        bool navigated = false;
        void ShowNoContactPanel()
        {
            if (ClientLink.Contacts.Count == 0)
            {
                if (m_LoginPanel == null)
                {
                    string LoginScript = ClientLink.ClientSettings["loginScript"];

                    try
                    {
                        if (!string.IsNullOrEmpty(LoginScript))
                        {
                            Type LoginType = null;
                            ContactInfo LoginContact = null;

                            if(LoginScript.StartsWith("shell://"))
                            {
                                if (!navigated)
                                {
                                    navigated = true;
                                    System.Diagnostics.Process.Start(LoginScript.Substring(8));
                                }
                            }
                            else if (Uri.IsWellFormedUriString(LoginScript, UriKind.Absolute) || LoginScript.StartsWith("file://"))
                            {
                                LoginType = typeof(LoginPanelControl);
                                LoginContact = new ContactInfo(ClientLink, 'U', "00000000000000000000000000000000", LoginScript);
                            }
                            else
                            {
                                LoginType = Type.GetType(LoginScript);
                                LoginContact = new ContactInfo(ClientLink, 'U', "00000000000000000000000000000000");
                            }

                            if (LoginType != null)
                            {
                                object LoginApp = null;

                                try
                                {
                                    LoginApp = Activator.CreateInstance(LoginType, ClientLink, LoginContact);
                                }
                                catch (MissingMethodException)
                                {
                                    try
                                    {
                                        LoginApp = Activator.CreateInstance(LoginType, ClientLink, LoginScript);
                                    }
                                    catch (MissingMethodException)
                                    {
                                        try
                                        {
                                            LoginApp = Activator.CreateInstance(LoginType, ClientLink);
                                        }
                                        catch (MissingMethodException)
                                        {
                                            LoginApp = Activator.CreateInstance(LoginType);
                                        }
                                    }
                                }

                                if (LoginApp is IClientApplication)
                                {
                                    m_LoginPanel = null; //  new WindowsFormHost((IClientApplication)LoginApp).UserControl;
                                    ((IClientApplication)LoginApp).StartScript(LoginContact, LoginContact);
                                }
                                else
                                {
                                    m_LoginPanel = (Control)LoginApp;
                                }

                                bool DoShow;

                                if (bool.TryParse(ClientLink.ClientSettings["showLoginScriptButton"], out DoShow) && DoShow)
                                {
                                    //TODO: must show the "show login panel" button
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                if (ClientLink.Commands.WaitForCall.Active
                    || ClientLink.Commands.WaitForChat.Active
                    || ClientLink.Commands.WaitForMail.Active)
                {
                    if (m_LoginPanel != null)
                    {
                        if (!navigated)
                            navigated = true;
                    }
                    else
                    {
                        if (m_WaitPanel == null)
                        {
                            if (!string.IsNullOrEmpty(ClientLink.ClientSettings["WaitPanelType"]))
                            {
                                try
                                {
                                    m_WaitPanel = Activator.CreateInstance(Type.GetType(ClientLink.ClientSettings["WaitControlType"]), ClientLink, "") as Control;
                                }
                                catch (MissingMethodException)
                                {
                                    try
                                    {
                                        m_WaitPanel = Activator.CreateInstance(Type.GetType(ClientLink.ClientSettings["WaitControlType"]), ClientLink) as Control;
                                    }
                                    catch
                                    {
                                    }
                                }
                                catch
                                {
                                }
                            }

                            if (m_WaitPanel == null)
                                m_WaitPanel = new WaitPanelControl(ClientLink);
                        }

                        ScriptContainer.Content = m_WaitPanel;
                    }
                }
                else
                {
                    if (m_LoginPanel != null)
                    {
                        ScriptContainer.Content = m_LoginPanel;
                    }
                    else
                    {
                        if (m_PausePanel == null)
                        {
                            if (!string.IsNullOrEmpty(ClientLink.ClientSettings["PausePanelType"]))
                            {
                                try
                                {
                                    m_PausePanel = Activator.CreateInstance(Type.GetType(ClientLink.ClientSettings["PausePanelType"]), ClientLink, "") as Control;
                                }
                                catch (MissingMethodException)
                                {
                                    try
                                    {
                                        m_PausePanel = Activator.CreateInstance(Type.GetType(ClientLink.ClientSettings["PausePanelType"]), ClientLink) as Control;
                                    }
                                    catch
                                    {
                                    }
                                }
                                catch
                                {
                                }
                            }

                            if (m_PausePanel == null)
                                m_PausePanel = new PausePanelControl(ClientLink);
                        }

                        ScriptContainer.Content = m_PausePanel;
                    }
                }
            }
        }

        void AgentFrameSet_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as AgentFrameSet).IsVisible)
            {
                Focus();

                NixxisBaseExpandPanel nep = FindResource("AgtPanel1") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.SetPanelCommand.Execute(nep);
                }

                nep = FindResource("VoiceCommands") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.SetPanelCommand.Execute(nep);
                }

                nep = FindResource("ChatCommands") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.SetPanelCommand.Execute(nep);
                }

                nep = FindResource("MailCommands") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.SetPanelCommand.Execute(nep);
                }

                nep = FindResource("EmptyCommands") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.SetPanelCommand.Execute(nep);
                }

                nep = FindResource("AgentCommands") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.SetPanelCommand.Execute(nep);
                }

                ShowContactPanel();

                nep = FindResource("InfoPanelStatusView") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.SetPanelCommand.Execute(nep);
                }


                NixxisExpandCoverFlowPanel necfp = FindResource("AgtPanel1") as NixxisExpandCoverFlowPanel;
                if (necfp != null)
                {
                    ContactInfo ctInfo = necfp.SelectedItem as ContactInfo;

                    if (ctInfo != null)
                    {

                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                        {
                            m_ClientLink.Contacts.ForceActiveContact(ctInfo);
                        }));
                    }


                }

            }
            else
            {
                NixxisBaseExpandPanel nep = FindResource("AgtPanel1") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.RemovePanelCommand.Execute(nep);
                }
                nep = FindResource("VoiceCommands") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.RemovePanelCommand.Execute(nep);
                }
                nep = FindResource("ChatCommands") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.RemovePanelCommand.Execute(nep);
                }
                nep = FindResource("MailCommands") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.RemovePanelCommand.Execute(nep);
                }
                nep = FindResource("EmptyCommands") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.RemovePanelCommand.Execute(nep);
                }
                nep = FindResource("AgentCommands") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.RemovePanelCommand.Execute(nep);
                }
                nep = FindResource("InfoPanelStatusView") as NixxisBaseExpandPanel;
                if (nep != null)
                {
                    nep.DataContext = this;

                    NixxisGrid.RemovePanelCommand.Execute(nep);
                }

            }
        }

        void AgentFrameSet_Loaded(object sender, RoutedEventArgs e)
        {
            NixxisButton Button;

            System.Diagnostics.Trace.WriteLine("Agent frameset loaded");

            ContentSplitter.MaximizeTop();
            ContentSplitter.Visibility = System.Windows.Visibility.Collapsed;


            NixxisBaseExpandPanel VoiceCommands = FindResource("VoiceCommands") as NixxisBaseExpandPanel;
            NixxisBaseExpandPanel ChatCommands = FindResource("ChatCommands") as NixxisBaseExpandPanel;
            NixxisBaseExpandPanel MailCommands = FindResource("MailCommands") as NixxisBaseExpandPanel;
            NixxisBaseExpandPanel AgentCommands = FindResource("AgentCommands") as NixxisBaseExpandPanel;

            foreach (ICommand Command in ClientLink.Commands.StandardCommands)
            {
                string CommandName = Command.Code.ToString().Replace("Command", "");

                if (AgentCommands != null && (Button = ((NixxisButton)AgentCommands.FindByName(CommandName, true))) != null)
                    Button.Command = Command;
                if (VoiceCommands != null && (Button = ((NixxisButton)VoiceCommands.FindByName(CommandName, true))) != null)
                    Button.Command = Command;
                if (ChatCommands != null && (Button = ((NixxisButton)ChatCommands.FindByName(CommandName, true))) != null)
                    Button.Command = Command;
                if (MailCommands != null && (Button = ((NixxisButton)MailCommands.FindByName(CommandName, true))) != null)
                    Button.Command = Command;

            }

            if ((Button = ((NixxisButton)VoiceCommands.FindByName("VoiceClose"))) != null)
                Button.Command = CloseContactCommand;

            if ((Button = ((NixxisButton)ChatCommands.FindByName("ChatClose"))) != null)
                Button.Command = CloseContactCommand;

            if ((Button = ((NixxisButton)MailCommands.FindByName("MailClose"))) != null)
                Button.Command = CloseContactCommand;

            if ((Button = ((NixxisButton)AgentCommands.FindByName("Ready"))) != null)
                ;


            ClientLink.Commands.Pause.BeforeExecute += ClientLink_Commands_Pause_BeforeExecute;

            ClientLink.Commands.MailReply.BeforeExecute += ClientLink_Commands_MailReply_BeforeExecute;

            ClientLink.Commands.MailForward.BeforeExecute += ClientLink_Commands_MailForward_BeforeExecute;

            ClientLink.Commands.VoiceForward.BeforeExecute += ClientLink_Commands_VoiceForward_BeforeExecute;

            ClientLink.Commands.ChatForward.BeforeExecute += ClientLink_Commands_ChatForward_BeforeExecute;
        }

        bool ClientLink_Commands_ChatForward_BeforeExecute(object parameter)
        {
            try
            {
                if (m_ClientLink.Contacts.ActiveContact != null)
                {
                    ChatForwardWindow cfw = new ChatForwardWindow();
                    cfw.Owner = Application.Current.MainWindow;
                    cfw.ClientLink = m_ClientLink;

                    if (cfw.ShowDialog().GetValueOrDefault() && cfw.Destination!=null)
                    {
                        m_ClientLink.Commands.ChatForward.Execute(cfw.Destination);
                    }

                    return true;
                }
                return false;
            }
            catch (Exception Ex)
            {
                System.Diagnostics.Trace.WriteLine(Ex.ToString());
                return false;
            }


        }

        private bool ClientLink_Commands_Pause_BeforeExecute(object x)
        {
            #region GoToPause
            if (ClientLink.PauseCodes.Count > 0)
            {
                try
                {
                    PauseWindow Dlg = new PauseWindow();
                    Dlg.SetItemSource(ClientLink.PauseCodes);

                    if (Application.Current.MainWindow.IsLoaded)
                        Dlg.Owner = Application.Current.MainWindow;

                    bool? DlgResult = Dlg.ShowDialog();

                    if (DlgResult.HasValue && DlgResult.Value && Dlg.SelectedItem != null)
                    {
                        ClientLink.Commands.Pause.Execute(Dlg.SelectedItem.PauseCodeId);
                        return true;
                    }
                    else if (DlgResult.Value == false)
                        return true;
                    else
                        return false;
                }
                catch (Exception Ex)
                {
                    System.Diagnostics.Trace.WriteLine(Ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
            #endregion
        }

        private bool ClientLink_Commands_MailReply_BeforeExecute(object x)
        {
            #region MailReply

            if (ClientLink.Contacts.ActiveContact == null)
                return false;

            SendCurrentMailResponse();

            return false;

            #endregion
        }

        private bool ClientLink_Commands_MailForward_BeforeExecute(object x)
        {
            #region MailForward
            try
            {
                MailForwardWindow dlg = new MailForwardWindow();
                dlg.DestinationSource = GetWellKnownDestinationCollection();

                if (Application.Current.MainWindow.IsLoaded)
                    dlg.Owner = Application.Current.MainWindow;

                bool? DlgResult = dlg.ShowDialog();

                if (DlgResult.HasValue && DlgResult.Value)
                {
                    SendCurrentMailResponse();

                    ClientLink.Commands.MailForward.Execute(dlg.Destination, ClientLink.Contacts.ActiveContactId, dlg.Delay == DateTime.MinValue ? "0" : ((int)dlg.Delay.Subtract(DateTime.Now).TotalSeconds).ToString(), dlg.SendResponseNow.ToString());

                    SetLastMailForwardDestination(dlg.Destination);
                    return true;
                }
                return false;
            }
            catch (Exception Ex)
            {
                System.Diagnostics.Trace.WriteLine(Ex.ToString());
                return false;
            }

            #endregion
        }

        private bool ClientLink_Commands_VoiceForward_BeforeExecute(object x)
        {
            #region VoiceForward
            try
            {
                if (m_ClientLink.Contacts.ActiveContact != null)
                {
                    // Call a forward
                    string destination = VoiceNewCall_RequestDestination("", TranslationContext.Translate("Forward..."));

                    if (destination != null)
                        m_ClientLink.Commands.VoiceForward.Execute(destination);

                    return true;
                }
                return false;
            }
            catch (Exception Ex)
            {
                System.Diagnostics.Trace.WriteLine(Ex.ToString());
                return false;
            }

            #endregion
        }

        void AgtFrameSet_Unloaded(object sender, RoutedEventArgs e)
        {

            ClientLink.Commands.Pause.BeforeExecute -= ClientLink_Commands_Pause_BeforeExecute;

            ClientLink.Commands.MailReply.BeforeExecute -= ClientLink_Commands_MailReply_BeforeExecute;

            ClientLink.Commands.MailForward.BeforeExecute -= ClientLink_Commands_MailForward_BeforeExecute;

            ClientLink.Commands.VoiceForward.BeforeExecute -= ClientLink_Commands_VoiceForward_BeforeExecute;

            ClientLink.Commands.ChatForward.BeforeExecute -= ClientLink_Commands_ChatForward_BeforeExecute;

            if (m_UserChatWindow != null)
            {
                try
                {
                    m_UserChatWindow.Close();
                    m_UserChatWindow = null;
                }
                catch { }
            }

            System.Diagnostics.Trace.WriteLine("Agent frameset unloaded");
        }

        private void SendCurrentMailResponse()
        {
            IMailPanel mailCtrl = ClientLink.Contacts.ActiveContact.UserData as IMailPanel;

            if (mailCtrl != null)
            {
                MailMessage msg = mailCtrl.MailMessage;
                ClientLink.MailReply(ClientLink.Contacts.ActiveContact, msg.ReplyFrom, msg.ReplyTo, null, msg.ReplySubject, mailCtrl.GetHtmlText(), mailCtrl.GetAttachmentIds().ToArray());
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("Incorrect mail panel type " + ((ClientLink.Contacts.ActiveContact.UserData == null) ? "(null)" : ClientLink.Contacts.ActiveContact.UserData.GetType().FullName));
            }
        }

        private string m_LastNumber;
        private string VoiceNewCall_RequestDestination(string defaultDestination, string title)
        {
            string KeyName = "Software\\Nixxis\\NixxisClient";

            try
            {
                string SetKey = System.Configuration.ConfigurationManager.AppSettings["RegistryKey"];

                if (!string.IsNullOrWhiteSpace(SetKey))
                {
                    if (SetKey.StartsWith("\\"))
                        KeyName = SetKey;
                    else
                        KeyName = KeyName + "\\" + SetKey;
                }
            }
            catch
            {
            }

            try
            {
                VoiceNewCallWindow Dlg = new VoiceNewCallWindow();

                List<string> ListDestinations = new List<string>(ClientLink.WellKnownCallDestinations);

                try
                {
                    using (Microsoft.Win32.RegistryKey ClientKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName, false))
                    {
                        if (ClientKey != null)
                        {
                            string[] LastDest = ClientKey.GetValue("LastManualCallDestinations", new string[0]) as string[];

                            ClientKey.Close();

                            for (int i = LastDest.Length - 1; i >= 0; i--)
                            {
                                for (int j = 0; j < ListDestinations.Count; j++)
                                {
                                    if (LastDest[i].Equals(ListDestinations[j] as string, StringComparison.OrdinalIgnoreCase))
                                    {
                                        ListDestinations.RemoveAt(j);
                                        break;
                                    }
                                }

                                ListDestinations.Insert(0, LastDest[i]);
                            }
                        }
                    }
                }
                catch
                {
                }

                string DefaultValue = defaultDestination;

                if (!string.IsNullOrEmpty(DefaultValue))
                {
                    if (ListDestinations.Contains(DefaultValue))
                        ListDestinations.Remove(DefaultValue);

                    ListDestinations.Insert(0, DefaultValue);
                }
                else
                {
                    DefaultValue = m_LastNumber ?? "";
                }

                Dlg.SetItemSource(ListDestinations.ToArray());
                Dlg.SetDefaultValue(DefaultValue);

                Dlg.Title = title;

                if (Application.Current.MainWindow.IsLoaded)
                    Dlg.Owner = Application.Current.MainWindow;

                bool? DlgResult = Dlg.ShowDialog();

                if (DlgResult.HasValue && DlgResult.Value && Dlg.SelectedItem != null)
                {
                    if (string.IsNullOrEmpty(defaultDestination))
                    {
                        m_LastNumber = Dlg.SelectedItem;

                        try
                        {
                            using (Microsoft.Win32.RegistryKey ClientKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName, true))
                            {
                                if (ClientKey != null)
                                {
                                    string[] LastDest = ClientKey.GetValue("LastManualCallDestinations", new string[0]) as string[];
                                    int Found = -1;

                                    for (int i = 0; i < LastDest.Length; i++)
                                    {
                                        if (LastDest[i].Equals(m_LastNumber, StringComparison.OrdinalIgnoreCase))
                                        {
                                            Found = i;
                                            break;
                                        }
                                    }

                                    if (Found < 0)
                                    {
                                        if (LastDest.Length < 250)
                                        {
                                            string[] NewDest = new string[LastDest.Length + 1];

                                            LastDest.CopyTo(NewDest, 0);
                                            LastDest = NewDest;
                                        }

                                        Found = LastDest.Length - 1;
                                    }

                                    for (int i = Found; i > 0; i--)
                                        LastDest[i] = LastDest[i - 1];

                                    LastDest[0] = m_LastNumber;

                                    ClientKey.SetValue("LastManualCallDestinations", LastDest);

                                    ClientKey.Close();
                                }
                            }
                        }
                        catch
                        {
                        }
                    }

                    return Dlg.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception Ex)
            {
                System.Diagnostics.Trace.WriteLine(Ex.ToString());
            }

            return null;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == DoNewCall)
            {
                #region DoNewCall
                ContactInfo CInfo = m_ClientLink.Contacts.ActiveContact;

                if (CInfo != null)
                {
                    if (CInfo.State == 'P')
                    {
                        // Call a preview
                        string destination = VoiceNewCall_RequestDestination(CInfo.To, TranslationContext.Translate("Activity dialing..."));

                        if (destination != null)
                            m_ClientLink.Commands.VoiceNewCall.Execute(destination, CInfo.Id);
                    }
                    else
                    {
                        // New manual call

                        bool HoldFirst = false;

                        if (bool.TryParse(m_ClientLink.ClientSettings["HoldOnConsultDialog"], out HoldFirst) && HoldFirst)
                            m_ClientLink.Commands.VoiceHold.Execute();

                        string destination = VoiceNewCall_RequestDestination("", TranslationContext.Translate("Manual dialing..."));

                        if (destination != null)
                            m_ClientLink.Commands.VoiceNewCall.Execute(destination, CInfo.Id);
                    }
                }
                else
                {
                    // New manual call

                    string destination = VoiceNewCall_RequestDestination("", TranslationContext.Translate("Manual dialing..."));

                    if (destination != null)
                        m_ClientLink.Commands.VoiceNewCall.Execute(destination, null);
                }
                #endregion
            }
            else if (e.Command == SetQualification)
            {
                #region SetQualification
                QualificationInfo QInfo = null;
                Nixxis.Client.Calendar cal = null;
                ContactInfo CInfo = ClientLink.Contacts.ActiveContact;

                if (CInfo != null)
                {
                    try
                    {
                        QInfo = QualificationInfo.FromActivityId(ClientLink, CInfo.Activity);
                    }
                    catch
                    {
                    }
                    try
                    {
                        cal = ClientLink.GetCallbacks(CInfo.Id, null, null);
                    }
                    catch
                    {
                    }

                    if (QInfo != null && QInfo.ListQualification.Count > 0)
                    {
                        try
                        {
                            QualificationWindow Dlg = new QualificationWindow();

                            BindingOperations.SetBinding(Dlg, QualificationWindow.ContactInfoProperty, new Binding(){Source= ClientLink.Contacts, Path = new PropertyPath("ActiveContact")});



                            Dlg.SetItemSource(QInfo);
                            Dlg.SetTimeZones(ClientLink.TimeZones);
                            Dlg.SetCallbackCalendar(cal);

                            if (CInfo.Direction == "I")
                                Dlg.CallbackDestination = CInfo.From;
                            else
                                Dlg.CallbackDestination = CInfo.To;

                            if (Application.Current.MainWindow.IsLoaded)
                                Dlg.Owner = Application.Current.MainWindow;

                            bool? DlgResult = Dlg.ShowDialog();

                            if (DlgResult.HasValue && DlgResult.Value && Dlg.SelectedItem != null)
                            {
                                if (Dlg.SelectedItem.Action == 4 || Dlg.SelectedItem.Action == 5)
                                {
                                    CInfo.Qualify(Dlg.SelectedItem.Id, TimeZoneInfo.ConvertTime(Dlg.CallbackCalendar.SelectedDate, Dlg.CallbackTimeZone), Dlg.CallbackDestination);
                                }
                                else
                                {
                                    CInfo.Qualify(Dlg.SelectedItem);
                                }
                            }
                        }
                        catch (Exception Ex)
                        {
                            System.Diagnostics.Trace.WriteLine(Ex.ToString());
                        }
                    }
                }
                #endregion
            }
            else if (e.Command == SearchMode)
            {
                #region SearchMode
                if (ClientLink.SearchModeActivities.Count > 0)
                {
                    try
                    {
                        SearchModeWindow Dlg = new SearchModeWindow();
                        Dlg.SetItemSource(ClientLink.SearchModeActivities);
                        
                        if (Application.Current.MainWindow.IsLoaded)
                            Dlg.Owner = Application.Current.MainWindow;

                        bool? DlgResult = Dlg.ShowDialog();

                        if (DlgResult.HasValue && DlgResult.Value && Dlg.SelectedItem != null)
                        {
                            ClientLink.Commands.SearchMode.Execute(Dlg.SelectedItem.ActivityId);
                        }
                    }
                    catch (Exception Ex)
                    {
                        System.Diagnostics.Trace.WriteLine(Ex.ToString());
                    }
                }
                #endregion
            }
            else if (e.Command == SendMessage)
            {
                #region SendMessage
                ClientLink.Commands.RequestAssistance.Execute();
                #endregion
            }
            else if (e.Command == TeamSelection)
            {
                #region TeamSelection
                if (ClientLink.Teams.Count > 0)
                {
                    try
                    {
                        TeamSelectionWindow Dlg = new TeamSelectionWindow();
                        Dlg.SetItemSource(ClientLink.Teams);

                        if (Application.Current.MainWindow.IsLoaded)
                            Dlg.Owner = Application.Current.MainWindow;

                        bool? DlgResult = Dlg.ShowDialog();

                        if (DlgResult.HasValue && DlgResult.Value)
                        {
                            for (int i = 0; i < Dlg.ItemList.Count; i++)
                            {
                                if (Dlg.ItemList[i].IsSelected != ((TeamInfo)Dlg.ItemList[i].Item).Active)
                                {
                                    if (Dlg.ItemList[i].IsSelected == null)
                                        ClientLink.ActivateTeam(((TeamInfo)Dlg.ItemList[i].Item).TeamId, false);
                                    else if (Dlg.ItemList[i].IsSelected == true)
                                        ClientLink.ActivateTeam(((TeamInfo)Dlg.ItemList[i].Item).TeamId, true);
                                    else
                                        ClientLink.ActivateTeam(((TeamInfo)Dlg.ItemList[i].Item).TeamId, false);
                                }
                            }
                        }
                    }
                    catch (Exception Ex)
                    {
                        System.Diagnostics.Trace.WriteLine(Ex.ToString());
                    }
                }
                #endregion
            }
            else if (e.Command == DisplayContactHistory)
            {
                #region DisplayContactHistory
                ContactInfo CInfo = ClientLink.Contacts.ActiveContact;

                if (CInfo != null)
                {
                    try
                    {
                        try
                        {
                            ContactHistoryWindow Dlg = new ContactHistoryWindow();
                            Dlg.SetItemSource(CInfo.GetHistory());

                            if (Application.Current.MainWindow.IsLoaded)
                                Dlg.Owner = Application.Current.MainWindow;

                            bool? DlgResult = Dlg.ShowDialog();

                            if (DlgResult.HasValue && DlgResult.Value && Dlg.SelectedItem != null)
                            {
                                SelectedContactHistory = Dlg.SelectedItem;
                            }
                        }
                        catch (Exception Ex)
                        {
                            System.Diagnostics.Trace.WriteLine(Ex.ToString());
                        }

                    }
                    catch (Exception Ex)
                    {
                        System.Diagnostics.Trace.WriteLine(Ex.ToString());
                    }
                }
                #endregion
            }
        }


        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == DoNewCall)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
            else if (e.Command == CloseContact)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
            else if (e.Command == SetQualification)
            {
                e.CanExecute = ClientLink.Commands.SetQualification.Authorized;
                e.Handled = true;
            }
            else if (e.Command == SearchMode)
            {
                e.CanExecute = ClientLink.SearchModeActivities.Count > 0;
                e.Handled = true; 
            }
            else if (e.Command == SendMessage)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
            else if (e.Command == TeamSelection)
            {
                e.CanExecute = m_HandleTeamSelection && ClientLink.Teams.Count > 0;
                e.Handled = true;
            }
            else if (e.Command == DisplayContactHistory)
            {
                e.CanExecute = ClientLink.Commands.ShowHistory.Authorized;
                e.Handled = true;
            }
            else
            {
            }
        }

        private void NixxisExpandCoverFlowPanel_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is NixxisExpandCoverFlowPanel && ((NixxisExpandCoverFlowPanel)sender).SelectedItem != null)
            {
                ContactInfo ctc = (sender as NixxisExpandCoverFlowPanel).SelectedItem as ContactInfo;

                m_ClientLink.Contacts.ActiveContact = ctc;
            }
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
        }


        #region WellKnownDestinations
        private WellKnownDestinationCollection GetWellKnownDestinationCollection()
        {
            WellKnownDestinationCollection collection = new WellKnownDestinationCollection();

            foreach(string item in ClientLink.WellKnownCallDestinations)
            {
                WellKnownDestinationItem des = new WellKnownDestinationItem();

                des.Destination = item;
                des.Type = item.Contains("@") ? "Mail" : "Voice";

                if (item.StartsWith("@"))
                    des.DisplayText = item.ToString().Substring(1);
                else
                    des.DisplayText = item;

                collection.Add(des);
            }

            List<string> local = GetLastMailForwardDestinations();

            foreach (string item in local)
            {
                if (!collection.Contains(item))
                {
                    WellKnownDestinationItem des = new WellKnownDestinationItem();

                    des.Destination = item;
                    des.Type = item.Contains("@") ? "Mail" : "Voice";

                    if (item.StartsWith("@"))
                        des.DisplayText = item.ToString().Substring(1);
                    else
                        des.DisplayText = item;

                    collection.Add(des);
                }
            }

            return collection;
        }

        private List<string> GetLastMailForwardDestinations()
        {
            string KeyName = "Software\\Nixxis\\NixxisClient";

            try
            {
                string SetKey = System.Configuration.ConfigurationManager.AppSettings["RegistryKey"];

                if (!string.IsNullOrWhiteSpace(SetKey))
                {
                    if (SetKey.StartsWith("\\"))
                        KeyName = SetKey;
                    else
                        KeyName = KeyName + "\\" + SetKey;
                }
            }
            catch
            {
            }

            List<string> lastMailForwardDestinations = new List<string>();

            try
            {
                Microsoft.Win32.RegistryKey ClientKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName, false);

                if (ClientKey != null)
                {
                    string[] LastDest = ClientKey.GetValue("LastMailForwardDestinations", new string[0]) as string[];

                    ClientKey.Close();

                    for (int i = LastDest.Length - 1; i >= 0; i--)
                    {
                        lastMailForwardDestinations.Add(LastDest[i]);
                    }
                }

            }
            catch
            {
            }
            return lastMailForwardDestinations;
        }
        private void SetLastMailForwardDestination(string destination)
        {
            string KeyName = "Software\\Nixxis\\NixxisClient";

            try
            {
                string SetKey = System.Configuration.ConfigurationManager.AppSettings["RegistryKey"];

                if (!string.IsNullOrWhiteSpace(SetKey))
                {
                    if (SetKey.StartsWith("\\"))
                        KeyName = SetKey;
                    else
                        KeyName = KeyName + "\\" + SetKey;
                }
            }
            catch
            {
            }

            try
            {
                Microsoft.Win32.RegistryKey ClientKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName, true);

                if (ClientKey != null)
                {
                    string[] LastDest = ClientKey.GetValue("LastMailForwardDestinations", new string[0]) as string[];
                    int Found = -1;

                    for (int i = 0; i < LastDest.Length; i++)
                    {
                        if (LastDest[i].Equals(destination, StringComparison.OrdinalIgnoreCase))
                        {
                            Found = i;
                            break;
                        }
                    }

                    if (Found < 0)
                    {
                        if (LastDest.Length < 250)
                        {
                            string[] NewDest = new string[LastDest.Length + 1];

                            LastDest.CopyTo(NewDest, 0);
                            LastDest = NewDest;
                        }

                        Found = LastDest.Length - 1;
                    }

                    for (int i = Found; i > 0; i--)
                        LastDest[i] = LastDest[i - 1];

                    LastDest[0] = destination;

                    ClientKey.SetValue("LastMailForwardDestinations", LastDest);

                    ClientKey.Close();
                }
            }
            catch
            {
            }
        }
        #endregion
    }
}
