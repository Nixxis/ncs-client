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

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for MailForwardWindow.xaml
    /// </summary>
    public partial class MailForwardWindow : Window
    {
        #region Properties XAML
        public static readonly DependencyProperty DestinationSourceProperty = DependencyProperty.Register("DestinationSource", typeof(object), typeof(MailForwardWindow), new PropertyMetadata(new PropertyChangedCallback(DestinationSourceChanged)));
        public object DestinationSource
        {
            get { return (object)GetValue(DestinationSourceProperty); }
            set { SetValue(DestinationSourceProperty, value); }
        }
        public static void DestinationSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        #region Properties

        public string Destination
        {
            get { return MailForwardCrtl.Destination; }
        }

        public DateTime Delay
        {
            get { return MailForwardCrtl.Delay; }
        }

        public bool SendResponseNow
        {
            get { return MailForwardCrtl.SendResponseNow; }
        }
        #endregion

        #region Constructors
        public MailForwardWindow()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }
        #endregion

        #region Members
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void btnSpam_Click(object sender, RoutedEventArgs e)
        {
            MailForwardCrtl.Destination = "@SPAM";
            this.DialogResult = true;
            Close();
        }
        #endregion

        
    }
}
