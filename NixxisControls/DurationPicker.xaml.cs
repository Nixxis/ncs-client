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
    /// Interaction logic for DurationPicker.xaml
    /// </summary>
    public partial class DurationPicker : UserControl
    {
        public static readonly DependencyProperty ShowMillisecondsProperty = DependencyProperty.Register("ShowMilliseconds", typeof(bool), typeof(DurationPicker), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowSecondsProperty = DependencyProperty.Register("ShowSeconds", typeof(bool), typeof(DurationPicker), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowSignProperty = DependencyProperty.Register("ShowSign", typeof(bool), typeof(DurationPicker), new PropertyMetadata(false));

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(Decimal), typeof(DurationPicker), new FrameworkPropertyMetadata(Decimal.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

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


        public DurationPicker()
        {
            InitializeComponent();
        }


        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            DlgDuration dlg = new DlgDuration();
            dlg.Owner = Application.Current.MainWindow;
            dlg.ShowMilliseconds = ShowMilliseconds;
            dlg.ShowSeconds = ShowSeconds;
            dlg.ShowSign = ShowSign;
            dlg.Duration = Duration;

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                Duration = dlg.Duration;
            }
        }
        
    }

    public class DurationConverter :  IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                return DurationHelpers.GetDefaultDurationString((Decimal)(value), false);
            else
            {
                bool includeSign = false;
                if (parameter is bool)
                    includeSign = (bool)parameter;
                else if (parameter is string)
                    bool.TryParse(parameter as string, out includeSign);
                return DurationHelpers.GetDefaultDurationString((Decimal)(value), includeSign);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Decimal temp = DurationHelpers.SplitDurationString((string)value);
            if (temp == -1)
            {
                return null;
            }

            return temp;
        }
    }
}
