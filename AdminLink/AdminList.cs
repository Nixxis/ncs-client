using System.Collections.Generic;
using System;
using System.Collections;
using System.Xml;
using System.Reflection;
using System.Collections.Specialized;

namespace Nixxis.Client.Admin
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class AdminListSortAttribute : Attribute
    {
        public string Property;

        public AdminListSortAttribute(string property)
        {
            Property = property;
        }
    }


    public abstract class AdminObjectList : AdminObject, INotifyCollectionChanged, IList
    {
        protected Type m_MemberType;
        protected PropertyInfo m_SortProperty;
        protected SortedList<string, string> m_Ids = new SortedList<string, string>();

        protected AdminObjectList(AdminObject parent)
            : base(parent)
        {
        }

        internal bool ContainsId(string id)
        {
            return m_Ids.ContainsKey(id);
        }

        public virtual void AddTest(AdminObject item)
        {
            m_Ids.Add(item.Id, item.Id);

            if (item.Parent == null)
                item.Parent = this;

            if (item.GetType().IsSubclassOf(typeof(AdminObjectLink)))
            {
                if (item is AdminObjectLink)
                {
                }
            }
            else if (item is AdminObjectReference)
            {
                item.Parent = this.Parent;
            }
            else
            {
                if (Parent.Id != null)
                    m_Core.AddObjectReference(Parent.Id, item.Id);
            }

            // Seems to be a bug in datagrid: delete last element then add crashes if not reset
            if (m_Ids.Count == 1)
                FireCollectionChanged(NotifyCollectionChangedAction.Reset);
            else
                FireCollectionChanged(NotifyCollectionChangedAction.Add, item);
        }

        internal virtual void AddId(string id)
        {
            m_Ids.Add(id, id);

            if (m_Ids.Count == 1)
                FireCollectionChanged(NotifyCollectionChangedAction.Reset);
            else
                FireCollectionChanged(NotifyCollectionChangedAction.Add, m_Core.GetAdminObject(id));
        }

        public virtual bool IsSingleton
        {
            get
            {
                return false;
            }
        }

        internal void RemoveId(string id)
        {
            if (m_Ids.Remove(id))
                FireCollectionChanged(NotifyCollectionChangedAction.Remove, m_Core.GetAdminObject(id));
        }

        internal virtual void ClearToLoad()
        {
            m_Ids.Clear();
        }

        internal abstract void Load(XmlElement node, PropertyInfo pInfo);

        internal virtual void Save(AdminObject caller, XmlElement node, PropertyInfo pInfo)
        { }

        public void FireCollectionChanged(NotifyCollectionChangedAction action)
        {
            m_Core.FireCollectionChanged(this, action);

            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action));
        }

        protected void FireCollectionChanged(NotifyCollectionChangedAction action, AdminObject item)
        {
            m_Core.FireCollectionChanged(this, action, item);

            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item));
        }

        protected void FireCollectionChanged(NotifyCollectionChangedAction action, AdminObject oldItem, AdminObject newItem)
        {
            m_Core.FireCollectionChanged(this, action, oldItem, newItem);

            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, oldItem, newItem));
        }

        protected void FireCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, e);
        }

        public void Refresh()
        {
            //TODO: reload if needed
            FireCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }

        internal IList<string> Ids
        {
            get
            {
                return m_Ids.Keys;
            }
        }

        internal PropertyInfo SortProperty
        {
            get
            {
                return m_SortProperty;
            }
            set
            {
                m_SortProperty = value;
            }
        }

        internal Type MemberType
        {
            get
            {
                return m_MemberType;
            }
        }

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        public virtual AdminObject Add(AdminObject item, bool deferNotifications)
        {

#if INTERNAL_DEBUG
            System.Diagnostics.Debug.WriteLine(string.Format("{0} adding foreign {1} - called from {2}", InternalDebug.ObjectFullName[this], InternalDebug.ObjectFullName[item], InternalDebug.CallerName));
            System.Diagnostics.Debug.Indent();
#endif

            if (m_MemberType.IsSubclassOf(typeof(AdminObjectLink)))
            {
#if INTERNAL_DEBUG
                System.Diagnostics.Debug.WriteLine(string.Format("Type {0} is a subclass of AdminObjectLink, adding to second collection", typeof(T).Name));
                System.Diagnostics.Debug.Indent();
#endif
                for (int i = 0; i < m_Ids.Keys.Count; i++)
                {
                    string CurrentId = m_Ids.Keys[i];
                    AdminObjectLink Link = (AdminObjectLink)m_Core.GetAdminObject(CurrentId);

                    if (Link.HasReference(item))
                    {
#if INTERNAL_DEBUG
                        System.Diagnostics.Debug.WriteLine(string.Format("Item already referenced by link {0}, aborting", InternalDebug.ObjectFullName[Link]));
                        System.Diagnostics.Debug.Unindent();
                        System.Diagnostics.Debug.Unindent();
#endif
                        return Link;
                    }
                }

#if INTERNAL_DEBUG
                System.Diagnostics.Debug.Unindent();
#endif

                AdminObjectLink NewLink = (AdminObjectLink)Activator.CreateInstance(m_MemberType, m_Parent);
                Type ItemType = item.GetType();
                Type T1 = m_MemberType.GetProperty("Object1", BindingFlags.NonPublic | BindingFlags.Instance).PropertyType;

                if (T1.Equals(ItemType))
                {
                    NewLink.SetIds(item.Id, Parent.Id);
                }
                else
                {
                    Type T2 = m_MemberType.GetProperty("Object2", BindingFlags.NonPublic | BindingFlags.Instance).PropertyType;

                    if (T2.Equals(ItemType))
                    {
                        NewLink.SetIds(m_Parent.Id, item.Id);
                    }
                    else if (ItemType.IsSubclassOf(T1))
                    {
                        NewLink.SetIds(item.Id, m_Parent.Id);
                    }
                    else if (ItemType.IsSubclassOf(T2))
                    {
                        NewLink.SetIds(m_Parent.Id, item.Id);
                    }
                }

                NewLink.Id = AdminObjectLink.GetCombinedId(NewLink.Id1, NewLink.Id2);

                AdminObjectLink PreviouslyDeleted = (AdminObjectLink)m_Core.RemoveDeletedObject(NewLink.Id);

                if (PreviouslyDeleted != null)
                    NewLink.RessurectFrom(PreviouslyDeleted);

                m_Core.SetAdminObject(NewLink);




                m_Ids.Add(item.Id, item.Id);

                if (item.Parent == null)
                    item.Parent = this;

                if (m_MemberType.IsSubclassOf(typeof(AdminObjectLink)))
                {
                    if (item is AdminObjectLink)
                    {
                    }
                }
                else if (item is AdminObjectReference)
                {
                    item.Parent = this.Parent;
                }
                else
                {
                    if (Parent.Id != null)
                        m_Core.AddObjectReference(Parent.Id, item.Id);
                }

                if (!deferNotifications)
                {
                    // Seems to be a bug in datagrid: delete last element then add crashes if not reset
                    if (m_Ids.Count == 1)
                        FireCollectionChanged(NotifyCollectionChangedAction.Reset);
                    else
                        FireCollectionChanged(NotifyCollectionChangedAction.Add, item);
                }

                NewLink.AddLinkSide(item);
                return NewLink;
            }
            else if (m_MemberType.IsSubclassOf(typeof(AdminObjectReference)))
            {
                throw new NotSupportedException("Reference lists are not implemented");
            }
            else
            {
                throw new Exception("Don't know how to add object");
            }

#if INTERNAL_DEBUG
            System.Diagnostics.Debug.Unindent();
#endif
            return null;
        }

        #region IList Members

        public int Add(object value)
        {            
            throw new NotImplementedException();
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            if (value is string)
                return m_Ids.ContainsKey((string)value);

            if (value == null)
                return false;

            return m_Ids.ContainsKey(((AdminObject)value).Id);
        }

        public virtual int IndexOf(object value)
        {
            if (value == null)
                return -1;
            if(value is AdminObject)
                return m_Ids.IndexOfKey(((AdminObject)value).Id);
            else if (value is string)
                return m_Ids.IndexOfKey((string)value);
            return -1;
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual void Remove(object value)
        {
            RemoveId(((AdminObject)value).Id);
        }

        public void RemoveAt(int index)
        {
            RemoveId(m_Ids.Keys[index]);
        }

        public virtual object this[int index]
        {
            get
            {
                if (m_Core == null)
                    return null;
                return m_Core.GetAdminObject(m_Ids.Keys[index], m_MemberType, this);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            int i = 0;

            foreach (string Id in m_Ids.Keys)
            {
                array.SetValue(m_Core.GetAdminObject(Id, m_MemberType, this), index + i++);
            }
        }

        public virtual int Count
        {
            get
            {
                return m_Ids.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object SyncRoot
        {
            get
            {
                //TODO: Be smarter ;-)
                return this;
            }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            foreach (string Id in m_Ids.Keys)
                yield return m_Core.GetAdminObject(Id, m_MemberType, this);
        }

        #endregion
    }

    public class SingletonAdminObjectList<T> : AdminObjectList<T> where T : AdminObject
    {
        internal SingletonAdminObjectList(AdminObject parent, bool canClone, bool preloadChildren)
            : base(parent, canClone, preloadChildren)
        {
        }

        internal SingletonAdminObjectList(AdminObject parent)
            : base(parent)
        {
        }

        public override bool IsSingleton
        {
            get
            {
                return true;
            }
        }

        protected override AdminObject Add(AdminObject item, bool deferNotifications)
        {
            if (Count > 0)
                Clear();

            return base.Add(item, deferNotifications);
        }

        protected override void Add(T item, bool deferNotifications)
        {
            if (Count > 0)
                Clear();

            base.Add(item, deferNotifications);
        }

        internal override void AddId(string id)
        {
            if (Count > 0)
                Clear();

            base.AddId(id);
        }
    }


    public class AdminObjectList<T> : AdminObjectList, IEnumerable<T>, ICollection<T> where T : AdminObject
    {
        protected bool m_CanClone;
        protected bool m_PreloadChildren;

        private static object[] m_CustomAttrs;
        private static PropertyInfo m_StaticSortProperty;

        internal AdminObjectList(AdminObject parent, bool canClone, bool preloadChildren)
            : base(parent)
        {
            // TestPreload
            preloadChildren = true;

            m_CanClone = canClone;
            m_PreloadChildren = preloadChildren;
            m_MemberType = typeof(T);

            if (m_CustomAttrs == null)
            {
                lock (this.GetType())
                {
                    if (m_CustomAttrs == null)
                    {
                        m_CustomAttrs = this.GetType().GetCustomAttributes(true);

                        foreach (object Attr in m_CustomAttrs)
                        {
                            if (Attr is AdminListSortAttribute)
                            {
                                m_StaticSortProperty = typeof(T).GetProperty(((AdminListSortAttribute)Attr).Property);
                                break;
                            }
                        }
                    }
                }
            }

            m_SortProperty = m_StaticSortProperty;
        }

        internal AdminObjectList(AdminObject parent)
            : this(parent, true, false)
        {
        }

        internal override void Load(XmlElement node, PropertyInfo pInfo)
        {
            string ListPath = "./*";
            string[] IdParts = null;
            bool Preload = m_PreloadChildren;
            object[] LoadAttributes = pInfo.GetCustomAttributes(typeof(AdminLoadAttribute), true);


            if (LoadAttributes != null && LoadAttributes.Length > 0)
            {
                AdminLoadAttribute LoadAttribute = (AdminLoadAttribute)LoadAttributes[0];

                if (!string.IsNullOrEmpty(LoadAttribute.Path))
                {
                    ListPath = string.Format(LoadAttribute.Path, m_Id ?? m_Parent.Id);

                }

                IdParts = LoadAttribute.IdParts;
            }
            else
            {
                if (node == null)
                    return;

                ListPath = string.Concat("./", pInfo.Name);
                Preload = true;
            }
            //System.Diagnostics.Trace.WriteLine(string.Format("{0} -> {1}", (node ?? m_Core.RootElement).Name, ListPath));
            if (ListPath.StartsWith("./SortFields"))
                ContactRoute.DiagnosticHelpers.DebugIfPossible();

            if (!ListPath.StartsWith("/"))
                Load((node ?? m_Core.RootElement).SelectNodes(ListPath), IdParts, Preload);
            else
            {
                try
                {
                    Load(m_Core.m_MultiIndexedXml.Get(ListPath), IdParts, Preload);
                }
                catch
                {
                    Load((node ?? m_Core.RootElement).SelectNodes(ListPath), IdParts, Preload);
                }
            }
        }

        internal override void Load(XmlElement node)
        {
            // Do not call base.Load(node);

            List<XmlNode> TmpList = new List<XmlNode>(100);

            for (XmlNode Child = node.FirstChild; Child != null; Child = Child.NextSibling)
            {
                if (Child.NodeType == XmlNodeType.Element)
                    TmpList.Add(Child);
            }

            System.Diagnostics.Stopwatch sw = null;
            if(Parent is AdminCore || Parent is AdminHidden)
            {
                sw = new System.Diagnostics.Stopwatch();
                sw.Start();
            }
            Load(TmpList, null, true);

            if (sw != null)
                System.Diagnostics.Trace.WriteLine(string.Format("Loading {0} took {1} ms", node.Name, sw.ElapsedMilliseconds), "Performances");
        }

        private void Load(IEnumerable nodeList, string[] idParts, bool preloadChildren)
        {
            preloadChildren = true;

            if (nodeList != null )
            {
                bool ElementIsLink = (typeof(T).IsSubclassOf(typeof(AdminObjectLink)));
                bool ElementIsReference = (typeof(T).IsSubclassOf(typeof(AdminObjectReference)));
                string Id1Name = null;
                string Id2Name = null;

                if (ElementIsLink)
                {
                    if (idParts != null && idParts.Length == 2)
                    {
                        Id1Name = idParts[0];
                        Id2Name = idParts[1];
                    }
                    else
                    {
                        Type[] ArgumentType = null;

                        for (Type TType = typeof(T); TType != null; TType = TType.BaseType)
                        {
                            if (TType.IsGenericType)
                            {
                                ArgumentType = TType.GetGenericArguments();
                                break;
                            }
                        }

                        Id1Name = AdminObjectLink.GetElementAttributeName(ArgumentType[0]);
                        Id2Name = AdminObjectLink.GetElementAttributeName(ArgumentType[1]);
                    }
                }

                foreach (XmlElement Element in nodeList)
                {

                    string ElementId = null;

                    if (Element.Attributes["id"] != null && !"id".Equals(Id1Name) && !"id".Equals(Id2Name))
                    {
                        if (ElementIsLink)
                            throw new Exception("Cannot load a link from an object tag");

                        int State = 10;

                        ElementId = Element.Attributes["id"].Value;

                        if (Element.Attributes["state"] != null)
                            State = int.Parse(Element.Attributes["state"].Value);

                        if (Element.Attributes["owner"] != null)
                            OwnerId = Element.Attributes["owner"].Value;


                        //TODO: Depends on what you want to do with the list...
                        m_Ids.Add(ElementId, ElementId);

                        if (State>0)
                        {
                            if (preloadChildren)
                            {
                                m_Core.GetAdminObject(ElementId, typeof(T), this);
                            }
                            else
                            {
                                if (!m_Core.m_NotPreloaded.ContainsKey(ElementId))
                                {
                                    m_Core.m_NotPreloaded.Add(ElementId, new Tuple<Type, AdminObject>(typeof(T), this));
                                }
                            }

                            m_Core.AddObjectReference(this.Parent.Id, ElementId);
                        }
                    }
                    else if (ElementIsLink)
                    {
                        // TODO: check for preload usage...
                        XmlAttribute Attr1 = Element.Attributes[Id1Name];
                        XmlAttribute Attr2 = Element.Attributes[Id2Name];
                        string Id1 = (Attr1 == null) ? null : Attr1.Value;
                        string Id2 = (Attr2 == null) ? null : Attr2.Value;

                        string CombinedId = null;
                        AdminObjectLink NewLink = null;

                        if (Id1 == null || Id2 == null || !m_Core.HasAdminObject(CombinedId = AdminObjectLink.GetCombinedId(Id1, Id2)))
                        {
                            // Link must compute its own ids
                            NewLink = (AdminObjectLink)Activator.CreateInstance(typeof(T), new object[] { this });
                            NewLink.Load(Element, Id1, Id2);

                            CombinedId = NewLink.Id;
                            m_Core.SetAdminObject(NewLink);
                        }

                        m_Core.AddObjectReference(this.Parent.Id, CombinedId);  /////////////////  needed to detect last reference lost  
                        m_Ids.Add(CombinedId, CombinedId);
                    }
                    else if (ElementIsReference)
                    {
                        AdminObjectReference NewRef = (AdminObjectReference)Activator.CreateInstance(typeof(T), new object[] { this });

                        NewRef.Load(Element);
                    }
                    else
                    {
                        AdminObject NewObj = (AdminObject)Activator.CreateInstance(typeof(T), new object[] { this });

                        try   // TODO: Check why a Campaigns node is loaded here !
                        {
                            NewObj.Load(Element);

                            if (NewObj.Id == null)
                            {
                                //TODO: use custom attribute to mark the type as 'must use local id'

                                NewObj.Id = Guid.NewGuid().ToString("N");
                                m_Core.SetAdminObject(NewObj);
                            }
                            // TODO: checked, to test   --   if not done, then AdminObjectList contains null entries (with AdminObjectList<FilterPart>, in outbound activities...)
                            else
                            {
                                m_Core.SetAdminObject(NewObj);
                            }

                            m_Core.AddObjectReference(this.Parent.Id, NewObj.Id);  /////////////////  needed to detect last reference lost  
                            m_Ids.Add(NewObj.Id, NewObj.Id);
                        }
                        catch (Exception Ex)
                        {
                            System.Diagnostics.Trace.WriteLine(Ex.ToString());

                            Load(Element.SelectNodes("./" + typeof(T).Name), idParts, preloadChildren);
                        }
                    }
                }
            }

            FireCollectionChanged(NotifyCollectionChangedAction.Reset);

            m_Loaded = true;
        }

        public T this[int index]
        {
            get
            {
                return m_Core.GetAdminObject(m_Ids.Keys[index], typeof(T), this) as T;
            }
        }

        public T this[string index]
        {
            get
            {
                return m_Core.GetAdminObject(m_Ids[index]) as T;
            }
        }

        public override void Dump(System.Text.StringBuilder sb, HashSet<AdminObject> dumpedObjects, string prefix, int indent)
        {
            base.Dump(sb, dumpedObjects, prefix, indent);

            for (int i = 0; i < m_Ids.Count; i++)
            {
                if (indent > 0)
                    sb.Append(new string(' ', indent));

                sb.Append("->");

                m_Core.GetAdminObject(m_Ids.Keys[i]).Dump(sb, dumpedObjects, null, -(indent + 3));
            }
        }

        #region IEnumerable<T> Members

        public new IEnumerator<T> GetEnumerator()
        {
            if (m_Core == null)
                yield break;
            foreach (string Id in m_Ids.Keys)
            {
                AdminObject ao = m_Core.GetAdminObject(Id, typeof(T), this);
                    yield return (T)ao;
            }
        }

        #endregion

        #region ICollection<T> Members

        protected virtual void Add(T item, bool deferNotifications)
        {
            m_Ids.Add(item.Id, item.Id);

            if (item.Parent == null)
                item.Parent = this;

            if (typeof(T).IsSubclassOf(typeof(AdminObjectLink)))
            {
                if (item is AdminObjectLink)
                {
                }
            }
            else if (item is AdminObjectReference)
            {
                item.Parent = this.Parent;
            }
            else
            {
                if (Parent.Id != null) 
                    m_Core.AddObjectReference(Parent.Id, item.Id);
            }
            if (Id != null)
            {
                if (m_PreloadChildren)
                    m_Core.GetAdminObject(Id);
            }

            if (!deferNotifications)
            {
                // Seems to be a bug in datagrid: delete last element then add crashes if not reset
                if (m_Ids.Count == 1)
                    FireCollectionChanged(NotifyCollectionChangedAction.Reset);
                else
                    FireCollectionChanged(NotifyCollectionChangedAction.Add, item);
            }
        }

        public void Add(T item)
        {
            Add(item, false);
        }

        protected virtual AdminObject Add(AdminObject item, bool deferNotifications)
        {
            if (item is T)
            {
                Add((T)item, deferNotifications);
                return item;
            }

#if INTERNAL_DEBUG
            System.Diagnostics.Debug.WriteLine(string.Format("{0} adding foreign {1} - called from {2}", InternalDebug.ObjectFullName[this], InternalDebug.ObjectFullName[item], InternalDebug.CallerName));
            System.Diagnostics.Debug.Indent();
#endif

            if (typeof(T).IsSubclassOf(typeof(AdminObjectLink)))
            {
#if INTERNAL_DEBUG
                System.Diagnostics.Debug.WriteLine(string.Format("Type {0} is a subclass of AdminObjectLink, adding to second collection", typeof(T).Name));
                System.Diagnostics.Debug.Indent();
#endif
                for (int i = 0; i < m_Ids.Keys.Count; i++)
                {
                    string CurrentId = m_Ids.Keys[i];
                    AdminObjectLink Link = (AdminObjectLink)m_Core.GetAdminObject(CurrentId);
                    
                    if (Link.HasReference(item))
                    {
#if INTERNAL_DEBUG
                        System.Diagnostics.Debug.WriteLine(string.Format("Item already referenced by link {0}, aborting", InternalDebug.ObjectFullName[Link]));
                        System.Diagnostics.Debug.Unindent();
                        System.Diagnostics.Debug.Unindent();
#endif
                        return Link;
                    }
                }

#if INTERNAL_DEBUG
                System.Diagnostics.Debug.Unindent();
#endif

                AdminObjectLink NewLink = (AdminObjectLink)Activator.CreateInstance(typeof(T), m_Parent);
                Type ItemType = item.GetType();
                Type T1 = typeof(T).GetProperty("Object1", BindingFlags.NonPublic | BindingFlags.Instance).PropertyType;

                if (T1.Equals(ItemType))
                {
                    NewLink.SetIds(item.Id, Parent.Id);
                }
                else
                {
                    Type T2 = typeof(T).GetProperty("Object2", BindingFlags.NonPublic | BindingFlags.Instance).PropertyType;

                    if (T2.Equals(ItemType))
                    {
                        NewLink.SetIds(m_Parent.Id, item.Id);
                    }
                    else if (ItemType.IsSubclassOf(T1))
                    {
                        NewLink.SetIds(item.Id, m_Parent.Id);
                    }
                    else if (ItemType.IsSubclassOf(T2))
                    {
                        NewLink.SetIds(m_Parent.Id, item.Id);
                    }
                }

                NewLink.Id = AdminObjectLink.GetCombinedId(NewLink.Id1, NewLink.Id2);

                AdminObjectLink PreviouslyDeleted = (AdminObjectLink)m_Core.RemoveDeletedObject(NewLink.Id);

                if (PreviouslyDeleted != null)
                    NewLink.RessurectFrom(PreviouslyDeleted);

                m_Core.SetAdminObject(NewLink);

                Add(NewLink, deferNotifications);
                NewLink.AddLinkSide(item);
                return NewLink;
            }
            else if (typeof(T).IsSubclassOf(typeof(AdminObjectReference)))
            {
                throw new NotSupportedException("Reference lists are not implemented");
            }
            else
            {
                throw new Exception("Don't know how to add object");
            }

#if INTERNAL_DEBUG
            System.Diagnostics.Debug.Unindent();
#endif
            return null;
        }

        public AdminObject AddLinkItem(AdminObject item)
        {
           return Add(item, false);
        }

        public void Add(AdminObject item)
        {
            AddLinkItem(item);
        }

        public void AddRange(IEnumerable items)
        {
            foreach (object Item in items)
            {
                System.Diagnostics.Debug.Assert(Item is AdminObject);

                Add((AdminObject)Item, true);
            }

            FireCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        public override void Clear()
        {
            Clear(false);
        }

        public void Clear(bool dontDeleteOnSave)
        {
            // TODO: Maybe better to do it without GetAdminObject...

            while(m_Ids.Count > 0)
            {
                if (!Remove(m_Core.GetAdminObject(m_Ids.Keys[0]) as T, true, dontDeleteOnSave))
                {
                    System.Diagnostics.Debug.WriteLine(string.Concat("Removing invalid object id ", m_Ids.Keys[0] ?? "<null>"));
                    m_Ids.RemoveAt(0);
                }
            }

            FireCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        public bool Contains(T item)
        {
            return m_Ids.ContainsKey(item.Id);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (string Id in m_Ids.Keys)
                array[arrayIndex++] = (T)m_Core.GetAdminObject(Id, m_MemberType, this);
        }

        public override int Count
        {
            get
            {
                return m_Ids.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override void Remove(object value)
        {
            if (value is T)
                Remove((T)value);
            else if (value is AdminObject)
                Remove((AdminObject)value);
            else
                base.Remove(value);
        }

        public bool Remove(T item)
        {
            return Remove(item, false);
        }

        public bool Remove(T item, bool deferNotification)
        {
            return Remove(item, deferNotification, false);
        }

        public bool Remove(T item, bool deferNotification, bool dontDeleteOnSave)
        {
            if (item == null)
                return false;

#if INTERNAL_DEBUG
            System.Diagnostics.Debug.WriteLine(string.Format("{0} removing {1} - called from {2}", InternalDebug.ClassName[this], InternalDebug.ObjectFullName[item], InternalDebug.CallerName));
            System.Diagnostics.Debug.Indent();
#endif
            if (m_Ids.Remove(item.Id))
            {
                if (item is AdminObjectLink)
                {
#if INTERNAL_DEBUG
                    System.Diagnostics.Debug.WriteLine("Item is AdminObjectLink");
#endif
                    m_Core.RemoveObjectReference(Parent.Id, item.Id, dontDeleteOnSave);
                }
                else if (item is AdminObjectReference)
                {
#if INTERNAL_DEBUG
                    System.Diagnostics.Debug.WriteLine("Item is AdminObjectReference");
#endif
                    if (item.Parent == this.Parent)
                    {
#if INTERNAL_DEBUG
                        System.Diagnostics.Debug.WriteLine("Item has same parent as list, setting to null");
#endif
                        item.Parent = null;
                    }
                }
                else
                {
#if INTERNAL_DEBUG
                    System.Diagnostics.Debug.WriteLine(string.Format("Removing core reference from {0}", InternalDebug.ObjectFullName[Parent]));
#endif
                    m_Core.RemoveObjectReference(Parent.Id, item.Id, dontDeleteOnSave);
                }

                if(!deferNotification)
                    FireCollectionChanged(NotifyCollectionChangedAction.Remove, item);

#if INTERNAL_DEBUG
                System.Diagnostics.Debug.Unindent();
#endif
                return true;
            }

#if INTERNAL_DEBUG
            System.Diagnostics.Debug.Unindent();
#endif
            return false;
        }

        #endregion

        public bool Remove(AdminObject item)
        {
            if (item is T)
                return Remove((T)item);

#if INTERNAL_DEBUG
            System.Diagnostics.Debug.WriteLine(string.Format("{0} removing foreign {1} - called from {2}", InternalDebug.ObjectFullName[this], InternalDebug.ObjectFullName[item], InternalDebug.CallerName));
            System.Diagnostics.Debug.Indent();
#endif

            if (typeof(T).IsSubclassOf(typeof(AdminObjectLink)))
            {
#if INTERNAL_DEBUG
                System.Diagnostics.Debug.WriteLine(string.Format("Type {0} is a subclass of AdminObjectLink, iterating through children", typeof(T).Name));
                System.Diagnostics.Debug.Indent();
#endif
                for (int i = 0; i < m_Ids.Keys.Count; i++)
                {
                    string CurrentId = m_Ids.Keys[i];
                    AdminObjectLink Link = (AdminObjectLink)m_Core.GetAdminObject(CurrentId);

                    if (Link != null && Link.HasReference(item))
                    {
#if INTERNAL_DEBUG
                        System.Diagnostics.Debug.WriteLine(string.Format("Item referenced by link {0}, removing link side", InternalDebug.ObjectFullName[Link]));
                        System.Diagnostics.Debug.Indent();
#endif

                        m_Core.RemoveObjectReference(item.Id, Parent.Id);
                        m_Core.RemoveAdminObject(Link);

                        Link.RemoveLinkSide(item);
                        RemoveId(CurrentId);

                        i--;

#if INTERNAL_DEBUG
                        System.Diagnostics.Debug.Unindent();
#endif
                    }
                }
#if INTERNAL_DEBUG
                System.Diagnostics.Debug.Unindent();
#endif
                return true;
            }
            else if (typeof(T).IsSubclassOf(typeof(AdminObjectReference)))
            {
#if INTERNAL_DEBUG
                System.Diagnostics.Debug.WriteLine(string.Format("Type {0} is a subclass of AdminObjectReference, iterating through children", typeof(T).Name));
                System.Diagnostics.Debug.Indent();
#endif
                foreach (string CurrentId in m_Ids.Keys)
                {
                    AdminObjectReference Ref = (AdminObjectReference)m_Core.GetAdminObject(CurrentId);

                    if (Ref.TargetId == item.Id)
                    {
#if INTERNAL_DEBUG
                        System.Diagnostics.Debug.WriteLine(string.Format("Item referenced by {0}, removing foreign reference", InternalDebug.ObjectFullName[Ref]));
                        System.Diagnostics.Debug.Indent();
#endif
                        Ref.TargetId = null;
                        RemoveId(CurrentId);
#if INTERNAL_DEBUG
                        System.Diagnostics.Debug.Unindent();
                        System.Diagnostics.Debug.Unindent();
#endif
                        return true;
                    }
                }
#if INTERNAL_DEBUG
                System.Diagnostics.Debug.Unindent();
#endif
            }

            return false;
        }

        public bool Remove(string id)
        {
            return Remove(m_Core.GetAdminObject(id));
        }
    }

    public class CheckedObject<T> : System.ComponentModel.INotifyPropertyChanged where T : AdminObject
    {
        private CheckedObjectList<T> m_List;
        private T m_Item;
        private string m_TargetId;

        internal CheckedObject(CheckedObjectList<T> list, T item)
            : this(list, item, item.Id)
        {
        }

        internal CheckedObject(CheckedObjectList<T> list, T item, string targetId)
        {
            m_List = list;
            m_Item = item;
            m_TargetId = targetId;
        }

        public T Item
        {
            get
            {
                return m_Item;
            }
        }

        internal string TargetId
        {
            get
            {
                return m_TargetId;
            }
        }

        public bool IsChecked
        {
            get
            {
                return m_List.IsChecked(this);
            }
            set
            {
                if (m_List.CheckedChanged(this, value))
                {
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("IsChecked"));
                    }
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public class CheckedObjectList<T> : AdminObjectList, IEnumerable<CheckedObject<T>>, ICollection<CheckedObject<T>> where T : AdminObject
    {
        private AdminObjectList<T> m_Source;
        private AdminObjectList m_Target;
        private AdminObject m_DefaultLink;
        private int m_LinkSide;
        private Type m_LinkType;
        private MethodInfo m_TargetAdd;
        private MethodInfo m_TargetRemove;
        private SortedList<string, CheckedObject<T>> m_RefList = new SortedList<string, CheckedObject<T>>();

        public CheckedObjectList(AdminObject parent, AdminObjectList<T> source, AdminObjectList target)
            : this(parent, source, target, null)
        {
        }

        public CheckedObjectList(AdminObject parent, AdminObjectList<T> source, AdminObjectList target, AdminObject defaultLink)
            : base(parent)
        {
            m_Source = source;
            m_Target = target;
            m_DefaultLink = defaultLink;

            Type TargetType = m_Target.GetType();

            if (TargetType.IsGenericType)
            {
                Type ListType = TargetType.GetGenericArguments()[0];

                if (ListType.IsSubclassOf(typeof(AdminObjectLink)))
                {
                    if (m_DefaultLink == null)
                        throw new NotSupportedException();

                    Type Obj1Type = ListType.GetProperty("Object1", BindingFlags.NonPublic | BindingFlags.Instance).PropertyType;

                    if (Obj1Type.IsInstanceOfType(m_DefaultLink) || m_DefaultLink.GetType().IsSubclassOf(Obj1Type))
                    {
                        m_LinkSide = 2;
                    }
                    else
                    {
                        m_LinkSide = 1;
                    }

                    m_LinkType = ListType;
                    m_TargetAdd = TargetType.GetMethod("Add", new Type[] { typeof(T) });
                    m_TargetRemove = TargetType.GetMethod("Remove", new Type[] { typeof(T) });
                }
            }

            m_Source.CollectionChanged += new NotifyCollectionChangedEventHandler(SourceCollectionChanged);
            SourceCollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool IsChecked(CheckedObject<T> item)
        {
            return m_Target.ContainsId(item.TargetId);
        }

        internal bool CheckedChanged(CheckedObject<T> item, bool isChecked)
        {
            bool oldChecked = IsChecked(item);

            if (oldChecked != isChecked)
            {
                if (isChecked)
                {
                    if (m_LinkSide != 0)
                    {
                        m_TargetAdd.Invoke(m_Target, new object[] { item.Item });
                    }
                    else
                    {
                        m_TargetAdd.Invoke(m_Target, new object[] { item.Item });
                    }
                }
                else
                {
                    m_TargetRemove.Invoke(m_Target, new object[] { item.Item });
                }

                return true;
            }

            return false;
        }

        private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            List<CheckedObject<T>> Objects = null;
            NotifyCollectionChangedEventArgs Args = null;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Objects = new List<CheckedObject<T>>();

                    foreach (object NewItem in e.NewItems)
                    {
                        string NewId = ((AdminObject)NewItem).Id;

                        switch (m_LinkSide)
                        {
                            case 1:
                                NewId = AdminObjectLink.GetCombinedId(((AdminObject)NewItem).Id, m_DefaultLink.Id);
                                break;

                            case 2:
                                NewId = AdminObjectLink.GetCombinedId(m_DefaultLink.Id, ((AdminObject)NewItem).Id);
                                break;
                        }

                        m_RefList.Add(NewId, new CheckedObject<T>(this, (T)NewItem, NewId));
                        Objects.Add(m_RefList[NewId]);
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    Objects = new List<CheckedObject<T>>();

                    foreach (object OldItem in e.OldItems)
                    {
                        string OldId = ((AdminObject)OldItem).Id;

                        if (m_RefList.ContainsKey(OldId))
                        {
                            m_Target.Remove((AdminObject)OldItem);
                            Objects.Add(m_RefList[OldId]);

                            m_RefList.Remove(OldId);
                        }
                    }
                    break;

                default:
                    m_RefList = new SortedList<string, CheckedObject<T>>();

                    foreach (T Item in m_Source)
                    {
                        string NewId = ((AdminObject)Item).Id;

                        switch (m_LinkSide)
                        {
                            case 1:
                                NewId = AdminObjectLink.GetCombinedId(((AdminObject)Item).Id, m_DefaultLink.Id);
                                break;

                            case 2:
                                NewId = AdminObjectLink.GetCombinedId(m_DefaultLink.Id, ((AdminObject)Item).Id);
                                break;
                        }

                        m_RefList.Add(Item.Id, new CheckedObject<T>(this, Item, NewId));
                    }

                    for (int i = 0; i < m_Target.Count; i++)
                    {
                        string SearchId;

                        if (m_LinkSide > 0)
                        {
                            if (m_LinkSide == 1)
                                SearchId = ((AdminObjectLink)m_Target[i]).Id1;
                            else
                                SearchId = ((AdminObjectLink)m_Target[i]).Id2;
                        }
                        else
                        {
                            SearchId = ((AdminObject)m_Target[i]).Id;
                        }

                        if (!m_RefList.ContainsKey(SearchId))
                        {
                            m_TargetRemove.Invoke(m_Target, new object[] { m_Target[i] });
                            i--;
                        }
                    }

                    Args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    break;
            }

            if (Args == null)
                Args = new NotifyCollectionChangedEventArgs(e.Action, Objects);

#if INTERNAL_DEBUG
            System.Diagnostics.Debug.WriteLine(string.Format("Collection {0} reflecting source {1} change", InternalDebug.ObjectFullName[this], InternalDebug.ObjectFullName[m_Source]));
#endif
            FireCollectionChanged(Args);
        }

        public override object this[int index]
        {
            get
            {
                return m_RefList[m_RefList.Keys[index]];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public CheckedObject<T> this[string id]
        {
            get
            {
                return m_RefList[id];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        internal override void Load(XmlElement node, PropertyInfo pInfo)
        {
            //TODO: Clean upper side and put exception back on
        }

        #region IEnumerable<CheckedObject<T>> Members

        public new IEnumerator<CheckedObject<T>> GetEnumerator()
        {
            foreach (string Id in m_RefList.Keys)
                yield return m_RefList[Id];
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ICollection<CheckedObject<T>> Members

        public void Add(CheckedObject<T> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(CheckedObject<T> item)
        {
            return m_RefList.ContainsKey(item.Item.Id);
        }

        public void CopyTo(CheckedObject<T>[] array, int arrayIndex)
        {
            m_RefList.Values.CopyTo(array, arrayIndex);
        }

        public override int Count
        {
            get
            {
                return m_RefList.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public bool Remove(CheckedObject<T> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override int IndexOf(object value)
        {
            return m_RefList.IndexOfKey(((CheckedObject<T>)value).Item.Id);
        }

        public override void Dump(System.Text.StringBuilder sb, HashSet<AdminObject> dumpedObjects, string prefix, int indent)
        {
            foreach (CheckedObject<T> Item in m_RefList.Values)
            {
                sb.Append(Item.IsChecked).Append('\t').Append(Item.Item.GetType().Name).Append(" ").Append(Item.Item.Id);
                sb.AppendLine();
            }
        }
    }
}
