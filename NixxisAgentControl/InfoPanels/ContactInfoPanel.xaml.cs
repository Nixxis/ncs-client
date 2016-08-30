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
    /// Interaction logic for ContactInfo.xaml
    /// </summary>
    public partial class ContactInfoPanel : UserControl
    {
        #region Class data
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ContactProperty = DependencyProperty.Register("Contact", typeof(ContactInfo), typeof(ContactInfoPanel), new FrameworkPropertyMetadata(ContactPropertyChanged));
        public ContactInfo Contact
        {
            get { return (ContactInfo)GetValue(ContactProperty); }
            set { SetValue(ContactProperty, value); }
        }
        public static void ContactPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(ContactInfoPanel), new FrameworkPropertyMetadata(IsCheckedPropertyChanged));
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
        public static void IsCheckedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty ControlProperty = DependencyProperty.Register("Control", typeof(FrameworkElement), typeof(ContactInfoPanel));
        public FrameworkElement Control
        {
            get { return (FrameworkElement)this.GetValue(ControlProperty); }
            set { this.SetValue(ControlProperty, value); }
        }
        #endregion

        #region Constructors
        public ContactInfoPanel()
        {
            InitializeComponent();
        }

        public ContactInfoPanel(ContactInfo info)
        {
            Contact = info;

            InitializeComponent();
        }
        #endregion

        private void MySelf_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.IsChecked = !this.IsChecked;
        }

        #region Members

        #endregion
    }
}
