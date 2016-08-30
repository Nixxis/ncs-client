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
    /// Interaction logic for MailForwardControl.xaml
    /// </summary>
    public partial class MailForwardControl : UserControl
    {
        #region Properties XAML
        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register("ItemSource", typeof(object), typeof(MailForwardControl), new PropertyMetadata(new PropertyChangedCallback(ItemSourceChanged)));
        public object ItemSource
        {
            get { return (object)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }
        public static void ItemSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty SelectedTextProperty = DependencyProperty.Register("SelectedText", typeof(string), typeof(MailForwardControl), new PropertyMetadata(new PropertyChangedCallback(SelectedTextChanged)));
        public string SelectedText
        {
            get { return (string)GetValue(SelectedTextProperty); }
            set { SetValue(SelectedTextProperty, value); }
        }
        public static void SelectedTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            MailForwardControl ctr = (MailForwardControl)obj;
            ctr.SetDestination();
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(MailForwardControl), new PropertyMetadata(new PropertyChangedCallback(SelectedItemChanged)));
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            MailForwardControl ctr = (MailForwardControl)obj;
            ctr.SetDestination();
        }

        public static readonly DependencyProperty DestinationProperty = DependencyProperty.Register("Destination", typeof(string), typeof(MailForwardControl), new PropertyMetadata(new PropertyChangedCallback(DestinationChanged)));
        public string Destination
        {
            get { return (string)GetValue(DestinationProperty); }
            set { SetValue(DestinationProperty, value); }
        }
        public static void DestinationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty IsDelayCheckedProperty = DependencyProperty.Register("IsDelayChecked", typeof(bool), typeof(MailForwardControl), new PropertyMetadata(new PropertyChangedCallback(IsDelayCheckedChanged)));
        public bool IsDelayChecked
        {
            get { return (bool)GetValue(IsDelayCheckedProperty); }
            set { SetValue(IsDelayCheckedProperty, value); }
        }
        public static void IsDelayCheckedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(DateTime), typeof(MailForwardControl), new PropertyMetadata(new PropertyChangedCallback(DelayChanged)));
        public DateTime Delay
        {
            get { return (DateTime)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }
        public static void DelayChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty DelayDateProperty = DependencyProperty.Register("DelayDate", typeof(DateTime), typeof(MailForwardControl), new PropertyMetadata(new DateTime(DateTime.Now.Year,DateTime.Now.Month, DateTime.Now.Day), new PropertyChangedCallback(DelayDateChanged)));
        public DateTime DelayDate
        {
            get { return (DateTime)GetValue(DelayDateProperty); }
            set { SetValue(DelayDateProperty, value); }
        }
        public static void DelayDateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            MailForwardControl ctrl = (MailForwardControl)obj;
            ctrl.SetDelay();
        }

        public static readonly DependencyProperty DelayTimeProperty = DependencyProperty.Register("DelayTime", typeof(TimeSpan), typeof(MailForwardControl), new PropertyMetadata(TimeSpan.Zero,new PropertyChangedCallback(DelayTimeChanged)));
        public TimeSpan DelayTime
        {
            get { return (TimeSpan)GetValue(DelayTimeProperty); }
            set { SetValue(DelayTimeProperty, value); }
        }
        public static void DelayTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            MailForwardControl ctrl = (MailForwardControl)obj;
            ctrl.SetDelay();
        }

        public static readonly DependencyProperty SendResponseNowProperty = DependencyProperty.Register("SendResponseNow", typeof(bool), typeof(MailForwardControl), new PropertyMetadata(new PropertyChangedCallback(SendResponseNowChanged)));
        public bool SendResponseNow
        {
            get { return (bool)GetValue(SendResponseNowProperty); }
            set { SetValue(SendResponseNowProperty, value); }
        }
        public static void SendResponseNowChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        #region Constructor
        public MailForwardControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Members
        private void SetDelay()
        {
            this.Delay = this.DelayDate.Add(this.DelayTime);
        }

        private void SetDestination()
        {
            if (this.SelectedItem == null)
            {
                this.Destination =this.SelectedText;
            }
            else
            {
                if (this.SelectedItem.GetType() == typeof(WellKnownDestinationItem))
                    this.Destination = ((WellKnownDestinationItem)this.SelectedItem).Destination;
                else
                    this.Destination = this.SelectedItem.ToString();
            }
        }

        private void WellKnowDestinationList_Filter(object sender, FilterEventArgs e)
        {
            WellKnownDestinationItem nr = e.Item as WellKnownDestinationItem;
            if (nr == null || !nr.Destination.Contains("@"))
            {
                e.Accepted = false;
            }
            else
            {
                e.Accepted = true;
            }
        }
        #endregion


    }
    public delegate void MailForwardEventHandler(object sender, MailForwardEventArg e);

    public class MailForwardEventArg
    {
        public string Action;
    }
}
