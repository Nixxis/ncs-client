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
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Nixxis.Client.Controls
{

    public class NixxisChatListBox : ListBox
    {
        #region Class data
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty AgentImageSourceProperty = DependencyProperty.Register("AgentImageSource", typeof(ImageSource), typeof(NixxisChatListBox), new PropertyMetadata(AgentImageSourcePropertyChanged));
        public ImageSource AgentImageSource
        {
            get { return (ImageSource)GetValue(AgentImageSourceProperty); }
            set { SetValue(AgentImageSourceProperty, value); }
        }
        public static void AgentImageSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisChatListBox item = (NixxisChatListBox)obj;
            item.UpdateAgentImageSource();
        }

        public static readonly DependencyProperty CustomerImageSourceProperty = DependencyProperty.Register("CustomerImageSource", typeof(ImageSource), typeof(NixxisChatListBox), new PropertyMetadata(CustomerImageSourcePropertyChanged));
        public ImageSource CustomerImageSource
        {
            get { return (ImageSource)GetValue(CustomerImageSourceProperty); }
            set { SetValue(CustomerImageSourceProperty, value); }
        }
        public static void CustomerImageSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisChatListBox item = (NixxisChatListBox)obj;
            item.UpdateCustomerImageSource();
        }
        #endregion

        #region Constructors
        static NixxisChatListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisChatListBox), new FrameworkPropertyMetadata(typeof(NixxisChatListBox)));
        }
        #endregion

        #region Members override
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (e.NewItems != null)
            {
                this.ScrollIntoView(e.NewItems[e.NewItems.Count - 1]);
            }
        }
        #endregion

        #region Members
        private void UpdateAgentImageSource()
        {
        }

        private void UpdateCustomerImageSource()
        {
        }
        #endregion

        #region Handlers
        #endregion
    }

    [ValueConversion(typeof(Nixxis.Client.ChatLine), typeof(string))]
    public class ChatLineToHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return string.Empty;

                if (value.GetType() == typeof(NixxisUserChatLine))
                {
                    NixxisUserChatLine line = value as NixxisUserChatLine;

                    return "[" + line.Time.ToString("HH:mm:ss") + "] " + line.From + "";
                }
                else
                {
                    Nixxis.Client.ChatLine line = value as Nixxis.Client.ChatLine;

                    return "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + line.From + "";
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

    [ValueConversion(typeof(Nixxis.Client.ChatLine), typeof(string))]
    public class ChatLineToOneLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return string.Empty;

                if (value.GetType() == typeof(NixxisUserChatLine))
                {
                    NixxisUserChatLine line = value as NixxisUserChatLine;

                    return "[" + line.Time.ToString("HH:mm:ss") + "] " + line.From + ": " + line.Message; 
                }
                else
                {
                    Nixxis.Client.ChatLine line = value as Nixxis.Client.ChatLine;

                    return "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + line.From + ": " + line.Message; 
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
}

