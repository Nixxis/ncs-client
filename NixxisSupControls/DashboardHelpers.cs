using Nixxis.ClientV2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

namespace Nixxis.Client.Supervisor
{
    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(double) || targetType==typeof(object))
            {
                if (value is double)
                    return value;
                if (value is float)
                    return (double)(float)value;
                if (value is TimeSpan)
                    return ((TimeSpan)value).TotalSeconds;
                if (value is int)
                    return (double)(int)value;
                if (value is string)
                    return ((string)value).Length;
                return 0;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VerticalAlignmentConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return VerticalAlignment.Stretch;
            else
                return VerticalAlignment.Bottom;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultiIndexerConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] == null)
                return null;
            string strParam = parameter as string;

            if (strParam!=null)
            {

                int count = (int)(values[0].GetType().GetProperty("Count").GetGetMethod().Invoke(values[0], new object[] { }));
                if (count == 0)
                    return null;

                object obj = values[0].GetType().GetProperty("Item").GetGetMethod().Invoke(values[0], new object[] { (int)(values[1]) % count });

                if (string.IsNullOrEmpty(strParam) || strParam == ".")
                    return obj;
                else
                    return obj.GetType().GetProperty(strParam).GetGetMethod().Invoke(obj, null);
            }
            else
            {
                return values[System.Convert.ToInt32(values[values.Length - 1])];
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ExtendedPanel : StackPanel
    {
        public static readonly DependencyProperty DesiredHeightProperty = DependencyProperty.RegisterAttached("DesiredHeight", typeof(double), typeof(ExtendedPanel), new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(DesiredHeightChanged)));
        public static readonly DependencyProperty VerticalZoomProperty = DependencyProperty.Register("VerticalZoom", typeof(double), typeof(ExtendedPanel), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty SharedVerticalZoomProperty = DependencyProperty.RegisterAttached("SharedVerticalZoom", typeof(double), typeof(ExtendedPanel), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty IsSharedVerticalZoomProperty = DependencyProperty.RegisterAttached("IsSharedVerticalZoom", typeof(bool), typeof(ExtendedPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty ZoomNeverAppliedProperty = DependencyProperty.RegisterAttached("ZoomNeverApplied", typeof(bool), typeof(ExtendedPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static void DesiredHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            while (obj != null && !(obj is ExtendedPanel))
                obj = VisualTreeHelper.GetParent(obj);

            if (obj != null)
                ((UIElement)obj).InvalidateMeasure();
        }

        public static bool GetIsSharedVerticalZoom(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSharedVerticalZoomProperty);
        }
        public static void SetIsSharedVerticalZoom(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSharedVerticalZoomProperty, value);
        }

        public static bool GetZoomNeverApplied(DependencyObject obj)
        {
            return (bool)obj.GetValue(ZoomNeverAppliedProperty);
        }
        public static void SetZoomNeverApplied(DependencyObject obj, bool value)
        {
            obj.SetValue(ZoomNeverAppliedProperty, value);
        }


        public static double GetDesiredHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(DesiredHeightProperty);
        }
        public static void SetDesiredHeight(DependencyObject obj, double value)
        {
            obj.SetValue(DesiredHeightProperty, value);
        }

        public static double GetSharedVerticalZoom(DependencyObject obj)
        {
            return (double)obj.GetValue(SharedVerticalZoomProperty);
        }
        public static void SetSharedVerticalZoom(DependencyObject obj, double value)
        {
            obj.SetValue(SharedVerticalZoomProperty, value);
        }

        public double VerticalZoom
        {
            get
            {
                return (double)GetValue(VerticalZoomProperty);
            }
            set
            {
                SetValue(VerticalZoomProperty, value);
            }
        }

        private double GetUIElementDesiredHeight(UIElement uie)
        {
            double desiredHeight = 0;
            if (uie is ContentPresenter)
            {
                if (VisualTreeHelper.GetChildrenCount(uie) != 0)
                {
                    desiredHeight = GetDesiredHeight(VisualTreeHelper.GetChild(uie, 0));                    
                }
                else
                {
                    // not yet rendered...
                    (uie as ContentPresenter).Loaded += ExtendedPanel_Loaded;
                }

            }
            else
            {
                desiredHeight = GetDesiredHeight(uie);                
            }
            return desiredHeight;
        }

        private bool GetUIElementZoomNeverApplied(UIElement uie)
        {
            bool zoomNeverApplied = false; ;
            if (uie is ContentPresenter)
            {
                if (VisualTreeHelper.GetChildrenCount(uie) != 0)
                {
                    zoomNeverApplied = GetZoomNeverApplied(VisualTreeHelper.GetChild(uie, 0));
                }
                else
                {
                    // not yet rendered...
                    (uie as ContentPresenter).Loaded += ExtendedPanel_Loaded;
                }
            }
            else
            {
                zoomNeverApplied = GetZoomNeverApplied(uie);
            }
            return zoomNeverApplied;
        }

        void ExtendedPanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.InvalidateMeasure();
            (sender as ContentPresenter).Loaded -= ExtendedPanel_Loaded;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            double totalHeight = 0;
            double totalUnscalable = 0;

            foreach (UIElement uie in this.InternalChildren)
            {
                if(GetUIElementZoomNeverApplied(uie))
                    totalUnscalable += GetUIElementDesiredHeight(uie);
                else
                    totalHeight += GetUIElementDesiredHeight(uie);
            }


            if (VerticalAlignment == System.Windows.VerticalAlignment.Stretch)
            {
                double scale = (constraint.Height - totalUnscalable) / totalHeight;
                foreach (UIElement uie in this.InternalChildren)
                {
                    double desiredHeight = GetUIElementDesiredHeight(uie);
                    Size sz;
                    if (scale == 0 || double.IsInfinity(scale))
                        sz = new Size(constraint.Width, desiredHeight);
                    else
                    {
                        if(GetUIElementZoomNeverApplied(uie))
                            sz = new Size(constraint.Width, desiredHeight);
                        else
                            sz = new Size(constraint.Width, desiredHeight * scale);
                    }
                    uie.Measure(sz);
                }

                return constraint;
            }
            else
            {

                if (autoZoom)
                {
                    double computedZoom = (constraint.Height - totalUnscalable) / totalHeight;
                    if (computedZoom < VerticalZoom)
                        VerticalZoom = computedZoom;
                }


                foreach (UIElement uie in this.InternalChildren)
                {
                    double desiredHeight = GetUIElementDesiredHeight(uie);
                    Size sz;
                    if (VerticalZoom != 0)
                    {
                        if(GetUIElementZoomNeverApplied(uie))
                            sz = new Size(constraint.Width, desiredHeight);
                        else
                            sz = new Size(constraint.Width, desiredHeight * VerticalZoom);
                    }
                    else
                        sz = new Size(constraint.Width, desiredHeight);
                    uie.Measure(sz);
                }
                return constraint;
            }

        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double totalHeight = 0;
            double totalUnscalable = 0;

            double y = 0;

            foreach (UIElement uie in this.InternalChildren)
            {
                if (GetUIElementZoomNeverApplied(uie))
                    totalUnscalable += GetUIElementDesiredHeight(uie);
                else
                    totalHeight += GetUIElementDesiredHeight(uie);
            }

            if (VerticalAlignment == System.Windows.VerticalAlignment.Stretch)
            {
                double scale = arrangeSize.Height / totalHeight;

                foreach (UIElement uie in this.InternalChildren)
                {
                    if (scale == 0 || double.IsInfinity(scale))
                    {
                        uie.Arrange(new Rect(0, y, arrangeSize.Width, uie.DesiredSize.Height));
                        y += uie.DesiredSize.Height;
                    }
                    else
                    {
                        if (GetUIElementZoomNeverApplied(uie))
                        {
                            uie.Arrange(new Rect(0, y, arrangeSize.Width, GetUIElementDesiredHeight(uie) ));
                            y += GetUIElementDesiredHeight(uie) ;
                        }
                        else
                        {
                            uie.Arrange(new Rect(0, y, arrangeSize.Width, GetUIElementDesiredHeight(uie) * scale));
                            y += GetUIElementDesiredHeight(uie) * scale;
                        }
                    }
                    
                }
                return arrangeSize;
            }
            else if (VerticalAlignment == System.Windows.VerticalAlignment.Top)
            {

            }
            else if (VerticalAlignment == System.Windows.VerticalAlignment.Center)
            {
                y = (arrangeSize.Height - totalHeight * VerticalZoom - totalUnscalable) / 2;
            }
            else if (VerticalAlignment == System.Windows.VerticalAlignment.Bottom)
            {
                y = arrangeSize.Height - totalHeight * VerticalZoom - totalUnscalable;
            }

            foreach (UIElement uie in this.InternalChildren)
            {
                if (VerticalZoom == 0)
                {
                    uie.Arrange(new Rect(0, y, arrangeSize.Width, GetUIElementDesiredHeight(uie)));
                    y += GetUIElementDesiredHeight(uie);

                }
                else
                {
                    if (GetUIElementZoomNeverApplied(uie))
                    {
                        uie.Arrange(new Rect(0, y, arrangeSize.Width, GetUIElementDesiredHeight(uie) ));
                        y += GetUIElementDesiredHeight(uie) ;
                    }
                    else
                    {
                        uie.Arrange(new Rect(0, y, arrangeSize.Width, GetUIElementDesiredHeight(uie) * VerticalZoom));
                        y += GetUIElementDesiredHeight(uie) * VerticalZoom;
                    }
                }
            }

            return arrangeSize;
        }

        bool autoZoom = false;
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            FrameworkElement dobj = VisualTreeHelper.GetParent(this) as FrameworkElement;

            while (dobj != null && !GetIsSharedVerticalZoom(dobj))
            {
                dobj = VisualTreeHelper.GetParent(dobj) as FrameworkElement;
            }

            if (dobj != null)
            {
                BindingOperations.SetBinding(this, VerticalZoomProperty, new Binding() { Source = dobj, Path = new PropertyPath(SharedVerticalZoomProperty) });

                autoZoom = true;
            }
        }

    }



    public class DashboardPropertyConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IDashboardWidget idc = value as IDashboardWidget;
            string strParam = parameter as string;
            if (idc != null && !string.IsNullOrEmpty(strParam))
            {
                return idc.GetType().InvokeMember(strParam, System.Reflection.BindingFlags.GetProperty, null, idc, new object[] { });
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object valToCompare = values[0];

            object[] refs = values.Skip(1).ToArray<object>();



            // This is problematic...
            // object[] parms = (object[])parameter;
            // Invalid cast exception...

            // It is why I had to use reflection

            Type tpe = parameter.GetType();
            if (valToCompare != null)
            {

                for (int i = 0; i < refs.Length; i++)
                {
                    if (valToCompare.Equals(refs[i]))
                        return tpe.InvokeMember("Get", System.Reflection.BindingFlags.InvokeMethod, null, parameter, new object[] { i });
                }
                try
                {
                    return tpe.InvokeMember("Get", System.Reflection.BindingFlags.InvokeMethod, null, parameter, new object[] { refs.Length });
                }
                catch
                {
                    return null;
                }

            }
            else
            {
                for (int i = 0; i < refs.Length; i++)
                {
                    if (valToCompare == null && refs[i] == null)
                        return tpe.InvokeMember("Get", System.Reflection.BindingFlags.InvokeMethod, null, parameter, new object[] { i });
                }
                try
                {
                    return tpe.InvokeMember("Get", System.Reflection.BindingFlags.InvokeMethod, null, parameter, new object[] { refs.Length });
                }
                catch
                {
                    return null;
                }

            }

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UniformStackPanel : Panel
    {
        protected override Size ArrangeOverride(Size finalSize)
        {
            Size childrenSize = new Size(finalSize.Width / base.InternalChildren.Count, finalSize.Height);
            double dbl = 0;
            foreach (UIElement uie in base.InternalChildren)
            {
                uie.Arrange(new Rect(new Point(dbl, 0), childrenSize));
                dbl += childrenSize.Width;
            }
            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size childrenSize = new Size(availableSize.Width / base.InternalChildren.Count, availableSize.Height);

            Size computedSize = new Size(0, 0);

            foreach (UIElement uie in base.InternalChildren)
            {
                uie.Measure(childrenSize);

                if (uie.DesiredSize.Height > computedSize.Height)
                    computedSize.Height = uie.DesiredSize.Height;
                computedSize.Width += uie.DesiredSize.Width;

            }

            if (!double.IsInfinity(availableSize.Height))
                computedSize.Height = availableSize.Height;
            if (!double.IsInfinity(availableSize.Width))
                computedSize.Width = availableSize.Width;

            return computedSize;
        }
    }

    public class SupervisionTypeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<Tuple<string, string>> temp = new List<Tuple<string, string>>();
            try
            {
                SupervisionSource ss = value as SupervisionSource;

                foreach (PropertyInfo pi in ss.GetType().GetProperties())
                {
                    foreach (Attribute att in pi.GetCustomAttributes(typeof(SupervisionSourceAttribute), true))
                    {
                        SupervisionSourceAttribute ssatt = (SupervisionSourceAttribute)att;
                        if(ssatt.Visible)
                            temp.Add(new Tuple<string, string>(pi.Name, ssatt.Name));
                    }
                }

            }
            catch
            {
            }
            return temp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectSelector : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                SupervisionSource ss = values[0] as SupervisionSource;
                string supType = values[1] as string;

                string description = null;
                try
                {
                    description = ((SupervisionDescriptionPropertyAttribute)(ss.GetType().GetProperty(supType).PropertyType.BaseType.GetGenericArguments()[0].GetCustomAttributes(typeof(SupervisionDescriptionPropertyAttribute), true).FirstOrDefault())).Name;
                }
                catch
                {

                }

                foreach (PropertyInfo pi in ss.GetType().GetProperties())
                {
                    if (pi.Name == supType)
                    {
                        if(description!=null)
                            return OrderBy(pi.GetValue(ss, null) as IEnumerable, description);
                        else
                            return pi.GetValue(ss, null);
                    }
                }

            }
            catch
            {

            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<object> OrderBy(IEnumerable collection, string sort)
        {
            List<object> returnValue = new List<object>();

            foreach(object obj in collection)
            {
                returnValue.Add(obj);
            }

            return returnValue.OrderBy((obj) => (obj.GetType().GetProperty(sort).GetValue(obj, null)));

        }
    }


    public class DescriptionSelector : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                SupervisionSource ss = values[0] as SupervisionSource;
                string supType = values[1] as string;

                return ((SupervisionDescriptionPropertyAttribute)(ss.GetType().GetProperty(supType).PropertyType.BaseType.GetGenericArguments()[0].GetCustomAttributes(typeof(SupervisionDescriptionPropertyAttribute), true).FirstOrDefault())).Name;

            }
            catch
            {

            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class PropertySelector : IMultiValueConverter
    {
        private static TranslationContext InboundsContext = new TranslationContext("NixxisSupervisionInbound");
        private static TranslationContext OutboundsContext = new TranslationContext("NixxisSupervisionOutbound");
        private static TranslationContext AgentsContext = new TranslationContext("NixxisSupervisionAgents");
        private static TranslationContext CampaignsContext = new TranslationContext("NixxisSupervisionCampaigns");
        private static TranslationContext QueuesContext = new TranslationContext("NixxisSupervisionQueues");

        private static string Translate(string strKey, string supType)
        {
            string strPrefix = null;
            string strSuffix = strKey;
            string strNewKey = null;
            if(strKey.Contains("."))
            {
                string[] strSplit = strKey.Split('.');
                strPrefix = TranslationContext.Default.Translate(strSplit[0]);
                if(strSplit[0].Equals("PeriodProduction"))
                {
                    strNewKey = string.Format("{0}_H", strSplit[1]);
                }
                else
                {
                    strNewKey = strSplit[1];
                }
            }
            else
            {
                strNewKey = strKey;
            }

            switch(supType)
            {
                case "Inbounds":
                    strSuffix = InboundsContext.Translate(strNewKey);
                    break;
                case "Outbounds":
                    strSuffix = OutboundsContext.Translate(strNewKey);
                    break;
                case "Agents":
                    strSuffix = AgentsContext.Translate(strNewKey);
                    break;
                case "Queues":
                    strSuffix = QueuesContext.Translate(strNewKey);
                    break;
                case "Campaigns":
                    strSuffix = CampaignsContext.Translate(strNewKey);
                    break;
                default:
                    break;
            }

            if(!string.IsNullOrEmpty(strPrefix))
            {
                return string.Format("{0} {1}", strPrefix, strSuffix);
            }
            else
            {
                return strSuffix;
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<PropertyHelper> temp = new List<PropertyHelper>();
            try
            {
                SupervisionSource ss = values[0] as SupervisionSource;
                string supType = values[1] as string;


                foreach (PropertyInfo pi in ss.GetType().GetProperties())
                {
                    if (pi.Name == supType)
                    {
                        foreach (PropertyInfo p in pi.PropertyType.BaseType.GetGenericArguments()[0].GetProperties())
                        {
                            foreach (Attribute at in p.GetCustomAttributes(true))
                            {
                                if (at is SupervisionDataAttribute)
                                {
                                    // at least one, it's ok

                                    string propertyDescription = Translate( p.Name, supType);

                                    temp.Add(new PropertyHelper() { Id = p.Name, Description = propertyDescription });
                                    break;
                                }
                                else if (at is SupervisionAutoInitPropertyAttribute)
                                {
                                    foreach (PropertyInfo subP in p.PropertyType.GetProperties())
                                    {
                                        foreach (Attribute subat in subP.GetCustomAttributes(typeof(SupervisionDataAttribute), true))
                                        {
                                            string propertyDescription = Translate( p.Name + "." + subP.Name, supType);

                                            temp.Add(new PropertyHelper() { Id = p.Name + "." + subP.Name, Description = propertyDescription });
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            return temp;
        }

        public static string GetPropertyDescription(object ss, string supType, string propertyId)
        {
            foreach (PropertyInfo pi in ss.GetType().GetProperties())
            {
                if (pi.Name == supType)
                {
                    if (pi.PropertyType.BaseType.IsGenericType)
                    {
                        foreach (PropertyInfo p in pi.PropertyType.BaseType.GetGenericArguments()[0].GetProperties())
                        {
                            foreach (Attribute at in p.GetCustomAttributes(true))
                            {
                                if (at is SupervisionDataAttribute)
                                {
                                    // at least one, it's ok

                                    if (p.Name.Equals(propertyId))
                                    {
                                        string propertyDescription = Translate( p.Name, supType);

                                        return propertyDescription;
                                    }
                                }
                                else if (at is SupervisionAutoInitPropertyAttribute)
                                {
                                    foreach (PropertyInfo subP in p.PropertyType.GetProperties())
                                    {
                                        foreach (Attribute subat in subP.GetCustomAttributes(typeof(SupervisionDataAttribute), true))
                                        {

                                            string propertyDescription = p.Name + "." + subP.Name;
                                            if (propertyDescription.Equals(propertyId))
                                            {
                                                return Translate( propertyDescription, supType);
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    else
                        return propertyId;
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PropertyHelper
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }

    public static class ComboBoxItemsSourceDecorator
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached(
            "ItemsSource", typeof(IEnumerable), typeof(ComboBoxItemsSourceDecorator), new PropertyMetadata(null, ItemsSourcePropertyChanged)
        );

        public static void SetItemsSource(UIElement element, Boolean value)
        {
            element.SetValue(ItemsSourceProperty, value);
        }

        public static IEnumerable GetItemsSource(UIElement element)
        {
            return (IEnumerable)element.GetValue(ItemsSourceProperty);
        }

        static void ItemsSourcePropertyChanged(DependencyObject element,
                        DependencyPropertyChangedEventArgs e)
        {
            var target = element as Selector;
            if (element == null)
                return;

            // Save original binding 
            var originalBinding = BindingOperations.GetBinding(target, Selector.SelectedValueProperty);

            BindingOperations.ClearBinding(target, Selector.SelectedValueProperty);
            try
            {
                target.ItemsSource = e.NewValue as IEnumerable;
            }
            finally
            {
                if (originalBinding != null)
                    BindingOperations.SetBinding(target, Selector.SelectedValueProperty, originalBinding);
            }
        }
    }



    public class CoefConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture) * System.Convert.ToDouble(parameter, System.Globalization.CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SizeConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(Size))
                return new Size((double)values[0], (double)values[1]);
            else if (targetType == typeof(Point))
                return new Point((double)values[0], (double)values[1]);
            else
                return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConstrainedGrid : Grid
    {
        public static readonly DependencyProperty SquareProperty = DependencyProperty.Register("Square", typeof(bool), typeof(ConstrainedGrid), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        protected override Size MeasureOverride(Size constraint)
        {
            if (Square)
            {
                double min = Math.Min(constraint.Width, constraint.Height);
                return base.MeasureOverride(new Size(min, min));
            }
            else
                return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Square)
            {
                double min = Math.Min(arrangeSize.Width, arrangeSize.Height);
                return base.ArrangeOverride(new Size(min, min));
            }
            else
                return base.ArrangeOverride(arrangeSize);
        }

        public bool Square
        {
            get
            {
                return (bool)GetValue(SquareProperty);
            }
            set
            {
                SetValue(SquareProperty, value);
            }
        }

    }

    public class FormatConverter: IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string format = values[0] as string;
            object value = values[1];


            if (targetType == typeof(string))
            {
                if (value == DependencyProperty.UnsetValue || value == null)
                    return string.Empty;

                if (string.IsNullOrEmpty(format))
                {
                    if (value is double || value is float)
                        return string.Format("{0:0.#}", value);
                }
                else
                {
                    try
                    {
                        return string.Format(format, value);
                    }
                    catch
                    {

                    }
                }

                return value.ToString();
            }
            return value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
