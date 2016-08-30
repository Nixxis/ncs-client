using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Specialized;
using System.IO;

namespace Nixxis.Client.Controls
{

    [ValueConversion(typeof(string), typeof(string))]
    public class TranslationConverter : IValueConverter, IMultiValueConverter
    {

        private static TranslationConverter m_TranslationConverter = new TranslationConverter();

        public static TranslationConverter Default
        {
            get
            {
                return m_TranslationConverter;
            }
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                TranslationContext context = null;
                if (value is TranslationContext)
                {
                    context = (TranslationContext)value;

                    return context.Translate(parameter.ToString());
                }
                else
                {
                    if (parameter != null && parameter is TranslationContext)
                    {
                        context = (TranslationContext)parameter;
                        return context.Translate(value.ToString());
                    }
                }
            }
            catch
            {
            }
            return parameter.ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string strText = values[0] as string;
                TranslationContext context = values[1] as TranslationContext;

                if(values.Length>2)
                {
                    context = new TranslationContext(context.Context + values[2].GetType().Name);
                }

                return context.Translate(strText);
            }
            catch
            {

            }
            return values[0].ToString();

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
