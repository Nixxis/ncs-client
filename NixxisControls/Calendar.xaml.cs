using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for Calendar.xaml
    /// </summary>
    public partial class Calendar : UserControl
    {
        public Calendar()
        {
            InitializeComponent();
            this.DataContextChanged += Calendar_DataContextChanged;
        }

        void Calendar_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Nixxis.Client.Calendar cal = this.DataContext as Nixxis.Client.Calendar;
            if (cal != null)
            {
                cal.PropertyChanged += cal_PropertyChanged;
            }
        }

        void cal_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Loaded" && lstHeader.SelectedIndex < 0)
            {
                Nixxis.Client.Calendar cal = this.DataContext as Nixxis.Client.Calendar;
                for (int i = 0; i < lstHeader.Items.Count; i++)
                {
                    if ((lstHeader.Items[i] as Nixxis.Client.Day).DayValue == cal.LastSelectedDate.Date)
                    {
                        lstHeader.SelectedIndex = i;
                        return;
                    }
                }
                lstHeader.SelectedIndex = 0;
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            int backup = lstHeader.SelectedIndex;
            Nixxis.Client.Calendar cal = DataContext as Nixxis.Client.Calendar;
            cal.LoadPreviousDays();
            lstHeader.SelectedIndex = backup;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            int backup = lstHeader.SelectedIndex;
            Nixxis.Client.Calendar cal = DataContext as Nixxis.Client.Calendar;
            cal.LoadNextDays();
            lstHeader.SelectedIndex = backup;
        }

        private void lstHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Nixxis.Client.Calendar cal = DataContext as Nixxis.Client.Calendar;
            cal.SelectedTime = cal.SelectedTime;
        }

        private void lstEntries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lstEntries.SelectedItem!=null)
                lstEntries.ScrollIntoView(lstEntries.SelectedItem);

            Nixxis.Client.Calendar cal = DataContext as Nixxis.Client.Calendar;
            if (lstEntries.SelectedItem == null)
            {
                cal.SelectedDate = DateTime.MinValue;
            }
            else
            {
                cal.SelectedDate = (lstEntries.SelectedItem as CalendarEntry).Start;
            }
        }


    }

    public class FillToColorConverter : IValueConverter, IMultiValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = (double)value;
            if (val == 0)
            {
                return Brushes.Transparent;
            }
            else if (val < 0.25)
            {
                return new SolidColorBrush(Color.FromArgb(64, 255, 0, 0));
            }
            else if (val < 0.5)
            {
                return new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
            }
            else if (val < 0.75)
            {
                return new SolidColorBrush(Color.FromArgb(192, 255, 0, 0));
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool useGradient = false;
            try
            {
                if (parameter is bool)
                {
                    useGradient = (bool)parameter;
                }
            }
            catch
            {
            }

            try
            {
                string strParam = parameter as string;
                if (strParam!=null)
                {
                    useGradient = Boolean.Parse(strParam);
                }
            }
            catch
            {
            }

            double val = (double)values[0];
            bool enabled = (bool)values[1];
            if (enabled)
            {
                if (useGradient)
                {
                    if (val == 0)
                    {
                        return Brushes.Transparent;
                    }
                    else if (val < 0.25)
                    {
                        return new LinearGradientBrush(new GradientStopCollection(new GradientStop[] { new GradientStop(Color.FromArgb(64, 255, 0, 0), 0), new GradientStop(Colors.Transparent, 0.5), new GradientStop(Color.FromArgb(64, 255, 0, 0), 1.0) }), 0);
                    }
                    else if (val < 0.5)
                    {
                        return new LinearGradientBrush(new GradientStopCollection(new GradientStop[] { new GradientStop(Color.FromArgb(128, 255, 0, 0), 0), new GradientStop(Colors.Transparent, 0.5), new GradientStop(Color.FromArgb(128, 255, 0, 0), 1.0) }), 0);
                    }
                    else if (val < 0.75)
                    {
                        return new LinearGradientBrush(new GradientStopCollection(new GradientStop[] { new GradientStop(Color.FromArgb(192, 255, 0, 0), 0), new GradientStop(Colors.Transparent, 0.5), new GradientStop(Color.FromArgb(192, 255, 0, 0), 1.0) }), 0);
                    }
                    else
                    {
                        return new LinearGradientBrush(new GradientStopCollection(new GradientStop[] { new GradientStop(Color.FromArgb(255, 255, 0, 0), 0), new GradientStop(Colors.Transparent, 0.5), new GradientStop(Color.FromArgb(255, 255, 0, 0), 1.0) }), 0);
                    }
                }
                else
                {
                    if (val == 0)
                    {
                        return Brushes.Transparent;
                    }
                    else if (val < 0.25)
                    {
                        return new SolidColorBrush(Color.FromArgb(64, 255, 0, 0));
                    }
                    else if (val < 0.5)
                    {
                        return new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
                    }
                    else if (val < 0.75)
                    {
                        return new SolidColorBrush(Color.FromArgb(192, 255, 0, 0));
                    }
                    else
                    {
                        return new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    }

                }
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            }

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FillToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double dbl = (double)value;

            if (dbl == 0)
                return string.Empty;
            else
                return string.Format("Filled: {0:0}%", (dbl * 100));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DateFormater : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format(parameter as string, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DayFormater : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format("{0:ddd}", value).Substring(0, 1).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CalVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime dt = (DateTime)value;
            if (dt.Date == DateTime.Now.Date)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
