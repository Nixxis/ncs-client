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
    /// Interaction logic for ContactHistoryWindow.xaml
    /// </summary>
    public partial class ContactHistoryWindow : Window
    {
        public ContactHistoryItem SelectedItem { get; private set; }

        public ContactHistoryWindow()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }
        public void SetItemSource(ContactHistory collection)
        {
            historyCrtl.ItemSource = collection;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = historyCrtl.SelectedItem as ContactHistoryItem;

            this.DialogResult = true;
            Close();
        }

        private void QualCrtl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (historyCrtl.SelectedItem != null)
            {
                SelectedItem = historyCrtl.SelectedItem as ContactHistoryItem;

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
