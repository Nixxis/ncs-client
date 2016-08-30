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
    /// Interaction logic for PauseWindow.xaml
    /// </summary>
    public partial class PauseWindow : Window
    {
        public PauseCodeInfo SelectedItem { get; private set; }

        public PauseWindow()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public void SetItemSource(PauseCodeCollection collection)
        {
            PauseCrtl.ItemSource = collection;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = PauseCrtl.SelectedItem as PauseCodeInfo;

            this.DialogResult = true;
            Close();
        }

        private void QualCrtl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PauseCrtl.SelectedItem != null)
            {
                SelectedItem = PauseCrtl.SelectedItem as PauseCodeInfo;

                this.DialogResult = true;
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
