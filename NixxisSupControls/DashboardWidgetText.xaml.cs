using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.WpfPropertyGrid;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Nixxis.Client.Supervisor
{
    [BrowsableProperty("DataSource", false)]
    [BrowsableProperty("Stretch", true)]
    [BrowsableProperty("HorizontalAlignment", true)]
    [BrowsableProperty("VerticalAlignment", true)]
    [BrowsableProperty("FontStyle", true)]
    [BrowsableProperty("FontWeight", true)]
    [BrowsableProperty("FontSize", true)]
    [BrowsableProperty("Underlined", true)]
    [BrowsableProperty("BackgroundVisible", true)]
    [BrowsableProperty("ForcedTextColor", true)]
    public partial class DashboardWidgetText : DashboardWidgetBase
    {
        private System.Windows.Media.Stretch m_Stretch = Stretch.Uniform;
        private VerticalAlignment m_VerticalAlignment = VerticalAlignment.Center;
        private HorizontalAlignment m_HorizontalAlignment = HorizontalAlignment.Center;
        private FontStyle m_FontStyle = FontStyles.Normal;
        private FontWeight m_FontWeight = FontWeights.Normal;
        private double m_FontSize = 8;
        private bool m_Underlined = false;
        private Brush m_ForcedTextColor;
        
        public DashboardWidgetText()
        {
            InitializeComponent();
            Palette.Name = DashboardWidgetPalette.DefaultRadialPaletteName;
        }

        public override string WidgetName
        {
            get
            {
                return "Text";
            }
        }

        public override Visibility TitleVisibility
        {
            get
            {
                return Visibility.Collapsed;
            }
        }

        [MultiLine(3,3,true)]
        public override string Title
        {
            get
            {
                return base.Title;
            }
            set
            {
                base.Title = value;
            }
        }

        [Description("Allow specifying how the text fills the available space")]
        [Category("Alignment")]
        [PropertyOrder(0)]
        public System.Windows.Media.Stretch Stretch
        {
            get
            {
                return m_Stretch;
            }
            set
            {
                m_Stretch = value;
                FirePropertyChanged("Stretch");
            }
        }

        [Description("Allow specifying how the text is vertically aligned")]
        [Category("Alignment")]
        [PropertyOrder(2)]
        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return m_VerticalAlignment;
            }
            set
            {
                m_VerticalAlignment = value;
                FirePropertyChanged("VerticalAlignment");

            }
        }

        [Description("Allow specifying how the text is horizontally aligned")]
        [Category("Alignment")]
        [PropertyOrder(1)]
        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return m_HorizontalAlignment;
            }
            set
            {
                m_HorizontalAlignment = value;
                FirePropertyChanged("HorizontalAlignment");
            }
        }

        [Description("Allow to specify text color (override theme)")]
        [Category("Font")]
        [PropertyOrder(0)]
        public Brush ForcedTextColor
        {
            get
            {
                return m_ForcedTextColor;
            }
            set
            {
                m_ForcedTextColor = value;
                FirePropertyChanged("ForcedTextColor");
            }
        }


        [Description("Allow specifying the font style")]
        [Category("Font")]
        [PropertyOrder(1)]
        public FontStyle FontStyle
        {
            get
            {
                return m_FontStyle;
            }
            set
            {
                m_FontStyle = value;
                FirePropertyChanged("FontStyle");
            }
        }

        [Description("Allow specifying the font weight")]
        [Category("Font")]
        [PropertyOrder(2)]
        public FontWeight FontWeight
        {
            get
            {
                return m_FontWeight;
            }
            set
            {
                m_FontWeight = value;
                FirePropertyChanged("FontWeight");
            }
        }

        [Description("Allow specifying the font size")]
        [Category("Font")]
        [PropertyOrder(3)]
        public double FontSize
        {
            get
            {
                return m_FontSize;
            }
            set
            {
                m_FontSize = value;
                FirePropertyChanged("FontSize");
            }
        }

        [Description("Specifies if the text must be underlined")]
        [Category("Font")]
        [PropertyOrder(4)]
        public bool Underlined
        {
            get
            {
                return m_Underlined;
            }
            set
            {
                m_Underlined = value;
                FirePropertyChanged("Underlined");
            }
        }

    }

    public class ConditionalSelectorConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object valToCompare = values[0];

            if(valToCompare == null || valToCompare==DependencyProperty.UnsetValue)
            {
                if (parameter == null)
                    return values[1];
                else
                    return values[2];
            }
            else
            {
                if (valToCompare.Equals(parameter))
                    return values[1];
                else
                    return values[2];
            }
           
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TextDecorationConverter: IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return TextDecorations.Underline;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsDefinedConverter: IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
