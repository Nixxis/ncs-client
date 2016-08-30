using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ContactRoute.Config
{
    public interface IBaseModel
    {
        string Name { get; set;  }
        string Description { get; }
        string Location { get; set; }
        string ConfigFile { get; set; }
        baseUcConfig Control { get; set; }
        IProfile Profile { get; set; }
        ConfigMode ConfigMode { get; set; }
        string UserModeId { get; set; }
        string UserXmlUrl { get; set; }
        string UserXmlKeyFormat { get; set; }

        void Show(Control parent);
        void LoadProfile();
        void Save();
    }

    public abstract class BaseModel : IBaseModel
    {
        #region Class Data
        protected string m_Name = "";
        protected string m_Description = "";
        protected string m_Location = string.Empty;
        protected string m_ConfigFile = "Profile.config";
        protected IProfile m_Profile = null;
        protected baseUcConfig m_Control = null;
        protected ConfigMode m_ConfigMode = ConfigMode.System;
        protected string m_UserModeId = "";
        protected string m_UserXmlUrl = "";
        protected string m_UserXmlKeyFormat = "xxxx_ {0}"; // {0} --> UserModeId
        #endregion

        #region Property

        public string Name
        {
            get { return m_Name;  }
            set { m_Name = value; }
        }
        public string Description
        {
            get { return m_Description; }
        }
        public string Location
        {
            get { return m_Location; }
            set { m_Location = value; }
        }
        public string ConfigFile
        {
            get { return m_ConfigFile; }
            set { m_ConfigFile = value; }
        }
        public IProfile Profile
        {
            get { return m_Profile; }
            set { m_Profile = value; }
        }
        public baseUcConfig Control
        {
            get { return m_Control; }
            set { m_Control = value; }
        }
        public ConfigMode ConfigMode
        {
            get { return m_ConfigMode; }
            set { m_ConfigMode = value; }
        }
        public string UserModeId
        {
            get { return m_UserModeId; }
            set { m_UserModeId = value; }
        }
        public string UserXmlUrl
        {
            get { return m_UserXmlUrl; }
            set { m_UserXmlUrl = value; }
        }
        public string UserXmlKeyFormat
        {
            get { return m_UserXmlKeyFormat; }
            set { m_UserXmlKeyFormat = value; }
        }
        #endregion

        public virtual void Show(Control parent)
        {
            if (m_Control == null) return;
            m_Control.ConfigMode = m_ConfigMode;
            m_Control.Dock = DockStyle.Fill;
            m_Control.ConfigMode = m_ConfigMode;

            if (m_Profile == null)
                LoadProfile();
            else
            {
                m_Profile.SetProfileData(m_Control);
                m_Control.SetProfileData(m_Profile.DataObj);
            }
            if (!parent.Controls.Contains(m_Control))
                parent.Controls.Add(m_Control);
        }
        public abstract void LoadProfile();
        public abstract void Save();
    }

}
