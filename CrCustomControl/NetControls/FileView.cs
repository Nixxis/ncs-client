using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Nixxis.Windows.Controls
{
    public partial class FileView : UserControl
    {
        #region Events
        public event FileViewOnSelectEventHandler SelectFile;
        private void OnSelect(string path)
        {
            if (SelectFile != null) SelectFile(path);
        }
        #endregion
        
        #region Class data

        private string m_Folder;
        private string m_SearchPattern = "*";
        private int m_ImageIndex = -1;
        private ImageList m_ImageList;
        private bool m_FullName = false;

        
        #endregion
        
        #region Constructor
        public FileView()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties
        public string Folder
        {
            get { return m_Folder; }
            set { m_Folder = value;}
        }
        public string SearchPattern
        {
            get { return m_SearchPattern; }
            set { m_SearchPattern = value; }
        }
        public int ImageIndex
        {
            get { return m_ImageIndex; }
            set { m_ImageIndex = value; }
        }
        public ImageList ImageList
        {
            get { return m_ImageList; }
            set { m_ImageList = value; }
        }
        public bool FullName
        {
            get { return m_FullName; }
            set { m_FullName = value; }
        }
        #endregion



        #region Members
        public void DisplayList()
        {
            DisplayFileList();
        }

        private void DisplayFileList()
        {
            trvFile.Nodes.Clear();

            if (!Directory.Exists(m_Folder)) return;

            string[] files = Directory.GetFiles(m_Folder, m_SearchPattern);

            foreach (string file in files)
            {
                if (m_FullName)
                    trvFile.Nodes.Add(file, file, m_ImageIndex);
                else
                    trvFile.Nodes.Add(file, Path.GetFileName(file), m_ImageIndex);
            }
        }
        #endregion

        private void trvFile_AfterSelect(object sender, TreeViewEventArgs e)
        {
            OnSelect(e.Node.Name);
        }
    }
}
