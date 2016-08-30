using System;

namespace Nixxis.Client.Admin
{
    [AdminSave(SkipSave=true)]
    [AdminObjectLinkCascadeAttribute(typeof(Phone), "Locations")]
    [AdminObjectLinkCascadeAttribute(typeof(Location), "Phones")]
    public class PhoneLocation : AdminObjectLink<Phone, Location>
    {
        public PhoneLocation(AdminObject parent)
            : base(parent)
        {
        }

        public string PhoneId
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


        public Phone Phone
        {
            get
            {
                if (m_Core == null)
                    return null;
                return (Phone)(m_Core.GetAdminObject(Id1));
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