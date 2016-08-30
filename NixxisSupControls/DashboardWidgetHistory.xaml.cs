using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

namespace Nixxis.Client.Supervisor
{
    /// <summary>
    /// Interaction logic for SimpleDashboardControl.xaml
    /// </summary>
    [BrowsableProperty("Palette", true)]
    [BrowsableProperty("HistoryLength", true)]
    [BrowsableProperty("UpdateZoomInterval", true)]
    [BrowsableProperty("RefreshInterval", true)]
    [BrowsableProperty("ShowCurrentValue", true)]
    [BrowsableProperty("Format", true)]    
    public partial class DashboardWidgetHistory : DashboardWidgetBase
    {
        private int m_RefreshInterval = 30;

        private int m_UpdateZoomInterval = 30;

        private bool m_ShowCurrentValue = true;

        private string m_Format = null;


        public DashboardWidgetHistory()
        {
            InitializeComponent();

            #region Generate sample data
            DisplayMode = WidgetDisplayMode.Icon;
            SampleDataSource.Objects[0].Values[0].Value = 5;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 6;
            SampleDataSource.Objects[0].Values[0].Value = 5;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 8;
            SampleDataSource.Objects[0].Values[0].Value = 7;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 9;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 11;
            SampleDataSource.Objects[0].Values[0].Value = 10;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 11;
            SampleDataSource.Objects[0].Values[0].Value = 10;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 9;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 9;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 8;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 6;
            SampleDataSource.Objects[0].Values[0].Value = 5;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 6;
            SampleDataSource.Objects[0].Values[0].Value = 5;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 8;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 8;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 7;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 7;
            SampleDataSource.Objects[0].Values[0].Value = 6;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 6;
            SampleDataSource.Objects[0].Values[0].Value = 5;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 6;
            SampleDataSource.Objects[0].Values[0].Value = 5;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 5;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 2;
            Timer(m_RefreshInterval);
            SampleDataSource.Objects[0].Values[0].Value = 3;
            Timer(m_RefreshInterval);
            (FindResource("historyConverter") as HistoryConverter).Reset();
            #endregion

        }

        public override string WidgetName
        {
            get
            {
                return "History";
            }
        }

        [Description("Number of items kept in history")]
        [Category("Behavior")]
        public int HistoryLength
        {
            get
            {
                return (FindResource("historyConverter") as HistoryConverter).HistoryLength;
            }
            set
            {
                (FindResource("historyConverter") as HistoryConverter).HistoryLength = value;
            }
        }

        public override void Timer(int counter)
        {
            if (counter % RefreshInterval == 0)
            {
                (FindResource("historyConverter") as HistoryConverter).UpdateValues();
            }

            if (counter % UpdateZoomInterval == 0)
            {
                double maxZoom = (double)ExtendedPanel.VerticalZoomProperty.DefaultMetadata.DefaultValue;

                double currentZoom = ExtendedPanel.GetSharedVerticalZoom(MainItemsControl) * 1.5;


                if (currentZoom < maxZoom)
                    ExtendedPanel.SetSharedVerticalZoom(MainItemsControl, currentZoom);
                else
                    ExtendedPanel.SetSharedVerticalZoom(MainItemsControl, maxZoom);
            }
        }

        [Description("Number of seconds between two refreshes")]
        [Category("Behavior")]
        public int RefreshInterval
        {
            get
            {
                return m_RefreshInterval;
            }
            set
            {
                m_RefreshInterval = value;
                FirePropertyChanged("RefreshInterval");
            }
        }

        [Description("Interval in seconds before zooming in")]
        [Category("Behavior")]
        public int UpdateZoomInterval
        {
            get
            {
                return m_UpdateZoomInterval;
            }
            set
            {
                m_UpdateZoomInterval = value;
                FirePropertyChanged("UpdateZoomInterval");
            }
        }

