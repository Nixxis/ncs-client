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

namespace Nixxis.Client.Agent
{
    /// <summary>
    /// Interaction logic for PauseControl.xaml
    /// </summary>
    public partial class PauseControl : UserControl
    {
        #region Properties XAML
        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register("ItemSource", typeof(object), typeof(PauseControl), new PropertyMetadata(new PropertyChangedCallback(ItemSourceChanged)));
        public object ItemSource
        {
            get { return (object)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }
        public static void ItemSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(PauseControl), new PropertyMetadata(new PropertyChangedCallback(SelectedItemChanged)));
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        #region Constructor
        public PauseControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Members
        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                this.SelectedItem = e.AddedItems[0];
            }
            else
                this.SelectedItem = null;
        }
        #endregion
    }
}
