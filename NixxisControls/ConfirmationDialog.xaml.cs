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
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : Window
    {
        public static readonly DependencyProperty MessageTextProperty = DependencyProperty.Register("MessageText", typeof(string), typeof(ConfirmationDialog));
        public static readonly DependencyProperty IsInfoDialogProperty = DependencyProperty.Register("IsInfoDialog", typeof(bool), typeof(ConfirmationDialog), new PropertyMetadata(false, new PropertyChangedCallback(InfoDialogChanged)));

        public static void InfoDialogChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ConfirmationDialog cnfDlg = obj as ConfirmationDialog;

            if ((bool)args.NewValue)
            {
                cnfDlg.Title = "Information...";
            }
            else
            {
                cnfDlg.Title = "Confirmation...";
            }
        }

        public string MessageText
        {
            get
            {
                return (string)GetValue(MessageTextProperty);
            }
            set
            {
                SetValue(MessageTextProperty, value);
            }
        }
        public bool IsInfoDialog
        {
            get
            {
                return (bool)GetValue(IsInfoDialogProperty);
            }
            set
            {
                SetValue(IsInfoDialogProperty, value);
            }
        }

        public ConfirmationDialog()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
