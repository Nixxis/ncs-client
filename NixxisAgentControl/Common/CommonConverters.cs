using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Agent
{
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(ContactInfo), typeof(string))]
    public class InfoPanelOriginatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                ContactInfo contactInfo = (ContactInfo)value;

                if (contactInfo.Direction == "I")
                {
                    return contactInfo.From;
                }
                else
                {
                    return contactInfo.To;
                }

            }
            catch { return value; }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class MediaTypeIcon : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool small = parameter as string == "small" ? true : false;
                

                string media = values[0] as string;
                string direction = values[1] as string;

                if (media == "V" && direction == "I")
                    return small ? new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/MediaTypes/MediaType_Inbound25.png")) : new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/MediaTypes/MediaType_Inbound.png"));
                else if (media == "V" && direction == "O")
                    return small ? new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/MediaTypes/MediaType_Outbound25.png")) : new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/MediaTypes/MediaType_Outbound.png"));
                else if (media == "M")
                    return small ? new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/MediaTypes/MediaType_Mail25.png")) : new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/MediaTypes/MediaType_Mail.png"));
                else if (media == "C")
                    return small ? new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/MediaTypes/MediaType_Chat25.png")) :new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/MediaTypes/MediaType_Chat.png"));
                else
                    return small ? new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/MediaTypes/MediaType_Undefined25.png")) : new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/MediaTypes/MediaType_Undefined.png"));

            }
            catch
            {
                return new BitmapImage(new Uri("pack://application:,,,/NixxisAgentControl;component/Images/MediaTypes/MediaType_Undefined25.png")); 
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MediaTypeDescription : IMultiValueConverter
    {
        public static TranslationContext TranslationContext = new TranslationContext("CommonConverters");

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string media = values[0] as string;
                string direction = values[1] as string;
                string mediaDescription = string.Empty;
                string directionDescription = string.Empty;

                if (media == "V")
                    mediaDescription = TranslationContext.Translate("Voice");
                else if (media == "C")
                    mediaDescription = TranslationContext.Translate("Chat");
                else if (media == "M")
                    mediaDescription = TranslationContext.Translate("E-Mail");
                else
                    mediaDescription = TranslationContext.Translate("Unknow");

                if (direction == "I")
                    directionDescription = TranslationContext.Translate("Inbound");
                else if (direction == "O")
                    directionDescription = TranslationContext.Translate("Outbound");
                else
                    directionDescription = TranslationContext.Translate("Unknow");

                return string.Format("{0} ({1})", mediaDescription, directionDescription);
            }
            catch
            {
                return TranslationContext.Translate("Undefined");
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    [ValueConversion(typeof(string), typeof(string))]
    public class StyleBold_StartsWithConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (string.IsNullOrEmpty(parameter as string))
                    return "Normal";

                if (string.IsNullOrEmpty(value as string))
                {
                    return "Normal";
                }
                else
                {
                    if (value.ToString().StartsWith(parameter.ToString()))
                        return "Bold";
                    else
                        return "Normal";
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

}