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
using System.Timers;
using System.Windows.Threading;
using Nixxis.Client.Controls;
using System.Collections.Specialized;
using System.Windows.Markup;
using System.IO;
using System.Xml;
using ContactRoute;
using Nixxis.ClientV2;
using System.ComponentModel;
using System.Net;


namespace Nixxis.Client.Supervisor
{
    /// <summary>
    /// Interaction logic for SupFrameSet.xaml
    /// </summary>
    public partial class SupFrameSet : UserControl
    {
        public static RoutedUICommand DoNewCall { get; private set; }

        public static TranslationContext TranslationContext = new TranslationContext("SupFrameSet");
        public static PersistentRoutedUICommand ShowCategory { get; private set; }

        public static RoutedUICommand SupViewShowcolumnSelectorOperation { get; private set; }
        public static RoutedUICommand SupViewAgentSendMsg { get; private set; }

        public static RoutedUICommand SupWorkspaceOpen { get; private set; }
        public static RoutedUICommand SupWorkspaceSave { get; private set; }

        static SupFrameSet()
        {
            DoNewCall = new RoutedUICommand(string.Empty, "DoNewCall", typeof(SupFrameSet));

            ShowCategory = new PersistentRoutedUICommand(string.Empty, "ShowCategory", typeof(SupFrameSet));
            SupViewShowcolumnSelectorOperation = new RoutedUICommand(string.Empty, "SupViewShowcolumnSelectorOperation", typeof(SupFrameSet));
            SupViewAgentSendMsg = new RoutedUICommand(string.Empty, "SupViewAgentSendMsg", typeof(SupFrameSet));

            SupWorkspaceOpen = new RoutedUICommand(string.Empty, "SupWorkspaceOpen", typeof(SupFrameSet));
            SupWorkspaceSave = new RoutedUICommand(string.Empty, "SupWorkspaceSave", typeof(SupFrameSet));
        }

        //InternalCommand CloseContactCommand;
        private NixxisBaseExpandPanel m_CurrentNepPanel = null;
        private Nixxis.ClientV2.Supervision m_SupervisionLink;
        private SavedContexts m_SavedContexts = null;        
        private ISession m_Session;
        private string m_MainWindowTitle = string.Empty;
        private NixxisUserChatCollection m_UserChatCollection = new NixxisUserChatCollection();
        private NixxisUserChatWindow m_UserChatWindow = null; 
        private AgentWarningMessageCollection m_AgtWarningMsg = new AgentWarningMessageCollection();
        private string m_UserId = string.Empty;
        public TeamList Teams { get; internal set; }

        public SupFrameSet(ISession session, Nixxis.ClientV2.Supervision supLink, string userId): this(session, supLink, userId, true)
        {

        }
        public SupFrameSet(ISession session, Nixxis.ClientV2.Supervision supLink, string userId, bool allowCommandBuilding)
        {
            m_MainWindowTitle = Application.Current.MainWindow.Title;
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(SupFrameSet_IsVisibleChanged);


            InitializeComponent();

            SupFrameSet.ShowCategory.State = tileView1.GetSelectedId();

            m_UserId = userId;
            m_SupervisionLink = supLink;
            m_Session = session;
            m_SupervisionLink.MessageReceived += new ClientV2.NixxisMessageReceivedHandler(SupervisionLink_MessageReceived);
            m_SupervisionLink.ClientLink.AgentWarning += new HttpLinkServerEventDelegate(ClientLink_AgentWarning);
            m_SupervisionLink.ClientLink.ContactStateChanged += new HttpLinkContactEventDelegate(ClientLink_ContactStateChanged); 
            m_SupervisionLink.ClientLink.ContactAdded += new HttpLinkContactEventDelegate(ClientLink_ContactAdded);
            m_SupervisionLink.ClientLink.ContactRemoved += new HttpLinkContactEventDelegate(ClientLink_ContactRemoved);

            m_SupervisionLink.ClientLink.Watch += ClientLink_Watch;
            m_SupervisionLink.ClientLink.SelectionRelatedCommands += ClientLink_SelectionRelatedCommands;


            SupInboundPanel.DataContext = m_SupervisionLink.Inbounds;

            SupAgentPanel.DataContextChanged += new DependencyPropertyChangedEventHandler(SupAgentPanel_DataContextChanged);
            SupAgentPanel.DataContext = m_SupervisionLink.AgentsFiltered;
            InitAgentList();

            SupOutboundPanel.DataContext = m_SupervisionLink.Outbounds;
            SupQueuePanel.DataContext = m_SupervisionLink.Queues;
            SupCampaignPanel.DataContext = m_SupervisionLink.Campaigns;
            SupAlertPanel.DataContext = m_SupervisionLink.Alerts;
            SupAlertPanel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SupAlertPanel_PropertyChanged);

            SupDashboardPanel.DataContext = m_SupervisionLink;

            Teams = m_SupervisionLink.Teams;
            tileView1.UnSelectItem(tileView1.SelectedItem);

            m_SavedContexts = new SavedContexts(m_SupervisionLink, string.Empty);            

        }

