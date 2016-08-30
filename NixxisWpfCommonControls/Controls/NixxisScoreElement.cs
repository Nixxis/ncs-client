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
using System.Windows.Controls.Primitives;

namespace Nixxis.Client.Controls
{
    [TemplatePart(Name = "PART_Label", Type = typeof(Label)),
     TemplatePart(Name = "PART_Score", Type = typeof(Panel)),
     TemplatePart(Name = "PART_Reset", Type = typeof(ButtonBase))]
    public class NixxisScoreElement : Control
    {
        #region Class data
        private ButtonBase m_BtnReset;
        private Panel m_ScoreRootPanel;
        private Rectangle m_ScoreNullObject;
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(NixxisScoreElement), new PropertyMetadata(""));
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty ShowLabelProperty = DependencyProperty.Register("ShowLabel", typeof(bool), typeof(NixxisScoreElement), new PropertyMetadata(true));
        public bool ShowLabel
        {
            get { return (bool)GetValue(ShowLabelProperty); }
            set { SetValue(ShowLabelProperty, value); }
        }

        public static readonly DependencyProperty ShowResetProperty = DependencyProperty.Register("ShowReset", typeof(bool), typeof(NixxisScoreElement), new PropertyMetadata(true));
        public bool ShowReset
        {
            get { return (bool)GetValue(ShowResetProperty); }
            set { SetValue(ShowResetProperty, value); }
        }

        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register("Length", typeof(int), typeof(NixxisScoreElement), new PropertyMetadata(5, LengthPropertyChanged));
        public int Length
        {
            get { return (int)GetValue(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }
        public static void LengthPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisScoreElement se = (NixxisScoreElement)obj;

            se.SetInitScore();
        }

        public static readonly DependencyProperty ActiveImageSourceProperty = DependencyProperty.Register("ActiveImageSource", typeof(ImageSource), typeof(NixxisScoreElement), new PropertyMetadata(ActiveImageSourcePropertyChanged));
        public ImageSource ActiveImageSource
        {
            get { return (ImageSource)GetValue(ActiveImageSourceProperty); }
            set { SetValue(ActiveImageSourceProperty, value); }
        }
        public static void ActiveImageSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisScoreElement se = (NixxisScoreElement)obj;

            se.UpdateScore();
        }

        public static readonly DependencyProperty InactiveImageSourceProperty = DependencyProperty.Register("InactiveImageSource", typeof(ImageSource), typeof(NixxisScoreElement), new PropertyMetadata(InactiveImageSourcePropertyChanged));
        public ImageSource InactiveImageSource
        {
            get { return (ImageSource)GetValue(InactiveImageSourceProperty); }
            set { SetValue(InactiveImageSourceProperty, value); }
        }
        public static void InactiveImageSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisScoreElement se = (NixxisScoreElement)obj;

            se.UpdateScore();
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(NixxisScoreElement), new PropertyMetadata(0, ValuePropertyChanged));
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static void ValuePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisScoreElement se = (NixxisScoreElement)obj;

            se.UpdateScore();
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(NixxisScoreElement), new PropertyMetadata(new CornerRadius(0)));
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        #endregion

        #region Constructors
        static NixxisScoreElement()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisScoreElement), new FrameworkPropertyMetadata(typeof(NixxisScoreElement)));
        }
        #endregion

        #region Members Override
        public override void OnApplyTemplate()
        {
            //ScoreNull rect
            m_ScoreNullObject = GetTemplateChild("ScoreNull") as Rectangle;
            m_ScoreNullObject.MouseEnter += new MouseEventHandler(ScoreNullObject_MouseEnter);
            m_ScoreNullObject.MouseUp += new MouseButtonEventHandler(ScoreNullObject_MouseUp);

            //ScorePanel
            m_ScoreRootPanel = GetTemplateChild("PART_Score") as Panel;
            SetInitScore();

            //Reset button
            m_BtnReset = GetTemplateChild("PART_Reset") as ButtonBase;
            m_BtnReset.Click += btnReset_Click;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            m_ScoreMouseDown = false;
        }
        
        #endregion

        #region Members
        private void UpdateScore()
        {
            try
            {
                if (m_ScoreRootPanel == null)
                    return;

                foreach (UIElement item in m_ScoreRootPanel.Children)
                {
                    Image img = (Image)item;

                    if ((int)img.Tag <= Value)
                        img.Source = this.ActiveImageSource;
                    else
                        img.Source = this.InactiveImageSource;
                }
            }
            catch { }
        }

        private void SetInitScore()
        {
            try
            {
                m_ScoreRootPanel.Children.Clear();

                for (int i = 0; i < Length; i++)
                {
                    Image img = new Image();

                    if (i < Value)
                        img.Source = this.ActiveImageSource;
                    else
                        img.Source = this.InactiveImageSource;

                    img.Tag = i + 1;
                    img.MouseDown += new MouseButtonEventHandler(img_MouseDown);
                    img.MouseUp += new MouseButtonEventHandler(img_MouseUp);
                    img.MouseEnter += new MouseEventHandler(img_MouseEnter);
                    m_ScoreRootPanel.Children.Add(img);
                }
            }
            catch { }
        }
        #endregion

        #region Handlers
        private bool m_ScoreMouseDown = false;

        private void img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            Value = (int)img.Tag;
            UpdateScore();

            m_ScoreMouseDown = true;
        }

        private void img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            m_ScoreMouseDown = false;
        }

        private void img_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!m_ScoreMouseDown)
                return;
            
            Image img = (Image)sender;
            Value = (int)img.Tag;
        }

        private void ScoreNullObject_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!m_ScoreMouseDown)
                return;

            Value = 0;
        }

        private void ScoreNullObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            m_ScoreMouseDown = false;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Value = 0;
        }
        #endregion
    }
}
