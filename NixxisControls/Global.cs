using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Markup;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Net;
using System.IO;
using System.Configuration;

namespace Nixxis.Client.Controls
{
    public class GeneralCommands
    {
        public static RoutedUICommand ShowApplication { get; private set; }

        static GeneralCommands()
        {
            ShowApplication = new RoutedUICommand(string.Empty, "ShowApplication", typeof(GeneralCommands));
        }
    }

    public class PersistentRoutedUICommand : RoutedUICommand, INotifyPropertyChanged
    {
        public PersistentRoutedUICommand()
            : base()
        {
        }
        public PersistentRoutedUICommand(string text, string name, Type ownerType)
            : base(text, name, ownerType)
        {
        }

        public PersistentRoutedUICommand(string text, string name, Type ownerType, InputGestureCollection inputGestures)
            : base(text, name, ownerType, inputGestures)
        {
        }

        private object m_Object = null;

        public object State
        {
            get
            {
                return m_Object;
            }
            set
            {
                m_Object = value;
                FirePropertyChanged("State");
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }

    public class FormatStringExtension : MarkupExtension
    {
        class ConcatStringSingleBinding : IValueConverter
        {
            public string FormatString { get; set; }
            public TranslationContext TranslationContext { get; set; }

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value == DependencyProperty.UnsetValue)
                    return string.Empty;

                if (TranslationContext == null)
                    return string.Format(FormatString, value);
                return string.Format(TranslationContext.Translate(FormatString), value);
            }

            public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class ConcatStringMultiBinding : IMultiValueConverter
        {
            public string FormatString { get; set; }
            public TranslationContext TranslationContext { get; set; }

            public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                foreach (object obj in values)
                    if (obj == DependencyProperty.UnsetValue)
                        return string.Empty;

                if (TranslationContext == null)
                    return string.Format(FormatString, values);

                return string.Format(TranslationContext.Translate(FormatString), values);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }


        public BindingBase BindTo { get; set; }
        public string FormatString { get; set; }
        public TranslationContext TranslationContext { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (BindTo is MultiBinding)
                ((MultiBinding)BindTo).Converter = new ConcatStringMultiBinding { FormatString = FormatString, TranslationContext = TranslationContext };
            else
                ((Binding)BindTo).Converter = new ConcatStringSingleBinding { FormatString = FormatString, TranslationContext = TranslationContext };

            return BindTo.ProvideValue(serviceProvider);
        }
    }


    public static class DurationHelpers
    {