        void ClientLink_SelectionRelatedCommands(ServerEventArgs eventArgs)
        {

            System.Diagnostics.Trace.WriteLine("ClientLink_SelectionRelatedCommands start " + eventArgs.Parameters);
            string param = eventArgs.Parameters;

            string[] parts = param.Split(new string[] { ",[", "]" }, StringSplitOptions.RemoveEmptyEntries);

            string strObjId = parts[0];

            Menu = null;

            if (parts.Length < 2)      
                return;


            string strCommands = parts[1];

            string[] strCommandsParts = strCommands.Split(new string[] {";" }, StringSplitOptions.RemoveEmptyEntries);




            foreach(string strCmd in strCommandsParts)
            {
                if(Menu == null)
                {
                    Menu = new ContextMenu();
                    Menu.Tag = strObjId;
                }

                string[] actionParts = Microsoft.JScript.GlobalObject.unescape(strCmd).Split(',');

                MenuItem mi = new MenuItem();
                mi.Header = TranslationContext.Translate(Microsoft.JScript.GlobalObject.unescape(actionParts[0]));

                if (actionParts[1].StartsWith("#"))
                {
                    mi.Click += ((a, b) => {
                        try
                        {
                            ICommand ic = m_SupervisionLink.ClientLink.Commands.StandardCommands.First((c) => (c.Code.ToString().Equals(actionParts[1].Substring(1))));
                            BaseCommand bc = ic as BaseCommand;
                            if (bc != null)
                            {
                                bc.Execute((object)null);
                            }
                            else
                            {
                                ic.Execute();
                            }
                        }
                        catch
                        {

                        }
                    });
                }
                else
                {
                    // not a "standard" command
                    mi.Click += ((a, b) => {
                        try
                        {
                            m_SupervisionLink.ClientLink.ExecuteCommand(actionParts[1], actionParts.Skip(2).ToArray());
                        }
                        catch
                        {

                        }
                    });
                }
                Menu.Items.Add(mi);
            }

            System.Diagnostics.Trace.WriteLine("ClientLink_SelectionRelatedCommands stop " + Menu.Items.Count.ToString());
        }


        public ContextMenu Menu = null;

        private NixxisViewAgentScreen dlgViewScreen = null;

        void ClientLink_Watch(ServerEventArgs eventArgs)
        {
            string param = eventArgs.Parameters;

            if (param.StartsWith("1"))
            {
                string[] parts = param.Substring(2).Split(new string[] { "[", "],[", "]" }, 3, StringSplitOptions.RemoveEmptyEntries);


                string[] ipAddresses = parts[1].Split(';').Select((a)=>(Microsoft.JScript.GlobalObject.unescape(a))).ToArray();

                dlgViewScreen = new NixxisViewAgentScreen();

                dlgViewScreen.SupervisionLink = m_SupervisionLink;

                IViewClientAddon addonObj = null;

                foreach(string strAddonType in parts[0].Split(';').Select((a)=>(Microsoft.JScript.GlobalObject.unescape(a))).ToArray())
                {
                    try
                    {
                        Type addonType = Type.GetType(strAddonType);
                        
                        if(addonType!=null)
                        {
                            addonObj = Activator.CreateInstance(addonType) as IViewClientAddon;        
                            if(addonObj!=null)
                                break;
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.ToString());
                    }
                }

                if(addonObj!=null)
                {

                    addonObj.ConnectionInfo = GetBestConnectionInfo(ipAddresses);

                    dlgViewScreen.MainGrid.Children.Add(addonObj as UIElement);

                }

                dlgViewScreen.Show();
            }
            else
            {
                if (dlgViewScreen != null)
                {
                    dlgViewScreen.CloseLevel = 10;

                    dlgViewScreen.Close();

                    dlgViewScreen = null;
                }
            }
        }

