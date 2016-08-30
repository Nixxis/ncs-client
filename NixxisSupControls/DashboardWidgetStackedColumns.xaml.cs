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
    [BrowsableProperty("Palette", true)]
    [BrowsableProperty("ShowLabels", true)]
    [BrowsableProperty("ShowValues", true)]
    [BrowsableProperty("Stack100Percent", true)]
    [BrowsableProperty("SwapAxis", true)]
    [BrowsableProperty("UpdateZoomInterval", true)]
    [BrowsableProperty("Format", true)]    
    public partial class DashboardWidgetStackedColumns : DashboardWidgetBase
    {

        private bool m_ShowLabels = true;

        private bool m_ShowValues = true;

        private bool m_Stack100Percent = false;

        private string m_Format = null;

        private bool m_SwapAxis = false;

        private int m_UpdateZoomInterval = 30;

        public DashboardWidgetStackedColumns()
        {
            InitializeComponent();
        }

        public override Size SetSize(Size size)
        {
            if (!Stack100Percent)
            {
                double maxZoom = (double)ExtendedPanel.VerticalZoomProperty.DefaultMetadata.DefaultValue;
                ExtendedPanel.SetSharedVerticalZoom(MainItemsControl, maxZoom);
            }

            return base.SetSize(size);
        }
        public override string WidgetName
        {
            get
            {
                return "Stacked columns";
            }
        }

        [Description("Indicates if labels are included")]
        [Category("Appearance")]
        public bool ShowLabels
        {
            get
            {
                return m_ShowLabels;
            }
            set
            {
                m_ShowLabels = value;
                FirePropertyChanged("ShowLabels");
            }
        }

        [Description("Indicates if values are also dispalyed as text")]
        [Category("Appearance")]
        public bool ShowValues
        {
            get
            {
                return m_ShowValues;
            }
            set
            {
                m_ShowValues = value;
                FirePropertyChanged("ShowValues");
            }
        }

        [Description("Indicates if columns are stacked to 100%")]
        [Category("Behavior")]
        public bool Stack100Percent
        {
            get
            {
                return m_Stack100Percent;
            }
            set
            {
                m_Stack100Percent = value;
                FirePropertyChanged("Stack100Percent");
            }
        }

        [Description("Indicates if axis must be swapped")]
        [Category("Behavior")]
        public bool SwapAxis
        {
            get
            {
                return m_SwapAxis;
            }
            set
            {
                m_SwapAxis = value;
                FirePropertyChanged("SwapAxis");
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


        public override void Timer(int counter)
        {
            if (counter % UpdateZoomInterval == 0)
            {
                if (!Stack100Percent)
                {
                    double maxZoom = (double)ExtendedPanel.VerticalZoomProperty.DefaultMetadata.DefaultValue;

                    double currentZoom = ExtendedPanel.GetSharedVerticalZoom(MainItemsControl) * 1.5;


                    if (currentZoom < maxZoom)
                        ExtendedPanel.SetSharedVerticalZoom(MainItemsControl, currentZoom);
                    else
                        ExtendedPanel.SetSharedVerticalZoom(MainItemsControl, maxZoom);
                }
            }
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {

            base.ReadXml(reader);

            try
            {
                ShowLabels = System.Xml.XmlConvert.ToBoolean(reader.ReadElementString("ShowLabels"));
                ShowValues = System.Xml.XmlConvert.ToBoolean(reader.ReadElementString("ShowValues"));
                Stack100Percent = System.Xml.XmlConvert.ToBoolean(reader.ReadElementString("Stack100Percent"));
                SwapAxis = System.Xml.XmlConvert.ToBoolean(reader.ReadElementString("SwapAxis"));
                UpdateZoomInterval = System.Xml.XmlConvert.ToInt32(reader.ReadElementString("UpdateZoomInterval"));
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

            writer.WriteElementString("ShowLabels", System.Xml.XmlConvert.ToString(ShowLabels));
            writer.WriteElementString("ShowValues", System.Xml.XmlConvert.ToString(ShowValues));
            writer.WriteElementString("Stack100Percent", System.Xml.XmlConvert.ToString(Stack100Percent));
            writer.WriteElementString("SwapAxis", System.Xml.XmlConvert.ToString(SwapAxis));
            writer.WriteElementString("UpdateZoomInterval", System.Xml.XmlConvert.ToString(UpdateZoomInterval));
            writer.WriteElementString("Format", Format);
        }


    }



}
