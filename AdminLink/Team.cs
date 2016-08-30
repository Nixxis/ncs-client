using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Xml;
using Nixxis;

namespace Nixxis.Client.Admin
{
    public class Team : SecuredAdminObject
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("Teams");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        private AdminObjectReference<SecurityContext> m_SecurityContext = null;
        private SingletonAdminObjectList<AdminObjectSecurityContext> m_SecurityContexts = null;


        internal override bool Reload(XmlNode node)
        {
            if (base.Reload(node))
            {
                if (node.Attributes["owner"] != null && node.Attributes["owner"].Value != null)
                {
                    SetFieldValue<string>("OwnerId", node.Attributes["owner"].Value);
                    SetFieldLoaded("OwnerId");
                }
                else
                {
                    SetFieldValue<string>("OwnerId", null);
                    SetFieldLoaded("OwnerId");
                }

                return true;
            }
            return false;
        }


        public Team(AdminCore core)
            : base(core)
        {
            Init();
        }

        public Team(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private void Init()
        {
            PauseGroup = null;// string.Empty;
            Cost = 0;
            SecurityContext = new AdminObjectReference<SecurityContext>(this);
            SecurityContexts = new SingletonAdminObjectList<AdminObjectSecurityContext>(this);

        }

        public string Description
        {
            get
            {
                return GetFieldValue<string>("Description");
            }
            set
            {
                SetFieldValue<string>("Description", value);
                FirePropertyChanged("ShortDisplayText");
                FirePropertyChanged("DisplayText");
                FirePropertyChanged("TypedDisplayText");
            }
        }

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

        [AdminLoad(Path = "/Admin/QueuesTeams/QueueTeam[@teamid=\"{0}\"]")]
        public AdminObjectList<QueueTeam> Queues
        {
            get; internal set;
        }

        [AdminLoad(Path = "/Admin/AgentsTeams/AgentTeam[@teamid=\"{0}\"]")]
        public AdminObjectList<AgentTeam> Agents
        {
            get; internal set;
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
                    return string.Concat("Team ", Description);
            }
        }

        public object CheckedQueues
        {
            get
            {
                return new AdminCheckedLinkList<Queue, QueueTeam>(m_Core.Queues, Queues, this);
            }
        }

        public AdminCheckedLinkList<Agent, AgentTeam> CheckedAgents
        {
            get
            {
                return new AdminCheckedLinkList<Agent, AgentTeam>(m_Core.Agents, Agents, this);
            }
        }

        public string PauseGroup
        {
            get
            {
                return GetFieldValue<string>("PauseGroup");
            }
            set
            {
                SetFieldValue<string>("PauseGroup", value);
            }
        }

        public float Cost
        {
            get
            {
                return GetFieldValue<float>("Cost");
            }
            set
            {
                SetFieldValue<float>("Cost", value);
            }
        }
        #region Autoready
        public bool WrapupExtendable
        {
            get
            {
                return GetFieldValue<bool>("WrapupExtendable");
            }
            set
            {
                SetFieldValue<bool>("WrapupExtendable", value);
                FirePropertyChanged("PostWrapupBehaviorDescription");
                FirePropertyChanged("IsPostWrapupBehaviorDefined");
            }
        }

        public int WrapupTime
        {
            get
            {
                return GetFieldValue<int>("WrapupTime");
            }
            set
            {
                SetFieldValue<int>("WrapupTime", value);
                FirePropertyChanged("IsPostWrapupBehaviorDefined");
                FirePropertyChanged("PostWrapupBehaviorDescription");
            }
        }

        public bool IsWrapupTimeModified
        {
            get
            {
                return PostWrapupOptionsIncreaseWrapupDuration || PostWrapupOptionsDecreaseWrapupDuration || PostWrapupOptionsSetWrapupDuration;
            }
            set
            {
                if (value)
                {
                    PostWrapupOptionsIncreaseWrapupDuration = true;
                }
                else
                {
                    PostWrapupOptionsIncreaseWrapupDuration = false;
                    PostWrapupOptionsDecreaseWrapupDuration = false;
                    PostWrapupOptionsSetWrapupDuration = false;
                }
            }
        }