        [Description("Indicates if current value is also dispalyed as text")]
        [Category("Appearance")]
        public bool ShowCurrentValue
        {
            get
            {
                return m_ShowCurrentValue;
            }
            set
            {
                m_ShowCurrentValue = value;
                FirePropertyChanged("ShowCurrentValue");
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


        public override void ReadXml(System.Xml.XmlReader reader)
        {

            base.ReadXml(reader);

            try
            {
                RefreshInterval = System.Xml.XmlConvert.ToInt32(reader.ReadElementString("RefreshInterval"));
                HistoryLength = System.Xml.XmlConvert.ToInt32(reader.ReadElementString("HistoryLength"));
                UpdateZoomInterval = System.Xml.XmlConvert.ToInt32(reader.ReadElementString("UpdateZoomInterval"));
                ShowCurrentValue = System.Xml.XmlConvert.ToBoolean(reader.ReadElementString("ShowCurrentValue"));
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

            writer.WriteElementString("RefreshInterval", System.Xml.XmlConvert.ToString(RefreshInterval));
            writer.WriteElementString("HistoryLength", System.Xml.XmlConvert.ToString(HistoryLength));
            writer.WriteElementString("UpdateZoomInterval", System.Xml.XmlConvert.ToString(UpdateZoomInterval));
            writer.WriteElementString("ShowCurrentValue", System.Xml.XmlConvert.ToString(ShowCurrentValue));
            writer.WriteElementString("Format", Format);

        }

    
    }

    public class HistoryConverter: IValueConverter
    {
        private ObservableCollection<MinMaxValue> m_Values = null;


        public int HistoryLength
        {
            get
            {
                return m_Values.Count;
            }
            set
            {
                while (m_Values.Count() > value)
                    m_Values.RemoveAt(0);

                for (int i = m_Values.Count; i < value; i++)
                    m_Values.Insert(0, new MinMaxValue());
            }
        }

        public void UpdateValues()
        {
            for(int i=1; i<m_Values.Count; i++)
            {
                m_Values[i - 1] = m_Values[i];
            }

            MinMaxValue val = new MinMaxValue( m_Values[m_Values.Count - 1].Value);

            m_Values[m_Values.Count - 1] = val;
        }

        public void Reset()
        {
            m_Values = new ObservableCollection<MinMaxValue>();
            for (int i = 0; i < 15; i++)
                m_Values.Add(new MinMaxValue());
        }

        public HistoryConverter()
        {
            Reset();
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double v = System.Convert.ToDouble(new DoubleConverter().Convert(value, typeof(double), null, null));

            m_Values[m_Values.Count - 1].Value = v;

            return m_Values;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public class MinMaxValue: INotifyPropertyChanged
        {
            private double m_Maximum = 0;
            private double m_Minimum = 0;
            private double m_Value = 0;

            public double Maximum
            {
                get
                {
                    return m_Maximum;
                }
                set
                {
                    m_Maximum = value;
                    FirePropertyChanged("Maximum");
                    FirePropertyChanged("MaximumDeviation");
                }
            }

            public double MaximumDeviation
            {
                get
                {
                    return m_Maximum - m_Value;
                }
            }

            public double ValueDeviation
            {
                get
                {
                    return m_Value - m_Minimum;
                }
            }
 
            public double Minimum
            {
                get
                {
                    return m_Minimum; ;
                }
                set
                {
                    m_Minimum = value;
                    FirePropertyChanged("Minimum");
                    FirePropertyChanged("ValueDeviation");
                }
            }

            public double Value
            {
                get
                {
                    return m_Value;
                }
                set
                {
                    m_Value = value;
                    if (Maximum < value)
                        Maximum = value;
                    if (Minimum > value)
                        Minimum = value;
                    FirePropertyChanged("Value");
                    FirePropertyChanged("ValueDeviation");
                    FirePropertyChanged("MaximumDeviation");
                }
            }

            public MinMaxValue(double val)
            {
                Value = val;
                Maximum = val;
                Minimum = val;
            }

            public MinMaxValue()
            {

            }

            private void FirePropertyChanged(string propName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }

            public event PropertyChangedEventHandler PropertyChanged;

        }
    }    

}
