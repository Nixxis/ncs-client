using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

namespace Nixxis.Client.Controls
{
    public class NixxisTreeView: TreeView
    {
        public static readonly DependencyProperty ClickedDataProperty = DependencyProperty.RegisterAttached("ClickedData", typeof(object), typeof(NixxisTreeView));

        public static void SetClickedData(UIElement element, object value)
        {
            element.SetValue(ClickedDataProperty, value);
        }
        public static object GetClickedData(UIElement element)
        {
            return (element.GetValue(ClickedDataProperty));
        }


        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            try
            {
                base.OnItemsSourceChanged(oldValue, newValue);
            }
            catch
            {
            }
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                base.OnItemsChanged(e);
            }
            catch
            {
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);
            if (this.ContextMenu != null)
            {
                Focus();
                TreeViewItem tvi = Helpers.FindVisualParent<TreeViewItem>((e.OriginalSource as FrameworkElement));
                if (tvi != null)
                    SetClickedData(this.ContextMenu, tvi.DataContext);
                else
                    SetClickedData(this.ContextMenu, null);

            }

        }

    }
}
