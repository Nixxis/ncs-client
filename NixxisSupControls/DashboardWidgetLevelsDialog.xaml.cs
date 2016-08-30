using Nixxis.Client.Controls;
using Nixxis.ClientV2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.WpfPropertyGrid;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nixxis.Client.Supervisor
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class DashboardWidgetLevelsDialog : Window, INotifyPropertyChanged
    {
        private ObservableCollection<ObservableContent> m_Values;
        public ObservableCollection<ObservableContent> Values { 
            get 
            {
                return m_Values;
            }
            set
            {
                m_Values = value;
                FirePropertyChanged("Values");
            }
        }

        public void FillValues(IEnumerable<int> values) 
        {
            foreach(int i in values)
            {
                Values.Add(new ObservableContent(){Value = i});
            }
        }

        public IEnumerable<int> GetValues()
        {
            List<int> templist = new List<int>();

            foreach(ObservableContent oc in Values)
                templist.Add(oc.Value);
            return templist;
        }

        public DashboardWidgetLevelsDialog()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
            Values = new ObservableCollection<ObservableContent>();
        }

        private void Add(object sender, RoutedEventArgs e)
        {

            if (Values.Count > 0)
                Values.Add(new ObservableContent() { Value = Values[Values.Count - 1].Value + 1 });
            else
                Values.Add(new ObservableContent() { Value = 0 });
        }

        private void Remove(object sender, RoutedEventArgs e)
        {
            if (lbValues.SelectedIndex >=0)
            {
                int backup = lbValues.SelectedIndex;

                Values.RemoveAt(lbValues.SelectedIndex);

                if (Values.Count != 0)
                {
                    if (backup >= Values.Count)
                        lbValues.SelectedIndex = Values.Count - 1;
                    else
                        lbValues.SelectedIndex = backup;
                }

            }


        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        public class ObservableContent: INotifyPropertyChanged
        {
            private int m_Value;
            public int Value 
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

            public event PropertyChangedEventHandler PropertyChanged;

            private void FirePropertyChanged(string propName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }

        }
    }

}
