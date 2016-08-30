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
    public partial class NixxisDetailedToggle : UserControl
    {
        public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent("Checked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisDetailedToggle));
        public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent("Unchecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisDetailedToggle));


        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(NixxisDetailedToggle), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(IsCheckedChanged) ));
        public static new readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(NixxisDetailedToggle), new PropertyMetadata(null));
        public static readonly DependencyProperty DetailContentProperty = DependencyProperty.Register("DetailContent", typeof(object), typeof(NixxisDetailedToggle), new PropertyMetadata(null));

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
            NixxisDetailedToggle ndcb = (NixxisDetailedToggle)obj;
            if (!(bool)args.NewValue)
            {
                ndcb.RaiseEvent(new RoutedEventArgs(UncheckedEvent));
            }
            else
            {
                ndcb.RaiseEvent(new RoutedEventArgs( CheckedEvent));
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

        static NixxisDetailedToggle()
        {
        }

        public NixxisDetailedToggle()
        {
            InitializeComponent();
        }

    }
}
