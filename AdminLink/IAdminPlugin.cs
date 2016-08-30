using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;

namespace Nixxis.Client.Admin
{
    public interface IAdminConfigurator
    {
        bool InitializeProvider(AdminCore core, MediaProvider mediaProviderInfo);
    }

    public class BaseAdminConfigurator : IAdminConfigurator
    {
        protected MediaProvider m_MediaProviderInfo;

        public virtual bool InitializeProvider(AdminCore core, MediaProvider mediaProviderInfo)
        {
            m_MediaProviderInfo = mediaProviderInfo;
            return true;
        }
    }

    [Flags] public enum MediaProviderConfiguratorCapabilities
    {
        ShowModalDialog = 1,
        InsertPropertyPage = 2
    }

    public interface IMediaProviderConfigurator : IAdminConfigurator, IDisposable
    {
        MediaProviderConfiguratorCapabilities Capabilities { get; }

        bool CanShowModalDialog { get; }
        bool CanInsertPropertyPage { get; }

        XmlDocument Config { get; set; }
        string ConfigDescription { get; }

        bool? ShowConfigurationDialog(DependencyObject owner);
        DependencyObject PropertyPage { get; }
    }

    public abstract class BaseMediaProviderConfigurator : BaseAdminConfigurator, IMediaProviderConfigurator
    {
        private XmlDocument m_Config;

        public bool CanShowModalDialog
        {
            get
            {
                return ((this.Capabilities & MediaProviderConfiguratorCapabilities.ShowModalDialog) != 0);
            }
        }

        public bool CanInsertPropertyPage  
        {
            get
            {
                return ((this.Capabilities & MediaProviderConfiguratorCapabilities.InsertPropertyPage) != 0);
            }
        }

        public virtual XmlDocument Config
        {
            get
            {
                return m_Config;
            }
            set
            {
                m_Config = value;
            }
        }

        public virtual void Dispose()
        {
        }

        public abstract MediaProviderConfiguratorCapabilities Capabilities
        {
            get;
        }

        public abstract string ConfigDescription
        {
            get;
        }

        public virtual bool? ShowConfigurationDialog(DependencyObject owner)
        {
            throw new NotSupportedException();
        }

        public virtual DependencyObject PropertyPage
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        protected void GenerateEmptyConfig()
        {
            Config = new XmlDocument();

            XmlElement elm = Config.CreateElement("Root");
            Config.AppendChild(elm);

            XmlElement elm2 = Config.CreateElement("Config");
            elm.AppendChild(elm2);

            XmlAttribute att = Config.CreateAttribute("name");
            att.Value = m_MediaProviderInfo.Description;
            elm2.Attributes.Append(att);

            att = Config.CreateAttribute("providerid");
            att.Value = m_MediaProviderInfo.Id;
            elm2.Attributes.Append(att);

            att = Config.CreateAttribute("providertype");
            att.Value = m_MediaProviderInfo.PluginType;
            elm2.Attributes.Append(att);
        }

        public virtual XmlNode ConfigNode
        {
            get
            {
                return m_Config.SelectSingleNode("Root/Config");
            }
        }

        public override bool InitializeProvider(AdminCore core, MediaProvider mediaProviderInfo)
        {
            bool res = base.InitializeProvider(core, mediaProviderInfo);
            GenerateEmptyConfig();
            return res;
        }
    }


}
