using System;

namespace Nixxis.Client.Admin
{
    [AdminObjectLinkCascadeAttribute(typeof(Resource), "Carriers")]
    [AdminObjectLinkCascadeAttribute(typeof(Carrier), "Resources")]
    public class ResourceCarrier : AdminObjectLink<Resource, Carrier>
    {
        public ResourceCarrier(AdminObject parent)
            : base(parent)
        {
        }

        public string ResourceId
        {
            get
            {
                return Id1;
            }
        }

        public string CarrierId
        {
            get
            {
                return Id2;
            }
        }

        public Resource Resource
        {
            get
            {
                if (m_Core == null)
                    return null;

                return (Resource)(m_Core.GetAdminObject(Id1));
            }
        }

        public Carrier Carrier
        {
            get 
            {
                if (m_Core == null)
                    return null;

                return (Carrier)(m_Core.GetAdminObject(Id2)); 
            }
        }

    }
}