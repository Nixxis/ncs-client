using ContactRoute;
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

namespace Nixxis.RfbClient
{
    /// <summary>
    /// Interaction logic for VncViewer.xaml
    /// </summary>
    public partial class Viewer : UserControl, IViewClientAddon
    {
        public Viewer()
        {


            InitializeComponent();

        }

        private void MenuItemAuto_Click(object sender, RoutedEventArgs e)
        {
            (wfh.Child as Nixxis.RfbClient.RemoteDesktop).SetScalingMode(true);
        }
        private void MenuItem100_Click(object sender, RoutedEventArgs e)
        {
            (wfh.Child as Nixxis.RfbClient.RemoteDesktop).SetScalingMode(false);
        }
        private void RemoteDesktop_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & System.Windows.Forms.MouseButtons.Right) == System.Windows.Forms.MouseButtons.Right)
                wfh.ContextMenu.IsOpen = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                (wfh.Child as Nixxis.RfbClient.RemoteDesktop).GetPassword = (() => ("NixPass"));
                (wfh.Child as Nixxis.RfbClient.RemoteDesktop).Connect(ConnectionInfo, true);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

        }

        private string m_ConnectionInfo = null;

        public string ConnectionInfo
        {
            get
            {
                return m_ConnectionInfo;
            }
            set
            {
                m_ConnectionInfo = value;
            }
        }
    }
}
