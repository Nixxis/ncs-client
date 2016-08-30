using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace Nixxis.Windows.Controls
{
    public partial class SelectionObjectCollection : UserControl
    {
        #region Events
        public event ObjectClickItemEventHandler ClickItem;
        public event ObjectBeforeAddItemEventHandler BeforeAddItem;
        public event ObjectAfterAddItemEventHandler AfterAddItem;
        public event ObjectDeleteItemEventHandler DeleteItem;
        public event MaxItemsReachedEventHandler MaxItems;
        #endregion

        #region Class data
        private float m_TextLabelWidth = 100F;
        private bool _IsAddVisible = true;
        private bool _IsDeleteVisible = true;
        private int m_MaxNumberOfItems = -1;        
        private string m_DisplayValue = "";
        //private SortedList<string, object> m_ObjectList_ = new SortedList<string, object>();
        private List<object> m_Objects = new List<object>();
        private List<string> m_ObjectText = new List<string>();

        private Type m_ObjectType;

        private int m_SelectedIndex = -1;
        private string m_SelectedKey = null;
        private object m_SelectedItem = null;
        private bool m_UpdateList = false;
        #endregion

        #region Constructor
        public SelectionObjectCollection()
        {
            InitializeComponent();
            lblText.Text = this.Name;
        }
        #endregion

        #region Properties
        public string TextLabel
        {
            get { return lblText.Text; }
            set { lblText.Text = value; }
        }
        public float TextLabelWidth
        {
            get { return m_TextLabelWidth; }
            set { tableLayoutPanel1.ColumnStyles[0].Width = value; m_TextLabelWidth = value; }
        }
        public ContentAlignment TextLabelAlign
        {
            get { return lblText.TextAlign; }
            set { lblText.TextAlign = value; }
        }
        public int MaxNumberOfItems
        {
            get { return m_MaxNumberOfItems; }
            set { m_MaxNumberOfItems = value; System.Diagnostics.Trace.Write(string.Format("SelectionObjectCollection property set id: {0} --> MaxNumberOfItems: {1}",this.Name ,value)); }
        }
        public string NewItem
        {
            get { return txtNewItem.Text; }
            set { txtNewItem.Text = value; }
        }
        public List<object> Items 
        {
            get { return m_Objects; }
        }
        public List<string> DisplayItem
        {
            get { return m_ObjectText; }
        }
        public bool IsAddVisible
        {
            get { return btnAddItem.Visible; }
            set { btnAddItem.Visible = value; _IsAddVisible = value; CheckButtonColumn(); System.Diagnostics.Trace.Write(string.Format("SelectionObjectCollection property set id: {0} --> IsAddVisible: {1}",this.Name ,value)); }
        }
        public bool IsDeleteVisible
        {
            get { return btnDeleteItem.Visible; }
            set { btnDeleteItem.Visible = value; _IsDeleteVisible = value; CheckButtonColumn(); System.Diagnostics.Trace.Write(string.Format("SelectionObjectCollection property set id: {0} --> IsDeleteVisible: {1}",this.Name ,value)); }
        }
        public int SelectIndex
        {
            get { return m_SelectedIndex; }
        }
        public string SelectedKey
        {
            get { return m_SelectedKey; }
        }
        public object SelectedItem
        {
            get { return m_SelectedItem; }
        }

        public string DisplayValue
        {
            get { return m_DisplayValue; }
            set { m_DisplayValue = value; }
        }
        public Type ItemType
        {
            get { return m_ObjectType; }
            set { m_ObjectType = value; }
        }
        #endregion

        #region Members Event
        public void OnClickItem(string id, object item)
        {
            if (ClickItem != null) ClickItem(new ItemInfoEventArgs(item, id));
        }
        public void OnBeforeAddItem(string id, object item)
        {
            ItemInfoEventArgs ev = new ItemInfoEventArgs(item, id);
            if (BeforeAddItem != null) BeforeAddItem(ev);
            AddItem(ev.Item);
        }
        public void OnAfterAddItem(string id, object item)
        {
            if (AfterAddItem != null) AfterAddItem(new ItemInfoEventArgs(item, id));
        }
        public void OnDeleteItem(string id, object item)
        {
            if (DeleteItem != null) DeleteItem(new ItemInfoEventArgs(item, id));
        }
        public void OnMaxItems(object item)
        {
            if (MaxItems != null) MaxItems(new ItemLimintReachedEventArgs(item, m_MaxNumberOfItems));
        }
        #endregion

        #region Members
        private void CheckButtonColumn()
        {
            if (_IsAddVisible || IsDeleteVisible)
            {
                tableLayoutPanel1.ColumnStyles[2].Width = 28F;
            }
            else
            {
                tableLayoutPanel1.ColumnStyles[2].Width = 0F;
            }
        }

        #region Item functions
        public void SetItems(object[] items)
        {
            m_Objects.Clear();
            m_ObjectText.Clear();

            if (items == null) return;
            if (items.Length <= 0) return;

            m_ObjectType = null;
            foreach (object item in items)
            {
                if (m_Objects.Count >= m_MaxNumberOfItems && m_MaxNumberOfItems > 0)
                {
                    OnMaxItems(item);
                }
                else
                {
                    if (m_Objects.Count == 0) m_ObjectType = item.GetType();

                    if (m_ObjectType == item.GetType())
                    {
                        string id = GetItemValue(item);

                        if (!m_ObjectText.Contains(id))
                        {
                            m_ObjectText.Add(id);
                            m_Objects.Add(item);
                        }
                    }
                }
            }
            DisplayItems();
        }
        public Object[] GetValues()
        {
            if (m_Objects.Count <= 0) return null;

            return m_Objects.ToArray();
        }
        public string[] GetKeys()
        {
            if (m_ObjectText.Count <= 0) return null;

            return m_ObjectText.ToArray();
        }
        public void AddItem(object item)
        {
            if (item == null) return;

            if (m_Objects.Count == 0) m_ObjectType = item.GetType();

            if (m_ObjectType == item.GetType())
            {
                if (m_Objects.Count >= m_MaxNumberOfItems && m_MaxNumberOfItems > 0)
                {
                    OnMaxItems(item);
                }
                else
                {
                    string id = GetItemValue(item);

                    if (!m_ObjectText.Contains(id))
                    {
                        m_ObjectText.Add(id);
                        m_Objects.Add(item);
                    }

                    OnAfterAddItem(id, item);
                }
            }

            txtNewItem.Text = "";

            DisplayItems();

        }
        public void DisplayItems()
        {
            m_UpdateList = true;
            int selectIndex = lstItems.SelectedIndex;
            lstItems.Items.Clear();

            foreach (object item in m_Objects)
            {
                string value = GetItemValue(item);

                lstItems.Items.Add(value);
            }
            if (selectIndex < lstItems.Items.Count) lstItems.SelectedIndex = selectIndex;
            m_UpdateList = false;
        }

        private string GetItemValue(object item)
        {
            string value = "?";
            if (item.GetType() == typeof(string))
            {
                value = item as string;
            }
            else if (string.IsNullOrEmpty(m_DisplayValue))
            {
                value = item.ToString();
            }
            else
            {
                PropertyInfo property = item.GetType().GetProperty(m_DisplayValue);
                if (property != null)
                {
                    value = property.GetValue(item, null).ToString();
                }
            }
            return value;
        }
        #endregion
        #region Datalist functions
        private void AddItemToList()
        {
            if (txtNewItem.Text.Trim() == "") return;
            object obj = null;

            if (m_ObjectType == null)
                m_ObjectType = typeof(string);

            if (m_ObjectType == typeof(string))
            {
                string val = txtNewItem.Text.Trim();
                obj = val;
            }
            else
            {
                try
                {
                    obj = Activator.CreateInstance(m_ObjectType);
                }
                catch (MissingMethodException error1)
                {
                    try
                    {
                        obj = Activator.CreateInstance(m_ObjectType, new object[] { txtNewItem.Text.Trim() });
                    }
                    catch (Exception error11)
                    {
                    }
                }
                catch (Exception error0)
                {
                }

                PropertyInfo property = obj.GetType().GetProperty(m_DisplayValue);
                
                if (property != null)
                {
                    property.SetValue(obj, txtNewItem.Text.Trim(), null);
                }
            }
            
            OnBeforeAddItem(txtNewItem.Text.Trim(), obj);
        }
        private void DeleteItemFormList()
        {
            if (lstItems.SelectedIndex >= 0)
            {
                m_UpdateList = true;
                int index = lstItems.SelectedIndex;
                string deletedId = lstItems.SelectedItem.ToString();
                int deleteIndex = m_ObjectText.Contains(deletedId) ? m_ObjectText.IndexOf(deletedId) : -1;
                object deletedObject = m_Objects[deleteIndex];

                lstItems.Items.RemoveAt(index);
                if (lstItems.Items.Count > 0) lstItems.SelectedIndex = 0;
                m_Objects.RemoveAt(deleteIndex);
                m_ObjectText.RemoveAt(deleteIndex);

                OnDeleteItem(deletedId, deletedObject);
                m_UpdateList = false;
            }
        }
        private void SelectItemFormList()
        {
            m_SelectedIndex = lstItems.SelectedIndex;
            m_SelectedItem = null;
            m_SelectedKey = null;
            if (lstItems.SelectedIndex == -1) return;
            string id = lstItems.SelectedItem.ToString();
            int index = m_ObjectText.Contains(id) ? m_ObjectText.IndexOf(id) : -1;
            object obj = m_Objects[index];

            m_SelectedKey = id;
            m_SelectedItem = obj;

            txtNewItem.Text = id;

            OnClickItem(id, obj);
        }
        #endregion

        #region Convert
        public string ToString(string seperator)
        {
            return string.Join(seperator, GetKeys());
        }
        public string[] ToStringArray()
        {
            if (m_Objects.Count <= 0) return new string[0];

            string[] list = new string[m_Objects.Count];

            for (int i = 0; i < m_Objects.Count; i++)
            {
                object item = m_Objects[i];
                if (item.GetType() == typeof(string))
                {
                    list[i] = item as string;
                }
                else
                {
                    list[i] = item.ToString();
                }
            }

            return list;
        }
        #endregion
        #endregion

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            AddItemToList();
        }
        private void btnDeleteItem_Click(object sender, EventArgs e)
        {
            DeleteItemFormList();
        }
        private void lstItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!m_UpdateList)
                SelectItemFormList();
        }
    }
}
