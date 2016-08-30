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

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for QualificationWindow.xaml
    /// </summary>
    public partial class QualificationWindow : Window
    {        
        #region XAML Properties
        public static readonly DependencyProperty CallbackCalendarProperty = DependencyProperty.Register("CallbackCalendar", typeof(Nixxis.Client.Calendar), typeof(QualificationWindow), new PropertyMetadata(new PropertyChangedCallback(CallbackCalendarChanged)));
        public Nixxis.Client.Calendar CallbackCalendar
        {
            get { return (Nixxis.Client.Calendar)GetValue(CallbackCalendarProperty); }
            set { SetValue(CallbackCalendarProperty, value); }
        }
        public static void CallbackCalendarChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }


        public static readonly DependencyProperty CallbackDestinationProperty = DependencyProperty.Register("CallbackDestination", typeof(string), typeof(QualificationWindow), new PropertyMetadata(new PropertyChangedCallback(CallbackDestinationChanged)));
        public string CallbackDestination
        {
            get { return (string)GetValue(CallbackDestinationProperty); }
            set { SetValue(CallbackDestinationProperty, value); }
        }
        public static void CallbackDestinationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty CallbackTimeZoneProperty = DependencyProperty.Register("CallbackTimeZone", typeof(TimeZoneInfo), typeof(QualificationWindow), new PropertyMetadata(new PropertyChangedCallback(CallbackTimeZoneChanged)));
        public TimeZoneInfo CallbackTimeZone
        {
            get { return (TimeZoneInfo)GetValue(CallbackTimeZoneProperty); }
            set { SetValue(CallbackTimeZoneProperty, value); }
        }
        public static void CallbackTimeZoneChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty VisibilityCallbackPanelProperty = DependencyProperty.Register("VisibilityCallbackPanel", typeof(Visibility), typeof(QualificationWindow), new PropertyMetadata(Visibility.Collapsed, new PropertyChangedCallback(VisibilityCallbackPanelChanged)));
        public Visibility VisibilityCallbackPanel
        {
            get { return (Visibility)GetValue(VisibilityCallbackPanelProperty); }
            set { SetValue(VisibilityCallbackPanelProperty, value); }
        }
        public static void VisibilityCallbackPanelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }


        public static readonly DependencyProperty ContactInfoProperty = DependencyProperty.Register("ContactInfo", typeof(ContactInfo), typeof(QualificationWindow), new PropertyMetadata(null, new PropertyChangedCallback(ContactInfoChanged)));
        public ContactInfo ContactInfo
        {
            get { return (ContactInfo)GetValue(ContactInfoProperty); }
            set { SetValue(ContactInfoProperty, value); }
        }
        public static void ContactInfoChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue != null)
            {
                QualificationWindow qw = obj as QualificationWindow;
                if (!qw.DialogResult.HasValue)
                {
                    qw.DialogResult = false;
                    qw.Close();
                }
            }
        }



        #endregion

        #region Properties
        public QualificationInfo SelectedItem { get; private set; }
        #endregion

        #region Constructors
        public QualificationWindow()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
            QualCrtl.SelectedQualificationChanged += new QualificationControlEventHandler(QualCrtl_SelectedQualificationChanged);
            CallbackTimeZone = TimeZoneInfo.Local;
        }
        #endregion

        #region Members

        public void SetCallbackCalendar(Nixxis.Client.Calendar calendar)
        {
            CallbackCalendar = calendar;
        }

        public void SetItemSource(QualificationInfo collection)
        {
            QualCrtl.ItemSource = collection;
        }

        public void SetTimeZones(IList<TimeZoneInfo> tz)
        {
            if (tz != null && tz.Count > 0)
            {
                callbackTimeZone.ItemsSource = tz;
                callbackTimeZone.Visibility = System.Windows.Visibility.Visible;

                CallbackTimeZone = tz[0];
            }
            else
            {
                callbackTimeZone.Visibility = System.Windows.Visibility.Collapsed;

                CallbackTimeZone = TimeZoneInfo.Local;
            }
        }

        private void QualCrtl_SelectedQualificationChanged(object sender, QualificationControlEventArg e)
        {
            if (e.NewItem == null)
            {
                this.VisibilityCallbackPanel = Visibility.Collapsed;
            }
            else
            {
                if (e.NewItem.Action == 4 || e.NewItem.Action == 5)
                {
                    this.VisibilityCallbackPanel = Visibility.Visible;
                    (callbackCalendar.DataContext as Nixxis.Client.Calendar).QualificationId = e.NewItem.Id;
                }
                else
                {
                    this.VisibilityCallbackPanel = Visibility.Collapsed;
                }
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem =  QualCrtl.SelectedItem as QualificationInfo;

            this.DialogResult = true;
            Close();
        }

        private void QualCrtl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (QualCrtl.SelectedItem != null && ((QualificationInfo)QualCrtl.SelectedItem).Action != 4 && ((QualificationInfo)QualCrtl.SelectedItem).Action != 5)
            {
                SelectedItem = QualCrtl.SelectedItem as QualificationInfo;

                this.DialogResult = true;
                Close();
            }
            else
            {
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
        #endregion
    }

    /// <summary>
    /// When object is null it will return false else it will be true
    /// </summary>
    public class ObjectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
            {
                if (value != null)
                    return true;
                else
                    return false;
            }
            else
            {
                if (value != null)
                    return (bool)parameter ? true : false;
                else
                    return (bool)parameter ? false : true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class SpecialConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {            
            Visibility vis = (Visibility) values[1];
            DateTime selected = DependencyProperty.UnsetValue.Equals(values[2]) || values[2]==null ? DateTime.MinValue : (DateTime) values[2];

            return values[0]!=null && ( vis != Visibility.Visible || selected != DateTime.MinValue );
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
