using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace crCustomControl
{
    public partial class SelectionStringCollection : UserControl
    {
        #region Class data
        //private string m_Text = "";
        //private string m_NewItem = "";
        //private StringCollection m_Items = new StringCollection();
        private bool m_AutoSize = true;
        private float m_TextWidth = 100F;
        #endregion

        #region Constructor
        public SelectionStringCollection()
        {
            InitializeComponent();
            //m_Text = this.Name;
            lblText.Text = this.Name;
        }
        #endregion

        #region Properties
        public string Text
        {
            get { return lblText.Text; }
            set { lblText.Text = value; SetTextWidth(); }
        }
        public float TextWidth
        {
            get { return m_TextWidth; }
            set { m_TextWidth = value; SetTextWidth(); }
        }
        public ContentAlignment TextAlign
        {
            get { return lblText.TextAlign; }
            set { lblText.TextAlign = value; }
        }
        public DockStyle TextDock
        {
            get { return lblText.Dock; }
            set { lblText.Dock = value; }
        }
        public bool TextAutoSize
        {
            get { return m_AutoSize; }
            set { m_AutoSize = value; SetTextWidth(); }
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
            set { btnAddItem.Visible = value; }
        }
        public bool IsDeleteVisible
        {
            get { return btnDeleteItem.Visible; }
            set { btnDeleteItem.Visible = value; }
        }
        #endregion

        #region Members
        private StringCollection GetItems()
        {
            StringCollection returnValue = new StringCollection();

            //for (int i = 0; i < lstItems.Items.Count; i++)
            //    returnValue.Add(lstItems.Items[i]);

            return returnValue;
        }
        private void SetItems(StringCollection items)
        {
            lstItems.Items.Clear();
            foreach (string item in items)
                lstItems.Items.Add(item);
        }
        private void SetTextWidth()
        {
            if (m_AutoSize)
            {

            }
            else
            {
                tableLayoutPanel1.ColumnStyles[0].Width = m_TextWidth;
            }
        }
        #endregion
    }
}
