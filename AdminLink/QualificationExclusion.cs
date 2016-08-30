using System;

namespace Nixxis.Client.Admin
{
    [AdminObjectLinkCascadeAttribute(typeof(Qualification), "ActivitiesExclusions")]
    [AdminObjectLinkCascadeAttribute(typeof(Activity), "QualificationsExclusions")]
    public class QualificationExclusion : AdminObjectLink<Activity, Qualification>
    {
        public QualificationExclusion(AdminObject parent)
            : base(parent)
        {
        }

        public string ActivityId
        {
            get
            {
                return Id1;
            }
        }

        public string QualificationId
        {
            get
            {
                return Id2;
            }
            set
            {
                Id2 = value;
            }
        }


        public Activity Activity
        {
            get
            {
                if (m_Core == null)
                    return null;

                return (Activity)(m_Core.GetAdminObject(Id1, typeof(Activity)));
            }
        }

        public Qualification Qualification
        {
            get 
            {
                if (m_Core == null)
                    return null;

                return (Qualification)(m_Core.GetAdminObject(Id2, typeof(Qualification))); 
            }
        }

    }
}