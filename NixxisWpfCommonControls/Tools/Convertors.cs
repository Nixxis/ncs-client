using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;

namespace Nixxis.Client.Controls
{
    //
    //Common converters general
    //
    #region Common converters general
    /// <summary>
    /// Change the name of the image depending on the value. (The parameters takes the name of the image)
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool active = (bool)value;
                string image = parameter as string;

                if (active)
                    return image + ".png";
                else
                    return image + "_Dis.png";

            }
            catch { return value; }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    /// <summary>
    /// Reversing the value of a bool
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class ReverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return !(bool)value;
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convert a bool to the Visibility Collapse enum
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (parameter == null)
                {
                    if ((bool)value)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
                else
                {
                    if ((bool)value)
                        return bool.Parse(parameter.ToString()) ? Visibility.Visible : Visibility.Collapsed;
                    else
                        return bool.Parse(parameter.ToString()) ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Convert a bool to the Visibility Hidden enum
    /// </summary>
    public class BoolToVisibilityHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (parameter == null)
                {
                    if ((bool)value)
                        return Visibility.Visible;
                    else
                        return Visibility.Hidden;
                }
                else
                {
                    if ((bool)value)
                        return bool.Parse(parameter.ToString()) ? Visibility.Visible : Visibility.Hidden;
                    else
                        return bool.Parse(parameter.ToString()) ? Visibility.Hidden : Visibility.Visible;
                }
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// when object is null Visibility is set depending on the parameter
    /// </summary>
    public class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (parameter == null)
                {
                    if (value != null)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
                else
                {
                    if (value != null)
                        return (bool)parameter ? Visibility.Visible : Visibility.Collapsed;
                    else
                        return (bool)parameter ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Compair the 'value' with the 'parameter' when equal the Visibility is set the visible. Can be used to compair an enum.
    /// </summary>
    public class ObjectCompairToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if(value == null)
                    return Visibility.Visible;

                if (value.GetType() == typeof(int))
                {
                    if (value.Equals(int.Parse(parameter as string)))
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                } 
                else if (value.GetType() == typeof(bool))
                {
                    if (value.Equals(bool.Parse(parameter as string)))
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
                else
                {
                    if (value.Equals(parameter))
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
            }
            catch
            { return Visibility.Visible; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Compair the 'value' with the 'parameter' when equal the Visibility is set the visible. Can be used to compair an enum.
    /// </summary>
    public class ObjectNotCompairToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return Visibility.Collapsed;

                if (value.GetType() == typeof(int))
                {
                    if (value.Equals(int.Parse(parameter as string)))
                        return Visibility.Collapsed;
                    else
                        return Visibility.Visible;
                }
                else if (value.GetType() == typeof(bool))
                {
                    if (value.Equals(bool.Parse(parameter as string)))
                        return Visibility.Collapsed;
                    else
                        return Visibility.Visible;
                }
                else
                {
                    if (value.Equals(parameter))
                        return Visibility.Collapsed;
                    else
                        return Visibility.Visible;
                }
            }
            catch
            { return Visibility.Collapsed; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// When object is null it will return false else it will be true
    /// </summary>
    [ValueConversion(typeof(object), typeof(bool))]
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
                    return bool.Parse(parameter.ToString()) ? true : false;
                else
                    return bool.Parse(parameter.ToString()) ? false : true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }




    /// <summary>
    /// Compair the 'value' with the 'parameter' when equal it returns true else false
    /// </summary>
    [ValueConversion(typeof(object), typeof(bool))]
    public class ObjectCompairToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return false;

                if (value.GetType() == typeof(int))
                {
                    if (value.Equals(int.Parse(parameter as string)))
                        return true;
                    else
                        return false;
                }
                else if (value.GetType() == typeof(string))
                {
                    if (value.Equals(parameter as string))
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (value.Equals(parameter))
                        return true;
                    else
                        return false;
                }
            }
            catch { return false; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// When string is null or empty it will return false else it will be true
    /// </summary>
    [ValueConversion(typeof(String), typeof(bool))]
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
            {
                if (value != null)
                    return string.IsNullOrEmpty(value.ToString()) ? false : true;
                else
                    return false;
            }
            else
            {
                if (value != null)
                {
                    if(string.IsNullOrEmpty(value.ToString()))
                        return bool.Parse(parameter.ToString()) ? false : true;
                    else
                        return bool.Parse(parameter.ToString()) ? true : false;
                }
                else
                    return bool.Parse(parameter.ToString()) ? false : true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Format a data time value to display date and time
    /// </summary>
    public class FormatDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                DateTime dte = (DateTime)value;

                if (parameter != null)
                {
                    if (dte == DateTime.MinValue || dte == DateTime.MaxValue)
                        return "";
                }

                return dte.ToString("dd/MM/yyyy HH:mm:ss");
            }
            catch
            { return value; }


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// Adding padding depending on the level of the item in the tree
    /// </summary>
    public class NixxisComboboxItemPadding : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int pad = 30;
                if(parameter != null)
                    int.TryParse(parameter.ToString(), out pad);

                return new Thickness((int)value * pad, 0, 0, 0);
            }
            catch
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Can be used to test a binding
    /// </summary>
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    //
    //Style converters
    //
    #region Style converters
    [ValueConversion(typeof(int), typeof(Color))]
    public class InfoPanelPriorityBackground : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int waitingContacts = (int)values[0];
                int vip = (int)values[1];

                if (vip > 0)
                    return Colors.Red;
                else if (waitingContacts > 0)
                    return Colors.Orange;
                else
                    return (Color)ColorConverter.ConvertFromString("#aeaeae");

            }
            catch { return (Color)ColorConverter.ConvertFromString("#aeaeae"); }

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ContactPanelBackground : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                LinearGradientBrush notSelected = new LinearGradientBrush();
                notSelected.StartPoint = new System.Windows.Point(0.5, 0);
                notSelected.EndPoint = new System.Windows.Point(0.5, 2);
                notSelected.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#404040"), 0.05));
                notSelected.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3b3b3b"), 0.25));
                notSelected.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#000000"), 0.25));
                notSelected.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#303030"), 0.45));

                LinearGradientBrush selected = new LinearGradientBrush();
                selected.StartPoint = new System.Windows.Point(0.5, 0);
                selected.EndPoint = new System.Windows.Point(0.5, 2);
                selected.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#4a9808"), 0.05));
                selected.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#7fd73e"), 0.25));
                selected.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#468f08"), 0.25));
                selected.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#72c008"), 0.45));

                LinearGradientBrush requestAction = new LinearGradientBrush();
                requestAction.StartPoint = new System.Windows.Point(0.5, 0);
                requestAction.EndPoint = new System.Windows.Point(0.5, 2);
                requestAction.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#6a005a"), 0.05));
                requestAction.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#972c89"), 0.25));
                requestAction.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#430033"), 0.25));
                requestAction.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#770066"), 0.45));

                bool isChecked = (bool)values[1];

                if (isChecked)
                    return selected;
                else
                {
                    if (values[0] == null)
                        return notSelected;

                    bool requestAgentAction = (bool)values[0];

                    if (requestAgentAction)
                    {
                        return requestAction;
                    }
                    else
                        return notSelected;
                }
            }
            catch { return (Color)ColorConverter.ConvertFromString("#aeaeae"); }

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(int), typeof(Color))]
    public class InfoPanelSetColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            /*
             * Main Color Code in html:
             * 000000 --> Black
             * FF0000 --> red
             * 00FF00 --> Green
             * 0000FF --> Blue
             * FFFFFF --> White
             * 
             * FFFF00 --> Yellow
             * FF00FF --> Pink
             * 00FFFF --> Cyan
             * FFA500 --> Orange
             * 
            */
            try
            {
                int val = (int)value;
                string color = parameter as string;

                if (!color.StartsWith("#"))
                    color = "#" + color;

                if (val > 0)
                    return (Color)ColorConverter.ConvertFromString(color);
                else
                    return Colors.Green;

            }
            catch { return Colors.Green; }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    //
    //Style converters
    //
    #region Style converters
    /// <summary>
    /// Returns parameter when true else normal
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class StyleFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (parameter == null)
                    return "Normal";

                if(value == null)
                {
                    return "Normal";
                }
                else if (value.GetType() == typeof(bool))
                {
                    if ((bool)value)
                        return parameter.ToString();
                    else
                        return "Normal";
                }
                else
                {
                    if (string.IsNullOrEmpty(value.ToString()))
                        return "Normal";
                    else
                        return parameter.ToString();
                }
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion


    //
    //Other converters
    //
    #region Other
    [ValueConversion(typeof(Color), typeof(Brush))]
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HtmlColorCodeToSolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string htmlColor = value as string;

            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(htmlColor));
            }
            catch { return new SolidColorBrush(Colors.Snow); }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
    
}