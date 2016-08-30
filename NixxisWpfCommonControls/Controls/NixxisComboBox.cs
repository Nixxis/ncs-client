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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;

namespace Nixxis.Client.Controls
{
    public class NixxisComboBox : ComboBox
    {
        #region Class data
        private int m_ProcessLevel = -1;
        #endregion

        #region Properies
        private IList<NixxisComboBoxItem> m_SelectedItemsList;
        public IList<NixxisComboBoxItem> SelectedItemsList
        {
            get { return m_SelectedItemsList; }
            set { m_SelectedItemsList = value; }
        }
        #endregion

        #region Properties XAML
        public static readonly DependencyProperty IsHierarchicalListProperty = DependencyProperty.Register("IsHierarchicalList", typeof(bool), typeof(NixxisComboBox), new FrameworkPropertyMetadata(true));
        public bool IsHierarchicalList
        {
            get { return (bool)GetValue(IsHierarchicalListProperty); }
            private set { SetValue(IsHierarchicalListProperty, value); }
        }

        public static readonly DependencyProperty ItemSingleListProperty = DependencyProperty.Register("ItemSingleList", typeof(IEnumerable<object>), typeof(NixxisComboBox), new FrameworkPropertyMetadata(ItemSingleListPropertyChanged));
        public IEnumerable<object> ItemSingleList
        {
            get { return (IEnumerable<object>)GetValue(ItemSingleListProperty); }
            set { SetValue(ItemSingleListProperty, value); }
        }
        public static void ItemSingleListPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisComboBox cbo = (NixxisComboBox)obj;
            cbo.IsHierarchicalList = false;
            cbo.ItemList = args.NewValue as IEnumerable<object>;
        }



