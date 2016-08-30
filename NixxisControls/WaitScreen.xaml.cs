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
using System.Windows.Shapes;

namespace Nixxis.Client.Controls
{
    /// <summary>
    /// Interaction logic for WaitScreen.xaml
    /// </summary>
    public partial class WaitScreen : Window
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(WaitScreen), new PropertyMetadata("Please wait...", new PropertyChangedCallback(TextChanged)));

        public void LoadProgressEvent(string description)
        {
            Text = description;
            Helpers.WaitForPriority();
        }

        public void DetailedLoadProgressEvent(int progress, string description, string progressDescription)
        {
            Progress = progress;
            ProgressDescription = progressDescription;
            Text = description;
            Helpers.WaitForPriority();
        }

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty ProgressDescriptionProperty = DependencyProperty.Register("ProgressDescription", typeof(string), typeof(WaitScreen), new PropertyMetadata(null, new PropertyChangedCallback(ProgressDescriptionChanged)));

        public string ProgressDescription
        {
            get
            {
                return (string)GetValue(ProgressDescriptionProperty);
            }
            set
            {
                SetValue(ProgressDescriptionProperty, value);
            }
        }


        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(int), typeof(WaitScreen), new PropertyMetadata(-1, new PropertyChangedCallback(ProgressChanged)));

        public int Progress
        {
            get
            {
                return (int)GetValue(ProgressProperty);
            }
            set
            {
                SetValue(ProgressProperty, value);
            }
        }


        public WaitScreen()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }


        public static void TextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((WaitScreen)obj).txtText.Text =  (string) args.NewValue;
        }

        public static void ProgressDescriptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((WaitScreen)obj).txtProgressDescription.Text = (string)args.NewValue;
        }


        public static void ProgressChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            int newvalue = (int)args.NewValue;
            WaitScreen ws = (WaitScreen)obj;
            ws.txtProgressPercent.Text = string.Concat(newvalue.ToString(), "%");
            if (newvalue >= 0)
            {
                ws.progress.Value = newvalue;
                if (ws.progress.Visibility != Visibility.Visible)
                {
                    ws.progress.Visibility = Visibility.Visible;
                    ws.txtProgressPercent.Visibility = Visibility.Visible;
                    ws.txtProgressDescription.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (ws.progress.Visibility != Visibility.Collapsed)
                {
                    ws.progress.Visibility = Visibility.Collapsed;
                    ws.txtProgressPercent.Visibility = Visibility.Collapsed;
                    ws.txtProgressDescription.Visibility = Visibility.Collapsed;
                }
            }
        }


    }
}
