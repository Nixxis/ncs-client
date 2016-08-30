using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections.Specialized;

namespace Common.Tools.Config
{
    //
    //Events
    //
    public class ProfileEventArgs : EventArgs
    {
        #region Class Data
        private string _Filename;
        #endregion

        #region Constructor
        public ProfileEventArgs(string filename)
        {
            this._Filename = filename;
        }
        #endregion

        #region Properties
        public string Filename
        {
            get { return this._Filename; }
            internal set { this._Filename = value; }
        }
        #endregion
    }

    public delegate void ProfileBaseEventHandler(object sender, ProfileEventArgs e);

    //
    //Class
    //
    public interface IBaseProfile
    {
        #region Properties
        string ProfileDescription
        {
            get;
            set;
        }
        int ProfileIndex
        {
            get;
        }
        bool IsDefaultProfile
        {
            get;
            set;
        }
        string sysKey
        {
            get;
            set;
        }
        #endregion

        #region Members
        #endregion
    }
    public abstract class BaseProfile : IBaseProfile
    {
        #region Class data
        public const string KeyPrefix = "Profile";
        protected string m_ProfileDescription;
        protected int m_ProfileIndex;
        protected bool m_IsDefaultProfile;
        protected string m_sysKey;
        #endregion

        #region Constructor
        public BaseProfile(int index)
        {
            m_ProfileIndex = index;
            m_sysKey = BaseProfile.KeyPrefix + index;
        }
        #endregion

        #region Properties
        public string ProfileDescription
        {
            get { return m_ProfileDescription; }
            set { m_ProfileDescription = value; }
        }
        public int ProfileIndex
        {
            get { return m_ProfileIndex; }
        }
        public bool IsDefaultProfile
        {
            get { return m_IsDefaultProfile; }
            set { m_IsDefaultProfile = value; }
        }
        public string sysKey
        {
            get { return m_sysKey; }
            set { m_sysKey = value; }
        }
        #endregion
    }


    public abstract class BaseProfiles : SortedList<string, IBaseProfile> 
    {
        #region Class data
        protected string m_DefaultConfigFilePrefix = "Default";
        protected string m_ConfigFile = "Settings.xml";
        protected string m_XmlBasePath = @"configuration/nixxisUserSettings/";
        protected string m_XmlSetting = @"setting";
        protected string m_XmlPath = "";
        protected string m_XmlNodePath = "";
        protected int m_DefaultProfileId = 0;
        protected IBaseProfile m_DefaultProfile = null;
        protected int m_CountInFile = 0;
        protected int m_CurrentProfile = 0;
        #endregion

        #region constructor
        public BaseProfiles()
        {
            SetXmlPath();
        }
        #endregion

        #region Properties
        public string ConfigFile
        {
            get { return m_ConfigFile; }
            set { m_ConfigFile = value; }
        }
        public int DefaultProfileId
        {
            get { return m_DefaultProfileId; }
            set { m_DefaultProfileId = value; }
        }
        public int CurrentProfile
        {
            get { return m_CurrentProfile; }
            set { m_CurrentProfile = value; }
        }
        #endregion

