using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using ContactRoute.Config;
using System.IO;

namespace CrConfigurator
{
    public partial class frmMain : Form
    {
        private SortedList<string, IBaseModel> m_SeriveList;
        private ApplicationSection m_AppConfig = null;

        public frmMain()
        {
            InitializeComponent();

            m_SeriveList = new SortedList<string, IBaseModel>();
        }


        #region Service

        public void CreateServiceList(TreeView parent)
        {
            parent.Nodes.Clear();
            m_SeriveList.Clear();

            //Addons
            TreeNode node = new TreeNode("Applications");

            for (int i = 0; i < m_AppConfig.Addons.Count; i++)
            {
                //Create service object
                Type tpe = Type.GetType(m_AppConfig.Addons[i].Type);
                if (tpe == null)
                    System.Diagnostics.Trace.WriteLine("Can't find serivce " + m_AppConfig.Addons[i].Name);
                else
                {
                    m_SeriveList.Add(m_AppConfig.Addons[i].Name, Activator.CreateInstance(tpe) as ContactRoute.Config.IBaseModel);
                    m_SeriveList[m_AppConfig.Addons[i].Name].Location = m_AppConfig.Addons[i].Location;
                    m_SeriveList[m_AppConfig.Addons[i].Name].ConfigFile = m_AppConfig.Addons[i].File;
                    m_SeriveList[m_AppConfig.Addons[i].Name].Name = m_AppConfig.Addons[i].Name;
                    m_SeriveList[m_AppConfig.Addons[i].Name].ConfigMode = (ConfigMode)Enum.Parse(typeof(ConfigMode), m_AppConfig.Addons[i].Mode);
                    m_SeriveList[m_AppConfig.Addons[i].Name].UserModeId = m_AppConfig.Addons[i].UserId;
                    //Create node
                    TreeNode appNde = new TreeNode(m_SeriveList[m_AppConfig.Addons[i].Name].Name);
                    appNde.Tag = m_AppConfig.Addons[i].Name;
                    node.Nodes.Add(appNde);
                }
            }
            node.Expand();
            parent.Nodes.Add(node);
        }

        #endregion

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                m_AppConfig = (ApplicationSection)ConfigurationManager.GetSection("applicationMain");
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLine("Error reading custom config" + err);
            }
            if (m_AppConfig == null) System.Diagnostics.Trace.WriteLine("No config found!");
            
            CreateServiceList(tvServices);
        }

        private void tvServices_AfterSelect(object sender, TreeViewEventArgs e)
        {
            plServiceContainer.Controls.Clear();

            if (e.Node == null) return;
            if (e.Node.Tag == null) return;

            string key = e.Node.Tag.ToString();

            IBaseModel tmpService;
            if (m_SeriveList.TryGetValue(key, out tmpService))
            {
                tmpService.LoadProfile();
                tmpService.Show(plServiceContainer);
                lblServiceTitle.Text = string.Format("{0} ({1})", tmpService.Name, tmpService.ConfigMode.ToString());
                lblVersionConfigFile.Text = string.Format("Config item version {0}", GetAssemblyVersion(tmpService.GetType().Assembly.ToString()));
                lblVersionModel.Text = string.Format("ConfigModel version {0}", GetAssemblyVersion(tmpService.GetType().BaseType.Assembly.ToString()));

                lbLocationl.ForeColor = Color.Black;
                lbLocationl.Text = tmpService.Location + " <-> " + tmpService.ConfigFile;
            }
        }

        private string GetAssemblyVersion(string data)
        {
            string rtn = "";

            string[] list = data.Split(new char[] { ',' });

            foreach (string item in list)
            {
                if (item.Trim().ToLower().StartsWith("version="))
                {
                    rtn = item.Trim().Substring("version=".Length);
                    break;
                }
            }
            return rtn;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (tvServices.SelectedNode == null) return;

            IBaseModel tmpService;
            if (m_SeriveList.TryGetValue(tvServices.SelectedNode.Tag.ToString(), out tmpService))
            {
                tmpService.Save();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try { Application.Exit(); }
            catch { }
            try { this.Close(); }
            catch { }
        }
    }
}
