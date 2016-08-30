using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nixxis;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;
using System.Collections.ObjectModel;
using ContactRoute;

namespace Nixxis.Client.Admin
{
    [EnumDefaultValue(OverflowConditions.Always)]
    public enum OverflowConditions
    {
        None = 0,
        Always = 1,
        NoAgentLoggedIn = 2,
        ItemsInQueueThreshold = 3,
        RatioInQueueAgents = 4,
        MaxWait = 5,
        AgentsReadySmallerThan = 6,
        MaxEWT = 7
    }

    public class Queue : SecuredAdminObject
    {
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
        private static TranslationContext m_TranslationContext = new TranslationContext("Queue");

        private AdminObjectReference<SecurityContext> m_SecurityContext = null;
        private SingletonAdminObjectList<AdminObjectSecurityContext> m_SecurityContexts = null;

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        public Queue(AdminCore core)
            : base(core)
        {
            Init();
        }

        public Queue(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        internal override void Load(System.Xml.XmlElement node)
        {
            base.Load(node);

            if (OverflowPreprocessor.HasTarget && OverflowPreprocessor.Target.ConfigType != null)
            {
                OverflowPreprocessorConfig = (BasePreprocessorConfig)Core.Create(OverflowPreprocessor.Target.ConfigType, this, string.Concat("_", Id));
                OverflowPreprocessorConfig.DeserializeFromText(OverflowPreprocessorParams);
                OverflowPreprocessorConfig.saveToTextStorage = ((t) => { OverflowPreprocessorParams = t; });
                
            }
        }


        private void Init()
        {            
            MediaType = Admin.MediaType.All;
            Prompts = new AdminObjectList<PromptLink>(this);
            Time0 = 30;
            Time1 = 60;
            Time2 = 120;
            Time3 = 180;

            Coef0 = 50;
            Coef1 = 70;
            Coef2 = 90;
            Coef3 = 100;

            OverflowPreprocessor = new AdminObjectReference<Preprocessor>(this);
            OverflowMessage = new AdminObjectReference<Prompt>(this);
            OverflowReroutePrompt = new AdminObjectReference<Prompt>(this);
            OverflowActionType = OverflowActions.None;
            WaitResource = "queue-v2";

            SecurityContext = new AdminObjectReference<SecurityContext>(this);
            SecurityContexts = new SingletonAdminObjectList<AdminObjectSecurityContext>(this);
        }

        public override void Clear()
        {
            while (Prompts.Count > 0)
            {
                Core.Delete(Prompts[0].Prompt);                
            }

            base.Clear();
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

                if (Teams != null && SystemTeam != null)
                {
                    SystemTeam.FirePropertyChanged("ShortDisplayText");
                    SystemTeam.FirePropertyChanged("DisplayText");
                    SystemTeam.FirePropertyChanged("TypedDisplayText");
                }
            }
        }

        public string ShortCode
        {
            get
            {
                return GetFieldValue<string>("ShortCode");
            }
            set
            {
                SetFieldValue<string>("ShortCode", value);
            }
        }

        public bool HighPriority
        {
            get
            {
                return GetFieldValue<bool>("HighPriority");
            }
            set
            {
                SetFieldValue<bool>("HighPriority", value);
            }
        }

        public string WaitResource
        {
            get
            {
                return GetFieldValue<string>("WaitResource");
            }
            set
            {
                SetFieldValue<string>("WaitResource", value);
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
                SetFieldValue<string>("GroupKey", value);
            }
        }