        public bool IsAutoReadyDelayModified
        {
            get
            {
                return PostWrapupOptionsIncreaseAutoReadyDelay || PostWrapupOptionsDecreaseAutoReadyDelay || PostWrapupOptionsSetAutoReadyDelay;
            }
            set
            {
                if (value)
                {
                    PostWrapupOptionsIncreaseAutoReadyDelay = true;
                }
                else
                {
                    PostWrapupOptionsIncreaseAutoReadyDelay = false;
                    PostWrapupOptionsDecreaseAutoReadyDelay = false;
                    PostWrapupOptionsSetAutoReadyDelay = false;
                }
            }
        }

        public int PostWrapupOptionsValue
        {
            get
            {
                return GetFieldValue<int>("PostWrapupOptionsValue");
            }
            set
            {
                SetFieldValue<int>("PostWrapupOptionsValue", value);
                FirePropertyChanged("IsPostWrapupBehaviorDefined");
                FirePropertyChanged("PostWrapupBehaviorDescription");

                FirePropertyChanged("PostWrapupOptionsIncreaseWrapupDuration");
                FirePropertyChanged("PostWrapupOptionsDecreaseWrapupDuration");
                FirePropertyChanged("PostWrapupOptionsSetWrapupDuration");
                FirePropertyChanged("PostWrapupOptionsIncreaseAutoReadyDelay");
                FirePropertyChanged("PostWrapupOptionsDecreaseAutoReadyDelay");
                FirePropertyChanged("PostWrapupOptionsSetAutoReadyDelay");
            }
        }

        public int AutoReadyDelay
        {
            get
            {
                return GetFieldValue<int>("AutoReadyDelay");
            }
            set
            {
                SetFieldValue<int>("AutoReadyDelay", value);
                FirePropertyChanged("PostWrapupBehaviorDescription");
                FirePropertyChanged("IsPostWrapupBehaviorDefined");
            }
        }

        private bool? EvaluatePostWrapupOptions(PostWrapupOption option)
        {
            PostWrapupOption pwo = PostWrapupOptionHelper.AllowedOptions(PostWrapupOptionsValue);
            if ((pwo | option) == pwo)
            {
                return true;
            }
            pwo = PostWrapupOptionHelper.DeniedOptions(PostWrapupOptionsValue);
            if ((pwo | option) == pwo)
            {
                return false;
            }
            return null;

        }

        private void SetPostWrapupoptions(bool? val, PostWrapupOption option)
        {
            if (val.HasValue)
            {
                if (val.Value)
                {
                    PostWrapupOptionsValue = PostWrapupOptionHelper.ComputeEncodedForm(PostWrapupOptionsValue, option, PostWrapupOption.None);
                }
                else
                {
                    PostWrapupOptionsValue = PostWrapupOptionHelper.ComputeEncodedForm(PostWrapupOptionsValue, PostWrapupOption.None, option);
                }
            }
            else
            {
                PostWrapupOptionsValue = PostWrapupOptionHelper.ComputeEncodedForm(PostWrapupOptionsValue, option);
            }

        }

