using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Nixxis.ClientV2;
using System.Windows;
using System.Windows.Media;

namespace Nixxis.Client.Supervisor
{
    public class NixxisToggleImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int count = (int)value;

            if (count > 0)
                return @"Images\Other\BooleanOff.png";
            else
                return @"Images\Other\BooleanOn.png";
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
            return value;
            if (parameter as string == "RealTime.Waiting")
            {
                return ((InboundItem)value).RealTime.Waiting;
            }
            else if (parameter as string == "c1")
            {
                return value;
            }
            try
            {
                return ((Nixxis.ClientV2.AgentItem)((NixxisSupervisionTileAgent)value).LastSelectedItem).RealTime.Status;
            }
            catch { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TeamsConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                TeamList tl = (TeamList)values[0];
                string[] strIds = (string[])values[1];
                List<string> retVal = new List<string>();

                foreach (string strId in strIds)
                {
                    TeamItem ti = tl.FirstOrDefault((t) => (t.Id.Equals(strId)));
                    if (ti != null)
                        retVal.Add(ti.Description);
                }
                retVal.Sort();
                return string.Join(",", retVal);
            }
            catch
            {

            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DataGridFormatCommonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value.GetType() == typeof(TimeSpan))
                {
                    TimeSpan val = (TimeSpan)value;

                    if (val == TimeSpan.Zero)
                        return "00:00:00";

                    DateTime dt = DateTime.MinValue.Add(val);


                    return dt.ToString("HH:mm:ss");
                }
                else if (value.GetType() == typeof(DateTime))
                {
                    DateTime dt = (DateTime)value;

                    return dt.ToString("dd/MM/yyyy HH:mm:ss");
                }
                else
                {
                    return value;
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

    public class DataGridFormatPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                double val = 0;
                if (value.GetType() == typeof(Single))
                    val = System.Convert.ToDouble((Single)value);
                else
                    val = (double)value;


                return val.ToString("#0.00") + "%";
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (parameter != null)
                {
                    string[] prms = parameter as string[];

                    if ((bool)value)
                        return prms[0];
                    else
                        return prms[1];
                }
                else
                {
                    if ((bool)value)
                        return @"Images\Other\BooleanOn.png";
                    else
                        return @"Images\Other\BooleanOff.png";
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

}
