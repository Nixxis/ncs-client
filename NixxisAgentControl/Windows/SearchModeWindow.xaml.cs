using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for SearchModeWindow.xaml
    /// </summary>
    public partial class SearchModeWindow : Window
    {
        public ActivityInfo SelectedItem { get; private set; }
        
        public SearchModeWindow()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }
        public void SetItemSource(ActivityCollection collection)
        {
            SearchCrtl.ItemSource = collection;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = SearchCrtl.SelectedItem as ActivityInfo;

            this.DialogResult = true;
            Close();
        }

        private void QualCrtl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SearchCrtl.SelectedItem != null)
            {
                SelectedItem = SearchCrtl.SelectedItem as ActivityInfo;

                this.DialogResult = true;
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
