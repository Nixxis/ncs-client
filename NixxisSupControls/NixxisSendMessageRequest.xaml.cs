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

namespace Nixxis.Client.Supervisor
{
    /// <summary>
    /// Interaction logic for NixxisSendMessageRequest.xaml
    /// </summary>
    public partial class NixxisSendMessageRequest : Window
    {
        public NixxisSendMessageRequest()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        #region Properties XAML
        public static readonly DependencyProperty ToAgentsProperty = DependencyProperty.Register("ToAgents", typeof(bool), typeof(NixxisSendMessageRequest), new PropertyMetadata(false, new PropertyChangedCallback(ToAgentsChanged)));
        public bool ToAgents
        {
            get { return (bool)GetValue(ToAgentsProperty); }
            set { SetValue(ToAgentsProperty, value); }
        }
        public static void ToAgentsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty ToTeamsProperty = DependencyProperty.Register("ToTeams", typeof(bool), typeof(NixxisSendMessageRequest), new PropertyMetadata(false, new PropertyChangedCallback(ToTeamsChanged)));
        public bool ToTeams
        {
            get { return (bool)GetValue(ToTeamsProperty); }
            set { SetValue(ToTeamsProperty, value); }
        }
        public static void ToTeamsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty SendTextProperty = DependencyProperty.Register("SendText", typeof(string), typeof(NixxisSendMessageRequest), new PropertyMetadata(string.Empty, new PropertyChangedCallback(SendTextChanged)));
        public string SendText
        {
            get { return (string)GetValue(SendTextProperty); }
            set { SetValue(SendTextProperty, value); }
        }
        public static void SendTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        private void MySelf_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
