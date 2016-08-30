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
using System.Collections;
using System.Collections.Specialized;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Interaction logic for OverflowHelper.xaml
    /// </summary>
    public partial class OutboundClosingHelper : UserControl
    {
        public static readonly DependencyProperty OutboundClosingActionTypeProperty = DependencyProperty.Register("OutboundClosingActionType", typeof(object), typeof(OutboundClosingHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault) );
        public static readonly DependencyProperty OutboundClosingDialingModeProperty = DependencyProperty.Register("OutboundClosingDialingMode", typeof(DialingMode), typeof(OutboundClosingHelper), new FrameworkPropertyMetadata(DialingMode.Progressive, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(OutboundClosingHelper));
        


        public object OutboundClosingActionType
        {
            get
            {
                return GetValue(OutboundClosingActionTypeProperty);
            }
            set
            {
                SetValue(OutboundClosingActionTypeProperty, value);
            }
        }
        public DialingMode OutboundClosingDialingMode
        {
            get
            {
                return (DialingMode)GetValue(OutboundClosingDialingModeProperty);
            }
            set
            {
                SetValue(OutboundClosingDialingModeProperty, value);
            }
        }
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public OutboundClosingHelper()
        {
            InitializeComponent();
        }

    }

}
