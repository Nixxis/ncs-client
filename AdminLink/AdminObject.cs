using System;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using Nixxis;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Nixxis.Client.Admin
{
    public class AdminLoadAttribute : Attribute
    {
        public string Path;
        public string[] IdParts;
        public bool SkipLoad = false;
    }

    public class AdminSaveAttribute : Attribute
    {
        public bool SkipSave = false;
    }

    public class AdminDeleteAttribute : Attribute
    {
        public bool SkipDelete = false;
        public bool DeleteByInactivation = false;

        public AdminDeleteAttribute()
        { }
    }

    public delegate void LoadProgressDelegate(string description);
    public delegate void DetailedLoadProgressDelegate(int progress, string description, string progressDescription);


    public abstract class AdminObject : System.ComponentModel.INotifyPropertyChanged, IDisplayTextProvider
    {
        public static event LoadProgressDelegate LoadProgressEvent;
        public static event DetailedLoadProgressDelegate DetailedLoadProgressEvent;

        private class AdminFieldValue
        {
            private object m_CurrentValue;

            internal bool IsModified { get; private set; }
            internal string Name { get; private set; }
            internal byte[] OriginalTimeStamp { get; private set; }
            internal object OriginalValue { get; private set; }
            internal bool IsReadOnly { get; set; }

            internal object CurrentValue
            {
                get
                {
                    return m_CurrentValue;
                }
                set
                {
                    m_CurrentValue = value;

                    IsModified = !object.Equals(OriginalValue, m_CurrentValue);
                }
            }

            internal void Save(XmlNode node, bool saveNullValues)
            {
                if (CurrentValue != null)
                {
                    string XmlValue = "";
                    string TypeName = null;
                    XmlNode XmlNodeValue = null;

                    if (m_CurrentValue is string)
                    {
                        XmlValue = (string)m_CurrentValue;
                    }
                    // TODO:  Check if wee need full docs
                    else if (m_CurrentValue is XmlDocument)
                    {

                        XmlNodeValue = node.OwnerDocument.ImportNode(((XmlDocument)m_CurrentValue).DocumentElement, true);
                        TypeName = "XmlDocument";
                    }
                    else
                    {
                        Type ValueType = m_CurrentValue.GetType();
                        MethodInfo Converter;

                        if (ValueType.IsEnum)
                        {
                            Converter = typeof(XmlConvert).GetMethod("ToString", BindingFlags.Static | BindingFlags.Public, null, new Type[] { Enum.GetUnderlyingType(ValueType) }, null);
                            TypeName = Enum.GetUnderlyingType(ValueType).Name;
                        }
                        else
                        {
                            Converter = typeof(XmlConvert).GetMethod("ToString", BindingFlags.Static | BindingFlags.Public, null, new Type[] { ValueType }, null);
                            TypeName = ValueType.Name;
                        }

                        if (Converter != null)
                        {
                            XmlValue = (string)Converter.Invoke(null, new object[] { m_CurrentValue });
                        }
                        else
                        {
                            ContactRoute.DiagnosticHelpers.DebugIfPossible();
                        }
                    }

                    if (!string.IsNullOrEmpty(TypeName))
                    {
                        XmlAttribute TypeAttr = node.OwnerDocument.CreateAttribute("type");

                        TypeAttr.Value = TypeName;
                        node.Attributes.Append(TypeAttr);
                    }

                    if (XmlNodeValue != null)
                        node.AppendChild(XmlNodeValue);
                    else
                        node.AppendChild(node.OwnerDocument.CreateTextNode(XmlValue));
                }
                else if(saveNullValues)
                {
                    XmlAttribute NilAttr = node.OwnerDocument.CreateAttribute("xsi", "nil", System.Xml.Schema.XmlSchema.InstanceNamespace);

                    NilAttr.Value = "true";
                    node.Attributes.Append(NilAttr);
                }
            }

            internal AdminFieldValue(string name, object value, byte[] timeStamp)
            {
                Name = name;
                m_CurrentValue = value;
                OriginalTimeStamp = timeStamp;
                IsModified = true;
            }

            internal AdminFieldValue(string name, object value)
                : this(name, value, null)
            {
            }

            public void DoneLoading()
            {
                OriginalValue = CurrentValue;
                IsModified = false;
            }

            public void MarkAsModified()
            {
                IsModified = true;
            }

        }

        private class AdminFieldCollection : System.Collections.ObjectModel.KeyedCollection<string, AdminFieldValue>
        {
            protected override string GetKeyForItem(AdminFieldValue item)
            {
                return item.Name;
            }
        }


        protected AdminCore m_Core;
        protected AdminObject m_Parent;
        protected bool m_Loaded;
        protected bool m_Deleted;
        protected string m_Id;
        protected int m_State = 10;
        protected string m_OwnerId;
        protected DateTime m_CreationTime = DateTime.MinValue;
        protected DateTime m_ModifyTime = DateTime.MinValue;
        internal string m_CreatorId;
        internal string m_ModificatorId;
        protected bool m_IsPartial = false;

        private AdminFieldCollection m_Fields = new AdminFieldCollection();

        public static bool IsDummyId(string id)
        {
            return id.StartsWith("dummy") && id.EndsWith("+");
        }

        public bool IsDummy
        {
            get
            {
                return (!string.IsNullOrEmpty(m_Id) && IsDummyId(m_Id) ); 
            }
        }

        public bool IsPartial
        {
            get
            {
                return m_IsPartial;
            }
            set
            {
                m_IsPartial = value;
            }
        }


        public AdminCore AdminCore
        {
            get
            {
                return m_Core;
            }
        }

        public virtual string Id
        {
            get
            {
                return m_Id;
            }
            internal set
            {
                m_Id = value;
            }
        }

        public virtual int State
        {
            get
            {
                return m_State;
            }
            set
            {
                m_State = value;

                SetFieldValue<int>("State", m_State);
            }
        }

        public virtual string OwnerId
        {
            get
            {
                return m_OwnerId;
            }
            set
            {
                if (!string.IsNullOrEmpty(m_OwnerId))
                {
                    AdminObject Owner = m_Core.GetAdminObject(m_OwnerId);

                    if (Owner != null)
                        Owner.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(OnOwnerPropertyChanged);
                }

                m_OwnerId = value;

                if (!string.IsNullOrEmpty(m_OwnerId))
                {
                    AdminObject Owner = m_Core.GetAdminObject(m_OwnerId);

                    if (Owner != null)
                        Owner.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OnOwnerPropertyChanged);
                }

                SetFieldValue<string>("OwnerId", m_OwnerId);
            }
        }

        protected virtual void OnOwnerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("DisplayText") || e.PropertyName.Equals("ShortDisplayText") || e.PropertyName.Equals("TypedDisplayText"))
                FirePropertyChanged(e.PropertyName);
        } 

        public virtual bool IsSystemWithValidOwner
        {
            get
            {
                return IsSystem && m_Core.HasAdminObject(m_OwnerId);
            }
        }

        public virtual bool IsSystem
        {
            get
            {
                return (!string.IsNullOrEmpty(m_OwnerId) && m_Core != null);
            }
        }

        internal virtual bool? Has_FullControlFlag(string rightid)
        {
            if (rightid == "admin+++++++++++++++++++++++++++")
            {

                bool? defaultSettings = m_Core.DefaultSettings.Has_FullControlFlag("admin+++++++++++++++++++++++++++");
                if (defaultSettings == null || defaultSettings.Value)
                {
                    bool? defaultSettings2 = null;
                    if (this is Queue)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Has_FullControlFlag("adminqueues+++++++++++++++++++++");
                    }
                    else if (this is Activity)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Has_FullControlFlag("adminactivities+++++++++++++++++");
                    }
                    else if (this is Campaign)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Has_FullControlFlag("admincampaigns++++++++++++++++++");
                    }
                    else if (this is Agent)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Has_FullControlFlag("adminagents+++++++++++++++++++++");
                    }
                    else if (this is Team)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Has_FullControlFlag("adminteams++++++++++++++++++++++");
                    }
                    else
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Has_FullControlFlag("adminothers+++++++++++++++++++++");
                    }
                    if (defaultSettings2.HasValue)
                    {
                        return defaultSettings2.Value;
                    }
                }
                else
                    return false;


                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;

            }
            else
            {
                bool? defaultSettings = (m_Core.DefaultSettings).Has_FullControlFlag(rightid);
                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;

            }
        }
        internal virtual bool? Has_PowerFlag(string rightid)
        {
            if (rightid == "admin+++++++++++++++++++++++++++")
            {

                bool? defaultSettings = m_Core.DefaultSettings.Has_PowerFlag("admin+++++++++++++++++++++++++++");
                if (defaultSettings == null || defaultSettings.Value)
                {
                    bool? defaultSettings2 = null;
                    if (this is Queue)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Has_PowerFlag("adminqueues+++++++++++++++++++++");
                    }
                    else if (this is Activity)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Has_PowerFlag("adminactivities+++++++++++++++++");
                    }
                    else if (this is Campaign)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Has_PowerFlag("admincampaigns++++++++++++++++++");
                    }
                    else if (this is Agent)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Has_PowerFlag("adminagents+++++++++++++++++++++");
                    }
                    else if (this is Team)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Has_PowerFlag("adminteams++++++++++++++++++++++");
                    }
                    else
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Has_PowerFlag("adminothers+++++++++++++++++++++");
                    }
                    if (defaultSettings2.HasValue)
                    {
                        return defaultSettings2.Value;
                    }
                }
                else
                    return false;


                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;
            }
            else
            {
                bool? defaultSettings = m_Core.DefaultSettings.Has_PowerFlag(rightid);
                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;

            }


        }
        internal virtual bool? Is_Deletable(string rightid)
        {
            if (rightid == "admin+++++++++++++++++++++++++++")
            {
                // TODO: this is not really clean...
                if (m_Id != null && m_Id.Contains("+"))
                    return false;

                bool? defaultSettings = m_Core.DefaultSettings.Is_Deletable(rightid);
                if (defaultSettings == null || defaultSettings.Value)
                {
                    bool? defaultSettings2 = null;
                    if (this is Queue)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Deletable("adminqueues+++++++++++++++++++++");
                    }
                    else if (this is Activity)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Deletable("adminactivities+++++++++++++++++");
                    }
                    else if (this is Campaign)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Deletable("admincampaigns++++++++++++++++++");
                    }
                    else if (this is Agent)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Deletable("adminagents+++++++++++++++++++++");
                    }
                    else if (this is Team)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Deletable("adminteams++++++++++++++++++++++");
                    }
                    else
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Deletable("adminothers+++++++++++++++++++++");
                    }
                    if (defaultSettings2.HasValue)
                    {
                        return defaultSettings2.Value;
                    }
                }
                else
                    return false;


                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;

            }
            else
            {
                bool? defaultSettings = (m_Core.DefaultSettings).Is_Deletable(rightid);
                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;

            }

        }
        internal virtual bool? Is_Modifiable(string rightid)
        {
            if (rightid == "admin+++++++++++++++++++++++++++")
            {


                bool? defaultSettings = m_Core.DefaultSettings.Is_Modifiable(rightid);

                if (defaultSettings == null || defaultSettings.Value)
                {
                    bool? defaultSettings2 = null;
                    if (this is Queue)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Modifiable("adminqueues+++++++++++++++++++++");
                    }
                    else if (this is Activity)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Modifiable("adminactivities+++++++++++++++++");
                    }
                    else if (this is Campaign)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Modifiable("admincampaigns++++++++++++++++++");
                    }
                    else if (this is Agent)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Modifiable("adminagents+++++++++++++++++++++");
                    }
                    else if (this is Team)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Modifiable("adminteams++++++++++++++++++++++");
                    }
                    else
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Modifiable("adminothers+++++++++++++++++++++");
                    }
                    if (defaultSettings2.HasValue)
                    {
                        return defaultSettings2.Value;
                    }
                }
                else
                    return false;

                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;

            }
            else
            {
                bool? defaultSettings = m_Core.DefaultSettings.Is_Modifiable(rightid);
                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;

            }

        }
        internal virtual bool? Is_RighstHandlingAllowed(string rightid)
        {
            if (rightid == "admin+++++++++++++++++++++++++++")
            {

                bool? defaultSettings = m_Core.DefaultSettings.Is_RighstHandlingAllowed(rightid);

                if (defaultSettings == null || defaultSettings.Value)
                {
                    bool? defaultSettings2 = null;
                    if (this is Queue)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_RighstHandlingAllowed("adminqueues+++++++++++++++++++++");
                    }
                    else if (this is Activity)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_RighstHandlingAllowed("adminactivities+++++++++++++++++");
                    }
                    else if (this is Campaign)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_RighstHandlingAllowed("admincampaigns++++++++++++++++++");
                    }
                    else if (this is Agent)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_RighstHandlingAllowed("adminagents+++++++++++++++++++++");
                    }
                    else if (this is Team)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_RighstHandlingAllowed("adminteams++++++++++++++++++++++");
                    }
                    else
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_RighstHandlingAllowed("adminothers+++++++++++++++++++++");
                    }

                    if (defaultSettings2.HasValue)
                    {
                        return defaultSettings2.Value;
                    }
                }
                else
                    return false;

                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;

            }
            else
            {
                bool? defaultSettings = m_Core.DefaultSettings.Is_RighstHandlingAllowed(rightid);
                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;

            }

        }
        public virtual bool? Is_Visible(string rightid)
        {
            if (rightid == "admin+++++++++++++++++++++++++++")
            {
                bool? defaultSettings = m_Core.DefaultSettings.Is_Visible(rightid);
                if (defaultSettings == null || defaultSettings.Value)
                {
                    bool? defaultSettings2 = null;
                    if (this is Queue)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Visible("adminqueues+++++++++++++++++++++");
                    }
                    else if (this is Activity)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Visible("adminactivities+++++++++++++++++");
                    }
                    else if (this is Campaign)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Visible("admincampaigns++++++++++++++++++");
                    }
                    else if (this is Agent)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Visible("adminagents+++++++++++++++++++++");
                    }
                    else if (this is Team)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Visible("adminteams++++++++++++++++++++++");
                    }
                    else
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Visible("adminothers+++++++++++++++++++++");
                    }
                    if (defaultSettings2.HasValue)
                    {
                        return defaultSettings2.Value;
                    }
                }
                else
                    return false;

                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;
            }
            else
            {
                bool? defaultSettings = m_Core.DefaultSettings.Is_Visible(rightid);
                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;
            }
        }
        public virtual bool? Is_Listable(string rightid)
        {
            if (rightid == "admin+++++++++++++++++++++++++++")
            {
                bool? defaultSettings = m_Core.DefaultSettings.Is_Listable(rightid);
                if (defaultSettings == null || defaultSettings.Value)
                {
                    bool? defaultSettings2 = null;
                    if (this is Queue)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Listable("adminqueues+++++++++++++++++++++");
                    }
                    else if (this is Activity)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Listable("adminactivities+++++++++++++++++");
                    }
                    else if (this is Campaign)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Listable("admincampaigns++++++++++++++++++");
                    }
                    else if (this is Agent)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Listable("adminagents+++++++++++++++++++++");
                    }
                    else if (this is Team)
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Listable("adminteams++++++++++++++++++++++");
                    }
                    else
                    {
                        defaultSettings2 = m_Core.DefaultSettings.Is_Listable("adminothers+++++++++++++++++++++");
                    }
                    if (defaultSettings2.HasValue)
                    {
                        return defaultSettings2.Value;
                    }
                }
                else
                    return false;

                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;
            }
            else
            {
                bool? defaultSettings = m_Core.DefaultSettings.Is_Listable(rightid);
                if (defaultSettings == null)
                    return null;
                else if (defaultSettings.Value)
                    return true;
                else
                    return false;
            }
        }
        

        public bool HasFullControlFlag
        {
            get
            {
                if (m_Core.m_CreatorId == "defaultagent++++++++++++++++++++")
                    return true;

                if (this is SecuredAdminObject && ((SecuredAdminObject)this).OwnerUser.TargetId == m_Core.m_CreatorId)
                    return true;

                return Has_FullControlFlag("admin+++++++++++++++++++++++++++").GetValueOrDefault(false);
            }
        }
        public bool HasPowerFlag
        {
            get
            {
                if (m_Core.m_CreatorId == "defaultagent++++++++++++++++++++")
                    return true;

                if (this is SecuredAdminObject && ((SecuredAdminObject)this).OwnerUser.TargetId == m_Core.m_CreatorId)
                    return true;

                return Has_PowerFlag("admin+++++++++++++++++++++++++++").GetValueOrDefault(false);
            }
        }
        public bool IsVisible
        {
            get
            {
                if (m_Core.m_CreatorId == "defaultagent++++++++++++++++++++")
                    return true;

                if (this is SecuredAdminObject && ((SecuredAdminObject)this).OwnerUser.TargetId == m_Core.m_CreatorId)
                    return true;

                return Is_Visible("admin+++++++++++++++++++++++++++").GetValueOrDefault(false);
            }
        }
        public bool IsListable
        {
            get
            {
                if (m_Core.m_CreatorId == "defaultagent++++++++++++++++++++")
                    return true;

                if (this is SecuredAdminObject && ((SecuredAdminObject)this).OwnerUser.TargetId == m_Core.m_CreatorId)
                    return true;

                return Is_Listable("admin+++++++++++++++++++++++++++").GetValueOrDefault(false);
            }
        }
        public virtual bool IsDeletable
        {
            get
            {
                if (m_Core.m_CreatorId == "defaultagent++++++++++++++++++++")
                    return true;

                if (this is SecuredAdminObject && ((SecuredAdminObject)this).OwnerUser.TargetId == m_Core.m_CreatorId)
                    return true;

                return Is_Deletable("admin+++++++++++++++++++++++++++").GetValueOrDefault(false);
            }
        }
        public bool IsReadOnly
        {
            get
            {
                if (m_Core.m_CreatorId == "defaultagent++++++++++++++++++++")
                    return false;

                if (this is SecuredAdminObject && ((SecuredAdminObject)this).OwnerUser.TargetId == m_Core.m_CreatorId)
                    return false;


                return !Is_Modifiable("admin+++++++++++++++++++++++++++").GetValueOrDefault(false);
            }
        }
        public bool IsRightsHandlingAllowed
        {
            get
            {
                if (m_Core.m_CreatorId == "defaultagent++++++++++++++++++++")
                    return true;

                if (this is SecuredAdminObject && ((SecuredAdminObject)this).OwnerUser.TargetId == m_Core.m_CreatorId)
                    return true;


                return Is_RighstHandlingAllowed("admin+++++++++++++++++++++++++++").GetValueOrDefault(false);
            }
        }

        public bool ReadOnly
        {
            get
            {
                return GetFieldValue<bool>("ReadOnly");
            }
            set
            {
                SetFieldValue<bool>("ReadOnly", value);
            }
        }

        // TODO: replace by Rights policy
        public virtual bool CheckEnabled(string strPolicy)
        {
            if (m_Core.m_CreatorId == "defaultagent++++++++++++++++++++")
                return true;

            if (strPolicy == "Campaign.Inbound.Paused" || strPolicy == "Campaign.Outbound.Paused" || strPolicy == "Campaign.Mail.Paused" || strPolicy == "Campaign.Chat.Paused" || strPolicy == "Campaign.Search.Paused")
            {
                return !IsReadOnly || HasPowerFlag || Has_PowerFlag("supervisor++++++++++++++++++++++").GetValueOrDefault(false);
            }
            else if (strPolicy == "Campaign.ConvertToAdvanced" || strPolicy == "Campaign.Outbound.MaxAbandon" || strPolicy == "Campaign.Outbound.AbandonRateInterpretation" || strPolicy == "Qualification.IsExpert" || strPolicy == "DataManage.MaintainDB" || strPolicy == "Campaign.IsExpert")
            {
                return HasPowerFlag;
            }
            else if (strPolicy == "Campaign.DataManage")
            {
                return HasPowerFlag;
            }
            else
            {
                return !ReadOnly;
            }
        }

        public virtual bool IsOwned
        {
            get
            {
                return false;
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return m_CreationTime;
            }
            set
            {
                m_CreationTime = value;
            }
        }

        public DateTime ModifyTime
        {
            get
            {
                return m_ModifyTime;
            }
            set
            {
                m_ModifyTime = value;                
            }
        }

        public string Creator
        {
            get
            {
                if (m_CreatorId != null)
                {
                    Agent agt = (Agent)(m_Core.GetAdminObject(m_CreatorId, typeof(Agent)));
                    if(agt!=null)
                        return agt.ShortDisplayText;
                }
                return string.Empty;
            }
        }

        public string Modificator
        {
            get
            {
                if (m_ModificatorId != null)
                {
                    Agent agt = (Agent)(m_Core.GetAdminObject(m_ModificatorId, typeof(Agent)));
                    if(agt!=null)
                        return agt.ShortDisplayText;
                }
                return string.Empty;
            }
        }

        public AdminCore Core
        {
            get
            {
                return m_Core;
            }
        }

        public IEnumerable<string> GroupKeys
        {
            get
            {
                return m_Core.GroupKeys(null);
            }
        }

        public IEnumerable<string> GroupKeysSameType
        {
            get
            {
                return m_Core.GroupKeys(this.GetType());
            }
        }

        public bool HasBeenLoaded
        {
            get
            {
                return m_Loaded;
            }
        }

        public bool HasBeenDeleted
        {
            get
            {
                return m_Deleted;
            }
            internal set
            {
                m_Deleted = value;
            }
        }

        protected AdminObject(AdminCore core)
        {
            m_CreationTime = DateTime.Now;
            m_ModifyTime = m_CreationTime;
            m_Core = core;
        }

        protected AdminObject(AdminObject parent)
        {
            m_CreationTime = DateTime.Now;
            m_ModifyTime = m_CreationTime;
            this.Parent = parent;
        }

        protected void Init(AdminObject source)
        {
            m_Id = source.m_Id;
        }
        protected T GetFieldValue<T>(string index) 
        {

            if (m_Fields.Contains(index))
            {
                object obj = m_Fields[index].CurrentValue;
                if (obj == null && typeof(T).IsValueType )
                    return default(T);
                else
                    return (T)obj;
            }
            else
                return default(T);


        }

        protected T GetOriginalFieldValue<T>(string index)
        {
            return m_Fields.Contains(index) ? (T)m_Fields[index].OriginalValue : default(T);
        }

        protected bool GetFieldIsModified(string index)
        {
            return m_Fields.Contains(index) ? m_Fields[index].IsModified : false;
        }

        protected void SetFieldLoaded(string index)
        {
            System.Diagnostics.Debug.Assert(m_Fields.Contains(index));
            m_Fields[index].DoneLoading();
        }

        protected bool GetFieldIsReadOnly(string index)
        {
            return m_Fields.Contains(index) ? m_Fields[index].IsReadOnly : false;
        }

        internal protected void SetFieldValue(string index, object value)
        {
            if (!m_Fields.Contains(index))
            {
                m_Fields.Add(new AdminFieldValue(index, value));

                FirePropertyChanged(index);
            }
            else
            {
               
                if (!(value is IComparable) || ((IComparable)value).CompareTo(m_Fields[index].CurrentValue) != 0 )
                {
                    m_Fields[index].CurrentValue = value;

                    FirePropertyChanged(index);
                }
            }
        }

        protected virtual void SetFieldReadOnly(string index)
        {
            if (!m_Fields.Contains(index))
            {
                // should not happen
            }
            else
            {
                m_Fields[index].IsReadOnly = true;
                FirePropertyChanged(index);
            }

        }

        protected virtual void SetFieldValue<T>(string index, T value)
        {
            SetFieldValue(index, (object)value);
        }


        internal virtual AdminObject Parent
        {
            get
            {
                return m_Parent;
            }
            set
            {
                m_Parent = value;

                if (m_Parent != null)
                {
                    m_Core = m_Parent.m_Core;
                }
            }
        }

        public AdminObject[] GetReferences()
        {
            return m_Core.GetObjectReferences(Id);
        }

        public void ReportProgress(string str)
        {
            if (LoadProgressEvent != null)
            {
                LoadProgressEvent(str);
            }
        }
        public void ReportDetailedProgress(int progress, string description, string progressDetail)
        {
            if (DetailedLoadProgressEvent != null)
            {
                DetailedLoadProgressEvent(progress, description, progressDetail);
            }
        }
        static internal DateTime lastLoadProgress = DateTime.MinValue;
        internal virtual void Load(XmlElement node)
        {
            DateTime now = DateTime.Now;
            List<string> LoadedAdminObjectLists = null;

            m_Loaded = true;

            if (this is Language)
                System.Diagnostics.Trace.WriteLine(string.Format("{0}", this.Id), "+++++Load1+++++");


            if (node != null)
            {
                string NodeId = null;

                if (node.Attributes["id"] != null)
                    NodeId = node.Attributes["id"].Value;


                if (NodeId != null)
                {
                    if (m_Id != null && m_Id != NodeId)
                    {
                        throw new NotSupportedException("Cannot overwrite object with data from a different id");
                    }

                    m_Id = NodeId;
                }


                if (node.Attributes["state"] != null)
                    State = XmlConvert.ToInt32(node.Attributes["state"].Value);

                if (node.Attributes["partial"] != null)
                    IsPartial = XmlConvert.ToBoolean(node.Attributes["partial"].Value);

                if (node.Attributes["owner"] != null)
                    OwnerId = node.Attributes["owner"].Value;

                if (node.Attributes["created"] != null)
                    CreationTime = XmlConvert.ToDateTime(node.Attributes["created"].Value, XmlDateTimeSerializationMode.Local);

                if (node.Attributes["modified"] != null)
                    ModifyTime = XmlConvert.ToDateTime(node.Attributes["modified"].Value, XmlDateTimeSerializationMode.Local);

                if (node.Attributes["modificator"] != null)
                    m_ModificatorId = node.Attributes["modificator"].Value;

                if (node.Attributes["creator"] != null)
                    m_CreatorId = node.Attributes["creator"].Value;

                if (node.Attributes["readonly"] != null)
                    ReadOnly = XmlConvert.ToBoolean( node.Attributes["readonly"].Value);


                Type TargetType = this.GetType();

                XmlNode tempNode = node.FirstChild ;

                // Load members from the XML content
                while (tempNode != null)
                {
                    if (tempNode.NodeType == XmlNodeType.Element)
                    {
                        XmlElement Element = (XmlElement)tempNode;
                        try
                        {

                            MethodInfo MInfo = TargetType.GetMethod(string.Concat("Set", Element.LocalName), BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                            PropertyInfo PInfo = null;
                            FieldInfo FInfo = null;
                            Type ValueType = null;
                            ConvertOptionsAttribute ValueAttr = null;

                            if (MInfo != null)
                            {
                                ParameterInfo[] Parameters = MInfo.GetParameters();

                                if (Parameters.Length != 1)
                                    throw new InvalidOperationException(string.Format("{0}.{1} method must accept one parameter", MInfo.DeclaringType.Name, MInfo.Name));

                                ValueType = Parameters[0].ParameterType;
                            }
                            else
                            {
                                PInfo = ReflectionHelper.GetPropertyInfo(TargetType, Element.LocalName);

                                if (PInfo != null)
                                {
                                    if (PInfo.GetSetMethod(true) == null)
                                    {
                                        PInfo = null;
                                    }
                                    else
                                    {
                                        ValueType = PInfo.PropertyType;

                                        object[] Attrs = ReflectionHelper.GetCustomAttributes(PInfo, typeof(ConvertOptionsAttribute));  

                                        if (Attrs.Length > 0)
                                            ValueAttr = (ConvertOptionsAttribute)Attrs[0];

                                        Attrs = ReflectionHelper.GetCustomAttributes(PInfo, typeof(AdminLoadAttribute));  

                                        if (Attrs.Length > 0 && ((AdminLoadAttribute)Attrs[0]).SkipLoad)
                                        {
                                            tempNode = tempNode.NextSibling;
                                            continue;
                                        }
                                    }
                                }
                            }


                            if (MInfo == null && PInfo == null)
                            {
                                FInfo = TargetType.GetField(string.Concat("m_", Element.LocalName), BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);

                                if (FInfo != null)
                                {
                                    ValueType = FInfo.FieldType;
                                }
                            }

                            if (ValueType != null)
                            {
                                object TargetValue = null;

                                if (ValueType.IsSubclassOf(typeof(AdminObjectList)))
                                {
                                    // Lists are always accessed by property
                                    AdminObjectList CurrentList = (AdminObjectList)PInfo.GetValue(this, null);

                                    if (CurrentList == null)
                                    {
                                        CurrentList = (AdminObjectList)ValueType.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, null, new object[] { this, false, false });
                                        PInfo.SetValue(this, CurrentList, null);
                                    }
                                    else
                                    {
                                        CurrentList.ClearToLoad();
                                    }

                                    CurrentList.Load(Element);

                                    if (LoadedAdminObjectLists == null)
                                        LoadedAdminObjectLists = new List<string>();

                                    LoadedAdminObjectLists.Add(Element.LocalName);
                                }
                                else if (PInfo != null && PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectReference)))
                                {
                                    // Admin objects are always accessed by property
                                    AdminObjectReference CurrentObject = (AdminObjectReference)PInfo.GetValue(this, null);

                                    if (CurrentObject == null)
                                    {
                                        CurrentObject = (AdminObjectReference)PInfo.PropertyType.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, null, new object[] { this });
                                        PInfo.SetValue(this, CurrentObject, null);
                                    }

                                    CurrentObject.Load(Element);
                                }
                                else if (ValueType.IsSubclassOf(typeof(AdminObject)))
                                {
                                    // Admin objects are always accessed by property
                                    AdminObject CurrentObject = (AdminObject)PInfo.GetValue(this, null);

                                    if (CurrentObject == null)
                                    {
                                        CurrentObject = (AdminObject)ValueType.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, null, new object[] { this });
                                        PInfo.SetValue(this, CurrentObject, null);
                                    }

                                    CurrentObject.Load(Element);
                                }
                                else
                                {
                                    //TODO: Convert the string value to the target value

                                    // todo: review and check if we need complete docs...
                                    if (ValueType == typeof(XmlDocument))
                                    {
                                        if (Element.HasAttributes && Element.Attributes["xsi:nil"] != null && "true".Equals(Element.Attributes["xsi:nil"].Value))
                                        {
                                            TargetValue = null;
                                        }
                                        else
                                        {
                                            TargetValue = new XmlDocument();
                                            ((XmlDocument)TargetValue).AppendChild(((XmlDocument)TargetValue).ImportNode(Element.ChildNodes[0], true));
                                        }
                                    }
                                    else
                                    {
                                        TargetValue = Convert.ChangeType(Element, ValueType, ValueAttr);
                                    }

                                    if (MInfo != null)
                                    {
                                        MInfo.Invoke(this, new object[] { TargetValue });
                                    }
                                    else if (PInfo != null)
                                    {
                                        PInfo.SetValue(this, TargetValue, null);
                                    }
                                    else if (FInfo != null)
                                    {
                                        FInfo.SetValue(this, TargetValue);
                                    }

                                    SetFieldValue(Element.LocalName, TargetValue);
                                }
                            }
                            else
                            {
                                SetFieldValue(Element.LocalName, Element.InnerText);
                            }

                            if(Element.HasAttributes)
                            {
                                if(Element.Attributes["readonly"]!=null)
                                {
                                    if (XmlConvert.ToBoolean(Element.Attributes["readonly"].Value))
                                        SetFieldReadOnly(Element.LocalName);
                                }
                            }
                        }
                        catch (Exception Ex)
                        {
                            System.Diagnostics.Trace.WriteLine(string.Format("Error loading member {0} from node {1}: {2}", Element.LocalName, node.LocalName, Ex.ToString()), "AdminObject.Load");
                        }
                    }
                    tempNode = tempNode.NextSibling;
                }
            }

            // Load child lists that could get content from other nodes
            LoadChildLists(node, LoadedAdminObjectLists);

            DoneLoading();
        }

        internal protected virtual void LoadChildLists(XmlElement node, List<string> ignoreObjects)
        {
            Type TargetType = this.GetType();

            foreach (PropertyInfo PInfo in TargetType.GetProperties())
            {
                if (ignoreObjects != null && ignoreObjects.Contains(PInfo.Name))
                    continue;

                object[] Attrs = ReflectionHelper.GetCustomAttributes(PInfo, typeof(AdminLoadAttribute)); 

                if (Attrs.Length > 0 && ((AdminLoadAttribute)Attrs[0]).SkipLoad)
                    continue;


                if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectList)))
                {
                    AdminObjectList CurrentList = (AdminObjectList)PInfo.GetValue(this, null);

                    if (CurrentList == null)
                    {
                        CurrentList = (AdminObjectList)PInfo.PropertyType.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, null, new object[] { this, false, false });
                        PInfo.SetValue(this, CurrentList, null);
                    }
                    else
                    {
                        CurrentList.ClearToLoad();
                    }

                    CurrentList.Load(node, PInfo);
                }
                else if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectReference)))
                {
                    // Admin objects are always accessed by property
                    AdminObject CurrentObject = (AdminObject)PInfo.GetValue(this, null);

                    if (CurrentObject == null)
                    {
                        CurrentObject = (AdminObject)PInfo.PropertyType.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, null, new object[] { this });
                        PInfo.SetValue(this, CurrentObject, null);
                    }
                }
                else if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObject)))
                {
                    if (Attrs.Length > 0 && !string.IsNullOrEmpty(((AdminLoadAttribute)Attrs[0]).Path))
                    {
                        AdminObject CurrentObject = (AdminObject)PInfo.GetValue(this, null);

                        if (CurrentObject == null)
                        {
                            CurrentObject = (AdminObject)PInfo.PropertyType.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, null, new object[] { this });
                            PInfo.SetValue(this, CurrentObject, null);
                        }

                        XmlNode LoadNode = (node == null) ? null : node.SelectSingleNode(((AdminLoadAttribute)Attrs[0]).Path);

                        CurrentObject.Load((LoadNode is XmlElement) ? (XmlElement)LoadNode : ((LoadNode is XmlDocument) ? ((XmlDocument)LoadNode).DocumentElement : null));
                    }
                }
            }
        }

        internal virtual bool Reload(XmlNode node)
        {
            Type tpe = this.GetType();
            m_Loaded = true;

            if (this is Language)
                System.Diagnostics.Trace.WriteLine(string.Format("{0}", this.Id), "+++++Load2+++++");


            if (node.Attributes["modified"] != null)
            {
                ModifyTime = XmlConvert.ToDateTime(node.Attributes["modified"].Value, XmlDateTimeSerializationMode.Local);
                FirePropertyChanged("ModifyTime");
            }

            if (node.Attributes["modificator"] != null)
            {
                m_ModificatorId = node.Attributes["modificator"].Value;
                FirePropertyChanged("Modificator");
            }

            
            foreach (XmlNode nde in node.ChildNodes)
            {
                try
                {
                    MethodInfo MInfo = tpe.GetMethod(string.Concat("Set", nde.LocalName), BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                    PropertyInfo PInfo = null;
                    FieldInfo FInfo = null;
                    Type ValueType = null;
                    ConvertOptionsAttribute ValueAttr = null;

                    if (MInfo != null)
                    {
                        ParameterInfo[] Parameters = MInfo.GetParameters();

                        if (Parameters.Length != 1)
                            throw new InvalidOperationException(string.Format("{0}.{1} method must accept one parameter", MInfo.DeclaringType.Name, MInfo.Name));

                        ValueType = Parameters[0].ParameterType;
                    }
                    else
                    {
                        PInfo = ReflectionHelper.GetPropertyInfo(tpe, nde.LocalName); 

                        if (PInfo != null)
                        {
                            if (PInfo.GetSetMethod(true) == null)
                            {
                                PInfo = null;
                            }
                            else
                            {
                                ValueType = PInfo.PropertyType;

                                object[] Attrs = PInfo.GetCustomAttributes(typeof(ConvertOptionsAttribute), true);

                                if (Attrs.Length > 0)
                                    ValueAttr = (ConvertOptionsAttribute)Attrs[0];

                                Attrs = PInfo.GetCustomAttributes(typeof(AdminLoadAttribute), true);

                                if (Attrs.Length > 0 && ((AdminLoadAttribute)Attrs[0]).SkipLoad)
                                    continue;
                            }
                        }
                    }


                    if (MInfo == null && PInfo == null)
                    {
                        FInfo = tpe.GetField(string.Concat("m_", nde.LocalName), BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);

                        if (FInfo != null)
                        {
                            ValueType = FInfo.FieldType;
                        }
                    }

                    if (ValueType != null)
                    {
                        object TargetValue = null;

                        if (ValueType.IsSubclassOf(typeof(AdminObjectList)))
                        {
                        }
                        else if (PInfo != null && PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectReference)))
                        {
                            // Admin objects are always accessed by property
                            AdminObjectReference CurrentObject = (AdminObjectReference)PInfo.GetValue(this, null);

                            if (CurrentObject == null)
                            {
                                CurrentObject = (AdminObjectReference)PInfo.PropertyType.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, null, new object[] { this });
                                PInfo.SetValue(this, CurrentObject, null);
                            }

                            if (!CurrentObject.Reload(nde as XmlElement))
                                return false;

                        }
                        else if (ValueType.IsSubclassOf(typeof(AdminObject)))
                        {
                        }
                        else
                        {
                            if (ValueType == typeof(XmlDocument))
                            {
                                if ((nde as XmlElement).HasAttributes && nde.Attributes["xsi:nil"] != null && "true".Equals(nde.Attributes["xsi:nil"].Value))
                                {
                                    TargetValue = null;
                                }
                                else
                                {
                                    TargetValue = new XmlDocument();
                                    ((XmlDocument)TargetValue).AppendChild(((XmlDocument)TargetValue).ImportNode(nde.ChildNodes[0], true));
                                }
                            }
                            else
                            {
                                TargetValue = Convert.ChangeType(nde, ValueType, ValueAttr);
                            }

                            if (MInfo != null)
                            {
                                MInfo.Invoke(this, new object[] { TargetValue });
                            }
                            else if (PInfo != null)
                            {
                                PInfo.SetValue(this, TargetValue, null);
                            }
                            else if (FInfo != null)
                            {
                                FInfo.SetValue(this, TargetValue);
                            }

                            SetFieldValue(nde.LocalName, TargetValue);
                            SetFieldLoaded(nde.LocalName);
                        }
                    }
                    else
                    {
                        SetFieldValue(nde.LocalName, nde.InnerText);
                        SetFieldLoaded(nde.LocalName);
                    }
                }
                catch (Exception Ex)
                {
                    System.Diagnostics.Trace.WriteLine(string.Format("Error reloading member {0}: {1}", nde.LocalName, Ex.ToString()), "AdminObject.Load");
                }                
            }
            return true;
        }

        protected virtual XmlElement CreateSaveNode(XmlDocument doc, string operation)
        {
            XmlElement ObjectNode = doc.CreateElement(this.GetType().Name);

            XmlAttribute OperationAttr = doc.CreateAttribute("operation");
            OperationAttr.Value = operation;

            ObjectNode.Attributes.Append(OperationAttr);

            if (this.GetType().IsSubclassOf(typeof(AdminObjectLink)))
            {
                Type[] ArgumentType = null;

                for (Type TType = this.GetType(); TType != null; TType = TType.BaseType)
                {
                    if (TType.IsGenericType)
                    {
                        ArgumentType = TType.GetGenericArguments();
                        break;
                    }
                }

                string Id1Name = AdminObjectLink.GetElementAttributeName(ArgumentType[0]);
                string Id2Name = AdminObjectLink.GetElementAttributeName(ArgumentType[1]);

                XmlAttribute IdAttr = doc.CreateAttribute(Id1Name.ToLower());
                IdAttr.Value = ((AdminObjectLink)this).Id1;

                ObjectNode.Attributes.Append(IdAttr);

                IdAttr = doc.CreateAttribute(Id2Name.ToLower());
                IdAttr.Value = ((AdminObjectLink)this).Id2;

                ObjectNode.Attributes.Append(IdAttr);
            }
            else
            {
                if (this.Id != null)
                {
                    XmlAttribute IdAttr = doc.CreateAttribute("id");
                    IdAttr.Value = this.Id;

                    ObjectNode.Attributes.Append(IdAttr);
                }
            }

            return ObjectNode;
        }

        public virtual void Save(XmlDocument doc)
        {

            object[] Attrs = this.GetType().GetCustomAttributes(typeof(AdminSaveAttribute), true);

            if (Attrs.Length > 0 && ((AdminSaveAttribute)Attrs[0]).SkipSave)
                return;

            if (IsDummy)
                return;

            if (IsPartial)
                return;

            XmlElement ObjectNode = null;

            if (this.HasBeenDeleted)
            {
                System.Diagnostics.Trace.WriteLine("Deleted admin object " + this.GetType().FullName + " " + this.Id ?? "");

                ObjectNode = CreateSaveNode(doc, "delete");
            }
            else
            {
                if (!this.HasBeenLoaded)
                {
                    System.Diagnostics.Trace.WriteLine("New admin object " + this.GetType().FullName + " " + this.Id ?? "");

                    ObjectNode = CreateSaveNode(doc, "create");
                    if (ObjectNode != null)
                    {
                        foreach (AdminFieldValue FieldValue in m_Fields)
                        {
                            if (FieldValue.CurrentValue != null)
                            {
                                XmlElement FieldNode = doc.CreateElement(FieldValue.Name);

                                FieldValue.Save(FieldNode, false);
                                ObjectNode.AppendChild(FieldNode);
                            }
                        }
                    }
                }
                else
                {
                    foreach (AdminFieldValue FieldValue in m_Fields)
                    {
                        if (FieldValue.IsModified)
                        {
                            System.Diagnostics.Trace.WriteLine("Modified field in " + this.GetType().FullName + " " + this.Id ?? "" + " : " + FieldValue.Name + "  " + (FieldValue.OriginalValue ?? "(null)").ToString() + " -> " + (FieldValue.CurrentValue ?? "(null)").ToString());


                            if (ObjectNode == null)
                                ObjectNode = CreateSaveNode(doc, "update");

                            XmlElement FieldNode = doc.CreateElement(FieldValue.Name);

                            FieldValue.Save(FieldNode, true);

                            ObjectNode.AppendChild(FieldNode);
                        }
                    }
                    if (ObjectNode == null)
                    {
                        // take a look at all properties that are AdminObjectReferences to verify if they have changed...
                        
                        Type ThisType = this.GetType();

                        foreach (PropertyInfo PInfo in ThisType.GetProperties())
                        {

                            object[] Attributes = PInfo.GetCustomAttributes(typeof(AdminSaveAttribute), true);

                            if (Attributes.Length > 0 && ((AdminSaveAttribute)Attributes[0]).SkipSave)
                                continue;


                            if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectReference)) && PInfo.GetIndexParameters().Length == 0)
                            {
                                AdminObjectReference CurrentObject = (AdminObjectReference)PInfo.GetValue(this, null);

                                if (CurrentObject != null && CurrentObject.IsModified)
                                {
                                    ObjectNode = CreateSaveNode(doc, "update");
                                    break;
                                }
                            }
                        }
                    }
                }
                if(ObjectNode!=null)
                    SaveChildLists(doc, ObjectNode, null);
            }

            if (ObjectNode != null)
            {
                doc.DocumentElement.AppendChild(ObjectNode);
            }
        }

        internal protected virtual void InternalClearReferences()
        {
            m_Core = null;
            m_Parent = null;
            m_Id = null;

            Type ThisType = this.GetType();

            foreach (PropertyInfo PInfo in ThisType.GetProperties())
            {
                try
                {
                    if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectList)) && PInfo.GetIndexParameters().Length == 0)
                    {
                        AdminObjectList CurrentList = (AdminObjectList)PInfo.GetValue(this, null);

                        if (CurrentList != null)
                        {
                            CurrentList.InternalClearReferences();
                        }
                    }
                    else if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectReference)) && PInfo.GetIndexParameters().Length == 0)
                    {
                        AdminObject CurrentObject = (AdminObject)PInfo.GetValue(this, null);

                        if (CurrentObject != null)
                        {
                            CurrentObject.InternalClearReferences();
                        }
                    }
                    else if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObject)) && PInfo.GetIndexParameters().Length == 0)
                    {
                        AdminObject CurrentObject = (AdminObject)PInfo.GetValue(this, null);

                        if (CurrentObject != null)
                        {
                            CurrentObject.InternalClearReferences();
                        }
                    }
                }
                catch
                {
                }
            }
        }

        internal protected virtual void SaveChildLists(XmlDocument doc, XmlElement node, List<string> ignoreObjects)
        {
            Type TargetType = this.GetType();

            foreach (PropertyInfo PInfo in TargetType.GetProperties())
            {
                if (ignoreObjects != null && ignoreObjects.Contains(PInfo.Name))
                    continue;

                object[] Attrs = PInfo.GetCustomAttributes(typeof(AdminSaveAttribute), true);

                if (Attrs.Length > 0 && ((AdminSaveAttribute)Attrs[0]).SkipSave)
                    continue;

                if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectList)))
                {
                    AdminObjectList CurrentList = (AdminObjectList)PInfo.GetValue(this, null);

                    if (CurrentList != null)
                    {
                        CurrentList.Save(this, node, PInfo);
                    }
                }
                else if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectReference)))
                {
                    // Admin objects are always accessed by property
                    AdminObjectReference CurrentObject = (AdminObjectReference)PInfo.GetValue(this, null);

                    if (CurrentObject != null && CurrentObject.IsModified)
                    {
                        if (node == null)
                        {
                            node = CreateSaveNode(doc, "update");
                            doc.DocumentElement.AppendChild(node);
                        }

                        XmlElement FieldNode = doc.CreateElement(PInfo.Name);

                        if (CurrentObject != null && CurrentObject.TargetId != null)
                        {
                            FieldNode.AppendChild(doc.CreateTextNode(CurrentObject.TargetId));
                        }
                        else
                        {
                            XmlAttribute NilAttr = doc.CreateAttribute("xsi", "nil", System.Xml.Schema.XmlSchema.InstanceNamespace);

                            NilAttr.Value = "true";
                            FieldNode.Attributes.Append(NilAttr);
                        }

                        node.AppendChild(FieldNode);
                    }
                }
            }
        }

        internal virtual void Delete()
        {
            HasBeenDeleted = false;
            DoneLoading();
        }

        public virtual void Clear()
        {
            m_Fields.Clear();

            ClearChildLists(null);
        }

        internal virtual void ClearChildLists(List<string> ignoreObjects)
        {
            Type TargetType = this.GetType();

            foreach (PropertyInfo PInfo in TargetType.GetProperties())
            {
                if (ignoreObjects != null && ignoreObjects.Contains(PInfo.Name))
                    continue;

                if (TypeHelper.GetAttributeOrDefault<AdminDeleteAttribute>(PInfo, true).SkipDelete)
                    continue;

                if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectList)))
                {
                    AdminObjectList CurrentList = (AdminObjectList)PInfo.GetValue(this, null);

                    if (CurrentList != null)
                    {
                        CurrentList.Remove(this);
                        CurrentList.Clear();
                    }
                }
                else if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectReference)))
                {
                    // Admin objects are always accessed by property
                    AdminObjectReference CurrentObject = (AdminObjectReference)PInfo.GetValue(this, null);

                    if (CurrentObject != null)
                    {
                        CurrentObject.Clear();
                    }
                }
            }
        }

        internal virtual void RessurectFrom(AdminObject source)
        {
            if(!this.GetType().Equals(source.GetType()))
                throw new InvalidCastException();

            m_Fields.Clear();

            foreach (AdminFieldValue FieldValue in source.m_Fields)
            {
                m_Fields.Add(FieldValue);
            }

            source.m_Fields.Clear();

            m_Loaded = source.m_Loaded;


            if (this is Language)
                System.Diagnostics.Trace.WriteLine(string.Format("{0}", this.Id), "+++++Load3+++++");

        }

        public virtual void DoneLoading()
        {
            m_Loaded = true;

            if (this is Language)
                System.Diagnostics.Trace.WriteLine(string.Format("{0}", this.Id), "+++++Load4+++++");


            foreach (AdminFieldValue FieldValue in m_Fields)
            {
                if (!object.Equals(FieldValue.OriginalValue, FieldValue.CurrentValue))
                {
                    FieldValue.DoneLoading();
                }
            }
        }

        public virtual void SaveApplied()
        {
            DoneLoading();
        }

        public virtual void EmptySave(XmlDocument doc)
        {
            if (HasBeenLoaded)
            {
                object[] Attrs = this.GetType().GetCustomAttributes(typeof(AdminSaveAttribute), true);

                if (Attrs.Length > 0 && ((AdminSaveAttribute)Attrs[0]).SkipSave)
                    return;

                XmlElement ObjectNode = CreateSaveNode(doc, "update");

                foreach (XmlElement elm in doc.DocumentElement.ChildNodes)
                {
                    if (elm.Name.Equals(ObjectNode.Name) && elm.Attributes["id"] != null && ObjectNode.Attributes["id"] != null && ObjectNode.Attributes["id"].Value != null && elm.Attributes["id"].Value != null && ObjectNode.Attributes["id"].Value.Equals(elm.Attributes["id"].Value))
                    {
                        return;
                    }
                }

                doc.DocumentElement.AppendChild(ObjectNode);
            }
        }

        public virtual string TypedDisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.TypedDisplayText;

                return ToString();
            }
        }

        public virtual string DisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.DisplayText;

                if (State>0)
                    return ToString();
                else
                    return string.Concat("[", ToString(), "]");
            }
        }

        public virtual string GroupKey
        {
            get
            {
                return null;
            }
            set
            {
                ;
            }
        }

        public virtual string ShortDisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.ShortDisplayText;

                return ToString();
            }
        }

        public AdminObject Owner
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return m_Core.GetAdminObject(OwnerId);
                else
                    return null;
            }
        }

        public AdminObject DescribingSource
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return m_Core.GetAdminObject(OwnerId);
                else
                    return this;
            }
        }

        public override string ToString()
        {
            return string.Concat(this.GetType().Name, " ", this.Id);
        }

        public virtual string ShortType
        {
            get
            {
                return this.GetType().Name;
            }
        }


        public virtual void Dump(System.Text.StringBuilder sb, HashSet<AdminObject> dumpedObjects, string prefix, int indent)
        {
            if (indent > 0)
                sb.Append(new string(' ', indent));

            if (!string.IsNullOrEmpty(prefix))
                sb.Append(prefix).Append(": ");

            sb.AppendLine(this.ToString());

            if (!dumpedObjects.Contains(this))
            {
                dumpedObjects.Add(this);

                foreach (PropertyInfo PInfo in this.GetType().GetProperties())
                {
                    try
                    {
                        if (PInfo.GetIndexParameters().Length == 0)
                        {
                            if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObject)))
                            {
                                AdminObject Value = (AdminObject)PInfo.GetValue(this, null);

                                Value.Dump(sb, dumpedObjects, PInfo.Name, ((indent < 0) ? -indent : indent) + 1);
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                dumpedObjects.Remove(this);
            }
        }

        public void FirePropertyChanged(string propertyName)
        {

            m_Core.FirePropertyChanged(this, propertyName);

            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public abstract class SecuredAdminObject : AdminObject
    {
        protected AdminObjectList<ObjectSecurity> m_Security = null;
        protected ObservableCollection<ObjectSecurityHelper> m_RolesOverview = null;


        public SecuredAdminObject(AdminCore core)
            : base(core)
        {
            Security = new AdminObjectList<ObjectSecurity>(this);
            RolesOverview = new ObservableCollection<ObjectSecurityHelper>();
            OwnerUser = new AdminObjectReference<Agent>(this);
            
        }

        public SecuredAdminObject(AdminObject parent)
            : base(parent)
        {
            Security = new AdminObjectList<ObjectSecurity>(this);
            RolesOverview = new ObservableCollection<ObjectSecurityHelper>();
            OwnerUser = new AdminObjectReference<Agent>(this);            
        }

        /*
        Not a good idea as some reference can be invalid, creating the null case, replacing the owner!
        protected internal override void LoadChildLists(XmlElement node, List<string> ignoreObjects)
        {
            if(string.IsNullOrEmpty(OwnerUser.TargetId))
                OwnerUser.TargetId = m_Core.m_CreatorId;
            base.LoadChildLists(node, ignoreObjects);
        }
        */

        [AdminLoad(Path = "/Admin/ObjectsSecurity/ObjectSecurity[@securedadminobjectid=\"{0}\"]")]
        public AdminObjectList<ObjectSecurity> Security
        {
            get
            {
                return m_Security;
            }
            internal set
            {
                if (m_Security != null)
                    m_Security.CollectionChanged -= new NotifyCollectionChangedEventHandler(m_Security_CollectionChanged);

                m_Security = value;

                FirePropertyChanged("RolesOverview");

                if (m_Security != null)
                    m_Security.CollectionChanged += new NotifyCollectionChangedEventHandler(m_Security_CollectionChanged);
            }
        }

        protected virtual void m_Security_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (Role role in m_Core.Roles)
            {
                ObjectSecurityHelper osec = null;
                if (!RolesOverview.Select((s) => (s.Role.TargetId)).Contains(role.Id))
                {
                    osec = new ObjectSecurityHelper(this);
                    osec.Role.TargetId = role.Id;
                    osec.SecuredAdminObjectId = this.Id;
                    RolesOverview.Insert(RolesOverview.Count((a) => (a.DisplayText.CompareTo(osec.DisplayText) < 0)), osec);

                }
                else
                {
                    osec = RolesOverview.First((osh) => (osh.Role.TargetId == role.Id));
                }

                foreach (ObjectSecurity os in Security)
                {
                    if (os.RoleId == role.Id)
                    {
                        if (osec.Security.FirstOrDefault((s) => (s.RoleId == os.RoleId && s.RightId == os.RightId)) == null)
                        {
                            os.PropertyChanged += os_PropertyChanged1;

                            osec.Security.Insert(osec.Security.Count((a) => (a.DisplayText.CompareTo(os.DisplayText) < 0)), os);
                        }
                    }
                }
                IEnumerable<Right> rightscollection;
                if (this is Setting)
                {
                    rightscollection = m_Core.GlobalRights;
                }
                else
                {
                    rightscollection = m_Core.ObjectsRights;
                }


                foreach (Right right in rightscollection)
                {
                    if (osec.Security.FirstOrDefault((s) => (s.RoleId == role.Id && s.RightId == right.Id)) == null)
                    {
                        ObjectSecurity os = new ObjectSecurity(this);
                        os.Id = string.Concat(role.Id, right.Id, Id);
                        os.RoleId = role.Id;
                        os.SecuredAdminObjectId = Id;
                        os.RightId = right.Id;
                        os.PropertyChanged += os_PropertyChanged;
                        osec.Security.Add(os);
                    }
                }

            }

            for (int i = 0; i < RolesOverview.Count(); i++)
            {
                if (!Core.Roles.Select((s) => (s.Id)).Contains(RolesOverview[i].Role.TargetId))
                {
                    for (int j = 0; j < RolesOverview[i].Security.Count; j++)
                    {
                        RolesOverview[i].Security.RemoveAt(j);
                        j--;
                    }

                    RolesOverview.RemoveAt(i);
                    i--;
                }
            }
            return;
            bool somethingdone = true;
            while (somethingdone)
            {
                somethingdone = false;
                for (int i = 0; i < RolesOverview.Count; i++)
                {
                    if (i < RolesOverview.Count - 1)
                    {
                        if (RolesOverview[i].DisplayText != null && RolesOverview[i].DisplayText.CompareTo(RolesOverview[i + 1].DisplayText) > 0)
                        {
                            ObjectSecurityHelper backup = RolesOverview[i];
                            RolesOverview[i] = RolesOverview[i + 1];
                            RolesOverview[i + 1] = backup;
                            somethingdone = true;
                        }
                    }

                    for (int j = 0; j < RolesOverview[i].Security.Count-1; j++)
                    {
                        string strRD = RolesOverview[i].Security[j].RightDescription;
                        if (strRD != null && strRD.CompareTo(RolesOverview[i].Security[j + 1].RightDescription) > 0)
                        {
                            ObjectSecurity backup = RolesOverview[i].Security[j];
                            RolesOverview[i].Security[j] = RolesOverview[i].Security[j+1];
                            RolesOverview[i].Security[j+1] = backup;
                            somethingdone = true;
                        }
                    }
                }
            }


        }

        void os_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ObjectSecurity os = sender as ObjectSecurity;
            if (e.PropertyName == "ListAllowed" 
                || e.PropertyName == "ReadAllowed" 
                || e.PropertyName == "WriteAllowed"
                || e.PropertyName == "CreateAllowed"
                || e.PropertyName == "DeleteAllowed"
                || e.PropertyName == "PowerUser"
                || e.PropertyName == "FullControl")
            {
                ObjectSecurityHelper osh = RolesOverview.FirstOrDefault((oshelper) => (oshelper.SecuredAdminObjectId == os.SecuredAdminObjectId && oshelper.Role.TargetId == os.RoleId));

                if (os.ListAllowed.HasValue || os.ReadAllowed.HasValue || os.WriteAllowed.HasValue || os.CreateAllowed.HasValue || os.DeleteAllowed.HasValue || os.PowerUser.HasValue || os.FullControl.HasValue)
                {
                    os.PropertyChanged -= os_PropertyChanged;
                    os.SecuredAdminObjectId = Id;
                    ObjectSecurity objsec = (ObjectSecurity)m_Core.SetAdminObject(os);
                    Security.Add(objsec);
                    objsec.PropertyChanged += os_PropertyChanged1;
                    m_Core.Roles[Security[Security.Count - 1].RoleId].SecuredObjects.Add(objsec);
                }
                else
                {
                }
                if (osh != null)
                {
                    osh.FirePropertyChanged("Rights");
                }
                Role r = m_Core.Roles[os.RoleId];
                if (r != null)
                {
                    r.SecuredObjects.FireCollectionChanged(NotifyCollectionChangedAction.Reset);
                }

            }
        }


        void os_PropertyChanged1(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ObjectSecurity os = sender as ObjectSecurity;
            if (e.PropertyName == "ListAllowed"
                || e.PropertyName == "ReadAllowed"
                || e.PropertyName == "WriteAllowed"
                || e.PropertyName == "CreateAllowed"
                || e.PropertyName == "DeleteAllowed"
                || e.PropertyName == "PowerUser"
                || e.PropertyName == "FullControl")
            {
                ObjectSecurityHelper osh = RolesOverview.FirstOrDefault((oshelper) => (oshelper.SecuredAdminObjectId == os.SecuredAdminObjectId && oshelper.Role.TargetId == os.RoleId));

                if (osh != null)
                {
                    osh.FirePropertyChanged("Rights");
                }

                Role r = m_Core.Roles[os.RoleId];
                if (r != null)
                {
                    r.SecuredObjects.FireCollectionChanged(NotifyCollectionChangedAction.Reset);
                }
            }
        }

        void objsec_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<ObjectSecurityHelper> RolesOverview
        {
            get;
            internal set;
        }

        protected void DuplicateSecurity(SecuredAdminObject duplicated)
        {
            foreach (ObjectSecurityHelper osh in RolesOverview)
            {
                foreach (ObjectSecurityHelper osh2 in duplicated.RolesOverview)
                {
                    if (osh.Role.TargetId == osh2.Role.TargetId)
                    {
                        foreach (ObjectSecurity os in osh.Security)
                        {
                            foreach(ObjectSecurity os2 in osh2.Security)
                            {
                                os2.ReadAllowed = os.ReadAllowed;
                                os2.CreateAllowed = os.CreateAllowed;
                                os2.ListAllowed = os.ListAllowed;
                                os2.WriteAllowed = os.WriteAllowed;
                                os2.PowerUser = os.PowerUser;
                                os2.FullControl = os.FullControl;
                                os2.DeleteAllowed = os.DeleteAllowed;

                            }
                        }
                    }
                }
            }

            foreach (ObjectSecurity os in Security)
            {
                bool bFound = false;
                foreach (ObjectSecurity objs in duplicated.Security)
                {
                    if (objs.RoleId == os.RoleId)
                    {
                        objs.ReadAllowed = os.ReadAllowed;
                        objs.WriteAllowed = os.WriteAllowed;
                        objs.CreateAllowed = os.CreateAllowed;
                        objs.DeleteAllowed = os.DeleteAllowed;
                        objs.PowerUser = os.PowerUser;
                        objs.FullControl = os.FullControl;
                        objs.ListAllowed = os.ListAllowed;
                        bFound = true;
                        break;
                    }
                }
                if (!bFound)
                {
                    // add it!
                    ObjectSecurity newos = os.Duplicate(duplicated.Id, os.RoleId, duplicated.Security);
                }
            }

        }

        public AdminObjectReference<Agent> OwnerUser
        { 
            get; 
            internal set; 
        }

        internal override bool? Has_FullControlFlag(string rightid)
        {
            bool? tempResult = base.Has_FullControlFlag(rightid);
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            if (rightid == "admin+++++++++++++++++++++++++++")
                rightid = "adminobject+++++++++++++++++++++";

            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.FullControl.HasValue && os.FullControl.Value)
                    {

                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }

                    if (os.FullControl.HasValue && !os.FullControl.Value)
                    {
                        // explictly denied
                        return false;
                    }


                }
            }

            return tempResult;
        }
        internal override bool? Has_PowerFlag(string rightid)
        {
            bool? tempResult = base.Has_PowerFlag(rightid);
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            if (rightid == "admin+++++++++++++++++++++++++++")
                rightid = "adminobject+++++++++++++++++++++";


            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.PowerUser.HasValue && os.PowerUser.Value)
                    {

                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }

                    if (os.PowerUser.HasValue && !os.PowerUser.Value)
                    {
                        // explictly denied
                        return false;
                    }


                }
            }

            return tempResult;
        }
        public override bool? Is_Visible(string rightid)
        {
            bool? tempResult = base.Is_Visible(rightid);
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            if (rightid == "admin+++++++++++++++++++++++++++")
                rightid = "adminobject+++++++++++++++++++++";


            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.ReadAllowed.HasValue && os.ReadAllowed.Value)
                    {

                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }

                    if (os.ReadAllowed.HasValue && !os.ReadAllowed.Value)
                    {
                        // explictly denied
                        return false;
                    }


                }
            }

            return tempResult;
        }
        public override bool? Is_Listable(string rightid)
        {
            bool? tempResult = base.Is_Listable(rightid);
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            if (rightid == "admin+++++++++++++++++++++++++++")
                rightid = "adminobject+++++++++++++++++++++";


            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.ListAllowed.HasValue && os.ListAllowed.Value)
                    {

                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }

                    if (os.ListAllowed.HasValue && !os.ListAllowed.Value)
                    {
                        // explictly denied
                        return false;
                    }


                }
            }

            return tempResult;
        }
        internal override bool? Is_Modifiable(string rightid)
        {
            bool? tempResult = base.Is_Modifiable(rightid);
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            if (rightid == "admin+++++++++++++++++++++++++++")
                rightid = "adminobject+++++++++++++++++++++";

            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.WriteAllowed.HasValue && os.WriteAllowed.Value)
                    {
                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }

                    if (os.WriteAllowed.HasValue && !os.WriteAllowed.Value)
                    {
                        // explictly denied
                        return false;
                    }


                }
            }

            return tempResult;
        }
        internal override bool? Is_Deletable(string rightid)
        {
            bool? tempResult = base.Is_Deletable(rightid);
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            if (rightid == "admin+++++++++++++++++++++++++++")
                rightid = "adminobject+++++++++++++++++++++";


            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.DeleteAllowed.HasValue && os.DeleteAllowed.Value)
                    {
                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }

                    if (os.DeleteAllowed.HasValue && !os.DeleteAllowed.Value)
                    {
                        // explictly denied
                        return false;
                    }
                }
            }

            return tempResult;
        }
        internal override bool? Is_RighstHandlingAllowed(string rightid)
        {
            bool? tempResult = base.Is_RighstHandlingAllowed(rightid);
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            if (rightid == "admin+++++++++++++++++++++++++++")
                rightid = "adminobject+++++++++++++++++++++";


            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.FullControl.HasValue && os.FullControl.Value)
                    {
                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }

                    if (os.FullControl.HasValue && !os.FullControl.Value)
                    {
                        // explictly denied
                        return false;
                    }
                }
            }

            return tempResult;
        }


        public void TakeOwnerShip()
        {
            OwnerUser.TargetId = m_Core.m_CreatorId;
        }

        public override bool IsOwned
        {
            get
            {
                return OwnerUser.TargetId == m_Core.m_CreatorId;
            }
        }
    }

    public class PlaceholderAdminObject : AdminObject
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("Global");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        public PlaceholderAdminObject(string id)
            : base((AdminCore)null)
        {
            m_Id = id;
        }

        public override string DisplayText
        {
            get
            {
                return Translate("<restricted>");
            }
        }

        public override string ShortDisplayText
        {
            get
            {
                return Translate("<restricted>");
            }
        }
    }

    public class NullAdminObject : AdminObject
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("Global");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        public NullAdminObject()
            : base((AdminCore)null)
        {
        }

        public override string DisplayText
        {
            get
            {
                return Translate("<none>");
            }
        }

        public override string  ShortDisplayText
        {
	        get 
	        {
                return Translate("<none>");
	        }
        }

        public Uri PathUri
        {
            get
            {
                return null;
            }
        }
    }


    public abstract class AdminObjectReference : AdminObject
    {
        internal override bool Reload(XmlNode node)
        {
            string RefId = null;

            foreach (XmlNode Child in node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Text)
                {
                    RefId = Child.Value;
                    break;
                }
            }

            this.TargetId = RefId;
            m_OriginalTargetId = this.TargetId;

            m_Loaded = true;

            if (this is Language)
                System.Diagnostics.Trace.WriteLine(string.Format("{0}", this.Id), "+++++Load5+++++");


            return  (TargetId == RefId || AdminObject.IsDummyId(TargetId));
        }


        protected string m_TargetId;
        protected Type m_TargetType;
        protected string m_OriginalTargetId;
        protected string m_LastTargetId;
        
        public event System.ComponentModel.PropertyChangedEventHandler TargetPropertyChanged;

        internal AdminObjectReference(AdminObject parent)
            : base(parent)
        {
        }


        protected void OnTargetPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs args)
        {
            if(TargetPropertyChanged != null)
                TargetPropertyChanged(this, args);
        }

        public override string Id
        {
            get
            {
                return base.Id;
            }
            internal set
            {
                if (m_Parent != null && m_Id != null)
                    m_Core.RemoveObjectReference(m_Parent.Id ?? m_Parent.Parent.Id, m_Id);

                base.Id = value;

                if (m_Parent != null && m_Id != null)
                    m_Core.AddObjectReference(m_Parent.Id ?? m_Parent.Parent.Id, m_Id);
            }
        }

        internal override AdminObject Parent
        {
            get
            {
                return base.Parent;
            }
            set
            {
                if (base.Parent != null && Id != null)
                    m_Core.RemoveObjectReference(base.Parent.Id ?? base.Parent.Parent.Id, Id);

                base.Parent = value;

                if (base.Parent != null && Id != null)
                    m_Core.AddObjectReference(base.Parent.Id ?? base.Parent.Parent.Id, Id);
            }
        }

        public bool HasTarget
        {
            get
            {
                return !string.IsNullOrEmpty(m_TargetId);
            }
        }

        public string TargetId
        {
            get
            {
                return m_TargetId;
            }
            set
            {
                if (m_TargetId != null && value != null && m_TargetId != value)
                {
                    TargetId = null;
                }

                System.Diagnostics.Debug.Assert(value == null || !string.IsNullOrEmpty(value));
                if (value != null && string.IsNullOrEmpty(value))
                    ContactRoute.DiagnosticHelpers.DebugIfPossible();


                if (m_TargetId != value)
                {
                    string ParentId = m_Parent.Id ?? m_Parent.Parent.Id;

                    m_LastTargetId = m_TargetId;

                    if (!string.IsNullOrEmpty(m_TargetId))
                    {
                        AdminObject TargetObject = m_Core.GetAdminObject(m_TargetId, m_TargetType);
                        
                        if(TargetObject != null && TargetObject is INotifyPropertyChanged)
                            ((INotifyPropertyChanged)TargetObject).PropertyChanged -= OnTargetPropertyChanged;

                        if (m_Parent != null && ParentId != null && ParentId != m_TargetId)
                            m_Core.RemoveObjectReference(ParentId, m_TargetId);
                    }

                    if (value != null && !m_Core.HasAdminObject(value, m_TargetType))
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Invalid object reference {0}", value));
                        
                        if (true /* testing stuffs */)
                        {
                            if (value != null)
                            {
                                System.Diagnostics.Trace.WriteLine(string.Format("Trying to use dummy object {0}", value));

                                if (m_TargetType == typeof(Queue))
                                {
                                    m_TargetId = "dummyqueue++++++++++++++++++++++";
                                }
                                else if (m_TargetType == typeof(Location))
                                {
                                    m_TargetId = "dummylocation+++++++++++++++++++";
                                }
                                else if (m_TargetType == typeof(CallbackRuleset))
                                {
                                    m_TargetId = "dummycbrules++++++++++++++++++++";
                                }
                                else if (m_TargetType == typeof(SecurityContext))
                                {                                    
                                    m_TargetId = "dummysecuritycontext++++++++++++";
                                }
                                else if (m_TargetType == typeof(Agent))
                                {
                                    m_TargetId = "dummyagent++++++++++++++++++++++";
                                }
                                else if (m_TargetType == typeof(Carrier))
                                {
                                    m_TargetId = "dummycarrier++++++++++++++++++++";
                                }
                                else if (m_TargetType == typeof(Preprocessor))
                                {
                                    m_TargetId = "dummypreprocessor+++++++++++++++";
                                }
                                else if (m_TargetType == typeof(NumberingPlanEntry))
                                {
                                    m_TargetId = "dummynumberingplanentry+++++++++";
                                }
                                else if (m_TargetType == typeof(AmdSettings))
                                {
                                    m_TargetId = "dummyamd++++++++++++++++++++++++";
                                }
                                else if (m_TargetType == typeof(OutboundActivity))
                                {
                                    m_TargetId = "dummyout++++++++++++++++++++++++";
                                }
                                else if (m_TargetType == typeof(Planning))
                                {
                                    m_TargetId = "dummyplanning+++++++++++++++++++";
                                }
                                else if (m_TargetType == typeof(PlanningTimeSpan))
                                {
                                    m_TargetId = "dummyplanningts+++++++++++++++++";
                                }
                                else if (m_TargetType == typeof(Prompt))
                                {
                                    m_TargetId = "dummyprompt+++++++++++++++++++++";
                                }
                                else
                                {
                                    ContactRoute.DiagnosticHelpers.DebugIfPossible();

                                }



                                if (!string.IsNullOrEmpty(m_TargetId))
                                {
                                    if (m_Parent != null && ParentId != null && ParentId != m_TargetId)
                                        m_Core.AddObjectReference(ParentId, m_TargetId);

                                    AdminObject TargetObject = m_Core.GetAdminObject(m_TargetId, m_TargetType);

                                    if (TargetObject != null && TargetObject is INotifyPropertyChanged)
                                        ((INotifyPropertyChanged)TargetObject).PropertyChanged += OnTargetPropertyChanged;
                                }

                            }
                        }
                    }
                    else
                    {
                        m_TargetId = value;

                        if (!string.IsNullOrEmpty(m_TargetId))
                        {
                            if (m_Parent != null && ParentId != null && ParentId != m_TargetId)
                                m_Core.AddObjectReference(ParentId, m_TargetId);

                            AdminObject TargetObject = m_Core.GetAdminObject(m_TargetId, m_TargetType);

                            if (TargetObject != null && TargetObject is INotifyPropertyChanged)
                                ((INotifyPropertyChanged)TargetObject).PropertyChanged += OnTargetPropertyChanged;
                        }
                    }

                    FirePropertyChanged("TargetId");
                    FirePropertyChanged("Target");
                    FirePropertyChanged("HasTarget");
                }
            }
        }

        internal override void Load(XmlElement node)
        {
            string RefId = null;

            foreach (XmlNode Child in node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Text)
                {
                    RefId = Child.Value;
                    break;
                }
            }

            this.TargetId = RefId;
            m_OriginalTargetId = this.TargetId;

            m_Loaded = true;

        }

        public override void DoneLoading()
        {
            m_OriginalTargetId = m_TargetId;
            m_Loaded = true;


            base.DoneLoading();
        }
        public override void Clear()
        {
            this.TargetId = null;
        }

        protected internal override void InternalClearReferences()
        {
            m_TargetId = null;

            base.InternalClearReferences();
        }
        public string OriginalTargetId
        {
            get
            {
                return m_OriginalTargetId;
            }
        }

        public string LastTargetId
        {
            get
            {
                return m_LastTargetId;
            }
        }

        public bool IsModified
        {
            get
            {
                return !string.Equals(m_TargetId, m_OriginalTargetId);
            }
        }
    }

    public class AdminObjectReference<T> : AdminObjectReference where T : AdminObject
    {

        public static implicit operator T(AdminObjectReference<T> reference)
        {
            System.Diagnostics.Debug.Assert(typeof(T) != typeof(AdminObject), "Casts cannot work with references to AdminObject");
            return reference.Target;
        }


        internal AdminObjectReference(AdminObject parent)
            : base(parent)
        {
            m_TargetType = typeof(T);
        }

        public T Target
        {
            get
            {
                if (m_TargetId == null)
                    return null;

                return m_Core.GetAdminObject(m_TargetId) as T;
            }
            set
            {
                TargetId = (value == null) ? null : value.Id;
            }
        }
    }

    public enum AdminObjectLinkCascadeAction
    {
        Cascade,
        Abort
    }

    public class AdminObjectLinkCascadeException : Exception
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AdminObjectLinkCascadeAttribute : Attribute
    {
        public Type OwnerType;
        public string OwnerPropertyName;

        public AdminObjectLinkCascadeAttribute(Type ownerType, string propertyName)
        {
            OwnerType = ownerType;
            OwnerPropertyName = propertyName;
        }
    }

    public abstract class AdminObjectLink : AdminObject
    {
        private string m_Id1;
        private string m_Id2;
        protected Type m_Type1;
        protected Type m_Type2;

        public static string GetElementAttributeName(Type elementType)
        {
            return string.Concat(elementType.Name.ToLower(), "id");
        }

        public static string GetElementAttributeValue(Type elementType, XmlNode node)
        {
            string AttrName = GetElementAttributeName(elementType);
            XmlAttribute Attr = node.Attributes[AttrName];

            if (Attr == null)
                return null;

            return Attr.Value;
        }

        public static string GetCombinedId(string id1, string id2)
        {
            return string.Concat(id1, "/", id2);
        }


        public AdminObjectLink(AdminObject parent)
            : base(parent)
        {
        }

        protected internal void SetIds(string id1, string id2)
        {
            if (id1 == "" || id2 == "")
                ContactRoute.DiagnosticHelpers.DebugIfPossible();


            if (!string.IsNullOrEmpty(m_Id1) && !string.IsNullOrEmpty(m_Id2))
            {
                m_Core.RemoveObjectReference(m_Id1, m_Id2);
                m_Core.RemoveObjectReference(m_Id2, m_Id1);
            }

            m_Id1 = id1;
            m_Id2 = id2;


            if (!string.IsNullOrEmpty(m_Id1) && !string.IsNullOrEmpty(m_Id2))
            {
                m_Core.AddObjectReference(m_Id1, m_Id2);
                m_Core.AddObjectReference(m_Id2, m_Id1);
            }
        }

        protected internal string Id1
        {
            get
            {
                return m_Id1;
            }
            set
            {
                if (value == "")
                    ContactRoute.DiagnosticHelpers.DebugIfPossible();


                if (m_Id1 != null && m_Id2 != null)
                {
                    m_Core.RemoveObjectReference(m_Id1, m_Id2);
                    m_Core.RemoveObjectReference(m_Id2, m_Id1);
                }

                m_Id1 = value;

                if (m_Id1 != null && m_Id2 != null)
                {
                    m_Core.AddObjectReference(m_Id1, m_Id2);
                    m_Core.AddObjectReference(m_Id2, m_Id1);
                }
            }
        }

        protected internal string Id2
        {
            get
            {
                return m_Id2;
            }
            set
            {
                if (value == "")
                    ContactRoute.DiagnosticHelpers.DebugIfPossible();


                if (m_Id1 != null && m_Id2 != null)
                {
                    m_Core.RemoveObjectReference(m_Id1, m_Id2);
                    m_Core.RemoveObjectReference(m_Id2, m_Id1);
                }

                m_Id2 = value;

                if (m_Id1 != null && m_Id2 != null)
                {
                    m_Core.AddObjectReference(m_Id1, m_Id2);
                    m_Core.AddObjectReference(m_Id2, m_Id1);
                }
            }
        }

        internal void Load(XmlElement node, string id1, string id2)
        {
            if(id1 != null && id2 != null)
                SetIds(id1, id2);

            this.Load(node);
        }

        internal Type Type1
        {
            get
            {
                return m_Type1;
            }
        }

        internal Type Type2
        {
            get
            {
                return m_Type2;
            }
        }

        public bool HasReference(string id)
        {
            return (id.Equals(Id1) || id.Equals(Id2));
        }

        public bool HasReference(AdminObject item)
        {
            return HasReference(item.Id);
        }

        internal abstract void AddLinkSide(Type sideType);

        internal virtual void AddLinkSide(AdminObject sideItem)
        {
            AddLinkSide(sideItem.GetType());
        }

        internal abstract void RemoveLinkSide(Type sideType);

        internal virtual void RemoveLinkSide(AdminObject sideItem)
        {
            RemoveLinkSide(sideItem.GetType());
        }
    }

    public class AdminObjectLink<T1, T2> : AdminObjectLink
        where T1 : AdminObject
        where T2 : AdminObject
    {
        public AdminObjectLink(AdminObject parent)
            : base(parent)
        {
            m_Type1 = typeof(T1);
            m_Type2 = typeof(T2);
        }

        public AdminObjectLink(AdminObject parent, string id1, string id2)
            : base(parent)
        {
            m_Type1 = typeof(T1);
            m_Type2 = typeof(T2);

            SetIds(id1, id2);
        }

        internal T1 Object1
        {
            get
            {
                return (T1)m_Core.GetAdminObject(Id1);
            }
        }

        internal T2 Object2
        {
            get
            {
                return (T2)m_Core.GetAdminObject(Id2);
            }
        }

        internal override void Load(XmlElement node)
        {
            if (Id1 == null || Id2 == null)
            {
                SetIds(GetElementAttributeValue(typeof(T1), node), GetElementAttributeValue(typeof(T2), node));
            }

            // Carefull...   To avoid preload AdminObjectList assumes that a link's id is always the 'combined' id of id1 and id2
            base.Id = GetCombinedId(Id1, Id2);
            base.Load(node);
        }

        internal override void AddLinkSide(Type sideType)
        {
            object AddTarget = null;

            if (sideType == typeof(T1))
            {
                AddTarget = Object1;
            }
            else if (sideType == typeof(T2))
            {
                AddTarget = Object2;
            }
            else
            {
                if (sideType.IsSubclassOf(typeof(T1)))
                {
                    AddTarget = Object1;
                }
                else if (sideType.IsSubclassOf(typeof(T2)))
                {
                    AddTarget = Object2;
                }
                else
                {
                    throw new AdminObjectLinkCascadeException();
                }
            }

            object[] LinkAttributes = this.GetType().GetCustomAttributes(typeof(AdminObjectLinkCascadeAttribute), true);
            List<AdminObjectLinkCascadeAttribute> FoundAttr = new List<AdminObjectLinkCascadeAttribute>();

            foreach (AdminObjectLinkCascadeAttribute LinkAttribute in LinkAttributes)
            {
                if (LinkAttribute.OwnerType == sideType)
                {
                    FoundAttr.Add(LinkAttribute);
                    break;
                }
            }

            if (FoundAttr.Count == 0)
            {
                foreach (AdminObjectLinkCascadeAttribute LinkAttribute in LinkAttributes)
                {
                    if (sideType.IsSubclassOf(LinkAttribute.OwnerType))
                    {
                        FoundAttr.Add(LinkAttribute);
                        break;
                    }
                }
            }

            foreach (AdminObjectLinkCascadeAttribute LinkAttribute in FoundAttr)
            {
                PropertyInfo PInfo = sideType.GetProperty(LinkAttribute.OwnerPropertyName);

                if (PInfo != null)
                {
                    if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectList)))
                    {
                        AdminObjectList List = (AdminObjectList)PInfo.GetValue(AddTarget, null);

                        if (List == null)
                        {
                            List = (AdminObjectList)PInfo.PropertyType.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, null, new object[] { AddTarget, false, false });
                            PInfo.SetValue(AddTarget, List, null);
                        }

                        if (!List.ContainsId(this.Id))
                        {
                            if (List.IsSingleton)
                            {
                            }

                            List.AddId(Id);
                        }
                    }
                    else if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectReference)))
                    {
                        ((AdminObjectReference)PInfo.GetValue(AddTarget, null)).TargetId = Id;
                    }
                    else
                    {
                        PInfo.SetValue(AddTarget, Id, null);
                    }
                }
            }
        }

        internal override void RemoveLinkSide(Type sideType)
        {
            object RemoveTarget = null;

            if (sideType == typeof(T1))
            {
                RemoveTarget = Object1;
            }
            else if (sideType == typeof(T2))
            {
                RemoveTarget = Object2;
            }
            else if (sideType.IsSubclassOf(typeof(T1)))
            {
                RemoveTarget = Object1;
            }
            else if (sideType.IsSubclassOf(typeof(T2)))
            {
                RemoveTarget = Object2;
            }
            else
            {
                throw new AdminObjectLinkCascadeException();
            }

            object[] LinkAttributes = this.GetType().GetCustomAttributes(typeof(AdminObjectLinkCascadeAttribute), true);

            foreach (AdminObjectLinkCascadeAttribute LinkAttribute in LinkAttributes)
            {
                if (sideType.Equals(LinkAttribute.OwnerType) || sideType.IsSubclassOf(LinkAttribute.OwnerType))
                {
                    PropertyInfo PInfo = sideType.GetProperty(LinkAttribute.OwnerPropertyName);

                    if (PInfo != null)
                    {
                        if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectList)))
                        {
                            AdminObjectList PValue = (AdminObjectList)PInfo.GetValue(RemoveTarget, null);

                            if(PValue != null)
                                PValue.RemoveId(this.Id);
                        }
                        else if (PInfo.PropertyType.IsSubclassOf(typeof(AdminObjectReference)))
                        {
                            ((AdminObjectReference)PInfo.GetValue(RemoveTarget, null)).TargetId = null;
                        }
                        else
                        {
                            PInfo.SetValue(RemoveTarget, null, null);
                        }
                    }
                }
            }
        }

        protected override XmlElement CreateSaveNode(XmlDocument doc, string operation)
        {

            try
            {
                Object1.EmptySave(doc);
            }
            catch
            {
            }
            
            try
            {
                Object2.EmptySave(doc);
            }
            catch
            {
            }

            return base.CreateSaveNode(doc, operation);
        }
    }
}