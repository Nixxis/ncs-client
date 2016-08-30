using System;
using System.IO;
using System.Reflection;
using System.Net;
using System.Runtime.InteropServices;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;

namespace Nixxis
{
    
    public class ServiceDescription
	{
		public string ServiceID { get; private set; }
		public string Location { get; private set; }
		public System.Collections.Specialized.NameValueCollection Settings { get; private set; }

		private ServiceDescription()
		{
			Settings = new System.Collections.Specialized.NameValueCollection();
		}

		public ServiceDescription(XmlNode node) : this()
		{
			if (node.Attributes["id"].Value.EndsWith("$"))
				ServiceID = node.Attributes["id"].Value.Substring(0, node.Attributes["id"].Value.Length - 1);
			else
				ServiceID = node.Attributes["id"].Value;

			Location = node.InnerText;

			foreach (XmlAttribute Attr in node.Attributes)
			{
				Settings.Set(Attr.Name, Attr.Value);
			}
		}
	}

    [Flags]
    public enum LoginOptions
    {
        None = 0,
        TrustIdentification = 1,
        UseComputerName = 2,
        CustomIdentification = 4
    }
    public class ServiceList : System.Collections.ObjectModel.KeyedCollection<string, ServiceDescription>, System.ComponentModel.INotifyPropertyChanged
	{
		public XmlDocument Source { get; private set; }

        private string m_Edition = string.Empty;
        private int m_AgentsLicenses = 0;
        private DateTime m_Validity = DateTime.Now;
        private string m_DetailedVersion = string.Format("{0}.{1}.{2}", Assembly.GetEntryAssembly().GetName().Version.Major, Assembly.GetEntryAssembly().GetName().Version.Minor, Assembly.GetEntryAssembly().GetName().Version.Build);
        private LoginOptions m_LoginOptions = LoginOptions.None;
        private string m_SsoUri = null;
        private string m_DomainId = string.Empty;
        private string m_DomainName = string.Empty;

        public string Version
        {
            get
            {
                string[] strSplit = DetailedVersion.Split('.');
                return string.Format("{0}.{1}", strSplit[0], strSplit[1]);
            }
        }

        public string DetailedVersion
        {
            get
            {
                return m_DetailedVersion;
            }
            private set
            {
                m_DetailedVersion = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("DetailedVersion"));
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("Version"));
                }
            }
        }

        public string Edition
        {
            get
            {
                return m_Edition;
            }
            set
            {
                m_Edition = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("Edition"));
                }
            }
        }
        public int AgentsLicenses
        {
            get
            {
                return m_AgentsLicenses;
            }
            set
            {
                m_AgentsLicenses = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("AgentsLicenses"));
                }
            }
        }
        public DateTime Validity
        {
            get
            {
                return m_Validity;
            }
            set
            {
                m_Validity = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("Validity"));
                }
            }
        }

        public LoginOptions LoginOptions
        {
            get
            {
                return m_LoginOptions;
            }
            set
            {
                m_LoginOptions = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("LoginOptions"));
                }
            }
        }

        public string SsoUri
        {
            get
            {
                return m_SsoUri;
            }
            set
            {
                m_SsoUri = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("SsoUri"));
                }
            }
        }


        public string DomainId
        {
            get
            {
                return m_DomainId;
            }
            set
            {
                m_DomainId = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("DomainId"));
                }
            }
        }

        public string DomainName
        {
            get
            {
                return m_DomainName;
            }
            set
            {
                m_DomainName = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("DomainName"));
                }
            }
        }
        public ServiceList()
		{
		}

        public void Load(XmlDocument document)
		{
            Reset();
			Source = document;

			XmlNode Root = Source.SelectSingleNode("//services");

            try
            {
                DetailedVersion = Root.GetAttributeValue("version");
            }
            catch
            {
            }

            try
            {
                Edition = Root.GetAttributeValue("product");
            }
            catch
            {
            }

            try
            {
                string strLic = Root.GetAttributeValue("license");
                if(!string.IsNullOrEmpty(strLic))
                    AgentsLicenses = System.Xml.XmlConvert.ToInt32(strLic.Split(' ')[0]);
            }
            catch
            {
            }

            try
            {
                string strValidity = Root.GetAttributeValue("validity");
                if(!string.IsNullOrEmpty(strValidity))
                    Validity = System.Xml.XmlConvert.ToDateTime(strValidity);
            }
            catch
            {
            }

            try
            {
                string strLoginOpt = Root.GetAttributeValue("loginOptions");
                if(!string.IsNullOrEmpty(strLoginOpt))
                    LoginOptions = (LoginOptions)Enum.Parse(typeof(LoginOptions), strLoginOpt, true);
            }
            catch
            {
            }

            try
            {
                SsoUri = Root.GetAttributeValue("ssoUri");
            }
            catch
            {
            }

            try
            {
                DomainId = Root.GetAttributeValue("id");
            }
            catch
            {
            }

            try
            {
                DomainName = Root.GetAttributeValue("name");
            }
            catch
            {
            }

			if (Root != null)
			{
				foreach (XmlNode Child in Root)
				{
					Add(new ServiceDescription(Child));
				}
			}
		}

        public void Reset()
        {
            this.Clear();
            this.ClearItems();           
        }

		protected override string GetKeyForItem(ServiceDescription item)
		{
			return item.ServiceID.ToLowerInvariant();
		}

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }

    public static class Extensions
    {
        public static string GetAttributeValue(this XmlNode node, string attributeName)
        {
            if (node == null || string.IsNullOrEmpty(attributeName))
                return null;

            if (node.NodeType == XmlNodeType.Text)
                return null;

            XmlAttribute att = node.Attributes[attributeName];
            if (att == null)
                return null;

            return att.Value;
        }

    }

}
