//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Nixxis.Client.Controls
{
    public class NixxisAdvListBox : ListBox
    {
        #region Class data
        private int m_InUpdate = 0;
        #endregion

        #region Properies
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty IsSelectedMemberPathProperty = DependencyProperty.Register("IsSelectedMemberPath", typeof(string), typeof(NixxisAdvListBox), new FrameworkPropertyMetadata(string.Empty, IsSelectedMemberPathPropertyChanged));
        public string IsSelectedMemberPath
        {
            get { return (string)GetValue(IsSelectedMemberPathProperty); }
            set { SetValue(IsSelectedMemberPathProperty, value); }
        }
        public static void IsSelectedMemberPathPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisAdvListBox ctrl = (NixxisAdvListBox)obj;
        }

        public static readonly DependencyProperty ItemSourceListProperty = DependencyProperty.Register("ItemSourceList", typeof(IEnumerable), typeof(NixxisAdvListBox), new FrameworkPropertyMetadata(ItemSourceListPropertyChanged));
        public IEnumerable ItemSourceList
        {
            get { return (IEnumerable)GetValue(ItemSourceListProperty); }
            set { SetValue(ItemSourceListProperty, value); }
        }
        public static void ItemSourceListPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisAdvListBox ctrl = (NixxisAdvListBox)obj;

            ctrl.SetItemSource();

            RaiseItemSourceListChangedEvent(obj);
        }

        public static readonly DependencyProperty NixxisSelectedItemProperty = DependencyProperty.Register("NixxisSelectedItem", typeof(object), typeof(NixxisAdvListBox), new FrameworkPropertyMetadata(NixxisSelectedItemPropertyChanged));
        public object NixxisSelectedItem
        {
            get { return (object)GetValue(NixxisSelectedItemProperty); }
            set { SetValue(NixxisSelectedItemProperty, value); }
        }
        public static void NixxisSelectedItemPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisAdvListBox ctrl = (NixxisAdvListBox)obj;
            ctrl.OnNixxisSelectionItem(args.NewValue);
        }

        public static readonly DependencyProperty NixxisSelectedItemsProperty = DependencyProperty.Register("NixxisSelectedItems", typeof(IList), typeof(NixxisAdvListBox), new FrameworkPropertyMetadata(NixxisSelectedItemsPropertyChanged));
        public IList NixxisSelectedItems
        {
            get { return (IList)GetValue(NixxisSelectedItemsProperty); }
            set { SetValue(NixxisSelectedItemsProperty, value); }
        }
        public static void NixxisSelectedItemsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public new static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(NixxisAdvListBox), new FrameworkPropertyMetadata(string.Empty, DisplayMemberPathPropertyChanged));
        public new string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }
        public static void DisplayMemberPathPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }
        #endregion

        public static readonly RoutedEvent ItemSourceListChangedEvent = EventManager.RegisterRoutedEvent("ItemSourceListChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NixxisAdvListBox));

        public event RoutedEventHandler ItemSourceListChanged
        {
            add { AddHandler(ItemSourceListChangedEvent, value); }
            remove { RemoveHandler(ItemSourceListChangedEvent, value); }
        }

        internal static RoutedEventArgs RaiseItemSourceListChangedEvent(DependencyObject target)
        {
            if (target == null) return null;

            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = ItemSourceListChangedEvent;

            if (target is UIElement)
            {
                (target as UIElement).RaiseEvent(args);
            }
            else if (target is ContentElement)
            {
                (target as ContentElement).RaiseEvent(args);
            }

            return args;
        }



        #region Static
        static NixxisAdvListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisAdvListBox), new FrameworkPropertyMetadata(typeof(NixxisAdvListBox)));
        }
        #endregion

        #region Members override

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            DisplayDebugInfo(string.Format("+---> OnSelectionChanged"), false, false);            
            
            if (m_InUpdate > 0)
                return;

            m_InUpdate++;

