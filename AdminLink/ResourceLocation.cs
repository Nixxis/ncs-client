using System;

namespace Nixxis.Client.Admin
{
    [AdminSave(SkipSave=true)]
    [AdminObjectLinkCascadeAttribute(typeof(Resource), "Locations")]
    [AdminObjectLinkCascadeAttribute(typeof(Location), "Resources")]
    public class ResourceLocation : AdminObjectLink<Resource, Location>
    {
        public ResourceLocation(AdminObject parent)
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

        public string LocationId
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

        public Location Location
        {
            get 
            {
                if (m_Core == null)
                    return null;
                return (Location)(m_Core.GetAdminObject(Id2)); 
            }
        }

    }
}