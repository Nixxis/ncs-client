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
using System.Windows.Controls.Primitives;

namespace Nixxis.Client.Controls
{
    /// <summary>
    /// Interaction logic for NixxisPlaning.xaml
    /// </summary>
    public partial class NixxisPlanning : UserControl
    {

        private List<ScheduleLine> ScheduleLines = new List<ScheduleLine>();

        internal class TimeOfDayConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                int val = (int)value % 1440;
                int hour = val / 60;
                int minutes = val - hour * 60;
                return string.Format("{0:00}:{1:00}", hour, minutes);
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        internal class SpecialConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return ((double)value - ((double[])(parameter))[1]) * ((double[])(parameter))[0] + ((double[])(parameter))[1];
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        internal class SpecialConverter2 : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return ((double)value - ((double[])(parameter))[1]) * ((double[])(parameter))[0] + ((double[])(parameter))[1] - ((double)value - ((double[])(parameter))[1]) / 16;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }


        public static readonly RoutedEvent NewTimeEvent = EventManager.RegisterRoutedEvent("NewTime", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisPlanning));
        public static readonly RoutedEvent RemoveTimeEvent = EventManager.RegisterRoutedEvent("removeTime", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisPlanning));
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisPlanning));

        public static readonly DependencyProperty StartTimePathProperty = DependencyProperty.Register("StartTimePath", typeof(string), typeof(NixxisPlanning));
        public static readonly DependencyProperty EndTimePathProperty = DependencyProperty.Register("EndTimePath", typeof(string), typeof(NixxisPlanning));
        public static readonly DependencyProperty ClosedPathProperty = DependencyProperty.Register("ClosedPath", typeof(string), typeof(NixxisPlanning));


        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(NixxisPlanning), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure, new PropertyChangedCallback(ItemsSource_Changed)));
        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue", typeof(object), typeof(NixxisPlanning), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure, new PropertyChangedCallback(SelectedValue_Changed)));
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(NixxisPlanning), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.Register("IsSelectable", typeof(bool), typeof(NixxisPlanning), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        public static readonly DependencyProperty StartTimeProperty = DependencyProperty.RegisterAttached("StartTime", typeof(int), typeof(NixxisPlanning), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure, new PropertyChangedCallback(StartTime_Changed)));
        public static readonly DependencyProperty EndTimeProperty = DependencyProperty.RegisterAttached("EndTime", typeof(int), typeof(NixxisPlanning), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        private object ExtractValue(object obj, string path)
        {
            if (string.IsNullOrEmpty(path))
                return obj;

            string[] parts = path.Split('.');
            object tempResult = null;
            if (parts.Length > 0)
            {
                tempResult = obj.GetType().GetProperty(parts[0], System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly).GetValue(obj, new object[0]);
            }
            if (parts.Length > 1)
            {
                return ExtractValue(tempResult, string.Join(".", parts, 1, parts.Length - 1));
            }
            return tempResult;
        }

        private ScheduleLine m_Selected = null;
        public ScheduleLine Selected
        {
            get
            {
                return m_Selected;
            }
            set
            {
                if (m_Selected != null)
                    m_Selected.IsSelected = false;

                m_Selected = value;

                if (m_Selected != null)
                {
                    m_Selected.IsSelected = true;
                    SelectedValue = m_Selected.Associated.DataContext;
                }
                else
                {
                    SelectedValue = null;
                }

                RaiseEvent(new RoutedEventArgs(SelectionChangedEvent));
            }
        }
        public object SelectedValue
        {
            get
            {
                return GetValue(SelectedValueProperty);
            }
            set
            {
                SetValue(SelectedValueProperty, value);
            }
        }

        public string StartTimePath
        {
            get
            {
                return (string)GetValue(StartTimePathProperty);
            }
            set
            {
                SetValue(StartTimePathProperty, value);
            }
        }
        public string EndTimePath
        {
            get
            {
                return (string)GetValue(EndTimePathProperty);
            }
            set
            {
                SetValue(EndTimePathProperty, value);
            }
        }

        public string ClosedPath
        {
            get
            {
                return (string)GetValue(ClosedPathProperty);
            }
            set
            {
                SetValue(ClosedPathProperty, value);
            }
        }


        public event RoutedEventHandler NewTime
        {
            add { AddHandler(NewTimeEvent, value); }
            remove { RemoveHandler(NewTimeEvent, value); }
        }
        public event RoutedEventHandler RemoveTime
        {
            add { AddHandler(RemoveTimeEvent, value); }
            remove { RemoveHandler(RemoveTimeEvent, value); }
        }
        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        public static void SetStartTime(UIElement element, int value)
        {
            element.SetValue(StartTimeProperty, value);
        }
        public static int GetStartTime(UIElement element)
        {
            return (int)(element.GetValue(StartTimeProperty));
        }

        public static void SetEndTime(UIElement element, int value)
        {
            element.SetValue(EndTimeProperty, value);
        }
        public static int GetEndTime(UIElement element)
        {
            return (int)(element.GetValue(EndTimeProperty));
        }


        public object ItemsSource
        {
            get
            {
                return GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }
        public bool IsReadOnly
        {
            get
            {
                return (bool)GetValue(IsReadOnlyProperty);
            }
            set
            {
                SetValue(IsReadOnlyProperty, value);
            }
        }
        public bool IsSelectable
        {
            get
            {
                return (bool)GetValue(IsSelectableProperty);
            }
            set
            {
                SetValue(IsSelectableProperty, value);
            }
        }

        private Cursor m_Backup = null;

        private int leftMargin = 70;
        internal int topMargin = 20;
        internal IOrderedEnumerable<object> sortedItems = null;
        private MouseButtonEventArgs m_LastMouseEvent = null;
        private DragStartedEventArgs m_DragStartedEvent = null;

        public static void ItemsSource_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisPlanning np = (NixxisPlanning)obj;

            np.m_LastMouseEvent = null;
            np.Selected = null;

            if (args.OldValue is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)args.OldValue).CollectionChanged -= new NotifyCollectionChangedEventHandler(np.NixxisPlanning_CollectionChanged);
            }

            if (args.NewValue is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)args.NewValue).CollectionChanged += new NotifyCollectionChangedEventHandler(np.NixxisPlanning_CollectionChanged);
            }

            np.Reset();

            if (args.NewValue is IEnumerable)
            {
                IEnumerable ienum = (IEnumerable)args.NewValue;

                foreach (object obje in ienum)
                {
                    np.AddThumb(obje);
                }
            }
        }

        public static void SelectedValue_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            
            NixxisPlanning np = (NixxisPlanning)obj;

            if (args.NewValue == null)
            {
                if (np.Selected != null)
                    np.Selected = null;
            }
            else
            {
                ScheduleThumb th = np.GetThumbFromData(args.NewValue);
                if (th != null)
                {
                    ScheduleLine sl = np.GetLineAfter(th);
                    if (sl != np.Selected)
                    {
                        np.Selected = sl;
                    }
                }
            }
        }


        private void Reset()
        {
            SelectedValue = null;

            for (int i = 0; i < DrawingCanvas.Children.Count; i++)
            {
                if (DrawingCanvas.Children[i] is ScheduleThumb /*&& GetStartTime(DrawingCanvas.Children[i])>=0*/)
                {
                    RemoveThumb((ScheduleThumb)(DrawingCanvas.Children[i]));
                    i--;
                }
            }
        }

        private void RemoveThumb(ScheduleThumb th)
        {
            BindingOperations.ClearAllBindings(th);

            DrawingCanvas.Children.Remove(th);

            RemoveScheduleFromThumb(th);

            Refresh();
        }

        private void AddThumb(object bindingSource)
        {
            ScheduleThumb th = new ScheduleThumb(ExtractValue(bindingSource, StartTimePath));
            if (IsReadOnly)
            {
                th.IsEnabled = false;
                th.Visibility = System.Windows.Visibility.Hidden;
            }

            Canvas.SetZIndex(th, int.MaxValue);
            th.SnapsToDevicePixels = true;
            th.Margin = new Thickness(-5, -5, 0, 0);

            DrawingCanvas.Children.Add(th);
            th.DragStarted += new DragStartedEventHandler(th_DragStarted);
            th.DragDelta += new DragDeltaEventHandler(th_DragDelta);
            th.DragCompleted += new DragCompletedEventHandler(th_DragCompleted);
            th.Width = 10;
            th.Height = 10;

            bool closed = (bool)ExtractValue(bindingSource, ClosedPath); 

            if (!closed)
            {
                th.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                th.Background = new SolidColorBrush(Colors.Red);
            }

            th.SetBinding(NixxisPlanning.StartTimeProperty, new Binding(StartTimePath) { Source = bindingSource, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            th.SetBinding(NixxisPlanning.EndTimeProperty, new Binding(EndTimePath) { Source = bindingSource, Mode = BindingMode.OneWayToSource, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            th.DataContext = bindingSource;

            TextBlock tb = new TextBlock();
            tb.SetBinding(TextBlock.TextProperty, new Binding(StartTimePath) { Source = th, Path = new PropertyPath(NixxisPlanning.StartTimeProperty), Converter = new TimeOfDayConverter() });
            th.ToolTip = tb;

            ScheduleLine ln = new ScheduleLine(this, th, !closed);
            ScheduleLines.Add(ln);


            StartTime_Changed(th, new DependencyPropertyChangedEventArgs());

            if (m_LastMouseEvent != null)
            {
                th.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent });
            }
        }

        private List<ScheduleThumb> SortItems()
        {
            // TODO: return only the Thumb needing redraw

            List<ScheduleThumb> retValue = new List<ScheduleThumb>();

            sortedItems = (ItemsSource as IEnumerable<object>).OrderBy((o) => (ExtractValue(o, StartTimePath)  ));

            foreach (object obj in sortedItems)
            {
                retValue.Add(GetThumbFromData(obj));
            }

            return retValue;
        }

        private int GetFirstTime()
        {
            object obj = sortedItems.FirstOrDefault();
            return (int)ExtractValue(obj, StartTimePath);
        }

        private ScheduleLine GetLineBefore(ScheduleThumb th)
        {
            ScheduleThumb after = GetThumbBefore(th);
            if (after == null)
                return null;
            return GetLineAfter(after);
        }

        private ScheduleLine GetLineAfter(ScheduleThumb th)
        {
            foreach (UIElement uie in DrawingCanvas.Children)
            {
                if (uie is Line && ((Line)uie).Tag is ScheduleLine && ((ScheduleLine)((Line)uie).Tag).Associated == th)
                {
                    return ((ScheduleLine)((Line)uie).Tag);
                }
            }
            return null;
        }

        private ScheduleThumb GetThumbFromData(object data)
        {
            foreach (UIElement uie in DrawingCanvas.Children)
            {
                if (uie is ScheduleThumb && ((ScheduleThumb)uie).DataContext == data)
                {
                    return (ScheduleThumb)uie;
                }
            }

            return null;
        }

        private ScheduleThumb GetThumbBefore(ScheduleThumb th)
        {
            object old = null;

            foreach (object ob in sortedItems)
            {
                if (ob == th.DataContext)
                {
                    break;
                }
                old = ob;
            }

            return GetThumbFromData(old);
        }

        private ScheduleThumb GetThumbAfter(ScheduleThumb th)
        {
            object old = null;
            bool stopNext = false;
            foreach (object ob in sortedItems)
            {
                if (stopNext)
                {
                    old = ob;
                    break;
                }
                stopNext = (ob == th.DataContext);
            }

            return GetThumbFromData(old);

        }

        public static void StartTime_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ScheduleThumb th = (ScheduleThumb)obj;
            Canvas cnv = (Canvas)(th.Parent);
            NixxisPlanning np = (NixxisPlanning)(cnv.Parent);

            if (GetStartTime(th) == -1)
            {
                Canvas.SetLeft(th, 20);
                Canvas.SetTop(th, 10);
            }
            else if (GetStartTime(th) == -2)
            {
                Canvas.SetLeft(th, 50);
                Canvas.SetTop(th, 10);
            }
            else
            {
                Point pt = np.ConvertTimeToPoint(GetStartTime(th));

                Canvas.SetLeft(th, pt.X);
                Canvas.SetTop(th, pt.Y);


                if (np.ItemsSource != null)
                {
                    List<ScheduleThumb> thumbsToCheck = np.SortItems();

                    foreach (ScheduleThumb t in thumbsToCheck)
                    {
                        if (t != null)
                        {
                            // The line after:
                            ScheduleLine ln = np.GetLineAfter(t);
                            if (ln == null)
                                continue;

                            ln.StartTime = GetStartTime(t);

                            ScheduleThumb nextThumb = np.GetThumbAfter(t);

                            if (nextThumb != null)
                            {

                                ln.EndTime = GetStartTime(nextThumb);
                            }
                            else
                            {
                                ln.EndTime = np.GetFirstTime();
                            }

                            SetEndTime(t, ln.EndTime);

                            // The line before:
                            ln = np.GetLineBefore(t);

                            if (ln == null)
                            {
                                continue;
                            }

                            ln.EndTime = GetStartTime(t);

                            ScheduleThumb previousThumb = np.GetThumbBefore(t);

                            if (previousThumb != null)
                            {
                                ln.StartTime = GetStartTime(previousThumb);
                                ln.Hidden = false;
                            }
                            else
                            {
                                ln.Hidden = true;
                            }
                        }
                    }
                }
            }
        }

        void NixxisPlanning_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        AddThumb(e.NewItems[i]);
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object obj in e.OldItems)
                        for (int i = 0; i < DrawingCanvas.Children.Count; i++)
                        {
                            UIElement uie = DrawingCanvas.Children[i];

                            if (uie is ScheduleThumb)
                            {
                                if (((ScheduleThumb)uie).DataContext == obj)
                                {
                                    RemoveThumb((ScheduleThumb)uie);
                                    i--;
                                }
                            }
                        }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    foreach (object obj in (sender as IEnumerable))
                    {
                        AddThumb(obj);
                    }
                    break;
            }
        }

        static void th_DragCompleted(object sender, DragCompletedEventArgs e)
        {

            ScheduleThumb th = (ScheduleThumb)sender;
            Canvas cnv = (Canvas)(th.Parent);
            NixxisPlanning np = (NixxisPlanning)(cnv.Parent);

            th.DescriptivePopup.IsOpen = false;

            int origTime = GetStartTime(th);
            if (origTime < 0)
            {

                int time = ConvertThumbToTime(th);

                SetStartTime(th, -3);// this will cause a change to the property -> puting it back at default location
                SetStartTime(th, origTime);// this will cause a change to the property -> puting it back at default location

                // Add the new 
                if (time >= 0 && time <= 1440 * 7)
                    np.RaiseEvent(new RoutedEventArgs(NixxisPlanning.NewTimeEvent, new object[] { time, origTime == -1 }));

            }
            else
            {
                int time = ConvertThumbToTime(th);

                if (time >= 0 && time <= 1440 * 7)
                {
                    if (e.Canceled)
                        return;

                    ScheduleThumb after = np.GetThumbAfter(th);
                    if (after != null && GetStartTime(after) == time)
                    {
                        np.RaiseEvent(new RoutedEventArgs(NixxisPlanning.RemoveTimeEvent, th.DataContext));
                        np.RaiseEvent(new RoutedEventArgs(NixxisPlanning.RemoveTimeEvent, after.DataContext));
                    }
                    else
                    {
                        ScheduleThumb before = np.GetThumbBefore(th);
                        if (before != null && GetStartTime(before) == time)
                        {
                            np.RaiseEvent(new RoutedEventArgs(NixxisPlanning.RemoveTimeEvent, th.DataContext));
                            np.RaiseEvent(new RoutedEventArgs(NixxisPlanning.RemoveTimeEvent, before.DataContext));
                        }
                    }
                }
                else
                {
                    np.RaiseEvent(new RoutedEventArgs(NixxisPlanning.RemoveTimeEvent, th.DataContext));
                }
            }



        }
        static void th_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ScheduleThumb th = (ScheduleThumb)sender;
            Canvas cnv = (Canvas)(th.Parent);
            NixxisPlanning np = (NixxisPlanning)(cnv.Parent);

            int origTime = GetStartTime(th);

            if (origTime < 0)
            {
                th.Opacity = 1;
                MoveByPosition(th, e.HorizontalChange - np.m_DragStartedEvent.HorizontalOffset, e.VerticalChange + np.m_DragStartedEvent.VerticalOffset);
            }
            else
            {

                int lowBound = 0;
                int highBound = 0;

                lowBound = origTime / 1440 * 1440;

                highBound = (origTime / 1440 + 1) * 1440;

                if (highBound > 7 * 1440)
                    highBound = 7 * 1440;

                int newTime = origTime + (int)((e.HorizontalChange + np.m_DragStartedEvent.HorizontalOffset) * 1440 / (np.ActualWidth - np.leftMargin));

                int granularity = newTime % 5;
                if (granularity < 3)
                    newTime = newTime - granularity;
                else
                    newTime = newTime + 5 - granularity;

                if (newTime < lowBound)
                {
                    newTime = lowBound;
                }

                if (newTime >= highBound)
                {
                    newTime = highBound - 1;
                }


                double verticalChange = Canvas.GetTop(th) + (e.VerticalChange + np.m_DragStartedEvent.VerticalOffset) - np.topMargin;

                double rowHeight = (np.ActualHeight - np.topMargin) / 8;

                int rowNum = (int)(verticalChange / rowHeight - 0.5);

                if (rowNum < 0)
                {
                    rowNum = 0;
                }

                if (rowNum > 6)
                {
                    th.Opacity = 0.5;
                    MoveByPosition(th, e.HorizontalChange + np.m_DragStartedEvent.HorizontalOffset, e.VerticalChange + np.m_DragStartedEvent.VerticalOffset);
                    return;
                }

                int origRow = newTime / 1440;

                newTime = newTime + (rowNum - origRow) * 1440;

                th.Opacity = 1;
                SetStartTime(th, newTime);
            }
        }
        static void th_DragStarted(object sender, DragStartedEventArgs e)
        {

            ScheduleThumb th = (ScheduleThumb)sender;
            Canvas cnv = (Canvas)(th.Parent);
            NixxisPlanning np = (NixxisPlanning)(cnv.Parent);
            np.m_DragStartedEvent = e;

            if (GetStartTime(th) >= 0)
            {
                th.DescriptivePopup.PlacementTarget = th.Parent as UIElement;
                th.DescriptivePopup.IsOpen = true;
            }
        }

        static private void MoveByPosition(ScheduleThumb th, double HorizontalChange, double VerticalChange)
        {
            Canvas cnv = (Canvas)(th.Parent);
            NixxisPlanning np = (NixxisPlanning)(cnv.Parent);

            double horiz = Canvas.GetLeft(th) + HorizontalChange;
            double verti = Canvas.GetTop(th) + VerticalChange;

            if (horiz < 0)
                horiz = 0;
            if (horiz > np.ActualWidth)
                horiz = np.ActualWidth;

            if (verti < 0)
                verti = 0;
            if (verti > np.ActualHeight)
                verti = np.ActualHeight;

            Canvas.SetZIndex(th, Int16.MaxValue);
            Canvas.SetTop(th, verti);
            Canvas.SetLeft(th, horiz);
        }

        static int ConvertThumbToTime(ScheduleThumb thumb)
        {
            ScheduleThumb th = (ScheduleThumb)thumb;
            Canvas cnv = (Canvas)(th.Parent);
            NixxisPlanning np = (NixxisPlanning)(cnv.Parent);

            double verticalPosition = Canvas.GetTop(th);
            double horizontalPosition = Canvas.GetLeft(th);
            double rowHeight = (np.ActualHeight - np.topMargin) / 8;

            int newTime = (int)((int)((double)(verticalPosition - np.topMargin) / (double)rowHeight - 0.5) * 1440.0 + (horizontalPosition - np.leftMargin) * 1440.0 / (np.ActualWidth - np.leftMargin));
            int granularity = newTime % 5;
            if (granularity < 3)
                newTime = newTime - granularity;
            else
                newTime = newTime + 5 - granularity;


            return newTime;
        }

        public int ConvertPointToTime(Point pt)
        {
            double verticalPosition = pt.Y;
            double horizontalPosition = pt.X;
            double rowHeight = (ActualHeight - topMargin) / 8;

            int newTime = (int)((int)((double)(verticalPosition - topMargin) / (double)rowHeight - 0.5) * 1440.0 + (horizontalPosition - leftMargin) * 1440.0 / (ActualWidth - leftMargin));
            int granularity = newTime % 5;
            if (granularity < 3)
                newTime = newTime - granularity;
            else
                newTime = newTime + 5 - granularity;

            return newTime;
        }

        public Point ConvertTimeToPoint(int time)
        {
            return new Point(leftMargin + time % 1440 * (ActualWidth - leftMargin) / 1440, topMargin + (time / 1440 + 1) * (ActualHeight - topMargin) / 8);
        }


        public NixxisPlanning()
        {

            InitializeComponent();

            m_Backup = Cursor;


            for (int i = 1; i <= 8; i++)
            {
                Line line = new Line();
                line.Opacity = 0.5;
                line.SnapsToDevicePixels = true;
                line.StrokeThickness = 1;
                line.Stroke = new SolidColorBrush(Colors.Yellow);

                line.X1 = 0;
                line.SetBinding(Line.X2Property, new Binding() { Source = this, Path = new PropertyPath(UserControl.ActualWidthProperty) });

                line.SetBinding(Line.Y1Property, new Binding() { Source = this, Path = new PropertyPath(UserControl.ActualHeightProperty), Converter = new SpecialConverter2(), ConverterParameter = new double[] { (double)i / 8.0, topMargin } });
                line.SetBinding(Line.Y2Property, new Binding() { Source = this, Path = new PropertyPath(UserControl.ActualHeightProperty), Converter = new SpecialConverter2(), ConverterParameter = new double[] { (double)i / 8.0, topMargin } });

                DrawingCanvas.Children.Add(line);
                if (i != 8)
                {
                    TextBlock tb = new TextBlock();
                    tb.SnapsToDevicePixels = true;
                    switch (i)
                    {
                        case 1:
                            tb.Text = TranslationContext.Default.Translate("Monday");
                            break;
                        case 2:
                            tb.Text = TranslationContext.Default.Translate("Tuesday");
                            break;
                        case 3:
                            tb.Text = TranslationContext.Default.Translate("Wednesday");
                            break;
                        case 4:
                            tb.Text = TranslationContext.Default.Translate("Thursday");
                            break;
                        case 5:
                            tb.Text = TranslationContext.Default.Translate("Friday");
                            break;
                        case 6:
                            tb.Text = TranslationContext.Default.Translate("Saturday");
                            break;
                        case 7:
                            tb.Text = TranslationContext.Default.Translate("Sunday");
                            break;
                    }

                    tb.Margin = new Thickness(0, -10, 0, 0);
                    tb.SetBinding(Canvas.TopProperty, new Binding() { Source = this, Path = new PropertyPath(UserControl.ActualHeightProperty), Converter = new SpecialConverter(), ConverterParameter = new double[] { (double)i / 8.0, topMargin } });
                    Canvas.SetLeft(tb, 0);

                    DrawingCanvas.Children.Add(tb);
                }
            }

            for (int i = 1; i < 24; i++)
            {
                Line line = new Line();
                line.Opacity = 0.5;
                line.SnapsToDevicePixels = true;
                line.StrokeThickness = 1;
                line.Stroke = new SolidColorBrush(Colors.Yellow);

                line.SetBinding(Line.X1Property, new Binding() { Source = this, Path = new PropertyPath(UserControl.ActualWidthProperty), Converter = new SpecialConverter(), ConverterParameter = new double[] { (double)i / 24, leftMargin } });
                line.SetBinding(Line.X2Property, new Binding() { Source = this, Path = new PropertyPath(UserControl.ActualWidthProperty), Converter = new SpecialConverter(), ConverterParameter = new double[] { (double)i / 24, leftMargin } });
                line.Y1 = topMargin;
                line.SetBinding(Line.Y2Property, new Binding() { Source = this, Path = new PropertyPath(UserControl.ActualHeightProperty) });


                DrawingCanvas.Children.Add(line);

                TextBlock tb = new TextBlock();
                tb.SnapsToDevicePixels = true;
                tb.Text = i.ToString();
                tb.Margin = new Thickness(-5, 0, 0, 0);

                tb.SetBinding(Canvas.LeftProperty, new Binding() { Source = this, Path = new PropertyPath(UserControl.ActualWidthProperty), Converter = new SpecialConverter(), ConverterParameter = new double[] { (double)i / 24, leftMargin } });
                Canvas.SetTop(tb, 0);

                DrawingCanvas.Children.Add(tb);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            Refresh();
        }

        private void Refresh()
        {
            try
            {
                foreach (UIElement uie in DrawingCanvas.Children)
                {
                    if (uie is ScheduleThumb)
                        StartTime_Changed(uie, new DependencyPropertyChangedEventArgs());
                }
            }
            catch
            {
            }
        }

        private void DrawingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_LastMouseEvent = null;
        }

        private void DrawingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (IsReadOnly)
            {
                if (!IsSelectable)
                    return;

                if (e.OriginalSource is TextBlock || e.OriginalSource is Line)
                {
                    ScheduleLine selected = FromUIElement(e.OriginalSource as UIElement);
                    if (selected != null)
                    {
                        Selected = selected;
                    }
                    else
                    {
                        Selected = null;
                    }
                }
                else
                {
                    Selected = null;
                }
                return;
            }

            int time = ConvertPointToTime(e.GetPosition(DrawingCanvas));
            if (time >= 0 && time <= 1440 * 7)
            {
                RaiseEvent(new RoutedEventArgs(NixxisPlanning.NewTimeEvent, new object[] { time, true }));

                m_LastMouseEvent = e;

                RaiseEvent(new RoutedEventArgs(NixxisPlanning.NewTimeEvent, new object[] { time, false }));

            }

        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point loc = e.GetPosition(this);
            if (loc.X > leftMargin && loc.X < ActualWidth && loc.Y > topMargin && loc.Y < ActualHeight && !IsReadOnly)
            {
                if (Cursor != Cursors.Cross)
                {
                    m_Backup = Cursor;
                    Cursor = Cursors.Cross;
                }
            }
            else
            {

                Cursor = m_Backup;
            }
        }

        public ScheduleLine FromUIElement(UIElement element)
        {
            for (int i = 0; i < ScheduleLines.Count; i++)
            {
                ScheduleLine schedLine = ScheduleLines[i];
                if (schedLine.m_TextBox == element)
                {
                    return schedLine;
                }
                else
                {
                    foreach (Line ln in schedLine.m_Lines)
                    {
                        if (ln == element)
                        {
                            return schedLine;
                        }
                    }
                }
            }
            return null;
        }


        public int RemoveScheduleFromThumb(ScheduleThumb th)
        {
            int countRemoved = 0;

            for (int i = 0; i < ScheduleLines.Count; i++)
            {
                ScheduleLine schedLine = ScheduleLines[i];
                if (schedLine.Associated == th)
                {
                    ScheduleLines.RemoveAt(i);

                    while (schedLine.m_Lines.Count != 0)
                    {
                        schedLine.m_Planning.DrawingCanvas.Children.Remove(schedLine.m_Lines[schedLine.m_Lines.Count - 1]);
                        schedLine.m_Lines.RemoveAt(schedLine.m_Lines.Count - 1);

                        countRemoved++;
                    }


                    BindingOperations.ClearAllBindings(schedLine.m_TextBox);
                    schedLine.m_Planning.DrawingCanvas.Children.Remove(schedLine.m_TextBox);

                    i--;
                }
            }

            return countRemoved;
        }


    }

    public class ScheduleLine
    {
        public NixxisPlanning m_Planning;
        private int m_StartTime;
        private int m_EndTime;
        private bool m_Hidden;
        private bool m_IsSelected;
        private bool m_IsOpen = false;
        private ScheduleThumb m_Associated;
        public List<Line> m_Lines = new List<Line>();
        public TextBlock m_TextBox = new TextBlock();

        public ScheduleThumb Associated
        {
            get
            {
                return m_Associated;
            }
        }
        public int StartTime
        {
            get
            {
                return m_StartTime;
            }
            set
            {
                m_StartTime = value;
                Redraw();
            }
        }
        public int EndTime
        {
            get
            {
                return m_EndTime;
            }
            set
            {
                m_EndTime = value;
                Redraw();
            }
        }
        public bool Hidden
        {
            get
            {
                return m_Hidden;
            }
            set
            {
                m_Hidden = value;
                Redraw();
            }
        }
        public bool IsSelected
        {
            get
            {
                return m_IsSelected;
            }
            set
            {
                m_IsSelected = value;
                Redraw();
            }

        }

        private static string TimeDescription(int time)
        {
            int day = time / 1440;
            int HoursMinutes = time - 1440 * day;
            string strDay = string.Empty;
            switch (day)
            {
                case 0:
                    strDay = TranslationContext.Default.Translate("Monday");
                    break;
                case 1:
                    strDay = TranslationContext.Default.Translate("Tuesday");
                    break;
                case 2:
                    strDay = TranslationContext.Default.Translate("Wednesday");
                    break;
                case 3:
                    strDay = TranslationContext.Default.Translate("Thursday");
                    break;
                case 4:
                    strDay = TranslationContext.Default.Translate("Friday");
                    break;
                case 5:
                    strDay = TranslationContext.Default.Translate("Saturday");
                    break;
                case 6:
                    strDay = TranslationContext.Default.Translate("Sunday");
                    break;
            }
            int hours = HoursMinutes / 60;
            int minutes = HoursMinutes - 60 * hours;
            return string.Format("{0} {1:00}:{2:00}", strDay, hours, minutes);
        }
        private string Description
        {
            get
            {
                return string.Format(TranslationContext.Default.Translate("{0} from {1} to {2}"), m_IsOpen ? TranslationContext.Default.Translate("Opened"): TranslationContext.Default.Translate("Closed"), TimeDescription(m_StartTime), TimeDescription(m_EndTime));
            }
        }


        public ScheduleLine(NixxisPlanning owner, ScheduleThumb associated, bool isOpen)
        {
            m_Planning = owner;
            m_Associated = associated;
            m_IsOpen = isOpen;
            m_TextBox.TextAlignment = TextAlignment.Center;
            m_TextBox.Tag = this;
            m_TextBox.Padding = new Thickness(5, 0, 5, 0);
            m_TextBox.TextTrimming = TextTrimming.CharacterEllipsis;
            m_TextBox.SetBinding(TextBlock.ToolTipProperty, new Binding() { Source = m_TextBox, Path = new PropertyPath(TextBlock.TextProperty) });
            Canvas.SetZIndex(m_TextBox, int.MaxValue - 1);
            m_Planning.DrawingCanvas.Children.Add(m_TextBox);
            Redraw();
        }

        private void Redraw()
        {
            int currentTime = m_StartTime;

            Point textPoint = new Point(0, 0);
            double textWidth = 0;

            List<int> transitions = new List<int>();
            if (m_StartTime < m_EndTime)
            {
                transitions.Add(m_StartTime);
                for (int i = 0; i < 8; i++)
                {
                    if (i * 1440 - 1 >= m_StartTime && i * 1440 <= m_EndTime)
                    {
                        transitions.Add(i * 1440 - 1);
                        transitions.Add(i * 1440);
                    }
                }
                transitions.Add(m_EndTime);
            }
            else if (m_StartTime == m_EndTime)
            {
                if (m_Planning.sortedItems.Count() == 1)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        transitions.Add(i * 1440);
                        transitions.Add(i * 1440 + 1439);
                    }
                }
                else
                {
                    transitions.Add(m_StartTime);
                    transitions.Add(m_EndTime);
                }
            }
            else
            {

                transitions.Add(m_StartTime);

                for (int i = 0; i < 8; i++)
                {
                    if (i * 1440 - 1 >= m_StartTime)
                    {
                        transitions.Add(i * 1440 - 1);
                    }
                    if (i * 1440 >= m_StartTime && i != 7)
                    {
                        transitions.Add(i * 1440);
                    }
                }

                for (int i = 0; i <= 8; i++)
                {
                    if (i * 1440 <= m_EndTime)
                    {
                        transitions.Add(i * 1440);
                    }
                    if (i * 1440 + 1439 <= m_EndTime)
                    {
                        transitions.Add(i * 1440 + 1439);
                    }
                }

                transitions.Add(m_EndTime);
            }

            int lineIndex = 0;

            for (int i = 0; i < transitions.Count - 1; i += 2)
            {
                Line ln = null;
                if (m_Lines.Count <= lineIndex)
                {
                    ln = new Line();
                    ln.Tag = this;

                    ln.Opacity = 0.5;


                    if (m_IsOpen)
                        ln.Stroke = new SolidColorBrush(Colors.Green);
                    else
                        ln.Stroke = new SolidColorBrush(Colors.Red);

                    m_Planning.DrawingCanvas.Children.Add(ln);
                    m_Lines.Add(ln);
                }
                else
                {

                    ln = m_Lines[lineIndex];
                }

                if (IsSelected)
                {
                    ln.OpacityMask = new LinearGradientBrush(new GradientStopCollection() { new GradientStop(Color.FromArgb(0, 0, 0, 0), 0), new GradientStop(Color.FromArgb(0, 0, 0, 0), 0.5), new GradientStop(Color.FromArgb(255, 0, 0, 0), 0.5), new GradientStop(Color.FromArgb(255, 0, 0, 0), 1) }, new Point(0, 0), new Point(5, 5)) { SpreadMethod = GradientSpreadMethod.Reflect, MappingMode = BrushMappingMode.Absolute };
                }
                else
                {
                    ln.OpacityMask = null;
                }


                ln.StrokeThickness = (m_Planning.ActualHeight - m_Planning.topMargin) / 8;

                if (m_Hidden)
                {
                    ln.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ln.Visibility = Visibility.Visible;
                }

                Point pt1 = m_Planning.ConvertTimeToPoint(transitions[i]);
                Point pt2 = m_Planning.ConvertTimeToPoint(transitions[i + 1]);

                ln.X1 = pt1.X;
                ln.Y1 = pt1.Y;
                ln.X2 = pt2.X;
                ln.Y2 = pt2.Y;

                lineIndex++;
            }

            if (transitions.Count >= 6)
            {
                // put it inb the middle of m_Lines[ (transitions.Count + 1)/2 -1]
                int index = (transitions.Count / 2 + 1) / 2 - 1;
                textPoint = new Point(m_Lines[index].X1, m_Lines[index].Y1);
                textWidth = m_Lines[index].X2 - m_Lines[index].X1;
            }
            else
            { // choose the largest line
                if (transitions.Count == 4)
                {
                    int best = 0;
                    double longer = 0;
                    for (int i = 0; i < 2; i++)
                    {
                        double tempLength = m_Lines[i].X2 - m_Lines[i].X1;
                        if (tempLength > longer)
                        {
                            longer = tempLength;
                            best = i;
                        }
                    }
                    textPoint = new Point(m_Lines[best].X1, m_Lines[best].Y1);
                    textWidth = longer;
                }
                else
                {
                    textPoint = new Point(m_Lines[0].X1, m_Lines[0].Y1);
                    textWidth = m_Lines[0].X2 - m_Lines[0].X1;
                }
            }

            if (textWidth >= 0)
            {
                m_TextBox.Width = textWidth;
                Canvas.SetLeft(m_TextBox, textPoint.X);
                if (m_TextBox.ActualHeight == 0)
                {
                    m_TextBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetTop(m_TextBox, textPoint.Y - m_TextBox.DesiredSize.Height / 2);
                }
                else
                {
                    Canvas.SetTop(m_TextBox, textPoint.Y - m_TextBox.ActualHeight / 2);
                }

                m_TextBox.Text = Description;
            }


            while (lineIndex != m_Lines.Count)
            {
                int counter = m_Planning.DrawingCanvas.Children.Count;
                m_Planning.DrawingCanvas.Children.Remove(m_Lines[m_Lines.Count - 1]);
                m_Lines.RemoveAt(m_Lines.Count - 1);
            }

        }
    }

    public class ScheduleThumb : Thumb
    {
        public class SpecialConverter : IValueConverter
        {

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return (double)value + 10;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }


        public Popup DescriptivePopup { get; set; }

        public ScheduleThumb(object bindingSource)
        {
            SetResourceReference(ScheduleThumb.StyleProperty, "NuclearSliderThumb");
            Cursor = Cursors.Hand;
            DescriptivePopup = new Popup();
            DescriptivePopup.Placement = PlacementMode.Relative;
            DescriptivePopup.Name = "DescriptivePopup";
            DescriptivePopup.AllowsTransparency = true;
            DescriptivePopup.SetBinding(Popup.HorizontalOffsetProperty, new Binding() { Source = this, Path = new PropertyPath(Canvas.LeftProperty) });
            DescriptivePopup.SetBinding(Popup.VerticalOffsetProperty, new Binding() { Source = this, Path = new PropertyPath(Canvas.TopProperty), Converter = new SpecialConverter() });

            this.AddLogicalChild(DescriptivePopup);
            Border border = new Border();
            TextBlock tb = new TextBlock();
            tb.Margin = new Thickness(3);

            tb.SetBinding(TextBlock.TextProperty, new Binding() { Source = bindingSource, Converter = new NixxisPlanning.TimeOfDayConverter() });

            border.Child = tb;
            DescriptivePopup.Child = border;
        }
    }

}
