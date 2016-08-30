using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using Nixxis.Client.Admin;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Recording
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
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DataGridFormatTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                TimeSpan val = (TimeSpan)value;

                if (val == TimeSpan.Zero)
                    return "00:00:00";

                DateTime dt = DateTime.MinValue.Add(val);

                return dt.ToString("HH:mm:ss");
            }
            catch
            { return value; }


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DataGridFormatDataTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                DateTime dt = (DateTime)value;

                return dt.ToString();
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

    public class AllowSearchIsCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool defaultValue = false;
                if (parameter != null)
                {
                    if (parameter.GetType() == typeof(bool))
                        defaultValue = (bool)parameter;
                    else
                        bool.TryParse(parameter.ToString(), out defaultValue);
                }

                if ((bool)value)
                    return defaultValue;
                else
                    return false;
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PlayerEnableButtonConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 2)
                return false;

            if (values[0] == null)
                return false;

            if (parameter == null)
                return true;

            ContactData selectedItem = (ContactData)values[0];
            Nixxis.Client.Recording.CallCenterControl.PlayerStates state = (Nixxis.Client.Recording.CallCenterControl.PlayerStates)values[1];

            if (parameter as string == "play")
            {
                if (state == CallCenterControl.PlayerStates.Playing)
                    return false;
                else
                    return true;
            }
            else if (parameter as string == "pause")
            {
                if (state == CallCenterControl.PlayerStates.Playing)
                    return true;
                else
                    return false;
            }
            else if (parameter as string == "stop")
            {
                if (state == CallCenterControl.PlayerStates.Playing || state == CallCenterControl.PlayerStates.Pause)
                    return true;
                else
                    return false;
            }

            return true;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GetQualificationListConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                
                if (values.Length < 3)
                    return null;

                //Search qualification list when contactdata is displayed
                AdminLight adminLight = (AdminLight)values[0];

                string campaignId = values[1] as string;
                if (campaignId == null || campaignId.Equals(CreateDisplayLists.NoSelection))
                    return null;

                string activitId = values[2] as string;
                if (activitId == null || activitId.Equals(CreateDisplayLists.NoSelection))
                {
                    //All qualifications of this campaign
                    if (parameter as string == "combobox")
                    {

                        int levelCount = 0;
                        NixxisComboBoxList list = new NixxisComboBoxList();

                        foreach (AdminObjectLight item in adminLight.Qualifications.Where((q) => (q.Related.Contains(campaignId))))
                        {
                            list.Add(new NixxisComboBoxItem(item.Id, item.Description, 0));
                        }

                        return list;
                    }
                    else
                    {
                        return adminLight.Qualifications.Where((q) => (q.Related.Contains(campaignId)));
                    }
                }
                
                //Only qualification of 1 activity
                if (parameter as string == "combobox")
                {

                    int levelCount = 0;
                    NixxisComboBoxList list = new NixxisComboBoxList();

                    foreach (AdminObjectLight item in adminLight.Qualifications.Where((q) => (q.Related.Contains(campaignId))))
                    {
                        list.Add(new NixxisComboBoxItem(item.Id, item.Description, 0));
                    }

                    return list;
                }
                else
                {
                    return adminLight.Qualifications.Where((q) => (q.Related.Contains(campaignId)));
                }
            }
            catch
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class NixxisTreeComboPadding : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                return new Thickness((int)value * 30, 0, 0, 0);


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

    //
    //Search parameter convertors
    //
    public class ParameterPositiveToDescription : IValueConverter
    {
        public static TranslationContext TranslationContext = new TranslationContext("CommonConverters");

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int val = (int)value;

                if (val > 0)
                    return TranslationContext.Translate("Positive");
                else if (val < 0)
                    return TranslationContext.Translate("Negative");
                else
                    return TranslationContext.Translate("Neutral");
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ParameterArguedToDescription : IValueConverter
    {
        public static TranslationContext TranslationContext = new TranslationContext("CommonConverters");

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                    return TranslationContext.Translate("Argued");
                else
                    return TranslationContext.Translate("Not argued");
            }
            catch
            { return value; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterForUpdateCommand : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
            {
                if (value != null)
                {
                    if (value is ContactData)
                    {
                        ContactData cd = (ContactData)value;
                        return cd.UpdateCommandEnabled;
                    }
                    return false;
                }
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