        private static string GetBestConnectionInfo(string[] remoteAddresses)
        {

            List<string> bestValues = new List<string>();

            List<IPAddress> lstRemoteAddresses = new List<IPAddress>();


            foreach(string strAddr in remoteAddresses)
            {
                string[] parts = strAddr.Split('/');
                IPAddress addr = null;

                if(IPAddress.TryParse(parts[0], out addr))
                {
                    lstRemoteAddresses.Add(addr);
                }
            }

            foreach (System.Net.NetworkInformation.NetworkInterface Nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (Nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && Nic.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                {
                    foreach (System.Net.NetworkInformation.UnicastIPAddressInformation Addr in Nic.GetIPProperties().UnicastAddresses)
                    {
                        if (Addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            for (int i = 0; i < lstRemoteAddresses.Count; i++ )
                            {
                                if( (lstRemoteAddresses[i].Address & Addr.IPv4Mask.Address) == (Addr.Address.Address & Addr.IPv4Mask.Address) )
                                {
                                    bestValues.Add(lstRemoteAddresses[i].ToString());
                                }
                            }
                        }
                    }
                }
            }
            if (bestValues.Count > 0)
                return bestValues[0];

            return remoteAddresses[0].Split('/')[0];
        }

        void SupAlertPanel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("lastselecteditem", StringComparison.InvariantCultureIgnoreCase))
            {
                Nixxis.ClientV2.AlertItem alertItem = ((Nixxis.ClientV2.AlertItem)((NixxisSupervisionTileItem)sender).LastSelectedItem);
                if (alertItem.UnRead)
                    alertItem.UnRead = false;
            }
        }

        private void TraceText(string text)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("ClientSupervisionV2 --> {0}", text));
        }
        private void InitAgentList()
        {
            m_SupervisionLink.Agents.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Agents_CollectionChanged);
            foreach (Nixxis.ClientV2.AgentItem item in m_SupervisionLink.Agents)
            {
                if (!m_SupervisionLink.AgentsFiltered.Contains(item))
                {
                    TraceText("New Agent " + item.Account);
                    if (item.RealTime.StatusIndex == 0)
                    {
                        TraceText("Status changed to 0 for Agent " + item.Account);
                        if (m_SupervisionLink.AgentsFiltered.Contains(item))
                        {
                            TraceText("Status changed remove Agent " + item.Account);
                            m_SupervisionLink.AgentsFiltered.Remove(item);
                        }
                    }
                    else
                    {
                        TraceText("Status changed diff from 0 for Agent " + item.Account);
                        if (!m_SupervisionLink.AgentsFiltered.Contains(item))
                        {
                            TraceText("Status changed add Agent " + item.Account);
                            m_SupervisionLink.AgentsFiltered.Add(item);
                        }
                    }
                }
                item.AgentStatusChanged += new Nixxis.ClientV2.AgentItem.AgentStatusChangedDelegate(item_AgentStatusChanged);
            }
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
        #endregion

        #region Messages and chat
        private void SupervisionLink_MessageReceived(ClientV2.AlertItem item)
        {
            if (item.Type == MessageType.Chat)
            {
                //Chat message
                m_UserChatCollection.AddMessage(item.Message, item.OriginatorId, item.Originator, NixxisUserChatLine.ChatLineOriginators.Customer);
                ShowUserChatWindow();
            }
            else if (item.Type == MessageType.ChatEnd)
            {
                //Chat system message
                m_UserChatCollection.AddMessage(item.Message, item.OriginatorId, item.Originator, NixxisUserChatLine.ChatLineOriginators.System, NixxisUserChatLine.ChatEventType.Leave);               
                ShowUserChatWindow();
            }
            else
            {
                //Other message:
            }
        }
        public void UserChatWindow_SendMessage(string msg, string userid)
        {
            m_SupervisionLink.ClientLink.SendMessage(MessageType.Chat, MessageDestinations.Agent, userid, msg);
        }
        public void UserChatWindow_EndConversation(string msg, string userid)
        {
            m_SupervisionLink.ClientLink.SendMessage(MessageType.ChatEnd, MessageDestinations.Agent, userid, msg);
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
            userChatWindow.Mode = NixxisUserChatWindow.Modes.Supervision;
            userChatWindow.SendMessage += new NixxisUserChatSendMsgHandler(UserChatWindow_SendMessage);
            userChatWindow.EndConversation += new NixxisUserChatSendMsgHandler(UserChatWindow_EndConversation);

            return userChatWindow;
        }

        void ClientLink_AgentWarning(ServerEventArgs eventArgs)
        {
            m_AgtWarningMsg.AddMessage(eventArgs.Parameters);
        }
        #endregion


        void Agents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Agents_CollectionChanged. Action {0} ", e.Action));

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Nixxis.ClientV2.AgentItem item in e.NewItems)
                {
                    if (!m_SupervisionLink.AgentsFiltered.Contains(item))
                    {
                        TraceText("New Agent " + item.Account);
                        m_SupervisionLink.AgentsFiltered.Add(item);
                    }
                    item.AgentStatusChanged += new Nixxis.ClientV2.AgentItem.AgentStatusChangedDelegate(item_AgentStatusChanged);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Nixxis.ClientV2.AgentItem item in e.NewItems)
                {
                    if (m_SupervisionLink.AgentsFiltered.Contains(item))
                    {
                        TraceText("Remove Agent " + item.Account);
                        m_SupervisionLink.AgentsFiltered.Remove(item);
                    }
                    item.AgentStatusChanged -= new Nixxis.ClientV2.AgentItem.AgentStatusChangedDelegate(item_AgentStatusChanged);
                }
            }

        }
        private void item_AgentStatusChanged(Nixxis.ClientV2.AgentItem item)
        {
            //To filter based on agent status
            //Undefined = 0,
            //Off = 1,
            //Login = 2,
            //Pause = 3,
            //Waiting = 4,
            //OnLine = 5,
            //WrapUp = 6,
            //Logout = 7,
            //Supervision = 8,
            //Preview = 9,
            //WaitingForVoice = 10,

            try
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    if (item.RealTime.StatusIndex == 0)
                    {
                        TraceText("Status changed to 0 for Agent " + item.Account);
                        if (m_SupervisionLink.AgentsFiltered.Contains(item))
                        {
                            TraceText("Status changed remove Agent " + item.Account);
                            m_SupervisionLink.AgentsFiltered.RemoveAt(m_SupervisionLink.AgentsFiltered.IndexOf(item));
                        }
                    }
                    else
                    {
                        TraceText("Status changed diff from 0 for Agent " + item.Account);
                        if (!m_SupervisionLink.AgentsFiltered.Contains(item))
                        {
                            TraceText("Status changed add Agent " + item.Account);
                            m_SupervisionLink.AgentsFiltered.Add(item);
                        } 
                    }
                }));
            }
            catch (Exception error)
            {
                TraceText("Status changed ERROR: " + error.ToString());
            }
        }
        void SupAgentPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("SupAgentPanel_DataContextChanged");
        }

        void m_SupervisionLink_RefreshCollection()
        {
            System.Diagnostics.Trace.WriteLine("m_SupervisionLink_RefreshCollection  --> " + CollectionViewSource.GetDefaultView(m_SupervisionLink.Agents).ToString());

            if (CollectionViewSource.GetDefaultView(m_SupervisionLink.Agents).Filter == null)
            {
                System.Diagnostics.Trace.WriteLine("No Filters");
                CollectionViewSource.GetDefaultView(m_SupervisionLink.Agents).Filter = new Predicate<object>(collection_Filter2);
            }

            CollectionViewSource.GetDefaultView(m_SupervisionLink.Agents).Refresh();
        }
        public bool collection_Filter2(object item)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Agent {0} has state {1}", ((Nixxis.ClientV2.AgentItem)item).Account, ((Nixxis.ClientV2.AgentItem)item).RealTime.StatusIndex));
            return ((Nixxis.ClientV2.AgentItem)item).RealTime.StatusIndex == 0 ? false : true;
        }
        void collection_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = ((Nixxis.ClientV2.AgentItem)e.Item).RealTime.StatusIndex == 0 ? false : true;
        }

        void tileView1_ViewStateChanged(object sender, NixxisTileViewStateChangedEventArgs e)
        {
            if (e.OldLagreItem != null)
            {
                ((NixxisSupervisionTileItem)e.OldLagreItem).RemoveToolbarPanel();
            }
            if (e.NewLagreItem != null)
            {
                ((NixxisSupervisionTileItem)e.NewLagreItem).SetToolbarPanel();
                Application.Current.MainWindow.Title = e.NewLagreItem.Header.Title + " - " + m_MainWindowTitle;
            }
            else
            {
                Application.Current.MainWindow.Title = TranslationContext.Translate("Dashboard view - ") + m_MainWindowTitle;
            }

            if (((NixxisTileView)sender).DisplayMode == NixxisTileView.DisplayModes.MediumOnly)
                SupFrameSet.ShowCategory.State = string.Empty;
            else
                SupFrameSet.ShowCategory.State = e.NewLagreItem.Id;
        }

        void SupFrameSet_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as SupFrameSet).IsVisible)
            {
                SupFrameSet_Loaded(sender, null);
            }
            else
            {
                SupFrameSet_Unloaded(sender, null);
            }
        }
        void SupFrameSet_Loaded(object sender, RoutedEventArgs e)
        {

            Focus();
            NixxisBaseExpandPanel nep = null;
            nep = FindResource("CoverflowAgent") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.SetPanelCommand.CanExecute(nep))
                {
                    NixxisGrid.SetPanelCommand.Execute(nep);
                }
            }

            if (tileView1 != null)
            {
                if (tileView1.SelectedItem != null)
                {
                    ((NixxisSupervisionTileItem)tileView1.SelectedItem).SetToolbarPanel();
                }
            }

            nep = FindResource("MainMenuPanel") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.SetPanelCommand.CanExecute(nep))
                {
                    NixxisGrid.SetPanelCommand.Execute(nep);
                }
            } 
            
            nep = FindResource("InfoPanelStatusView") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                NixxisGrid.SetPanelCommand.Execute(nep);
            } 
            //
            //Telephony commands
            //
            NixxisBaseExpandPanel VoiceCommands = FindResource("MainMenuPanel") as NixxisBaseExpandPanel;
            NixxisBaseExpandPanel SupCommands = FindResource("SupervisionAgentToolbarPanel") as NixxisBaseExpandPanel;
            NixxisButton Button;


            if (VoiceCommands != null && SupCommands!=null) 
            {
                foreach (ICommand Command in m_SupervisionLink.ClientLink.Commands.StandardCommands)
                {
                    string CommandName = Command.Code.ToString().Replace("Command", "");

                    if ((Button = ((NixxisButton)VoiceCommands.FindByName(CommandName, true))) != null)
                        Button.Command = Command;
                    else if ((Button = ((NixxisButton)SupCommands.FindByName(CommandName, true))) != null)
                        Button.Command = Command;
                }
            }


            m_SupervisionLink.ClientLink.Commands.VoiceListen.BeforeExecute += VoiceListen_BeforeExecute;
            m_SupervisionLink.ClientLink.Commands.VoiceRecord.BeforeExecute += VoiceRecord_BeforeExecute;
            m_SupervisionLink.ClientLink.Commands.ViewScreen.BeforeExecute += ViewScreen_BeforeExecute;

        }

        bool VoiceListen_BeforeExecute(object parameter)
        {
            NixxisSupervisionTileItem item = (NixxisSupervisionTileItem)tileView1.GetItemById("agents");

            if (item != null)
            {
                Nixxis.ClientV2.AgentItem agt = (Nixxis.ClientV2.AgentItem)item.LastSelectedItem;


                if (m_SupervisionLink.ClientLink.Commands.VoiceListen.Active3S.GetValueOrDefault(true))
                    m_SupervisionLink.ClientLink.Commands.VoiceListen.Execute();
                else
                    m_SupervisionLink.ClientLink.Commands.VoiceListen.Execute(agt.Id);
            }

            return true;
        }

        bool ViewScreen_BeforeExecute(object parameter)
        {
            NixxisSupervisionTileItem item = (NixxisSupervisionTileItem)tileView1.GetItemById("agents");

            if (item != null)
            {
                Nixxis.ClientV2.AgentItem agt = (Nixxis.ClientV2.AgentItem)item.LastSelectedItem;





                if (m_SupervisionLink.ClientLink.Commands.ViewScreen.Active3S.GetValueOrDefault(true))
                {
                    System.Diagnostics.Trace.WriteLine("ViewScreen stop requested", DateTime.Now.ToString());
                    m_SupervisionLink.ClientLink.Commands.ViewScreen.Execute();
                }
                else
                {

                    List<string> parameters = new List<string>();
                    foreach (System.Net.NetworkInformation.NetworkInterface Nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (Nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && Nic.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                        {
                            foreach (System.Net.NetworkInformation.UnicastIPAddressInformation Addr in Nic.GetIPProperties().UnicastAddresses)
                            {
                                if (Addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    parameters.Add(string.Concat(Addr.Address.ToString(), "/", Addr.IPv4Mask.ToString()));
                                }
                            }
                        }
                    }
                    parameters.Insert(0, agt.Id);

                    m_SupervisionLink.ClientLink.Commands.ViewScreen.Execute(parameters.ToArray());
                }
            }

            return true;
        }


        bool VoiceRecord_BeforeExecute(object parameter)
        {
            NixxisSupervisionTileItem item = (NixxisSupervisionTileItem)tileView1.GetItemById("agents");

            if (item != null)
            {
                Nixxis.ClientV2.AgentItem agt = (Nixxis.ClientV2.AgentItem)item.LastSelectedItem;


                if (m_SupervisionLink.ClientLink.Commands.VoiceRecord.Active3S.HasValue)
                {
                    if(m_SupervisionLink.ClientLink.Commands.VoiceRecord.Active3S.Value)
                    {
                        m_SupervisionLink.ClientLink.Commands.VoiceRecord.Execute("False", agt.Id);
                    }
                    else
                    {
                        m_SupervisionLink.ClientLink.Commands.VoiceRecord.Execute("True", agt.Id);
                    }
                }
                else
                    m_SupervisionLink.ClientLink.Commands.VoiceRecord.Execute("null", agt.Id);


            }

            return true;
        }

        void SupFrameSet_Unloaded(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < Application.Current.Windows.Count; i++)
            {
                if (Application.Current.Windows[i] is StandAloneWindow && ((StandAloneWindow)Application.Current.Windows[i]).Content is DashboardWidgetsContainer)
                {
                    Application.Current.Windows[i].Close();
                    i--;
                }
                if (Application.Current.Windows[i] is NixxisViewAgentScreen )
                {
                    Application.Current.Windows[i].Close();
                    i--;
                }
            }



            if (m_UserChatWindow != null)
            {
                try
                {
                    m_UserChatWindow.Close();
                    m_UserChatWindow = null;
                }
                catch { }
            }

            if (Application.Current.MainWindow!=null)
            Application.Current.MainWindow.Title = m_MainWindowTitle; 

            NixxisBaseExpandPanel nep = null;
            nep = FindResource("CoverflowAgent") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                    NixxisGrid.RemovePanelCommand.Execute(nep);
            }

            if (tileView1 != null)
            {
                if (tileView1.SelectedItem != null)
                {
                    ((NixxisSupervisionTileItem)tileView1.SelectedItem).RemoveToolbarPanel();
                }
            }

            nep = FindResource("MainMenuPanel") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                if (NixxisGrid.RemovePanelCommand.CanExecute(nep))
                    NixxisGrid.RemovePanelCommand.Execute(nep);
            }

            nep = FindResource("InfoPanelStatusView") as NixxisBaseExpandPanel;
            if (nep != null)
            {
                nep.DataContext = this;

                NixxisGrid.RemovePanelCommand.Execute(nep);
            }
        }

        #region Telephone function
        void ClientLink_ContactStateChanged(ContactEventArgs eventArgs)
        {
            if (eventArgs.Contact.Media == ContactMedia.Voice && eventArgs.Contact.State == 'D')
            {
                if (m_SupervisionLink.ClientLink.Contacts.Contains(eventArgs.Contact))
                {
                    m_SupervisionLink.ClientLink.Commands.TerminateContact.ExecuteDirect((string[])null,eventArgs.ContactId);
                }
            }
        }
        
        void ClientLink_ContactAdded(ContactEventArgs eventArgs)
        {
            if (string.IsNullOrEmpty(m_SupervisionLink.ClientLink.Contacts.ActiveContactId) || eventArgs.Contact.Media == ContactMedia.Voice)
                m_SupervisionLink.ClientLink.Contacts.ActiveContact = eventArgs.Contact;
        }

        void ClientLink_ContactRemoved(ContactEventArgs eventArgs)
        {
            
        }

        private string VoiceNewCall_RequestDestination(string defaultDestination, string title)
        {
            try
            {
                VoiceNewCallWindow Dlg = new VoiceNewCallWindow();
                Dlg.SetItemSource(m_SupervisionLink.ClientLink.WellKnownCallDestinations);
                Dlg.SetDefaultValue(defaultDestination);
                Dlg.Title = title;

                if (Application.Current.MainWindow.IsLoaded)
                    Dlg.Owner = Application.Current.MainWindow;

                bool? DlgResult = Dlg.ShowDialog();

                if (DlgResult.HasValue && DlgResult.Value && Dlg.SelectedItem != null)
                {
                    return Dlg.SelectedItem;
                }
                else
                    return null;
            }
            catch (Exception Ex)
            {
                System.Diagnostics.Trace.WriteLine(Ex.ToString());
            }

            return null;
        }
        
        #endregion

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == SupFrameSet.ShowCategory)
            {
                switch (e.Parameter as string)
                {
                    case "agents":
                        SupAgentPanel.OnSelectItem();
                        break;
                    case "inbounds":
                        SupInboundPanel.OnSelectItem(); ;
                        break;
                    case "outbounds":
                        SupOutboundPanel.OnSelectItem();
                        break;
                    case "queues":
                        SupQueuePanel.OnSelectItem();
                        break;
                    case "campaigns":
                        SupCampaignPanel.OnSelectItem();
                        break;
                    case "alerts":
                        SupAlertPanel.OnSelectItem();
                        break;
                    case "dashboards":
                        SupDashboardPanel.OnSelectItem();
                        break;
                }

            }
            else if (e.Command == SupFrameSet.SupViewShowcolumnSelectorOperation)
            {
                if (Window.GetWindow(this) == Application.Current.MainWindow)
                {
                    System.Diagnostics.Trace.WriteLine("Execute main window");
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Execute other window");
                }


                NixxisSupervisionTileItem item = (NixxisSupervisionTileItem)tileView1.GetItemById(e.Parameter as string);
                if(item!=null)
                item.ShowColumnSelector();
            }
            else if (e.Command == SupFrameSet.SupViewAgentSendMsg)
            {
                #region Send Msg to agent
                //
                //Popup window to request action to do
                //
                NixxisSendMessageRequest frm = new NixxisSendMessageRequest();
                if (tileView1.SelectedItem.Id == "agents")
                {
                    frm.ToAgents = true;
                    frm.ToTeams = false;
                }
                else
                {
                    frm.ToAgents = false;
                    frm.ToTeams = false;
                }

                if (Application.Current.MainWindow.IsLoaded)
                    frm.Owner = Application.Current.MainWindow;

                frm.ShowDialog();

                if (frm.DialogResult == true)
                {
                    NixxisSupervisionTileItem item = (NixxisSupervisionTileItem)tileView1.SelectedItem;

                    if (tileView1.SelectedItem.Id == "agents")
                    {
                        if (frm.radioOpenChat.IsChecked == true)
                        {
                            if (item == null || item.LastSelectedItem == null)
                                return;

                            Nixxis.ClientV2.AgentItem agt = (Nixxis.ClientV2.AgentItem)item.LastSelectedItem;

                            if (string.IsNullOrEmpty(agt.Id))
                                return;

                            m_UserChatCollection.AddUnique(new NixxisUserChatItem { UserId = agt.Id, UserDescription = agt.Account + ", " + agt.Firstname + " " + agt.Lastname });

                            NixxisUserChatItem chatItem = m_UserChatCollection.GetItem(agt.Id);
                            chatItem.Conversation.AddLine(chatItem.UserDescription, frm.SendText, NixxisUserChatLine.ChatLineOriginators.User);
                            m_SupervisionLink.ClientLink.SendMessage(MessageType.Chat, MessageDestinations.Agent, agt.Id, frm.SendText);

                            ShowUserChatWindow();
                        }
                        else if (frm.radioSendToSelectedAgent.IsChecked == true)
                        {
                            if (item == null || item.LastSelectedItem == null)
                                return;

                            Nixxis.ClientV2.AgentItem agt = (Nixxis.ClientV2.AgentItem)item.LastSelectedItem;

                            if (string.IsNullOrEmpty(agt.Id))
                                return;

                            m_SupervisionLink.ClientLink.SendMessage(MessageType.Default, MessageDestinations.Agent, agt.Id, frm.SendText);
                        }
                        else if (frm.radioSendToAllAgents.IsChecked == true)
                        {
                            m_SupervisionLink.ClientLink.SendMessage(MessageType.Default, MessageDestinations.Agent, "", frm.SendText);
                        }
                    }
                    else
                    {
                        if (frm.radioOpenChat.IsChecked == true)
                        {
                            ShowUserChatWindow();
                        }
                    }
                }

                
                #endregion
            }
            else if (e.Command == SupFrameSet.SupWorkspaceOpen)
            {
                #region Workspace open

                SavedContextsOpenDialog dlg = new SavedContextsOpenDialog();
                dlg.Owner = Application.Current.MainWindow;
                
                dlg.DataContext = m_SavedContexts;

                if(dlg.ShowDialog().GetValueOrDefault())
                {
                    XmlDocument doc =  (dlg.lstContexts.SelectedValue as SavedContext).Content;
                    WorkSpace ws = new WorkSpace();
                    ws.LoadXml(doc.InnerXml);

                    ws.SetTileItem(SupAgentPanel);
                    ws.SetTileItem(SupAlertPanel);
                    ws.SetTileItem(SupCampaignPanel);
                    ws.SetTileItem(SupQueuePanel);
                    ws.SetTileItem(SupInboundPanel);
                    ws.SetTileItem(SupOutboundPanel);
                    ws.SetTileItem(SupDashboardPanel);
                    

                    Application.Current.MainWindow.Height = ws.GetWindowHeight(-1);
                    Application.Current.MainWindow.Width = ws.GetWindowWidth(-1);
                    Application.Current.MainWindow.Left = ws.GetWindowLeft(-1);
                    Application.Current.MainWindow.Top = ws.GetWindowTop(-1);
                    Application.Current.MainWindow.WindowState = (WindowState)(ws.GetWindowState(-1));




                    for (int i = 0; i < Application.Current.Windows.Count; i++)
                    {
                        if(Application.Current.Windows[i] is StandAloneWindow && ((StandAloneWindow)Application.Current.Windows[i]).Content is DashboardWidgetsContainer )
                        {
                            Application.Current.Windows[i].Close();
                            i--;
                        }
                    }


                    int counter = 0;
                    while (true)
                    {
                        XmlNode n = ws.GetWindowContent(counter);
                        if (n == null)
                            break;


                        StandAloneWindow wnd = new StandAloneWindow();

                        DashboardWidgetsContainer dwc = new DashboardWidgetsContainer();

                        dwc.DataContext = m_SupervisionLink;

                        wnd.Content = dwc;

                        wnd.Show();

                        wnd.WindowState = WindowState.Normal;

                        wnd.Height = ws.GetWindowHeight(counter);

                        wnd.Width = ws.GetWindowWidth(counter);

                        wnd.Left = ws.GetWindowLeft(counter);

                        wnd.Top = ws.GetWindowTop(counter);

                        wnd.WindowState = (WindowState)ws.GetWindowState(counter);


                        XmlDocument tempdoc = new XmlDocument();
                        tempdoc.AppendChild( tempdoc.ImportNode(n, true));

                        dwc.LoadDashboard(tempdoc);

                        counter++;
                    }


                    if (ws.CurrentWindow != null)
                    {
                        if (((string)ShowCategory.State) == ws.CurrentWindow)
                        {
                        }
                        else
                        {
                            if (ws.CurrentWindow == string.Empty)
                            {
                                switch (ShowCategory.State as string)
                                {
                                    case "agents":
                                        SupAgentPanel.OnSelectItem();
                                        break;
                                    case "inbounds":
                                        SupInboundPanel.OnSelectItem(); ;
                                        break;
                                    case "outbounds":
                                        SupOutboundPanel.OnSelectItem();
                                        break;
                                    case "queues":
                                        SupQueuePanel.OnSelectItem();
                                        break;
                                    case "campaigns":
                                        SupCampaignPanel.OnSelectItem();
                                        break;
                                    case "alerts":
                                        SupAlertPanel.OnSelectItem();
                                        break;
                                    case "dashboards":
                                        SupDashboardPanel.OnSelectItem();
                                        break;
                                }

                            }
                            else
                            {
                                switch (ws.CurrentWindow)
                                {
                                    case "agents":
                                        SupAgentPanel.OnSelectItem();
                                        break;
                                    case "inbounds":
                                        SupInboundPanel.OnSelectItem(); ;
                                        break;
                                    case "outbounds":
                                        SupOutboundPanel.OnSelectItem();
                                        break;
                                    case "queues":
                                        SupQueuePanel.OnSelectItem();
                                        break;
                                    case "campaigns":
                                        SupCampaignPanel.OnSelectItem();
                                        break;
                                    case "alerts":
                                        SupAlertPanel.OnSelectItem();
                                        break;
                                    case "dashboards":
                                        SupDashboardPanel.OnSelectItem();
                                        break;
                                }
                            }
                            ShowCategory.State = ws.CurrentWindow;
                        }
                    }
                }

                return;

                #endregion
            }
            else if (e.Command == SupFrameSet.SupWorkspaceSave)
            {
                #region Workspace Save

                SavedContextsSaveDialog dlg = new SavedContextsSaveDialog();

                dlg.Owner = Application.Current.MainWindow;
                dlg.DataContext = m_SavedContexts;


                if(dlg.ShowDialog().GetValueOrDefault())
                {
                    SavedContext context = null;

                    if(dlg.radioNew.IsChecked.GetValueOrDefault())
                    {
                        context = m_SavedContexts.Add(dlg.TxtName.Text, dlg.IsShared.IsChecked.GetValueOrDefault());
                    }
                else
                    {
                        context = (dlg.lstContexts.SelectedValue as SavedContext);                        
                    }

                    WorkSpace workspace = new WorkSpace();
                    workspace.Id = context.Id;
                    workspace.ModifyDateTime = DateTime.Now;
                    workspace.GetTileItem(SupAgentPanel);
                    workspace.GetTileItem(SupAlertPanel);
                    workspace.GetTileItem(SupCampaignPanel);
                    workspace.GetTileItem(SupQueuePanel);
                    workspace.GetTileItem(SupInboundPanel);
                    workspace.GetTileItem(SupOutboundPanel);
                    workspace.GetTileItem(SupDashboardPanel);
                    workspace.CurrentWindow = ShowCategory.State as string;

                    workspace.SetWindowHeight(-1, Application.Current.MainWindow.Height);
                    workspace.SetWindowWidth(-1,Application.Current.MainWindow.Width);
                    workspace.SetWindowLeft(-1, Application.Current.MainWindow.Left);
                    workspace.SetWindowTop(-1, Application.Current.MainWindow.Top);

                    workspace.SetWindowState(-1,(int)Application.Current.MainWindow.WindowState);


                    int counter = 0;
                    foreach (Window wnd in Application.Current.Windows)
                    {
                        if (wnd is StandAloneWindow)
                        {
                            StandAloneWindow sawnd = (StandAloneWindow)wnd;
                            if (sawnd.Content is DashboardWidgetsContainer)
                            {
                                // save window position and size...

                                workspace.SetWindowHeight(counter, wnd.Height);
                                workspace.SetWindowWidth(counter , wnd.Width);
                                workspace.SetWindowLeft(counter, wnd.Left);
                                workspace.SetWindowTop(counter, wnd.Top);
                                workspace.SetWindowState(counter, (int)wnd.WindowState);

                                workspace.SetWindowContent(counter, ((DashboardWidgetsContainer)sawnd.Content).SaveDashboard().DocumentElement);

                                counter++;
                            }
                        }
                    }

                    context.Content = workspace.XmlDoc;

                }

                #endregion
            }
            else if (e.Command == DoNewCall)
            {
                #region DoNewCall
                // New manual call

                string destination = VoiceNewCall_RequestDestination("", TranslationContext.Translate("Manual dialing..."));

                if (destination != null)
                    m_SupervisionLink.ClientLink.Commands.VoiceNewCall.Execute(destination, m_SupervisionLink.ClientLink.Contacts.ActiveContactId);

                #endregion
            }
        }



        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
            if (e.Command == SupFrameSet.ShowCategory)
            {
                e.CanExecute = true; 
            }
            else if (e.Command == SupFrameSet.SupViewShowcolumnSelectorOperation)
            {
                e.CanExecute = true;
            }
            else if (e.Command == SupFrameSet.SupViewAgentSendMsg)
            {
                e.CanExecute = SupAgentPanel.LastSelectedItem == null ? false : true;
            }
            else if (e.Command == SupFrameSet.SupWorkspaceOpen)
            {
                e.CanExecute = true;
            }
            else if (e.Command == SupFrameSet.SupWorkspaceSave)
            {
                e.CanExecute = true;
            }
            else if (e.Command == SupFrameSet.DoNewCall)
            {
                e.CanExecute = true;
            }
            }
            catch
            {
                e.CanExecute = false;
            }

            e.Handled = true;
        }

        private void SelectionChanged(object sender, RoutedEventArgs args)
        {
            System.Diagnostics.Trace.WriteLine("Selection changed");

            if (m_SupervisionLink == null)
                return;

            SupervisionItem obj = (sender as NixxisSupervisionTileItem).LastSelectedItem as SupervisionItem;
            
            if(obj!=null)
            {
                string extractedType = obj.GetType().Name;
                extractedType = extractedType.Substring(0, extractedType.Length - "Item".Length);
                int column = (sender as NixxisSupervisionTileItem).LastSelectedColumn;
                m_SupervisionLink.ClientLink.Commands.SupervisionSelect.Execute(new string[] { extractedType, obj.Id, column.ToString() });
            }
            else
            {
                m_SupervisionLink.ClientLink.Commands.SupervisionSelect.Execute(new string[] { null, null, null });

            }

        }
    }
}
