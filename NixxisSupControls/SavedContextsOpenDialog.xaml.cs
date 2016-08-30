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
    public partial class SavedContextsOpenDialog : Window, INotifyPropertyChanged
    {

        public SavedContextsOpenDialog()
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            if(chkOnlyMyContexts.IsChecked.GetValueOrDefault())
            {
                e.Accepted = !(e.Item as SavedContext).Shared;
            }
            else
            {
                e.Accepted = true;
            }
        }

        private void chkOnlyMyContexts_Checked(object sender, RoutedEventArgs e)
        {
            (this.FindResource("cvs") as CollectionViewSource).View.Refresh();
        }

        private void chkOnlyMyContexts_Unchecked(object sender, RoutedEventArgs e)
        {
            (this.FindResource("cvs") as CollectionViewSource).View.Refresh();
        }

    }

}
