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

namespace Nixxis.Client.Controls
{


    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {

        public static readonly DependencyProperty NumberFormatProperty = DependencyProperty.Register("NumberFormat", typeof(string), typeof(NumericUpDown), new PropertyMetadata("0.0"));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(Decimal), typeof(NumericUpDown), new FrameworkPropertyMetadata(Decimal.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty MinimumValueProperty = DependencyProperty.Register("MinimumValue", typeof(Decimal), typeof(NumericUpDown), new PropertyMetadata(Decimal.Zero));
        public static readonly DependencyProperty IsInExceptionProperty = DependencyProperty.Register("IsInException", typeof(bool), typeof(NumericUpDown), new PropertyMetadata(false));
        public static readonly DependencyProperty MaximumValueProperty = DependencyProperty.Register("MaximumValue", typeof(Decimal), typeof(NumericUpDown), new PropertyMetadata(Decimal.MaxValue));
        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(Decimal), typeof(NumericUpDown), new PropertyMetadata(Decimal.One));

        public string NumberFormat
        {
            get
            {
                return (string)GetValue(NumberFormatProperty);
            }
            set
            {
                SetValue(NumberFormatProperty, value);
            }
        }
        public Decimal Value
        {
            get
            {
                return (Decimal)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }
        public Decimal MinimumValue
        {
            get
            {
                return (Decimal)GetValue(MinimumValueProperty);
            }
            set
            {
                SetValue(MinimumValueProperty, value);
            }
        }
        public Decimal MaximumValue
        {
            get
            {
                return (Decimal)GetValue(MaximumValueProperty);
            }
            set
            {
                SetValue(MaximumValueProperty, value);
            }
        }
        public bool IsInException
        {
            get
            {
                return (bool)GetValue(IsInExceptionProperty);
            }
            internal set
            {
                SetValue(IsInExceptionProperty, value);
            }
        }

        public Decimal Increment
        {
            get
            {
                return (Decimal)GetValue(IncrementProperty);
            }
            set
            {
                SetValue(IncrementProperty, value);
            }
        }

        public NumericUpDown()
        {
            InitializeComponent();
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            if (Value + Increment > MaximumValue)
                Value = MaximumValue;
            else
                Value+=Increment;
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (Value - Increment < MinimumValue)
                Value = MinimumValue;
            else
                Value -= Increment;
        }



        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox t = sender as TextBox;

            Decimal i;
            if (!Decimal.TryParse(t.Text, out i))
                return;

            switch (e.Key)
            {
                case System.Windows.Input.Key.Up:
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        i += 2 * Increment;
                    else
                        i += Increment;
                    break;
                case System.Windows.Input.Key.Down:
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        i -= 2 * Increment;
                    else
                        i -= Increment;
                    break;
                case System.Windows.Input.Key.PageUp:
                    i += 3*Increment;
                    break;
                case System.Windows.Input.Key.PageDown:
                    i -= 3*Increment;
                    break;
                default:
                    return;
            }

            if (i < MinimumValue)
                i = MinimumValue;
            if (i > MaximumValue)
                i = MaximumValue;

            if (BindingOperations.IsDataBound(t, TextBox.TextProperty))
            {
                try
                {
                    Binding binding = BindingOperations.GetBinding(t, TextBox.TextProperty);
                    t.Text = (string)binding.Converter.Convert(i, null, binding.ConverterParameter, binding.ConverterCulture);
                }
                catch
                {
                    t.Text = i.ToString();
                }
            }
            else
                t.Text = i.ToString();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox t = sender as TextBox;
            Decimal i;

            if (!Decimal.TryParse(string.Concat(t.Text, e.Text), out i))
            {
                e.Handled = true;
                return;
            }

        }

        private void txtInput_Error(object sender, ValidationErrorEventArgs e)
        {
            
            if (e.Action == ValidationErrorEventAction.Added)
            {
                IsInException = true;
            }
            else
            {
                IsInException = false;
            }
        }
    }




    public class DecimalConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((Decimal)values[0]).ToString((string)(values[1]));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {

            Decimal temp;
            if (Decimal.TryParse(value as string, out temp))
                return new object[]{temp, "0"};
            return null;
        }
    }
}
