using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace ContactRoute.Config
{
    public class ApplicationSection : ConfigurationSection
    {

        [ConfigurationProperty("addons")]
        public AddonElementCollection Addons
        {
            get { return (AddonElementCollection)this["addons"]  ?? new AddonElementCollection(); }
            set { this["addons"] = value; }
        }
    }

    public class AddonElements : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return (string)this["type"]; }
            set { this["type"] = value; }
        }

        [ConfigurationProperty("location", IsRequired = true)]
        public string Location
        {
            get { return (string)this["location"]; }
            set { this["location"] = value; }
        }

        [ConfigurationProperty("file", IsRequired = true)]
        public string File
        {
            get { return (string)this["file"]; }
            set { this["file"] = value; }
        }

        [ConfigurationProperty("mode", DefaultValue = "Default", IsRequired = false)]
        public string Mode
        {
            get { return (string)this["mode"]; }
            set { this["mode"] = value; }
        }

        [ConfigurationProperty("userid", DefaultValue = "", IsRequired = false)]
        public string UserId
        {
            get { return (string)this["userid"]; }
            set { this["userid"] = value; }
        }
    }

    public class AddonElementCollection : ConfigurationElementCollection
    {
        public AddonElements this[int index]
        {
            get
            {
                return base.BaseGet(index) as AddonElements;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }
        protected override ConfigurationElement CreateNewElement()
        {
            return new AddonElements();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AddonElements)element).Name;
        }

    }

}
