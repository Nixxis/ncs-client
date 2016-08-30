using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace Nixxis.Client.Admin
{

    public class ObjectSecurity : AdminObject
    {
        private bool willExistAfterCommit = true;
        private bool exists = true;

        internal override bool Reload(System.Xml.XmlNode node)
        {
            return base.Reload(node);
        }

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            willExistAfterCommit = exists;
            XmlElement node;
            XmlAttribute att;

            if (!HasBeenLoaded)
            {
                // it is a dummy object
                if (ListAllowed ==null && ReadAllowed == null && WriteAllowed == null && CreateAllowed == null && DeleteAllowed == null && PowerUser == null && FullControl == null && operation == "create")
                    return null;

                if (operation == "update")
                {
                    if (ListAllowed == null && ReadAllowed == null && WriteAllowed == null && CreateAllowed == null && DeleteAllowed == null && PowerUser == null && FullControl == null)
                    {
                        willExistAfterCommit = false;

                        node = base.CreateSaveNode(doc, "delete");
                        att = doc.CreateAttribute("securedadminobjectid");
                        att.Value = SecuredAdminObjectId;
                        node.Attributes.Append(att);

                        att = doc.CreateAttribute("roleid");
                        att.Value = RoleId;
                        node.Attributes.Append(att);

                        att = doc.CreateAttribute("rightid");
                        att.Value = RightId;
                        node.Attributes.Append(att);
                        return node;

                    }
                    else
                    {
                        willExistAfterCommit = true;

                        node = base.CreateSaveNode(doc, "create");
                        att = doc.CreateAttribute("securedadminobjectid");
                        att.Value = SecuredAdminObjectId;
                        node.Attributes.Append(att);

                        att = doc.CreateAttribute("roleid");
                        att.Value = RoleId;
                        node.Attributes.Append(att);

                        att = doc.CreateAttribute("rightid");
                        att.Value = RightId;
                        node.Attributes.Append(att);
                        return node;
                    }
                }
                if (operation == "delete")
                {
                    return null;
                }
            }
            else
            {
                if (ListAllowed == null && ReadAllowed == null && WriteAllowed == null && CreateAllowed == null && DeleteAllowed == null && PowerUser == null && FullControl == null && operation == "update")
                {
                    willExistAfterCommit = false;

                    node = base.CreateSaveNode(doc, "delete");
                    att = doc.CreateAttribute("securedadminobjectid");
                    att.Value = SecuredAdminObjectId;
                    node.Attributes.Append(att);

                    att = doc.CreateAttribute("roleid");
                    att.Value = RoleId;
                    node.Attributes.Append(att);

                    att = doc.CreateAttribute("rightid");
                    att.Value = RightId;
                    node.Attributes.Append(att);
                    return node;
                }
            }

            if(!exists && operation=="update")
            {
                operation = "create";
                willExistAfterCommit = true;
            }
            
            node = base.CreateSaveNode(doc, operation);
            att = doc.CreateAttribute("securedadminobjectid");
            att.Value = SecuredAdminObjectId;
            node.Attributes.Append(att);

            att = doc.CreateAttribute("roleid");
            att.Value = RoleId;
            node.Attributes.Append(att);

            att = doc.CreateAttribute("rightid");
            att.Value = RightId;
            node.Attributes.Append(att);
            return node;
        }

        public override void SaveApplied()
        {
            exists = willExistAfterCommit;

            base.SaveApplied();
        }

        public ObjectSecurity(AdminCore core)
            : base(core)
        {
            Init();
        }

        public ObjectSecurity(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private void Init()
        {
            SetFieldValue<bool?>("ListAllowed", null);
            SetFieldValue<bool?>("ReadAllowed", null);
            SetFieldValue<bool?>("WriteAllowed", null);
            SetFieldValue<bool?>("CreateAllowed", null);
            SetFieldValue<bool?>("DeleteAllowed", null);
            SetFieldValue<bool?>("PowerUser", null);
            SetFieldValue<bool?>("FullControl", null);
        }

        public string SecuredAdminObjectId
        {
            get
            {
                return GetFieldValue<string>("SecuredAdminObjectId");
            }
            set
            {
                SetFieldValue<string>("SecuredAdminObjectId", value);
            }
        }

        public string RoleId
        {
            get
            {
                return GetFieldValue<string>("RoleId");
            }
            set
            {
                SetFieldValue<string>("RoleId", value);
            }
        }

        public string RightId
        {
            get
            {
                return GetFieldValue<string>("RightId");
            }
            set
            {
                SetFieldValue<string>("RightId", value);
            }
        }

        public string RightDescription
        {
            get
            {
                string strRightId = RightId;
                if (string.IsNullOrEmpty(strRightId))
                    return string.Empty;
                return m_Core.Rights[strRightId].Description;
            }
        }



        public bool? ListAllowed
        {
            get
            {
                return GetFieldValue<bool?>("ListAllowed");
            }
            set
            {
                if (IsListAllowedVisible)
                {
                    SetFieldValue<bool?>("ListAllowed", value);

                    if (value.HasValue)
                    {
                        if (value.Value)
                        {
                        }
                        else
                        {
                            ReadAllowed = false;
                        }
                    }
                    else
                    {
                        ReadAllowed = null;
                    }
                }
            }
        }

        public bool? ReadAllowed
        {
            get
            {
                return GetFieldValue<bool?>("ReadAllowed");
            }
            set
            {
                if (IsReadAllowedVisible)
                {
                    SetFieldValue<bool?>("ReadAllowed", value);

                    if (value.HasValue)
                    {
                        if (value.Value)
                        {
                            ListAllowed = true;
                        }
                        else
                        {
                            WriteAllowed = false;
                            DeleteAllowed = false;
                            PowerUser = false;
                            FullControl = false;
                        }
                    }
                    else
                    {
                        WriteAllowed = null;
                        DeleteAllowed = null;
                        PowerUser = null;
                        FullControl = null;
                    }
                }
            }
        }

        public bool? WriteAllowed
        {
            get
            {
                return GetFieldValue<bool?>("WriteAllowed");
            }
            set
            {
                if (IsWriteAllowedVisible)
                {
                    SetFieldValue<bool?>("WriteAllowed", value);

                    if (value.HasValue)
                    {
                        if (value.Value)
                        {

                            ReadAllowed = true;
                        }
                        else
                        {
                            FullControl = false;
                        }
                    }
                    else
                    {
                        FullControl = null;
                    }
                }
            }
        }
        
        public bool? CreateAllowed
        {
            get
            {
                return GetFieldValue<bool?>("CreateAllowed");
            }
            set
            {
                if (IsCreateAllowedVisible)
                {
                    SetFieldValue<bool?>("CreateAllowed", value);

                    if (value.HasValue)
                    {
                        if (value.Value)
                        {

                        }
                        else
                        {
                            FullControl = false;
                        }
                    }
                    else
                    {
                        FullControl = null;
                    }
                }

            }
        }
        
        public bool? DeleteAllowed
        {
            get
            {
                return GetFieldValue<bool?>("DeleteAllowed");
            }
            set
            {
                if (IsDeleteAllowedVisible)
                {
                    SetFieldValue<bool?>("DeleteAllowed", value);

                    if (value.HasValue)
                    {
                        if (value.Value)
                        {
                            ReadAllowed = true;
                        }
                        else
                        {
                            FullControl = false;
                        }
                    }
                    else
                    {
                        FullControl = null;
                    }
                }
            }
        }
        
        public bool? PowerUser
        {
            get
            {
                return GetFieldValue<bool?>("PowerUser");
            }
            set
            {
                if (IsPowerUserVisible)
                {
                    SetFieldValue<bool?>("PowerUser", value);

                    if (value.HasValue)
                    {
                        if (value.Value)
                        {
                            ReadAllowed = true;
                        }
                        else
                        {
                            FullControl = false;
                        }
                    }
                    else
                    {
                        FullControl = null;
                    }
                }
            }
        }
        
        public bool? FullControl
        {
            get
            {
                return GetFieldValue<bool?>("FullControl");
            }
            set
            {
                if (IsFullControlVisible)
                {
                    SetFieldValue<bool?>("FullControl", value);

                    if (value.HasValue)
                    {
                        if (value.Value)
                        {
                            ReadAllowed = true;
                            WriteAllowed = true;
                            CreateAllowed = true;
                            DeleteAllowed = true;
                            PowerUser = true;
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }

            }
        }



        public bool IsListAllowedVisible
        {
            get
            {
                if (string.IsNullOrEmpty(RightId))
                    return false;
                return !string.IsNullOrEmpty(m_Core.Rights[RightId].ListAllowedMeaning);
            }
        }

        public bool IsReadAllowedVisible
        {
            get
            {
                if (string.IsNullOrEmpty(RightId))
                    return false;
                return !string.IsNullOrEmpty(m_Core.Rights[RightId].ReadAllowedMeaning);
            }
        }

        public bool IsWriteAllowedVisible
        {
            get
            {
                if (string.IsNullOrEmpty(RightId))
                    return false;

                return !string.IsNullOrEmpty(m_Core.Rights[RightId].WriteAllowedMeaning);
            }
        }

        public bool IsCreateAllowedVisible
        {
            get
            {
                if (string.IsNullOrEmpty(RightId))
                    return false;

                return !string.IsNullOrEmpty(m_Core.Rights[RightId].CreateAllowedMeaning);
            }
        }

        public bool IsPowerUserVisible
        {
            get
            {
                if (string.IsNullOrEmpty(RightId))
                    return false;

                return !string.IsNullOrEmpty(m_Core.Rights[RightId].PowerUserMeaning);
            }
        }

        public bool IsFullControlVisible
        {
            get
            {
                if (string.IsNullOrEmpty(RightId))
                    return false;

                return !string.IsNullOrEmpty(m_Core.Rights[RightId].FullControlMeaning);
            }
        }

        public bool IsDeleteAllowedVisible
        {
            get
            {
                if (string.IsNullOrEmpty(RightId))
                    return false;

                return !string.IsNullOrEmpty(m_Core.Rights[RightId].DeleteAllowedMeaning);
            }
        }



        public bool IsListAllowedEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsReadAllowedEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsWriteAllowedEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsCreateAllowedEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsPowerUserEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsFullControlEnabled
        {
            get
            {
                return true;
            }
        }

        public bool IsDeleteAllowedEnabled
        {
            get
            {
                return true;
            }
        }



        public string ListAllowedMeaning
        {
            get
            {
                return m_Core.Rights[RightId].ListAllowedMeaning;
            }
        }

        public string ReadAllowedMeaning
        {
            get
            {
                return m_Core.Rights[RightId].ReadAllowedMeaning;
            }
        }

        public string WriteAllowedMeaning
        {
            get
            {
                return m_Core.Rights[RightId].WriteAllowedMeaning;
            }
        }

        public string CreateAllowedMeaning
        {
            get
            {
                return m_Core.Rights[RightId].CreateAllowedMeaning;
            }
        }

        public string PowerUserMeaning
        {
            get
            {
                return m_Core.Rights[RightId].PowerUserMeaning;
            }
        }

        public string FullControlMeaning
        {
            get
            {
                return m_Core.Rights[RightId].FullControlMeaning;
            }
        }

        public string DeleteAllowedMeaning
        {
            get
            {
                return m_Core.Rights[RightId].DeleteAllowedMeaning;
            }
        }


        public override string DisplayText
        {
            get
            {
                if(! string.IsNullOrEmpty(RoleId))
                    return m_Core.Roles[RoleId].DisplayText;
                return string.Empty;
            }
        }

        public AdminObject SecuredAdminObject
        {
            get
            {
                if (!string.IsNullOrEmpty(SecuredAdminObjectId))
                {
                    AdminObject ao = m_Core.GetAdminObject(SecuredAdminObjectId);
                    if (ao != null)
                    {
                        return ao;
                    }
                }
                return null;
            }
        }

        public ObjectSecurity Duplicate(string objectid, string roleId, AdminObjectList<ObjectSecurity> collection)
        {
            ObjectSecurity newos = Core.Create<ObjectSecurity>(Core);
            newos.SecuredAdminObjectId = objectid;
            newos.RoleId = roleId;
            newos.RightId = RightId;
            collection.Add(newos);
            newos.CreateAllowed = CreateAllowed;
            newos.DeleteAllowed = DeleteAllowed;
            newos.FullControl = FullControl;
            newos.ListAllowed = ListAllowed;
            newos.ReadAllowed = ReadAllowed;
            newos.WriteAllowed = WriteAllowed;
            newos.PowerUser = PowerUser;
            newos.GroupKey = GroupKey;
            return newos;
        }

    }

    [Admin.AdminSave(SkipSave = true)]
    public class ObjectSecurityHelper : AdminObject
    {        
        private AdminObjectReference<Role> m_Role = null;
        
        public ObjectSecurityHelper(AdminCore core)
            : base(core)
        {
            Init();
        }

        public ObjectSecurityHelper(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private void Init()
        {
            Role = new AdminObjectReference<Role>(this);
            Security = new ObservableCollection<ObjectSecurity>();
        }

        public string SecuredAdminObjectId
        {
            get
            {
                return GetFieldValue<string>("SecuredAdminObjectId");
            }
            set
            {
                SetFieldValue<string>("SecuredAdminObjectId", value);
            }
        }
        void m_SecuredAdminObjectId_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        public AdminObjectReference<Role> Role
        {
            get
            {
                return m_Role;
            }
            internal set
            {
                if (m_Role != null)
                {
                    m_Role.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Role_PropertyChanged);
                    m_Role.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Role_PropertyChanged);
                }

                m_Role = value;

                if (m_Role != null)
                {
                    m_Role.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Role_PropertyChanged);
                    m_Role.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Role_PropertyChanged);
                }
                FirePropertyChanged("DisplayText");
            }
        }

        void m_Role_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FirePropertyChanged("DisplayText");            
        }

        public override string DisplayText
        {
            get
            {
                return Role.Target.DisplayText;
            }
        }

        public string Rights
        {
            get
            {
                List<string> lstDescription = new List<string>();

                foreach (ObjectSecurity os in Security)
                {
                    List<string> lstSubDescription = new List<string>();
                    if (os.ListAllowed.HasValue && !string.IsNullOrEmpty(os.ListAllowedMeaning))
                    {
                        lstSubDescription.Add(string.Format("{0}{1}", os.ListAllowedMeaning, os.ListAllowed.Value ? string.Empty : " denied"));
                    }
                    if (os.ReadAllowed.HasValue && !string.IsNullOrEmpty(os.ReadAllowedMeaning))
                    {
                        lstSubDescription.Add(string.Format("{0}{1}", os.ReadAllowedMeaning, os.ReadAllowed.Value ? string.Empty : " denied"));
                    }
                    if (os.CreateAllowed.HasValue && !string.IsNullOrEmpty(os.CreateAllowedMeaning))
                    {
                        lstSubDescription.Add(string.Format("{0}{1}", os.CreateAllowedMeaning, os.CreateAllowed.Value ? string.Empty : " denied"));
                    }
                    if (os.WriteAllowed.HasValue && !string.IsNullOrEmpty(os.WriteAllowedMeaning))
                    {
                        lstSubDescription.Add(string.Format("{0}{1}", os.WriteAllowedMeaning, os.WriteAllowed.Value ? string.Empty : " denied"));
                    }
                    if (os.DeleteAllowed.HasValue && !string.IsNullOrEmpty(os.DeleteAllowedMeaning))
                    {
                        lstSubDescription.Add(string.Format("{0}{1}", os.DeleteAllowedMeaning, os.DeleteAllowed.Value ? string.Empty : " denied"));
                    }
                    if (os.PowerUser.HasValue && !string.IsNullOrEmpty(os.PowerUserMeaning))
                    {
                        lstSubDescription.Add(string.Format("{0}{1}", os.PowerUserMeaning, os.PowerUser.Value ? string.Empty : " denied"));
                    }
                    if (os.FullControl.HasValue && !string.IsNullOrEmpty(os.FullControlMeaning))
                    {
                        lstSubDescription.Add(string.Format("{0}{1}", os.FullControlMeaning, os.FullControl.Value ? string.Empty : " denied"));
                    }

                    if (lstSubDescription.Count > 0)
                    {
                        lstDescription.Add(string.Format("{0}({1})", os.RightDescription, string.Join(", ", lstSubDescription)));
                    }

                }
                if (lstDescription.Count == 0)
                    return string.Empty;

                return string.Join(", ", lstDescription);
 
            }
        }

        public ObservableCollection<ObjectSecurity> Security
        {
            get;
            internal set;
        }

        public AdminObject SecuredAdminObject
        {
            get
            {
                if (!string.IsNullOrEmpty(SecuredAdminObjectId))
                {
                    AdminObject ao = m_Core.GetAdminObject(SecuredAdminObjectId);
                    if (ao != null)
                    {
                        return ao;
                    }
                }
                return null;
            }
        }

    }

    [Admin.AdminSave(SkipSave = true)]
    public class Right : AdminObject
    {
        public Right(AdminCore core)
            : base(core)
        {
        }
        public Right(AdminObject parent)
            : base(parent)
        {
        }


        public string Description
        {
            get
            {
                return GetFieldValue<string>("Description");
            }
            set
            {
                SetFieldValue<string>("Description", value);

            }
        }

        public int Applicability
        {
            get
            {
                return GetFieldValue<int>("Applicability");
            }
            set
            {
                SetFieldValue<int>("Applicability", value);
            }
        }
        public string ListAllowedMeaning
        {
            get
            {
                return GetFieldValue<string>("ListAllowedMeaning");
            }
            set
            {
                SetFieldValue<string>("ListAllowedMeaning", value);
            }
        }

        public string ReadAllowedMeaning
        {
            get
            {
                return GetFieldValue<string>("ReadAllowedMeaning");
            }
            set
            {
                SetFieldValue<string>("ReadAllowedMeaning", value);
            }
        }
        public string WriteAllowedMeaning
        {
            get
            {
                return GetFieldValue<string>("WriteAllowedMeaning");
            }
            set
            {
                SetFieldValue<string>("WriteAllowedMeaning", value);
            }
        }
        public string CreateAllowedMeaning
        {
            get
            {
                return GetFieldValue<string>("CreateAllowedMeaning");
            }
            set
            {
                SetFieldValue<string>("CreateAllowedMeaning", value);
            }
        }
        public string PowerUserMeaning
        {
            get
            {
                return GetFieldValue<string>("PowerUserMeaning");
            }
            set
            {
                SetFieldValue<string>("PowerUserMeaning", value);
            }
        }
        public string FullControlMeaning
        {
            get
            {
                return GetFieldValue<string>("FullControlMeaning");
            }
            set
            {
                SetFieldValue<string>("FullControlMeaning", value);
            }
        }
        public string DeleteAllowedMeaning
        {
            get
            {
                return GetFieldValue<string>("DeleteAllowedMeaning");
            }
            set
            {
                SetFieldValue<string>("DeleteAllowedMeaning", value);
            }
        }

    }

}