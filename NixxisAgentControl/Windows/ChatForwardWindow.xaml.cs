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
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for MailForwardWindow.xaml
    /// </summary>
    public partial class ChatForwardWindow : Window
    {
        private HttpLink m_ClientLink;
        private string m_LastNumber = null;
        private ObservableCollection<Tuple<string, string, bool>> m_DestinationsList = new ObservableCollection<Tuple<string, string, bool>>();


        public static readonly DependencyProperty DestinationSourceProperty = DependencyProperty.Register("DestinationSource", typeof(object), typeof(MailForwardWindow), new PropertyMetadata(new PropertyChangedCallback(DestinationSourceChanged)));
        public object DestinationSource
        {
            get { return (object)GetValue(DestinationSourceProperty); }
            set { SetValue(DestinationSourceProperty, value); }
        }
        public static void DestinationSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public string Destination
        {
            get
            {
                if (cboDestination.SelectedIndex >= 0)
                {
                    return cboDestination.SelectedValue as string;
                }
                else
                {
                    return cboDestination.Text;
                }
            }
        }

        public ObservableCollection<Tuple<string, string, bool>> DestinationsList
        {
            get
            {
                return m_DestinationsList;
            }
            set
            {
                m_DestinationsList = value;
            }
        }

        public HttpLink ClientLink
        {
            get
            {
                return m_ClientLink;
            }
            set
            {
                m_ClientLink = value;
                PrepareDestinationsList();
            }
        }

        public ChatForwardWindow()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();            
        }





        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();

            foreach (Tuple<string, string, bool> t in DestinationsList)
            {
                if (t.Item3)
                    return;
            }


            if (!string.IsNullOrEmpty(Destination))
            {
                m_LastNumber = Destination;
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
                    using (Microsoft.Win32.RegistryKey ClientKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName, true))
                    {
                        if (ClientKey != null)
                        {
                            string[] LastDest = ClientKey.GetValue("LastChatForwardDestinations", new string[0]) as string[];
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

                            ClientKey.SetValue("LastChatForwardDestinations", LastDest);

                            ClientKey.Close();
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void PrepareDestinationsList()
        {
            List<string> ListDestinations = new List<string>(ClientLink.WellKnownCallDestinations);
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
                using (Microsoft.Win32.RegistryKey ClientKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName, false))
                {
                    if (ClientKey != null)
                    {
                        string[] LastDest = ClientKey.GetValue("LastChatForwardDestinations", new string[0]) as string[];

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

            foreach (string str in ListDestinations)
            {
                DestinationsList.Add(new Tuple<string, string, bool>(str, str, false));
            }

        }

        private void cboDestination_DropDownOpened(object sender, EventArgs e)
        {

            for (int i = 0; i < DestinationsList.Count; i++)
            {
                if (DestinationsList[i].Item3)
                {
                    DestinationsList.RemoveAt(i);
                    i--;
                }
            }

            foreach (ExternalAgentInfo agi in m_ClientLink.GetTeamMembers(m_ClientLink.AgentId))
            {
                DestinationsList.Add(new Tuple<string, string, bool>(agi.AgentId, agi.Description, true));
            }

            
        }

        
    }
}
