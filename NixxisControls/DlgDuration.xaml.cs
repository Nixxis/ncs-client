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

namespace Nixxis.Client.Controls
{
    /// <summary>
    /// Interaction logic for DlgDuration.xaml
    /// </summary>
    public partial class DlgDuration : Window
    {
        public static readonly DependencyProperty ShowSecondsProperty = DependencyProperty.Register("ShowSeconds", typeof(bool), typeof(DlgDuration), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowSignProperty = DependencyProperty.Register("ShowSign", typeof(bool), typeof(DlgDuration), new PropertyMetadata(false));
        public static readonly DependencyProperty ShowMillisecondsProperty = DependencyProperty.Register("ShowMilliseconds", typeof(bool), typeof(DlgDuration), new PropertyMetadata(false));
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(Decimal), typeof(DlgDuration), new PropertyMetadata(Decimal.Zero, new PropertyChangedCallback(DurationChanged)));
        public static readonly DependencyProperty DaysDurationProperty = DependencyProperty.Register("DaysDuration", typeof(Decimal), typeof(DlgDuration), new PropertyMetadata(Decimal.Zero));
        public static readonly DependencyProperty HoursDurationProperty = DependencyProperty.Register("HoursDuration", typeof(Decimal), typeof(DlgDuration), new PropertyMetadata(Decimal.Zero));
        public static readonly DependencyProperty MinutesDurationProperty = DependencyProperty.Register("MinutesDuration", typeof(Decimal), typeof(DlgDuration), new PropertyMetadata(Decimal.Zero));
        public static readonly DependencyProperty SecondsDurationProperty = DependencyProperty.Register("SecondsDuration", typeof(Decimal), typeof(DlgDuration), new PropertyMetadata(Decimal.Zero));

        public static void DurationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            DlgDuration dlg = sender as DlgDuration;

            Decimal temp = (Decimal)args.NewValue;

            bool isNegative = false;
            if (temp < 0)
            {
                isNegative = true;
                temp = Math.Abs(temp);
            }


            dlg.DaysDuration = Decimal.Floor(temp /24/60/60);
            temp -= dlg.DaysDuration * 24 *60 *60;
            dlg.HoursDuration = Decimal.Floor(temp / 60 /60);
            temp -= dlg.HoursDuration * 60 * 60;
            dlg.MinutesDuration = Decimal.Floor(temp /60);
            temp -= dlg.MinutesDuration * 60;
            if (dlg.ShowSeconds)
            {
                if (dlg.ShowMilliseconds)
                    dlg.SecondsDuration = temp;
                else
                    dlg.SecondsDuration = Decimal.Floor(temp);
            }
            else
            {
                dlg.SecondsDuration = 0;
            }

            dlg.chkSign.IsChecked = isNegative;
        }

        public Decimal Duration
        {
            get
            {
                return (Decimal)GetValue(DurationProperty);
            }
            set
            {
                SetValue(DurationProperty, value);
            }
        }
        public Decimal DaysDuration
        {
            get
            {
                return (Decimal)GetValue(DaysDurationProperty);
            }
            set
            {
                SetValue(DaysDurationProperty, value);
            }
        }
        public Decimal HoursDuration
        {
            get
            {
                return (Decimal)GetValue(HoursDurationProperty);
            }
            set
            {
                SetValue(HoursDurationProperty, value);
            }
        }
        public Decimal MinutesDuration
        {
            get
            {
                return (Decimal)GetValue(MinutesDurationProperty);
            }
            set
            {
                SetValue(MinutesDurationProperty, value);
            }
        }
        public Decimal SecondsDuration
        {
            get
            {
                return (Decimal)GetValue(SecondsDurationProperty);
            }
            set
            {
                SetValue(SecondsDurationProperty, value);
            }
        }

        public bool ShowMilliseconds
        {
            get
            {
                return (bool)GetValue(ShowMillisecondsProperty);
            }
            set
            {
                SetValue(ShowMillisecondsProperty, value);
            }
        }
        public bool ShowSeconds
        {
            get
            {
                return (bool)GetValue(ShowSecondsProperty);
            }
            set
            {
                SetValue(ShowSecondsProperty, value);
            }
        }
        public bool ShowSign
        {
            get
            {
                return (bool)GetValue(ShowSignProperty);
            }
            set
            {
                SetValue(ShowSignProperty, value);
            }
        }


        public DlgDuration()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if(ShowMilliseconds)
                Duration = SecondsDuration + MinutesDuration * 60 + HoursDuration * 60 * 60 + DaysDuration * 24 * 60 * 60;
            else
                Duration = Decimal.Floor(SecondsDuration) + MinutesDuration * 60 + HoursDuration * 60 * 60 + DaysDuration * 24 * 60 * 60;

            if (chkSign.IsChecked.GetValueOrDefault())
                Duration = - Math.Abs(Duration);

            DialogResult = true;
        }
    }
}
