using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nixxis;
using System.Security.Cryptography;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Collections.Specialized;

namespace Nixxis.Client.Admin
{

    public class Role : AdminObject
    {
        public Role(AdminCore core)
            : base(core)
        {
            Init();
        }

        public Role(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private AdminObjectList<ObjectSecurity> m_SecuredObjects = null;
        private ObservableCollection<ObjectSecurityHelper> m_FilteredSecuredObjects = new ObservableCollection<ObjectSecurityHelper>();

        private void Init()
        {
            SecuredObjects = new AdminObjectList<ObjectSecurity>(this);
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

        public override string GroupKey
        {
            get
            {
                return GetFieldValue<string>("GroupKey");
            }
            set
            {
                if(string.IsNullOrEmpty(value))
                    SetFieldValue<string>("GroupKey", null);
                else
                    SetFieldValue<string>("GroupKey", value);
            }
        }

        public override string ShortDisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.ShortDisplayText;
                else
                    return Description;
            }
        }

        public override string DisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.DisplayText;
                else
                    return Description;
            }
        }

        public override string TypedDisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.TypedDisplayText;
                else
                    return string.Concat("Role ", Description);
            }
        }

        public bool NotAllowedMeansDenied
        {
            get
            {
                return GetFieldValue<bool>("NotAllowedMeansDenied");
            }
            set
            {
                 SetFieldValue<bool>("NotAllowedMeansDenied", value);
            }
        }

        public Role Duplicate()
        {
            Role newrole = AdminCore.Create<Role>();
            newrole.Description = string.Concat(Description, "*");
            newrole.GroupKey = GroupKey;
            newrole.NotAllowedMeansDenied = NotAllowedMeansDenied;
            newrole.AdminCore.Roles.Add(newrole);

            foreach (RoleMember rm in Agents)
            {
                newrole.Agents.Add(rm.Agent);
            }

            foreach (ObjectSecurity os in DefaultRights.Security)
            {
                foreach (ObjectSecurity os2 in newrole.DefaultRights.Security)
                {
                    if (os2.RightId == os.RightId)
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

            List<ObjectSecurity> tempos = new List<ObjectSecurity>();
            foreach (ObjectSecurity os in SecuredObjects)
                tempos.Add(os);

            foreach (ObjectSecurity os in tempos)
            {
                ObjectSecurity newos = os.Duplicate(os.SecuredAdminObjectId, newrole.Id, newrole.SecuredObjects);

                foreach (ObjectSecurityHelper osh in (os.SecuredAdminObject as SecuredAdminObject).RolesOverview)
                {
                    if (osh.Role.TargetId == newrole.Id)
                    {
                        foreach (ObjectSecurity os2 in osh.Security)
                        {
                            if (os2.SecuredAdminObjectId == newos.SecuredAdminObjectId && os2.RoleId == newos.RoleId && os2.RightId == newos.RightId)
                            {
                                os2.ReadAllowed = newos.ReadAllowed;
                                os2.CreateAllowed = newos.CreateAllowed;
                                os2.ListAllowed = newos.ListAllowed;
                                os2.WriteAllowed = newos.WriteAllowed;
                                os2.PowerUser = newos.PowerUser;
                                os2.FullControl = newos.FullControl;
                                os2.DeleteAllowed = newos.DeleteAllowed;
                            }
                        }
                    }
                }
               
            }

            return newrole;
        }

        [AdminLoad(Path = "/Admin/RoleMembers/RoleMember[@roleid=\"{0}\"]")]
        public AdminObjectList<RoleMember> Agents
        {
            get;
            internal set;
        }

        public AdminCheckedLinkList<Agent, RoleMember> CheckedAgents
        {
            get
            {
                return new AdminCheckedLinkList<Agent, RoleMember>(m_Core.Agents, Agents, this);
            }
        }



        [AdminLoad(Path = "/Admin/ObjectsSecurity/ObjectSecurity[@roleid=\"{0}\"]")]
        public AdminObjectList<ObjectSecurity> SecuredObjects
        {
            get
            {
                return m_SecuredObjects;
            }
            internal set
            {
                if (m_SecuredObjects != null)
                    m_SecuredObjects.CollectionChanged -= new NotifyCollectionChangedEventHandler(m_SecuredObjects_CollectionChanged);

                m_SecuredObjects = value;

                m_SecuredObjects_CollectionChanged(null, null);

                if (m_SecuredObjects != null)
                    m_SecuredObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(m_SecuredObjects_CollectionChanged);
            }
        }

        void m_SecuredObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            m_FilteredSecuredObjects.Clear();
            foreach (ObjectSecurity os in SecuredObjects)
            {
                if (os.SecuredAdminObjectId != "defaultsetting++++++++++++++++++" && (os.ListAllowed.HasValue || os.ReadAllowed.HasValue || os.WriteAllowed.HasValue || os.CreateAllowed.HasValue || os.DeleteAllowed.HasValue || os.PowerUser.HasValue || os.FullControl.HasValue))
                {
                    ObjectSecurityHelper oshelper = m_FilteredSecuredObjects.FirstOrDefault((osh) => (osh.SecuredAdminObjectId == os.SecuredAdminObjectId && osh.Role.Target == this));
                    if (oshelper == null)
                    {
                        oshelper = new ObjectSecurityHelper(this);
                        oshelper.Role.Target = this;
                        oshelper.SecuredAdminObjectId = os.SecuredAdminObjectId;
                        m_FilteredSecuredObjects.Add(oshelper);
                    }

                    if (oshelper.Security.FirstOrDefault((osh) => (osh.RoleId == os.RoleId && osh.RightId == os.RightId && osh.SecuredAdminObjectId == os.SecuredAdminObjectId)) == null)
                    {
                        oshelper.Security.Add(os);
                    }
                }
            }
        }


        public ObservableCollection<ObjectSecurityHelper> FilteredSecuredObjects
        {
            get
            {
                return m_FilteredSecuredObjects;
            }
        }


        public ObjectSecurityHelper DefaultRights
        {
            get
            {
                ObjectSecurityHelper os = Core.DefaultSettings.RolesOverview.FirstOrDefault((o) => (o.Role.TargetId == this.Id));
                return os;
            }
        }

    }
}
