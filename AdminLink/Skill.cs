namespace Nixxis.Client.Admin
{
    public class Skill : AdminObject
    {

        public override string GroupKey
        {
            get
            {
                return GetFieldValue<string>("GroupKey");
            }
            set
            {
                SetFieldValue("GroupKey", value);
            }
        }

        public string Description
        {
            get
            {
                return GetFieldValue<string>("Description") ;
            }
            set
            {
                SetFieldValue<string>("Description", value);
                FirePropertyChanged("DisplayText");
                FirePropertyChanged("TypedDisplayText");
            }
        }

        public override string ShortDisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.ShortDisplayText;
                else
                    return Description;
            }
        }

        public override string DisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.DisplayText;
                else
                    return Description;
            }
        }

        public override string TypedDisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.TypedDisplayText;
                else
                    return string.Concat("Skill ", Description);
            }
        }


        public Skill(AdminObject parent)
            : base(parent)
        {           
        }

        public Skill(AdminCore core)
            : base(core)
        {
        }


        [AdminLoad(Path = "/Admin/AgentsSkills/AgentSkill[@skillid=\"{0}\"]")]
        public AdminObjectList<AgentSkill> Agents
        {
            get;
            internal set;
        }

        public object CheckedAgents
        {
            get
            {
                return new AdminCheckedLinkList<Agent, AgentSkill>(m_Core.Agents, Agents, this);
            }
        }


        [AdminLoad(Path = "/Admin/ActivitiesSkills/ActivitySkill[@skillid=\"{0}\"]")]
        public AdminObjectList<ActivitySkill> Activities
        {
            get;
            internal set;
        }

        public object CheckedActivities
        {
            get
            {
                return new AdminCheckedLinkList<Activity, ActivitySkill>(m_Core.InboundActivities, Activities, this);
            }
        }

        public Skill Duplicate()
        {
            Skill newskill = AdminCore.Create<Skill>();

            newskill.Description = string.Concat(Description, "*");
            newskill.GroupKey = GroupKey;

            foreach (ActivitySkill ask in Activities)
            {
                newskill.Activities.Add(ask.Activity);
                newskill.Activities[newskill.Activities.Count - 1].Level = ask.Level;
                newskill.Activities[newskill.Activities.Count - 1].MinimumLevel = ask.MinimumLevel;
            }

            foreach (AgentSkill ags in Agents)
            {
                newskill.Agents.Add(ags.Agent);
                newskill.Agents[newskill.Agents.Count - 1].Level = ags.Level;
            }

            newskill.AdminCore.Skills.Add(newskill);
            return newskill;
        }
    }
}