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
    public partial class DashboardWidgetPaletteDialog : Window, INotifyPropertyChanged
    {
        private ResourceDictionary m_ResourceDictionary = null;

        private ObservableCollection<Tuple<string, string>> m_Palettes = null;
        public ObservableCollection<Tuple<string, string>> Palettes { get { return m_Palettes; } set { m_Palettes = value; FirePropertyChanged("Palettes"); } }

        private ObservableCollection<Brush> m_Brushes = null;
        public ObservableCollection<Brush> Brushes { get { return m_Brushes; } set { m_Brushes = value; FirePropertyChanged("Brushes"); } }

        private List<int> m_Order = null;
        public List<int> Order { get { return m_Order; } set { m_Order = value; FirePropertyChanged("Order"); } }

        private string m_PaletteName;
        public string PaletteName { get { return m_PaletteName; } 
            set { 
                m_PaletteName = value; 
                FirePropertyChanged("PaletteName");


                Brushes.Clear();
                Order.Clear();

                if (!string.IsNullOrEmpty(value))
                {
                    foreach (Brush b in GetBrushesInPalette(value))
                    {
                        Order.Add(Brushes.Count);
                        Brushes.Add(b);
                    }
                }

            } 
        }

        public DashboardWidgetPaletteDialog()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
            Palettes = new ObservableCollection<Tuple<string, string>>();
            Brushes = new ObservableCollection<Brush>();
            Order = new List<int>();
        }

        public void FillPalettes(ResourceDictionary rd)
        {
            m_ResourceDictionary = rd;

            Palettes.Clear();
            Palettes.Add(new Tuple<string, string>(string.Empty, "Custom palette"));
            foreach(ResourceDictionary merged in rd.MergedDictionaries)
            {

                foreach (DictionaryEntry entry in merged)
                {
                    if (entry.Value is Brush[])
                    {
                        string key = entry.Key as string;
                        if (key != null)
                            Palettes.Add(new Tuple<string, string>(key, string.Format("Based on theme's pallette [{0}]", key)));
                    }
                }
            }
        }

        public void FillOrder(IEnumerable<int> list)
        {
            List<Brush> newOrder = new List<Brush>();

            int count = 0;
            if (list == null || list.Count()==0)
                return;
            foreach(int i in list)
            {
                if (count >= Order.Count)
                    break;
                Order[count] = i;
                newOrder.Add(Brushes[i]);
                count++;
            }

            Brushes = new ObservableCollection<Brush>(newOrder);
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

        public Brush[] GetBrushesInPalette(string strPalette)
        {
            foreach (ResourceDictionary merged in m_ResourceDictionary.MergedDictionaries)
            {

                foreach (DictionaryEntry entry in merged)
                {
                    if (entry.Key.Equals(strPalette) && entry.Value is Brush[])
                    {
                        return (Brush[])entry.Value;
                    }
                }
            }
            return new Brush[] { };
        }

        private void ThemeColorMoveDown(object sender, RoutedEventArgs e)
        {
            if (lbObjects.SelectedIndex < 0)
                return;
            int toInsert = lbObjects.SelectedIndex + 1;
            if (toInsert >= Brushes.Count)
                return;

            int selectedIndex =lbObjects.SelectedIndex;

            Brush obj = Brushes[selectedIndex];
            int order = Order[selectedIndex];

            Brushes.RemoveAt(selectedIndex);
            Order.RemoveAt(selectedIndex);

            Brushes.Insert(toInsert, obj);
            Order.Insert(toInsert, order);

            lbObjects.SelectedIndex = toInsert;
            
        }

        private void ThemeColorMoveUp(object sender, RoutedEventArgs e)
        {
            if (lbObjects.SelectedIndex < 0)
                return;
            int toInsert = lbObjects.SelectedIndex - 1;
            if (toInsert < 0)
                return;

            int selectedIndex = lbObjects.SelectedIndex;

            Brush obj = Brushes[selectedIndex];
            int order = Order[selectedIndex];

            Brushes.RemoveAt(selectedIndex);
            Order.RemoveAt(selectedIndex);

            Brushes.Insert(toInsert, obj);
            Order.Insert(toInsert, order);

            lbObjects.SelectedIndex = toInsert;
        }


        private void CustomColorMoveDown(object sender, RoutedEventArgs e)
        {
            if (lbSolidBrushes.SelectedIndex < 0)
                return;
            int toInsert = lbSolidBrushes.SelectedIndex + 1;
            if (toInsert >= Brushes.Count)
                return;

            int selectedIndex = lbSolidBrushes.SelectedIndex;

            Brush obj = Brushes[selectedIndex];

            Brushes.RemoveAt(selectedIndex);

            Brushes.Insert(toInsert, obj);

            lbSolidBrushes.SelectedIndex = toInsert;
        }

        private void CustomColorMoveUp(object sender, RoutedEventArgs e)
        {
            if (lbSolidBrushes.SelectedIndex < 0)
                return;
            int toInsert = lbSolidBrushes.SelectedIndex - 1;
            if (toInsert < 0)
                return;

            int selectedIndex = lbSolidBrushes.SelectedIndex;

            Brush obj = Brushes[selectedIndex];

            Brushes.RemoveAt(selectedIndex);

            Brushes.Insert(toInsert, obj);

            lbSolidBrushes.SelectedIndex = toInsert;

        }

        private void CustomColorAdd(object sender, RoutedEventArgs e)
        {
            Brushes.Add(new SolidColorBrush(Colors.AliceBlue));
            lbObjects.SelectedIndex = Brushes.Count - 1;
        }

        private void CustomColorRemove(object sender, RoutedEventArgs e)
        {

            if (lbSolidBrushes.SelectedIndex >= 0)
            {
                int backup = lbSolidBrushes.SelectedIndex;

                Brushes.RemoveAt(lbSolidBrushes.SelectedIndex);

                if (Brushes.Count != 0)
                {
                    if (backup >= Brushes.Count)
                        lbSolidBrushes.SelectedIndex = Brushes.Count - 1;
                    else
                        lbSolidBrushes.SelectedIndex = backup;
                }

            }



        }
    }

}
