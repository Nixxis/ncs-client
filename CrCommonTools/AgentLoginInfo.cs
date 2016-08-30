using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nixxis.Client
{
    public interface IAgentLoginInfoContainer
    {
        AgentLoginInfo AgentLoginInfo { get; }
    }

    public class BaseInfos
    {
        public string Id { get; internal set; }
        public string Description { get; internal set; }
        public string ExternalData { get; internal set; }
        public string GroupKey { get; internal set; }

        private BaseInfos()
        {
        }

        internal BaseInfos(XmlNode node)
        {
            if (node.Attributes["id"] != null)
            {
                Id = node.Attributes["id"].Value;
            }

            LoadFromNode(node);
        }

        public void LoadFromNode(XmlNode node)
        {
            foreach (XmlNode ChildNode in node)
            {
                if (!LoadChildElement(ChildNode))
                {
                    System.Reflection.PropertyInfo PInfo = this.GetType().GetProperty(ChildNode.Name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);

                    if (PInfo != null && (ChildNode.Attributes["nil"] == null || !Boolean.Parse(ChildNode.Attributes["nil"].Value)))
                    {
                        try
                        {
                            object Value = Convert.ChangeType(ChildNode.InnerText, PInfo.PropertyType);

                            PInfo.SetValue(this, Value, null);
                        }
                        catch(Exception e) { }
                    }
                }
            }
        }

        internal void AppendToList(XmlNode node, System.Collections.IList list, Type itemType, string tagName)
        {
            XmlNodeList Childs = node.SelectNodes(tagName);

            foreach (XmlNode ChildNode in Childs)
            {
                BaseInfos Item = null;

                if (ChildNode.Attributes["ref"] != null)
                {
                    XmlNode ChildDefinition = node.OwnerDocument.SelectSingleNode(string.Concat("//", tagName, "[@id=\"", ChildNode.Attributes["ref"].Value, "\"]"));

                    if (ChildDefinition == null)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Missing definition in AgentLoginInfo ({0})", ChildNode.Attributes["ref"].Value), "Warning");
                    }
                    else
                    {
                        Item = Activator.CreateInstance(itemType, ChildDefinition) as BaseInfos;

                        Item.LoadFromNode(ChildNode);
                    }
                }
                else if(ChildNode.Name.Equals("add", StringComparison.InvariantCultureIgnoreCase))
                {
                    list.Add(new KeyValuePair<string, string>(ChildNode.Attributes["key"].Value, ChildNode.Attributes["value"].Value));
                }
                else
                {
                    Item = Activator.CreateInstance(itemType, ChildNode) as BaseInfos;
                }

                if(Item!=null)
                    list.Add(Item);
            }
        }

        protected virtual bool LoadChildElement(XmlNode child)
        {
            return false;
        }
    }

    public class AgentLoginInfo : BaseInfos
    {
        public class Language : BaseInfos
        {
            public string Iso { get; protected set; }
            public int Level { get; protected set; }

            public Language(XmlNode node)
                : base(node)
            {
                if (node.Attributes["iso"] != null)
                {
                    Iso = node.Attributes["iso"].Value;
                }
            }
        }
        //TODO: Level is float and not int.
        public class Skill : BaseInfos
        {
            public int Level { get; protected set; }

            public Skill(XmlNode node)
                : base(node)
            {
            }
        }

        public class Team : BaseInfos
        {
            public int Cost { get; protected set; }
            public bool AutoReady { get; protected set; }
            public bool WrapUpExtendable { get; protected set; }
            public string WrapUpTime { get; protected set; }
            public int BaseLevel { get; protected set; }

            public Team(XmlNode node)
                : base(node)
            {
            }
        }

        public string Token { get; private set; }
        public bool Active { get; protected set; }
        public string Account { get; protected set; }
        public string FirstName { get; protected set; }
        public string LastName { get; protected set; }
        public bool AutoReady { get; protected set; }
        public bool WrapUpExtendable { get; protected set; }
        public string WrapUpTime { get; protected set; }
        public bool IsAgent { get; protected set; }
        public bool IsAdmin { get; protected set; }
        public bool IsSupervisor { get; protected set; }
        public bool IsReporter { get; protected set; }
        public bool IsRecorder { get; protected set; }
        public int CustomerVisibilityLevel { get; protected set; }
        public bool AllowTeamSelection { get; set; }
        public string GuiLanguage { get; protected set; }

        public string ReportServerUrl { get; protected set; }
        public string ReportsBasePath { get; protected set; }
        public string ReportsStatsBasePath { get; protected set; }
        public string ReportServerCredentialsUser { get; protected set; }
        public string ReportServerCredentialsPassword { get; protected set; }
        public string ReportServerCredentialsDomain { get; protected set; }


        public AgentLoginInfo(XmlNode node)
            : base(node)
        {
            if (node.ParentNode.Attributes["isagent"] != null)
            {
                IsAgent = System.Xml.XmlConvert.ToBoolean(node.ParentNode.Attributes["isagent"].Value);
            }

            if (node.ParentNode.Attributes["isadmin"] != null)
            {
                IsAdmin = System.Xml.XmlConvert.ToBoolean(node.ParentNode.Attributes["isadmin"].Value);
            }

            if (node.ParentNode.Attributes["issupervisor"] != null)
            {
                IsSupervisor = System.Xml.XmlConvert.ToBoolean(node.ParentNode.Attributes["issupervisor"].Value);
            }
            
            if (node.ParentNode.Attributes["isreporter"] != null)
            {
                IsReporter = System.Xml.XmlConvert.ToBoolean(node.ParentNode.Attributes["isreporter"].Value);
            }

            if (node.ParentNode.Attributes["isrecorder"] != null)
            {
                IsRecorder = System.Xml.XmlConvert.ToBoolean(node.ParentNode.Attributes["isrecorder"].Value);
            }


            if (node.Attributes["token"] != null)
            {
                Token = node.Attributes["token"].Value;
            }
        }
        

        protected override bool LoadChildElement(XmlNode child)
        {
            if (child.Name.Equals("teams", StringComparison.OrdinalIgnoreCase))
            {
                AppendToList(child, m_Teams, typeof(Team), "team");
                return true;
            }
            else if (child.Name.Equals("skills", StringComparison.OrdinalIgnoreCase))
            {
                AppendToList(child, m_Skills, typeof(Skill), "skill");
                return true;
            }
            else if (child.Name.Equals("languages", StringComparison.OrdinalIgnoreCase))
            {
                AppendToList(child, m_Languages, typeof(Language), "language");
                return true;
            }
            else if (child.Name.Equals("settings", StringComparison.OrdinalIgnoreCase))
            {
                AppendToList(child, m_Settings, null, null);
                return true;
            }
            else
                return false;
        }


        private List<KeyValuePair<string, string>> m_Settings = new List<KeyValuePair<string, string>>();

        public List<KeyValuePair<string, string>> Settings
        {
            get
            {
                return m_Settings;
            }
        }
        
        //List of Teams
        private List<Team> m_Teams = new List<Team>();

        public List<Team> Teams
        {
            get
            {
                return m_Teams;
            }
        }

        public Team GetTeam(string Id)
        {
            foreach (Team t in m_Teams)
            {
                if (t.Id == Id)
                    return t;
            }
            return null;
        }

        //List of Skills
        private List<Skill> m_Skills = new List<Skill>();

        public List<Skill> Skills
        {
            get
            {
                return m_Skills;
            }
        }
        public Skill GetSkill(string Id)
        {
            foreach (Skill s in m_Skills)
            {
                if (s.Id == Id)
                    return s;
            }
            return null;
        }

        //List of langages
        private List<Language> m_Languages = new List<Language>();

        public List<Language> Languages
        {
            get
            {
                return m_Languages;
            }
        }

        public Language GetLanguage(string iso)
        {
            foreach (Language l in m_Languages)
            {
                if (l.Iso == iso)
                    return l;
            }
            return null;
        }
    }
}
