using Nixxis.ClientV2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls.WpfPropertyGrid;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Nixxis.Client.Supervisor
{

    /// <summary>
    /// Class describing a configurable palette of brushes, based on palettes defined in themes or made of colors specified by the user.  
    /// </summary>
    /// <conceptualLink target="b2064519-4e6a-4473-9588-7e890ffc1911"/>
    public class DashboardWidgetPalette: DependencyObject, IXmlSerializable
    {
        /// <summary>
        /// Defines the key identifying the default horizontal palette in themes. This value is also the fallback when a reference is made to inexisting palette (for example when a new theme is chosen but that theme does not define the previously sleected palette).
        /// </summary>
        public static string DefaultHorizontalPaletteName = "DefaultHorizontalPalette";
        /// <summary>
        /// Defines the key identifying the default radial palette in themes. 
        /// </summary>
        public static string DefaultRadialPaletteName = "DefaultRadialPalette";

        /// <summary>
        /// Identifies the <see cref="Name"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(DashboardWidgetPalette), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(NamePropertyChanged)));
        /// <summary>
        /// Identifies the <see cref="Order"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrderProperty = DependencyProperty.Register("Order", typeof(ObservableCollection<int>), typeof(DashboardWidgetPalette), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(OrderPropertyChanged)));
        /// <summary>
        /// Identifies the <see cref="Colors"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorsProperty = DependencyProperty.Register("Colors", typeof(ObservableCollection<Color>), typeof(DashboardWidgetPalette), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(ColorsPropertyChanged)));
        /// <summary>
        /// Identifies the <see cref="Brushes"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BrushesProperty = DependencyProperty.Register("Brushes", typeof(ObservableCollection<Brush>), typeof(DashboardWidgetPalette), null);


        private static void NamePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetPalette palette = obj as DashboardWidgetPalette;

            palette.EvaluateBrushes();
        }
        private static void OrderPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetPalette palette = obj as DashboardWidgetPalette;

            if (args.OldValue != null)
                (args.OldValue as INotifyCollectionChanged).CollectionChanged -= palette.OrderCollectionChanged;

            if(args.NewValue!=null)           
                (args.NewValue as INotifyCollectionChanged).CollectionChanged += palette.OrderCollectionChanged;

            palette.EvaluateBrushes();
        }
        private static void ColorsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DashboardWidgetPalette palette = obj as DashboardWidgetPalette;

            if (args.OldValue != null)
                (args.OldValue as INotifyCollectionChanged).CollectionChanged -= palette.ColorsCollectionChanged;

            if (args.NewValue != null)
                (args.NewValue as INotifyCollectionChanged).CollectionChanged += palette.ColorsCollectionChanged;

            palette.EvaluateBrushes();
        }

        private void OrderCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            EvaluateBrushes();
        }
        private void ColorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            EvaluateBrushes();
        }
        private void EvaluateBrushes()
        {
            ObservableCollection<Brush> temp = new ObservableCollection<Brush>();

            if (string.IsNullOrEmpty(Name))
            {
                if (Colors != null)
                {
                    foreach (Brush b in Colors.Select((c) => (new SolidColorBrush(c))))
                        temp.Add(b);
                }
            }
            else
            {
                if (m_Owner.Parent != null)
                {                    
                    Brush[] palette = m_Owner.TryFindResource(Name) as Brush[];
                    if (palette == null)
                    {
                        Name = DefaultHorizontalPaletteName;
                        palette = m_Owner.FindResource(Name) as Brush[];
                    }

                    if (Order == null || Order.Count==0)
                    {
                        foreach (Brush b in palette)
                            temp.Add(b);
                    }
                    else
                    {
                        foreach (int i in Order)
                            temp.Add(palette[i]);
                    }
                }
            }
            Brushes = temp;
        }

        /// <summary>
        /// Get or sets the name of the theme's pallete related to this instance.
        /// </summary>
        /// <value>Palette name identifying a palette defined in a theme.</value>
        /// <remarks>
        /// Related to <see cref="NameProperty"/> dependency property.
        /// </remarks>
        public string Name
        {
            get
            {
                return (string)GetValue(NameProperty);
            }
            set
            {
                SetValue(NameProperty, value);
            }
        }
        /// <summary>
        /// Get or sets the collection of indexes specifying the order in which <see cref="Colors"/> must be evaluated when generating <see cref="Brushes"/>.
        /// </summary>
        /// <value>Collection of indexes specifying the order of colors.</value>
        /// <remarks>
        /// Related to <see cref="OrderProperty"/> dependency property.
        /// </remarks>
        public ObservableCollection<int> Order
        {
            get
            {
                return (ObservableCollection<int>)GetValue(OrderProperty);
            }
            set
            {
                SetValue(OrderProperty, value);
            }
        }
        /// <summary>
        /// Get or sets the collection of <see cref="System.Windows.Media.Color"/> defining the colors of this palette.
        /// </summary>
        /// <value>Colors collection defining the palette.</value>
        /// <remarks>
        /// Related to <see cref="ColorsProperty"/> dependency property.
        /// </remarks>
        public ObservableCollection<Color> Colors
        {
            get
            {
                return (ObservableCollection<Color>)GetValue(ColorsProperty);
            }
            set
            {
                SetValue(ColorsProperty, value);
            }
        }
        /// <summary>
        /// Get or sets the collection of resulting <see cref="System.Windows.Media.Brush"/> according to <see cref="Name"/>, <see cref="Order"/> and <see cref="Colors"/>.
        /// </summary>
        /// <value>Collection of <see cref="System.Windows.Media.Brush"/> related to the palette.</value>
        /// <remarks>
        /// Related to <see cref="BrushesProperty"/> dependency property.
        /// </remarks>
        public ObservableCollection<Brush> Brushes
        {
            get
            {
                return (ObservableCollection<Brush>)GetValue(BrushesProperty);
            }
            set
            {
                SetValue(BrushesProperty, value);
            }
        }

        private FrameworkElement m_Owner;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="owner">The <see cref="FrameworkElement"/> instance owning the current instance. This is parameter is usually a widget control. This parameter enables the <see cref="Nixxis.Client.Supervisor.DashboardWidgetPalette"/> object to access the widget's resources, including palettes defined in themes.</param>
        public DashboardWidgetPalette(FrameworkElement owner): base()
        {
            m_Owner = owner;

            m_Owner.Loaded += m_Owner_Loaded;
            
            Brushes = new ObservableCollection<Brush>();

            Name = DefaultHorizontalPaletteName;
        }
        private void m_Owner_Loaded(object sender, RoutedEventArgs e)
        {
            EvaluateBrushes();
        }

        /// <summary>
        /// Implementation of <see cref="IXmlSerializable"/>. This is needed in order to be able to save and load palette configuration.
        /// </summary>
        /// <returns>The <see cref="System.Xml.Schema.XmlSchema"/> corresponding to the palette.</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Implementation of <see cref="IXmlSerializable"/>. This is needed in order to be able to save and load palette configuration.
        /// Called by the dashboard framework to load the palette settings.
        /// </summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> instance needed to load the palette configuration.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {

            reader.MoveToContent();
            Name = reader.GetAttribute("Name");

            reader.ReadStartElement();

            List<int> lstOrders = new List<int>();

            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement();
                while (reader.MoveToContent() != System.Xml.XmlNodeType.EndElement)
                {
                    lstOrders.Add(System.Xml.XmlConvert.ToInt32( reader.ReadElementString("Value")));
                }
                reader.ReadEndElement();
            }
            else
            {
                reader.Skip();
            }

            Order = new ObservableCollection<int>(lstOrders);


            List<Color> lstColors = new List<Color>();

            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement();
                while (reader.MoveToContent() != System.Xml.XmlNodeType.EndElement)
                {
                    lstColors.Add( Color.FromArgb(
                        System.Xml.XmlConvert.ToByte(reader.GetAttribute("A")),
                        System.Xml.XmlConvert.ToByte(reader.GetAttribute("R")),
                        System.Xml.XmlConvert.ToByte(reader.GetAttribute("G")),
                        System.Xml.XmlConvert.ToByte(reader.GetAttribute("B"))
                        ));
                    reader.Skip();
                }
            }
            else
            {
                reader.Skip();
            }

            Colors = new ObservableCollection<Color>(lstColors);

            reader.ReadEndElement();
        }

        /// <summary>
        /// Implementation of <see cref="IXmlSerializable"/>. This is needed in order to be able to save and load palette configuration.
        /// Called by the dashboard framework to save the palette settings.
        /// </summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> instance needed to save the palette configuration.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            writer.WriteStartElement("Order");
            if (Order != null)
            {
                foreach (int n in Order)
                    writer.WriteElementString("Value", System.Xml.XmlConvert.ToString(n));
            }
            writer.WriteEndElement();
            writer.WriteStartElement("Colors");
            if (Colors != null)
            {
                foreach (Color col in Colors)
                {
                    writer.WriteStartElement("Color");
                    writer.WriteAttributeString("A", System.Xml.XmlConvert.ToString(col.A));
                    writer.WriteAttributeString("R", System.Xml.XmlConvert.ToString(col.R));
                    writer.WriteAttributeString("G", System.Xml.XmlConvert.ToString(col.G));
                    writer.WriteAttributeString("B", System.Xml.XmlConvert.ToString(col.B));
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();

        }
    }

}