        public static string GetDefaultDurationString(Decimal duration, bool forceSign)
        {
            try
            {
                string symbol = string.Empty;
                if (forceSign)
                    symbol = "+";


                if (duration < 0)
                    symbol = "-";

                duration = Math.Abs(duration);
                long value = (long)(Decimal.Floor(duration));
                decimal milisec = duration - (Decimal)value;
                long days = 0;
                int hours = 0;
                int minutes = 0;
                int seconds = 0;


                days = value / (24 * 60 * 60);
                value = value - days * (24 * 60 * 60);
                hours = (int)(value / (60 * 60));
                value = value - hours * (60 * 60);
                minutes = (int)(value / 60);
                value = value - minutes * 60;
                seconds = (int)value;


                if (days > 0)
                {
                    if (hours > 0)
                    {
                        if (minutes > 0)
                        {
                            if (seconds > 0)
                            {
                                return string.Format("{8}{0} {1} {2} {3} {4} {5} {6} {7}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"),
                                    seconds, seconds > 1 ? TranslationContext.Default.Translate("secs") : TranslationContext.Default.Translate("sec"),symbol);
                            }
                            else
                            {
                                return string.Format("{6}{0} {1} {2} {3} {4} {5}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"), symbol);
                            }
                        }
                        else
                        {
                            if (seconds > 0)
                            {
                                return string.Format("{8}{0} {1} {2} {3} {4} {5} {6} {7}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"),
                                    seconds, seconds > 1 ? TranslationContext.Default.Translate("secs") : TranslationContext.Default.Translate("sec"), symbol);
                            }
                            else
                            {
                                return string.Format("{4}{0} {1} {2} {3}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"), symbol
                                    );
                            }
                        }
                    }
                    else
                    {
                        if (minutes > 0)
                        {
                            if (seconds > 0)
                            {
                                return string.Format("{8}{0} {1} {2} {3} {4} {5} {6} {7}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"),
                                    seconds, seconds > 1 ? TranslationContext.Default.Translate("secs") : TranslationContext.Default.Translate("sec"), symbol);
                            }
                            else
                            {
                                return string.Format("{6}{0} {1} {2} {3} {4} {5}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"), symbol);
                            }
                        }
                        else
                        {
                            if (seconds > 0)
                            {
                                return string.Format("{8}{0} {1} {2} {3} {4} {5} {6} {7}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"),
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"),
                                    seconds, seconds > 1 ? TranslationContext.Default.Translate("secs") : TranslationContext.Default.Translate("sec"), symbol);
                            }
                            else
                            {
                                return string.Format("{2}{0} {1}",
                                    days, days > 1 ? TranslationContext.Default.Translate("days") : TranslationContext.Default.Translate("day"), symbol
                                    );
                            }
                        }
                    }
                }
                else
                {
                    if (hours > 0)
                    {
                        if (minutes > 0)
                        {
                            if (seconds > 0)
                            {
                                return string.Format("{6}{0} {1} {2} {3} {4} {5}",
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"),
                                    seconds, seconds > 1 ? TranslationContext.Default.Translate("secs") : TranslationContext.Default.Translate("sec"), symbol);
                            }
                            else
                            {
                                return string.Format("{4}{0} {1} {2} {3}",
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("minutes") : TranslationContext.Default.Translate("minute"), symbol);
                            }
                        }
                        else
                        {
                            if (seconds > 0)
                            {
                                return string.Format("{6}{0} {1} {2} {3} {4} {5}",
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"),
                                    minutes, minutes > 1 ? TranslationContext.Default.Translate("min") : TranslationContext.Default.Translate("min"),
                                    seconds, seconds > 1 ? TranslationContext.Default.Translate("secs") : TranslationContext.Default.Translate("sec"), symbol);
                            }
                            else
                            {
                                return string.Format("{2}{0} {1}",
                                    hours, hours > 1 ? TranslationContext.Default.Translate("hours") : TranslationContext.Default.Translate("hour"), symbol
                                    );
                            }
                        }
                    }
                    else
                    {
                        if (minutes > 0)
                        {
                            if (seconds > 0)
                            {
                                if (milisec == 0)
                                {
                                    return string.Format("{4}{0} {1} {2} {3}",
                                        minutes, minutes > 1 ? TranslationContext.Default.Translate("minutes") : TranslationContext.Default.Translate("minute"),
                                        seconds, seconds > 1 ? TranslationContext.Default.Translate("seconds") : TranslationContext.Default.Translate("second"), symbol);
                                }
                                else
                                {
                                    return string.Format("{4}{0} {1} {2:0.000} {3}",
                                        minutes, minutes > 1 ? TranslationContext.Default.Translate("minutes") : TranslationContext.Default.Translate("minute"),
                                        seconds + milisec, seconds > 1 ? TranslationContext.Default.Translate("seconds") : TranslationContext.Default.Translate("second"), symbol);
                                }

                            }
                            else
                            {
                                if (milisec == 0)
                                {
                                    return string.Format("{2}{0} {1}",
                                        minutes, minutes > 1 ? TranslationContext.Default.Translate("minutes") : TranslationContext.Default.Translate("minute"), symbol);
                                }
                                else
                                {
                                    return string.Format("{4}{0} {1} {2:0.000} {3}",
                                        minutes, minutes > 1 ? TranslationContext.Default.Translate("minutes") : TranslationContext.Default.Translate("minute"), milisec, TranslationContext.Default.Translate("second"), symbol);
                                }
                            }
                        }
                        else
                        {
                            if (seconds > 0)
                            {
                                if (milisec == 0)
                                {
                                    return string.Format("{2}{0} {1}",
                                        seconds, seconds > 1 ? TranslationContext.Default.Translate("seconds") : TranslationContext.Default.Translate("second"), symbol);
                                }
                                else
                                {
                                    return string.Format("{2}{0:0.000} {1}",
                                        seconds + milisec, seconds > 1 ? TranslationContext.Default.Translate("seconds") : TranslationContext.Default.Translate("second"), symbol);
                                }
                            }
                            else
                            {
                                if (milisec == 0)
                                {
                                    return string.Format("{2}{0} {1}",
                                        0, TranslationContext.Default.Translate("second"), symbol
                                        );
                                }
                                else
                                {
                                    return string.Format("{2}{0:0.000} {1}",
                                        milisec, TranslationContext.Default.Translate("second"), symbol
                                        );

                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static Decimal SplitDurationString(string strWorking)
        {
            Decimal nDays = 0;
            Decimal nHours = 0;
            Decimal nMinutes = 0;
            Decimal nSeconds = 0;

            string strNumeric = "0123456789,.";
            strWorking = strWorking.ToLower().Replace(" ", string.Empty);
            string strNumber = string.Empty;
            string strUnit = string.Empty;
            bool inUnit = false;
            bool isNegative = false;

            if (strWorking.StartsWith("-"))
            {
                isNegative = true;
                strWorking = strWorking.Substring(1);
            }
            else if (strWorking.StartsWith("+"))
            {
                strWorking = strWorking.Substring(1);
            }


            for (int i = 0; i < strWorking.Length; i++)
            {
                string curChar = strWorking.Substring(i, 1);

                if (inUnit)
                {
                    if (strNumeric.Contains(curChar))
                    {
                        // check what is the unit to affect right value;
                        int counter = 0;
                        bool unitMatch = false;
                        while (true)
                        {
                            if (counter < strDays.Length && strDays[counter].Equals(strUnit))
                            {
                                if (!Decimal.TryParse(strNumber, out nDays))
                                    return -1;
                                unitMatch = true;
                                break;
                            }
                            else if (counter < strHours.Length && strHours[counter].Equals(strUnit))
                            {
                                if (!Decimal.TryParse(strNumber, out nHours))
                                    return -1;
                                unitMatch = true;
                                break;
                            }
                            else if (counter < strMinutes.Length && strMinutes[counter].Equals(strUnit))
                            {
                                if (!Decimal.TryParse(strNumber, out nMinutes))
                                    return -1;
                                unitMatch = true;
                                break;
                            }
                            else if (counter < strSeconds.Length && strSeconds[counter].Equals(strUnit))
                            {
                                if (!Decimal.TryParse(strNumber, out nSeconds))
                                    return -1;
                                unitMatch = true;
                                break;
                            }

                            counter++;
                            if (counter >= strDays.Length && counter >= strHours.Length && counter >= strMinutes.Length && counter >= strSeconds.Length)
                                break;
                        }

                        if (!unitMatch)
                            return -1;

                        inUnit = false;
                        strNumber = curChar;
                    }
                    else
                    {
                        strUnit = string.Concat(strUnit, curChar);
                    }

                }
                else
                {
                    if (strNumeric.Contains(curChar))
                    {
                        strNumber = string.Concat(strNumber, curChar);
                    }
                    else
                    {
                        inUnit = true;
                        strUnit = curChar;
                    }
                }
            }

            if (inUnit)
            {
                int counter = 0;
                bool unitMatch = false;
                while (true)
                {
                    if (counter < strDays.Length && strDays[counter].Equals(strUnit))
                    {
                        if (!Decimal.TryParse(strNumber, out nDays))
                            return -1;
                        unitMatch = true;
                        break;

                    }
                    else if (counter < strHours.Length && strHours[counter].Equals(strUnit))
                    {
                        if (!Decimal.TryParse(strNumber, out nHours))
                            return -1;
                        unitMatch = true;
                        break;

                    }
                    else if (counter < strMinutes.Length && strMinutes[counter].Equals(strUnit))
                    {
                        if (!Decimal.TryParse(strNumber, out nMinutes))
                            return -1;
                        unitMatch = true;
                        break;

                    }
                    else if (counter < strSeconds.Length && strSeconds[counter].Equals(strUnit))
                    {
                        if (!Decimal.TryParse(strNumber, out nSeconds))
                            return -1;
                        unitMatch = true;
                        break;

                    }

                    counter++;
                    if (counter >= strDays.Length && counter >= strHours.Length && counter >= strMinutes.Length && counter >= strSeconds.Length)
                        break;
                }
                if (!unitMatch)
                    return -1;

            }
            else
            {
                // let's imagine it is about seconds?
                if (!Decimal.TryParse(strNumber, out nSeconds))
                    return -1;
            }
            return (nDays * 60 * 60 * 24 + nHours * 60 * 60 + nMinutes * 60 + nSeconds) * (isNegative ? -1: 1);
        }

        private static string[] strDays = TranslationContext.Default.Translate("days;day;d").Split(';');
        private static string[] strHours = TranslationContext.Default.Translate("hours;hour;h").Split(';');
        private static string[] strMinutes = TranslationContext.Default.Translate("minutes;minute;min;m").Split(';');
        private static string[] strSeconds = TranslationContext.Default.Translate("seconds;second;sec;s").Split(';');
    }

    public static class TreeViewHelpers
    {
        public static void SelectItem(this ItemsControl parentContainer, List<object> path)
        {
            var head = path.First();
            var tail = path.GetRange(1, path.Count - 1);
            var itemContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(head) as TreeViewItem;

            if (itemContainer != null && (itemContainer.Items.Count == 0 || tail.Count==0 ))
            {
                try
                {
                    itemContainer.IsSelected = true;
                }
                catch
                {
                }

                var selectMethod = typeof(TreeViewItem).GetMethod("Select", BindingFlags.NonPublic | BindingFlags.Instance);
                selectMethod.Invoke(itemContainer, new object[] { true });
            }
            else if (itemContainer != null)
            {
                itemContainer.IsExpanded = true;

                if (itemContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                {
                    itemContainer.ItemContainerGenerator.StatusChanged += delegate
                    {
                        SelectItem(itemContainer, tail);
                    };
                }
                else
                {
                    SelectItem(itemContainer, tail);
                }
            }
        }

        public static void UnSelectItem(this ItemsControl parentContainer, object item)
        {
            var itemContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

            if (itemContainer != null)
            {
                try
                {
                    itemContainer.IsSelected = false;
                }
                catch { }
            }
        }
    }

    public class Helpers
    {
        public static long GetFileSize(string path)
        {
            long retValue = -1;
            try
            {
                if (path.StartsWith("http://"))
                {
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(path);
                    webRequest.Method = WebRequestMethods.Http.Head;
                    try
                    {
                        using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                retValue = 0;
                                retValue = long.Parse(response.Headers[HttpResponseHeader.ContentLength]);
                            }
                            response.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.ToString());
                    }
                }
                else if (path.StartsWith("file:///"))
                {
                    string temp = path.Substring("file:///".Length).Replace("/", "\\");
                    retValue = (new FileInfo(Uri.UnescapeDataString(temp))).Length;
                }
                else if (path.StartsWith("file://"))
                {
                    string temp = path.Substring("file:".Length).Replace("/", "\\");
                    retValue = (new FileInfo(Uri.UnescapeDataString(temp))).Length;
                }
            }
            catch
            {
            }
            return retValue;
        }


        private static WaitScreen m_WaitScreen;

        public static void StartLongRunningTask(DoWorkEventHandler taskToRun)
        {
            if (m_WaitScreen != null)
                throw new NotSupportedException();

            BackgroundWorker worker = new BackgroundWorker();
            m_WaitScreen = new WaitScreen();

            worker.DoWork += taskToRun;
            worker.RunWorkerCompleted += WorkerCompleted;
            worker.RunWorkerAsync();
            if (Application.Current.MainWindow.IsLoaded)
                m_WaitScreen.Owner = Application.Current.MainWindow;
            m_WaitScreen.ShowDialog();
            m_WaitScreen = null;
        }

        private static void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_WaitScreen.Close();
            m_WaitScreen = null;
        }

        public static void ApplyToChildren<T>(FrameworkElement element, DependencyProperty property, object value) where T : FrameworkElement
        {
            if (element == null)
                return;

            foreach (object fe in LogicalTreeHelper.GetChildren(element))
            {
                if (fe is FrameworkElement)
                    InternalApplyToChildren<T>((FrameworkElement)fe, property, value);
            }
        }

        private static void InternalApplyToChildren<T>(FrameworkElement element, DependencyProperty property, object value) where T : FrameworkElement
        {
            if (element == null)
                return;

            if (element is T && element is DependencyObject)
            {
                ((DependencyObject)element).SetValue(property, value);
            }

            foreach (object fe in LogicalTreeHelper.GetChildren(element))
            {
                FrameworkElement felem = fe as FrameworkElement;
                if (felem!=null)
                    InternalApplyToChildren<T>(felem, property, value);
            }
        }

        public static void ApplyToChildren<T>(FrameworkElement element, DependencyProperty property, object value, Func<FrameworkElement, bool> condition) where T : FrameworkElement
        {
            if (element == null)
                return;


            foreach (object fe in LogicalTreeHelper.GetChildren(element))
            {
                if (fe is FrameworkElement)
                    InternalApplyToChildren<T>((FrameworkElement)fe, property, value, condition);
            }
        }

        private static void InternalApplyToChildren<T>(FrameworkElement element, DependencyProperty property, object value, Func<FrameworkElement, bool> condition) where T : FrameworkElement
        {
            if (element == null)
                return;


            if (element is T && element is DependencyObject && condition(element))
            {
                ((DependencyObject)element).SetValue(property, value);
            }

            foreach (object fe in LogicalTreeHelper.GetChildren(element))
            {
                if (fe is FrameworkElement)
                    InternalApplyToChildren<T>((FrameworkElement)fe, property, value, condition);
            }

        }

        public static T FindParent<T>(FrameworkElement element) where T : FrameworkElement
        {
            FrameworkElement parent = LogicalTreeHelper.GetParent(element) as FrameworkElement;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                    return correctlyTyped;
                else
                    return FindParent<T>(parent);
            }

            return null;
        }

        /// <summary>
        ///     Walks up the templated parent tree looking for a parent type.
        /// </summary>
        public static T FindTemplatedParent<T>(FrameworkElement element) where T : FrameworkElement
        {
            FrameworkElement parent = element.TemplatedParent as FrameworkElement;

            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = parent.TemplatedParent as FrameworkElement;
            }

            return null;
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }

        public static DependencyObject FindFirstControlInChildren(DependencyObject obj, string controlType)
        {
            if (obj == null)
                return null;

            // Get a list of all occurrences of a particular type of control (eg "CheckBox") 
            IEnumerable<DependencyObject> ctrls = FindInVisualTreeDown(obj, controlType);
            if (ctrls.Count() == 0)
                return null;

            return ctrls.First();
        }

        public static IEnumerable<DependencyObject> FindInVisualTreeDown(DependencyObject obj, string type)
        {
            if (obj != null)
            {
                if (obj.GetType().ToString().EndsWith(type))
                {
                    yield return obj;
                }

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    foreach (var child in FindInVisualTreeDown(VisualTreeHelper.GetChild(obj, i), type))
                    {
                        if (child != null)
                        {
                            yield return child;
                        }
                    }
                }
            }
            yield break;
        }

        public static FrameworkElement[] FindChildren<T>(FrameworkElement element, Func<FrameworkElement, bool> condition) where T : FrameworkElement
        {
            List<FrameworkElement> result = new List<FrameworkElement>();

            if (element == null)
                return result.ToArray<FrameworkElement>();


            foreach (object fe in LogicalTreeHelper.GetChildren(element))
            {
                if (fe is FrameworkElement)
                {
                    FrameworkElement[] temp = InternalFindChildren<T>((FrameworkElement)fe, condition);
                    if (temp != null)
                    {
                        result.AddRange(temp);
                    }

                }
            }
            return result.ToArray<FrameworkElement>();
        }

        private static FrameworkElement[] InternalFindChildren<T>(FrameworkElement element, Func<FrameworkElement, bool> condition) where T : FrameworkElement
        {
            List<FrameworkElement> result = new List<FrameworkElement>();

            if (element == null)
                return result.ToArray<FrameworkElement>();


            if (element is T && element is DependencyObject && condition(element))
            {
                result.Add(element);
            }

            foreach (object fe in LogicalTreeHelper.GetChildren(element))
            {
                if (fe is FrameworkElement)
                {
                    FrameworkElement[] temp = InternalFindChildren<T>((FrameworkElement)fe, condition);
                    if (temp != null)
                        result.AddRange(temp);
                }
            }
            return result.ToArray<FrameworkElement>();
        }

        public static FrameworkElement FindChild<T>(FrameworkElement element, Func<FrameworkElement, bool> condition) where T : FrameworkElement
        {

            if (element == null)
                return null;


            foreach (object fe in LogicalTreeHelper.GetChildren(element))
            {
                if (fe is FrameworkElement)
                {
                    FrameworkElement temp = InternalFindChild<T>((FrameworkElement)fe, condition);
                    if (temp != null)
                    {
                        return temp;
                    }
                }
            }
            return null;
        }

        private static FrameworkElement InternalFindChild<T>(FrameworkElement element, Func<FrameworkElement, bool> condition) where T : FrameworkElement
        {
            if (element == null)
                return null;


            if (element is T && element is DependencyObject && condition(element))
            {
                return element;
            }

            foreach (object fe in LogicalTreeHelper.GetChildren(element))
            {
                if (fe is FrameworkElement)
                {
                    FrameworkElement temp = InternalFindChild<T>((FrameworkElement)fe, condition);
                    if (temp != null)
                        return temp;
                }
            }
            return null;
        }

        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T TryFindParent<T>(DependencyObject child)
          where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }


        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Do note, that for content element,
        /// this method falls back to the logical tree of the element.
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(DependencyObject child)
        {
            if (child == null) return null;
            ContentElement contentElement = child as ContentElement;

            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //if it's not a ContentElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }


        public static T TryFindFromPoint<T>(UIElement reference, Point point)
  where T : DependencyObject
        {
            DependencyObject element = reference.InputHitTest(point)
                                         as DependencyObject;
            if (element == null) return null;
            else if (element is T) return (T)element;
            else return TryFindParent<T>(element);
        }


        public static void ApplyToVisualChildren<T>(DependencyObject element, DependencyProperty property, object value) where T : FrameworkElement
        {
            if (element == null)
                return;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                InternalApplyToVisualChildren<T>(VisualTreeHelper.GetChild(element, i), property, value);
            }
        }

        private static void InternalApplyToVisualChildren<T>(DependencyObject element, DependencyProperty property, object value) where T : FrameworkElement
        {
            if (element == null)
                return;

            if (element is T)
            {
                element.SetValue(property, value);
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                InternalApplyToVisualChildren<T>(VisualTreeHelper.GetChild(element, i), property, value);
            }
        }

        public static void ApplyToVisualChildren<T>(DependencyObject element, DependencyProperty property, object value, Func<DependencyObject, bool> condition) where T : FrameworkElement
        {
            if (element == null)
                return;


            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                InternalApplyToVisualChildren<T>(VisualTreeHelper.GetChild(element, i), property, value, condition);
            }
        }

        private static void InternalApplyToVisualChildren<T>(DependencyObject element, DependencyProperty property, object value, Func<DependencyObject, bool> condition) where T : FrameworkElement
        {
            if (element == null)
                return;


            if (element is T && condition(element))
            {
                element.SetValue(property, value);
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                InternalApplyToVisualChildren<T>(VisualTreeHelper.GetChild(element, i), property, value, condition);
            }

        }



        private static readonly DispatcherOperationCallback exitFrameCallback = ExitFrame;

        /// <summary> 
        /// Processes all UI messages currently in the message queue. 
        /// </summary> 
        public static void WaitForPriority()
        {
            // Create new nested message pump. 
            DispatcherFrame nestedFrame = new DispatcherFrame();

            // Dispatch a callback to the current message queue, when getting called, 
            // this callback will end the nested message loop. 
            // The priority of this callback should be lower than that of event message you want to process. 
            DispatcherOperation exitOperation = Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle, exitFrameCallback, nestedFrame);

            // pump the nested message loop, the nested message loop will immediately 
            // process the messages left inside the message queue. 
            try
            {
                Dispatcher.PushFrame(nestedFrame);
            }
            catch
            {
                return;
            }

            // If the "exitFrame" callback is not finished, abort it. 
            if (exitOperation.Status != DispatcherOperationStatus.Completed)
            {
                exitOperation.Abort();
            }
        }

        private static Object ExitFrame(Object state)
        {
            DispatcherFrame frame = state as DispatcherFrame;

            // Exit the nested message loop. 
            frame.Continue = false;
            return null;
        }

        public static Window GetTopParentWindow(FrameworkElement fe)
        {
            DependencyObject dpParent = fe.Parent;
            do
            {
                dpParent = LogicalTreeHelper.GetParent(dpParent);
            } while (dpParent.GetType().BaseType != typeof(Window));
            return dpParent as Window;
        }

        public static bool SetFocusOnFirstTabStop(object panel)
        {
            if (panel is ScrollViewer)
            {
                return SetFocusOnFirstTabStop(((ScrollViewer)panel).Content);
            }
            if (panel is Panel)
            {
                foreach (UIElement uie in ((Panel)panel).Children)
                {
                    if (uie is Control && ((Control)uie).IsTabStop)
                    {

                        uie.Focus();
                        TextBox tb = uie as TextBox;
                        if (tb!=null)
                        {
                            tb.SelectAll();
                        }
                        return true;
                    }
                    else
                    {
                        if (uie is UserControl)
                        {
                            FrameworkElement ctrl = Helpers.FindChild<Control>((FrameworkElement)uie, (fe) => (fe is Control && ((Control)fe).IsTabStop));
                            if (ctrl != null)
                            {
                                ctrl.Focus();
                                TextBox tb = ctrl as TextBox;
                                if (tb!=null)
                                {
                                    tb.SelectAll();
                                }
                                return true;
                            }
                        }
                        else if (uie is ScrollViewer || uie is Panel)
                        {
                            if (SetFocusOnFirstTabStop(uie))
                                return true;
                        }
                    }
                }
                return false;
            }
            return false;
        }

        public static FrameworkElement FindVisualChild<T>(FrameworkElement element, Func<FrameworkElement, bool> condition) where T : FrameworkElement
        {

            if (element == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++ )
            {
                object fe = VisualTreeHelper.GetChild(element, i);
                if (fe is FrameworkElement)
                {
                    FrameworkElement temp = InternalFindVisualChild<T>((FrameworkElement)fe, condition);
                    if (temp != null)
                    {
                        return temp;
                    }
                }

            }
            return null;
        }

        private static FrameworkElement InternalFindVisualChild<T>(FrameworkElement element, Func<FrameworkElement, bool> condition) where T : FrameworkElement
        {
            if (element == null)
                return null;


            if (element is T && element is DependencyObject && condition(element))
            {
                return element;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++ )
            {
                object fe = VisualTreeHelper.GetChild(element, i);
                if (fe is FrameworkElement)
                {
                    FrameworkElement temp = InternalFindVisualChild<T>((FrameworkElement)fe, condition);
                    if (temp != null)
                        return temp;
                }
            }

            return null;
        }

    }

    public class OpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return 0.0;

            if ((int)value < 0)
                return 0.0;

            return ((int)value) / 255.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (targetType == typeof(Visibility))
            {
                if (value == null)
                {
                    if (parameter == null)
                        return Visibility.Collapsed;
                    else
                        return Visibility.Collapsed;
                }
                if (value.Equals(parameter))
                {
                    return Visibility.Visible;
                }
                else
                {
                    if (value is int && parameter is string)
                        if (int.Parse((string)parameter).Equals(value))
                            return Visibility.Visible;

                    return Visibility.Collapsed;
                }
            }
            else
            {

                if (value == null)
                {
                    return parameter != null;
                }
                return (!value.Equals(parameter));
            }




            return value;
            if (value is bool)
                return !(bool)(value);
            else
                return !(bool)System.Convert.ChangeType(value, typeof(bool));


            if (parameter != null)
            {
                if (parameter.ToString() == "aaa")
                {
                    ContactRoute.DiagnosticHelpers.DebugIfPossible();

                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class ScrollBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility vis = (Visibility)value;
            switch (vis)
            {
                case Visibility.Collapsed:
                    return ScrollBarVisibility.Hidden;
                case Visibility.Hidden:
                    return ScrollBarVisibility.Hidden;
                case Visibility.Visible:
                    return ScrollBarVisibility.Visible;
            }
            return ScrollBarVisibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TooltipConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value is string && value!=null)
                return (value as string).Replace(". ", "\n").TrimEnd('\n');
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BiggerThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int prm = 0;

            if (parameter is int)
                prm = (int)parameter;
            else if (parameter is string)
                int.TryParse(parameter as string, out prm);
            else
                return null;
            if (value != null && ((int)value) > 0 && ((int)value) > prm)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SmallerThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int prm = 0;

            if (parameter is int)
                prm = (int)parameter;
            else if (parameter is string)
                int.TryParse(parameter as string, out prm);
            else
                return null;
            if (value !=null && ((int)value)>0 && ((int)value) <= prm)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                bool prm = true;

                try
                {
                    if (parameter != null)
                        prm = Boolean.Parse(parameter as string);
                }
                catch
                {
                }

                if (prm)
                {
                    if ((bool)value)
                        return Visibility.Visible;
                    else
                        return Visibility.Hidden;
                }
                else
                {
                    if ((bool)value)
                        return Visibility.Hidden;
                    else
                        return Visibility.Visible;
                }
            }
            else if (value is Visibility)
            {
                bool prm = true;

                try
                {
                    if (parameter != null)
                        prm = Boolean.Parse(parameter as string);
                }
                catch
                {
                }

                if (prm)
                {
                    return ((Visibility)value == Visibility.Visible);
                }
                else
                {
                    return ((Visibility)value != Visibility.Visible);
                }
            }

            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }

    public class TextMappingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return TranslationContext.Default.Translate(parameter as string);
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConcatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<string> lst = new List<string>();
            foreach (object obj in values)
            {
                if (obj != null)
                {
                    lst.Add((string)obj);
                }
            }
            return string.Join(parameter == null ? " " : (string)parameter, lst);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibilityWithCollapseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool prm = true;

            try
            {
                if (parameter != null)
                    prm = Boolean.Parse(parameter as string);
            }
            catch
            {
            }

            if (prm)
            {
                if ((bool)value)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            else
            {
                if ((bool)value)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = true;
            bool[] bParams = null;

            if (parameter != null)
            {
                string[] prms = parameter.ToString().Split(',');
                bParams = new bool[prms.Length];
                for (int i = 0; i < bParams.Length; i++)
                {
                    bParams[i] = bool.Parse(prms[i]);
                }
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (bParams != null && i < bParams.Length)
                    if (bParams[i])
                        result = result && !((bool)values[i]);
                    else
                        result = result && ((bool)values[i]);
                else
                    result = result && ((bool)values[i]);
            }

            if (result)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibilityWithCollapseMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = true;
            bool[] bParams = null;

            if (parameter != null && "debug".Equals(parameter as string))
            {
                parameter = null;
                ContactRoute.DiagnosticHelpers.DebugIfPossible();

            }

            if (parameter != null)
            {
                string[] prms = parameter.ToString().Split(',');
                bParams = new bool[prms.Length];
                for (int i = 0; i < bParams.Length; i++)
                {
                    bParams[i] = bool.Parse(prms[i]);
                }
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (DependencyProperty.UnsetValue.Equals(values[i]) || values[i] == null)
                    return Visibility.Collapsed;


                if (bParams != null && i < bParams.Length)
                    if (bParams[i])
                        result = result && !((bool)values[i]);
                    else
                        result = result && ((bool)values[i]);
                else
                    result = result && ((bool)values[i]);
            }

            if (result)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanMultiAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter != null && "debug".Equals(parameter as string))
            {
                parameter = null;
                ContactRoute.DiagnosticHelpers.DebugIfPossible();

            }
            bool result = true;
            bool[] bParams = null;

            if (parameter != null)
            {
                string[] prms = parameter.ToString().Split(',');
                bParams = new bool[prms.Length];
                for (int i = 0; i < bParams.Length; i++)
                {
                    bParams[i] = bool.Parse(prms[i]);
                }
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (DependencyProperty.UnsetValue.Equals(values[i]))
                    return null;

                if (bParams != null && i < bParams.Length)
                    if (!bParams[i])
                        result = result && !((bool)values[i]);
                    else
                        result = result && ((bool)values[i]);
                else
                    result = result && ((bool)values[i]);
            }

            if (targetType == typeof(Visibility))
                return result ? Visibility.Visible : Visibility.Collapsed;

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanMultiOrConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;
            bool[] bParams = null;

            if (parameter != null)
            {
                string[] prms = parameter.ToString().Split(',');
                bParams = new bool[prms.Length];
                for (int i = 0; i < bParams.Length; i++)
                {
                    bParams[i] = bool.Parse(prms[i]);
                }
            }

            for (int i = 0; i < values.Length && !result; i++)
            {
                if (DependencyProperty.UnsetValue.Equals(values[i]))
                    return null;

                if (bParams != null && i < bParams.Length)
                    if (!bParams[i])
                        result = result || !((bool)values[i]);
                    else
                        result = result || ((bool)values[i]);
                else
                    result = result || ((bool)values[i]);
            }

            if (targetType == typeof(Visibility))
                return result ? Visibility.Visible : Visibility.Collapsed;

            return result;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GridLengthAnimation : AnimationTimeline
    {
        private bool isCompleted;


        public static readonly DependencyProperty ReverseValueProperty = DependencyProperty.Register("ReverseValue", typeof(double), typeof(GridLengthAnimation), new UIPropertyMetadata(0.0));
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));
        public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(EasingFunctionBase), typeof(GridLengthAnimation));

        public EasingFunctionBase EasingFunction
        {
            get
            {
                return (EasingFunctionBase)(GetValue(EasingFunctionProperty));
            }
            set
            {
                SetValue(EasingFunctionProperty, value);
            }
        }

        public bool IsCompleted
        {
            get { return isCompleted; }
            set { isCompleted = value; }
        }

        public double ReverseValue
        {
            get { return (double)GetValue(ReverseValueProperty); }
            set { SetValue(ReverseValueProperty, value); }
        }

        public override Type TargetPropertyType
        {
            get
            {
                return typeof(GridLength);
            }
        }

        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public GridLength From
        {
            get
            {
                return (GridLength)GetValue(GridLengthAnimation.FromProperty);
            }
            set
            {
                SetValue(GridLengthAnimation.FromProperty, value);
            }
        }

        public GridLength To
        {
            get
            {
                return (GridLength)GetValue(GridLengthAnimation.ToProperty);
            }
            set
            {
                SetValue(GridLengthAnimation.ToProperty, value);
            }
        }

        AnimationClock clock;

        void VerifyAnimationCompletedStatus(AnimationClock clock)
        {
            if (this.clock == null)
            {
                this.clock = clock;
                this.clock.Completed += new EventHandler(delegate(object sender, EventArgs e) { isCompleted = true; });
            }
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {

            VerifyAnimationCompletedStatus(animationClock);


            if (isCompleted)
                return (GridLength)defaultDestinationValue;

            double fromVal = this.From.Value;
            if (this.From.IsAuto)
                fromVal = ((GridLength)defaultOriginValue).Value;

            double toVal = this.To.Value;


            if (((GridLength)defaultOriginValue).Value == toVal)
            {
                fromVal = toVal;
                toVal = this.ReverseValue;
            }
            else
                if (animationClock.CurrentProgress.Value == 1.0)
                    return To;


            if (fromVal > toVal)
                return new GridLength((1 - animationClock.CurrentProgress.Value) * (fromVal - toVal) + toVal, this.From.IsStar ? GridUnitType.Star : GridUnitType.Pixel);
            else
                return new GridLength(animationClock.CurrentProgress.Value * (toVal - fromVal) + fromVal, this.From.IsStar ? GridUnitType.Star : GridUnitType.Pixel);
        }
    }




    public class EqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                if ((value == null && parameter == null) || (value!=null && value.Equals(parameter) ))
                {
                    return Visibility.Hidden;
                }
                else
                {
                    if (value is int && parameter is string)
                        if (int.Parse((string)parameter).Equals(value))
                            return Visibility.Hidden;

                    return Visibility.Visible;
                }
            }
            else
            {
                if (value == null)
                    return (parameter == null);
                else
                    if (value.Equals(parameter))
                        return true;
                    else if (value is int && parameter is string)
                    {
                        return int.Parse((string)parameter).Equals(value);
                    }
                    else
                        return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                return parameter;
            }
            else
            {
                if (targetType.IsValueType)
                {
                    return Convert(0, targetType, null, null);
                }
                else
                {
                    return null;
                }
            }
        }
    }
   
    
    public class InequalityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                if (value == null)
                {
                    if (parameter == null)
                        return Visibility.Visible;
                    else
                        return Visibility.Hidden;
                }
                else if (value.Equals(parameter))
                {
                    return Visibility.Visible;
                }
                else
                {
                    if (value is int && parameter is string)
                        if (int.Parse((string)parameter).Equals(value))
                            return Visibility.Visible;

                    return Visibility.Hidden;
                }
            }
            else
            {
                if (value == null)
                    return (parameter != null);
                else
                    return !value.Equals(parameter);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(bool)value)
            {
                return parameter;
            }
            else
            {
                object[] Attrs = targetType.GetCustomAttributes(typeof(ContactRoute.EnumDefaultValueAttribute), true);

                if (Attrs.Length > 0)
                {
                    return ((ContactRoute.EnumDefaultValueAttribute)Attrs[0]).Value;
                }
                if (targetType == typeof(Single))
                    return 0;
                //return null;
                throw new NotSupportedException();
            }
        }
    }

    public class EqualityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                if (value == null)
                {
                    if (parameter == null)
                    {
                        return Visibility.Collapsed;
                    }
                    else
                        return Visibility.Visible;
                }
                else
                {
                    if (value.Equals(parameter))
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        if (value is int && parameter is string)
                            if (int.Parse((string)parameter).Equals(value))
                                return Visibility.Collapsed;

                        return Visibility.Visible;
                    }
                }
            }
            else
            {
                if (value == null)
                {
                    return parameter == null;
                }
                return (value.Equals(parameter));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                return parameter;
            }
            else
            {
                if (targetType.IsValueType)
                {
                    return Convert(0, targetType, null, null);
                }
                else
                {
                    return null;
                }
            }
        }
    }

    public class InequalityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                if (value == null)
                {
                    if (parameter == null)
                        return Visibility.Collapsed;
                    else
                        return Visibility.Collapsed;
                }
                if (value.Equals(parameter))
                {
                    return Visibility.Visible;
                }
                else
                {
                    if (value is int && parameter is string)
                        if (int.Parse((string)parameter).Equals(value))
                            return Visibility.Visible;

                    return Visibility.Collapsed;
                }
            }
            else
            {

                if (value == null)
                {
                    return parameter != null;
                }
                return (!value.Equals(parameter));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(bool)value)
            {
                return parameter;
            }
            else
            {
                object[] Attrs = targetType.GetCustomAttributes(typeof(ContactRoute.EnumDefaultValueAttribute), true);

                if (Attrs.Length > 0)
                {
                    return ((ContactRoute.EnumDefaultValueAttribute)Attrs[0]).Value;
                }

                throw new NotSupportedException();
            }
        }
    }



    public class BoolInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
                return !(bool)(value);
            else
                return !(bool)System.Convert.ChangeType(value, typeof(bool));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
                return !(bool)(value);
            else
                return !(bool)System.Convert.ChangeType(value, typeof(bool));
        }
    }

    public class FirstNonNullConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            for (int i = 0; i < values.Length; i++)
                if (values[i] != null && !DependencyProperty.UnsetValue.Equals(values[i]))
                    return values[i];
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NixxisCommand : DependencyObject, System.Windows.Input.ICommand, INotifyPropertyChanged
    {
        Action<NixxisCommand, object> m_Action;
        bool m_IsEnabled;
        object m_State;
        bool m_IsAsync;

        public NixxisCommand(Action<NixxisCommand, object> action)
            : this(action, null, true)
        {
        }

        public NixxisCommand(Action<NixxisCommand, object> action, object state)
            : this(action, state, true)
        {
        }

        public NixxisCommand(Action<NixxisCommand, object> action, object state, bool isEnabled)
        {
            m_Action = action;
            m_State = state;
            m_IsEnabled = isEnabled;
        }

        public bool IsEnabled
        {
            get
            {
                return m_IsEnabled;
            }
            set
            {
                if (m_IsEnabled != value)
                {
                    m_IsEnabled = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));

                    if (CanExecuteChanged != null)
                        CanExecuteChanged(this, EventArgs.Empty);
                }
            }
        }

        public object State
        {
            get
            {
                return m_State;
            }
            set
            {
                if (!object.Equals(m_State, value))
                {
                    m_State = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("State"));
                }
            }
        }

        public bool IsAsync
        {
            get
            {
                return m_IsAsync;
            }
            set
            {
                if (m_IsAsync != value)
                {
                    m_IsAsync = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsAsync"));
                }
            }
        }

        public Action<NixxisCommand, object> Action
        {
            get
            {
                return m_Action;
            }
            set
            {
                if (!Action<NixxisCommand, object>.Equals(m_Action, value))
                {
                    bool WasNull = (m_Action == null);

                    m_Action = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Action"));

                    if (WasNull && m_Action != null && m_IsEnabled)
                    {
                        if (PropertyChanged != null)
                            PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));

                        if (CanExecuteChanged != null)
                            CanExecuteChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return (m_IsEnabled && m_Action != null);
        }

        public event EventHandler CanExecuteChanged;

        void OnEndInvoke(IAsyncResult asyncResult)
        {
            m_Action.EndInvoke(asyncResult);
        }

        public void Execute(object parameter)
        {
            if (m_IsEnabled && m_Action != null)
            {
                if (m_IsAsync)
                {
                    m_Action.BeginInvoke(this, parameter, OnEndInvoke, null);
                }
                else
                {
                    m_Action.Invoke(this, parameter);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class MultiEqConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values[0] == values[1];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntegerInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
                return -(double)(value);
            return -(int)(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class RequiredConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            for (int i = 0; i < values.Length; i += 2)
            {
                if ((bool)values[i])
                {
                    object obj = values[i + 1];
                    if (obj is bool)
                    {
                        if (!(bool)obj)
                            return false;
                    }
                    else
                    {
                        if (obj == null)
                            return false;
                        if (obj is DateTime)
                        {
                        }
                        else if (string.IsNullOrEmpty((string)obj))
                            return false;
                    }
                }
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultiplicatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value == null)
                return System.Convert.ChangeType((double)System.Convert.ChangeType(0, typeof(double)) / double.Parse(parameter.ToString()), targetType);

            return System.Convert.ChangeType( (double)System.Convert.ChangeType(value, typeof(double)) / double.Parse(parameter.ToString()) , targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ChangeType((double)System.Convert.ChangeType(value, typeof(double)) * double.Parse(parameter.ToString()), targetType);
        }
    }

    public class MenuItemStyleSelector : StyleSelector
    {
        public Style MenuItemStyle
        {
            get;
            set;
        }

        public Style SeparatorStyle
        {
            get;
            set;
        }

        public override Style SelectStyle(object item, DependencyObject container)
        {

            if (item is MenuItem)
            {
                return MenuItemStyle;
            }
            else if (item is Separator)
            {
                return SeparatorStyle;
            }
            return base.SelectStyle(item, container);
        }
    }

    public class CoefConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ChangeType((double)System.Convert.ChangeType(value, typeof(double)) * double.Parse(parameter.ToString()), targetType);

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ChangeType((double)System.Convert.ChangeType(value, typeof(double)) / double.Parse(parameter.ToString()), targetType);
        }
    }


    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return TimeSpan.FromMinutes((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((TimeSpan)value).TotalMinutes;
        }
    }


    public class SourceChooserConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (DependencyProperty.UnsetValue != values[0] && (bool)values[0])
            {
                return values[1];
            }
            else
            {
                return values[2];
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VSPHelper : INotifyPropertyChanged
    {

        static private bool m_IsVirtualizing = false;

        public bool IsVirtualizing
        {
            get
            {
                return m_IsVirtualizing;
            }
            set
            {
                m_IsVirtualizing = value;
                FirePropertyChanged("IsVirtualizing");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
