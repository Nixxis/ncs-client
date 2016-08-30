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

namespace Nixxis.Client.Controls
{
    /// <summary>
    /// Interaction logic for CheckBoxWithDetails.xaml
    /// </summary>
    public partial class NixxisDetailedCheckBox : UserControl
    {
        public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent("Checked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisDetailedCheckBox));
        public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent("Unchecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisDetailedCheckBox));


        public static readonly DependencyProperty DetailVisibilityProperty = DependencyProperty.Register("DetailVisibility", typeof(Visibility), typeof(NixxisDetailedCheckBox), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(NixxisDetailedCheckBox), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(IsCheckedChanged)));
        public static new readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(NixxisDetailedCheckBox), new PropertyMetadata(null));
        public static readonly DependencyProperty DetailContentProperty = DependencyProperty.Register("DetailContent", typeof(object), typeof(NixxisDetailedCheckBox), new PropertyMetadata(null));
        public static readonly DependencyProperty IsDetailCheckedProperty = DependencyProperty.Register("IsDetailChecked", typeof(bool), typeof(NixxisDetailedCheckBox), new PropertyMetadata(false));

        public event RoutedEventHandler Checked
        {
            add { AddHandler(CheckedEvent, value); }
            remove { RemoveHandler(CheckedEvent, value); }
        }
        public event RoutedEventHandler Unchecked
        {
            add { AddHandler(UncheckedEvent, value); }
            remove { RemoveHandler(UncheckedEvent, value); }
        }


        public static void IsCheckedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisDetailedCheckBox ndcb = (NixxisDetailedCheckBox)obj;
            if (!(bool)args.NewValue)
            {
                ndcb.IsDetailChecked = false;

                ndcb.RaiseEvent(new RoutedEventArgs(UncheckedEvent));
            }
            else
            {
                ndcb.RaiseEvent(new RoutedEventArgs(CheckedEvent));
            }
        }


        public Visibility DetailVisibility
        {
            get
            {
                return (Visibility)GetValue(DetailVisibilityProperty);
            }
            set
            {
                SetValue(DetailVisibilityProperty, value);
            }
        }
        public bool IsDetailChecked
        {
            get
            {
                return (bool)GetValue(IsDetailCheckedProperty);
            }
            set
            {
                SetValue(IsDetailCheckedProperty, value);
            }
        }
        public object DetailContent
        {
            get
            {
                return GetValue(DetailContentProperty);
            }
            set
            {
                SetValue(DetailContentProperty, value);
            }
        }
        public bool IsChecked
        {
            get
            {
                return (bool)GetValue(IsCheckedProperty);
            }
            set
            {
                SetValue(IsCheckedProperty, value);
            }
        }
        public new object Content
        {
            get
            {
                return GetValue(ContentProperty);
            }
            set
            {
                SetValue(ContentProperty, value);
            }
        }

        static NixxisDetailedCheckBox()
        {
        }

        public NixxisDetailedCheckBox()
        {
            InitializeComponent();

            Binding.AddSourceUpdatedHandler(this, new EventHandler<DataTransferEventArgs>(MainCheckbox_SourceUpdated));
        }

        private void MainCheckbox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if(IsChecked)
                IsDetailChecked = true;
        }

    }
}
