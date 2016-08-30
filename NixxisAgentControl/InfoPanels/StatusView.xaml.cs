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

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for StatusView.xaml
    /// </summary>
    public partial class StatusView : NixxisPanelSelectorItem
    {
        #region Class data
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(string), typeof(StatusView), new FrameworkPropertyMetadata("---", StatePropertyChanged));
        public static readonly DependencyProperty StateDurationProperty = DependencyProperty.Register("StateDuration", typeof(string), typeof(StatusView), new FrameworkPropertyMetadata("---", StateDurationPropertyChanged));
        public static readonly DependencyProperty ContactProperty = DependencyProperty.Register("Contact", typeof(ContactInfo), typeof(StatusView), new FrameworkPropertyMetadata(ContactPropertyChanged));
        
        public ContactInfo Contact
        {
            get { return (ContactInfo)GetValue(ContactProperty); }
            set { SetValue(ContactProperty, value); }
        }
        public string State
        {
            get
            {
                return (string)GetValue(StateProperty);
            }
            set
            {
                SetValue(StateProperty, value);
            }
            
        }
        public string StateDuration
        {
            get
            {
                return (string)GetValue(StateDurationProperty);
            }
            set
            {
                SetValue(StateDurationProperty, value);
            }

        }

        public static void StatePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        public static void StateDurationPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        public static void ContactPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        #region Constructors
        public StatusView()
        {
            InitializeComponent();
        }

        public StatusView(HttpLink link, ContactInfo info)
        {
            ClientLink = link;
            Contact = info;

            InitializeComponent();
        }
        #endregion

        #region Members

        #endregion
    }
}
