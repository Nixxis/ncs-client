using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using Nixxis.ClientV2;

namespace Nixxis.Client.Supervisor
{
    public class SupervisionColumnCollection : KeyedCollection<string, SupervisionColumnItem>
    {
        protected override string GetKeyForItem(SupervisionColumnItem item)
        {
            return item.InternalId;
        }
    }

    public class SupervisionColumnItem : INotifyPropertyChanged
    {
        public static readonly DependencyProperty IdProperty = DependencyProperty.RegisterAttached(
            "Id",
            typeof(string),
            typeof(SupervisionColumnItem),
            new FrameworkPropertyMetadata(string.Empty)
            );

        public static void SetId(DependencyObject element, string value)
        {
            element.SetValue(IdProperty, value);
        }
        public static string GetId(DependencyObject element)
        {
            return (string)element.GetValue(IdProperty);
        }

        #region Enums
        #endregion

        #region Class Data
        private Visibility m_Visible = Visibility.Visible;
        private string m_Id = string.Empty;
        private int m_Index = -1;
        private string m_Description = string.Empty;
        private System.Windows.Controls.DataGridLength m_Width;
        private int m_DisplayIndex;
        private ListSortDirection? m_SortDirection;
        #endregion

        #region Properties
        public Visibility Visible
        {
            get { return m_Visible; }
            set { m_Visible = value; FirePropertyChanged("Visible"); }
        }
        public System.Windows.Controls.DataGridLength Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                m_Width = value; 
                FirePropertyChanged("Width");
            }
        }
        public ListSortDirection? SortDirection
        {
            get
            {
                return m_SortDirection;
            }
            set
            {
                m_SortDirection = value;
                FirePropertyChanged("SortDirection");
            }
        }
        public int DisplayIndex
        {
            get
            {
                return m_DisplayIndex;
            }
            set
            {
                m_DisplayIndex = value;
                FirePropertyChanged("DisplayIndex");
            }
        }
        public string Id
        {
            get { return m_Id; }
            set { m_Id = value; FirePropertyChanged("Id"); FirePropertyChanged("InternalId"); }
        }
        public string InternalId
        {
            get { return m_Index + "," + m_Id; }
        }
        public int Index
        {
            get { return m_Index; }
            set { m_Index = value; FirePropertyChanged("Index"); FirePropertyChanged("InternalId"); }
        }
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; FirePropertyChanged("Description"); }
        }
        #endregion

        #region Members
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion
    }


    public class WorkSpace : INotifyPropertyChanged
    {
        #region Class data
        private string m_Id = Guid.NewGuid().ToString("N");
        private string m_UserId = string.Empty;
        private DateTime m_CreationDateTime = DateTime.Now;
        private DateTime m_ModifyDateTime = DateTime.Now;
        private string m_Version = string.Empty;

        private XmlDocument m_Xmldoc;
        #endregion

        #region Constructors
        public WorkSpace()
        {
            m_Version = "1";
            m_Xmldoc = GetXmlDocument();
        }
        #endregion


        public XmlDocument XmlDoc
        {
            get { return GetXmlDocument(); }
            set { m_Xmldoc = value; }
        }

        public DateTime CreationDateTime
        {
            get { return m_CreationDateTime; }
            set { m_CreationDateTime = value; }
        }
        public DateTime ModifyDateTime
        {
            get { return m_ModifyDateTime; }
            set { m_ModifyDateTime = value; }
        }
        public string Version
        {
            get { return m_Version; }
            set { m_Version = value; }
        }
        public string Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }
        public string UserId
        {
            get { return m_UserId; }
            set { m_UserId = value; }
        }
        public string CurrentWindow
        {
            get
            {
                if (XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsCurrentWindow] != null)
                    return XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsCurrentWindow].Value as string;
                else
                    return null;
            }
            set
            {
                if (XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsCurrentWindow]!=null)
                    XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsCurrentWindow].Value = value;
                else
                {
                    XmlAttribute att = XmlDoc.CreateAttribute(XmlWorkSpaceNames.WsCurrentWindow);
                    att.Value = value;
                    XmlDoc.DocumentElement.Attributes.Append(att);
                }
            }
        }

        public double GetWindowHeight(int index)
        {
                if (XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowHeight + index.ToString()] != null)
                    return System.Xml.XmlConvert.ToDouble(XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowHeight + index.ToString()].Value);
                else
                    return 0.0;
        }
        public double GetWindowWidth(int index)
        {
            if (XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowWidth + index.ToString()] != null)
                return System.Xml.XmlConvert.ToDouble(XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowWidth + index.ToString()].Value);
            else
                return 0.0;

        }
        public double GetWindowLeft(int index)
        {
            if (XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowX + index.ToString()] != null)
                return System.Xml.XmlConvert.ToDouble(XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowX + index.ToString()].Value);
            else
                return 0.0;
        }
        public double GetWindowTop(int index)
        {
            if (XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowY + index.ToString()] != null)
                return System.Xml.XmlConvert.ToDouble(XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowY + index.ToString()].Value);
            else
                return 0.0;

        }
        public int GetWindowState(int index)
        {
            if (XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowState + index.ToString()] != null)
                return System.Xml.XmlConvert.ToInt32(XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowState + index.ToString()].Value);
            else
                return 0;
        }


        public void SetWindowHeight(int index, double value)
        {
            if (XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowHeight + index.ToString()] != null)
                XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowHeight + index.ToString()].Value = System.Xml.XmlConvert.ToString(value);
            else
            {
                XmlAttribute att = XmlDoc.CreateAttribute(XmlWorkSpaceNames.WsWindowHeight+ index.ToString());
                att.Value = System.Xml.XmlConvert.ToString(value);
                XmlDoc.DocumentElement.Attributes.Append(att);
            }
        }
        public void SetWindowWidth(int index, double value)
        {
            if (XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowWidth + index.ToString()] != null)
                XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowWidth + index.ToString()].Value = System.Xml.XmlConvert.ToString(value);
            else
            {
                XmlAttribute att = XmlDoc.CreateAttribute(XmlWorkSpaceNames.WsWindowWidth + index.ToString());
                att.Value = System.Xml.XmlConvert.ToString(value);
                XmlDoc.DocumentElement.Attributes.Append(att);
            }

        }
        public void SetWindowState(int index, int value)
        {
            if (XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowState + index.ToString()] != null)
                XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowState + index.ToString()].Value = System.Xml.XmlConvert.ToString(value);
            else
            {
                XmlAttribute att = XmlDoc.CreateAttribute(XmlWorkSpaceNames.WsWindowState + index.ToString());
                att.Value = System.Xml.XmlConvert.ToString(value);
                XmlDoc.DocumentElement.Attributes.Append(att);
            }
        }
        public void SetWindowLeft(int index, double value)
        {
            if (XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowX + index.ToString()] != null)
                XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowX + index.ToString()].Value = System.Xml.XmlConvert.ToString(value);
            else
            {
                XmlAttribute att = XmlDoc.CreateAttribute(XmlWorkSpaceNames.WsWindowX + index.ToString());
                att.Value = System.Xml.XmlConvert.ToString(value);
                XmlDoc.DocumentElement.Attributes.Append(att);
            }
        }
        public void SetWindowTop(int index, double value)
        {
            if (XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowY + index.ToString()] != null)
                XmlDoc.DocumentElement.Attributes[XmlWorkSpaceNames.WsWindowY + index.ToString()].Value = System.Xml.XmlConvert.ToString(value);
            else
            {
                XmlAttribute att = XmlDoc.CreateAttribute(XmlWorkSpaceNames.WsWindowY + index.ToString());
                att.Value = System.Xml.XmlConvert.ToString(value);
                XmlDoc.DocumentElement.Attributes.Append(att);
            }

        }

        public void SetWindowContent(int index, XmlNode node)
        {
            XmlNode n = null;
            n = XmlDoc.DocumentElement.SelectSingleNode(XmlWorkSpaceNames.WsSubWindow + index.ToString());
            if (n != null)
            {
                n.RemoveAll();

            }
            else
            {
                n = XmlDoc.DocumentElement.AppendChild(XmlDoc.CreateElement(XmlWorkSpaceNames.WsSubWindow + index.ToString()));
            }

            n.AppendChild(XmlDoc.ImportNode(node, true));
        }

        public XmlNode GetWindowContent(int index)
        {
            XmlNode n = XmlDoc.DocumentElement.SelectSingleNode(XmlWorkSpaceNames.WsSubWindow + index.ToString());
            if (n != null)
            {
                return n.ChildNodes[0];
            }
            else
            {
                return null;
            }

        }

        public void GetTileItem(NixxisSupervisionTileItem tile)
        {
            if (tile == null)
                return;

            XmlNode nde;
            XmlNode att;

            //
            //Document
            //
            m_Xmldoc = GetXmlDocument();

            //
            //Tile
            //
            XmlNode tileNde = m_Xmldoc.CreateNode(XmlNodeType.Element, XmlWorkSpaceNames.TileItem, "");
            att = m_Xmldoc.CreateNode(XmlNodeType.Attribute, "id", "");
            att.Value = tile.Id;
            tileNde.Attributes.SetNamedItem(att);


            XmlNode d =  tile.GetExtraSettings();
            if (d != null)
            {

                nde = m_Xmldoc.ImportNode(d, true);

                d = m_Xmldoc.CreateElement(XmlWorkSpaceNames.ExtraSettings);

                d.AppendChild(nde);

                tileNde.AppendChild(d);
            }

            //
            //Columns
            //
            XmlNode columnsNde = m_Xmldoc.CreateNode(XmlNodeType.Element, XmlWorkSpaceNames.ColumnItems, "");
            tileNde.AppendChild(columnsNde);

            //
            //Column
            //

            SupervisionColumnCollection col = tile.GetColumnSettings();
            if(col!=null)
            { 

                foreach (SupervisionColumnItem item in col)
                {
                    XmlNode colNde = m_Xmldoc.CreateNode(XmlNodeType.Element, XmlWorkSpaceNames.ColumnItem, "");
                    att = m_Xmldoc.CreateNode(XmlNodeType.Attribute, XmlColumnNames.Id, "");
                    att.Value = item.InternalId;
                    colNde.Attributes.SetNamedItem(att);

                    nde = m_Xmldoc.CreateNode(XmlNodeType.Element, XmlColumnNames.Visible, "");
                    nde.InnerText = item.Visible.ToString();
                    colNde.AppendChild(nde);

                    nde = m_Xmldoc.CreateNode(XmlNodeType.Element, XmlColumnNames.Width, "");
                    nde.InnerText = string.Format("{0};{1}",item.Width.UnitType, XmlConvert.ToString(item.Width.Value) );
                    colNde.AppendChild(nde);

                    nde = m_Xmldoc.CreateNode(XmlNodeType.Element, XmlColumnNames.DisplayIndex, "");
                    nde.InnerText = XmlConvert.ToString(item.DisplayIndex);
                    colNde.AppendChild(nde);

                    nde = m_Xmldoc.CreateNode(XmlNodeType.Element, XmlColumnNames.SortDirection, "");
                    nde.InnerText = item.SortDirection.ToString();
                    colNde.AppendChild(nde);

                    columnsNde.AppendChild(colNde);
                }
            }



            //
            //Add node
            //
            AddOrUpdateTileNode(tileNde, tile.Id);
        }
        public void SetTileItem(NixxisSupervisionTileItem tile)
        {
            XmlNode nde;
            //
            //Document
            //
            m_Xmldoc = GetXmlDocument();

            //
            //Tile
            //
            XmlNode tileNde = m_Xmldoc.SelectSingleNode(XmlSelectNode(XmlWorkSpaceNames.TileItemFull, "id", tile.Id));

            if (tileNde == null)
                return;

            nde = tileNde.SelectSingleNode(XmlWorkSpaceNames.ExtraSettings);
            if(nde!=null)
            {
                XmlDocument d = new XmlDocument();
                nde = d.ImportNode(nde.ChildNodes[0], true);
                tile.SetExtraSettings(nde);
            }


            //
            //Columns
            //
            SupervisionColumnCollection collection = new SupervisionColumnCollection();

            XmlNode columnsNde = tileNde.SelectSingleNode(XmlWorkSpaceNames.ColumnItems);

            if (columnsNde != null)
            {
                foreach (XmlNode item in columnsNde.ChildNodes)
                {
                    string idString = item.Attributes.GetNamedItem("id").Value;
                    string[] list = idString.Split(new char[] { ',' });

                    if (list.Length > 0)
                    {
                        string id = string.Empty;
                        int index = -1;
                        int displayIndex = -1;
                        Visibility vis = Visibility.Visible;
                        ListSortDirection? sortDirection = null;
                        System.Windows.Controls.DataGridLength width = new System.Windows.Controls.DataGridLength(1.0, System.Windows.Controls.DataGridLengthUnitType.Auto);

                        try
                        {
                            index = int.Parse(list[0]);
                            if (list.Length > 1)
                                id = list[1];

                            nde = item.SelectSingleNode(XmlColumnNames.Visible);
                            vis = (Visibility)Enum.Parse(typeof(Visibility), nde.InnerText);

                            nde = item.SelectSingleNode(XmlColumnNames.Width);
                            if (nde != null)
                            {
                                string[] tempAr = nde.InnerText.Split(';');
                                width = new System.Windows.Controls.DataGridLength(
                                    XmlConvert.ToDouble(tempAr[1]),
                                    (System.Windows.Controls.DataGridLengthUnitType)(Enum.Parse(typeof(System.Windows.Controls.DataGridLengthUnitType), tempAr[0])));
                            }

                            nde = item.SelectSingleNode(XmlColumnNames.DisplayIndex);
                            if (nde != null)
                            {
                                displayIndex = XmlConvert.ToInt32(nde.InnerText);
                            }
                            nde = item.SelectSingleNode(XmlColumnNames.SortDirection);
                            if (nde != null)
                            {
                                if (string.IsNullOrEmpty(nde.InnerText))
                                    sortDirection = null;
                                else
                                    sortDirection = (ListSortDirection) Enum.Parse(typeof(ListSortDirection), nde.InnerText);
                            }
                        }
                        catch { }

                        SupervisionColumnItem col = new SupervisionColumnItem() { Index = index, Id = id, Visible = vis, Width = width, DisplayIndex = displayIndex, SortDirection = sortDirection };

                        collection.Add(col);
                    }
                }
            }
            tile.SetColumnSettings(collection);

        }


        public void LoadXml(string xml)
        {
            GetXmlDocument();
            m_Xmldoc.LoadXml(xml);
        }

        private XmlDocument GetXmlDocument()
        {
            if (m_Xmldoc == null)
            {
                m_Xmldoc = new XmlDocument();
                m_Xmldoc.LoadXml(string.Format("<?xml version=\"1.0\" encoding=\"utf-8\" ?><{0}> </{0}>", XmlWorkSpaceNames.WorkSpace));
                m_Xmldoc.LastChild.AppendChild(m_Xmldoc.CreateNode(XmlNodeType.Element, XmlWorkSpaceNames.TileItems, ""));

            }

            return m_Xmldoc;
        }
        private void AddOrUpdateTileNode(XmlNode node, string key)
        {
            //
            //Document
            //
            m_Xmldoc = GetXmlDocument();

            //
            //Add or update tile info
            //
            XmlNode nde = m_Xmldoc.SelectSingleNode(XmlSelectNode(XmlWorkSpaceNames.TileItemFull, "id", key));

            if (nde == null)
            {
                nde = m_Xmldoc.SelectSingleNode(XmlWorkSpaceNames.TileItemsFull);

                if (nde != null)
                {
                    nde.AppendChild(node);
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Can't add or update node");
                }
            }
            else
            {
                nde.ParentNode.ReplaceChild(node, nde);
            }
        }

        private string XmlSelectNode(string path, string key, string value)
        {
            return string.Format(@"{0}[@{1}='{2}']", path, key, value);
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        internal void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }


    }

    public static class XmlWorkSpaceNames
    {
        public static string WorkSpace = "supervisionworkspace";
        public static string WorkSpaces = "supervisionworkspaces";

        public static string WsDescription = "description";
        public static string WsCreationDateTime = "creationdatetime";
        public static string WsModifyDateTime = "modifydatetime";
        public static string WsVersion = "version";
        public static string WsId = "id";
        public static string WsUserId = "userid";
        public static string WsCurrentWindow = "currentWindow";

        public static string WsWindowWidth = "width";
        public static string WsWindowHeight = "height";
        public static string WsWindowX = "X";
        public static string WsWindowY = "Y";
        public static string WsWindowState = "state";

        public static string WsSubWindow = "subwindow";

        public static string TileItems = "tiles";
        public static string TileItemsFull = XmlWorkSpaceNames.WorkSpace + "/tiles";

        public static string TileItem = "tile";
        public static string TileItemFull = XmlWorkSpaceNames.TileItemsFull + "/tile";

        public static string ExtraSettings = "extrasettings";
        
        public static string ColumnItems = "columns";
        public static string ColumnItemsFull = XmlWorkSpaceNames.TileItemFull + "/columns";

        public static string ColumnItem = "column";
        public static string ColumnItemFull = XmlWorkSpaceNames.ColumnItemsFull + "/column";
    }

    public static class XmlColumnNames
    {
        public static string Id = "id";
        public static string Visible = "visible";
        public static string Width = "width";
        public static string DisplayIndex = "displayIndex";
        public static string SortDirection = "sort";
    }



    public class SavedContexts : IDictionary<string, SavedContext>
    {
        private Dictionary<string, SavedContext> m_InternalList = new Dictionary<string, SavedContext>();
        private string m_ContextName;
        private Supervision m_Supervision;
        private string m_DefaultNewNameBase = "New context {0}";
        public SavedContexts(Supervision supervision, string contextName)
            : base()
        {
            m_Supervision = supervision;

            m_ContextName = contextName;
        }

        private void RefreshList()
        {
            m_InternalList.Clear();

            InternalLoadList(false);

            InternalLoadList(true);
        }



        private void InternalLoadList(bool shared)
        {
            // TODO: implement cache?

            ResponseData rd = m_Supervision.ClientLink.LoadData((shared ? "*" : m_Supervision.ClientLink.AgentId) + m_ContextName);


            if (rd.Valid)
            {
                XmlDocument doc = new XmlDocument();

                try
                {
                    doc.LoadXml(rd.ToString());

                    foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                    {
                        string id = node.Attributes["id"] != null ? node.Attributes["id"].Value : null;
                        string description = node.Attributes["description"] != null ? node.Attributes["description"].Value : null;

                        if (id == null)
                        {
                            foreach (XmlNode subnode in node.ChildNodes)
                            {
                                if (subnode.Name.Equals("id", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    id = subnode.InnerText;
                                    break;
                                }
                            }
                        }
                        if (description == null)
                        {
                            foreach (XmlNode subnode in node.ChildNodes)
                            {
                                if (subnode.Name.Equals("description", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    description = subnode.InnerText;
                                    break;
                                }
                            }
                        }

                        m_InternalList.Add(id, new SavedContext(m_Supervision, id, shared, description));
                    }
                }
                catch
                {

                }
            }

        }

        private void InternalSaveList(bool shared)
        {
            string strKey = (shared ? "*" : m_Supervision.ClientLink.AgentId) + m_ContextName;

            XmlDocument doc = new XmlDocument();

            doc.AppendChild(doc.CreateElement("Contexts"));

            foreach (SavedContext sc in m_InternalList.Values.Where((sc) => (sc.Shared == shared)))
            {
                XmlElement elm = doc.CreateElement("Context");
                XmlAttribute att = doc.CreateAttribute("id");
                att.Value = sc.Id;
                elm.Attributes.Append(att);

                att = doc.CreateAttribute("description");
                att.Value = sc.Name;
                elm.Attributes.Append(att);
                doc.DocumentElement.AppendChild(elm);
            }

            m_Supervision.ClientLink.SaveData(strKey, doc.OuterXml);
        }

        private void InternalRemove(string key)
        {
            // TODO... There is no way of removing from contextdata 
        }

        public string DefaultNewNameBase
        {
            get
            {
                return m_DefaultNewNameBase;
            }
            set
            {
                m_DefaultNewNameBase = value;
            }
        }

        public string DefaultNewName
        {
            get
            {
                int counter = 0;

                string strNewNameBase = string.Format(m_DefaultNewNameBase, m_Supervision.ClientLink.Account);
                string strNewName = strNewNameBase;
                // TODO
                while (this.Values.FirstOrDefault((sc) => (sc.Name == strNewName)) != null)
                {
                    strNewName = strNewNameBase + " " + counter.ToString(); ;
                    counter++;
                }

                return strNewName;
            }
        }

        public bool ContainsKey(string key)
        {
            lock (m_InternalList)
            {
                RefreshList();
                return m_InternalList.ContainsKey(key);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                lock (m_InternalList)
                {
                    RefreshList();
                    return m_InternalList.Keys;
                }
            }
        }

        public bool TryGetValue(string key, out SavedContext value)
        {
            lock (m_InternalList)
            {
                RefreshList();
                return m_InternalList.TryGetValue(key, out value);
            }
        }

        public int Count
        {
            get
            {
                lock (m_InternalList)
                {
                    RefreshList();
                    return m_InternalList.Count;
                }
            }
        }

        public SavedContext Add(string description, bool shared)
        {
            string strKey = System.Guid.NewGuid().ToString("N");
            SavedContext sc = new SavedContext(m_Supervision, strKey, shared, description);
            Add(strKey, sc);
            return sc;
        }

        public void Add(string key, SavedContext value)
        {
            lock (m_InternalList)
            {
                RefreshList();

                if (m_InternalList.ContainsKey(key))
                {
                    this[key] = value;
                    return;
                }
                else
                    m_InternalList.Add(key, value);
            }

            InternalSaveList(value.Shared);
        }


        public bool Remove(string key)
        {
            RefreshList();
            SavedContext sd = null;
            if (m_InternalList.ContainsKey(key))
                sd = m_InternalList[key];

            if (m_InternalList.Remove(key))
            {
                InternalSaveList(sd.Shared);

                InternalRemove(key);
                return true;
            }
            else
                return false;
        }



        public ICollection<SavedContext> Values
        {
            get
            {
                RefreshList();
                return m_InternalList.Values;
            }
        }

        public SavedContext this[string key]
        {
            get
            {
                RefreshList();
                return m_InternalList[key];
            }
            set
            {
                RefreshList();
                if (m_InternalList.ContainsKey(key))
                {
                    m_InternalList[key] = value;

                    InternalSaveList(value.Shared);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public void Add(KeyValuePair<string, SavedContext> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            RefreshList();
            foreach (string key in m_InternalList.Keys)
            {
                Remove(key);
            }
        }

        public bool Contains(KeyValuePair<string, SavedContext> item)
        {
            RefreshList();
            return m_InternalList.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, SavedContext>[] array, int arrayIndex)
        {
            RefreshList();
            foreach (KeyValuePair<string, SavedContext> item in m_InternalList)
            {

                array[arrayIndex] = new KeyValuePair<string, SavedContext>(item.Key, item.Value);
                arrayIndex++;
            }
        }


        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, SavedContext> item)
        {
            return Remove(item.Key);

        }

        public IEnumerator<KeyValuePair<string, SavedContext>> GetEnumerator()
        {
            RefreshList();
            return m_InternalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            RefreshList();
            return m_InternalList.GetEnumerator();
        }
    }


    public class SavedContext
    {
        private Supervision m_Supervision;
        private XmlDocument m_Content = null;

        private void RefreshContent()
        {
            // TODO: implement cache?
            ResponseData rd = m_Supervision.ClientLink.LoadData(Id);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(rd.ToString());
            m_Content = doc;
        }


        public string Id { get; set; }
        public bool Shared { get; set; }
        public string Name { get; set; }
        public XmlDocument Content
        {
            get
            {
                RefreshContent();
                return m_Content;
            }
            set
            {
                m_Supervision.ClientLink.SaveData(Id, value.OuterXml);
            }
        }

        public SavedContext(Supervision supervision, string id, bool shared, string name)
        {
            m_Supervision = supervision;
            Id = id;
            Shared = shared;
            Name = name;
        }

    }

}

