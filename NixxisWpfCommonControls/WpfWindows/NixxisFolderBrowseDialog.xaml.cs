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
using System.Windows.Shapes;
using System.IO;

namespace Nixxis.Client.Controls
{
    //TO DO: to allow some action like new folder, remove folder, rename folder, ...
    /// <summary>
    /// Interaction logic for NixxisFolderBrowseDialog.xaml
    /// </summary>
    public partial class NixxisFolderBrowseDialog : Window
    {
        #region Enums
        public enum SpecialFolders : int
        {
            Desktop = 0,
            Documents = 1,
            Pictures = 2,
            Videos = 3,
            Computer = 4,
            Music = 5,
            StartMenu = 6,
            ProgramFiles = 7,
            programFilesX86 = 8
        }
        #endregion

        #region Class data
        private Dictionary<SpecialFolders, string> m_SpecialFolders = new Dictionary<SpecialFolders,string>();
        #endregion

        #region Constructors
        public NixxisFolderBrowseDialog()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
                FlowDirection = System.Windows.FlowDirection.RightToLeft;
            else
                FlowDirection = System.Windows.FlowDirection.LeftToRight;

            InitializeComponent();
            
            #region Init SpecialFolders
            m_SpecialFolders.Add(SpecialFolders.Desktop, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            m_SpecialFolders.Add(SpecialFolders.Documents, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            m_SpecialFolders.Add(SpecialFolders.Pictures, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            m_SpecialFolders.Add(SpecialFolders.Videos, Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
            m_SpecialFolders.Add(SpecialFolders.Computer, Environment.GetFolderPath(Environment.SpecialFolder.MyComputer));
            m_SpecialFolders.Add(SpecialFolders.Music, Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            m_SpecialFolders.Add(SpecialFolders.StartMenu, Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            m_SpecialFolders.Add(SpecialFolders.ProgramFiles, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            m_SpecialFolders.Add(SpecialFolders.programFilesX86, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            #endregion

            this.RootFolder = m_SpecialFolders[SpecialFolders.Desktop];
            this.SelectedPath = string.Empty; 
            
            this.TextForComputer = "Computer";
            this.TextForNewFolder = "New Folder";
            this.TextForRename = "Rename";
            this.TextForDelete = "Delete";
            this.TreeDepth = 0;
            this.ShowEmptyFolders = true;
            this.ShowHiddenFolders = true;
        }
        #endregion

        #region Properties
        public string SelectedPath { get; set; }
        public string RootFolder { get; set; }        
        public string TextForComputer { get; set; }
        public string TextForNewFolder { get; set; }
        public string TextForRename { get; set; }
        public string TextForDelete { get; set; }
        public int TreeDepth { get; set; }
        public bool ShowEmptyFolders { get; set; }
        public bool ShowHiddenFolders { get; set; }
        #endregion

        #region Members Override
        public new bool? ShowDialog()
        {
            LoadFolders();

            this.treeViewFolders.Focus();
            return base.ShowDialog();
        }
        
        #endregion

        #region Members
        private void LoadFolders()
        {
            this.treeViewFolders.Items.Clear();

            if (!Directory.Exists(this.RootFolder))
            {
                Exception e = new Exception("The Initial Path doesn't exist.");
                throw e;
            }
            if (this.SelectedPath != "")
            {
                if (!Directory.Exists(this.SelectedPath))
                {
                    Exception e = new Exception("The Selected Path doesn't exist.");
                    throw e;
                }
                if (!IsSelectedPathInRootPath())
                {
                    Exception e = new Exception("The Selected Path isn't part of the Initial Path.");
                    throw e;
                }
            }
            treeViewFolders.Items.Add(LoadDirectories(this.RootFolder));
            ((TreeViewItem)treeViewFolders.Items[0]).IsExpanded = true;
            SelectPath();
        }
        private TreeViewItem LoadDirectories(string path)
        {
            TreeViewItem tvi = CreateTreeVieItem(path);
            tvi.Items.Clear();
            if (this.RootFolder == m_SpecialFolders[SpecialFolders.Desktop])
            {
                LoadDesktop(tvi);
            }
            string[] folders = Directory.GetDirectories(path);
            for (int i = 0; i < folders.Length; i++)
            {
                try
                {
                    if (this.ShowEmptyFolders != true)
                    {
                        if (!IsFolderEmpty(folders[i]))
                            tvi.Items.Add(CreateTreeVieItem(folders[i]));
                    }
                    else if (this.ShowHiddenFolders != true)
                    {
                        if (!IsFolderHidden(folders[i]))
                            tvi.Items.Add(CreateTreeVieItem(folders[i]));
                    }
                    else
                        tvi.Items.Add(CreateTreeVieItem(folders[i]));
                }
                catch (Exception e) //If there are no rights for the folder, bad solution but best now
                { }
            }
            return tvi;
        }
        private TreeViewItem CreateTreeVieItem(string path)
        {
            TreeViewItem tvi = new TreeViewItem();
            string header = path.Remove(0, path.LastIndexOf("\\") + 1);
            if (header == "") //The Path is a Drive (e.g. C\, after removing \ -> "")
            {
                header = path.Remove(path.Length - 1);
            }
            tvi.Header = header;
            tvi.ToolTip = path;
            tvi.MouseRightButtonDown += new MouseButtonEventHandler(TreeViewItems_MouseRightButtonDown);
            if (this.TreeDepth == 0) //Without depth
            {
                if (path != "")
                {
                    if (Directory.GetDirectories(path).Length > 0)
                    {
                        tvi.Items.Add(new TreeViewItem());
                        tvi.Expanded += new RoutedEventHandler(Tvi_Expanded);
                        tvi.Selected += new RoutedEventHandler(Tvi_Selected);
                    }
                }
            }
            else //with depth
            {
                if (path != "" && GetFolderDepth(path) < this.TreeDepth)
                {
                    if (Directory.GetDirectories(path).Length > 0)
                    {
                        tvi.Items.Add(new TreeViewItem());
                        tvi.Expanded += new RoutedEventHandler(Tvi_Expanded);
                        tvi.Selected += new RoutedEventHandler(Tvi_Selected);
                    }
                }
                else
                {
                    tvi.Expanded += new RoutedEventHandler(DoNothing_Event);
                    tvi.Selected += new RoutedEventHandler(DoNothing_Event);
                }
            }
            return tvi;
        }
        private TreeViewItem GetTreeViewItem(TreeViewItem actual, string subItemName)
        {
            for (int i = 0; i < actual.Items.Count; i++)
            {
                if (((TreeViewItem)actual.Items[i]).Header.ToString().Equals(subItemName, StringComparison.InvariantCultureIgnoreCase))
                    return (TreeViewItem)actual.Items[i];
            }
            return null;
        }
        private void LoadDesktop(TreeViewItem tvi)
        {
            tvi.Header = m_SpecialFolders[SpecialFolders.Desktop].Remove(0, m_SpecialFolders[SpecialFolders.Desktop].LastIndexOf("\\") + 1);
            tvi.ToolTip = m_SpecialFolders[SpecialFolders.Desktop];
            tvi.Items.Add(CreateTreeVieItem(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
            TreeViewItem computer = new TreeViewItem();
            computer.Header = "Computer"; //To check this.TextForComputer;
            computer.Expanded += new RoutedEventHandler(Computer_Expanded);
            computer.Selected += new RoutedEventHandler(Tvi_Selected);
            computer.Items.Add("");
            tvi.Items.Add(computer);
        }

        private ContextMenu CreateContextMenu(bool drive = false)
        {
            ContextMenu cm = new ContextMenu();
            MenuItem mi = new MenuItem();
            mi.Header = this.TextForNewFolder;
            mi.Click += new RoutedEventHandler(ButtonNewFolder_Click);
            cm.Items.Add(mi);
            if (!drive)
            {
                mi = new MenuItem();
                mi.Header = this.TextForRename;
                mi.Click += new RoutedEventHandler(Rename_Click);
                cm.Items.Add(mi);
                mi = new MenuItem();
                mi.Header = this.TextForDelete;
                mi.Click += new RoutedEventHandler(Delete_Click);
                cm.Items.Add(mi);
            }
            return cm;
        }

        private int GetFolderDepth(string path)
        {
            int depth = GetFolderDefaultDepth(path);
            if (this.RootFolder == m_SpecialFolders[SpecialFolders.Desktop])
            {
                if (path.Contains(m_SpecialFolders[SpecialFolders.Desktop])) //Desktop->
                    depth -= GetFolderDefaultDepth(m_SpecialFolders[SpecialFolders.Desktop]);
                else if (path.Contains(m_SpecialFolders[SpecialFolders.Documents])) //Desktop->Documents->
                {
                    depth -= GetFolderDefaultDepth(m_SpecialFolders[SpecialFolders.Documents]);
                    if (path == m_SpecialFolders[SpecialFolders.Documents]) // + Desktop
                        depth += 1;
                    else // + Desktop + Documents
                        depth += 2;
                }
                else if (path.Length == 3) // all drives: C:\, D:\, etc..
                    depth = 3;
                else
                    depth -= 3; // Desktop->Computer->Drive->
            }
            else
            {
                depth -= GetFolderDefaultDepth(this.RootFolder);
                depth += 1; // + the Root of TreeView
            }
            return depth;
        }
        private int GetFolderDefaultDepth(string path)
        {
            return path.Split('\\').Length;
        }

        private void SelectPath()
        {
            if (this.SelectedPath != "" && this.SelectedPath != this.RootFolder)
            {
                string[] selected = this.SelectedPath.Split('\\');
                TreeViewItem actual = (TreeViewItem)this.treeViewFolders.Items[0];
                int startindex = 0; //if desktop
                if (this.RootFolder != m_SpecialFolders[SpecialFolders.Desktop])
                {
                    string[] initial = this.RootFolder.Split('\\');
                    startindex = selected.Length - initial.Length;
                    startindex += 2; //to correct the substraction
                }
                else
                {
                    actual = GetTreeViewItem(actual, "Computer");
                    actual.IsExpanded = true;
                }
                for (int i = startindex; i < selected.Length; i++)
                {
                    actual = GetTreeViewItem(actual, selected[i]);
                    actual.IsExpanded = true;
                }
                try
                {
                    actual.IsSelected = true;
                }
                catch
                {
                    try
                    {
                        actual.IsSelected = true;
                    }
                    catch { }
                }


                this.buttonNewFolder.IsEnabled = true;
            }
            else if (this.SelectedPath == this.RootFolder)
            {
                TreeViewItem actual = (TreeViewItem)this.treeViewFolders.Items[0];
                actual.IsSelected = true;
                actual.IsExpanded = true;
            }
        }
        
        private bool IsSelectedPathInRootPath()
        {
            if (this.RootFolder != m_SpecialFolders[SpecialFolders.Desktop])
            {
                string[] initial = this.RootFolder.Split('\\');
                string[] selected = this.SelectedPath.Split('\\');
                for (int i = 0; i < initial.Length; i++)
                {
                    if (selected[i] != initial[i])
                        return false;
                }
            }
            return true;
        }
        private bool IsFolderEmpty(string path)
        {
            if (Directory.GetDirectories(path).Length > 0)
                return false;
            if (Directory.GetFiles(path).Length > 0)
                return false;
            return true;
        }
        private bool IsFolderHidden(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Attributes.ToString().Contains(FileAttributes.Hidden.ToString()))
                return true;

            return false;
        }

        /// <summary>
        /// getNativeDepth (gND)
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>the depth of the path in the filesystem</returns>
        
        #endregion

        private void treeViewFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (((TreeViewItem)this.treeViewFolders.SelectedItem).ToolTip == null)
                    this.buttonNewFolder.IsEnabled = false;
                else
                {
                    this.SelectedPath = ((TreeViewItem)this.treeViewFolders.SelectedItem).ToolTip.ToString();
                    this.buttonNewFolder.IsEnabled = true;
                }
   
            }
            catch
            {
                this.buttonNewFolder.IsEnabled = false;
            }
            e.Handled = true;
        }
        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
        private void ButtonNewFolder_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void TreeViewItems_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem tvi = (TreeViewItem)sender;
            tvi.IsSelected = true;
            e.Handled = true;
        }
        private void Tvi_Selected(object sender, RoutedEventArgs e)
        {
            var tvi = e.OriginalSource as TreeViewItem;
            if (tvi.Items.Count <= 1 && !tvi.IsExpanded)
                tvi.IsExpanded = true;
            e.Handled = true;
        }
        private void Tvi_Expanded(object sender, RoutedEventArgs e)
        {
            var tvi = e.OriginalSource as TreeViewItem;
            if (tvi.Items.Count <= 1 && tvi.IsExpanded)
            {
                tvi.Items.Clear();
                string[] folders = Directory.GetDirectories(tvi.ToolTip.ToString());
                for (int i = 0; i < folders.Length; i++)
                {
                    try
                    {
                        if (this.ShowEmptyFolders != true)
                        {
                            if (!IsFolderEmpty(folders[i]))
                                tvi.Items.Add(CreateTreeVieItem(folders[i]));
                        }
                        else if (this.ShowHiddenFolders != true)
                        {
                            if (!IsFolderHidden(folders[i]))
                                tvi.Items.Add(CreateTreeVieItem(folders[i]));
                        }
                        else
                            tvi.Items.Add(CreateTreeVieItem(folders[i]));
                    }
                    catch (Exception ex) //If there are no rights for the folder, bad solution but best now
                    { }
                }
            }
            e.Handled = true;
        }
        private void Computer_Expanded(object sender, RoutedEventArgs e)
        {
            var tvi = GetTreeViewItem((TreeViewItem)this.treeViewFolders.Items[0], "Computer");
            if (tvi.Items.Count <= 1 && tvi.IsExpanded)
            {
                tvi.Items.Clear();
                string[] folders = Directory.GetLogicalDrives();
                for (int i = 0; i < folders.Length; i++)
                {
                    try
                    {
                        if (this.ShowEmptyFolders != true)
                        {
                            if (!IsFolderEmpty(folders[i]))
                                tvi.Items.Add(CreateTreeVieItem(folders[i]));
                        }
                        else if (this.ShowHiddenFolders != true)
                        {
                            if (!IsFolderHidden(folders[i]))
                                tvi.Items.Add(CreateTreeVieItem(folders[i]));
                        }
                        else
                            tvi.Items.Add(CreateTreeVieItem(folders[i]));
                    }
                    catch (Exception ex) //The Drive is not ready, bad solution but best now
                    { }
                }
            }
            e.Handled = true;
        }
        private void DoNothing_Event(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.treeViewFolders.SelectedItem != null)
            {
                try
                {
                    this.SelectedPath = ((TreeViewItem)this.treeViewFolders.SelectedItem).ToolTip.ToString();
                }
                catch (Exception ex) //Throwing exception if "Computer" or sth.else was choosen (sth. without a path), the developer has to handle this.
                {
                    Exception own = new Exception("No valid Folder choosen.");
                    own.Source = "WPF_Dialogs.dll:FolderBrowseDialog:Throwing exception if \"Computer\" or sth.else was choosen (sth. without a path), the developer has to handle this.";
                    throw own;
                }
                this.DialogResult = true;
            }
            this.Close();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
