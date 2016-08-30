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
    public partial class DashboardWidgetDatasourceDialog : Window, INotifyPropertyChanged
    {

        private string m_ObjType;
        public string ObjType
        {
            get { return m_ObjType; }
            set
            {
                m_ObjType = value;
                ObjectDescriptionProperty = new DescriptionSelector().Convert(new Object[] { DataContext, ObjType }, null, null, null) as string;
                FirePropertyChanged("ObjType");
            }
        }
        public List<string> ObjIds
        {
            get
            {
                if (chkAll.IsChecked.GetValueOrDefault())
                    return new List<string>();
                else
                    return new List<string>(Objects.Select((obj) => (obj.GetType().GetProperty("Id").GetValue(obj, null) as string)));
            }
            set
            {
                if (!String.IsNullOrEmpty(ObjType))
                {
                    if (value.Count == 0)
                    {
                        chkAll.IsChecked = true;
                    }
                    else
                    {
                        IEnumerable ienum = new ObjectSelector().Convert(new object[] { DataContext, ObjType }, null, null, null) as IEnumerable;
                        foreach (string strVal in value)
                        {
                            foreach (object o in ienum)
                                if (o.GetType().GetProperty("Id").GetValue(o, null).Equals(strVal))
                                    Objects.Add(o);
                        }
                    }

                }
                FirePropertyChanged("Objects");
            }
        }
        public List<string> ObjProperties
        {
            get
            {
                return new List<string>(Properties.Select((obj) => (obj.GetType().GetProperty("Id").GetValue(obj, null) as string)));

            }
            set
            {
                if (!String.IsNullOrEmpty(ObjType))
                {
                    IEnumerable ienum = new PropertySelector().Convert(new object[] { DataContext, ObjType }, null, null, null) as IEnumerable;

                    foreach (string strVal in value)
                    {
                        foreach (object o in ienum)
                            if (o.GetType().GetProperty("Id").GetValue(o, null).Equals(strVal))
                                Properties.Add(o);
                    }
                }
                FirePropertyChanged("Properties");
            }
        }





        private ObservableCollection<object> m_Objects = new ObservableCollection<object>();
        public ObservableCollection<object> Objects { get { return m_Objects; } set { m_Objects = value; } }

        private ObservableCollection<object> m_Properties = new ObservableCollection<object>();
        public ObservableCollection<object> Properties { get { return m_Properties; } set { m_Properties = value; } }


        private string m_ObjectDescriptionProperty;
        public string ObjectDescriptionProperty
        {
            get { return m_ObjectDescriptionProperty; }
            set
            {
                m_ObjectDescriptionProperty = value;
                FirePropertyChanged("ObjectDescriptionProperty");
            }
        }

        public DashboardWidgetDatasourceDialog()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void AddObject(object sender, RoutedEventArgs e)
        {
            DashboardWidgetObjectSelectorDialog dlg = new DashboardWidgetObjectSelectorDialog();
            dlg.DataContext = this.DataContext;
            dlg.Owner = this;
            dlg.ObjType = ObjType;


            if (dlg.ShowDialog().GetValueOrDefault())
            {
                ObjectDescriptionProperty = new DescriptionSelector().Convert(new Object[] { DataContext, ObjType }, null, null, null) as string;

                foreach(object obj in dlg.lbObjects.SelectedItems)
                    Objects.Add(obj);
            }


        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private void RemoveObject(object sender, RoutedEventArgs e)
        {
            int backup = lbObjects.SelectedIndex;

            Objects.RemoveAt(lbObjects.SelectedIndex);

            if (Objects.Count != 0)
            {
                if (backup >= Objects.Count)
                    lbObjects.SelectedIndex = Objects.Count - 1;
                else
                    lbObjects.SelectedIndex = backup;
            }

        }

        private void ObjectMoveUp(object sender, RoutedEventArgs e)
        {
            if (lbObjects.SelectedIndex < 0)
                return;
            int toInsert = lbObjects.SelectedIndex - 1;
            if (toInsert < 0)
                return;
            object obj = Objects[lbObjects.SelectedIndex];
            Objects.RemoveAt(lbObjects.SelectedIndex);
            Objects.Insert(toInsert, obj);
            lbObjects.SelectedIndex = toInsert;
        }

        private void ObjectMoveDown(object sender, RoutedEventArgs e)
        {
            if (lbObjects.SelectedIndex < 0)
                return;
            int toInsert = lbObjects.SelectedIndex + 1;
            if (toInsert >= Objects.Count)
                return;
            object obj = Objects[lbObjects.SelectedIndex];
            Objects.RemoveAt(lbObjects.SelectedIndex);
            Objects.Insert(toInsert, obj);
            lbObjects.SelectedIndex = toInsert;
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            lbObjects.SelectedIndex = -1;
        }

        private void PropertyAdd(object sender, RoutedEventArgs e)
        {
            DashboardWidgetPropertySelectorDialog dlg = new DashboardWidgetPropertySelectorDialog();
            dlg.DataContext = this.DataContext;
            dlg.Owner = this;
            dlg.ObjType = ObjType;


            if (dlg.ShowDialog().GetValueOrDefault())
            {
                foreach (object obj in dlg.lbProperties.SelectedItems)
                    Properties.Add(obj);
            }

        }

        private void PropertyRemove(object sender, RoutedEventArgs e)
        {
            int backup = lbProperties.SelectedIndex;

            Properties.RemoveAt(lbProperties.SelectedIndex);

            if (Properties.Count != 0)
            {
                if (backup >= Properties.Count)
                    lbProperties.SelectedIndex = Properties.Count - 1;
                else
                    lbProperties.SelectedIndex = backup;
            }
        }

        private void PropertyMoveUp(object sender, RoutedEventArgs e)
        {
            if (lbProperties.SelectedIndex < 0)
                return;
            int toInsert = lbProperties.SelectedIndex - 1;
            if (toInsert < 0)
                return;
            object obj = Properties[lbProperties.SelectedIndex];
            Properties.RemoveAt(lbProperties.SelectedIndex);
            Properties.Insert(toInsert, obj);
            lbProperties.SelectedIndex = toInsert;

        }

        private void PropertyMoveDown(object sender, RoutedEventArgs e)
        {
            if (lbProperties.SelectedIndex < 0)
                return;
            int toInsert = lbProperties.SelectedIndex + 1;
            if (toInsert >= Properties.Count)
                return;
            object obj = Properties[lbProperties.SelectedIndex];
            Properties.RemoveAt(lbProperties.SelectedIndex);
            Properties.Insert(toInsert, obj);
            lbProperties.SelectedIndex = toInsert;

        }

    }



}
