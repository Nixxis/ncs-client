using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nixxis;
using System.Security.Cryptography;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Nixxis.Client.Admin
{

    public class SecurityContext : SecuredAdminObject
    {
        public SecurityContext(AdminCore core)
            : base(core)
        {
            Init();
        }

        public SecurityContext(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private void Init()
        {
            //GroupId = null;
            SecuredAdminObjects = new AdminObjectList<AdminObjectSecurityContext>(this, false, false);
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


        public SecurityContext Duplicate()
        {
            SecurityContext newsecuritycontext = AdminCore.Create<SecurityContext>();
            newsecuritycontext.Description = string.Concat(Description, "*");
            newsecuritycontext.GroupKey = GroupKey;
            newsecuritycontext.AdminCore.SecurityContexts.Add(newsecuritycontext);
            DuplicateSecurity(newsecuritycontext);
            return newsecuritycontext;
        }

        [AdminLoad(SkipLoad = true)]
        public AdminObjectList<AdminObjectSecurityContext> SecuredAdminObjects
        {
            get;
            internal set;
        }

        public object CheckedAdminObjects
        {
            get
            {
                return new AdminCheckedLinkList<SecuredAdminObject, AdminObjectSecurityContext>(m_Core.AllSecuredObjects.Where( (sc) => (!(sc is SecurityContext) && !sc.IsSystem ) ), SecuredAdminObjects, this);
            }
        }

    }
}