#if DEBUG
            if (e.AddedItems == null)
            {
                if (e.RemovedItems == null)
                    DisplayDebugInfo(string.Format("OnSelectionChanged START ----> Added: null, Removed: null"), false, true);
                else
                    DisplayDebugInfo(string.Format("OnSelectionChanged START ----> Added: null, Removed: {0}", e.RemovedItems.Count), false, true);
            }
            else
            {
                if (e.RemovedItems == null)
                    DisplayDebugInfo(string.Format("OnSelectionChanged START ----> Added: {0}, Removed: null", e.AddedItems.Count), false, true);
                else
                    DisplayDebugInfo(string.Format("OnSelectionChanged START ----> Added: {0}, Removed: {1}", e.AddedItems.Count, e.RemovedItems.Count), false, true);
            }
#endif

            if (this.NixxisSelectedItems == null)
                this.NixxisSelectedItems = new NixxisSelectedItemCollection();

            if (e.AddedItems != null)
            {
                foreach (object item in e.AddedItems)
                {
                    if (item.GetType() == typeof(NixxisAdvListBoxItem))
                    {
                        if (((NixxisAdvListBoxItem)item).IsSelected == false)
                            ((NixxisAdvListBoxItem)item).SetIsSelected(true);
                    }

                    if (!this.NixxisSelectedItems.Contains(((NixxisAdvListBoxItem)item).Item))
                    {
                        this.NixxisSelectedItems.Add(((NixxisAdvListBoxItem)item).Item);
                    }
                }
            }


            if (e.RemovedItems != null)
            {
                foreach (object item in e.RemovedItems)
                {
                    if (item.GetType() == typeof(NixxisAdvListBoxItem))
                    {
                        if (((NixxisAdvListBoxItem)item).IsSelected == true)
                            ((NixxisAdvListBoxItem)item).SetIsSelected(false);
                    }

                    if (this.NixxisSelectedItems.Contains(((NixxisAdvListBoxItem)item).Item))
                    {
                        this.NixxisSelectedItems.Remove(((NixxisAdvListBoxItem)item).Item);
                    }
                }
            }

            if (this.SelectedItem == null)
                this.NixxisSelectedItem = null;
            else
                this.NixxisSelectedItem = ((NixxisAdvListBoxItem)this.SelectedItem).Item;
            
            DisplayDebugInfo(string.Format("OnSelectionChanged --- In Progress --- "), false, true);


            m_InUpdate--;
            base.OnSelectionChanged(e); 
            
            if (this.SelectedItem == null)
                this.NixxisSelectedItem = null;
            else
                this.NixxisSelectedItem = ((NixxisAdvListBoxItem)this.SelectedItem).Item;

            DisplayDebugInfo(string.Format("OnSelectionChanged END <----"), false, true);
        }

        public override void BeginInit()
        {
            base.BeginInit();

            DisplayDebugInfo(string.Format("BeginInit"), true, false);

            this.NixxisSelectedItems = new NixxisSelectedItemCollection();
            ((NixxisSelectedItemCollection)this.NixxisSelectedItems).CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(NixxisSelectedItems_CollectionChanged);
        }

        void ItemSourceList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DisplayDebugInfo(string.Format("+---> ItemSourceList_CollectionChanged"), false, false);

            if (m_InUpdate > 0)
                return;

            m_InUpdate++;

            DisplayDebugInfo(string.Format("ItemSourceList_CollectionChanged START ----> action: {0}, count: {1}", e.Action.ToString(), e.NewItems==null ? -1:  e.NewItems.Count), true, false);

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (object item in e.NewItems)
                    {
                        NixxisAdvListBoxItem lstItem;
                        if (item.GetType() == typeof(NixxisAdvListBoxItem))
                            lstItem = (NixxisAdvListBoxItem)item;
                        else
                            lstItem = CreateListBoxItem(item);

                        if (lstItem != null)
                        {
                            ((NixxisAdvListBoxCollection)this.ItemsSource).Add(lstItem);
                        }
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (object item in e.OldItems)
                    {
                        NixxisAdvListBoxItem lstItem = null;
                        if (item.GetType() == typeof(NixxisAdvListBoxItem))
                        {
                            lstItem = (NixxisAdvListBoxItem)item;
                            if (lstItem != null)
                            {
                                if(NixxisSelectedItems.Contains(lstItem))
                                    NixxisSelectedItems.Remove(lstItem);
                                ((NixxisAdvListBoxCollection)this.ItemsSource).Remove(lstItem);
                            }
                        }
                        else if (this.ItemSourceList is NixxisAdvListBoxCollection)
                        {
                            lstItem = ((NixxisAdvListBoxCollection)this.ItemSourceList).GetListBoxItem(item);
                            if (lstItem != null)
                            {
                                if (NixxisSelectedItems.Contains(lstItem))
                                    NixxisSelectedItems.Remove(lstItem);
                                ((NixxisAdvListBoxCollection)this.ItemsSource).Remove(lstItem);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < ((NixxisAdvListBoxCollection)this.ItemsSource).Count; i++)
                            {
                                if (((NixxisAdvListBoxCollection)this.ItemsSource)[i].Item == item)
                                {
                                    if (NixxisSelectedItems.Contains(item))
                                        NixxisSelectedItems.Remove(item);
                                    ((NixxisAdvListBoxCollection)this.ItemsSource).RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                this.SetItemSource();
            }

            DisplayDebugInfo(string.Format("ItemSourceList_CollectionChanged END <---- action: {0}, count: {1}", e.Action.ToString(), e.NewItems==null ? -1 : e.NewItems.Count), true, false);
            m_InUpdate--;

               
        }

        void NixxisSelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
                DisplayDebugInfo(string.Format("NixxisAdvListBox_CollectionChanged ----> action {0}", e.Action.ToString()), false, true);
            else
                DisplayDebugInfo(string.Format("NixxisAdvListBox_CollectionChanged ----> action {0} values {1}", e.Action.ToString(), e.NewItems.Count), false, true);

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                this.OnNixxisSelectionItems(e.NewItems, System.Collections.Specialized.NotifyCollectionChangedAction.Add);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                this.OnNixxisSelectionItems(e.NewItems, System.Collections.Specialized.NotifyCollectionChangedAction.Remove);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                if (this.NixxisSelectedItems.Count > 0)
                    this.OnNixxisSelectionItems(this.NixxisSelectedItems, System.Collections.Specialized.NotifyCollectionChangedAction.Remove);
            }
        }
        #endregion

        #region Members
        private void OnNixxisSelectionItem(object value)
        {
            DisplayDebugInfo(string.Format("+---> OnNixxisSelectionItem"), false, false);

            if (m_InUpdate > 0)
                return;

            if (value == null)
                DisplayDebugInfo(string.Format("OnNixxisSelectionItem ----> null"), false, true);
            else
                DisplayDebugInfo(string.Format("OnNixxisSelectionItem START ----> {0}", value.GetType().ToString()), false, true);

            if (value == null)
                return;

            if (this.ItemsSource == null || this.ItemsSource.GetType() != typeof(NixxisAdvListBoxCollection))
                return;

            NixxisAdvListBoxCollection collection = (NixxisAdvListBoxCollection)this.ItemsSource;
            NixxisAdvListBoxItem lstItem = collection.GetListBoxItem(value);

            if (lstItem == null)
                return;

            this.SelectedItem = lstItem;

            DisplayDebugInfo(string.Format("OnNixxisSelectionItem END <----"), false, true);
        }

        private void OnNixxisSelectionItems(object values, System.Collections.Specialized.NotifyCollectionChangedAction action)
        {
            DisplayDebugInfo(string.Format("+---> OnNixxisSelectionItems"), false, false);
            
            if (values == null)
                return;
            
            if (m_InUpdate > 0)
                return;

            m_InUpdate++;

            if (values == null)
                DisplayDebugInfo(string.Format("OnNixxisSelectionItems ----> null"), false, true);
            else
                DisplayDebugInfo(string.Format("OnNixxisSelectionItems START ----> action {0} values {1}", action.ToString(), values.GetType().ToString()), false, true);
            

            if (action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (object item in (IList)values)
                {
                    NixxisAdvListBoxItem lstItem;
                    if (item.GetType() == typeof(NixxisAdvListBoxItem))
                        lstItem = (NixxisAdvListBoxItem)item;
                    else if (this.ItemsSource == null)
                        lstItem = this.GetListBoxItem(item);
                    else
                        lstItem = ((NixxisAdvListBoxCollection)this.ItemsSource).GetListBoxItem(item);

                    if (lstItem != null)
                    {
                        if(!this.SelectedItems.Contains(lstItem))
                            this.SelectedItems.Add(lstItem);

                        if (lstItem.IsSelected == false)
                            lstItem.SetIsSelected(true);
                    }
                }
            }
            else if (action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in (IList)values)
                {
                    NixxisAdvListBoxItem lstItem;
                    if (item.GetType() == typeof(NixxisAdvListBoxItem))
                        lstItem = (NixxisAdvListBoxItem)item;
                    else if (this.ItemsSource == null)
                        lstItem = this.GetListBoxItem(item);
                    else
                        lstItem = ((NixxisAdvListBoxCollection)this.ItemsSource).GetListBoxItem(item);

                    if (lstItem != null)
                        this.SelectedItems.Remove(lstItem);

                    if (lstItem.IsSelected == true)
                        lstItem.SetIsSelected(false);
                }
            }

            if (this.SelectedItem == null)
                this.NixxisSelectedItem = null;
            else
                this.NixxisSelectedItem = ((NixxisAdvListBoxItem)this.SelectedItem).Item;



            m_InUpdate--;
            DisplayDebugInfo(string.Format("OnNixxisSelectionItems END <----"), false, true);
        }

        public NixxisAdvListBoxItem GetListBoxItem(object value)
        {
            foreach (object item in this.Items)
            {
                if (item.GetType() == typeof(NixxisAdvListBoxItem))
                {
                    if (((NixxisAdvListBoxItem)item).Item.Equals(value))
                        return (NixxisAdvListBoxItem)item;
                }
            }
            return null;
        }

       /// <summary>
        /// To set the itemSource from the itemSourceList
        /// </summary>
        private void SetItemSource()
        {
            DisplayDebugInfo(string.Format("+---> OnNixxisSelectionItems"), false, false);

            if (m_InUpdate > 0)
                return;

            if (this.ItemSourceList == null)
                return;

            m_InUpdate++;

            DisplayDebugInfo(string.Format("--> SetItemSource Start."), true, false);

            this.ItemsSource = new NixxisAdvListBoxCollection();

            if (ItemSourceList is INotifyCollectionChanged)
            {
                try
                {
                    ((INotifyCollectionChanged)ItemSourceList).CollectionChanged -= ItemSourceList_CollectionChanged;
                }
                catch { }

                ((INotifyCollectionChanged)this.ItemSourceList).CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ItemSourceList_CollectionChanged);
            }

 

            foreach (object item in this.ItemSourceList)
            {
                NixxisAdvListBoxItem lstItem = CreateListBoxItem(item);

                if(lstItem != null)
                    ((NixxisAdvListBoxCollection)this.ItemsSource).Add(lstItem);
            }

            DisplayDebugInfo(string.Format("SetItemSource End. <--"), true, false);

            m_InUpdate--;
        }


        private NixxisAdvListBoxItem CreateListBoxItem(object value)
        {
            NixxisAdvListBoxItem lstItem = new NixxisAdvListBoxItem();

            if (value == null)
                return lstItem;

            lstItem.Item = value;
            lstItem.Owner = this;

            if (string.IsNullOrEmpty(this.DisplayMemberPath.Trim()))
                lstItem.Text = value.ToString();
            else
            {
                PropertyInfo propInfo = value.GetType().GetProperty(this.DisplayMemberPath);

                if (propInfo != null)
                    lstItem.Text = propInfo.GetValue(value, null).ToString();
                else
                    lstItem.Text = value.ToString();
            }

            if (!string.IsNullOrEmpty(this.IsSelectedMemberPath.Trim()))
            {
                PropertyInfo propInfo = value.GetType().GetProperty(this.IsSelectedMemberPath);

                if (propInfo != null)
                {
                    if(propInfo.PropertyType == typeof(bool))
                        lstItem.IsSelected = (bool)propInfo.GetValue(value, null);
                }
            }

            return lstItem;
        }

        public void DisplayDebugInfo(string msg, bool fullItemSourceInfo, bool fullSelectionInfo)
        {
            if (fullItemSourceInfo == false && fullSelectionInfo == false)
            {
                Tools.Log(string.Format(">>>>>---> Name:{0}, InUpdate={1}. MSG: {2}", this.Name, this.m_InUpdate.ToString(), msg), Tools.LogType.Info);
            }
            else
            {
                int counter;
                Tools.Log(string.Format("----------------------------- Begin (name:{0})-- InUpdate={1} ------------------------------", this.Name, this.m_InUpdate.ToString()), Tools.LogType.Info);
                Tools.Log(msg, Tools.LogType.Info);

                if (fullItemSourceInfo)
                {
                    Tools.Log("----------------------------- Item source --------------------------------", Tools.LogType.Info);
                    //
                    //ItemSourceList
                    //
                    if (this.ItemSourceList == null)
                    {
                        Tools.Log(string.Format("ItemSourceList is null"), Tools.LogType.Info);
                    }
                    else
                    {
                        Tools.Log(string.Format("ItemSourceList is of type {0}", this.ItemSourceList.GetType()), Tools.LogType.Info);
                        counter = 0;
                        foreach (object item in this.ItemSourceList)
                        {
                            if (counter < 5)
                                Tools.Log(string.Format("ItemSourceList --> Item {0} is of type {1}", counter, item.GetType().ToString()), Tools.LogType.Info);

                            counter++;
                        }
                        Tools.Log(string.Format("ItemSourceList --> count: {0}", counter), Tools.LogType.Info);
                    }
                    //
                    //ItemSource
                    //
                    if (this.ItemsSource == null)
                    {
                        Tools.Log(string.Format("ItemsSource is null"), Tools.LogType.Info);
                    }
                    else
                    {
                        Tools.Log(string.Format("ItemsSource is of type {0}", this.ItemsSource.GetType()), Tools.LogType.Info);
                        counter = 0;
                        foreach (object item in this.ItemsSource)
                        {
                            if (counter < 5)
                                Tools.Log(string.Format("ItemsSource --> Item {0} is of type {1}", counter, item.GetType().ToString()), Tools.LogType.Info);
                            counter++;
                        }
                        Tools.Log(string.Format("ItemsSource --> count: {0}", counter), Tools.LogType.Info);
                    }
                    //
                    //Items
                    //
                    Tools.Log(string.Format("Items.Count: {0}", this.Items.Count), Tools.LogType.Info);
                    counter = 0;
                    foreach (object item in this.Items)
                    {
                        Tools.Log(string.Format("Items --> Item {0} is of type {1}", counter, item.GetType().ToString()), Tools.LogType.Info);
                        counter++;

                        if (counter == 5)
                            break;
                    }
                }

                if (fullSelectionInfo)
                {
                    Tools.Log("----------------------------- Selection Info --------------------------------", Tools.LogType.Info);
                    Tools.Log(string.Format("SelectionMode: {0}", this.SelectionMode), Tools.LogType.Info);
                    Tools.Log(string.Format("SelectedIndex: {0}", this.SelectedIndex), Tools.LogType.Info);
                    Tools.Log(string.Format("SelectedValue: {0}", this.SelectedValue), Tools.LogType.Info);
                    Tools.Log(string.Format("SelectedValuePath: {0}", this.SelectedValuePath), Tools.LogType.Info);

                    if (SelectedItem == null)
                        Tools.Log(string.Format("SelectedItem: null"), Tools.LogType.Info);
                    else
                    {
                        Tools.Log(string.Format("SelectedItem: {0}", this.SelectedItem.ToString()), Tools.LogType.Info);
                        Tools.Log(string.Format("SelectedItem value: {0}", ((NixxisAdvListBoxItem)this.SelectedItem).Item.ToString()), Tools.LogType.Info);
                    }

                    if (this.SelectedItems == null)
                        Tools.Log(string.Format("SelectedItems: null"), Tools.LogType.Info);
                    else
                    {
                        counter = 0;
                        foreach (object item in this.SelectedItems)
                        {
                            Tools.Log(string.Format("SelectedItems --> Item {0} is of type {1}", counter, item.GetType().ToString()), Tools.LogType.Info);
                            counter++;
                        }
                        Tools.Log(string.Format("SelectedItems.Count: {0}", this.SelectedItems.Count), Tools.LogType.Info);
                    }

                    if (NixxisSelectedItem == null)
                        Tools.Log(string.Format("NixxisSelectedItem: null"), Tools.LogType.Info);
                    else
                        Tools.Log(string.Format("NixxisSelectedItem: {0}", this.NixxisSelectedItem.ToString()), Tools.LogType.Info);

                    if (this.NixxisSelectedItems == null)
                        Tools.Log(string.Format("NixxisSelectedItems: null"), Tools.LogType.Info);
                    else
                    {
                        counter = 0;
                        foreach (object item in this.NixxisSelectedItems)
                        {
                            Tools.Log(string.Format("NixxisSelectedItems --> Item {0} is of type {1}", counter, item.GetType().ToString()), Tools.LogType.Info);
                            counter++;
                        }
                        Tools.Log(string.Format("NixxisSelectedItems.Count: {0}", this.NixxisSelectedItems.Count), Tools.LogType.Info);
                    }
                }
            }
        }
        #endregion

        #region Handlers

        #endregion
    }
    //
    //Nixxis treeview in combobox template & Helper
    //
    public class NixxisAdvListBoxCollection : ObservableCollection<NixxisAdvListBoxItem>
    {

        public void AddRange(IEnumerable<NixxisAdvListBoxItem> collection)
        {
            foreach (NixxisAdvListBoxItem item in collection)
                this.Add(item);
        }

        public bool ContainsItem(object value)
        {
            foreach (NixxisAdvListBoxItem item in this)
            {
                if (item.Item.Equals(value))
                    return true;
            }
            return false;
        }

        public NixxisAdvListBoxItem GetListBoxItem(object value)
        {
            foreach (NixxisAdvListBoxItem item in this)
            {
                if (item.Item.Equals(value))
                    return item;
            }
            return null;
        }

        public void SetInUpdate()
        {
            foreach (NixxisAdvListBoxItem item in this)
            {
                item.InUpdate = true;
            }
        }
    }

    public class NixxisAdvListBoxItem : INotifyPropertyChanged
    {
        #region Class data
        private string m_Text = string.Empty;
        private bool? m_IsSelected = false;
        private NixxisAdvListBox m_Owner = null;
        private object m_Item;
        private bool m_InUpdate = false;
        #endregion

        #region Constructors
        public NixxisAdvListBoxItem()
        {
        }
        #endregion

        #region Propeties
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; SetSelection(); FirePropertyChanged("Text"); }
        }
        public bool? IsSelected
        {
            get { return m_IsSelected; }
            set { m_IsSelected = value; SetSelection(); FirePropertyChanged("IsSelected"); }
        }
        public object Item
        {
            get { return m_Item; }
            set { m_Item = value; FirePropertyChanged("Item"); }
        }
        public NixxisAdvListBox Owner
        {
            get { return m_Owner; }
            set { m_Owner = value; FirePropertyChanged("Owner"); }
        }
        public bool InUpdate
        {
            get { return m_InUpdate; }
            set { m_InUpdate = value; FirePropertyChanged("InUpdate"); }
        }
        #endregion

        #region Members
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }

        private void SetSelection()
        {
            if (m_Owner != null)
            {
                if (this.IsSelected == true)
                {
                    if (!m_Owner.SelectedItems.Contains(this))
                        m_Owner.SelectedItems.Add(this);
                }
                else
                {
                    if (m_Owner.SelectedItems.Contains(this))
                        m_Owner.SelectedItems.Remove(this);
                }
            }
        }

        public void SetIsSelected(bool? value)
        {
            m_IsSelected = value;
            FirePropertyChanged("IsSelected");
        }
        #endregion
    }


}
