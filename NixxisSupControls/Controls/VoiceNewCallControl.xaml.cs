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

namespace Nixxis.Client.Supervisor
{
    /// <summary>
    /// Interaction logic for ManualCallControl.xaml
    /// </summary>
    public partial class VoiceNewCallControl : UserControl
    {
        public event VoiceNewCallDialHandeler DialRequest;
        public void OnDialRequest(string destination)
        {
            if (DialRequest != null)
                DialRequest(destination);
        }
            
        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register("ItemSource", typeof(object), typeof(VoiceNewCallControl), new PropertyMetadata(new PropertyChangedCallback(ItemSourceChanged)));
        public object ItemSource
        {
            get { return (object)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }
        public static void ItemSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(VoiceNewCallControl), new PropertyMetadata(new PropertyChangedCallback(SelectedItemChanged)));
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        public VoiceNewCallControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cboDestination.Text != null)
            {
                if (cboDestination.Text.Length < 100)
                    cboDestination.Text += ((Button)sender).Tag.ToString();
            }
            else
            {
                cboDestination.Text = ((Button)sender).Tag.ToString();
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(cboDestination.Text))
                cboDestination.Text = cboDestination.Text.Substring(0, cboDestination.Text.Length - 1);
        }

        private void ButtonDial_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedItem = cboDestination.Text;
            OnDialRequest(this.SelectedItem as string);
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                this.SelectedItem = e.AddedItems[0];
            }
            else
                this.SelectedItem = cboDestination.Text;
        }

        private void WellKnowDestinationList_Filter(object sender, FilterEventArgs e)
        {
            string nr = e.Item as string;
            if (string.IsNullOrEmpty(nr) || nr.Contains("@"))
            {
                e.Accepted = false;
            }
            else
            {
                e.Accepted = true;
            }
        }
    }

    public delegate void VoiceNewCallDialHandeler(string destination);
}
