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
    /// Interaction logic for PredefinedTextControl.xaml
    /// </summary>
    public partial class PredefinedTextControl : UserControl
    {
        #region Events
        public event PredefinedTextSelectHandeler TextSelected;
        internal void OnTextSelected(PredefinedTextItem text)
        {
            if (TextSelected != null)
                TextSelected(this, text);
        }
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register("ItemSource", typeof(PredefinedTextCollection), typeof(PredefinedTextControl), new PropertyMetadata(new PropertyChangedCallback(ItemSourceChanged)));
        public PredefinedTextCollection ItemSource
        {
            get { return (PredefinedTextCollection)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }
        public static void ItemSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(PredefinedTextItem), typeof(PredefinedTextControl), new PropertyMetadata(new PropertyChangedCallback(SelectedItemChanged)));
        public PredefinedTextItem SelectedItem
        {
            get { return (PredefinedTextItem)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        #region Constrcutors
        public PredefinedTextControl()
        {
            InitializeComponent(); 
        }
        #endregion

        #region Members override
        #endregion


        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.SelectedItem == null)
                    this.OnTextSelected(null);
                else
                    this.OnTextSelected((PredefinedTextItem)this.SelectedItem);
            }
        }
    }

    public delegate void PredefinedTextSelectHandeler(object sender, PredefinedTextItem text);
}
