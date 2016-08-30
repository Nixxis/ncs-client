using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Nixxis.Client.Controls
{
    [TemplatePart(Name = "PART_ColorSelection", Type = typeof(ListBox))]
    public class NixxisColorPicker : Control
    {
        #region Class data
        private NixxisColorCollection m_StandardColors = new NixxisColorCollection();
        private ListBox m_StandardList;
        #endregion

        #region Properties
        public NixxisColorCollection StandardColors
        {
            get { return m_StandardColors; }
            set { m_StandardColors = value; }
        }
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(NixxisColorPicker), new PropertyMetadata(new CornerRadius(3)));
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty HasFocusProperty = DependencyProperty.Register("HasFocus", typeof(bool), typeof(NixxisColorPicker), new PropertyMetadata(false));
        public bool HasFocus
        {
            get { return (bool)GetValue(HasFocusProperty); }
            set { SetValue(HasFocusProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(NixxisColor), typeof(NixxisColorPicker), new PropertyMetadata(null, new PropertyChangedCallback(SelectedItemChanged)));
        public static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisColorPicker ctrl = (NixxisColorPicker)obj;
            if (args.NewValue != null 
                && args.NewValue.GetType() == typeof(NixxisColor))
            {
                ctrl.SetColor((NixxisColor)args.NewValue);
                ctrl.SetIndicatorImage();
            }
        }
        public NixxisColor SelectedItem
        {
            get { return (NixxisColor)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty IndicatorImageSourceProperty = DependencyProperty.Register("IndicatorImageSource", typeof(ImageSource), typeof(NixxisColorPicker), new PropertyMetadata(null, new PropertyChangedCallback(IndicatorImageSourceChanged)));
        public static void IndicatorImageSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisColorPicker ctrl = (NixxisColorPicker)obj;
            if (args.NewValue != null
                && args.NewValue.GetType() == typeof(NixxisColor))
            {
                ctrl.m_ImgIndicator = null;
                ctrl.SetIndicatorImage();
            }
        }
        public ImageSource IndicatorImageSource
        {
            get { return (ImageSource)GetValue(IndicatorImageSourceProperty); }
            set { SetValue(IndicatorImageSourceProperty, value); }
        }

        public static readonly DependencyProperty IndicatorRectangleProperty = DependencyProperty.Register("IndicatorRectangle", typeof(Rect), typeof(NixxisColorPicker), new PropertyMetadata(new Rect(), new PropertyChangedCallback(IndicatorRectangleChanged)));
        public static void IndicatorRectangleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisColorPicker ctrl = (NixxisColorPicker)obj;
            if (args.NewValue != null
                && args.NewValue.GetType() == typeof(NixxisColor))
            {
                ctrl.SetIndicatorImage();
            }
        }
        public Rect IndicatorRectangle
        {
            get { return (Rect)GetValue(IndicatorRectangleProperty); }
            set { SetValue(IndicatorRectangleProperty, value); }
        }
        #endregion        
        
        #region Constructors
        static NixxisColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisColorPicker), new FrameworkPropertyMetadata(typeof(NixxisColorPicker)));            
        }
        public NixxisColorPicker()
        {
            m_StandardColors.DefaultForeColor = new NixxisColor("#FFFFFF", "White");
            m_StandardColors.DefaultBackColor = new NixxisColor("#000000", "Black");

            m_StandardColors.Add(m_StandardColors.DefaultBackColor);
            m_StandardColors.Add(new NixxisColor("#C0C0C0", "Silver"));
            m_StandardColors.Add(new NixxisColor("#808080", "Gray"));
            m_StandardColors.Add(m_StandardColors.DefaultForeColor);
            m_StandardColors.Add(new NixxisColor("#800000", "Maroon"));
            m_StandardColors.Add(new NixxisColor("#FF0000", "Red"));
            m_StandardColors.Add(new NixxisColor("#800080", "Purple"));
            m_StandardColors.Add(new NixxisColor("#FF00FF", "Fuchsia"));
            m_StandardColors.Add(new NixxisColor("#008000", "Green"));
            m_StandardColors.Add(new NixxisColor("#00FF00", "Lime"));
            m_StandardColors.Add(new NixxisColor("#808000", "Olive"));
            m_StandardColors.Add(new NixxisColor("#FFFF00", "Yellow"));
            m_StandardColors.Add(new NixxisColor("#000080", "Navy"));
            m_StandardColors.Add(new NixxisColor("#0000FF", "Blue"));
            m_StandardColors.Add(new NixxisColor("#008080", "Teal"));
            m_StandardColors.Add(new NixxisColor("#00FFFF", "Aqua"));
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

            m_StandardList = GetTemplateChild("PART_ColorSelection") as ListBox;
            m_StandardList.ItemsSource = m_StandardColors;
            m_StandardList.SelectionChanged += new SelectionChangedEventHandler(PART_ColorSelection_SelectionChanged);
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

        private void PART_ColorSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_StandardList.SelectedItem != null && m_StandardList.SelectedItem.GetType() == typeof(NixxisColor))
            {
                this.SelectedItem = (NixxisColor)m_StandardList.SelectedItem;
                OnColorSelected(this.SelectedItem);
            }
        }
        private void SetColor(NixxisColor color)
        {
            if (m_StandardList != null)
            {
                m_StandardList.SelectedItem = color;
                OnSelectionChanged(this.SelectedItem);
            }
        }
        /// <summary>
        /// To clear the current selected item from the listbox. This will not affect the SelectedItem property.
        /// </summary>
        public void ClearSelection()
        {
            if (m_StandardList != null)
            {
                m_StandardList.UnselectAll();
            }
        }
        
        //
        //To maintain indicator om image
        //
        private BitmapImage m_ImgIndicator = null;

        public void SetIndicatorImage()
        {
            if (IndicatorImageSource == null)
                return;

            if (m_ImgIndicator == null)
            {
                if (IndicatorImageSource.GetType() == typeof(RenderTargetBitmap))
                {
                    m_ImgIndicator = (BitmapImage)IndicatorImageSource;
                }
                else
                {
                    BitmapImage img = new BitmapImage(new Uri(IndicatorImageSource.ToString()));
                    m_ImgIndicator = img.Clone();
                }
            }
            
            IndicatorImageSource = DrawIndicator(m_ImgIndicator);
        }

        public RenderTargetBitmap DrawIndicator(BitmapImage source)
        {
            if (SelectedItem != null)
            {
                if (this.IndicatorRectangle == null)
                {
                    this.IndicatorRectangle = new Rect();
                }
                return DrawIndicator(source, this.IndicatorRectangle, this.SelectedItem);
            }
            else
                return null;
        }
        public RenderTargetBitmap DrawIndicator(BitmapImage source, Rect rectangle)
        {
            if(SelectedItem != null)
                return DrawIndicator(source, rectangle, this.SelectedItem);
            else
                return null;
        }
        public RenderTargetBitmap DrawIndicator(BitmapImage source, Rect rectangle, NixxisColor color)
        {
            if (source == null) 
                return null;

            RenderTargetBitmap rtb = new RenderTargetBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, PixelFormats.Pbgra32);
            DrawingVisual dv = CreateDrawingVisual(source, new Rect(new Size(source.Width, source.Height)), color.Color, rectangle);
            rtb.Render(dv);

            return rtb;
        }
        
        private DrawingVisual CreateDrawingVisual(ImageSource source, Rect rect, Color color, Rect rectangleIndictator)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(source, rect);
            drawingContext.DrawRectangle(new SolidColorBrush(color), null, rectangleIndictator);
            drawingContext.Close();

            return drawingVisual;
        }
        #endregion

        #region Events
        public event NixxisColorPickerEventHandler SelectionChanged;
        protected void OnSelectionChanged(NixxisColor value)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, value);
        }

        public event NixxisColorPickerEventHandler ColorSelected;
        protected void OnColorSelected(NixxisColor value)
        {
            if (ColorSelected != null)
                ColorSelected(this, value);
        }
        #endregion
    }

    public delegate void NixxisColorPickerEventHandler(object sender, NixxisColor e);

    public class NixxisColorCollection : ObservableCollection<NixxisColor>
    {
        public NixxisColor DefaultForeColor { get; set; }
        public NixxisColor DefaultBackColor { get; set; }

        public NixxisColor GetForeColor(Color value)
        {
            return GetBackColor(value, false);
        }
        public NixxisColor GetForeColor(Color value, bool createWhenNotFound)
        {
            if (value == null)
                return DefaultForeColor;

            foreach (NixxisColor item in this)
            {
                if(item.Color.Equals(value))
                {
                    return item;
                }
            }

            if (createWhenNotFound)
            {
                NixxisColor color = new NixxisColor(value, "New Color");

                this.Add(color);

                return color;
            }
            else
                return DefaultForeColor;
        }
        public NixxisColor GetBackColor(Color value)
        {
            return GetBackColor(value, false);
        }
        public NixxisColor GetBackColor(Color value, bool createWhenNotFound)
        {
            if (value == null)
                return DefaultBackColor;

            foreach (NixxisColor item in this)
            {
                if (item.Color.Equals(value))
                {
                    return item;
                }
            }

            if (createWhenNotFound)
            {
                NixxisColor color = new NixxisColor(value, "New Color");

                this.Add(color);

                return color;
            }
            else
                return DefaultBackColor;
        }

        public static Color ConvertToColor(string value)
        {
            byte red, green, blue;
            // sometimes clrs is HEX organized as (RED)(GREEN)(BLUE)
            if (value.StartsWith("#"))
            {
                int clrn = Convert.ToInt32(value.Substring(1), 16);
                red = Convert.ToByte((clrn >> 16) & 255);
                green = Convert.ToByte((clrn >> 8) & 255);
                blue = Convert.ToByte(clrn & 255);
            }
            else // otherwise clrs is DECIMAL organized as (BlUE)(GREEN)(RED)
            {
                int clrn = Convert.ToInt32(value);
                red = Convert.ToByte(clrn & 255);
                green = Convert.ToByte((clrn >> 8) & 255);
                blue = Convert.ToByte((clrn >> 16) & 255);
            }
            return Color.FromArgb(255, red, green, blue);
        }
    }

    public class NixxisColor : INotifyPropertyChanged
    {
        #region Class data
        private Color m_Color;
        private string m_Description;
        #endregion

        #region Constructors
        public NixxisColor()
        {
        }
        public NixxisColor(Color color, string description)
        {
            m_Color = color;
            m_Description = description;
        }
        public NixxisColor(string htmlColor, string description)
        {
            m_Color = (Color)ColorConverter.ConvertFromString(htmlColor);
            m_Description = description;
        }
        #endregion

        #region Propeties
        public Color Color
        {
            get { return m_Color; }
            set { m_Color = value; FirePropertyChanged("Color"); }
        }
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; FirePropertyChanged("Description"); }
        }
        
        #endregion

        #region Members
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
