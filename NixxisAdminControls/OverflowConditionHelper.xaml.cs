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

namespace Nixxis.Client.Admin
{
    /// <summary>
    /// Interaction logic for OverflowHelper.xaml
    /// </summary>
    public partial class OverflowConditionHelper : UserControl
    {
        public static readonly DependencyProperty OverflowConditionProperty = DependencyProperty.Register("OverflowCondition", typeof(object), typeof(OverflowConditionHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty OverflowConditionItemsInQueueThresholdProperty = DependencyProperty.Register("OverflowConditionItemsInQueueThreshold", typeof(int), typeof(OverflowConditionHelper), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty OverflowConditionAgentsReadySmallerThanThresholdProperty = DependencyProperty.Register("OverflowConditionAgentsReadySmallerThanThreshold", typeof(int), typeof(OverflowConditionHelper), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty OverflowConditionRatioInQueueAgentsProperty = DependencyProperty.Register("OverflowConditionRatioInQueueAgents", typeof(double), typeof(OverflowConditionHelper), new FrameworkPropertyMetadata(-1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty OverflowConditionMaxWaitProperty = DependencyProperty.Register("OverflowConditionMaxWait", typeof(int), typeof(OverflowConditionHelper), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty OverflowConditionMaxEWTProperty = DependencyProperty.Register("OverflowConditionMaxEWT", typeof(int), typeof(OverflowConditionHelper), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(OverflowConditionHelper));


        public object OverflowCondition
        {
            get
            {
                return GetValue(OverflowConditionProperty);
            }
            set
            {
                SetValue(OverflowConditionProperty, value);
            }
        }
        public int OverflowConditionItemsInQueueThreshold
        {
            get
            {
                return (int)GetValue(OverflowConditionItemsInQueueThresholdProperty);
            }
            set
            {

                SetValue(OverflowConditionItemsInQueueThresholdProperty, value);

            }
        }
        public int OverflowConditionAgentsReadySmallerThanThreshold
        {
            get
            {
                return (int)GetValue(OverflowConditionAgentsReadySmallerThanThresholdProperty);
            }
            set
            {

                SetValue(OverflowConditionAgentsReadySmallerThanThresholdProperty, value);

            }
        }
        public int OverflowConditionRatioInQueueAgents
        {
            get
            {
                return (int)GetValue(OverflowConditionRatioInQueueAgentsProperty);
            }
            set
            {
                SetValue(OverflowConditionRatioInQueueAgentsProperty, value);
            }
        }
        public int OverflowConditionMaxWait
        {
            get
            {
                return (int)GetValue(OverflowConditionMaxWaitProperty);
            }
            set
            {
                SetValue(OverflowConditionMaxWaitProperty, value);
            }
        }
        public int OverflowConditionMaxEWT
        {
            get
            {
                return (int)GetValue(OverflowConditionMaxEWTProperty);
            }
            set
            {
                SetValue(OverflowConditionMaxEWTProperty, value);
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

        public OverflowConditionHelper()
        {
            InitializeComponent();
        }
    }
}
