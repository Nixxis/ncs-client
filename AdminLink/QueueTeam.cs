using System;

namespace Nixxis.Client.Admin
{
    [AdminObjectLinkCascadeAttribute(typeof(Queue), "Teams")]
    [AdminObjectLinkCascadeAttribute(typeof(Team), "Queues")]
    public class QueueTeam : AdminObjectLink<Queue, Team>
    {
        public QueueTeam(AdminObject parent)
            : base(parent)
        {
            BaseLevel = 100;
            MaxWaitTime = -1;
            MinWaitTime = -1;
        }


        public string QueueId
        {
            get
            {
                return Id1;
            }
        }

        public string TeamId
        {
            get
            {
                return Id2;
            }
        }

        public float BaseLevel
        {
            get { return GetFieldValue<float>("BaseLevel"); }
            set { SetFieldValue<float>("BaseLevel", value); }
        }

        public Queue Queue
        {
            get
            {
                if (m_Core == null)
                    return null;
                return (Queue)(m_Core.GetAdminObject(Id1));
            }
        }

        public Team Team
        {
            get 
            {
                if (m_Core == null)
                    return null;
                return (Team)(m_Core.GetAdminObject(Id2));
            }
        }

        public float MaxWaitTime
        {
            get
            {
                return GetFieldValue<float>("MaxWaitTime");
            }
            set
            {
                SetFieldValue<float>("MaxWaitTime", value);
            }
        }

        public float MinWaitTime
        {
            get
            {
                return GetFieldValue<float>("MinWaitTime");
            }
            set
            {
                SetFieldValue<float>("MinWaitTime", value);
            }
        }
    }
}