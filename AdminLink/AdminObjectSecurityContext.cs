using System;

namespace Nixxis.Client.Admin
{
    [AdminSave(SkipSave=true)]
    [AdminObjectLinkCascadeAttribute(typeof(SecuredAdminObject), "SecurityContexts")]
    [AdminObjectLinkCascadeAttribute(typeof(SecurityContext), "SecuredAdminObjects")]
    public class AdminObjectSecurityContext : AdminObjectLink<SecuredAdminObject, SecurityContext>
    {
        public AdminObjectSecurityContext(AdminObject parent)
            : base(parent)
        {
        }

        public string SecuredAdminObjectId
        {
            get
            {
                return Id1;
            }
        }

        public string SecurityContextId
        {
            get
            {
                return Id2;
            }
        }


        public SecuredAdminObject SecuredAdminObject
        {
            get
            {
                if (m_Core == null)
                    return null;
                return (SecuredAdminObject)(m_Core.GetAdminObject(Id1));
            }
        }

        public SecurityContext SecurityContext
        {
            get 
            {
                if (m_Core == null)
                    return null;

                return (SecurityContext)(m_Core.GetAdminObject(Id2)); 
            }
        }

    }
}