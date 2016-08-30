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
using System.Collections;
using Nixxis.Client.Controls;

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for TeamSelectionControl.xaml
    /// </summary>
    public partial class TeamSelectionControl : UserControl
    {
        #region Properties XAML
        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register("ItemSource", typeof(object), typeof(TeamSelectionControl), new PropertyMetadata(new PropertyChangedCallback(ItemSourceChanged)));
        public object ItemSource
        {
            get { return (object)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }
        public static void ItemSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {

        }

        public NixxisAdvListBoxCollection ItemList
        {
            get { return (NixxisAdvListBoxCollection)lstTeam.ItemsSource; }
        }
        #endregion

        #region Constructor
        public TeamSelectionControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Members

        #endregion
    }
}
