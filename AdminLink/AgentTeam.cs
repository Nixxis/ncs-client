using System;

namespace Nixxis.Client.Admin
{
    [AdminObjectLinkCascadeAttribute(typeof(Agent), "Teams")]
    [AdminObjectLinkCascadeAttribute(typeof(Team), "Agents")]
    public class AgentTeam : AdminObjectLink<Agent, Team>
    {
        public AgentTeam(AdminObject parent)
            : base(parent)
        {
            BaseLevel = 100;
        }


        public string AgentId
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

        public Agent Agent
        {
            get
            {
                if (m_Core == null)
                    return null;
                return (Agent)(m_Core.GetAdminObject(Id1));
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

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {

            System.Xml.XmlElement elm = base.CreateSaveNode(doc, operation);
            return elm;
        }

        public override void Save(System.Xml.XmlDocument doc)
        {
            base.Save(doc);
        }
    }
}