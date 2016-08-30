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

namespace Nixxis.Client.Supervisor
{
    /// <summary>
    /// Interaction logic for NixxisViewAgentScreen.xaml
    /// </summary>
    public partial class NixxisViewAgentScreen : Window
    {
        public Nixxis.ClientV2.Supervision SupervisionLink;
        public int CloseLevel = 0;

        public NixxisViewAgentScreen()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
        }

        private void MySelf_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CloseLevel < 5)
            {
                CloseLevel++;
                e.Cancel = true;
                SupervisionLink.ClientLink.Commands.ViewScreen.Execute();
            }
        }

    }
}
