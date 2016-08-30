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
    /// Interaction logic for LoginPanelControl.xaml
    /// </summary>
    public partial class LoginPanelControl : UserControl
    {
        #region Class data
        string m_LastUrl;
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ClientLinkProperty = DependencyProperty.Register("ClientLink", typeof(HttpLink), typeof(LoginPanelControl), new FrameworkPropertyMetadata(ClientLinkPropertyChanged));
        public HttpLink ClientLink
        {
            get { return (HttpLink)GetValue(ClientLinkProperty); }
            set { SetValue(ClientLinkProperty, value); }
        }
        public static void ClientLinkPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty ContactProperty = DependencyProperty.Register("Contact", typeof(ContactInfo), typeof(LoginPanelControl), new FrameworkPropertyMetadata(ContactPropertyChanged));
        public ContactInfo Contact
        {
            get { return (ContactInfo)GetValue(ContactProperty); }
            set { SetValue(ContactProperty, value); }
        }
        public static void ContactPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        #region Constructors
        public LoginPanelControl()
        {
            InitializeComponent();
        }

        public LoginPanelControl(HttpLink link, ContactInfo info)
        {
            ClientLink = link;
            Contact = info;

            if(Contact != null)
                m_LastUrl = Contact.Script;

            InitializeComponent();
        }
        #endregion

        #region Members
        public void Navigate(string url, bool force)
        {
            if(force || !string.Equals(url, m_LastUrl))
                webscript.Navigate(url);
        }
        #endregion
    }
}
