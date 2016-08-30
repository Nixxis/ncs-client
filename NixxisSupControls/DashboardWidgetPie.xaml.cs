using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    [Flags]
    public enum LabelsContent
    {
        None = 0,
        Description = 1,
        Value = 2,
        Percentage = 4
    }


    [BrowsableProperty("Palette", true)]
    [BrowsableProperty("SwapAxis", true)]
    [BrowsableProperty("ShowDescription", true)]
    [BrowsableProperty("ShowValue", true)]
    [BrowsableProperty("ShowPercentage", true)]
    [BrowsableProperty("KeepAspectRatio", true)]
    [BrowsableProperty("Format", true)]    
    public partial class DashboardWidgetPie : DashboardWidgetBase
    {
        private bool m_SwapAxis = false;

        private bool m_KeepAspectRatio = true;

        private string m_Format = null;

        private LabelsContent m_LabelsContent = LabelsContent.None;

        public DashboardWidgetPie()
        {
            InitializeComponent();
            Palette.Name = DashboardWidgetPalette.DefaultRadialPaletteName;
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

        [Description("Indicates if the graph aspect ratio must be kept")]
        [Category("Appearance")]
        public bool KeepAspectRatio
        {
            get
            {
                return m_KeepAspectRatio;
            }
            set
            {
                m_KeepAspectRatio = value;
                FirePropertyChanged("KeepAspectRatio");
            }
        }

        public LabelsContent LabelsContent
        {
            get
            {
                return m_LabelsContent;
            }
            set
            {
                m_LabelsContent = value;
                FirePropertyChanged("LabelsContent");
                FirePropertyChanged("ShowDescription");
                FirePropertyChanged("ShowValue");
                FirePropertyChanged("ShowPercentage");
            }
        }

        [Description("Indicates if labels include descriptions")]
        [Category("Appearance")]
        public bool ShowDescription
        {
            get
            {
                return (LabelsContent & Supervisor.LabelsContent.Description) == Supervisor.LabelsContent.Description;
            }
            set
            {
                if (value)
                    LabelsContent |= Supervisor.LabelsContent.Description;
                else
                    LabelsContent &= ~Supervisor.LabelsContent.Description;
            }
        }

        [Description("Indicates if labels include values")]
        [Category("Appearance")]
        public bool ShowValue
        {
            get
            {
                return (LabelsContent & Supervisor.LabelsContent.Value) == Supervisor.LabelsContent.Value;
            }
            set
            {
                if (value)
                    LabelsContent |= Supervisor.LabelsContent.Value;
                else
                    LabelsContent &= ~Supervisor.LabelsContent.Value;

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


        [Description("Indicates if labels include percentages")]
        [Category("Appearance")]
        public bool ShowPercentage
        {
            get
            {
                return (LabelsContent & Supervisor.LabelsContent.Percentage) == Supervisor.LabelsContent.Percentage;
            }
            set
            {
                if (value)
                    LabelsContent |= Supervisor.LabelsContent.Percentage;
                else
                    LabelsContent &= ~Supervisor.LabelsContent.Percentage;

            }
        }

        public override string WidgetName
        {
            get
            {
                return "Pie";
            }
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {

            base.ReadXml(reader);

            try
            {
                SwapAxis = System.Xml.XmlConvert.ToBoolean(reader.ReadElementString("SwapAxis"));
                KeepAspectRatio = System.Xml.XmlConvert.ToBoolean(reader.ReadElementString("KeepAspectRatio"));
                LabelsContent = (LabelsContent)System.Xml.XmlConvert.ToInt32(reader.ReadElementString("LabelsContent"));
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

            writer.WriteElementString("SwapAxis", System.Xml.XmlConvert.ToString(SwapAxis));
            writer.WriteElementString("KeepAspectRatio", System.Xml.XmlConvert.ToString(KeepAspectRatio));
            writer.WriteElementString("LabelsContent", System.Xml.XmlConvert.ToString((Int32)LabelsContent));
            writer.WriteElementString("Format", Format);
        }
    }

    public class ProgressToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                return System.Convert.ToDouble(value) * 360;
            else
                return System.Convert.ToDouble(value) * 2 * Math.PI;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CoordinateConverter : IMultiValueConverter, IValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(bool))
            {
                if ((System.Convert.ToDouble(values[0])) > 0.5)
                    return true;
                else
                    return false;
            }
            else
            {
                double angle = (double)(new ProgressToAngleConverter()).Convert(values[0], typeof(double), "rad", null);
                Size sz;
                if (values[1] is Size)
                {
                    sz = (Size)(values[1]);
                }
                else
                {
                    sz = new Size(System.Convert.ToDouble(values[1]), System.Convert.ToDouble(values[2]));
                }

                if ("X".Equals(parameter as string))
                {
                    return Math.Cos(angle) * (sz.Width * .75) + sz.Width - System.Convert.ToDouble(values[3]) / 2.0;
                }
                if ("Y".Equals(parameter as string))
                {
                    return Math.Sin(angle) * (sz.Height * .75) + sz.Height - System.Convert.ToDouble(values[4]) / 2.0;
                }

                return new Point(Math.Cos(angle) * sz.Width + sz.Width, Math.Sin(angle) * sz.Height + sz.Height);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                double dbl = System.Convert.ToDouble(value);

                if (dbl <= 0.0)
                    return Visibility.Collapsed;

                if (parameter != null)
                {
                    if (dbl >= 1.0)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
                else
                {
                    if (dbl >= 1.0)
                        return Visibility.Collapsed;
                    else
                        return Visibility.Visible;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PieHelperVisibilityConverter: IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CumulativeConverter.PieHelper ph = value as CumulativeConverter.PieHelper;
            if(ph!=null)
            {
                if (double.IsNaN(ph.Start) || double.IsNaN(ph.End))
                    return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CumulativeConverter : IValueConverter, IMultiValueConverter
    {
        public class PieHelper : INotifyPropertyChanged
        {
            private double m_Start;
            private double m_End;
            private object m_Value;
            private string m_Description;

            public double Start
            {
                get
                {
                    return m_Start;
                }
                set
                {
                    m_Start = value;
                    FirePropertyChanged("Start");
                    FirePropertyChanged("Middle");
                    FirePropertyChanged("Width");
                    FirePropertyChanged("Percentage");

                }
            }
            public double End
            {
                get
                {
                    return m_End;
                }
                set
                {
                    m_End = value;
                    FirePropertyChanged("End");
                    FirePropertyChanged("Middle");
                    FirePropertyChanged("Width");
                    FirePropertyChanged("Percentage");
                }
            }
            public double Middle
            {
                get
                {
                    return (Start + End) / 2.0;
                }
            }
            public double Width
            {
                get
                {
                    return End - Start;
                }
            }
            public string Percentage
            {
                get
                {
                    return string.Format("{0:0.0}%", Width * 100.0);
                }
            }
            public object Value
            {
                get
                {
                    return m_Value;
                }
                set
                {
                    m_Value = value;
                    FirePropertyChanged("Value");
                }
            }

            public string Description
            {
                get
                {
                    return m_Description;
                }
                set
                {
                    m_Description = value;
                    FirePropertyChanged("Description");
                }
            }
            private void FirePropertyChanged(string propName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }


        private INotifyCollectionChanged m_SourceCollection = null;
        private ObservableCollection<PieHelper> m_ResultingCollection = new ObservableCollection<PieHelper>();
        private string m_Parameter = null;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            m_Parameter = parameter as string;

            if (m_SourceCollection != null)
            {
                foreach (object obj in (IEnumerable<object>) m_SourceCollection)
                {
                    if (obj is INotifyPropertyChanged)
                        ((INotifyPropertyChanged)obj).PropertyChanged -= PropertyChanged;
                }

                m_SourceCollection.CollectionChanged -= CollectionChanged;
            }

            if (value is INotifyCollectionChanged)
            {
                m_SourceCollection = (INotifyCollectionChanged)value;

                foreach (object obj in (IEnumerable<object>)m_SourceCollection)
                {
                    if (obj is INotifyPropertyChanged)
                        ((INotifyPropertyChanged)obj).PropertyChanged += PropertyChanged;
                }

                m_SourceCollection.CollectionChanged += CollectionChanged;
            }

            GenerateCollection();

            return m_ResultingCollection;
        }

        void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value" || e.PropertyName == "Description")
            {               
                GenerateCollection();
            }
        }

        void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (object obj in e.OldItems)
                {
                    if (obj is INotifyPropertyChanged)
                        ((INotifyPropertyChanged)obj).PropertyChanged -= PropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (object obj in e.NewItems)
                {
                    if (obj is INotifyPropertyChanged)
                        ((INotifyPropertyChanged)obj).PropertyChanged += PropertyChanged;
                }
            }

            GenerateCollection();
        }

        void GenerateCollection()
        {
            double total = 0;
            foreach (object obj in (IEnumerable<object>)m_SourceCollection)
            {
                total += System.Convert.ToDouble(new DoubleConverter().Convert(obj.GetType().GetProperty("Value").GetValue(obj, null), typeof(double), null, null));
            }
            double start = 0;
            int count = 0;
            foreach (object obj in (IEnumerable<object>)m_SourceCollection)
            {
                double width;

                object val;

                string description;

                val = obj.GetType().GetProperty("Value").GetValue(obj, null);

                description = obj.GetType().GetProperty("Description").GetValue(obj, null) as string;

                width = System.Convert.ToDouble(new DoubleConverter().Convert(val, typeof(double), null, null)) / total;


                if (m_ResultingCollection.Count > count)
                    m_ResultingCollection[count] = new PieHelper() { End = width + start, Start = start, Value = val, Description = description };
                else
                    m_ResultingCollection.Add(new PieHelper() { End = width + start, Start = start, Value = val, Description = description });

                start += width;
                count++;
            }

            while (m_ResultingCollection.Count > count)
                m_ResultingCollection.RemoveAt(m_ResultingCollection.Count - 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object obj = values[System.Convert.ToInt32(values[values.Length - 1])];
            if (obj != null && DependencyProperty.UnsetValue != obj)
                return Convert(obj, targetType, parameter, culture);
            else
                return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LabelsContentConverter: IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CumulativeConverter.PieHelper helper = values[0] as CumulativeConverter.PieHelper;
            if(helper!=null)
            {
                LabelsContent lc = (LabelsContent)values[1];
                string format = (string)values[2];
                List<string> tempList = new List<string>();

                if((lc & LabelsContent.Description) == LabelsContent.Description)
                {
                    tempList.Add(helper.Description);
                }
                if ((lc & LabelsContent.Percentage) == LabelsContent.Percentage)
                {
                    tempList.Add(helper.Percentage);
                }
                if ((lc & LabelsContent.Value) == LabelsContent.Value)
                {
                    tempList.Add((new FormatConverter()).Convert(new object[]{format, helper.Value}, typeof(string), null, null).ToString());
                }
                return string.Join("\n", tempList.ToArray());
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
