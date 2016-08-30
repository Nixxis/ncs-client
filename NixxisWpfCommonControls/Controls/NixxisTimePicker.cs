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
using System.Windows.Controls.Primitives;
using System.Timers;
using System.Windows.Threading;

namespace Nixxis.Client.Controls
{
    [TemplatePart(Name = "PART_Hours", Type = typeof(TextBox)),
     TemplatePart(Name = "PART_Minutes", Type = typeof(TextBox)),
     TemplatePart(Name = "PART_Seconds", Type = typeof(TextBox)),
     TemplatePart(Name = "PART_Increase", Type = typeof(ButtonBase)),
     TemplatePart(Name = "PART_Decrement", Type = typeof(ButtonBase)),
     TemplatePart(Name = "PART_FastSelect", Type = typeof(ButtonBase))]
    public class TimePicker : Control
    {
        #region Enums
        public enum TimePart { Hours, Minutes, Seconds };
        #endregion

        #region Class data
        private string m_VersionBuild = "4";
        private bool m_InUpdate = false;
        private TextBox m_txtHours;
        private TextBox m_txtMinutes;
        private TextBox m_txtSeconds;
        private ButtonBase m_btnIncrease;
        private ButtonBase m_btnDecrement;
        private ButtonBase m_btnFastSelect;
        private TextBox m_CurrentSelectedTextBox;
        private Popup m_fs_PopupHour;
        private Popup m_fs_PopupMin;

        private bool m_fs_IsHourOpen = false;
        private int m_fs_Hour = -1;
        private bool m_fs_IsMinOpen = false;
        private int m_fs_Min = -1;
        private ButtonBase m_fs_CurrentSelectedButton = null;
        private Timer m_fs_TimerHour = new Timer();
        private Timer m_fs_TimerMin = new Timer();
        #endregion

        #region Properties XAML 
        public static readonly DependencyProperty HasFocusProperty = DependencyProperty.Register("HasFocus", typeof(bool), typeof(TimePicker), new PropertyMetadata(false));
        public bool HasFocus
        {
            get { return (bool)GetValue(HasFocusProperty); }
            set { SetValue(HasFocusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(TimePicker), new PropertyMetadata(new CornerRadius(0)));
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register("SelectedTime", typeof(TimeSpan), typeof(TimePicker), new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedTimePropertyChanged));
        public TimeSpan SelectedTime
        {
            get { return (TimeSpan)GetValue(SelectedTimeProperty); }
            set { SetValue(SelectedTimeProperty, value); }
        }
        public static void SelectedTimePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            TimePicker timePicker = (TimePicker)obj;
            TimeSpan newTime = (TimeSpan)args.NewValue;
            TimeSpan oldTime = (TimeSpan)args.OldValue;

            if (newTime != oldTime)
                timePicker.UpdateTimeParts();
        }

        public static readonly DependencyProperty SelectedHourProperty = DependencyProperty.Register("SelectedHour", typeof(int), typeof(TimePicker), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedHourPropertyChanged));
        public int SelectedHour
        {
            get { return (int)GetValue(SelectedHourProperty); }
            set { SetValue(SelectedHourProperty, value); }
        }
        public static void SelectedHourPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            TimePicker timePicker = (TimePicker)obj;

