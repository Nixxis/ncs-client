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
using System.Collections;

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for TeamSelectionWindow.xaml
    /// </summary>
    public partial class TeamSelectionWindow : Window
    {
        public Nixxis.Client.Controls.NixxisAdvListBoxCollection ItemList
        {
            get { return TeamCrtl.ItemList; }
        }

        public TeamSelectionWindow()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        public void SetItemSource(TeamCollection collection)
        {
            TeamCrtl.ItemSource = collection;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
