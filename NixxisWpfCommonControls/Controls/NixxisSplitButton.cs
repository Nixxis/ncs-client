using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Nixxis.Client.Controls
{
    [TemplatePart(Name = "PART_DefaultAction", Type = typeof(ButtonBase)),
     TemplatePart(Name = "PART_SelectAction", Type = typeof(ButtonBase))]
    public class NixxisSplitButton : Control
    {
        #region Class data
        private ButtonBase m_BtnAction = null;
        private ButtonBase m_BtnSelection = null;
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(NixxisSplitButton), new PropertyMetadata(new CornerRadius(3)));
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty HasFocusProperty = DependencyProperty.Register("HasFocus", typeof(bool), typeof(NixxisSplitButton), new PropertyMetadata(false));
        public bool HasFocus
        {
            get { return (bool)GetValue(HasFocusProperty); }
            set { SetValue(HasFocusProperty, value); }
        }

        public static readonly DependencyProperty DropDownButtonStyleProperty = DependencyProperty.Register("DropDownButtonStyle", typeof(Style), typeof(NixxisSplitButton));
        public Style DropDownButtonStyle
        {
            get { return (Style)GetValue(DropDownButtonStyleProperty); }
            set { SetValue(DropDownButtonStyleProperty, value); }
        }

        public static readonly DependencyProperty DefaultActionStyleProperty = DependencyProperty.Register("DefaultActionStyle", typeof(Style), typeof(NixxisSplitButton));
        public Style DefaultActionStyle
        {
            get { return (Style)GetValue(DefaultActionStyleProperty); }
            set { SetValue(DefaultActionStyleProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(NixxisSplitButton));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextVisibilityProperty = DependencyProperty.Register("TextVisibility", typeof(Visibility), typeof(NixxisSplitButton), new PropertyMetadata(Visibility.Collapsed));
        public Visibility TextVisibility
        {
            get { return (Visibility)GetValue(TextVisibilityProperty); }
            set { SetValue(TextVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(NixxisSplitButton));
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageVisibilityProperty = DependencyProperty.Register("ImageVisibility", typeof(Visibility), typeof(NixxisSplitButton), new PropertyMetadata(Visibility.Visible));
        public Visibility ImageVisibility
        {
            get { return (Visibility)GetValue(ImageVisibilityProperty); }
            set { SetValue(ImageVisibilityProperty, value); }
        }
        #endregion        
        
        #region Constructors
        static NixxisSplitButton()
        {  
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisSplitButton), new FrameworkPropertyMetadata(typeof(NixxisSplitButton)));            
        }
        #endregion

        #region Members Override
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            CheckFocus();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            CheckFocus();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_BtnAction = GetTemplateChild("PART_DefaultAction") as ButtonBase;
            m_BtnAction.Click += BtnAction_Click;
            m_BtnAction.GotFocus += Common_Focus;
            m_BtnAction.LostFocus += Common_Focus;

            m_BtnSelection = GetTemplateChild("PART_SelectAction") as ButtonBase;
            m_BtnSelection.Click += BtnSelection_Click;
            m_BtnSelection.GotFocus += Common_Focus;
            m_BtnSelection.LostFocus += Common_Focus;
        }
        #endregion
        
        #region Members
        private void CheckFocus()
        {
            this.HasFocus = this.IsFocused;
        }
        
        private void Common_Focus(object sender, RoutedEventArgs e)
        {
            this.CheckFocus();
        }

        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            OnClick();
         
        }
        private void BtnSelection_Click(object sender, RoutedEventArgs e)
        {
            if (m_BtnAction != null && this.ContextMenu != null)
            {
                this.ContextMenu.PlacementTarget = m_BtnAction;
                this.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                this.ContextMenu.IsOpen = true;
            }
        }
        #endregion

        #region Events
        public event NixxisSplitButtonEventHandler Click;
        protected void OnClick()
        {
            if (Click != null)
                Click(this, new RoutedEventArgs());
        }
        #endregion
    }

    public delegate void NixxisSplitButtonEventHandler(object sender, RoutedEventArgs e);
}
