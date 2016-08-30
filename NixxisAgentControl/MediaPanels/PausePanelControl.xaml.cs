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

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for PausePanelControl.xaml
    /// </summary>
    public partial class PausePanelControl : UserControl
    {        
        
        #region Class data
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ClientLinkProperty = DependencyProperty.Register("ClientLink", typeof(HttpLink), typeof(PausePanelControl), new FrameworkPropertyMetadata(ClientLinkPropertyChanged));
        public HttpLink ClientLink
        {
            get { return (HttpLink)GetValue(ClientLinkProperty); }
            set { SetValue(ClientLinkProperty, value); }
        }
        public static void ClientLinkPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        #region Constructors
        public PausePanelControl()
        {
            InitializeComponent();
        }

        public PausePanelControl(HttpLink link)
        {
            ClientLink = link;

            InitializeComponent();
        }
        #endregion


        private void MySelf_Unloaded(object sender, RoutedEventArgs e)
        {
        }

    }
}
