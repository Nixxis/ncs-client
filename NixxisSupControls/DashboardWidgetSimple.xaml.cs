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
    [BrowsableProperty("Palette", true)]
    [BrowsableProperty("Levels", true)]
    [BrowsableProperty("FlashLevel", true)]
    [BrowsableProperty("Format", true)]    
    public partial class DashboardWidgetSimple : DashboardWidgetBase
    {
        private int m_FlashLevel = -1;
        private ObservableCollection<int> m_Levels;
        private string m_Format = null;
        public DashboardWidgetSimple()
        {
            InitializeComponent();
            Levels = new ObservableCollection<int>();
            Palette.Name = DashboardWidgetPalette.DefaultRadialPaletteName;
        }

        public override Size SetSize(Size size)
        {
            return new Size(1,1);
        }

        public override string WidgetName
        {
            get
            {
                return "Value with alert";
            }
        }

        [Description("Indicates the levels triggering the color changes")]
        [Category("Behavior")]
        public ObservableCollection<int> Levels
        {
            get
            {
                return m_Levels;
            }
            set
            {
                if(m_Levels!=null)
                    m_Levels.CollectionChanged -= m_Levels_CollectionChanged;

                m_Levels = value;

                if (m_Levels != null)
                    m_Levels.CollectionChanged += m_Levels_CollectionChanged;

                FirePropertyChanged("Levels");
            }
        }

        [Description("Indicates the level causing the widget to flash")]
        [Category("Behavior")]
        public int FlashLevel
        {
            get
            {
                return m_FlashLevel;
            }
            set
            {
                m_FlashLevel = value;
                FirePropertyChanged("FlashLevel");
            }
        }
        
        [Description("Allow specifying the format")]
        [Category("Appearance")]
        public string Format
        {
            get
            {
                return m_Format;
            }
            set
            {
                m_Format = value;
                FirePropertyChanged("Format");
            }
        }

        public override void Timer(int counter)
        {
            if (m_FlashLevel >= 0)
            {
                double val = 0;
                if (Data.Objects.Count > 0)
                {
                    if(Data.Objects[0].Values.Count>0)
                    {
                        val = (double)(new DoubleConverter().Convert(Data.Objects[0].Values[0].Value, typeof(double), null, null));
                        if(val >= m_FlashLevel)
                        {
                            if (colorRectangle.Visibility == System.Windows.Visibility.Visible)
                                colorRectangle.Visibility = System.Windows.Visibility.Collapsed;
                            else
                                colorRectangle.Visibility = System.Windows.Visibility.Visible;
                            return;
                        }
                    }
                }
            }

            if(colorRectangle.Visibility != System.Windows.Visibility.Visible)
                colorRectangle.Visibility = System.Windows.Visibility.Visible;
        }
        void m_Levels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FirePropertyChanged("Levels");
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            base.ReadXml(reader);

            try
            {
                FlashLevel = System.Xml.XmlConvert.ToInt32(reader.ReadElementString("FlashLevel"));

                List<int> strObjectIds = new List<int>();
                if (!reader.IsEmptyElement)
                {
                    reader.ReadStartElement();
                    while (reader.MoveToContent() != System.Xml.XmlNodeType.EndElement)
                    {
                        strObjectIds.Add(System.Xml.XmlConvert.ToInt32(reader.ReadElementString("Value")));
                    }
                    reader.ReadEndElement();
                }
                else
                {
                    reader.Skip();
                }

                Levels = new ObservableCollection<int>(strObjectIds);

                Format = reader.ReadElementString("Format");

            }
            catch
            {

            }

            reader.ReadEndElement();
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteElementString("FlashLevel", System.Xml.XmlConvert.ToString(FlashLevel));
            writer.WriteStartElement("Levels");
            foreach (int str in Levels)
                writer.WriteElementString("Value", System.Xml.XmlConvert.ToString(str));
            writer.WriteEndElement();
            writer.WriteElementString("Format", Format);
        }

    }

    public class LevelsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] == null)
                return null;

            int countColors = (int)(values[0].GetType().GetProperty("Count").GetGetMethod().Invoke(values[0], new object[] { }));
            if (countColors == 0)
                return null;

            int countLevels = (int)(values[1].GetType().GetProperty("Count").GetGetMethod().Invoke(values[1], new object[] { }));
            if (countLevels == 0)
                return null;

            if (values[2] == null || DependencyProperty.UnsetValue.Equals(values[2]))
                return null;

            double value = System.Convert.ToDouble(values[2]);

            for(int i=0; i<countLevels; i++)
            {
                int currentLevel = (int)(values[1].GetType().GetProperty("Item").GetGetMethod().Invoke(values[1], new object[] { i}));
                int next = int.MaxValue;
                if(i+1 < countLevels)
                    next = (int)(values[1].GetType().GetProperty("Item").GetGetMethod().Invoke(values[1], new object[] { (i+1) }));
                if(currentLevel <= value && value < next)
                {
                    if (i >= countColors)
                        i = countColors - 1;
                    return values[0].GetType().GetProperty("Item").GetGetMethod().Invoke(values[0], new object[] { i});
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Provide a base class to derive widgets from. This base class is a <see cref="UserControl"/>, providing methods to load and save widget settings (using <see cref="IXmlSerializable"/>). The class also implement <see cref="INotifyPropertyChanged"/> to ensure XAML bindings react correctly on changes.
    /// </summary>
    [BrowsableProperty("*", false)]
    [BrowsableProperty("Title", true)]
    [BrowsableProperty("DataSource", true)]    
    public class DashboardWidgetBase: UserControl, IDashboardWidget, INotifyPropertyChanged, IXmlSerializable
    {
        private WidgetDisplayMode m_DisplayMode = (WidgetDisplayMode)(-1);
        private string m_Title;
        private string m_CurrentTheme;
        private static int m_Count;
        private bool m_BackgroundVisible = true;

        /// <summary>
        /// Default constructor. This constructor initialize <see cref="Title"/> with the value of <see cref="WidgetName"/> followed by a counter to ensure uniquenes. It also initialize <see cref="SampleDataSource"/> with predefined data samples.
        /// </summary>
        public DashboardWidgetBase()
        {
            m_Count++;
            m_Title = string.Concat(WidgetName + " " + m_Count);
            DataContextChanged += DashboardWidgetBase_DataContextChanged;
            DataSource = new DashboardWidgetDataSource();
            SampleDataSource = new DashboardWidgetDataSource();
            SampleDataSource.DataContext = new Samples();
            SampleDataSource.ObjectType = "Data";
            SampleDataSource.ObjectIds = new System.Collections.ObjectModel.ObservableCollection<string>();
            SampleDataSource.ObjectProperties = new System.Collections.ObjectModel.ObservableCollection<string>(new string[]{"Numeric1", "Numeric2", "Numeric3"});
            Palette = new DashboardWidgetPalette(this);
        }

        private void DashboardWidgetBase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DataSource.DataContext = DataContext;
        }

        /// <inheritdoc />
        public virtual WidgetDisplayMode DisplayMode
        {
            get
            {
                return m_DisplayMode;
            }
            set
            {
                if (m_DisplayMode != value)
                {
                    m_DisplayMode = value;
                    FirePropertyChanged("Data");
                    FirePropertyChanged("DisplayMode");
                }
            }
        }
        
        /// <summary>
        /// Helper method generating a change notification.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected virtual void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// <see cref="PropertyChangedEventHandler"/> event related to the <see cref="INotifyPropertyChanged"/> interface implementation.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        [Description("The widget's title")]
        [Category("Widget base")]
        public virtual string Title
        {
            get
            {
                return m_Title;
            }
            set
            {
                m_Title = value;
                FirePropertyChanged("Title");
            }
        }

        /// <inheritdoc />
        public virtual Visibility TitleVisibility
        {
            get
            {
                return Visibility.Visible;
            }
        }

        [Category("Appearance")]
        [Description("Indicates if the widget's background must be displayed")]
        public virtual bool BackgroundVisible
        {
            get
            {
                return m_BackgroundVisible;
            }
            set
            {
                m_BackgroundVisible = value;
                FirePropertyChanged("BackgroundVisible");
            }
        }

        /// <inheritdoc/>
        public virtual Size SetSize(Size size)
        {
            return size;
        }

        /// <inheritdoc />
        public virtual string WidgetName { get; set; }

        /// <inheritdoc />
        public virtual void Timer(int count)
        {

        }


        /// <summary>
        /// The <see cref="DashboardWidgetDataSource"/> instance used as supervision datasource for the widget. This instance is supposed to be pointing to real supervision data.
        /// </summary>
        [Description("The widget's datasource")]
        [Category("Widget base")]
        public DashboardWidgetDataSource DataSource { get; set; }

        /// <summary>
        /// The <see cref="DashboardWidgetDataSource"/> instance used as sample datasource for the widget. The sample datasource purpose is to provide a predefined data example that can be used in <see cref="WidgetDisplayMode.Icon"/> or <see cref="WidgetDisplayMode.Design"/> modes.  
        /// </summary>
        public DashboardWidgetDataSource SampleDataSource { get; set; }

        /// <summary>
        /// The <see cref="DashboardWidgetPalette"/> instance used to select the colors and brushes available for the widget.
        /// </summary>
        [Category("Appearance")]
        public DashboardWidgetPalette Palette { get; set; }

        /// <inheritdoc />
        public virtual string CurrentTheme
        {
            get 
            {
                return m_CurrentTheme; 
            }
            set
            {
                m_CurrentTheme = value;
                FirePropertyChanged("CurrentTheme");
            }
        }

        /// <summary>
        /// The <see cref="DashboardWidgetDataSource"/> instance used a datasource for the widget. Depending on the <see cref="DisplayMode"/>, the returned value is pointing to <see cref="DataSource"/> or <see cref="SampleDataSource"/>.
        /// </summary>
        public DashboardWidgetDataSource Data
        {
            get
            {
                switch (DisplayMode)
                {
                    case WidgetDisplayMode.Icon:
                        return SampleDataSource;

                    case WidgetDisplayMode.Design:
                    case WidgetDisplayMode.Run:
                        return DataSource;

                }

                return null;
            }
        }

        /// <summary>
        /// Implementation of <see cref="IXmlSerializable"/> to allow the widget configuration to be saved and loaded. 
        /// </summary>
        /// <returns>The <see cref="System.Xml.Schema.XmlSchema"/> corresponding to the palette.</returns>
        public virtual System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Implementation of <see cref="IXmlSerializable"/> to allow the widget configuration to be saved and loaded. 
        /// </summary>
        /// <param name="reader"></param>
        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();
            reader.ReadStartElement();
            Title = reader.ReadElementString("Title");

            XmlSerializer serializer = new XmlSerializer(DataSource.GetType());
            DataSource = (DashboardWidgetDataSource)serializer.Deserialize(reader);

            Palette.ReadXml(reader);

            if (!reader.IsEmptyElement)
            {
                if (!reader.IsStartElement())
                    reader.ReadEndElement();
            }

        }

        /// <summary>
        /// Implementation of <see cref="IXmlSerializable"/> to allow the widget configuration to be saved and loaded. 
        /// </summary>
        /// <param name="writer"></param>
        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteElementString("Title", Title);
            XmlSerializer serializer = new XmlSerializer(DataSource.GetType());            
            serializer.Serialize(writer, DataSource);

            writer.WriteStartElement("Palette");
            Palette.WriteXml(writer);
            writer.WriteEndElement();
        }
    }

    public class Samples
    {
        public List<Sample> Data {get;set;}

        public Samples()
        {
            Data = new List<Sample>();
            Data.Add(new Sample() { Id = "0", Description = "Sample0", Numeric1 = 10, Numeric2 = 13, Numeric3 = 5 });
            Data.Add(new Sample() { Id = "1", Description = "Sample1", Numeric1 = 1, Numeric2 = 3, Numeric3 = 13 });
            Data.Add(new Sample() { Id = "2", Description = "Sample2", Numeric1 = 15, Numeric2 = 23, Numeric3 = 9 });
        }
    }

    public class Sample
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public int Numeric1 { get; set; }
        public int Numeric2 { get; set; }
        public int Numeric3 { get; set; }

    }

}
