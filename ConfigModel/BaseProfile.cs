using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Forms;

namespace ContactRoute.Config
{
    //
    //TO DO:
    //
    //1. Use location for saving to. Saving is now only using the config file name.
    //

    public interface IProfile
    {
        Object DataObj { get; set; }
        Type Type { get; set; }
        ConfigMode ConfigMode { get; set; }
        string UserModeId { get; set; }
        string UserXmlUrl { get; set; }
        string UserXmlKeyFormat { get; set; }

        bool Load();
        bool Save();

        void SetProfileData(Control control);
        void GetProfileData(Control control);
    }

    public abstract class BaseProfile : IProfile
    {
        #region Class data

        protected string m_DefaultConfigFilePrefix = "Default";
        protected string m_ConfigFile = "Profile.config";
        protected string m_Location = string.Empty;
        protected string m_UserValuesPrefix = "User{0}-";
        protected ConfigMode m_ConfigMode = ConfigMode.Default;
        protected string m_UserModeId = "";
        protected string m_UserXmlUrl = "";
        protected string m_UserXmlKeyFormat = "xxxx_ {0}"; // {0} --> UserModeId
        
        protected Type m_Type = null;
        protected Object m_DataObj = null;

        protected string m_XmlConfigurationPath = @"configuration/nixxisUserSettings";
        protected string m_XmlConfigurationTypeAttribute = "type";
        protected string m_XmlBasePath = @"configuration/nixxisUserSettings/";
        protected string m_XmlSetting = @"add";
        protected string m_XmlProperty = @"property";
        protected string m_XmlPath = "";
        protected string m_XmlNodePath = "";
        protected string m_XmlPropertyPath = "";
        
        private object m_Tag = null;

        private XmlDocument m_MainConfigXml = null;
        private XmlDocument m_UserConfigXml = null;
        private bool m_ReloadConfig = true;
        #endregion

        #region Constructor

        public BaseProfile(Type type)
        {
            m_Type = type;
            SetXmlPath();
        }

        #endregion

        #region Properties
        public Object DataObj
        {
            get { return m_DataObj; }
            set { m_DataObj = value; }
        }
        public Type Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }
        public string ConfigFile
        {
            get { return m_ConfigFile; }
            set { m_ConfigFile = value; }
        }
        public string Location
        {
            get { return m_Location; }
            set { m_Location = value; }
        }
        public string UserModeId
        {
            get { return m_UserModeId; }
            set { m_UserModeId = value; }
        }
        public object Tag
        {
            get { return m_Tag; }
            set { m_Tag = value; }
        }
        public ConfigMode ConfigMode
        {
            get { return m_ConfigMode; }
            set { m_ConfigMode = value; }
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

        #region Member

        #region XML
        
        private void SetXmlPath()
        {
            m_XmlPath = m_XmlBasePath + m_XmlSetting;
            m_XmlNodePath = m_XmlPath + @"[@" + XmlConfigAttributes.Name + "='{0}']";
            m_XmlPropertyPath = m_XmlProperty + @"[@" + XmlConfigAttributes.Name + "='{0}']";
        }
        private XmlDocument OpenCreateDoc()
        {
            return OpenCreateDoc(false);
        }
        private XmlDocument OpenCreateDoc(bool userfile)
        {
            if (!m_ReloadConfig)
            {
                if (userfile)
                {
                    if (m_UserConfigXml != null)
                        return m_UserConfigXml;
                }
                else
                {
                    if (m_MainConfigXml != null)
                        return m_MainConfigXml;
                }
            }

            string file = GetFileName(userfile);

            XmlDocument doc = new XmlDocument();
            try
            {
                string fileLoc = string.Empty;

                if (m_Location.ToLower().StartsWith("http"))
                    fileLoc = new Uri(new Uri(m_Location.EndsWith("/") ? m_Location : m_Location + "/"), file).ToString();
                else if (!string.IsNullOrEmpty(m_Location))
                    fileLoc = Path.Combine(m_Location, file);
                else
                    fileLoc = file;

                doc.Load(fileLoc);
            }
            catch (FileNotFoundException err)
            {
                try
                {
                    doc.Load(file);
                }
                catch
                {
                    try
                    {
                        doc.Load(GetDefaultFileName(userfile));
                    }
                    catch
                    {
                        try
                        {
                            doc.Load(m_DefaultConfigFilePrefix + file);
                        }
                        catch
                        {
                            doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\" ?><configuration> </configuration>");
                        }
                    }
                }
            }
            catch (System.Net.WebException err)
            {
                try
                {
                    doc.Load(file);
                }
                catch
                {
                    try
                    {
                        doc.Load(GetDefaultFileName(userfile));
                    }
                    catch
                    {
                        try
                        {
                            doc.Load(m_DefaultConfigFilePrefix + file);
                        }
                        catch
                        {
                            doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\" ?><configuration> </configuration>");
                        }
                    }
                }
            }
            catch (XmlException err)
            {
                System.Diagnostics.Trace.WriteLine("Err loading Xml doc. Problably due to a problem in the Xml document. Besure to have a root element. " + err.ToString());
                throw new XmlException();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLine("Err loading Xml doc. " + err.ToString());
                throw new Exception();
            }

