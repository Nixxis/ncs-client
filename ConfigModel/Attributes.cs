using System;
using System.Collections.Generic;
using System.Text;

namespace ContactRoute.Config
{


    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigFileValueFieldAttribute : Attribute
    {
        #region Class data
        private string m_Name = string.Empty;
        private Type m_Type = null;
        private bool m_UserValue = false;
        private bool m_ReadOnly = false;
        #endregion

        public ConfigFileValueFieldAttribute()
        {
        }

        #region Properties
        public string Name
        {
            set { m_Name = value; }
            get { return m_Name; }
        }
        public Type Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }
        public bool UserValue
        {
            get { return m_UserValue; }
            set { m_UserValue = value; }
        }
        public bool ReadOnly
        {
            get { return m_ReadOnly; }
            set { m_ReadOnly = value; }
        }
        #endregion
    }
}
