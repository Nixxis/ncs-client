using ContactRoute;
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
    /// Interaction logic for NixxisDoubleSlider.xaml
    /// </summary>
    public partial class NixxisDoubleSlider : UserControl
    {

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(NixxisDoubleSlider), new UIPropertyMetadata(0d));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(DoubleRange), typeof(NixxisDoubleSlider), new UIPropertyMetadata());

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(NixxisDoubleSlider), new UIPropertyMetadata(1d));


        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        
        public DoubleRange Value
        {
            get
            {
                return (DoubleRange)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);                
            }
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }


        public NixxisDoubleSlider()
        {
            InitializeComponent();
            this.Loaded += RangeSlider_Loaded;
            this.Unloaded += RangeSlider_Unloaded;

        }

        void RangeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            LowerSlider.ValueChanged += LowerSlider_ValueChanged;
            UpperSlider.ValueChanged += UpperSlider_ValueChanged;
        }

        void RangeSlider_Unloaded(object sender, RoutedEventArgs e)
        {
            LowerSlider.ValueChanged -= LowerSlider_ValueChanged;
            UpperSlider.ValueChanged -= UpperSlider_ValueChanged;
        }

        private void LowerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpperSlider.Value = Math.Max(UpperSlider.Value, LowerSlider.Value);
            if (LowerSlider.Value == 100)
            {
                Panel.SetZIndex(LowerSlider, 10);
                Panel.SetZIndex(UpperSlider, 9);
            }
            else
            {
                Panel.SetZIndex(LowerSlider, 9);
                Panel.SetZIndex(UpperSlider, 10);
            }
        }

        private void UpperSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
                LowerSlider.Value = Math.Min(UpperSlider.Value, LowerSlider.Value);
        }

        public override void BeginInit()
        {
            base.BeginInit();
            Value = new DoubleRange();
        }

    }
}
