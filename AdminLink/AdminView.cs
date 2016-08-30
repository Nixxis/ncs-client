using System;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows;
using System.Collections;
using System.Linq;

namespace Nixxis.Client.Admin
{
    public interface IDeferRefreshTarget
    {
        void DeferRefreshDisposed();
    }

    internal class DeferRefresh : IDisposable
    {
        IDeferRefreshTarget m_Target;

        private DeferRefresh()
        {
        }

        ~DeferRefresh()
        {
            Dispose();
        }

        public DeferRefresh(IDeferRefreshTarget target)
        {
            m_Target = target;
        }

        public void Dispose()
        {
            IDeferRefreshTarget CurrentTarget = System.Threading.Interlocked.Exchange(ref m_Target, null);

            if (CurrentTarget != null)
            {
                GC.SuppressFinalize(this);

                CurrentTarget.DeferRefreshDisposed();
            }
        }
    }

    public abstract class AdminCheckedLinkList : IEnumerable, INotifyCollectionChanged
    {
        public abstract event NotifyCollectionChangedEventHandler CollectionChanged;

        public abstract IEnumerator GetEnumerator();

        public abstract BaseAdminCheckedLinkItem FindItem(AdminObject link);
        public abstract BaseAdminCheckedLinkItem FindLink(AdminObject link);
    }

    public abstract class BaseAdminCheckedLinkItem : IComparable, INotifyPropertyChanged
    {
            protected AdminCore m_Core;
            protected string m_FullItemId;
            protected string m_LinkItemId;
            protected bool m_ForwardPropertyChanged;
            private INotifyPropertyChanged m_FullItemPropertyChanged;
            private INotifyPropertyChanged m_LinkItemPropertyChanged;

            protected BaseAdminCheckedLinkItem(AdminCore core, string fullItemId, string linkItemId)
            {
                m_Core = core;
                m_FullItemId = fullItemId;
                m_LinkItemId = linkItemId;
            }

            ~BaseAdminCheckedLinkItem()
            {
                if (m_FullItemPropertyChanged != null)
                    m_FullItemPropertyChanged.PropertyChanged -= FullItem_PropertyChanged;

                if (m_LinkItemPropertyChanged != null)
                    m_LinkItemPropertyChanged.PropertyChanged -= FullItem_PropertyChanged;
            }

            internal string FullItemId
            {
                get
                {
                    return m_FullItemId;
                }
            }

            internal string LinkItemId
            {
                get
                {
                    return m_LinkItemId;
                }
            }

            public int CompareTo(object obj)
            {
                return m_LinkItemId.CompareTo(((BaseAdminCheckedLinkItem)obj).m_LinkItemId);
            }

            public AdminObject Item
            {
                get
                {
                    AdminObject FullItem = m_Core.GetAdminObject(m_FullItemId) as AdminObject;

                    if (m_ForwardPropertyChanged && FullItem != null && !FullItem.Equals(m_FullItemPropertyChanged) && FullItem is INotifyPropertyChanged)
                    {
                        if (m_FullItemPropertyChanged != null)
                            m_FullItemPropertyChanged.PropertyChanged -= FullItem_PropertyChanged;

                        m_FullItemPropertyChanged = (INotifyPropertyChanged)FullItem;

                        m_FullItemPropertyChanged.PropertyChanged += FullItem_PropertyChanged;
                    }

                    return FullItem;
                }
            }

            void FullItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(new PropertyChangedEventArgs("Item"));
            }

            public AdminObject Link
            {
                get
                {
                    AdminObject LinkItem = m_Core.GetAdminObject(m_LinkItemId) as AdminObject;

                    if (m_ForwardPropertyChanged && LinkItem != null && !LinkItem.Equals(m_LinkItemPropertyChanged) && LinkItem is INotifyPropertyChanged)
                    {
                        if (m_LinkItemPropertyChanged != null)
                            m_LinkItemPropertyChanged.PropertyChanged -= LinkItem_PropertyChanged;

                        m_LinkItemPropertyChanged = (INotifyPropertyChanged)LinkItem;

                        m_LinkItemPropertyChanged.PropertyChanged += LinkItem_PropertyChanged;
                    }

                    return LinkItem;
                }
            }

