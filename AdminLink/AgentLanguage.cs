using System;

namespace Nixxis.Client.Admin
{
    [AdminObjectLinkCascadeAttribute(typeof(Agent), "Languages")]
    [AdminObjectLinkCascadeAttribute(typeof(Language), "Agents")]
    public class AgentLanguage : AdminObjectLink<Agent, Language>
    {
        public AgentLanguage(AdminObject parent)
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

        public string LanguageId
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

        public Language Language
        {
            get 
            {
                if (m_Core == null)
                    return null;

                return (Language)(m_Core.GetAdminObject(Id2)); 
            }
        }
    }
}