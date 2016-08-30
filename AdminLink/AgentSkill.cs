using System;

namespace Nixxis.Client.Admin
{
    [AdminObjectLinkCascadeAttribute(typeof(Agent), "Skills")]
    [AdminObjectLinkCascadeAttribute(typeof(Skill), "Agents")]
    public class AgentSkill : AdminObjectLink<Agent, Skill>
    {
        public AgentSkill(AdminObject parent)
            : base(parent)
        {
            Level = 100;
        }

        public string AgentId
        {
            get
            {
                return Id1;
            }
        }

        public string SkillId
        {
            get
            {
                return Id2;
            }
        }

        public float Level
        {
            get { return GetFieldValue<float>("Level"); }
            set { SetFieldValue<float>("Level", value); }
        }

        public Agent Agent
        {
            get
            {
                if (m_Core == null)
                    return null;

                return (Agent)(m_Core.GetAdminObject(Id1));
            }
        }

        public Skill Skill
        {
            get 
            {
                if (m_Core == null)
                    return null;

                return (Skill)(m_Core.GetAdminObject(Id2)); 
            }
        }

    }
}