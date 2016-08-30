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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for WallboardView.xaml
    /// </summary>
    public partial class WallboardView : NixxisPanelSelectorItem
    {
        #region Class data
        #endregion

        #region Properties XAML
        #endregion

        #region Constructors
        public WallboardView()
        {
            InitializeComponent();
        }

        public WallboardView(HttpLink link)
        {
            ClientLink = link;

            InitializeComponent();
        }
        #endregion

        #region Members

        #endregion
    }
}