            void LinkItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(new PropertyChangedEventArgs("Link"));
            }

            protected abstract bool IsChildOf(BaseAdminCheckedLinkItem item);

            public abstract IEnumerable<BaseAdminCheckedLinkItem> Children
            {
                get;
            }

            internal void OnPropertyChanged(PropertyChangedEventArgs args)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, args);
            }

            public event PropertyChangedEventHandler PropertyChanged;
    }

    public class AdminCheckedLinkList<BaseT, LinkT> : AdminCheckedLinkList, IList, IList<AdminCheckedLinkList<BaseT, LinkT>.AdminCheckedLinkItem>, INotifyCollectionChanged, IWeakEventListener//, ICollectionViewFactory
        where BaseT : AdminObject
        where LinkT : AdminObjectLink
    {
        public class AdminCheckedLinkItem : BaseAdminCheckedLinkItem
        {
            private AdminCheckedLinkList<BaseT, LinkT> m_View;
            private bool m_HasIndirectLink;

            internal AdminCheckedLinkItem(AdminCore core, AdminCheckedLinkList<BaseT, LinkT> view, string fullItemId, string linkItemId, bool hasIndirectLink)
                : base(core, fullItemId, linkItemId)
            {
                m_HasIndirectLink = hasIndirectLink;
                m_View = view;
                m_ForwardPropertyChanged = m_View.ForwardItemPropertyChanged;
            }

            public new BaseT Item
            {
                get
                {
                    return base.Item as BaseT;
                }
            }

            public new LinkT Link
            {
                get
                {
                    return base.Link as LinkT;
                }
            }

            public bool? HasLink
            {
                get
                {
                    if (m_Core.HasAdminObject(m_LinkItemId))
                        return true;

                    if (m_HasIndirectLink)
                        return null;

                    return false;
                }
                set
                {
                    bool LinkExisted = m_Core.HasAdminObject(m_LinkItemId);
                    bool NewValue = (value.HasValue) ? (value.Value ? true : ((!LinkExisted && m_HasIndirectLink) ? true : false)) : (LinkExisted ? false : m_HasIndirectLink);
                    if (LinkExisted != NewValue)
                    {
                        if (NewValue)
                        {
                            m_View.m_Links.Add(m_Core.GetAdminObject(m_FullItemId));
                        }
                        else
                        {
                            m_View.m_Links.Remove(m_Core.GetAdminObject(m_FullItemId));
                        }
                    }

                    OnPropertyChanged(new PropertyChangedEventArgs("HasLink"));
                    OnPropertyChanged(new PropertyChangedEventArgs("Link"));
                }
            }

            protected override bool IsChildOf(BaseAdminCheckedLinkItem item)
            {
                if (m_View.m_ParentIdProperty == null)
                    return false;

                string ParentId = (string)m_View.m_ParentIdProperty.GetValue(Item, null);

                return (string.Equals(item.FullItemId, ParentId));
            }

            public override IEnumerable<BaseAdminCheckedLinkItem> Children
            {
                get
                {
                    return System.Linq.Queryable.Where<AdminCheckedLinkList<BaseT, LinkT>.AdminCheckedLinkItem>(System.Linq.Queryable.AsQueryable<AdminCheckedLinkList<BaseT, LinkT>.AdminCheckedLinkItem>(m_View), (item) => item.IsChildOf(this));
                }
            }

            public override string ToString()
            {
                return string.Concat("LinkItem: ", m_LinkItemId);
            }
        }
        
        private IEnumerable m_FullList;
        private IEnumerable<AdminObjectList> m_IndirectLists;
        private string[] m_IndirectPropertyNames;
        private bool m_FullListIsRefList;
        private AdminObjectList<LinkT> m_Links;
        private AdminObject m_ReferentObject;
        private Type m_ReferentType;
        private int m_ReferentSide;
        private int m_ListItemSide;
        private List<AdminCheckedLinkItem> m_List = new List<AdminCheckedLinkItem>();
        private int m_CurrentPosition = -1;
        private bool FreezeCollectionChanged = false;
        private PropertyInfo m_ParentIdProperty = null;
        private List<object> m_CollectionChangedSources = new List<object>();

        public bool ForwardItemPropertyChanged { get; set; }


        public AdminCheckedLinkList(IEnumerable<BaseT> fullList, AdminObjectList<LinkT> linkList, AdminObject referent)
            : this(fullList, null, null, linkList, referent, null)
        { }

        public AdminCheckedLinkList(IEnumerable<BaseT> fullList, AdminObjectList<LinkT> linkList, AdminObject referent, string parentIdPropertyName)
        {
            Init(fullList, null, null, linkList, referent, parentIdPropertyName);
            Refresh();
        }

        public AdminCheckedLinkList(IEnumerable<BaseT> fullList, IEnumerable<AdminObjectList> indirectLists, string indirectPropertyName, AdminObjectList<LinkT> linkList, AdminObject referent, string parentIdPropertyName)
        {
            Init(fullList, indirectLists, indirectPropertyName, linkList, referent, parentIdPropertyName);
            Refresh();
        }

        void Init(IEnumerable fullList, IEnumerable<AdminObjectList> indirectLists, string indirectPropertyName, AdminObjectList<LinkT> linkList, AdminObject referent, string parentIdPropertyName)
        {
            List<AdminObject> tempList = new List<AdminObject>();
            foreach (AdminObject ao in fullList)
                if(!ao.IsDummy)
                    tempList.Add(ao);

            m_FullList = tempList;
            m_Links = linkList;
            m_ReferentObject = referent;
            m_IndirectLists = indirectLists;



            if (!string.IsNullOrEmpty(parentIdPropertyName))
            {
                m_ParentIdProperty = typeof(BaseT).GetProperty(parentIdPropertyName);
            }

            m_ReferentType = m_ReferentObject.GetType();

            for (Type InnerLinkType = typeof(LinkT); InnerLinkType.BaseType != null; InnerLinkType = InnerLinkType.BaseType)
            {
                if (InnerLinkType.BaseType.Equals(typeof(AdminObjectLink)) && InnerLinkType.IsGenericType)
                {
                    Type[] GenericArgs = InnerLinkType.GetGenericArguments();

                    if (GenericArgs[0].Equals(m_ReferentType))
                    {
                        m_ReferentSide = 1;
                        m_ListItemSide = 2;
                    }
                    else if (GenericArgs[1].Equals(m_ReferentType))
                    {
                        m_ReferentSide = 2;
                        m_ListItemSide = 1;
                    }
                    else if (m_ReferentType.IsSubclassOf(GenericArgs[0]))
                    {
                        m_ReferentSide = 1;
                        m_ListItemSide = 2;
                    }
                    else if (m_ReferentType.IsSubclassOf(GenericArgs[1]))
                    {
                        m_ReferentSide = 2;
                        m_ListItemSide = 1;
                    }


                    if (!GenericArgs[m_ListItemSide - 1].Equals(typeof(BaseT)))
                        throw new InvalidCastException();

                    break;
                }
            }

            if (m_FullList is INotifyCollectionChanged)
            {
                CollectionChangedEventManager.AddListener((INotifyCollectionChanged)m_FullList, this);
            }

            if (!string.IsNullOrEmpty(indirectPropertyName))
                m_IndirectPropertyNames = indirectPropertyName.Split('.');

            m_CollectionChangedSources.Add(m_Links);
            CollectionChangedEventManager.AddListener(m_Links, this);
        }

        public void Refresh()
        {
            SortedList<string, string> IndirectIds = new SortedList<string,string>();

            if (m_IndirectLists != null)
            {
                PropertyInfo[] PInfos = null;

                if (m_IndirectLists is INotifyCollectionChanged && !m_CollectionChangedSources.Contains(m_IndirectLists))
                {
                    m_CollectionChangedSources.Add(m_IndirectLists);
                    CollectionChangedEventManager.AddListener((INotifyCollectionChanged)m_IndirectLists, this);
                }

                foreach (AdminObjectList Indirect in m_IndirectLists)
                {
                    if (Indirect is INotifyCollectionChanged && !m_CollectionChangedSources.Contains(Indirect))
                    {
                        m_CollectionChangedSources.Add(Indirect);
                        CollectionChangedEventManager.AddListener((INotifyCollectionChanged)Indirect, this);
                    }

                    if (PInfos == null)
                    {
                        PInfos = new PropertyInfo[m_IndirectPropertyNames.Length];

                        for (int i = 0; i < PInfos.Length; i++)
                        {
                            PInfos[i] = ((i == 0) ? Indirect.MemberType : PInfos[i - 1].PropertyType).GetProperty(m_IndirectPropertyNames[i]);

                            if(PInfos[i] == null)
                                throw new InvalidOperationException(string.Concat("Cannot resolve ", string.Join(".", m_IndirectPropertyNames)));
                        }
                    }

                    if (Indirect.Equals(m_Links))
                        continue;

                    foreach(AdminObject IndirectItem in Indirect)
                    {
                        AdminObject TmpObj = null;

                        if (IndirectItem.Equals(m_Links))
                            continue;

                        for (int i = 0; i < PInfos.Length; i++)
                        {
                            TmpObj = (AdminObject)PInfos[i].GetValue((i == 0) ? IndirectItem : TmpObj, null);
                        }

                        if (TmpObj is AdminObjectList)
                        {
                            if (!TmpObj.Equals(m_Links))
                            {
                                if (TmpObj is INotifyCollectionChanged && !m_CollectionChangedSources.Contains(TmpObj))
                                {
                                    m_CollectionChangedSources.Add(TmpObj);
                                    CollectionChangedEventManager.AddListener((INotifyCollectionChanged)TmpObj, this);
                                }

                                if (((AdminObjectList)TmpObj).MemberType.IsSubclassOf(typeof(AdminObjectLink)))
                                {
                                    int IdSide = 0;

                                    foreach (AdminObjectLink IndirectLink in (AdminObjectList)TmpObj)
                                    {
                                        if (IdSide == 0)
                                        {
                                            if (IndirectLink.Type1.Equals(typeof(BaseT)))
                                                IdSide = 1;
                                            else
                                                IdSide = 2;
                                        }

                                        string IndirectId = ((IdSide == 1) ? IndirectLink.Id1 : IndirectLink.Id2);

                                        if (!IndirectIds.ContainsKey(IndirectId))
                                            IndirectIds.Add(IndirectId, IndirectId);
                                    }
                                }
                                else
                                {
                                    foreach (AdminObject IndirectChild in (AdminObjectList)TmpObj)
                                    {
                                        if (!IndirectIds.ContainsKey(IndirectChild.Id))
                                            IndirectIds.Add(IndirectChild.Id, IndirectChild.Id);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!IndirectIds.ContainsKey(TmpObj.Id))
                                IndirectIds.Add(TmpObj.Id, TmpObj.Id);
                        }
                    }
                }
            }

            m_List.Clear();

            if (m_FullListIsRefList)
            {
                foreach (AdminObjectReference Ref in m_FullList)
                {
                    m_List.Add(new AdminCheckedLinkItem(Ref.Core, this, Ref.TargetId, AdminObjectLink.GetCombinedId((m_ReferentSide == 1) ? m_ReferentObject.Id : Ref.TargetId, (m_ReferentSide == 1) ? Ref.TargetId : m_ReferentObject.Id), IndirectIds.ContainsKey(Ref.TargetId)));
                }
            }
            else if (m_FullList is AdminObjectList)
            {
                foreach (string FullId in ((AdminObjectList)m_FullList).Ids)
                {
                    m_List.Add(new AdminCheckedLinkItem(((AdminObjectList)m_FullList).AdminCore, this, FullId, AdminObjectLink.GetCombinedId((m_ReferentSide == 1) ? m_ReferentObject.Id : FullId, (m_ReferentSide == 1) ? FullId : m_ReferentObject.Id), IndirectIds.ContainsKey(FullId)));
                }
            }
            else 
            {
                foreach (BaseT Item in m_FullList)
                {
                    m_List.Add(new AdminCheckedLinkItem(Item.AdminCore, this, Item.Id, AdminObjectLink.GetCombinedId((m_ReferentSide == 1) ? m_ReferentObject.Id : Item.Id, (m_ReferentSide == 1) ? Item.Id : m_ReferentObject.Id), IndirectIds.ContainsKey(Item.Id)));
                }
            }

            m_List.Sort();

            FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public override BaseAdminCheckedLinkItem FindItem(AdminObject link)
        {
            foreach (AdminCheckedLinkItem LinkItem in m_List)
            {
                if (LinkItem.FullItemId == link.Id)
                    return LinkItem;
            }

            return null;
        }

        public override BaseAdminCheckedLinkItem FindLink(AdminObject link)
        {
            foreach (AdminCheckedLinkItem LinkItem in m_List)
            {
                if (LinkItem.LinkItemId == link.Id)
                    return LinkItem;
            }

            return null;
        }

        public int IndexOf(AdminCheckedLinkList<BaseT, LinkT>.AdminCheckedLinkItem item)
        {
            return m_List.IndexOf(item);
        }

        public void Insert(int index, AdminCheckedLinkList<BaseT, LinkT>.AdminCheckedLinkItem item)
        {
            m_List.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_List.RemoveAt(index);
        }

        public AdminCheckedLinkList<BaseT, LinkT>.AdminCheckedLinkItem this[int index]
        {
            get
            {
                return m_List[index];
            }
            set
            {
                m_List[index] = value;
            }
        }

        public void Add(AdminCheckedLinkList<BaseT, LinkT>.AdminCheckedLinkItem item)
        {
            m_List.Add(item);
        }

        public void Clear()
        {
            m_List.Clear();
        }

        public bool Contains(AdminCheckedLinkList<BaseT, LinkT>.AdminCheckedLinkItem item)
        {
            return m_List.Contains(item);
        }

        public void CopyTo(AdminCheckedLinkList<BaseT, LinkT>.AdminCheckedLinkItem[] array, int arrayIndex)
        {
            for (int i = 0; i < m_List.Count; i++)
                array.SetValue(m_List[i], i + arrayIndex);
        }

        public int Count
        {
            get { return m_List.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(AdminCheckedLinkList<BaseT, LinkT>.AdminCheckedLinkItem item)
        {
            return m_List.Remove(item);
        }

        IEnumerator<AdminCheckedLinkList<BaseT, LinkT>.AdminCheckedLinkItem> IEnumerable<AdminCheckedLinkList<BaseT, LinkT>.AdminCheckedLinkItem>.GetEnumerator()
        {
            for (int i = 0; i < m_List.Count; i++)
                yield return m_List[i];
        }

        public override IEnumerator GetEnumerator()
        {
            for (int i = 0; i < m_List.Count; i++)
                yield return m_List[i];
        }

        private void FireCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null && !FreezeCollectionChanged)
                CollectionChanged(this, args);
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        public int Add(object value)
        {
            Add((AdminCheckedLinkItem)value);

            return -1;
        }

        public bool Contains(object value)
        {
            return Contains((AdminCheckedLinkItem)value);
        }

        public int IndexOf(object value)
        {
            return IndexOf((AdminCheckedLinkItem)value);
        }

        public void Insert(int index, object value)
        {
            Insert(index, (AdminCheckedLinkItem)value);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            Remove((AdminCheckedLinkItem)value);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (AdminCheckedLinkItem)value;
            }
        }

        public void CopyTo(Array array, int index)
        {
            for (int i = 0; i < m_List.Count; i++)
                array.SetValue(m_List[i], i + index);
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (e is NotifyCollectionChangedEventArgs)
            {
                NotifyCollectionChangedEventArgs EvtArgs = (NotifyCollectionChangedEventArgs)e;

                if (sender.Equals(m_Links))
                {
                    if (EvtArgs.Action == NotifyCollectionChangedAction.Add && EvtArgs.NewItems != null)
                    {
                        foreach (LinkT Item in EvtArgs.NewItems)
                        {
                            foreach (AdminCheckedLinkItem LinkItem in m_List)
                            {
                                if (LinkItem.LinkItemId == Item.Id)
                                {
                                    LinkItem.OnPropertyChanged(new PropertyChangedEventArgs("HasLink"));
                                    LinkItem.OnPropertyChanged(new PropertyChangedEventArgs("Link"));
                                }
                            }
                        }
                    }
                    else if (EvtArgs.Action == NotifyCollectionChangedAction.Remove && EvtArgs.OldItems != null)
                    {
                        foreach (LinkT Item in EvtArgs.OldItems)
                        {
                            foreach (AdminCheckedLinkItem LinkItem in m_List)
                            {
                                if (LinkItem.LinkItemId == Item.Id)
                                {
                                    LinkItem.OnPropertyChanged(new PropertyChangedEventArgs("HasLink"));
                                    LinkItem.OnPropertyChanged(new PropertyChangedEventArgs("Link"));
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (AdminCheckedLinkItem LinkItem in m_List)
                        {
                            LinkItem.OnPropertyChanged(new PropertyChangedEventArgs("HasLink"));
                            LinkItem.OnPropertyChanged(new PropertyChangedEventArgs("Link"));
                        }
                    }
                }
                else
                {
                    Refresh();
                }
            }

            return true;
        }
    }
}