        #region Members
        #region Tools 
        private void SetXmlPath()
        {
            m_XmlPath = m_XmlBasePath + m_XmlSetting;
            m_XmlNodePath = m_XmlPath + @"[@name='{0}']";
        }
        private XmlDocument OpenCreateDoc()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(m_ConfigFile);
            }
            catch (FileNotFoundException err)
            {
                try
                {
                    doc.Load(m_DefaultConfigFilePrefix + m_ConfigFile);
                }
                catch
                {
                    doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\" ?><configuration> </configuration>");
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
            return doc;
        }
        private void CheckXmlDocument(XmlDocument doc)
        {
            XmlNode nde = doc.DocumentElement;
            XmlNode ndeSearch = null;
            
            string[] list = m_XmlBasePath.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
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
        #endregion
        public abstract bool Load();
        public abstract bool ReadParameters(string key);
        public abstract bool Save();

        public void SetDefaultProfile(string sysKey)
        {
            foreach (IBaseProfile item in Values)
            {
                if (item.sysKey == sysKey)
                {
                    item.IsDefaultProfile = true;
                    m_DefaultProfileId = item.ProfileIndex;
                    m_DefaultProfile = item;
                }
                else
                    item.IsDefaultProfile = false;
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
        public string ReadItem(string item, int index)
        {
            XmlDocument doc = OpenCreateDoc();

            XmlNode nde = null;
            nde = doc.SelectSingleNode(string.Format(m_XmlNodePath, item));

            if (nde == null) return "";

            if (nde.Attributes["type"].Value.ToLower() == "arrayofstring")
            {
                if (nde.ChildNodes.Count > 0 && nde.ChildNodes.Count >= index)
                    return nde.ChildNodes[index].InnerText;
                else
                    return "";
            }
            else
                return nde.InnerText;
        }
        public object Read(string item)
        {
            XmlDocument doc = OpenCreateDoc();

            XmlNode nde = null;
            nde = doc.SelectSingleNode(string.Format(m_XmlNodePath, item));

            if (nde == null) return "";

            if (nde.Attributes["type"].Value.ToLower() == "arrayofstring")
            {
                return ReadArrayOfString(item);
            }
            else if (nde.Attributes["type"].Value.ToLower() == "int")
            {
                int vInt = 0;
                int.TryParse(nde.InnerText, out vInt);
                return vInt;
            }
            else if (nde.Attributes["type"].Value.ToLower() == "bool")
            {
                bool vBool = false;
                bool.TryParse(nde.InnerText, out vBool);
                return vBool;
            }
            else if (nde.Attributes["type"].Value.ToLower() == "string")
            {
                return ReadArrayOfString(item);
            }
            else
                return nde.Value;
        }

        public void SaveItem(string name, object value)
        {
            XmlDocument doc = OpenCreateDoc();
            CheckXmlDocument(doc);
            XmlNode nde = doc.SelectSingleNode(string.Format(m_XmlNodePath, name));
            //Name
            if (nde == null)
            {
                nde = doc.CreateNode(XmlNodeType.Element, m_XmlSetting, "");
                XmlNode att = doc.CreateNode(XmlNodeType.Attribute, "name", "");
                att.Value = name;
                nde.Attributes.SetNamedItem(att);

                XmlNode parent = doc.SelectSingleNode(m_XmlBasePath.TrimEnd(new char[] {'/'}));
                if (parent != null) parent.AppendChild(nde);
                
            }
            //Type
            XmlNode type = nde.Attributes.GetNamedItem("type");
            if (type == null)
            {
                type = doc.CreateNode(XmlNodeType.Attribute, "type", "");
                nde.Attributes.SetNamedItem(type);
            }
            if (value.GetType() == typeof(int) || value.GetType() == typeof(Int16) || value.GetType() == typeof(Int32) || value.GetType() == typeof(Int64))
                type.Value = XmlConfigAttributesType.Int;
            else if (value.GetType() == typeof(string[]) || value.GetType() == typeof(StringCollection))
                type.Value = XmlConfigAttributesType.ArrayOfString;
            else if (value.GetType() == typeof(Boolean) || value.GetType() == typeof(bool))
                type.Value = XmlConfigAttributesType.Bool;
            else if (value.GetType() == typeof(string) || value.GetType() == typeof(String))
                type.Value = XmlConfigAttributesType.String;
            else
                type.Value = value.GetType().ToString();
            //Value
            nde.InnerText = "";
            if (value.GetType() == typeof(int) || value.GetType() == typeof(Int16) || value.GetType() == typeof(Int32) || value.GetType() == typeof(Int64))
                nde.InnerText = value.ToString();
            else if (value.GetType() == typeof(string[]))
                CreateArrayOfStringNode((string[])value, doc, nde);
            else if (value.GetType() == typeof(StringCollection))
                CreateArrayOfStringNode((StringCollection)value, doc, nde);
            else if (value.GetType() == typeof(Boolean) || value.GetType() == typeof(bool))
                nde.InnerText = value.ToString();
            else if (value.GetType() == typeof(string) || value.GetType() == typeof(String))
                nde.InnerText = value.ToString();
            else
                nde.InnerText = value.ToString();           

            doc.Save(m_ConfigFile);
        }
        #endregion
    }
    public static class XmlConfigAttributes
    {
        public static string Type = "type";
        public static string Name = "name";
    }
    public static class XmlConfigAttributesType
    {
        public static string Int = "int";
        public static string ArrayOfString = "arrayofstring";
        public static string Bool = "bool";
        public static string String = "string";
    }
}