        public bool? PostWrapupOptionsSupAlert
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.AlertSup);
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.AlertSup);
            }
        }
        public bool? PostWrapupOptionsAlert
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.Alert);
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.Alert);
            }
        }
        public bool? PostWrapupOptionsCloseScript
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.CloseScript);
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.CloseScript);
            }
        }
        public bool? PostWrapupOptionsForceReady
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.ForceReady);
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.ForceReady);

            }
        }
        public bool? PostWrapupOptionsReadyWhenScriptIsClosed
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.ReadyWhenScriptIsClosed);
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.ReadyWhenScriptIsClosed);
            }
        }
        public bool PostWrapupOptionsIncreaseAutoReadyDelay
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.IncreaseAutoReadyDelay).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.IncreaseAutoReadyDelay);
            }
        }
        public bool PostWrapupOptionsDecreaseAutoReadyDelay
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.DecreaseAutoReadyDelay).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.DecreaseAutoReadyDelay);
            }
        }
        public bool PostWrapupOptionsSetAutoReadyDelay
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.SetAutoReadyDelay).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.SetAutoReadyDelay);
            }
        }
        public bool PostWrapupOptionsIncreaseWrapupDuration
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.IncreaseWrapupDuration).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.IncreaseWrapupDuration);
            }
        }
        public bool PostWrapupOptionsDecreaseWrapupDuration
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.DecreaseWrapupDuration).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.DecreaseWrapupDuration);
            }
        }
        public bool PostWrapupOptionsSetWrapupDuration
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.SetWrapupDuration).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.SetWrapupDuration);
            }
        }


        private bool m_IsPostWrapupBehaviorDefined = false;

        public bool IsPostWrapupBehaviorDefined
        {
            get
            {
                return m_IsPostWrapupBehaviorDefined || WrapupTime != 0 || PostWrapupOptionsValue != 0 || AutoReadyDelay != 0 || WrapupExtendable;
            }
            set
            {
                if (value)
                {
                    m_IsPostWrapupBehaviorDefined = true;
                }
                else
                {
                    m_IsPostWrapupBehaviorDefined = false;
                    WrapupTime = 0;
                    PostWrapupOptionsValue = 0;
                    AutoReadyDelay = 0;
                    WrapupExtendable = false;
                }
            }
        }
        public string PostWrapupBehaviorDescription
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                if (PostWrapupOptionsIncreaseWrapupDuration)
                {
                    builder.AppendFormat(Translate("Wrapup standard duration is increased by {0}. "), DurationHelpers.GetDefaultDurationString(WrapupTime / 1000, false));
                }
                else if (PostWrapupOptionsDecreaseWrapupDuration)
                {
                    builder.AppendFormat(Translate("Wrapup standard duration is decreased by {0}. "), DurationHelpers.GetDefaultDurationString(WrapupTime / 1000, false));
                }
                else if (PostWrapupOptionsSetWrapupDuration)
                {
                    builder.AppendFormat(Translate("Wrapup standard duration is set to {0}. "), DurationHelpers.GetDefaultDurationString(WrapupTime / 1000, false));
                }

                if (WrapupExtendable)
                {
                    builder.AppendFormat(Translate("The wrapup standard duration can be extended. "));
                }


                List<string> listOptions = new List<string>();

                if (PostWrapupOptionsSupAlert.HasValue)
                {
                    if (PostWrapupOptionsSupAlert.Value)
                    {
                        listOptions.Add(Translate("supervisor receives an alert"));
                    }
                    else
                    {
                        listOptions.Add(Translate("supervisor alert is cancelled"));
                    }
                }
                if (PostWrapupOptionsAlert.HasValue)
                {
                    if (PostWrapupOptionsAlert.Value)
                    {
                        listOptions.Add(Translate("agent receives an alert"));
                    }
                    else
                    {
                        listOptions.Add(Translate("agent alert is cancelled"));
                    }
                }
                if (PostWrapupOptionsForceReady.HasValue)
                {
                    if (PostWrapupOptionsForceReady.Value)
                    {
                        listOptions.Add(Translate("agent is forced to ready"));
                    }
                    else
                    {
                        listOptions.Add(Translate("agent is not forced to ready"));
                    }
                }
                if (PostWrapupOptionsCloseScript.HasValue)
                {
                    if (PostWrapupOptionsCloseScript.Value)
                    {
                        listOptions.Add(Translate("script is closed"));
                    }
                    else
                    {
                        listOptions.Add(Translate("script stays open"));
                    }
                }

                if (listOptions.Count > 0)
                {
                    builder.Append(Translate("When wrapup standard duration is ellapsed, "));
                    for (int i = 0; i < listOptions.Count; i++)
                    {
                        string str = listOptions[i];
                        builder.Append(str);
                        if (i < listOptions.Count - 1)
                            builder.Append(Translate(", "));
                        else
                            builder.Append(Translate(". "));
                    }
                }

                if (PostWrapupOptionsIncreaseAutoReadyDelay)
                {
                    builder.AppendFormat(Translate("The delay before being ready is increased by {0}. "), DurationHelpers.GetDefaultDurationString(AutoReadyDelay / 1000, false));
                }
                else if (PostWrapupOptionsDecreaseAutoReadyDelay)
                {
                    builder.AppendFormat(Translate("The delay before being ready is decreased by {0}. "), DurationHelpers.GetDefaultDurationString(AutoReadyDelay / 1000, false));
                }
                else if (PostWrapupOptionsSetAutoReadyDelay)
                {
                    builder.AppendFormat(Translate("The delay before being ready is set to {0}. "), DurationHelpers.GetDefaultDurationString(AutoReadyDelay / 1000, false));
                }

                if (PostWrapupOptionsReadyWhenScriptIsClosed.HasValue)
                {
                    if (PostWrapupOptionsReadyWhenScriptIsClosed.Value)
                        builder.AppendFormat(Translate("Closing the script sets the agent ready. "));
                    else
                        builder.AppendFormat(Translate("Closing the script does not set the agent ready. "));
                }

                return builder.ToString();
            }
        }
        #endregion

        public Team Duplicate()
        {
            Team newteam = AdminCore.Create<Team>();

            newteam.Description = string.Concat(Description, "*");
            newteam.Cost = Cost;
            newteam.GroupKey = GroupKey;
            newteam.PauseGroup = PauseGroup;

            newteam.AutoReadyDelay = AutoReadyDelay;
            newteam.WrapupExtendable = WrapupExtendable;
            newteam.WrapupTime = WrapupTime;
            newteam.PostWrapupOptionsValue = PostWrapupOptionsValue;

            foreach (AgentTeam agt in Agents)
            {
                newteam.Agents.Add(agt.Agent);
                newteam.Agents[newteam.Agents.Count - 1].BaseLevel = agt.BaseLevel;
            }

            foreach (QueueTeam qt in Queues)
            {
                newteam.Queues.Add(qt.Queue);
                newteam.Queues[newteam.Queues.Count - 1].BaseLevel = qt.BaseLevel;
                newteam.Queues[newteam.Queues.Count - 1].MaxWaitTime = qt.MaxWaitTime;
                newteam.Queues[newteam.Queues.Count - 1].MinWaitTime = qt.MinWaitTime;
            }
            

            newteam.SecurityContext.TargetId = SecurityContext.TargetId;

            newteam.AdminCore.Teams.Add(newteam);

            DuplicateSecurity(newteam);

            return newteam;
       }
        protected override XmlElement CreateSaveNode(XmlDocument doc, string operation)
        {
            try
            {
                if (Owner != null)
                {
                    Owner.EmptySave(doc);
                }
            }
            catch
            {
            }

            if (PauseGroup == string.Empty)
                PauseGroup = null;
            return base.CreateSaveNode(doc, operation);
        }

        public override void EmptySave(XmlDocument doc)
        {
            try
            {
                if (Owner != null)
                {
                    Owner.EmptySave(doc);
                }
            }
            catch
            {
            }
            base.EmptySave(doc);
        }

        public AdminObjectReference<SecurityContext> SecurityContext
        {
            get
            {
                return m_SecurityContext;
            }
            internal set
            {
                if (m_SecurityContext != null)
                {
                    m_SecurityContext.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_SecurityContext_PropertyChanged);
                    m_SecurityContext.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_SecurityContext_PropertyChanged);
                }

                m_SecurityContext = value;

                if (m_SecurityContext != null)
                {
                    m_SecurityContext.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_SecurityContext_PropertyChanged);
                    m_SecurityContext.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_SecurityContext_PropertyChanged);
                }

                SyncSecurityContextsFromSecurityContext();
            }
        }

        void m_SecurityContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SyncSecurityContextsFromSecurityContext();
        }

        [AdminLoad(SkipLoad = true)]
        public SingletonAdminObjectList<AdminObjectSecurityContext> SecurityContexts
        {
            get
            {
                return m_SecurityContexts;
            }
            internal set
            {
                if (m_SecurityContexts != null)
                    m_SecurityContexts.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_SecurityContexts_CollectionChanged);

                m_SecurityContexts = value;

                if (m_SecurityContexts != null)
                    m_SecurityContexts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_SecurityContexts_CollectionChanged);

                SyncSecurityContextFromSecurityContexts();
            }

        }

        void m_SecurityContexts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SyncSecurityContextFromSecurityContexts();
        }

        private bool Syncing = false;

        private void SyncSecurityContextsFromSecurityContext()
        {
            lock (this)
            {
                if (SecurityContexts == null || SecurityContext == null || Syncing)
                    return;

                Syncing = true;
            }

            if (!SecurityContext.HasTarget)
            {
                if (SecurityContexts.Count == 0)
                {
                }
                else if (SecurityContexts.Count == 1)
                {
                    SecurityContexts[0].SecurityContext.SecuredAdminObjects.Remove(this);
                }
                else if (SecurityContexts.Count > 1)
                {
                    System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
                }
            }
            else
            {
                if (SecurityContexts.Count == 0)
                {
                    SecurityContexts.Add(SecurityContext.Target);
                }
                else if (SecurityContexts.Count == 1)
                {
                    if (SecurityContexts[0].SecurityContextId != SecurityContext.TargetId)
                    {
                        SecurityContexts.RemoveAt(0);
                        SecurityContexts.Add(SecurityContext.Target);
                    }
                }
                else if (SecurityContexts.Count > 1)
                {
                    System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
                }
            }

            lock (this)
            {
                Syncing = false;
            }


        }

        private void SyncSecurityContextFromSecurityContexts()
        {
            lock (this)
            {
                if (SecurityContexts == null || SecurityContext == null || Syncing)
                    return;

                Syncing = true;
            }

            if (SecurityContexts.Count == 0)
            {
                if (SecurityContext.HasTarget)
                {
                    SecurityContext.TargetId = null;
                }
            }
            else if (SecurityContexts.Count == 1)
            {
                if (SecurityContext.HasTarget)
                {
                    if (SecurityContext.TargetId != SecurityContexts[0].SecurityContextId)
                        SecurityContext.TargetId = SecurityContexts[0].SecurityContextId;
                }
                else
                {
                    SecurityContext.TargetId = SecurityContexts[0].SecurityContextId;
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
            }

            lock (this)
            {
                Syncing = false;
            }
        }


        public override bool? Is_Visible(string rightid)
        {
            bool? tempResult1 = base.Is_Visible(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Is_Visible(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }
            }

            return tempResult1;
        }
        public override bool? Is_Listable(string rightid)
        {
            bool? tempResult1 = base.Is_Listable(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Is_Listable(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }
            }

            return tempResult1;
        }

        internal override bool? Has_FullControlFlag(string rightid)
        {
            bool? tempResult1 = base.Has_FullControlFlag(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Has_FullControlFlag(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }
            }

            return tempResult1;
        }

        internal override bool? Has_PowerFlag(string rightid)
        {
            bool? tempResult1 = base.Has_PowerFlag(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Has_PowerFlag(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }
            }

            return tempResult1;
        }

        internal override bool? Is_Modifiable(string rightid)
        {
            bool? tempResult1 = base.Is_Modifiable(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Is_Modifiable(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }

            }

            return tempResult1;
        }

        internal override bool? Is_Deletable(string rightid)
        {
            bool? tempResult1 = base.Is_Deletable(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Is_Deletable(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }

            }

            return tempResult1;
        }

        internal override bool? Is_RighstHandlingAllowed(string rightid)
        {
            bool? tempResult1 = base.Is_RighstHandlingAllowed(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Is_RighstHandlingAllowed(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }

            }

            return tempResult1;
        }

    }
}