        public static readonly DependencyProperty ItemListProperty = DependencyProperty.Register("ItemList", typeof(IEnumerable<object>), typeof(NixxisComboBox), new FrameworkPropertyMetadata(ItemListPropertyChanged));
        public IEnumerable<object> ItemList
        {
            get { return (IEnumerable<object>)GetValue(ItemListProperty); }
            set { SetValue(ItemListProperty, value); }
        }
        public static void ItemListPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisComboBox cbo = (NixxisComboBox)obj;
            cbo.SetItemSource();
        }

        public static readonly DependencyProperty ItemListChildrenPropertyProperty = DependencyProperty.Register("ItemListChildrenProperty", typeof(string), typeof(NixxisComboBox), new FrameworkPropertyMetadata(ItemListChildrenPropertyChanged));
        public string ItemListChildrenProperty
        {
            get { return (string)GetValue(ItemListChildrenPropertyProperty); }
            set { SetValue(ItemListChildrenPropertyProperty, value); }
        }
        public static void ItemListChildrenPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisComboBox cbo = (NixxisComboBox)obj;
            cbo.SetItemSource();
        }

        public static readonly DependencyProperty ItemListIdPropertyProperty = DependencyProperty.Register("ItemListIdProperty", typeof(string), typeof(NixxisComboBox), new FrameworkPropertyMetadata(ItemListIdPropertyChanged));
        public string ItemListIdProperty
        {
            get { return (string)GetValue(ItemListIdPropertyProperty); }
            set { SetValue(ItemListIdPropertyProperty, value); }
        }
        public static void ItemListIdPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisComboBox cbo = (NixxisComboBox)obj;
            cbo.SetItemSource();
        }

        public static readonly DependencyProperty ItemListDescriptionPropertyProperty = DependencyProperty.Register("ItemListDescriptionProperty", typeof(string), typeof(NixxisComboBox), new FrameworkPropertyMetadata(ItemListDescriptionPropertyChanged));
        public string ItemListDescriptionProperty
        {
            get { return (string)GetValue(ItemListDescriptionPropertyProperty); }
            set { SetValue(ItemListDescriptionPropertyProperty, value); }
        }
        public static void ItemListDescriptionPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisComboBox cbo = (NixxisComboBox)obj;
            cbo.SetItemSource();
        }

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", typeof(NixxisComboBoxItem[]), typeof(NixxisComboBox));
        public NixxisComboBoxItem[] SelectedItems
        {
            get { return (NixxisComboBoxItem[])GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty ExpandImageProperty = DependencyProperty.Register("ExpandImage", typeof(ImageSource), typeof(NixxisComboBox), new PropertyMetadata(null, new PropertyChangedCallback(ExpandImageChanged)));
        public ImageSource ExpandImage
        {
            get { return GetValue(ExpandImageProperty) as ImageSource; }
            set { SetValue(ExpandImageProperty, value); }
        }
        public static void ExpandImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty CollapseImageProperty = DependencyProperty.Register("CollapseImage", typeof(ImageSource), typeof(NixxisComboBox), new PropertyMetadata(null, new PropertyChangedCallback(CollapseImageChanged)));
        public ImageSource CollapseImage
        {
            get { return GetValue(CollapseImageProperty) as ImageSource; }
            set { SetValue(CollapseImageProperty, value); }
        }
        public static void CollapseImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty MultiSelectionProperty = DependencyProperty.Register("MultiSelection", typeof(bool), typeof(NixxisComboBox), new PropertyMetadata(false, new PropertyChangedCallback(MultiSelectionChanged)));
        public bool MultiSelection
        {
            get { return (bool)GetValue(MultiSelectionProperty); }
            set { SetValue(MultiSelectionProperty, value); }
        }
        public static void MultiSelectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        public static readonly DependencyProperty SelectedTextProperty = DependencyProperty.Register("SelectedText", typeof(string), typeof(NixxisComboBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedTextChanged));
        public string SelectedText
        {
            get { return (string)GetValue(SelectedTextProperty); }
            set { SetValue(SelectedTextProperty, value); }
        }
        public static void SelectedTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisComboBox cbo = (NixxisComboBox)obj;

        }

        public static readonly DependencyProperty SelectedTextSeparatorProperty = DependencyProperty.Register("SelectedTextSeparator", typeof(string), typeof(NixxisComboBox), new FrameworkPropertyMetadata(";", SelectedTextSeparatorChanged));
        public string SelectedTextSeparator
        {
            get { return (string)GetValue(SelectedTextSeparatorProperty); }
            set { SetValue(SelectedTextSeparatorProperty, value); }
        }
        public static void SelectedTextSeparatorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            NixxisComboBox cbo = (NixxisComboBox)obj;
            cbo.ItemCheckChanged(null);
        }

        public static readonly DependencyProperty SelectedItemsIdProperty = DependencyProperty.Register("SelectedItemsId", typeof(string), typeof(NixxisComboBox), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string SelectedItemsId
        {
            get { return (string)GetValue(SelectedItemsIdProperty); }
            set { SetValue(SelectedItemsIdProperty, value); }
        }
        #endregion

        #region Static
        static NixxisComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NixxisComboBox), new FrameworkPropertyMetadata(typeof(NixxisComboBox)));
        }
        #endregion

        #region Members override
        public override void BeginInit()
        {
            base.BeginInit();
            this.m_SelectedItemsList = new List<NixxisComboBoxItem>();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (e.AddedItems != null)
            {
                foreach (object item in e.AddedItems)
                {
                    if (item.GetType() == typeof(NixxisComboBoxItem))
                    {
                        NixxisComboBoxItem cboItem = (NixxisComboBoxItem)item;
                        
                        if (cboItem.IsSelected == false)
                            cboItem.IsSelected = true;
                    }
                }
            }

            if (this.MultiSelection == false)
            {
                if (e.RemovedItems != null)
                {
                    foreach (object item in e.RemovedItems)
                    {
                        if (item.GetType() == typeof(NixxisComboBoxItem))
                        {
                            NixxisComboBoxItem cboItem = (NixxisComboBoxItem)item;

                            if (cboItem.IsSelected == true)
                                cboItem.IsSelected = false;
                        }
                    }
                }
            }
        }
        #endregion

        #region Members
        /// <summary>
        /// To set the itemsource from the itemlistsource
        /// </summary>
        private void SetItemSource()
        {
            if (this.ItemList == null)
                return;

            this.ItemsSource = null;

            if (IsHierarchicalList && string.IsNullOrEmpty(this.ItemListChildrenProperty))
                return;
            if (string.IsNullOrEmpty(this.ItemListDescriptionProperty))
                return;
            if (string.IsNullOrEmpty(this.ItemListIdProperty))
                return;

            m_ProcessLevel = -1;
            NixxisComboBoxList lst = ProcessNode(this.ItemList, null);
            this.ItemsSource = lst;
        }
        
        private NixxisComboBoxList ProcessNode(IEnumerable<object> collection, NixxisComboBoxItem parent)
        {
            m_ProcessLevel++;
            NixxisComboBoxList list = new NixxisComboBoxList();

            foreach (object item in collection)
            {
                NixxisComboBoxItem cboItem = new NixxisComboBoxItem();
                PropertyInfo info = null;
                NixxisComboBoxList childrenList = null; 
                
                cboItem.Level = m_ProcessLevel;
                cboItem.ReturnObject = item;
                cboItem.Owner = this;

                info = item.GetType().GetProperty(this.ItemListIdProperty);
                if (info != null)
                    cboItem.Id = info.GetValue(item, null).ToString();

                info = item.GetType().GetProperty(this.ItemListDescriptionProperty);
                if (info != null)
                    cboItem.Description = info.GetValue(item, null).ToString();

                if (IsHierarchicalList)
                {
                    info = item.GetType().GetProperty(this.ItemListChildrenProperty);
                    if (info != null)
                    {
                        IEnumerable<object> children = (IEnumerable<object>)info.GetValue(item, null);
                        if (children != null)
                            childrenList = ProcessNode(children, cboItem);
                        if (childrenList != null && childrenList.Count > 0)
                            cboItem.HasChildren = true;
                    }
                }
                else
                {
                    info = null;
                }

                
                cboItem.Parent = parent;
                list.Add(cboItem);
                if (childrenList != null && childrenList.Count > 0)
                    list.AddRange(childrenList);
            }
            m_ProcessLevel--;
            return list;
        }

        private int m_ItemChecking = 0;
        internal void ItemCheckChanged(NixxisComboBoxItem item)
        {
            if(this.m_SelectedItemsList == null)
                this.m_SelectedItemsList = new List<NixxisComboBoxItem>();

            m_ItemChecking++;
            if (item != null)
            {
                if (item.IsSelected == true)
                {
                    if (item.HasChildren)
                    {
                        if (this.ItemsSource.GetType() == typeof(NixxisComboBoxList))
                        {
                            NixxisComboBoxList lst = (NixxisComboBoxList)this.ItemsSource;
                            int index = lst.IndexOf(item);

                            for (int i = index + 1; i < lst.Count; i++)
                            {
                                if (lst[i].Level <= item.Level)
                                    break;
                                else if (lst[i].Parent == item)
                                {
                                    if (lst[i].IsSelected == false)
                                        lst[i].IsSelected = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!this.m_SelectedItemsList.Contains(item))
                        {
                            object oldValue = this.m_SelectedItemsList;
                            IList<NixxisComboBoxItem> currentValue = this.m_SelectedItemsList;

                            this.m_SelectedItemsList.Add(item);
                            this.SelectedItems = m_SelectedItemsList.ToArray<NixxisComboBoxItem>();
                        }
                    }
                }
                else if (item.IsSelected == false)
                {
                    if (item.HasChildren)
                    {
                        if (this.ItemsSource.GetType() == typeof(NixxisComboBoxList))
                        {
                            NixxisComboBoxList lst = (NixxisComboBoxList)this.ItemsSource;
                            int index = lst.IndexOf(item);

                            for (int i = index + 1; i < lst.Count; i++)
                            {
                                if (lst[i].Level <= item.Level)
                                    break;
                                else if (lst[i].Parent == item)
                                {
                                    if (lst[i].IsSelected == true)
                                        lst[i].IsSelected = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (this.m_SelectedItemsList.Contains(item))
                        {
                            this.m_SelectedItemsList.Remove(item);
                            this.SelectedItems = m_SelectedItemsList.ToArray<NixxisComboBoxItem>();
                        }
                    }
                }

                if (m_ItemChecking == 1)
                {
                    //Checking state for parent checkbox
                    NixxisComboBoxItem currentItem = item.Parent;

                    while (currentItem != null)
                    {
                        bool? parentState = CheckItemState(currentItem);

                        if (currentItem.IsSelected != parentState)
                            currentItem.IsSelected = parentState;

                        currentItem = currentItem.Parent;
                    }

                    //Chekking selectedindex and item
                    if (item.IsSelected == true)
                    {
                        this.SelectedItem = item;
                    }
                    else if (item.IsSelected == false)
                    {
                        if (this.m_SelectedItemsList.Count > 0)
                            this.SelectedItem = this.m_SelectedItemsList[0];
                        else
                            this.SelectedItem = null;
                    }
                }
            }

            string newText = GetSelectedText();

            if(this.SelectedText == null || !this.SelectedText.Equals(newText))
                this.SelectedText = newText;

            m_ItemChecking--;
        }

        private bool? CheckItemState(NixxisComboBoxItem item)
        {
            bool HasTrueItem = false;
            bool HasFalseItem = false;

            NixxisComboBoxList lst = (NixxisComboBoxList)this.ItemsSource;
            int index = lst.IndexOf(item);

            for (int i = index + 1; i < lst.Count; i++)
            {
                if (lst[i].Level < item.Level)
                    break;
                else if (lst[i].Parent == item)
                {
                    if (lst[i].IsSelected == true)
                    {
                        HasTrueItem = true;
                        if(HasFalseItem)
                            break;
                    }
                    else if (lst[i].IsSelected == false)
                    {
                        HasFalseItem = true;
                        if (HasTrueItem)
                            break;
                    }
                    else if (lst[i].IsSelected == null)
                    {
                        HasFalseItem = true;
                        HasTrueItem = true;
                        break;
                    }
                }
            }

            if(HasTrueItem && HasFalseItem)
                return null;
            else if (!HasTrueItem)
                return false;
            else
                return true;
        }
        private string GetSelectedText()
        {
            string rtn = string.Empty;
            string rtnIds = string.Empty;

            foreach (NixxisComboBoxItem item in this.m_SelectedItemsList)
            {
                if (string.IsNullOrEmpty(rtn))
                {
                    rtn = item.Description;
                    rtnIds = item.Id;
                }
                else
                {
                    rtn = rtn + this.SelectedTextSeparator + " " + item.Description;
                    rtnIds = rtnIds + this.SelectedTextSeparator + item.Id;
                }
            }
            if (!SelectedItemsIdNotificationStopped)
                this.SelectedItemsId = rtnIds;
            return rtn;
        }
        #endregion

        #region Handlers

        #endregion


        private bool SelectedItemsIdNotificationStopped = false;
        public void StopSelectedItemsIdChangeNotifications()
        {
            SelectedItemsIdNotificationStopped = true;
        }

        public void DoSelectedItemsIdChangeNotifications()
        {
            SelectedItemsIdNotificationStopped = false;
            GetSelectedText();
        }


    }

    //
    //Nixxis treeview in combobox template & Helper
    //
    public class NixxisComboBoxList : ObservableCollection<NixxisComboBoxItem>
    {
        public void Sort(Comparison<NixxisComboBoxItem> comparison)
        {
            List<NixxisComboBoxItem> list = this.ToList<NixxisComboBoxItem>();
            list.Sort(comparison);

            this.Clear();
            foreach (NixxisComboBoxItem item in list)
                this.Add(item);
        }
        public void Sort(IComparer<NixxisComboBoxItem> comparer)
        {
            List<NixxisComboBoxItem> list = this.ToList<NixxisComboBoxItem>();
            list.Sort(comparer);

            this.Clear();
            foreach (NixxisComboBoxItem item in list)
                this.Add(item);
        }

        public void AddRange(IEnumerable<NixxisComboBoxItem> collection)
        {
            foreach (NixxisComboBoxItem item in collection)
                this.Add(item);
        }
    }

    public class NixxisComboBoxItem : INotifyPropertyChanged
    {
        #region Class data
        private string m_Id;
        private string m_Description;
        private int m_Level;
        private bool m_IsExpanded = true; 
        private bool m_IsVisible = true;
        private bool? m_IsSelected = false;
        private bool m_HasChildren = false;
        private NixxisComboBoxItem m_Parent;
        private object m_ReturnObject;
        private NixxisComboBox m_Owner = null;
        #endregion

        #region Constructors
        public NixxisComboBoxItem()
        {
            TreeViewItem e = new TreeViewItem();
            ComboBoxItem i = new ComboBoxItem();
            CheckBox c = new CheckBox();
            ListBoxItem l = new ListBoxItem();
        }
        public NixxisComboBoxItem(string id, string description, int level)
        {
            m_Id = id;
            m_Description = description;
            m_Level = level;
        }
        #endregion

        #region Propeties
        public string Id
        {
            get { return m_Id; }
            set { m_Id = value; FirePropertyChanged("Id"); }
        }
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; FirePropertyChanged("Description"); }
        }
        public int Level
        {
            get { return m_Level; }
            set { m_Level = value; FirePropertyChanged("Level"); }
        }
        public bool IsExpanded
        {
            get { return m_IsExpanded; }
            set { m_IsExpanded = value; FirePropertyChanged("IsExpanded"); }
        }
        public bool IsVisible
        {
            get { return m_IsVisible; }
            set { m_IsVisible = value; FirePropertyChanged("IsVisible"); }
        }
        public bool? IsSelected
        {
            get { return m_IsSelected; }
            set { m_IsSelected = value; SetSelection(); FirePropertyChanged("IsSelected"); }
        }
        public bool HasChildren
        {
            get { return m_HasChildren; }
            set { m_HasChildren = value; FirePropertyChanged("HasChildren"); }
        }
        public NixxisComboBoxItem Parent
        {
            get { return m_Parent; }
            set 
            {
                m_Parent = value; 
                if(value != null)
                    m_Parent.PropertyChanged += new PropertyChangedEventHandler(m_Parent_PropertyChanged); 
                FirePropertyChanged("Parent"); 
            }
        }
        public object ReturnObject
        {
            get { return m_ReturnObject; }
            set { m_ReturnObject = value; FirePropertyChanged("ReturnObject"); }
        }
        public NixxisComboBox Owner
        {
            get { return m_Owner; }
            set { m_Owner = value; FirePropertyChanged("Owner"); }
        }
        #endregion

        #region Members
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }

        private void m_Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsExpanded"))
                this.IsVisible = this.Parent.IsExpanded;
            else if (e.PropertyName.Equals("IsVisible"))
            {
                if (this.Parent.IsExpanded)
                    this.IsVisible = this.Parent.IsVisible;
            }
        }

        private void SetSelection()
        {
            if(m_Owner != null)
                m_Owner.ItemCheckChanged(this);
        }
        #endregion
    }
}
