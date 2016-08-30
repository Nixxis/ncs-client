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
    /// Interaction logic for QualificationControl.xaml
    /// </summary>
    public partial class QualificationControl : UserControl
    {
        #region XAML Properties
        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register("ItemSource", typeof(object), typeof(QualificationControl), new PropertyMetadata(new PropertyChangedCallback(ItemSourceChanged)));
        public object ItemSource
        {
            get { return (object)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }
        public static void ItemSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(QualificationControl), new PropertyMetadata(new PropertyChangedCallback(SelectedItemChanged)));
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        #region Constructors
        public QualificationControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Members
        private void SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (((QualificationInfo)e.NewValue).ListQualification.Count == 0)
            {
                OnSelectedQualificationChanged((QualificationInfo)e.NewValue, this.SelectedItem == null ? null : (QualificationInfo)this.SelectedItem);
                this.SelectedItem = e.NewValue;
            }
            else
            {
                OnSelectedQualificationChanged(null, this.SelectedItem == null ? null : (QualificationInfo)this.SelectedItem);
                this.SelectedItem = null;
            }
        }
        #endregion

        #region Events
        public event QualificationControlEventHandler SelectedQualificationChanged;
        private void OnSelectedQualificationChanged(QualificationInfo newItem, QualificationInfo oldItem)
        {
            if (SelectedQualificationChanged != null)
            {
                SelectedQualificationChanged(this, new QualificationControlEventArg { NewItem = newItem, OldItem = oldItem });
            }
        }
        #endregion
    }

    public delegate void QualificationControlEventHandler(object sender, QualificationControlEventArg e);

    public class QualificationControlEventArg
    {
        public QualificationInfo NewItem;
        public QualificationInfo OldItem;
    }
}
