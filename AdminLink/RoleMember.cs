using System;

namespace Nixxis.Client.Admin
{
    [AdminObjectLinkCascadeAttribute(typeof(Agent), "Roles")]
    [AdminObjectLinkCascadeAttribute(typeof(Role), "Agents")]
    public class RoleMember : AdminObjectLink<Agent, Role>
    {
        public RoleMember(AdminObject parent)
            : base(parent)
        {
        }


        public string AgentId
        {
            get
            {
                return Id1;
            }
        }

        public string RoleId
        {
            get
            {
                return Id2;
            }
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

        public Role Role
        {
            get 
            {
                if (m_Core == null)
                    return null;
                return (Role)(m_Core.GetAdminObject(Id2)); 
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