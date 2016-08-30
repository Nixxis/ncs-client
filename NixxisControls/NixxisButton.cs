using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Input;

namespace Nixxis.Client.Controls
{
    public class NixxisButton : ToggleButton
    {
        private class OrConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                for(int i=0; i< values.Length; i++)
                    if((bool)(values[i]))
                        return true;
                return false;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            {
                object[] objs = new object[targetTypes.Length];
                for (int i = 0; i < objs.Length; i++)
                    objs[i] = false;
                return objs;
            }
        }

        static NixxisButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisButton), new FrameworkPropertyMetadata(typeof(NixxisButton)));            
        }

        public static readonly DependencyProperty DropDownCloserProperty = DependencyProperty.RegisterAttached("DropDownCloser", typeof(bool), typeof(NixxisButton), new PropertyMetadata(false, new PropertyChangedCallback(DropDownCloserChanged)));

        /// <summary>
        /// Indicates if the command associated wit the the current button will come from its selected dropdown
        /// </summary>
        public static readonly DependencyProperty DropDownReplaceCommandProperty = DependencyProperty.Register("DropDownReplaceCommand", typeof(bool), typeof(NixxisButton), new PropertyMetadata(false));        
        /// <summary>
        /// Indicates if the content of the current button will be the one of the selected dropdown
        /// </summary>
        public static readonly DependencyProperty DropDownReplaceContentProperty = DependencyProperty.Register("DropDownReplaceContent", typeof(bool), typeof(NixxisButton), new PropertyMetadata(false, new PropertyChangedCallback(DropDownReplaceContentChanged)));
        public static readonly DependencyProperty DropDownReplaceIsCheckedProperty = DependencyProperty.Register("DropDownReplaceIsChecked", typeof(bool), typeof(NixxisButton), new PropertyMetadata(false, new PropertyChangedCallback(DropDownReplaceIsCheckedChanged)));

        /// <summary>
        /// When the current button has dropdown buttons, indicates if the small V expand button is still needed
        /// </summary>
        public static readonly DependencyProperty KeepDropDownButtonProperty = DependencyProperty.Register("KeepDropDownButton", typeof(bool), typeof(NixxisButton), new PropertyMetadata(false));
        public static readonly DependencyProperty ClickMeansDropDownProperty = DependencyProperty.Register("ClickMeansDropDown", typeof(bool), typeof(NixxisButton), new PropertyMetadata(false));        
        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register("ImagePath", typeof(string), typeof(NixxisButton));
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(NixxisButton));
        public static readonly DependencyProperty IsLightedProperty = DependencyProperty.Register("IsLighted", typeof(bool?), typeof(NixxisButton));
        public static readonly DependencyProperty IsToggleButtonProperty = DependencyProperty.Register("IsToggleButton", typeof(bool), typeof(NixxisButton), new PropertyMetadata(false));
        public static readonly DependencyProperty DropDownProperty = DependencyProperty.Register("DropDown", typeof(ContextMenu), typeof(NixxisButton), new UIPropertyMetadata(null, new PropertyChangedCallback(DropDownChanged)));
        public static readonly DependencyProperty HasDropDownProperty = DependencyProperty.Register("HasDropDown", typeof(bool), typeof(NixxisButton), new UIPropertyMetadata(false));
        public static readonly DependencyProperty DropDownButtonProperty = DependencyProperty.Register("DropDownButton", typeof(ToggleButton), typeof(NixxisButton));
        public static readonly DependencyProperty DropDownButtonTemplateProperty = DependencyProperty.Register("DropDownButtonTemplate", typeof(ControlTemplate), typeof(NixxisButton), new PropertyMetadata(null, new PropertyChangedCallback(DropDownButtonTemplateChanged)));
        public static readonly DependencyProperty ContextMenuStyleProperty = DependencyProperty.Register("ContextMenuStyle", typeof(Style), typeof(NixxisButton), new PropertyMetadata(null, new PropertyChangedCallback(ContextMenuStyleChanged)));

        public static void DropDownCloserChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                FrameworkElement elm = obj as FrameworkElement;

                if (elm != null)
                {
                    ContextMenu owner = Helpers.FindParent<ContextMenu>(elm);
                    NixxisButton btn = owner.PlacementTarget as NixxisButton;
                    if (btn != null)
                    {
                        if ((bool)args.NewValue)
                        {
                            elm.AddHandler(Button.ClickEvent, new RoutedEventHandler(btn.Closer_Clicked));
                            if (elm is NixxisButton)
                            {
                                NixxisButton nb = elm as NixxisButton;
                                btn.AddIsCheckedBinding(nb);
                            }
                        }
                        else
                        {
                            elm.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(btn.Closer_Clicked));
                        }
                    }
                }
            }
        }

        public static void DropDownReplaceContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null && (bool)args.NewValue)
            {
                NixxisButton btn = obj as NixxisButton;
                if(!btn.KeepDropDownButton)
                    btn.SetValue(HasDropDownProperty, false);
            }

        }
        public static void DropDownReplaceIsCheckedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null && (bool)args.NewValue)
            {
                NixxisButton btn = obj as NixxisButton;
                if (!btn.KeepDropDownButton)
                    btn.SetValue(HasDropDownProperty, false);

            }


        }
        public static void DropDownChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                NixxisButton btn = obj as NixxisButton;

                btn.SetValue(HasDropDownProperty, (!btn.DropDownReplaceContent &&  !btn.DropDownReplaceIsChecked) || btn.KeepDropDownButton);

                if (btn.ContextMenuStyle != null)
                    (args.NewValue as ContextMenu).Style = btn.ContextMenuStyle;
                (args.NewValue as ContextMenu).PlacementTarget = btn;
                (args.NewValue as ContextMenu).Placement = PlacementMode.Bottom;
                (args.NewValue as ContextMenu).Closed += new RoutedEventHandler(btn.DropDown_ContextMenuClosed);
            }
        }
        public static void DropDownButtonTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisButton btn = obj as NixxisButton;
            if (btn != null && btn.DropDownButton != null)
                btn.DropDownButton.Template = args.NewValue as ControlTemplate;
        }
        public static void ContextMenuStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisButton btn = obj as NixxisButton;
            if (btn != null && btn.DropDown != null && args.NewValue != null && (args.NewValue as Style).TargetType.Equals(btn.DropDown.GetType()))
                btn.DropDown.Style = args.NewValue as Style;
        }

        public static void SetDropDownCloser(UIElement element, bool value)
        {
            element.SetValue(DropDownCloserProperty, value);
        }
        public static bool GetDropDownCloser(UIElement element)
        {
            return (bool)(element.GetValue(DropDownCloserProperty));
        }
        
        public bool KeepDropDownButton
        {
            get
            {
                return (bool)GetValue(KeepDropDownButtonProperty);
            }
            set
            {
                SetValue(KeepDropDownButtonProperty, value);
            }
        }
        public bool ClickMeansDropDown
        {
            get
            {
                return (bool)GetValue(ClickMeansDropDownProperty);
            }
            set
            {
                SetValue(ClickMeansDropDownProperty, value);
            }
        }
        public bool DropDownReplaceCommand
        {
            get
            {
                return (bool)GetValue(DropDownReplaceCommandProperty);
            }
            set
            {
                SetValue(DropDownReplaceCommandProperty, value);
            }
        }
        public bool DropDownReplaceContent
        {
            get
            {
                return (bool)GetValue(DropDownReplaceContentProperty);
            }
            set
            {
                SetValue(DropDownReplaceContentProperty, value);
            }
        }
        public bool DropDownReplaceIsChecked
        {
            get
            {
                return (bool)GetValue(DropDownReplaceIsCheckedProperty);
            }
            set
            {
                SetValue(DropDownReplaceIsCheckedProperty, value);
            }
        }
        public ToggleButton DropDownButton
        {
            get
            {
                return (ToggleButton)GetValue(DropDownButtonProperty);
            }
            set
            {
                SetValue(DropDownButtonProperty, value);
            }
        }
        public bool? IsLighted
        {
            get
            {
                return (bool?)GetValue(IsLightedProperty);
            }
            set
            {
                SetValue(IsLightedProperty, value);
            }
        }
        public string ImagePath
        {
            get
            {
                return (string)GetValue(ImagePathProperty);
            }
            set
            {
                SetValue(ImagePathProperty, value);
            }
        }
        public ImageSource ImageSource
        {
            get
            {
                return (ImageSource)GetValue(ImageSourceProperty);
            }
            set
            {
                SetValue(ImageSourceProperty, value);
            }
        }
        public bool IsToggleButton
        {
            get
            {
                return (bool)GetValue(IsToggleButtonProperty);
            }
            set
            {
                SetValue(IsToggleButtonProperty, value);
            }
        }
        public ContextMenu DropDown
        {
            get
            {
                return (ContextMenu)GetValue(DropDownProperty);
            }
            set
            {
                SetValue(DropDownProperty, value);
            }
        }
        public ControlTemplate DropDownButtonTemplate
        {
            get
            {
                return GetValue(DropDownButtonTemplateProperty) as ControlTemplate;
            }
            set
            {
                SetValue(DropDownButtonTemplateProperty, value);
            }
        }
        public Style ContextMenuStyle
        {
            get
            {
                return GetValue(ContextMenuStyleProperty) as Style;
            }
            set
            {
                SetValue(ContextMenuStyleProperty, value);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {            
            base.OnInitialized(e);
            
            ToggleButton toggle = new ToggleButton();
            toggle.Checked += new RoutedEventHandler(Toggle_Checked);
            toggle.Unchecked += new RoutedEventHandler(Toggle_Unchecked);
            if (DropDownButtonTemplate != null)
                toggle.Template = DropDownButtonTemplate;
            DropDownButton = toggle;
        }

        void Closer_Clicked(object sender, RoutedEventArgs eBase)
        {
            if (sender is NixxisButton)
            {
                NixxisButton clicked = (NixxisButton)sender;

                if (DropDownReplaceContent)
                {
                    ImageSource = clicked.ImageSource;
                    Foreground = clicked.Foreground;
                    Background = clicked.Background;
                    Content = clicked.Content;
                    if (DropDownReplaceCommand)
                    {
                        Command = clicked.Command;
                        CommandParameter = clicked.CommandParameter;
                        CommandTarget = clicked.CommandTarget;
                    }
                }
            }
            DropDownButton.IsChecked = false;
        }
        void Toggle_Checked(object sender, RoutedEventArgs e)
        {

            if (DropDownOpening != null)
                DropDownOpening(sender, e);

            DropDown.IsOpen = true;
        }
        void Toggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DropDownClosing != null)
                DropDownClosing(sender, e);

            DropDown.IsOpen = false;
        }
        void DropDown_ContextMenuClosed(object sender, RoutedEventArgs e)
        {
            DropDownButton.IsChecked = false;
        }

        private void AddIsCheckedBinding(NixxisButton btn)
        {
            MultiBinding isCheckedBinding;            
            isCheckedBinding = new MultiBinding();
            isCheckedBinding.Converter = new OrConverter();

            MultiBinding backup = BindingOperations.GetMultiBinding(this, ToggleButton.IsCheckedProperty);
            if (backup != null)
            {
                foreach(BindingBase bb in backup.Bindings)
                {
                    isCheckedBinding.Bindings.Add(bb);     
                }
            }

            isCheckedBinding.Bindings.Add(new Binding() { Source = btn, Path = new PropertyPath(ToggleButton.IsCheckedProperty), Mode= BindingMode.OneWay});
            
            if (DropDownReplaceIsChecked)
            {
                BindingOperations.SetBinding(this, ToggleButton.IsCheckedProperty, isCheckedBinding);
            }
        }

        protected override void OnClick()
        {
            bool? backup = IsChecked;
            base.OnClick();
            if (!IsToggleButton)
                IsChecked = backup;

            if (DropDownReplaceContent && !KeepDropDownButton)
                DropDownButton.IsChecked = true;

            if(ClickMeansDropDown && DropDown!=null)
                DropDownButton.IsChecked = true;
        }

        public event RoutedEventHandler DropDownOpening;
        public event RoutedEventHandler DropDownClosing;


    }

    public class NixxisContextMenu : ContextMenu
    {
        static NixxisContextMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisContextMenu), new FrameworkPropertyMetadata(typeof(NixxisContextMenu)));
        }
    }

    public class CoverflowElement : ToggleButton
    {
    }
}