            if (userfile)
                m_UserConfigXml = doc;
            else
                m_MainConfigXml = doc;

            System.Diagnostics.Trace.WriteLine("ConfigModel. Config=" + doc.BaseURI + ". Request file=" + m_Location + "<->" + file);

            m_ReloadConfig = false;
            return doc;
        }

        private string GetFileName(bool user)
        {
            string file;
            string cfgFile = m_ConfigFile;
            if (string.IsNullOrEmpty(cfgFile)) cfgFile = "Profile.config";

            if (user && m_ConfigMode != ConfigMode.System)
            {
                file = string.Format(m_UserValuesPrefix, m_UserModeId) + m_ConfigFile;
            }
            else
                file = m_ConfigFile;

            return file;
        }
        private string GetDefaultFileName(bool user)
        {
            string file;
            string cfgFile = "Profile.config";

            if (user && m_ConfigMode != ConfigMode.System)
            {
                file = string.Format(m_UserValuesPrefix, m_UserModeId) + cfgFile;
            }
            else
                file = cfgFile;

            return file;
        }
        private void CheckXmlDocument(XmlDocument doc)
        {
            XmlNode nde = doc.DocumentElement;
            XmlNode ndeSearch = null;

            string[] list = m_XmlBasePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < list.Length; i++)
            {
                ndeSearch = nde.SelectSingleNode(list[i]);
                if (ndeSearch == null)
                {
                    XmlNode newNode = doc.CreateNode(XmlNodeType.Element, list[i], "");
                    nde.AppendChild(newNode);
                    nde = newNode;
                }
                else
                    nde = ndeSearch;
            }
        }
        private void CreateArrayOfStringNode(string[] value, XmlDocument doc, XmlNode parent)
        {
            StringCollection val = new StringCollection();
            val.AddRange(value);
            CreateArrayOfStringNode(val, doc, parent);
        }
        private void CreateArrayOfStringNode(StringCollection value, XmlDocument doc, XmlNode parent)
        {
            foreach (string item in value)
            {
                XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "string", "");
                newNode.InnerText = item;
                parent.AppendChild(newNode);
            }
        }
        
        public string[] ReadArrayOfString(string item)
        {
            XmlDocument doc = OpenCreateDoc();

            XmlNode nde = null;
            nde = doc.SelectSingleNode(string.Format(m_XmlNodePath, item));

            if (nde == null) return null;

            if (nde.Attributes["type"].Value.ToLower() == "arrayofstring")
            {

                if (nde.ChildNodes.Count > 0)
                {
                    string[] list = new string[nde.ChildNodes.Count];
                    for (int i = 0; i < list.Length; i++)
                        list[i] = nde.ChildNodes[i].InnerText;

                    return list;
                }
                else
                    return null;
            }
            else
                return null;
        }
        public object Read(string item)
        {
            return Read(item, false);
        }
        public object Read(string item, bool userValue)
        {
            object rtnObj = null;
            XmlNode nde = null;
            XmlDocument doc = null;
            //
            //Getting the default value
            //
            doc = OpenCreateDoc();
            nde = doc.SelectSingleNode(string.Format(m_XmlNodePath, item));
            if (nde != null)
            {
                rtnObj = GetValue(nde, item);
            }
            //
            //If this is a uservalue then tried to get the uservalue;
            //
            if (m_ConfigMode != ConfigMode.System)
            {
                bool isValueUserValue = userValue;
                try
                {
                    XmlAttribute ndeAttributte = nde.Attributes["UserValue"];

                    if (ndeAttributte != null)
                    {
                        isValueUserValue = bool.Parse(nde.Attributes["UserValue"].Value);
                    }
                }
                catch { }

                if (isValueUserValue)
                {
                    doc = OpenCreateDoc(true);
                    nde = doc.SelectSingleNode(string.Format(m_XmlNodePath, item));
                    if (nde == null) return rtnObj;
                    rtnObj = GetValue(nde, item);
                }
            }
            return rtnObj;
        }
        private object GetValue(XmlNode nde, string item)
        {
            object rtnObj = null;

            if (nde == null) return null;

            if (nde.Attributes["type"].Value.ToLower() == "arrayofstring")
            {
                rtnObj = ReadArrayOfString(item);
            }
            else if (nde.Attributes["type"].Value.ToLower() == "int")
            {
                int vInt = 0;
                int.TryParse(nde.InnerText, out vInt);
                rtnObj = vInt;
            }
            else if (nde.Attributes["type"].Value.ToLower() == "bool")
            {
                bool vBool = false;
                bool.TryParse(nde.InnerText, out vBool);
                rtnObj = vBool;
            }
            else if (nde.Attributes["type"].Value.ToLower() == "string")
            {
                rtnObj = nde.InnerText.ToString();
            }
            else
            {
                try
                {
                    Type tpe = Type.GetType(nde.Attributes["type"].Value); 
                    rtnObj = Activator.CreateInstance(tpe);
                    rtnObj = DeserializationProfileObject(rtnObj, nde);
                }
                catch
                {
                    rtnObj = nde.Value;
                }
            }

            return rtnObj;
        }

        public void SaveItem(string name, object value)
        {
            SaveItem(name, value, false);
        }
        public void SaveItem(string name, object value, bool userValue)
        {
            m_ReloadConfig = true;
            XmlDocument doc = null;
            //Check if it is a user value or not and open the correct file
            if (!userValue && m_ConfigMode == ConfigMode.User) return;
            if (userValue && m_ConfigMode != ConfigMode.System)
                doc = OpenCreateDoc(true);
            else
                doc = OpenCreateDoc();

            CheckXmlDocument(doc);
            XmlNode nde = doc.SelectSingleNode(string.Format(m_XmlNodePath, name));
            //Name
            if (nde == null)
            {
                nde = doc.CreateNode(XmlNodeType.Element, m_XmlSetting, "");
                XmlNode att = doc.CreateNode(XmlNodeType.Attribute, XmlConfigAttributes.Name, "");
                att.Value = name;
                nde.Attributes.SetNamedItem(att);

                XmlNode parent = doc.SelectSingleNode(m_XmlBasePath.TrimEnd(new char[] { '/' }));
                if (parent != null) parent.AppendChild(nde);

            }
            XmlNode type = nde.Attributes.GetNamedItem("type");
            nde.InnerText = "";
            if (type == null)
            {
                type = doc.CreateNode(XmlNodeType.Attribute, "type", "");
                nde.Attributes.SetNamedItem(type);
            }
            if (value.GetType() == typeof(int) || value.GetType() == typeof(Int16) || value.GetType() == typeof(Int32) || value.GetType() == typeof(Int64))
            {
                type.Value = XmlConfigAttributesType.Int;
                nde.InnerText = value.ToString();
            }
            else if (value.GetType() == typeof(string[]))
            {
                type.Value = XmlConfigAttributesType.ArrayOfString;
                CreateArrayOfStringNode((string[])value, doc, nde);
            }
            else if (value.GetType() == typeof(StringCollection))
            {
                type.Value = XmlConfigAttributesType.ArrayOfString;
                CreateArrayOfStringNode((StringCollection)value, doc, nde);
            }
            else if (value.GetType() == typeof(Boolean) || value.GetType() == typeof(bool))
            {
                type.Value = XmlConfigAttributesType.Bool;
                nde.InnerText = value.ToString();
            }
            else if (value.GetType() == typeof(string) || value.GetType() == typeof(String))
            {  
                type.Value = XmlConfigAttributesType.String;
                nde.InnerText = value.ToString();
            }
            else if (value.GetType().BaseType == typeof(System.Enum))
            {
                type.Value = XmlConfigAttributesType.Int;
                nde.InnerText = value.GetHashCode().ToString();
            }
            else
            {
                type.Value = value.GetType().ToString() + ", " + value.GetType().Assembly.FullName.Split(new char[] { ',' })[0];
                XmlNodeList list = SerializationProfileObject(value);
                for (int i = 0; i < list.Count; i++)
                {
                    XmlNode newNode = doc.CreateNode(list[i].NodeType, list[i].Name, "");
                    for (int j = 0; j < list[i].Attributes.Count; j++)
                    {
                        XmlNode newAtt = doc.CreateNode(list[i].Attributes[j].NodeType, list[i].Attributes[j].Name, "");
                        newAtt.Value = list[i].Attributes[j].Value;
                        newNode.Attributes.SetNamedItem(newAtt);
                    }
                    newNode.InnerText = list[i].InnerText;
                    nde.AppendChild(newNode);
                }
            }

            doc.Save(GetFileName(userValue));
        }

        public void RemoveItem(string name, object value, bool userValue)
        {
            m_ReloadConfig = true;
            XmlNode nde = null;
            XmlDocument doc = null;

            if (!userValue && m_ConfigMode == ConfigMode.User) return;
            if (userValue && m_ConfigMode != ConfigMode.System)
                doc = OpenCreateDoc(true);
            else
                doc = OpenCreateDoc();

            if (value.GetType() == typeof(string[]) ||
                value.GetType().BaseType == typeof(System.Array))
            {
                bool nextValue = true;
                int index = 0;
                
                while(nextValue)
                {
                    nde = doc.SelectSingleNode(string.Format(m_XmlNodePath, name + index.ToString()));
                    if (nde != null)
                    {
                        nde.ParentNode.RemoveChild(nde);
                        index++;
                    }
                    else
                    {
                        nextValue = false;
                    }
                }
            }
            else
            {
                nde = doc.SelectSingleNode(string.Format(m_XmlNodePath, name));
                if (nde != null)
                {
                   doc.RemoveChild(nde);
                }
            }

            doc.Save(GetFileName(userValue));
        }
        public string GetConfigType()
        {
            XmlDocument doc = OpenCreateDoc();

            XmlNode nde = null;
            try
            {
                nde = doc.SelectSingleNode(m_XmlConfigurationPath);
            }
            catch
            {
                return null;
            }

            if (nde == null) return null;

            try
            {
                return nde.Attributes["type"].Value;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion


        #region Inhertens impl
        public virtual bool Load()
        {
            //
            //Load special item (used for loading user based config from the appserver
            //
            try { m_UserXmlUrl = Read(XmlConfigSpecialNodes.UserXmlUrl, false).ToString(); }
            catch { System.Diagnostics.Trace.WriteLine(string.Format("UserXmlUrl --> {0}", m_UserXmlUrl)); }
            try { m_UserXmlKeyFormat = Read(XmlConfigSpecialNodes.UserXmlKeyFormat, false).ToString(); }
            catch { System.Diagnostics.Trace.WriteLine(string.Format("UserXmlKeyFormat --> {0}", m_UserXmlKeyFormat)); }
            //
            //Load class data
            //
            string dfltMsg = "Parameter {0} is not present in the config file default value will be used.";
            foreach (PropertyInfo property in m_Type.GetProperties())
            {
                foreach (ConfigFileValueFieldAttribute attr in property.GetCustomAttributes(typeof(ConfigFileValueFieldAttribute), true))
                {
                    //Name of parameter
                    string name = string.Empty;
                    if (string.IsNullOrEmpty(attr.Name))
                        name = property.Name;
                    else
                        name = attr.Name;
                    //User value
                    bool userValue =  attr.UserValue;

                    try 
                    {
                        if (property.PropertyType == typeof(Boolean) || property.PropertyType == typeof(bool))
                        {
                            property.SetValue(m_DataObj, (bool)Read(name, userValue), null);
                        }
                        else if (property.PropertyType == typeof(string) || property.PropertyType == typeof(String))
                        {
                            property.SetValue(m_DataObj, Read(name, userValue).ToString(), null);
                        }
                        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(Int16) || property.PropertyType == typeof(Int32) || property.PropertyType == typeof(Int64))
                        {
                            property.SetValue(m_DataObj, (int)Read(name, userValue), null);
                        }
                        else if (property.PropertyType == typeof(string[]))
                        {
                            object objVal = Read(name, userValue);

                            if (objVal != null)
                                property.SetValue(m_DataObj, objVal, null);
                            else
                                System.Diagnostics.Trace.WriteLine(string.Format(dfltMsg, name));
                        }
                        else if (property.PropertyType == typeof(StringCollection))
                        {
                            property.SetValue(m_DataObj, Read(name, userValue), null);
                        }
                        else if (property.PropertyType.BaseType == typeof(System.Enum))
                        {
                            property.SetValue(m_DataObj, Enum.Parse(property.PropertyType, Read(name, userValue).ToString()), null);
                        }
                        else if (property.PropertyType.BaseType == typeof(System.Array))
                        {
                            int startIndex = 0;
                            object objVal = Read(name + startIndex, userValue);
                            List<object> objList = new List<object>();

                            while(objVal != null)
                            {
                                objVal = Read(name + startIndex, userValue);
                                if (objVal != null) objList.Add(objVal);
                                startIndex++;
                            }

                            if (objList.Count > 0)
                            {
                                Array arrValue = Array.CreateInstance(objList[0].GetType(), objList.Count);

                                for (int i = 0; i < objList.Count; i++)
                                    arrValue.SetValue(objList[i], i);

                                property.SetValue(m_DataObj, arrValue, null);
                            }
                            else
                                property.SetValue(m_DataObj, null, null);
                        }
                        else
                        {
                            object objVal = Read(name, userValue);
                            if(objVal != null)
                                property.SetValue(m_DataObj, Read(name, userValue), null);
                        }
                    }
                    catch (Exception err) { System.Diagnostics.Trace.WriteLine(string.Format(dfltMsg, name, err.ToString())); }
                }
            }

            return true;
        }
        public virtual bool Save()
        {
            string dfltMsg = "Couldn't save parameter {0}. (Msg: {1})";
            foreach (PropertyInfo property in m_Type.GetProperties())
            {
                foreach (ConfigFileValueFieldAttribute attr in property.GetCustomAttributes(typeof(ConfigFileValueFieldAttribute), true))
                {
                    if (!attr.ReadOnly)
                    {
                        //Name of parameter
                        string name = string.Empty;
                        if (string.IsNullOrEmpty(attr.Name))
                            name = property.Name;
                        else
                            name = attr.Name;
                        //User value
                        bool userValue = attr.UserValue;

                        try
                        {
                            if (property.PropertyType != typeof(String[]) && property.PropertyType.BaseType == typeof(System.Array))
                            {
                                Array arrValue = (Array)property.GetValue(m_DataObj, null);

                                RemoveItem(name, arrValue, userValue);

                                for (int i = 0; i < arrValue.Length; i++)
                                {
                                    SaveItem(name + i, arrValue.GetValue(i), userValue);
                                }
                            }
                            else
                                SaveItem(name, property.GetValue(m_DataObj, null), userValue);
                        }
                        catch (Exception err) { System.Diagnostics.Trace.WriteLine(string.Format(dfltMsg, name, err.ToString())); }
                    }
                }
            }

            return true;
        }

        private XmlNodeList SerializationProfileObject(object obj)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\" ?><root></root>");
            XmlNode nde;
            XmlNode att;

            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {

                string name = property.Name;
                string type = property.PropertyType.ToString();
                string value = string.Empty;
                
                bool save = false;

                if (property.PropertyType == typeof(Boolean) || property.PropertyType == typeof(bool))
                {
                    value = property.GetValue(obj, null).ToString();
                    save = true;
                }
                else if (property.PropertyType == typeof(string) || property.PropertyType == typeof(String))
                {
                    value = string.IsNullOrEmpty((string)property.GetValue(obj, null)) ? "" : property.GetValue(obj, null).ToString();
                    save = true;
                }
                else if (property.PropertyType == typeof(string[]))
                {
                    string[] val = property.GetValue(obj, null) as string[];
                    value = string.Join(";", val);
                    save = true;
                }
                else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(Int16) || property.PropertyType == typeof(Int32) || property.PropertyType == typeof(Int64))
                {
                    value = property.GetValue(obj, null).ToString();
                    save = true;
                }
                else if (property.PropertyType.BaseType == typeof(System.Enum))
                {
                    value = property.GetValue(obj, null).ToString();
                    save = true;
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine(string.Format("Configurator.SerializationProfileObject. Wrn: property {0} of type {1} will not be saved", name, property.PropertyType.ToString()));
                }

                if (save)
                {
                    nde = doc.CreateNode(XmlNodeType.Element, m_XmlProperty, "");

                    att = doc.CreateNode(XmlNodeType.Attribute, XmlConfigAttributes.Name, "");
                    att.Value = name;
                    nde.Attributes.SetNamedItem(att);

                    att = doc.CreateNode(XmlNodeType.Attribute, XmlConfigAttributes.Type, "");
                    att.Value = type;
                    nde.Attributes.SetNamedItem(att);

                    nde.InnerText = value;

                    doc.ChildNodes[1].AppendChild(nde);
                }
            }
            return doc.ChildNodes[1].ChildNodes;
        }
        
        private Object DeserializationProfileObject(object obj, XmlNode node)
        {
            XmlNode nde;
            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                nde = node.SelectSingleNode(string.Format(m_XmlPropertyPath, property.Name));
                if (nde != null) 
                {
                    try
                    {
                        if (property.PropertyType == typeof(Boolean) || property.PropertyType == typeof(bool))
                        {
                            property.SetValue(obj, bool.Parse(nde.InnerText), null);
                        }
                        else if (property.PropertyType == typeof(string) || property.PropertyType == typeof(String))
                        {
                            property.SetValue(obj, nde.InnerText, null);
                        }
                        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(Int16) || property.PropertyType == typeof(Int32) || property.PropertyType == typeof(Int64))
                        {
                            property.SetValue(obj, int.Parse(nde.InnerText), null);
                        }
                        else if (property.PropertyType == typeof(string[]))
                        {
                            property.SetValue(obj, nde.InnerText.Split(new string[]{";"}, StringSplitOptions.None), null);
                        }
                        else if (property.PropertyType.BaseType == typeof(System.Enum))
                        {
                            property.SetValue(obj, Enum.Parse(property.PropertyType, nde.InnerText), null);
                        }
                        else
                        {
                        }
                    }
                    catch (Exception err) { System.Diagnostics.Trace.WriteLine("DeserializationProfileObject Err" + err.ToString()); }
                }
            }
            return obj;
        }

        public void SetProfileData(Control control)
        {

            if (control == null) return;

            ContactRoute.Config.Common.GuiHelp.SetDataToControl(m_DataObj, control, "#", true);
        }

        public void GetProfileData(Control control)
        {
            if (control == null) return;

            try
            {
                foreach (Control item in control.Controls)
                {
                    string tag = Convert.ToString(item.Tag);

                    if (!string.IsNullOrEmpty(tag))
                    {
                        if (tag.StartsWith("#"))
                        {
                            string[] part = tag.Split('#');
                            string parameterName = null;
                            //Option list
                            bool LogicNOT = false;

                            if (part.Length > 2)
                            {
                                string[] option = part[1].Split(',');

                                foreach (string op in option)
                                {
                                    if (op.ToLower() == "not")
                                    {
                                        LogicNOT = true;
                                    }
                                }
                                parameterName = part[2];
                            }
                            else
                            {
                                parameterName = tag.Substring(1);
                            }                            
                            
                            
                            bool found = false;

                            foreach (PropertyInfo property in m_Type.GetProperties())
                            {
                                foreach (ConfigFileValueFieldAttribute attr in property.GetCustomAttributes(typeof(ConfigFileValueFieldAttribute), true))
                                {
                                    string name = string.Empty;
                                    if (string.IsNullOrEmpty(attr.Name))
                                        name = property.Name;
                                    else
                                        name = attr.Name;

                                    if (name.Trim() == parameterName.Trim())
                                    {
                                        //
                                        //Getting the value depping on the type of control
                                        //
                                        object value = null;
                                        if (item.GetType() == typeof(TextBox))
                                        {
                                            value = item.Text;
                                        }
                                        else if (item.GetType() == typeof(CheckBox))
                                        {
                                            if(LogicNOT)
                                                value = !((CheckBox)item).Checked;
                                            else
                                                value = ((CheckBox)item).Checked;
                                        }
                                        else
                                        {
                                            value = item.Text;
                                        }
                                        //
                                        //Saving the value into the data object depping on the type of the property
                                        //
                                        if (property.PropertyType == typeof(Boolean) || property.PropertyType == typeof(bool))
                                        {
                                            property.SetValue(m_DataObj, (bool)value, null);
                                        }
                                        else if (property.PropertyType == typeof(string) || property.PropertyType == typeof(String))
                                        {
                                            property.SetValue(m_DataObj, value.ToString(), null);
                                        }
                                        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(Int16) || property.PropertyType == typeof(Int32) || property.PropertyType == typeof(Int64))
                                        {
                                            property.SetValue(m_DataObj, System.Convert.ToInt32(value), null);
                                        }
                                        else if (property.PropertyType == typeof(string[]))
                                        {
                                            property.SetValue(m_DataObj, value, null);
                                        }
                                        else if (property.PropertyType == typeof(StringCollection))
                                        {
                                            property.SetValue(m_DataObj, value, null);
                                        }
                                        else if (property.PropertyType.BaseType == typeof(System.Enum))
                                        {
                                            property.SetValue(m_DataObj, Enum.Parse(property.PropertyType, value.ToString()), null);
                                        }
                                        else
                                        {
                                            property.SetValue(m_DataObj, value.ToString(), null);
                                        }
                                        found = true;
                                    }
                                    if (found) break;
                                }
                                if (found) break;
                            }
                        }
                    }
                    if (item.Controls.Count > 0) GetProfileData(item);
                }
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.WriteLine("Configurator.GetProfileData. Error: " + error.ToString());
            }
        }

        private object Parameter(string parameter)
        {
            string dfltMsg = "Didn't find parameter {0}.";
            foreach (PropertyInfo property in m_Type.GetProperties())
            {
                foreach (ConfigFileValueFieldAttribute attr in property.GetCustomAttributes(typeof(ConfigFileValueFieldAttribute), true))
                {
                    string name = string.Empty;
                    if (string.IsNullOrEmpty(attr.Name))
                        name = property.Name;
                    else
                        name = attr.Name;
                    //User value
                    bool userValue = attr.UserValue;

                    try
                    {
                        if (property.PropertyType == typeof(Boolean) || property.PropertyType == typeof(bool))
                        {
                            property.SetValue(m_DataObj, (bool)Read(name, userValue), null);
                        }
                        else if (property.PropertyType == typeof(string) || property.PropertyType == typeof(String))
                        {
                            property.SetValue(m_DataObj, Read(name, userValue).ToString(), null);
                        }
                        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(Int16) || property.PropertyType == typeof(Int32) || property.PropertyType == typeof(Int64))
                        {
                            property.SetValue(m_DataObj, (int)Read(name, userValue), null);
                        }
                        else if (property.PropertyType == typeof(string[]))
                        {
                            property.SetValue(m_DataObj, Read(name, userValue), null);
                        }
                        else if (property.PropertyType == typeof(StringCollection))
                        {
                            property.SetValue(m_DataObj, Read(name, userValue), null);
                        }
                        else if (property.PropertyType.BaseType == typeof(System.Enum))
                        {
                            property.SetValue(m_DataObj, Enum.Parse(property.PropertyType, Read(name, userValue).ToString()), null);
                        }
                        else
                        {
                            property.SetValue(m_DataObj, Read(name, userValue).ToString(), null);
                        }
                    }
                    catch (Exception err) { System.Diagnostics.Trace.WriteLine(string.Format(dfltMsg, name, err.ToString())); }
                }
            }

            return true;
        }
        #endregion 

        #endregion
    }
    public static class XmlConfigSpecialNodes
    {
        public static string UserXmlUrl = "@@UserXmlUrl";
        public static string UserXmlKeyFormat = "@@UserXmlKeyFormat";
    }
    public static class AppServerXmlConfigActions
    {
        public static string Savedata = "~savedata";
        public static string Loaddata = "~loaddata";
    }
    public static class XmlConfigAttributes
    {
        public static string Type = "type";
        public static string Name = "key";
    }
    public static class XmlConfigAttributesType
    {
        public static string Int = "int";
        public static string ArrayOfString = "arrayofstring";
        public static string Bool = "bool";
        public static string String = "string";
    }
}