        public bool ProfitEvaluatorActive
        {
            get
            {
                return GetFieldValue<bool>("ProfitEvaluatorActive");
            }
            set
            {
                SetFieldValue("ProfitEvaluatorActive", value);
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public MediaType MediaType
        {
            get
            {
                return GetFieldValue<MediaType>("MediaType");
            }
            set
            {
                SetFieldValue<MediaType>("MediaType", value);
            }
        }

        public float TimeCoef
        {
            get
            {
                return GetFieldValue<float>("TimeCoef");
            }
            set
            {
                SetFieldValue("TimeCoef", value);
            }

        }

        public float Time0
        {
            get
            {
                return GetFieldValue<float>("Time0");
            }
            set
            {
                SetFieldValue("Time0", value);
                FirePropertyChanged("TimeCoefHelper");
            }

        }

        public float Time1
        {
            get
            {
                return GetFieldValue<float>("Time1");
            }
            set
            {
                SetFieldValue("Time1", value);
                FirePropertyChanged("TimeCoefHelper");
            }

        }

        public float Time2
        {
            get
            {
                return GetFieldValue<float>("Time2");
            }
            set
            {
                SetFieldValue("Time2", value);
                FirePropertyChanged("TimeCoefHelper");
            }

        }

        public float Time3
        {
            get
            {
                return GetFieldValue<float>("Time3");
            }
            set
            {
                SetFieldValue("Time3", value);
                FirePropertyChanged("TimeCoefHelper");
            }

        }

        public float Coef0
        {
            get
            {
                return GetFieldValue<float>("Coef0");
            }
            set
            {
                SetFieldValue("Coef0", value);
                FirePropertyChanged("TimeCoefHelper");
            }

        }

        public float Coef1
        {
            get
            {
                return GetFieldValue<float>("Coef1");
            }
            set
            {
                SetFieldValue("Coef1", value);
                FirePropertyChanged("TimeCoefHelper");
            }
        }

        public float Coef2
        {
            get
            {
                return GetFieldValue<float>("Coef2");
            }
            set
            {
                SetFieldValue("Coef2", value);
                FirePropertyChanged("TimeCoefHelper");
            }
        }

        public float Coef3
        {
            get
            {
                return GetFieldValue<float>("Coef3");
            }
            set
            {
                SetFieldValue("Coef3", value);
                FirePropertyChanged("TimeCoefHelper");
            }
        }

        public float TimeCoefHelper
        {
            get
            {
                return 0;
            }
        }

        [AdminLoad(Path = "/Admin/QueuesTeams/QueueTeam[@queueid=\"{0}\"]")]
        public AdminObjectList<QueueTeam> Teams
        {
            get;
            internal set;
        }

        public Team SystemTeam
        {
            get
            {
                foreach (QueueTeam qt in Teams)
                {
                    if (qt.Team.IsSystemWithValidOwner)
                    {
                        return qt.Team;
                    }
                }
                return null;
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
                    return string.Concat(Translate("Queue "), Description);
            }
        }

        public object CheckedTeams
        {
            get
            {
                return new AdminCheckedLinkList<Team, QueueTeam>(m_Core.Teams, Teams, this);
            }
        }

        public Campaign SystemCampaign
        {
            get
            {
                return Owner as Campaign;
            }
        }

        #region ============================= Overflow =============================

        private AdminObjectReference<Prompt> m_OverflowMessage;

        private AdminObjectReference<Prompt> m_OverflowReroutePrompt;

        private AdminObjectReference<Preprocessor> m_OverflowPreprocessor;

        public OverflowActions OverflowActionType
        {
            get
            {
                return GetFieldValue<OverflowActions>("OverflowActionType");
            }
            set
            {
                SetFieldValue<OverflowActions>("OverflowActionType", value);
                FirePropertyChanged("OverflowActionDescription");
                FirePropertyChanged("OverflowSettings");
                OverflowParam = null;
            }
        }

        public string OverflowParam
        {
            get
            {
                return GetFieldValue<string>("OverflowParam");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = null;

                SetFieldValue<string>("OverflowParam", value);
                switch (OverflowActionType)
                {
                    case OverflowActions.Disconnect:
                        OverflowMessage.TargetId = null;
                        OverflowPreprocessor.TargetId = null;
                        OverflowRerouteDestination = null;
                        break;
                    case OverflowActions.IVR:
                        if (OverflowPreprocessor.TargetId != value)
                            OverflowPreprocessor.TargetId = value;
                        OverflowMessage.TargetId = null;
                        OverflowRerouteDestination = null;
                        break;
                    case OverflowActions.Message:
                        OverflowMessage.TargetId = value;
                        OverflowPreprocessor.TargetId = null;
                        OverflowRerouteDestination = null;
                        break;
                    case OverflowActions.None:
                        OverflowMessage.TargetId = null;
                        OverflowPreprocessor.TargetId = null;
                        OverflowRerouteDestination = null;
                        break;
                    case OverflowActions.Reroute:
                        OverflowRerouteDestination = value;
                        OverflowMessage.TargetId = null;
                        OverflowPreprocessor.TargetId = null;
                        break;
                }
            }
        }

        public string OverflowPreprocessorParams
        {
            get
            {
                return GetFieldValue<string>("OverflowPreprocessorParams");
            }
            set
            {
                SetFieldValue<string>("OverflowPreprocessorParams", value);
            }
        }

        public AdminObjectReference<Prompt> OverflowReroutePrompt
        {
            get
            {
                return m_OverflowReroutePrompt;
            }
            set
            {
                m_OverflowReroutePrompt = value;
            }
        }

        public BasePreprocessorConfig OverflowPreprocessorConfig
        {
            get;
            set;
        }

        public string OverflowActionDescription
        {
            get
            {
                string strDesc = (new OverflowActionsHelper()).Single((eh) => (eh.EnumValue == OverflowActionType)).Description;
                switch (OverflowActionType)
                {
                    case OverflowActions.Disconnect:
                    case OverflowActions.None:
                        return strDesc;
                    case OverflowActions.Message:
                        return string.Concat(strDesc, " ", OverflowMessage.HasTarget ? OverflowMessage.Target.DisplayText : "?");
                    case OverflowActions.IVR:
                        return string.Concat(strDesc, " ", OverflowPreprocessor.HasTarget ? OverflowPreprocessor.Target.DisplayText : "?");
                    case OverflowActions.Reroute:
                        return string.Concat(strDesc, " ", OverflowRerouteDestination);
                }

                return null;
            }
        }

        public string OverflowRerouteDestination
        {
            get
            {
                return OverflowParam;
            }
            set
            {
                if (OverflowActionType == OverflowActions.Reroute && OverflowParam != value)
                {
                    OverflowParam = value;
                }
                FirePropertyChanged("OverflowRerouteDestination");
                FirePropertyChanged("OverflowActionDescription");
                FirePropertyChanged("OverflowSettings");
            }
        }

        public string OverflowMessageString
        {
            get
            {
                return OverflowParam;
            }
            set
            {
                if (OverflowActionType == OverflowActions.Message && OverflowParam != value)
                {
                    OverflowParam = value;
                }
                FirePropertyChanged("OverflowMessage");
                FirePropertyChanged("OverflowPreprocessor");
                FirePropertyChanged("OverflowRerouteDestination");
                FirePropertyChanged("OverflowActionDescription");
                FirePropertyChanged("OverflowSettings");
            }
        }

        public string OverflowPreprocessorString
        {
            get
            {
                return OverflowParam;
            }
            set
            {
                if (OverflowActionType == OverflowActions.IVR && OverflowParam != value)
                {
                    OverflowParam = value;
                }
                FirePropertyChanged("OverflowMessage");
                FirePropertyChanged("OverflowPreprocessor");
                FirePropertyChanged("OverflowRerouteDestination");
                FirePropertyChanged("OverflowActionDescription");
                FirePropertyChanged("OverflowSettings");
            }
        }

        [AdminSave(SkipSave = true)]
        public AdminObjectReference<Prompt> OverflowMessage
        {
            get
            {
                return m_OverflowMessage;
            }
            set
            {
                if (m_OverflowMessage != null)
                {
                    m_OverflowMessage.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_OverflowMessage_PropertyChanged);
                    m_OverflowMessage.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_OverflowMessage_PropertyChanged);
                }

                m_OverflowMessage = value;

                if (m_OverflowMessage != null)
                {
                    m_OverflowMessage.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_OverflowMessage_PropertyChanged);
                    m_OverflowMessage.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_OverflowMessage_PropertyChanged);
                }
            }
        }

        [AdminSave(SkipSave = true)]
        public AdminObjectReference<Preprocessor> OverflowPreprocessor
        {
            get
            {
                return m_OverflowPreprocessor;
            }
            internal set
            {
                if (m_OverflowPreprocessor != null)
                {
                    m_OverflowPreprocessor.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_OverflowPreprocessor_PropertyChanged);
                    m_OverflowPreprocessor.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_OverflowPreprocessor_PropertyChanged);
                }

                m_OverflowPreprocessor = value;

                if (m_OverflowPreprocessor != null)
                {
                    m_OverflowPreprocessor.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_OverflowPreprocessor_PropertyChanged);
                    m_OverflowPreprocessor.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_OverflowPreprocessor_PropertyChanged);
                }
            }
        }

        void m_OverflowMessage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OverflowMessageString = OverflowMessage.TargetId;
        }

        void m_OverflowPreprocessor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OverflowPreprocessorString = OverflowPreprocessor.TargetId;
        }
        #endregion


        public OverflowConditions OverflowCondition
        {
            get
            {
                return GetFieldValue<OverflowConditions>("OverflowCondition");
            }
            set
            {
                SetFieldValue<OverflowConditions>("OverflowCondition", value);
                FirePropertyChanged("OverflowConditionDescription");
                FirePropertyChanged("OverflowSettings");
            }
        }

        public string OverflowConditionDescription
        {
            get
            {
                string strDesc = (new OverflowConditionsHelper()).Single((eh) => (eh.EnumValue == OverflowCondition)).Description;
                switch (OverflowCondition)
                {
                    case OverflowConditions.Always:
                    case OverflowConditions.None:
                    case OverflowConditions.NoAgentLoggedIn:
                        return strDesc;
                    case OverflowConditions.ItemsInQueueThreshold:
                        return string.Concat(strDesc, " ", OverflowConditionItemsInQueueThreshold);
                    case OverflowConditions.MaxWait:
                        return String.Concat(strDesc, " ", DurationHelpers.GetDefaultDurationString(OverflowConditionMaxWait, false));
                    case OverflowConditions.RatioInQueueAgents:
                        return String.Format("{0} {1:0.00}", strDesc, OverflowConditionRatioInQueueAgents);
                    case OverflowConditions.AgentsReadySmallerThan:
                        return string.Concat(strDesc, " ", OverflowConditionAgentsReadySmallerThanThreshold);
                    case OverflowConditions.MaxEWT:
                        return String.Concat(strDesc, " ", DurationHelpers.GetDefaultDurationString(OverflowConditionMaxWait, false));

                }
                return null;
            }
        }

        public string OverflowConditionParam
        {
            get
            {
                return GetFieldValue<string>("OverflowConditionParam");
            }
            set
            {
                SetFieldValue<string>("OverflowConditionParam", value);
            }
        }

        public int OverflowConditionItemsInQueueThreshold
        {
            get
            {
                if (string.IsNullOrEmpty(OverflowConditionParam))
                    return 0;
                return (int) (XmlConvert.ToDouble(OverflowConditionParam));
            }
            set
            {
                if (OverflowCondition == OverflowConditions.ItemsInQueueThreshold && OverflowParam != value.ToString())
                {
                    OverflowConditionParam = XmlConvert.ToString((double)value);
                }
                FirePropertyChanged("OverflowConditionItemsInQueueThreshold");
                FirePropertyChanged("OverflowSettings");
            }
        }

        public int OverflowConditionAgentsReadySmallerThanThreshold
        {
            get
            {
                if (string.IsNullOrEmpty(OverflowConditionParam))
                    return 1;
                int temp = (int)(XmlConvert.ToDouble(OverflowConditionParam));
                if (temp < 1)
                {
                    return 1;
                }
                return temp;
            }
            set
            {
                if (OverflowCondition == OverflowConditions.AgentsReadySmallerThan && OverflowParam != value.ToString())
                {
                    OverflowConditionParam = XmlConvert.ToString((double)value);
                }
                FirePropertyChanged("OverflowConditionAgentsReadySmallerThanThreshold");
                FirePropertyChanged("OverflowSettings");
            }
        }
        public double OverflowConditionRatioInQueueAgents
        {
            get
            {
                if (string.IsNullOrEmpty(OverflowConditionParam))
                    return 0.0;

                return XmlConvert.ToDouble(OverflowConditionParam) / 100.0;
            }
            set
            {
                if (OverflowCondition == OverflowConditions.RatioInQueueAgents && OverflowParam != ((int)(value * 100)).ToString())
                {
                    OverflowConditionParam = XmlConvert.ToString((double)((int)(value * 100)));
                }
                FirePropertyChanged("OverflowConditionRatioInQueueAgents");
                FirePropertyChanged("OverflowSettings");
            }
        }

        public int OverflowConditionMaxWait
        {
            get
            {
                if (string.IsNullOrEmpty(OverflowConditionParam))
                    return 0;

                return (int) (XmlConvert.ToDouble(OverflowConditionParam));
            }
            set
            {
                if (OverflowCondition == OverflowConditions.MaxWait && OverflowParam != value.ToString())
                {
                    OverflowConditionParam = XmlConvert.ToString((double)value);
                }
                FirePropertyChanged("OverflowConditionMaxWait");
                FirePropertyChanged("OverflowSettings");

            }
        }
        public int OverflowConditionMaxEWT
        {
            get
            {
                if (string.IsNullOrEmpty(OverflowConditionParam))
                    return 0;

                return (int)(XmlConvert.ToDouble(OverflowConditionParam));
            }
            set
            {
                if (OverflowCondition == OverflowConditions.MaxEWT && OverflowParam != value.ToString())
                {
                    OverflowConditionParam = XmlConvert.ToString((double)value);
                }
                FirePropertyChanged("OverflowConditionMaxEWT");
                FirePropertyChanged("OverflowSettings");

            }
        }


        public string OverflowSettings
        {
            get
            {
                return string.Format(Translate("Condition: {0}; Action: {1}"), OverflowConditionDescription, OverflowActionDescription);
            }
        }

        private AdminObjectList<PromptLink> m_Prompts;

        [AdminLoad(Path = "/Admin/PromptsLinks/PromptLink[@adminobjectid=\"{0}\"]")]
        public AdminObjectList<PromptLink> Prompts
        {
            get
            {
                return m_Prompts;
            }
            internal set
            {
                if (m_Prompts != null)
                {
                    m_Prompts.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_Prompts_Collection_Changed);
                }

                m_Prompts = value;

                if (m_Prompts != null)
                {
                    m_Prompts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_Prompts_Collection_Changed);
                }
                FirePropertyChanged("AllPrompts");
            }
        }

        private void m_Prompts_Collection_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            FirePropertyChanged("AllPrompts");
        }

        public IEnumerable<PromptLink> AllPrompts
        {
            get
            {
                return Prompts.Union(Core.GlobalPrompts.Prompts).OrderBy( (p) => (p.Prompt.DisplayText) );
            }
        }

        public IEnumerable<Preprocessor> Preprocessors
        {
            get
            {
                return Core.Preprocessors.Where((a) => (a.MediaType == Admin.MediaType.Voice));
            }
        }

        public Queue Duplicate()
        {
            Queue newQueue = AdminCore.Create<Queue>();

            newQueue.Description = string.Concat(Description, "*");
            newQueue.Coef0 = Coef0;
            newQueue.Coef1 = Coef1;
            newQueue.Coef2 = Coef2;
            newQueue.Coef3 = Coef3;

            newQueue.GroupKey = GroupKey;
            newQueue.HighPriority = HighPriority;

            newQueue.MediaType = MediaType;
            newQueue.OverflowActionType = OverflowActionType;
            newQueue.OverflowCondition = OverflowCondition;
            newQueue.OverflowConditionParam = OverflowConditionParam;
            newQueue.OverflowReroutePrompt.TargetId = OverflowReroutePrompt.TargetId;
            newQueue.OverflowParam = OverflowParam;
            newQueue.OverflowPreprocessorParams = OverflowPreprocessorParams;
            newQueue.ShortCode = ShortCode;

            newQueue.Time0 = Time0;
            newQueue.Time1 = Time1;
            newQueue.Time2 = Time2;
            newQueue.Time3 = Time3;

            newQueue.TimeCoef = TimeCoef;

            newQueue.WaitResource = WaitResource;

            foreach (QueueTeam qt in Teams)
            {
                newQueue.Teams.Add(qt.Team);
                newQueue.Teams[newQueue.Teams.Count - 1].MinWaitTime = qt.MinWaitTime;
                newQueue.Teams[newQueue.Teams.Count - 1].MaxWaitTime = qt.MaxWaitTime;
                newQueue.Teams[newQueue.Teams.Count - 1].BaseLevel = qt.BaseLevel;
            }

            while (Prompts.Count > 0)
            {
                PromptLink pl = Prompts[0];
                Queue queue = ((Queue)(pl.Link));
                pl.Prompt.Description = string.Concat(pl.Prompt.Description, "*");
                queue.Core.GlobalPrompts.Prompts.Add(pl.Prompt);
                queue.Prompts.Remove(pl.Prompt);
            }

           

            newQueue.SecurityContext.TargetId = SecurityContext.TargetId;

            newQueue.ProfitEvaluatorActive = ProfitEvaluatorActive;

            newQueue.AdminCore.Queues.Add(newQueue);

            DuplicateSecurity(newQueue);

            return newQueue;
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
            if( tempResult1==null || tempResult1.Value )
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