            if (!timePicker.m_InUpdate)
                timePicker.SelectedTime = new TimeSpan(timePicker.SelectedHour, timePicker.SelectedMinute, timePicker.SelectedSecond);
        }

        public static readonly DependencyProperty SelectedMinuteProperty = DependencyProperty.Register("SelectedMinute", typeof(int), typeof(TimePicker), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedMinutePropertyChanged));
        public int SelectedMinute
        {
            get { return (int)GetValue(SelectedMinuteProperty); }
            set { SetValue(SelectedMinuteProperty, value); }
        }
        public static void SelectedMinutePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            TimePicker timePicker = (TimePicker)obj;

            if (!timePicker.m_InUpdate)
                timePicker.SelectedTime = new TimeSpan(timePicker.SelectedHour, timePicker.SelectedMinute, timePicker.SelectedSecond);
        }

        public static readonly DependencyProperty SelectedSecondProperty = DependencyProperty.Register("SelectedSecond", typeof(int), typeof(TimePicker), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedSecondPropertyChanged));
        public int SelectedSecond
        {
            get { return (int)GetValue(SelectedSecondProperty); }
            set { SetValue(SelectedSecondProperty, value); }
        }
        public static void SelectedSecondPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            TimePicker timePicker = (TimePicker)obj;

            if (!timePicker.m_InUpdate)
                timePicker.SelectedTime = new TimeSpan(timePicker.SelectedHour, timePicker.SelectedMinute, timePicker.SelectedSecond);
        }

        public static readonly DependencyProperty IsHourVisibleProperty = DependencyProperty.Register("IsHourVisible", typeof(bool), typeof(TimePicker), new UIPropertyMetadata(true));
        public bool IsHourVisible
        {
            get { return (bool)GetValue(IsHourVisibleProperty); }
            set { SetValue(IsHourVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsMinuteVisibleProperty = DependencyProperty.Register("IsMinuteVisible", typeof(bool), typeof(TimePicker), new UIPropertyMetadata(true));
        public bool IsMinuteVisible
        {
            get { return (bool)GetValue(IsMinuteVisibleProperty); }
            set { SetValue(IsMinuteVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsSecondeVisibleProperty = DependencyProperty.Register("IsSecondeVisible", typeof(bool), typeof(TimePicker), new UIPropertyMetadata(true));
        public bool IsSecondeVisible
        {
            get { return (bool)GetValue(IsSecondeVisibleProperty); }
            set { SetValue(IsSecondeVisibleProperty, value); }
        }

        public static readonly DependencyProperty FastSelectionDisplayTimeProperty = DependencyProperty.Register("FastSelectionDisplayTime", typeof(double), typeof(TimePicker), new UIPropertyMetadata((double)500, FastSelectionDisplayTimePropertyChanged));
        public double FastSelectionDisplayTime
        {
            get { return (double)GetValue(FastSelectionDisplayTimeProperty); }
            set { SetValue(FastSelectionDisplayTimeProperty, value); }
        }
        public static void FastSelectionDisplayTimePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            TimePicker timePicker = (TimePicker)obj;

            timePicker.m_fs_TimerHour.Interval = timePicker.FastSelectionDisplayTime;
            timePicker.m_fs_TimerMin.Interval = timePicker.FastSelectionDisplayTime;
        }
        #endregion

        #region Constructors
        static TimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
        }
        #endregion

        #region Members override
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            m_fs_TimerHour.Interval = this.FastSelectionDisplayTime;
            m_fs_TimerHour.Elapsed += new ElapsedEventHandler(TimerHour_Elapsed);

            m_fs_TimerMin.Interval = this.FastSelectionDisplayTime;
            m_fs_TimerMin.Elapsed += new ElapsedEventHandler(TimerMin_Elapsed);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            CheckFocus();
        }
        
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            CheckFocus();
        }
        
        public override void OnApplyTemplate()
        {
            //Hours
            m_txtHours = GetTemplateChild("PART_Hours") as TextBox;
            m_txtHours.KeyUp += TextBox_KeyUp;
            m_txtHours.GotFocus += TextBox_GotFocus;
            m_txtHours.LostFocus += TextBox_LostFocus;

            //Minutes
            m_txtMinutes = GetTemplateChild("PART_Minutes") as TextBox;
            m_txtMinutes.GotFocus += TextBox_GotFocus;
            m_txtMinutes.LostFocus += TextBox_LostFocus;

            //Seconds
            m_txtSeconds = GetTemplateChild("PART_Seconds") as TextBox;
            m_txtSeconds.GotFocus += TextBox_GotFocus;            
            m_txtSeconds.LostFocus += TextBox_LostFocus;

            //Increase button
            m_btnIncrease = GetTemplateChild("PART_Increase") as ButtonBase;
            m_btnIncrease.Click += Increase_Click;
            m_btnIncrease.GotFocus += Increase_GotFocus;
            m_btnIncrease.LostFocus += Increase_LostFocus;

            //Decrease button
            m_btnDecrement = GetTemplateChild("PART_Decrement") as ButtonBase;
            m_btnDecrement.Click += Decrement_Click;
            m_btnDecrement.GotFocus += Decrement_GotFocus;
            m_btnDecrement.LostFocus += Decrement_LostFocus;

            //Fast selection button
            m_btnFastSelect = GetTemplateChild("PART_FastSelect") as ButtonBase;
            m_btnFastSelect.Click += FasSelectBtn_Click;
            m_btnFastSelect.GotFocus += FasSelectBtn_GotFocus;
            m_btnFastSelect.LostFocus += FasSelectBtn_LostFocus;

            try
            {
                m_fs_PopupHour = GetTemplateChild("FastSelectHours") as Popup;
                m_fs_PopupHour.MouseLeave += PopupHour_MouseLeave;
                m_fs_PopupHour.MouseEnter += PopupHour_MouseEnter;
                m_fs_PopupHour.Closed += PopupHour_Closed;
            }
            catch { }

            try
            {
                m_fs_PopupMin = GetTemplateChild("FastSelectMinutes") as Popup;
                m_fs_PopupMin.MouseLeave += PopupMin_MouseLeave;
                m_fs_PopupMin.MouseEnter += PopupMin_MouseEnter;
                m_fs_PopupMin.Closed += PopupMin_Closed;
            }
            catch { }

            //Fast selection hours
            for (int i = 0; i < 24; i++)
            {
                ButtonBase btn = GetTemplateChild("FastSelectHour" + i.ToString("d2")) as ButtonBase;
                btn.Click += FsBtnHour_Click;
            }

            for (int i = 0; i < 4; i++)
            {
                ButtonBase btn = GetTemplateChild("FastSelectMin" + i.ToString("d2")) as ButtonBase;
                btn.Click += FsBtnMin_Click;
            }
        }
        #endregion

        #region Members
        private void CheckFocus()
        {
            this.HasFocus = this.IsFocused || m_txtHours.IsFocused || m_txtMinutes.IsFocused || m_txtSeconds.IsFocused || m_btnIncrease.IsFocused || m_btnDecrement.IsFocused || m_btnFastSelect.IsFocused;
        }

        private void UpdateTimeParts()
        {
            if (m_InUpdate) return;

            m_InUpdate = true;

            TimeSpan currentTime = this.SelectedTime;

            this.SelectedHour = currentTime.Hours;
            this.SelectedMinute = currentTime.Minutes;
            this.SelectedSecond = currentTime.Seconds;

            m_InUpdate = false;
        }

        private void IncreaseTime(TimePart part)
        {
            if (this.SelectedTime == TimeSpan.MaxValue)
                return;

            TimeSpan newTime = this.SelectedTime;
            switch (part)
            {
                case TimePart.Hours:
                    newTime += new TimeSpan(1, 0, 0);
                    break;
                case TimePart.Minutes:
                    newTime += new TimeSpan(0, 1, 0);
                    break;
                case TimePart.Seconds:
                    newTime += new TimeSpan(0, 0, 1);
                    break;
            }

            if (newTime <= new TimeSpan(23, 59, 59))
                this.SelectedTime = newTime;
        }

        private void DecrementTime(TimePart part)
        {
            if (this.SelectedTime == TimeSpan.MinValue)
                return;

            TimeSpan newTime = this.SelectedTime;
            switch (part)
            {
                case TimePart.Hours:
                    newTime -= new TimeSpan(1, 0, 0);
                    break;
                case TimePart.Minutes:
                    newTime -= new TimeSpan(0, 1, 0);
                    break;
                case TimePart.Seconds:
                    newTime -= new TimeSpan(0, 0, 1);
                    break;
            }

            if (newTime >= new TimeSpan(0, 0, 0))
                this.SelectedTime = newTime;
        }
        #endregion

        #region Handlers
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = (TextBox)sender;

            if (!sender.Equals(m_CurrentSelectedTextBox))
            {
                m_CurrentSelectedTextBox = sender as TextBox;
            }

            this.CheckFocus();
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.CheckFocus();
        }

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            if (m_CurrentSelectedTextBox == null)
                m_CurrentSelectedTextBox = m_txtHours;

            switch (m_CurrentSelectedTextBox.Name)
            {
                case "PART_Hours":
                    IncreaseTime(TimePart.Hours);
                    break;
                case "PART_Minutes":
                    IncreaseTime(TimePart.Minutes);
                    break;
                case "PART_Seconds":
                    IncreaseTime(TimePart.Seconds);
                    break;
            }
        }
        private void Increase_GotFocus(object sender, RoutedEventArgs e)
        {
            this.CheckFocus();
        }
        private void Increase_LostFocus(object sender, RoutedEventArgs e)
        {
            this.CheckFocus();
        }

        private void Decrement_Click(object sender, RoutedEventArgs e)
        {
            if (m_CurrentSelectedTextBox == null)
                m_CurrentSelectedTextBox = m_txtHours;

            switch (m_CurrentSelectedTextBox.Name)
            {
                case "PART_Hours":
                    DecrementTime(TimePart.Hours);
                    break;
                case "PART_Minutes":
                    DecrementTime(TimePart.Minutes);
                    break;
                case "PART_Seconds":
                    DecrementTime(TimePart.Seconds);
                    break;
            }
        }
        private void Decrement_GotFocus(object sender, RoutedEventArgs e)
        {
            this.CheckFocus();
        }
        private void Decrement_LostFocus(object sender, RoutedEventArgs e)
        {
            this.CheckFocus();
        }

        private void FasSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            m_fs_IsHourOpen = true;
        }
        private void FasSelectBtn_GotFocus(object sender, RoutedEventArgs e)
        {
            this.CheckFocus();
        }
        private void FasSelectBtn_LostFocus(object sender, RoutedEventArgs e)
        {
            this.CheckFocus();
        }

        private void PopupHour_MouseLeave(object sender, MouseEventArgs e)
        {
            //m_fs_CanHourClose = true;
            m_fs_TimerHour.Start();
        }
        private void PopupHour_MouseEnter(object sender, MouseEventArgs e)
        {
            m_fs_TimerHour.Stop();
        }
        private void PopupHour_Closed(object sender, EventArgs e)
        {
            m_fs_IsHourOpen = false;
            m_fs_IsMinOpen = false;
            m_fs_Hour = -1;
            m_fs_Min = -1;

            m_fs_CurrentSelectedButton = null;
            m_fs_PopupMin.IsOpen = false;
            m_fs_TimerHour.Stop();
        }
        private void TimerHour_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!m_fs_IsMinOpen)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    m_fs_PopupHour.IsOpen = false;
                }));
            }
        }

        private void PopupMin_MouseLeave(object sender, MouseEventArgs e)
        {
            m_fs_TimerMin.Start();
        }
        private void PopupMin_MouseEnter(object sender, MouseEventArgs e)
        {
            m_fs_TimerMin.Stop();
        }
        private void PopupMin_Closed(object sender, EventArgs e)
        {
            m_fs_TimerMin.Stop();
        }
        private void TimerMin_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                m_fs_PopupMin.IsOpen = false;
                m_fs_IsMinOpen = false;
            }));
        }

        private void FsBtnHour_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase btn = (ButtonBase)sender;


            //Unselect previous button
            if (m_fs_CurrentSelectedButton != null)
                m_fs_PopupMin.IsOpen = false;

            m_fs_IsMinOpen = true;
            m_fs_PopupMin.IsOpen = true;
            m_fs_Hour = int.Parse(btn.Tag.ToString());
            m_fs_CurrentSelectedButton = btn;
        }

        private void FsBtnMin_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase btn = (ButtonBase)sender;

            if (m_fs_IsHourOpen)
            {
                m_fs_Min = int.Parse(btn.Tag.ToString());
                this.SelectedTime = new TimeSpan(m_fs_Hour, m_fs_Min, 0);

                m_fs_PopupHour.IsOpen = false;
                m_fs_PopupMin.IsOpen = false;
            }
        }
        #endregion

        #region Events
        public static readonly RoutedEvent SelectedTimeChangedEvent = EventManager.RegisterRoutedEvent("SelectedTimeChanged", RoutingStrategy.Bubble, typeof(TimeSelectedChangedEventHandler), typeof(TimePicker));

        public event TimeSelectedChangedEventHandler SelectedTimeChanged
        {
            add { AddHandler(SelectedTimeChangedEvent, value); }
            remove { RemoveHandler(SelectedTimeChangedEvent, value); }
        }
        #endregion
    }

    public delegate void TimeSelectedChangedEventHandler(object sender, TimeSelectedChangedRoutedEventArgs e);

    public class TimeSelectedChangedRoutedEventArgs : RoutedEventArgs
    {
        public Nullable<TimeSpan> NewTime { get; set; }
        public Nullable<TimeSpan> OldTime { get; set; }

        public TimeSelectedChangedRoutedEventArgs(RoutedEvent routedEvent)
            : base(routedEvent) { }
    }

    public class TimePickerDisplayConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return int.Parse(value.ToString()).ToString("d2");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int min = 0;
            int max = 60;
            int.TryParse(parameter.ToString(), out max);
            int val = 0;
            int.TryParse(value.ToString(), out val);

            if (val < min)
                return min;
            else if (val > max)
                return max;
            else
                return val;
        }
    }

    public class MultiPartToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool visMin = (bool)values[0];
                bool visSec = (bool)values[1];

                if (visMin)
                {
                    if (visSec)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            catch { return Visibility.Visible; }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
