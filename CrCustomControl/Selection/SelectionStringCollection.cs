using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Nixxis.Windows.Controls
{
    public partial class SelectionStringCollectionV1 : UserControl
    {
        #region Events
        public event StringClickItemEventHandlerV1 ClickItem;
        public event StringBeforeAddItemEventHandlerV1 BeforeAddItem;
        #endregion

        #region Class data
        private float m_TextLabelWidth = 100F;
        private bool _IsAddVisible = true;
        private bool _IsDeleteVisible = true;
        private bool _HasMetaData = false;
        private char[] _MetaDataSeperator = new char[] { '|' };
        private StringCollection _MetaData = new StringCollection();
        
        private string m_DisplayValue = "";
        private string m_ReturnValue = "";
        private List<object> m_ObjectList = new List<object>();
        #endregion

        #region Constructor
        public SelectionStringCollectionV1()
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

        public string NewItem
        {
            get { return txtNewItem.Text; }
            set { txtNewItem.Text = value; }
        }
        public StringCollection Items
        {
            get { return GetItems(); }
            set { SetItems(value); }
        }
        public bool IsAddVisible
        {
            get { return btnAddItem.Visible; }
            set { btnAddItem.Visible = value; _IsAddVisible = value; CheckButtonColumn(); }
        }
        public bool IsDeleteVisible
        {
            get { return btnDeleteItem.Visible; }
            set { btnDeleteItem.Visible = value; _IsDeleteVisible = value; CheckButtonColumn(); }
        }
        public bool HasMetaData
        {
            get { return _HasMetaData; }
            set { _HasMetaData = value; }
        }
        public int SelectIndex
        {
            get { return lstItems.SelectedIndex; }
        }

        public string DisplayValue
        {
            get { return m_DisplayValue; }
            set { m_DisplayValue = value; }
        }
        public string ReturnValue
        {
            get { return m_ReturnValue; }
            set { m_ReturnValue = value; }
        }
        #endregion

        #region Members Event
        public void OnClickItem(string item, string metaData)
        {
            if (ClickItem != null) ClickItem(new ItemEventArgsV1(item, metaData));
        }
        public void OnBeforeAddItem(string item)
        {
            ItemEventArgsV1 ev = new ItemEventArgsV1(item, "");
            if (BeforeAddItem != null) BeforeAddItem(ev);
            AddItem(ev.Item, ev.MetaData);
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
        private StringCollection GetItems()
        {
            StringCollection returnValue = new StringCollection();

            for (int i = 0; i < lstItems.Items.Count; i++)
            {
                string value = lstItems.Items[i].ToString();
                if (_HasMetaData)
                {
                    value += _MetaDataSeperator;
                    value += _MetaData[i];
                }
                returnValue.Add(value);
            }

            return returnValue;
        }

        #region Setting datalist
        public void SetItems(string[] items)
        {
            lstItems.Items.Clear();
            if (items.Length > 0)
            {
                foreach (string item in items)
                {
                    string value = item;
                    string metaData = "";

                    if (_HasMetaData)
                    {
                        string[] val = item.Split(_MetaDataSeperator);
                        value = val[0];
                        metaData = val[1];
                    }
                    lstItems.Items.Add(value);
                    
                    if (_HasMetaData)
                        _MetaData.Add(metaData);
                }
            }
        }
        public void SetItems(string items)
        {
            string[] list = items.Split(new char[] { ';' });
            SetItems(list);
        }
        public void SetItems(StringCollection items)
        {
            if (items.Count <= 0) return;
            string[] list = new string[items.Count];
            items.CopyTo(list, 0);
            SetItems(list);
        }

        //There are two ways to use metadata:
        //The V1 old methode inline with the string data then the property HasMetadata has to be set (Is still here because of comp)
        //The V2 is working with a list of object and you tell what the return and dispaly property is.
        public void SetItems(object[] items)
        {
            lstItems.Items.Clear();
            if (items.Length > 0)
            {
                foreach (string item in items)
                {
                    string value = item;
                    string metaData = "";

                    lstItems.Items.Add(value);

                    if (_HasMetaData)
                        _MetaData.Add(metaData);
                }
            }
        }
        #endregion

        public string[] ToStringArray()
        {
            StringCollection listItems = GetItems();
            string[] returnStr = new string[listItems.Count];
            for (int i = 0; i < listItems.Count; i++)
                returnStr[i] = listItems[i];

            return returnStr;
        }
        public string ToString(string seperator)
        {
            return string.Join(seperator, ToStringArray());
        }

        public void AddItem(string item)
        {
            this.AddItem(item, "");
        }
        public void AddItem(string item, string metaData)
        {
            if (item == "") return;
            if (lstItems.Items.Contains(txtNewItem.Text)) return;

            lstItems.Items.Add(txtNewItem.Text);
            txtNewItem.Text = "";
            if (_HasMetaData)
                _MetaData.Add(metaData);
        }

        public void SetMetaData(int index, string value)
        {
            if (_HasMetaData)
            {
                if (index < _MetaData.Count && index >= 0)
                    _MetaData[index] = value;
            }
        }
        #endregion

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (txtNewItem.Text.Trim() == "") return;
            OnBeforeAddItem(txtNewItem.Text.Trim());
        }

        private void btnDeleteItem_Click(object sender, EventArgs e)
        {
            if (lstItems.SelectedIndex >= 0)
            {
                lstItems.Items.RemoveAt(lstItems.SelectedIndex);
                if (lstItems.Items.Count > 0) lstItems.SelectedIndex = 0;
            }
        }

        private void lstItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstItems.SelectedIndex == -1) return;
            string value = lstItems.Items[lstItems.SelectedIndex].ToString();
            string metaData = "";
            txtNewItem.Text = value;
            if (lstItems.SelectedIndex < _MetaData.Count)
                metaData = _MetaData[lstItems.SelectedIndex];

            OnClickItem(value, metaData);
        }
    }
}
