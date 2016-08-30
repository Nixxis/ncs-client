using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Nixxis.Client.Controls
{
    public class NixxisTabItem: TabItem
    {
        public bool m_IsSystemHidden = false;

        public NixxisTabItem()
        {
            IsVisibleChanged += new System.Windows.DependencyPropertyChangedEventHandler(NixxisTabItem_IsVisibleChanged);
        }

        void NixxisTabItem_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {

            if (!((bool)e.NewValue) && IsSelected)
            {
                IsSelected = false;
            }

            int visibleCount = 0;

            NixxisTabItem TheOneVisible = null;
            NixxisTabItem TheOneSystemHidden = null;

            for (int i = 0; i < ((sender as NixxisTabItem).Parent as NixxisTabControl).Items.Count; i++)
            {
                if ((((sender as NixxisTabItem).Parent as NixxisTabControl).Items[i] as NixxisTabItem).Visibility == System.Windows.Visibility.Visible)
                {
                    TheOneVisible = ((sender as NixxisTabItem).Parent as NixxisTabControl).Items[i] as NixxisTabItem;

                    visibleCount++;
                }
                else
                {
                    if ((((sender as NixxisTabItem).Parent as NixxisTabControl).Items[i] as NixxisTabItem).m_IsSystemHidden)
                    {
                        TheOneSystemHidden = ((sender as NixxisTabItem).Parent as NixxisTabControl).Items[i] as NixxisTabItem;

                    }
                }
            }

            if (visibleCount == 1)
            {
                // TODO later: for now it is better to display the "General" tab even when it is the only one rather than not displaying the info at all.
                return;


                if (TheOneSystemHidden == null)
                {

                    TheOneVisible.Visibility = System.Windows.Visibility.Collapsed;
                    TheOneVisible.m_IsSystemHidden = true;

                }
                else
                {
                    TheOneSystemHidden.Visibility = System.Windows.Visibility.Visible;
                    TheOneSystemHidden.m_IsSystemHidden = false;
                    TheOneVisible.IsSelected = true;
                    TheOneSystemHidden.IsSelected = true;
                    if (!((bool)e.NewValue) && IsSelected)
                    {
                        IsSelected = false;
                    }

                }
            }
            else
            {
                if (!((bool)e.NewValue) && IsSelected)
                {
                    IsSelected = false;
                }
            }
        }

    }

    public class NixxisTabControl : TabControl
    {
        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register("ReadOnly", typeof(bool), typeof(NixxisTabControl), new PropertyMetadata(false, new PropertyChangedCallback(ReadOnlyChanging)));

        public NixxisTabControl()
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(NixxisTabControl_IsVisibleChanged);
        }

        public bool ReadOnly
        {
            get
            {
                return (bool)GetValue(ReadOnlyProperty);
            }
            set
            {
                SetValue(ReadOnlyProperty, value);
            }
        }

        public static void ReadOnlyChanging(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisTabControl ntb = obj as NixxisTabControl;

            Helpers.ApplyToChildren<DataGrid>(ntb, DataGrid.IsReadOnlyProperty, args.NewValue);

            Helpers.ApplyToChildren<FrameworkElement>(ntb, FrameworkElement.IsEnabledProperty, !((bool)args.NewValue), (elm) => (!(elm is TabItem) && !(elm is DataGrid) && !(elm is Grid)));            

        }

        void NixxisTabControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (IsVisible)
            {
                if (SelectedIndex == -1 || (Items[SelectedIndex] as NixxisTabItem).Visibility != System.Windows.Visibility.Visible)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if ((Items[i] as NixxisTabItem).Visibility == System.Windows.Visibility.Visible)
                        {
                            SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {

            base.OnSelectionChanged(e);
            if (IsVisible)
            {
                if (SelectedIndex == -1 || (Items[SelectedIndex] as NixxisTabItem).Visibility != System.Windows.Visibility.Visible)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if ((Items[i] as NixxisTabItem).Visibility == System.Windows.Visibility.Visible || (Items[i] as NixxisTabItem).m_IsSystemHidden)
                        {
                            SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (SelectedIndex == -1 || (Items[SelectedIndex] as NixxisTabItem).Visibility != System.Windows.Visibility.Visible)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if ((Items[i] as NixxisTabItem).Visibility == System.Windows.Visibility.Visible )
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
            }
        }

    }
}
