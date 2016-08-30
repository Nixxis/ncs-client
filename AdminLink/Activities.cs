using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Xml;
using System.Configuration;
using Nixxis;
using ContactRoute;

namespace Nixxis.Client.Admin
{
    public enum AbandonRateMode
    {
        Standard = 0,
        AnsweringmachineIncluded = 1,
        EveryDialedIncluded = 2
    }

    public enum Frequency
    {
        Never = 0,
        Once = 1,
        WhenChanged = 2,
        Continuously = 3
    }

    public enum DialingMode
    {
        CallbacksOnly = 1,
        Preview = 2,
        Progressive = 3,
        Power = 4,
        Predictive = 5,
        Fixed = 6,
        RestrictedPower = 7,
        Search = 8
    }

    [Flags]
    public enum MediaType
    {
        Hidden = -1,
        None = 0,
        Voice = 1,
        Chat = 2,
        Mail = 4,
        Custom1 = 8,
        Custom2 = 16,
        All = 31
    }

    [EnumDefaultValue(OverflowActions.Message)]
    public enum OverflowActions
    {
        None = 0,
        Disconnect = 1,
        Message = 2,
        IVR = 3,
        Reroute = 4
    }

    [EnumDefaultValue(OutboundClosingAction.PauseActivity)]
    public enum OutboundClosingAction
    {
        None = 0,
        PauseActivity = 1,
        ChangeDialingMode = 2
    }

    public enum SalesforceCampaignMode
    {
        CampaignMembers,
        Activities
    }



    public abstract class Activity : SecuredAdminObject
    {
        internal override bool Reload(XmlNode node)
        {
            return base.Reload(node);
        }

        private static TranslationContext m_TranslationContext = new TranslationContext("Activities");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        private AdminObjectReference<Planning> m_Planning;
        private AdminObjectList<QualificationExclusion> m_QualificationsExclusions;


        public Activity(AdminCore core)
            : base(core)
        {
            Init();
        }

        public Activity(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private void Init()
        {
            WaitMusicProcessor = new AdminObjectReference<Admin.Preprocessor>(this);
            Script = new AdminObjectReference<Admin.Preprocessor>(this);
            Prompts = new AdminObjectList<PromptLink>(this);
            QualificationsExclusions = new AdminObjectList<QualificationExclusion>(this);
            Description = string.Format(Translate( "New activity {0}"), DateTime.Now);
            
            AutomaticRecording = false;
            AutomaticRecording = null;
            
            ListenAllowed = true;
            WaitResource = "queue-v2";
            Lines = -1;
            Planning = new AdminObjectReference<Planning>(this);
        }

        public void SetDescriptionLoaded()
        {
            SetFieldLoaded("Description");
        }

        internal override void Load(XmlElement node)
        {
            base.Load(node);

            if (Preprocessor.HasTarget && Preprocessor.Target.ConfigType != null)
            {
                PreprocessorConfiguration = (BasePreprocessorConfig)Core.Create(Preprocessor.Target.ConfigType, this, string.Concat("_", Id));
                PreprocessorConfiguration.DeserializeFromText(PreprocessorParams);
                PreprocessorConfiguration.saveToTextStorage = ( (t) => { PreprocessorParams = t; } );
            }

            if (WaitMusicProcessor.HasTarget && WaitMusicProcessor.Target.ConfigType != null)
            {
                WaitMusicProcessorConfiguration = (BasePreprocessorConfig)Core.Create(WaitMusicProcessor.Target.ConfigType, this, string.Concat("_", Id));
                WaitMusicProcessorConfiguration.DeserializeFromText(MusicPrompt);
                WaitMusicProcessorConfiguration.saveToTextStorage = ((t) => { MusicPrompt = t; });
            }

            if (Postprocessor.HasTarget && Postprocessor.Target.ConfigType != null)
            {
                PostprocessorConfiguration = (BasePreprocessorConfig)Core.Create(Postprocessor.Target.ConfigType, this, string.Concat("p", Id));
                PostprocessorConfiguration.DeserializeFromText(PostprocessorParams);
                PostprocessorConfiguration.saveToTextStorage = ((t) => { PostprocessorParams = t; });
            }

        }

        public override void Clear()
        {
            while (Prompts.Count > 0)
            {
                Core.Delete(Prompts[0].Prompt);
            }
            base.Clear();
        }

        protected override XmlElement CreateSaveNode(XmlDocument doc, string operation)
        {
            try
            {
                if (Owner != null)
                    Owner.EmptySave(doc);
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
                    Owner.EmptySave(doc);
            }
            catch
            {
            }

            base.EmptySave(doc);
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

        public AdminObjectReference<Campaign> Campaign
        {
            get;
            internal set;
        }

        public AdminObjectReference<Queue> Queue
        {
            get;
            internal set;
        }

        public AdminObjectReference<Planning> Planning
        {
            get
            {
                return m_Planning;
            }
            internal set
            {
                if (m_Planning!=null )
                {
                    m_Planning.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(PlanningPropertyChanged);
                    m_Planning.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(PlanningPropertyChanged);
                }

                m_Planning = value;

                if (m_Planning != null)
                {
                    m_Planning.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PlanningPropertyChanged);
                    m_Planning.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PlanningPropertyChanged);
                }
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
                SetFieldValue("MediaType", value);
                FirePropertyChanged("DisplayText");
            }
        }


        virtual protected void PlanningPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FirePropertyChanged("Planning");
            FirePropertyChanged("HasPlanning");
        }

        private bool m_HasPlanning = false;

        public bool HasPlanning
        {
            get
            {
                return Planning.HasTarget || m_HasPlanning;
            }
            set
            {
                m_HasPlanning = value;
                if (!m_HasPlanning)
                    Planning.TargetId = null;
            }
        }

        public AdminObjectReference<Preprocessor> Preprocessor
        {
            get;
            internal set;
        }


        private AdminObjectReference<Preprocessor> m_WaitMusicProcessor;

        public AdminObjectReference<Preprocessor> WaitMusicProcessor
        {
            get
            {
                return m_WaitMusicProcessor;
            }
            internal set
            {
                if (m_WaitMusicProcessor != null)
                {
                    m_WaitMusicProcessor.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_WaitMusicProcessor_TargetPropertyChanged);
                    m_WaitMusicProcessor.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_WaitMusicProcessor_TargetPropertyChanged);
                }

                m_WaitMusicProcessor = value;

                if (m_WaitMusicProcessor != null)
                {
                    m_WaitMusicProcessor.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_WaitMusicProcessor_TargetPropertyChanged);
                    m_WaitMusicProcessor.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_WaitMusicProcessor_TargetPropertyChanged);
                }
            }
        }

        void m_WaitMusicProcessor_TargetPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FirePropertyChanged("WaitLoopDescription");
            FirePropertyChanged("WaitMusicDescription");
        }

        
        public string PreprocessorParams
        {
            get
            {
                return GetFieldValue<string>("PreprocessorParams");
            }
            set
            {
                SetFieldValue<string>("PreprocessorParams", value);
            }
        }

        public BasePreprocessorConfig PreprocessorConfiguration
        {
            get;
            set;
        }

        public BasePreprocessorConfig WaitMusicProcessorConfiguration
        {
            get;
            set;
        }


        public AdminObjectReference<Preprocessor> Postprocessor
        {
            get;
            internal set;
        }

        public string PostprocessorParams
        {
            get
            {
                return GetFieldValue<string>("PostprocessorParams");
            }
            set
            {
                SetFieldValue<string>("PostprocessorParams", value);
            }
        }

        public BasePreprocessorConfig PostprocessorConfiguration
        {
            get;
            set;
        }

        private AdminObjectReference<Preprocessor> m_Script;

        public AdminObjectReference<Preprocessor> Script
        {
            get
            {
                return m_Script;
            }
            internal set
            {
                if (m_Script != null)
                {
                    m_Script.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Script_TargetPropertyChanged);
                    m_Script.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Script_TargetPropertyChanged);
                }

                m_Script = value;

                if (m_Script != null)
                {
                    m_Script.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Script_TargetPropertyChanged);
                    m_Script.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Script_TargetPropertyChanged);
                }
            }
        }

        void m_Script_TargetPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }

        public string ScriptUrl
        {
            get
            {
                return GetFieldValue<string>("ScriptUrl");
            }
            set
            {
                SetFieldValue<string>("ScriptUrl", value);
            }
        }

        public bool? AutomaticRecording
        {
            get
            {
                return (bool?)GetFieldValue<bool?>("AutomaticRecording");
            }
            set
            {
                SetFieldValue<bool?>("AutomaticRecording", value);
                FirePropertyChanged("AutomaticRecordingDescription");

                if (Campaign!=null && Campaign.HasTarget)
                {
                    try
                    {
                        Campaign.Target.FirePropertyChanged("AutomaticRecordingDescription");
                    }
                    catch
                    {

                    }
                }
            }
        }

        public string AutomaticRecordingDescription
        {
            get
            {
                if (!AutomaticRecording.HasValue)
                {
                    if (Campaign.HasTarget)
                    {
                        if(!Campaign.Target.AutomaticRecording.HasValue)
                        {
                            if(m_Core.Settings[0].AutomaticRecording.GetValueOrDefault())
                                return Translate("Conversations are recorded (global settings)");
                            else
                                return Translate("Conversations are not recorded (global settings)");
                        }
                        else
                        {
                            if(Campaign.Target.AutomaticRecording.Value)
                                return Translate("Conversations are recorded (inherited from campaign)");
                            else
                                return Translate("Conversations are not recorded (inherited from campaign)");

                        }
                    }
                    return "No campaign found";
                }
                else
                {
                    if (AutomaticRecording.Value)
                        return Translate("All activity conversations are recorded");
                    else
                        return Translate("All activity conversations are not recorded");
                }
            }

        }

        public bool ListenAllowed
        {
            get
            {
                return (bool)GetFieldValue<bool>("ListenAllowed");
            }
            set
            {
                SetFieldValue<bool>("ListenAllowed", value);
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

        [AdminLoad(Path = "/Admin/QualificationsExclusions/QualificationExclusion[@activityid=\"{0}\"]")]
        public AdminObjectList<QualificationExclusion> QualificationsExclusions
        {
            get
            {
                return m_QualificationsExclusions;
            }
            internal set
            {
                if(m_QualificationsExclusions!=null)
                    m_QualificationsExclusions.CollectionChanged -= new NotifyCollectionChangedEventHandler(m_QualificationsExclusions_CollectionChanged);
            
                m_QualificationsExclusions = value;

                if (m_QualificationsExclusions != null)
                    m_QualificationsExclusions.CollectionChanged += new NotifyCollectionChangedEventHandler(m_QualificationsExclusions_CollectionChanged);
            }
        }

        void m_QualificationsExclusions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (QualificationsExclusions.HasBeenLoaded)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (QualificationExclusion qe in e.NewItems)
                    {
                        qe.Qualification.Exclude(this);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (QualificationExclusion qe in e.OldItems)
                    {
                        qe.Qualification.Include(this);
                    }
                }
                else
                {
                    // We know this one has been called due to workaround for datagrid bug -> do the same as for Add...
                    for (int i = 0; i < QualificationsExclusions.Count; i++)
                        QualificationsExclusions[i].Qualification.Exclude(this);
                }
            }
        }

        public object CheckedQualificationsExclusions
        {
            get
            {
                AdminCheckedLinkList<Qualification, QualificationExclusion> retValue = new AdminCheckedLinkList<Qualification, QualificationExclusion>(Campaign.Target.Qualifications, QualificationsExclusions, this, "ParentId");
                if (retValue.Count > 0)
                    return retValue.Single((cl) => string.IsNullOrEmpty( cl.Item.ParentId) ).Children;
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
                    return string.Concat("Activity ", Description);
            }
        }

        public string ContextualDisplayText
        {
            get
            {
                return string.Format("{0} - {1}", Campaign.Target.DisplayText, DisplayText);
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

        public int RecordingPlaybackLevel
        {
            get
            {
                return GetFieldValue<int>("RecordingPlaybackLevel");
            }
            set
            {
                SetFieldValue<int>("RecordingPlaybackLevel", value);
            }
        }


        public int DisableManualQualification
        {
            get
            {
                return GetFieldValue<int>("DisableManualQualification");
            }
            set
            {
                SetFieldValue<int>("DisableManualQualification", value);
            }
        }


        public bool QualificationRequired
        {
            get
            {
                return GetFieldValue<bool>("QualificationRequired");
            }
            set
            {
                SetFieldValue<bool>("QualificationRequired", value);
            }
        }

        public int Lines
        {
            get
            {
                return GetFieldValue<int>("Lines");
            }
            set
            {
                SetFieldValue<int>("Lines", value);
                FirePropertyChanged("LinesAreRestricted");
                FirePropertyChanged("LinesRestrictionDescription");
            }
        }

        public bool LinesAreRestricted
        {
            get
            {
                return Lines != -1;
            }
            set
            {
                if (value)
                    Lines = 0;
                else
                    Lines = -1;
            }
        }

        public string LinesRestrictionDescription
        {
            get
            {
                if (!LinesAreRestricted)
                    return Translate("No restriction");
                else
                    return string.Format(Translate("Usage is restricted to {0} lines"), Lines);

            }
        }

        [AdminLoad(Path = "/Admin/ActivitiesSkills/ActivitySkill[@activityid=\"{0}\"]")]
        public AdminObjectList<ActivitySkill> Skills
        {
            get;
            internal set;
        }

        [AdminLoad(Path = "/Admin/ActivitiesLanguages/ActivityLanguage[@activityid=\"{0}\"]")]
        public AdminObjectList<ActivityLanguage> Languages
        {
            get;
            internal set;
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
                FirePropertyChanged("MusicPrompts");
            }
        }

        private void m_Prompts_Collection_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            FirePropertyChanged("AllPrompts");
            FirePropertyChanged("MusicPrompts");
        }

        public IEnumerable<PromptLink> AllPrompts
        {
            get
            {
                if(Campaign!=null && Campaign.HasTarget)
                    return Prompts.Union(Campaign.Target.Prompts).Union(Core.GlobalPrompts.Prompts).OrderBy( (p) => (p.Prompt.DisplayText) ) ;
                return Prompts.Union(Core.GlobalPrompts.Prompts).OrderBy((p) => (p.Prompt.DisplayText));
            }
        }

        public string MusicPrompt
        {
            get
            {
                return GetFieldValue<string>("MusicPrompt");
            }
            set
            {
                SetFieldValue<string>("MusicPrompt", value);
                FirePropertyChanged("WaitLoopDescription");
                FirePropertyChanged("WaitMusicDescription");
            }
        }

        public object CheckedSkills
        {
            get
            {
                return new AdminCheckedLinkList<Skill, ActivitySkill>(m_Core.Skills, Skills, this);
            }
        }

        public object CheckedLanguages
        {
            get
            {
                return new AdminCheckedLinkList<Language, ActivityLanguage>(m_Core.Languages, Languages, this);
            }
        }

        abstract public bool Paused { get; set; }

        public void CopyProperties(Activity src, bool attachToCampaign)
        {
            AutomaticRecording = src.AutomaticRecording;
            if(attachToCampaign)
                Campaign.TargetId = src.Campaign.TargetId;
            Description = string.Concat(src.Description, "*");
            DisableManualQualification = src.DisableManualQualification;

            foreach (ActivityLanguage al in src.Languages)
            {
                Languages.Add(al.Language);
                Languages[Languages.Count - 1].Level = al.Level;
                Languages[Languages.Count - 1].MinimumLevel = al.MinimumLevel;
            }

            Lines = src.Lines;
            ListenAllowed = src.ListenAllowed;
            MediaType = src.MediaType;
            Paused = src.Paused;
            Planning.TargetId = src.Planning.TargetId;

            Postprocessor.TargetId = src.Postprocessor.TargetId;
            PostprocessorParams = src.PostprocessorParams;
            if (Postprocessor.HasTarget && Postprocessor.Target != null && Postprocessor.Target.ConfigType!=null)
            {
                PostprocessorConfiguration = (BasePreprocessorConfig)Core.Create(Postprocessor.Target.ConfigType, this, string.Concat("_", Id));
                PostprocessorConfiguration.DeserializeFromText(PostprocessorParams);
                PostprocessorConfiguration.saveToTextStorage = ((t) => { PostprocessorParams = t; });
            }


            Preprocessor.TargetId = src.Preprocessor.TargetId;
            PreprocessorParams = src.PreprocessorParams;
            if (Preprocessor.HasTarget && Preprocessor.Target != null && Preprocessor.Target.ConfigType!=null)
            {
                PreprocessorConfiguration = (BasePreprocessorConfig)Core.Create(Preprocessor.Target.ConfigType, this, string.Concat("_", Id));
                PreprocessorConfiguration.DeserializeFromText(PreprocessorParams);
                PreprocessorConfiguration.saveToTextStorage = ((t) => { PreprocessorParams = t; });
            }


            WaitMusicProcessor.TargetId = src.WaitMusicProcessor.TargetId;
            MusicPrompt = src.MusicPrompt;
            if (WaitMusicProcessor.HasTarget && WaitMusicProcessor.Target != null && WaitMusicProcessor.Target.ConfigType != null)
            {
                WaitMusicProcessorConfiguration = (BasePreprocessorConfig)Core.Create(WaitMusicProcessor.Target.ConfigType, this, string.Concat("_", Id));
                WaitMusicProcessorConfiguration.DeserializeFromText(PreprocessorParams);
                WaitMusicProcessorConfiguration.saveToTextStorage = ((t) => { PreprocessorParams = t; });
            }



            foreach (PromptLink pl in src.Prompts)
            {
                Prompts.Add(pl.Prompt);
            }

            QualificationRequired = src.QualificationRequired;

            if (attachToCampaign)
            {
                foreach (QualificationExclusion qe in src.QualificationsExclusions)
                {
                    QualificationsExclusions.Add(qe.Qualification);
                }
            }

            Queue.TargetId = src.Queue.TargetId;
            RecordingPlaybackLevel = src.RecordingPlaybackLevel;
            Script.TargetId = src.Script.TargetId;
            ScriptUrl = src.ScriptUrl;

            foreach (ActivitySkill ask in src.Skills)
            {
                Skills.Add(ask.Skill);
                Skills[Skills.Count - 1].Level = ask.Level;
                Skills[Skills.Count - 1].MinimumLevel = ask.MinimumLevel;
            }

            WaitResource = src.WaitResource;
            WrapupExtendable = src.WrapupExtendable;
            WrapupTime = src.WrapupTime;

            PostWrapupOptionsValue = src.PostWrapupOptionsValue;
            AutoReadyDelay = src.AutoReadyDelay;


        }

        public IMediaProviderConfigurator ProviderConfig
        {
            get
            {
                try
                {
                    string strProviderId = ProviderConfigSettings.SelectSingleNode("Root/Config").Attributes["providerid"].Value;
                    MediaProvider mediaProvider = null;
                    foreach(MediaProvider mp in m_Core.MediaProviders)
                    {
                        if(mp.Id.Equals(strProviderId))
                        {
                            mediaProvider = mp;
                            break;
                        }
                    }
                    if(mediaProvider!=null)
                    {
                        IMediaProviderConfigurator prov = Activator.CreateInstance(Type.GetType(mediaProvider.PluginType)) as IMediaProviderConfigurator;
                        prov.InitializeProvider(m_Core, mediaProvider); 
                        prov.Config = ProviderConfigSettings;
                        return prov;
                    }
                }
                catch
                {
                }
                return null;
            }
        }

        public XmlDocument ProviderConfigSettings
        {
            get
            {
                return GetFieldValue<XmlDocument>("ProviderConfigSettings");
            }
            set
            {
                SetFieldValue<XmlDocument>("ProviderConfigSettings", value);

            }
        }

    }

    public class InboundActivity : Activity
    {
        private string m_LastNumberingPlanEntryDisplayPrefix = null;

        public void SetNoAgentClosedActionDeleted()
        {
            NoAgentActionType = OverflowActions.None;
            NoAgentParam = null;
            SetFieldLoaded("NoAgentActionType");
            SetFieldLoaded("NoAgentParam");
        }

        public void ClosedActionDeleted()
        {
            ClosedActionType = OverflowActions.None;
            ClosedParam = null;
            Planning.TargetId = null;
            Planning.DoneLoading();
            SetFieldLoaded("ClosedActionType");
            SetFieldLoaded("ClosedParam");
        }

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
                if (Campaign!=null && Campaign.HasTarget)
                {
                    if (!Campaign.Target.InboundActivities.Contains(this))
                        Campaign.Target.InboundActivities.Add(this);
                }
                return true;
            }
            return false;
        }



        public override void Clear()
        {
            if (MediaType == Admin.MediaType.Voice && PredefinedDestination.HasTarget)
            {
                PredefinedDestination.Target.Activity.TargetId = null;

            }
            
            base.Clear();
        }


        private AdminObjectReference<Prompt> m_OverflowPrompt;
        private AdminObjectReference<NumberingPlanEntry> m_PredefinedDestination;
        private AdminObjectReference<SecurityContext> m_SecurityContext = null;
        private SingletonAdminObjectList<AdminObjectSecurityContext> m_SecurityContexts = null;


        public InboundActivity(AdminCore core)
            : base(core)
        {
            Init();
        }

        public InboundActivity(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        internal override void Load(XmlElement node)
        {
            base.Load(node);

            if (OverflowPreprocessor.HasTarget && OverflowPreprocessor.Target.ConfigType != null)
            {
                OverflowPreprocessorConfig = (BasePreprocessorConfig)Core.Create(OverflowPreprocessor.Target.ConfigType, this, string.Concat("o", Id));
                OverflowPreprocessorConfig.DeserializeFromText(OverflowPreprocessorParams);
                OverflowPreprocessorConfig.saveToTextStorage = ((t) => { OverflowPreprocessorParams = t; });
            }

            if (ClosedPreprocessor.HasTarget && ClosedPreprocessor.Target.ConfigType != null)
            {
                ClosedPreprocessorConfig = (BasePreprocessorConfig)Core.Create(ClosedPreprocessor.Target.ConfigType, this, string.Concat("c", Id));
                ClosedPreprocessorConfig.DeserializeFromText(ClosedPreprocessorParams);
                ClosedPreprocessorConfig.saveToTextStorage = ((t) => { ClosedPreprocessorParams = t; });
            }

            if (NoAgentPreprocessor.HasTarget && NoAgentPreprocessor.Target.ConfigType != null)
            {
                NoAgentPreprocessorConfig = (BasePreprocessorConfig)Core.Create(NoAgentPreprocessor.Target.ConfigType, this, string.Concat("n", Id));
                NoAgentPreprocessorConfig.DeserializeFromText(NoAgentPreprocessorParams);
                NoAgentPreprocessorConfig.saveToTextStorage = ((t) => { NoAgentPreprocessorParams = t; });

            }

        }

        private void Init()
        {
            DynamicParameters = new AdminObjectList<DynamicParameterLink>(this);
            PredefinedDestination = new AdminObjectReference<NumberingPlanEntry>(this);
            OverflowPrompt = new AdminObjectReference<Prompt>(this);
            CallbackActivity = new AdminObjectReference<OutboundActivity>(this);
            AbandonsCallbackActivity = new AdminObjectReference<OutboundActivity>(this);

            OverflowMessage = new AdminObjectReference<Prompt>(this);
            OverflowReroutePrompt = new AdminObjectReference<Prompt>(this);
            NoAgentMessage = new AdminObjectReference<Prompt>(this);
            NoAgentReroutePrompt = new AdminObjectReference<Prompt>(this);
            ClosedMessage = new AdminObjectReference<Prompt>(this);
            ClosedReroutePrompt = new AdminObjectReference<Prompt>(this);

            OverflowPreprocessor = new AdminObjectReference<Preprocessor>(this);
            ClosedPreprocessor = new AdminObjectReference<Preprocessor>(this);
            NoAgentPreprocessor = new AdminObjectReference<Preprocessor>(this);

            TransmitEWT = Frequency.Never;
            TransmitPosition = Frequency.Never;
            WaitMusicLength = 30;
            Paused = false;

            ClosedActionType = OverflowActions.None;
            NoAgentActionType = OverflowActions.None;
            OverflowActionType = OverflowActions.None;

            ClosedParam = null;
            NoAgentParam = null;
            OverflowParam = null;

            UsePreferredAgent = false;
            PreferredAgentQueueTime = 60;
            PreferredAgentValidity = 60;

            SlaTime = 60;
            SlaMode = 0;
            SlaIncludeOverflow = true;
            SlaPercentageToHandle = 90;
            SlaPercentageHandledInTime = 90;

            SecurityContext = new AdminObjectReference<SecurityContext>(this);
            SecurityContexts = new SingletonAdminObjectList<AdminObjectSecurityContext>(this);

        }


        public string PreparedDestination
        {
            get
            {
                if (this.MediaType == Admin.MediaType.Voice)
                {
                    if (PredefinedDestination.HasTarget)
                        return PredefinedDestination.Target.DisplayText;
                    else
                        return null;
                }
                else
                    return Destination;
            }
        }

        public string Destination
        {
            get
            {
                return GetFieldValue<string>("Destination");
            }
            set
            {
                SetFieldValue("Destination", value);
                if (this.MediaType == Admin.MediaType.Voice)
                {
                    if (PredefinedDestination.TargetId != value )
                        if( PredefinedDestination.TargetId!=null || value!=string.Empty )
                            PredefinedDestination.TargetId = value;
                }
                FirePropertyChanged("PreparedDestination");
            }
        }

        [AdminSave(SkipSave=true)]
        public string TempDestination
        {
            get
            {
                if (PredefinedDestination.HasTarget)
                    return PredefinedDestination.Target.DisplayText;
                else
                    return new NullAdminObject().DisplayText;
            }
            set
            {
                if (this.MediaType == Admin.MediaType.Voice)
                {
                    if (string.IsNullOrEmpty(value))
                        return;

                    if (PredefinedDestination.HasTarget && value.Equals(PredefinedDestination.Target.DisplayText))
                        return;

                    if (!PredefinedDestination.HasTarget && value.Equals(new NullAdminObject().DisplayText))
                        return;

                    if (value.StartsWith(m_LastNumberingPlanEntryDisplayPrefix))
                    {
                        value = value.Substring(m_LastNumberingPlanEntryDisplayPrefix.Length).Trim();
                    }

                    if (this.Core.GetAdminObject<NumberingPlanEntry>(value) == null)
                    {
                        Carrier c = Core.GetAdminObject<Carrier>("defaultcarrier++++++++++++++++++");

                        NumberingPlanEntry npe = c.CreateNumberingPlanEntry(value);
                        if (npe != null)
                            value = npe.Id;
                        else
                            value = PredefinedDestination.TargetId;
                    }

                    Destination = value;
                }
            }
        }

        [AdminSave(SkipSave = true)]
        [AdminLoad(SkipLoad = true)]
        public AdminObjectReference<NumberingPlanEntry> PredefinedDestination
        {
            get
            {
                return m_PredefinedDestination;
            }
            internal set
            {
                if (m_PredefinedDestination != null)
                {
                    m_PredefinedDestination.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_PredefinedDestination_PropertyChanged);
                    m_PredefinedDestination.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_PredefinedDestination_PropertyChanged);
                }

                m_PredefinedDestination = value;

                if (m_PredefinedDestination != null)
                {
                    m_PredefinedDestination.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_PredefinedDestination_PropertyChanged);
                    m_PredefinedDestination.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_PredefinedDestination_PropertyChanged);
                }
            }
        }

        void m_PredefinedDestination_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (m_PredefinedDestination.HasTarget && m_PredefinedDestination.Target.Activity!=null)
            {
                
                m_PredefinedDestination.Target.Activity.Target = this;
            }

            if (Destination != m_PredefinedDestination.TargetId)
            {
                if (Destination != null)
                {
                    NumberingPlanEntry npe = m_Core.GetAdminObject<NumberingPlanEntry>(Destination);
                    if (npe != null && npe.Activity != null)
                    {
                        m_LastNumberingPlanEntryDisplayPrefix = npe.DisplayTextPrefix;

                        npe.Activity.TargetId = null;
                    }
                }

                Destination = m_PredefinedDestination.TargetId;
            }

        }

        public IEnumerable<NumberingPlanEntry> NumberingPlanEntries
        {
            get
            {
                return m_Core.NumberingPlanEntries.Where((a) => (!a.Activity.HasTarget || a.Activity.Target == this));
            }
        }

        public IEnumerable<NumberingPlanEntry> NumberingPlanEntriesNoDummies
        {
            get
            {
                return m_Core.NumberingPlanEntries.Where((a) => ( (!a.Activity.HasTarget || a.Activity.Target == this) && !a.IsDummy));
            }
        }


        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public Frequency TransmitEWT
        {
            get
            {
                return GetFieldValue<Frequency>("TransmitEWT");
            }
            set
            {
                SetFieldValue<Frequency>("TransmitEWT", value);
                FirePropertyChanged("WaitLoopDescription");
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public Frequency TransmitPosition
        {
            get
            {
                return GetFieldValue<Frequency>("TransmitPosition");
            }
            set
            {
                SetFieldValue<Frequency>("TransmitPosition", value);
                FirePropertyChanged("WaitLoopDescription");
            }
        }

        public int WaitMusicLength
        {
            get
            {
                return GetFieldValue<int>("WaitMusicLength");
            }
            set
            {
                SetFieldValue<int>("WaitMusicLength", value);
            }
        }

        public bool Ring
        {
            get
            {
                return GetFieldValue<bool>("Ring");
            }
            set
            {
                SetFieldValue<bool>("Ring", value);
            }
        }

        public bool Reject
        {
            get
            {
                return GetFieldValue<bool>("Reject");
            }
            set
            {
                SetFieldValue<bool>("Reject", value);
                FirePropertyChanged("Paused");

            }
        }

        public override bool Paused
        {
            get
            {
                return Reject;
            }
            set
            {
                Reject = value;

                if (Campaign != null && Campaign.HasTarget)
                {
                    Campaign.Target.FirePropertyChanged("InboundStatus");
                    Campaign.Target.FirePropertyChanged("MailStatus");
                    Campaign.Target.FirePropertyChanged("ChatStatus");
                }

                FirePropertyChanged("Reject");
            }
        }

        public string WaitMusicDescription
        {
            get
            {
                if (WaitMusicProcessor.HasTarget && WaitMusicProcessorConfiguration != null && !string.IsNullOrEmpty(MusicPrompt))
                {
                    return string.Concat(Translate("Wait music: "),  WaitMusicProcessorConfiguration.DisplayText);
                }
                else
                {
                    return string.Concat(Translate("Wait music: "), Translate("Default music"));
                }
            }
        }


        public string WaitLoopDescription
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (WaitMusicProcessor.HasTarget && WaitMusicProcessorConfiguration!=null && !string.IsNullOrEmpty(MusicPrompt))
                {
                    sb.AppendFormat(Translate("Wait loop: play {0}"), WaitMusicProcessorConfiguration.DisplayText);
                }
                else
                {
                    sb.AppendFormat(Translate("Wait loop: play default music"));
                }

                switch (TransmitEWT)
                {
                    case Frequency.Continuously:
                        sb.Append(Translate(", transmit EWT continuously"));
                        break;
                    case Frequency.Never:
                        break;
                    case Frequency.Once:
                        sb.Append(Translate(", transmit EWT once"));
                        break;
                    case Frequency.WhenChanged:
                        sb.Append(Translate(", transmit EWT when it changes"));
                        break;
                }
                switch (TransmitPosition)
                {
                    case Frequency.Continuously:
                        sb.Append(Translate(", transmit position continuously"));
                        break;
                    case Frequency.Never:
                        break;
                    case Frequency.Once:
                        sb.Append(Translate(", transmit position once"));
                        break;
                    case Frequency.WhenChanged:
                        sb.Append(Translate(", transmit position when it changes"));
                        break;
                }

                if (TransmitPosition == Frequency.Never && TransmitEWT == Frequency.Never)
                {
                    sb.Append(Translate(", no EWT nor position"));
                }

                if (PromptForOverflow)
                {
                    sb.Append(Translate(", prompt for overflow"));
                }
                else
                {
                    sb.Append(Translate(", do not prompt for overflow"));
                }

                return sb.ToString();
            }
        }

        public bool PromptForOverflow
        {
            get
            {
                return GetFieldValue<bool>("PromptForOverflow");
            }
            set
            {
                SetFieldValue<bool>("PromptForOverflow", value);
                FirePropertyChanged("WaitLoopDescription");
            }
        }

        public int QueueMusicDelay
        {
            get
            {
                return GetFieldValue<int>("QueueMusicDelay");
            }
            set
            {
                SetFieldValue<int>("QueueMusicDelay", value);
            }
        }

        public AdminObjectReference<Prompt> OverflowPrompt
        {
            get
            {
                return m_OverflowPrompt;
            }
            internal set
            {
                if (m_OverflowPrompt != null)
                {
                    m_OverflowPrompt.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_OverflowPrompt_PropertyChanged);
                    m_OverflowPrompt.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_OverflowPrompt_PropertyChanged);
                }
                
                m_OverflowPrompt = value;

                if (m_OverflowPrompt != null)
                {
                    m_OverflowPrompt.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_OverflowPrompt_PropertyChanged);
                    m_OverflowPrompt.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_OverflowPrompt_PropertyChanged);
                }
            }
        }

        void m_OverflowPrompt_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FirePropertyChanged("OverflowPromptDescription");
        }

        public string OverflowActiveDTMFs
        {
            get
            {
                return GetFieldValue<string>("OverflowActiveDTMFs");
            }
            set
            {
                SetFieldValue<string>("OverflowActiveDTMFs", value);
                FirePropertyChanged("OverflowPromptDescription");
            }
        }

        public int OverflowPromptStartingLoop
        {
            get
            {
                return GetFieldValue<int>("OverflowPromptStartingLoop");
            }
            set
            {
                SetFieldValue<int>("OverflowPromptStartingLoop", value);
                FirePropertyChanged("OverflowPromptDescription");
            }
        }

        public string OverflowPromptDescription
        {
            get
            {
                if (OverflowPrompt.HasTarget)
                {
                    if (OverflowPromptStartingLoop == 0)
                        return string.Format(Translate("Prompt {0} and wait for DTMFs {1}..."), OverflowPrompt.Target.DisplayText, OverflowActiveDTMFs);
                    else
                        return string.Format(Translate("Prompt {0} after {1} loop and wait for DTMFs {2}..."), OverflowPrompt.Target.DisplayText, OverflowPromptStartingLoop, OverflowActiveDTMFs);
                }
                else
                {
                    return Translate("No message configured yet...");
                }
            }
        }

        #region ============================= Overflow =============================

        private AdminObjectReference<Preprocessor> m_OverflowPreprocessor;
        
        private AdminObjectReference<Prompt> m_OverflowMessage;

        private AdminObjectReference<Prompt> m_OverflowReroutePrompt;

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public OverflowActions OverflowActionType
        {
            get
            {
                return GetFieldValue<OverflowActions>("OverflowActionType");
            }
            set
            {
                OverflowActions old = GetFieldValue<OverflowActions>("OverflowActionType");

                SetFieldValue<OverflowActions>("OverflowActionType", value);

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
                        if (OverflowMessage.TargetId != value)
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
                m_OverflowReroutePrompt = value; ;
            }
        }

        public BasePreprocessorConfig OverflowPreprocessorConfig
        {
            get;
            set;
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
                    m_OverflowPreprocessor.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(OverflowPreprocessorPropertyChanged);
                }

                m_OverflowPreprocessor = value;

                if (m_OverflowPreprocessor != null)
                {
                    m_OverflowPreprocessor.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OverflowPreprocessorPropertyChanged);
                }
            }
        }

        protected void OverflowPreprocessorPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OverflowPreprocessorString = OverflowPreprocessor.TargetId;
        }

        void m_OverflowMessage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OverflowMessageString = OverflowMessage.TargetId;
        }


        #endregion

        public override string DisplayText
        {
            get
            {
                switch (MediaType)
                {
                    case Admin.MediaType.Voice:
                        return string.Concat(Translate( "Inbound "), ShortDisplayText);
                    case Admin.MediaType.Chat:
                        return string.Concat(Translate( "Chat "), ShortDisplayText);
                    case Admin.MediaType.Mail:
                        return string.Concat(Translate( "Mail "), ShortDisplayText);
                }
                return Translate("Unknown");
            }
        }

        public bool PreprocessorReplacesSkills
        {
            get
            {
                return GetFieldValue<Boolean>("PreprocessorReplacesSkills");
            }
            set
            {
                SetFieldValue<Boolean>("PreprocessorReplacesSkills", value);
            }
        }

        public bool PreprocessorReplacesLanguages
        {
            get
            {
                return GetFieldValue<Boolean>("PreprocessorReplacesLanguages");
            }
            set
            {
                SetFieldValue<Boolean>("PreprocessorReplacesLanguages", value);
            }
        }

        [AdminLoad(Path = "/Admin/InboundActivityTimeSpanActions/InboundActivityTimeSpanAction[@inboundactivityid=\"{0}\"]")]
        public AdminObjectList<InboundActivityTimeSpanAction> TimeSpanActions
        {
            get;
            internal set;
        }

        [AdminLoad(Path = "/Admin/InboundActivitySpecialDayActions/InboundActivitySpecialDayAction[@inboundactivityid=\"{0}\"]")]
        public AdminObjectList<InboundActivitySpecialDayAction> SpecialDayActions
        {
            get;
            internal set;
        }

        public object CheckedTimeSpanActions
        {
            get
            {
                if (Planning.HasTarget)
                {
                    Object obj = new AdminCheckedLinkList<PlanningTimeSpan, InboundActivityTimeSpanAction>(Planning.Target.TimeSpans, TimeSpanActions, this);
                    return obj;
                }
                return null;
            }
        }

        public object CheckedSpecialDayActions
        {
            get
            {
                if (Planning.HasTarget)
                    return new AdminCheckedLinkList<SpecialDay, InboundActivitySpecialDayAction>(Planning.Target.SpecialDays, SpecialDayActions, this);
                return null;
            }
        }

        public string PlanningSettings
        {
            get
            {
                if (Planning.Target != null)
                {
                    return string.Format(Translate("{0} is used; closed action is {1}"), Planning.Target.DisplayText, ClosedActionDescription );
                }
                return string.Empty;
            }
        }

        override protected void PlanningPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.PlanningPropertyChanged(sender, e);
            FirePropertyChanged("PlanningSettings");
            FirePropertyChanged("ClosedActionDescription");
            FirePropertyChanged("CheckedTimeSpanActions");
            FirePropertyChanged("SpecialDayActions");
            FirePropertyChanged("CheckedSpecialDayActions");

        }

        #region ============================= Closed =============================

        private AdminObjectReference<Prompt> m_ClosedMessage;

        private AdminObjectReference<Prompt> m_ClosedReroutePrompt;

        private AdminObjectReference<Preprocessor> m_ClosedPreprocessor;

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public OverflowActions ClosedActionType
        {
            get
            {
                return GetFieldValue<OverflowActions>("ClosedActionType");
            }
            set
            {
                SetFieldValue<OverflowActions>("ClosedActionType", value);
                FirePropertyChanged("ClosedActionDescription");
                FirePropertyChanged("PlanningSettings");
                ClosedParam = null;
            }
        }

        public string ClosedParam
        {
            get
            {
                return GetFieldValue<string>("ClosedParam");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = null;

                SetFieldValue<string>("ClosedParam", value);
                switch (ClosedActionType)
                {
                    case OverflowActions.Disconnect:
                        ClosedMessage.TargetId = null;
                        ClosedPreprocessor.TargetId = null;
                        ClosedRerouteDestination = null;
                        break;
                    case OverflowActions.IVR:
                        if (ClosedPreprocessor.TargetId != value)
                            ClosedPreprocessor.TargetId = value;
                        ClosedMessage.TargetId = null;
                        ClosedRerouteDestination = null;
                        break;
                    case OverflowActions.Message:
                        if (ClosedMessage.TargetId != value)
                            ClosedMessage.TargetId = value;
                        ClosedPreprocessor.TargetId = null;
                        ClosedRerouteDestination = null;
                        break;
                    case OverflowActions.None:
                        ClosedMessage.TargetId = null;
                        ClosedPreprocessor.TargetId = null;
                        ClosedRerouteDestination = null;
                        break;
                    case OverflowActions.Reroute:
                        ClosedRerouteDestination = value;
                        ClosedMessage.TargetId = null;
                        ClosedPreprocessor.TargetId = null;
                        break;
                }
            }
        }

        public string ClosedPreprocessorParams
        {
            get
            {
                return GetFieldValue<string>("ClosedPreprocessorParams");
            }
            set
            {
                SetFieldValue<string>("ClosedPreprocessorParams", value);
            }
        }

        public AdminObjectReference<Prompt> ClosedReroutePrompt
        {
            get
            {
                return m_ClosedReroutePrompt;
            }
            set
            {
                m_ClosedReroutePrompt = value;
            }
        }

        public BasePreprocessorConfig ClosedPreprocessorConfig
        {
            get;
            set;
        }

        public string ClosedActionDescription
        {
            get
            {
                string strDesc = (new OverflowActionsHelper()).Single((eh) => (eh.EnumValue == ClosedActionType)).Description;
                switch (ClosedActionType)
                {
                    case OverflowActions.Disconnect:
                    case OverflowActions.None:
                        return strDesc;
                    case OverflowActions.Message:
                        return string.Concat(strDesc, " ", ClosedMessage.HasTarget ? ClosedMessage.Target.DisplayText : "?");
                    case OverflowActions.IVR:
                        return string.Concat(strDesc, " ", ClosedPreprocessor.HasTarget ? ClosedPreprocessor.Target.DisplayText : "?");
                    case OverflowActions.Reroute:
                        return string.Concat(strDesc, " ", ClosedRerouteDestination);
                }

                return null;
            }
        }

        public string ClosedRerouteDestination
        {
            get
            {
                return ClosedParam;
            }
            set
            {
                if (ClosedActionType == OverflowActions.Reroute && ClosedParam != value)
                {
                    ClosedParam = value;
                }
                FirePropertyChanged("ClosedRerouteDestination");
                FirePropertyChanged("ClosedActionDescription");
                FirePropertyChanged("PlanningSettings");
            }
        }

        public string ClosedMessageString
        {
            get
            {
                return ClosedParam;
            }
            set
            {
                if (ClosedActionType == OverflowActions.Message && ClosedParam != value)
                {
                    ClosedParam = value;
                }
                FirePropertyChanged("ClosedMessage");
                FirePropertyChanged("ClosedPreprocessor");
                FirePropertyChanged("ClosedRerouteDestination");
                FirePropertyChanged("ClosedActionDescription");
                FirePropertyChanged("PlanningSettings");
            }
        }

        public string ClosedPreprocessorString
        {
            get
            {
                return ClosedParam;
            }
            set
            {
                if (ClosedActionType == OverflowActions.IVR && ClosedParam != value)
                {
                    ClosedParam = value;
                }
                FirePropertyChanged("ClosedMessage");
                FirePropertyChanged("ClosedPreprocessor");
                FirePropertyChanged("ClosedRerouteDestination");
                FirePropertyChanged("ClosedActionDescription");
                FirePropertyChanged("PlanningSettings");

            }
        }

        [AdminSave(SkipSave = true)]
        public AdminObjectReference<Prompt> ClosedMessage
        {
            get
            {
                return m_ClosedMessage;
            }
            set
            {
                if (m_ClosedMessage != null)
                {
                    m_ClosedMessage.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_ClosedMessage_PropertyChanged);
                    m_ClosedMessage.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_ClosedMessage_PropertyChanged);
                }

                m_ClosedMessage = value;

                if (m_ClosedMessage != null)
                {
                    m_ClosedMessage.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_ClosedMessage_PropertyChanged);
                    m_ClosedMessage.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_ClosedMessage_PropertyChanged);
                }
            }
        }

        [AdminSave(SkipSave = true)]
        public AdminObjectReference<Preprocessor> ClosedPreprocessor
        {
            get
            {
                return m_ClosedPreprocessor;
            }
            internal set
            {
                if (m_ClosedPreprocessor != null)
                {
                    m_ClosedPreprocessor.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_ClosedPreprocessor_PropertyChanged);
                    m_ClosedPreprocessor.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_ClosedPreprocessor_PropertyChanged);
                }

                m_ClosedPreprocessor = value;

                if (m_ClosedPreprocessor != null)
                {
                    m_ClosedPreprocessor.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_ClosedPreprocessor_PropertyChanged);
                    m_ClosedPreprocessor.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_ClosedPreprocessor_PropertyChanged);
                }
            }
        }

        void m_ClosedMessage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ClosedMessageString = ClosedMessage.TargetId;
        }

        void m_ClosedPreprocessor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ClosedPreprocessorString = ClosedPreprocessor.TargetId;
        }

        #endregion

        #region ============================= NoAgent =============================

        private AdminObjectReference<Prompt> m_NoAgentMessage;

        private AdminObjectReference<Prompt> m_NoAgentReroutePrompt;

        private AdminObjectReference<Preprocessor> m_NoAgentPreprocessor;

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public OverflowActions NoAgentActionType
        {
            get
            {
                return GetFieldValue<OverflowActions>("NoAgentActionType");
            }
            set
            {
                SetFieldValue<OverflowActions>("NoAgentActionType", value);
                FirePropertyChanged("NoAgentActionDescription");
                NoAgentParam = null;
            }
        }

        public string NoAgentParam
        {
            get
            {
                return GetFieldValue<string>("NoAgentParam");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = null;

                SetFieldValue<string>("NoAgentParam", value);
                switch (NoAgentActionType)
                {
                    case OverflowActions.Disconnect:
                        NoAgentMessage.TargetId = null;
                        NoAgentPreprocessor.TargetId = null;
                        NoAgentRerouteDestination = null;
                        break;
                    case OverflowActions.IVR:
                        if (NoAgentPreprocessor.TargetId != value)
                            NoAgentPreprocessor.TargetId = value;
                        NoAgentMessage.TargetId = null;
                        NoAgentRerouteDestination = null;
                        break;
                    case OverflowActions.Message:
                        if (NoAgentMessage.TargetId != value)
                            NoAgentMessage.TargetId = value;
                        NoAgentPreprocessor.TargetId = null;
                        NoAgentRerouteDestination = null;
                        break;
                    case OverflowActions.None:
                        NoAgentMessage.TargetId = null;
                        NoAgentPreprocessor.TargetId = null;
                        NoAgentRerouteDestination = null;
                        break;
                    case OverflowActions.Reroute:
                        NoAgentRerouteDestination = value;
                        NoAgentMessage.TargetId = null;
                        NoAgentPreprocessor.TargetId = null;
                        break;
                }
            }
        }

        public string NoAgentPreprocessorParams
        {
            get
            {
                return GetFieldValue<string>("NoAgentPreprocessorParams");
            }
            set
            {
                SetFieldValue<string>("NoAgentPreprocessorParams", value);
            }
        }

        public AdminObjectReference<Prompt> NoAgentReroutePrompt
        {
            get
            {
                return m_NoAgentReroutePrompt;
            }
            set
            {
                m_NoAgentReroutePrompt = value;
            }
        }


        public BasePreprocessorConfig NoAgentPreprocessorConfig
        {
            get;
            set;
        }


        public string NoAgentActionDescription
        {
            get
            {

                string strDesc = (new OverflowActionsHelper()).Single((eh) => (eh.EnumValue == NoAgentActionType)).Description;
                switch (NoAgentActionType)
                {
                    case OverflowActions.Disconnect:
                    case OverflowActions.None:
                        return strDesc;
                    case OverflowActions.Message:
                        return string.Concat(strDesc, " ", NoAgentMessage.HasTarget ? NoAgentMessage.Target.DisplayText : "?");
                    case OverflowActions.IVR:
                        return string.Concat(strDesc, " ", NoAgentPreprocessor.HasTarget ? NoAgentPreprocessor.Target.DisplayText : "?");
                    case OverflowActions.Reroute:
                        return string.Concat(strDesc, " ", NoAgentRerouteDestination);
                }

                return null;
            }
        }

        public string NoAgentRerouteDestination
        {
            get
            {
                return NoAgentParam;
            }
            set
            {
                if (NoAgentActionType == OverflowActions.Reroute && NoAgentParam != value)
                {
                    NoAgentParam = value;
                }
                FirePropertyChanged("NoAgentRerouteDestination");
                FirePropertyChanged("NoAgentActionDescription");
            }
        }

        public string NoAgentMessageString
        {
            get
            {
                return NoAgentParam;
            }
            set
            {
                if (NoAgentActionType == OverflowActions.Message && NoAgentParam != value)
                {
                    NoAgentParam = value;
                }
                FirePropertyChanged("NoAgentMessage");
                FirePropertyChanged("NoAgentPreprocessor");
                FirePropertyChanged("NoAgentRerouteDestination");
                FirePropertyChanged("NoAgentActionDescription");
            }
        }

        public string NoAgentPreprocessorString
        {
            get
            {
                return NoAgentParam;
            }
            set
            {
                if (NoAgentActionType == OverflowActions.IVR && NoAgentParam != value)
                {
                    NoAgentParam = value;
                }
                FirePropertyChanged("NoAgentMessage");
                FirePropertyChanged("NoAgentPreprocessor");
                FirePropertyChanged("NoAgentRerouteDestination");
                FirePropertyChanged("NoAgentActionDescription");
            }
        }

        [AdminSave(SkipSave = true)]
        public AdminObjectReference<Prompt> NoAgentMessage
        {
            get
            {
                return m_NoAgentMessage;
            }
            set
            {
                if (m_NoAgentMessage != null)
                {
                    m_NoAgentMessage.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_NoAgentMessage_PropertyChanged);
                    m_NoAgentMessage.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_NoAgentMessage_PropertyChanged);
                }

                m_NoAgentMessage = value;

                if (m_NoAgentMessage != null)
                {
                    m_NoAgentMessage.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_NoAgentMessage_PropertyChanged);
                    m_NoAgentMessage.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_NoAgentMessage_PropertyChanged);
                }
            }
        }

        [AdminSave(SkipSave = true)]
        public AdminObjectReference<Preprocessor> NoAgentPreprocessor
        {
            get
            {
                return m_NoAgentPreprocessor;
            }
            internal set
            {
                if (m_NoAgentPreprocessor != null)
                {
                    m_NoAgentPreprocessor.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_NoAgentPreprocessor_PropertyChanged);
                    m_NoAgentPreprocessor.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_NoAgentPreprocessor_PropertyChanged);
                }

                m_NoAgentPreprocessor = value;

                if (m_NoAgentPreprocessor != null)
                {
                    m_NoAgentPreprocessor.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_NoAgentPreprocessor_PropertyChanged);
                    m_NoAgentPreprocessor.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_NoAgentPreprocessor_PropertyChanged);
                }

            }
        }
        
        void m_NoAgentMessage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NoAgentMessageString = NoAgentMessage.TargetId;
        }

        void m_NoAgentPreprocessor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {            
            NoAgentPreprocessorString = NoAgentPreprocessor.TargetId;
        }

        #endregion

        private AdminObjectReference<OutboundActivity> m_CallbackActivity;
        private AdminObjectReference<OutboundActivity> m_AbandonsCallbackActivity;
        public AdminObjectReference<OutboundActivity> CallbackActivity
        {
            get
            {
                return m_CallbackActivity;
            }
            internal set
            {
                if (m_CallbackActivity != null)
                {
                    m_CallbackActivity.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_CallbackActivity_PropertyChanged);
                    m_CallbackActivity.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_CallbackActivity_PropertyChanged);
                }

                m_CallbackActivity = value;

                if (m_CallbackActivity != null)
                {
                    m_CallbackActivity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_CallbackActivity_PropertyChanged);
                    m_CallbackActivity.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_CallbackActivity_PropertyChanged);
                }

            }
        }
        public bool AbandonsAreCalledBack
        {
            get
            {
                return GetFieldValue<bool>("AbandonsAreCalledBack");
            }
            set
            {
                SetFieldValue<bool>("AbandonsAreCalledBack", value);
                FirePropertyChanged("AbandonsCallbackDescription");
            }
        }
        public string AbandonsCallbackDescription
        {
            get
            {
                if (!AbandonsAreCalledBack)
                    return string.Empty;
                if (AbandonsCallbackActivity.HasTarget)
                    return string.Format(Translate("using activity {0}"), AbandonsCallbackActivity.Target.ShortDisplayText);
                else
                    return "using default activity";
            }
        }

        public AdminObjectReference<OutboundActivity> AbandonsCallbackActivity
        {
            get
            {
                return m_AbandonsCallbackActivity;
            }
            internal set
            {
                if (m_AbandonsCallbackActivity != null)
                {
                    m_AbandonsCallbackActivity.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_AbandonsCallbackActivity_PropertyChanged);
                    m_AbandonsCallbackActivity.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_AbandonsCallbackActivity_PropertyChanged);
                }

                m_AbandonsCallbackActivity = value;

                if (m_AbandonsCallbackActivity != null)
                {
                    m_AbandonsCallbackActivity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_AbandonsCallbackActivity_PropertyChanged);
                    m_AbandonsCallbackActivity.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_AbandonsCallbackActivity_PropertyChanged);
                }

            }
        }

        void m_CallbackActivity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {  
        }

        void m_AbandonsCallbackActivity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FirePropertyChanged("AbandonsCallbackDescription");
        }

        public int InitialProfit
        {
            get
            {
                return GetFieldValue<int>("InitialProfit");
            }
            set
            {
                SetFieldValue<int>("InitialProfit", value);
                FirePropertyChanged("ProfitRulesAreUsed");
                FirePropertyChanged("ProfitDescription");
            }
        }

        public int AlternateInitialProfit
        {
            get
            {
                return GetFieldValue<int>("AlternateInitialProfit");
            }
            set
            {
                SetFieldValue<int>("AlternateInitialProfit", value);
                FirePropertyChanged("ProfitRulesAreUsed");
                FirePropertyChanged("ProfitDescription");
            }
        }

        public string AlternateInitialProfitRule
        {
            get
            {
                return GetFieldValue<string>("AlternateInitialProfitRule");
            }
            set
            {
                SetFieldValue<string>("AlternateInitialProfitRule", value);
                FirePropertyChanged("ProfitRulesAreUsed");
                FirePropertyChanged("ProfitDescription");
            }

        }

        public bool ProfitRulesAreUsed
        {
            get
            {
                return !string.IsNullOrEmpty(AlternateInitialProfitRule) || InitialProfit != 0 || AlternateInitialProfit!=1;
            }
            set
            {
                if (!value)
                {
                    AlternateInitialProfit = 1;
                    InitialProfit = 0;
                    AlternateInitialProfitRule = string.Empty;
                }
                else
                {
                    InitialProfit = 10;
                }
            }
        }

        public string ProfitDescription
        {
            get
            {
                if(string.IsNullOrEmpty(AlternateInitialProfitRule))
                    return string.Format(Translate("Initial value is {0}"), InitialProfit);
                else
                    return string.Format(Translate("Initial value is {0} but when caller is matching '{1}', value {2} is used"), InitialProfit, AlternateInitialProfitRule, AlternateInitialProfit);
            }
        }

        public IEnumerable<Preprocessor> Preprocessors
        {
            get
            {
                return Core.Preprocessors.Where((a) => (a.MediaType == this.MediaType));
            }
        }

        public int SlaPercentageHandledInTime
        {
            get
            {
                return GetFieldValue<int>("SlaPercentageHandledInTime");
            }
            set
            {
                SetFieldValue<int>("SlaPercentageHandledInTime", value);
            }
        }

        public int SlaPercentageToHandle
        {
            get
            {
                return GetFieldValue<int>("SlaPercentageToHandle");
            }
            set
            {
                SetFieldValue<int>("SlaPercentageToHandle", value);
            }
        }

        public int SlaTime
        {
            get
            {
                return GetFieldValue<int>("SlaTime");
            }
            set
            {
                SetFieldValue<int>("SlaTime", value);
                FirePropertyChanged("SlaDescription");
            }
        }

        public bool SlaIncludeOverflow
        {
            get
            {
                return GetFieldValue<bool>("SlaIncludeOverflow");
            }
            set
            {
                SetFieldValue<bool>("SlaIncludeOverflow", value);

                FirePropertyChanged("SlaDescription");
            }
        }


        public int SlaMode
        {
            get
            {
                return GetFieldValue<int>("SlaMode");
            }
            set
            {
                SetFieldValue<int>("SlaMode", value);

                FirePropertyChanged("SlaDescription");
            }
        }

        public string SlaDescription
        {
            get
            {
                if (SlaMode > 0)
                {
                    if (SlaIncludeOverflow)
                        return string.Format(Translate("Overflow calls included in computation, SLA time {0}"),  DurationHelpers.GetDefaultDurationString( SlaTime, false));
                    else
                        return string.Format(Translate("Overflow calls excluded in computation, SLA time {0}"),  DurationHelpers.GetDefaultDurationString( SlaTime, false));
                }
                else
                {
                    return Translate("SLA is not computed");
                }
            }
        }

        public bool UsePreferredAgent
        {
            get
            {
                return GetFieldValue<bool>("UsePreferredAgent");
            }
            set
            {
                SetFieldValue<bool>("UsePreferredAgent", value);
                FirePropertyChanged("PreferredAgentSettings");
            }
        }

        public int PreferredAgentQueueTime
        {
            get
            {
                return GetFieldValue<int>("PreferredAgentQueueTime");
            }
            set
            {
                SetFieldValue<int>("PreferredAgentQueueTime", value);
                FirePropertyChanged("PreferredAgentSettings");
            }
        }

        public int PreferredAgentValidity
        {
            get
            {
                return GetFieldValue<int>("PreferredAgentValidity");
            }
            set
            {
                SetFieldValue<int>("PreferredAgentValidity", value);
                FirePropertyChanged("PreferredAgentSettings");
            }
        }


        public string PreferredAgentSettings
        {
            get
            {
                return string.Format("Try to distribute to preferred agent during {0} seconds", PreferredAgentQueueTime);
            }
        }

        public InboundActivity Duplicate(bool attachToCampaign)
        {
            InboundActivity newia = AdminCore.Create<InboundActivity>();

            newia.CopyProperties(this, attachToCampaign);            

            newia.AlternateInitialProfit = AlternateInitialProfit;
            newia.AlternateInitialProfitRule = AlternateInitialProfitRule;

            newia.CallbackActivity.TargetId = CallbackActivity.TargetId;

            newia.ClosedActionType = ClosedActionType;
            newia.ClosedParam = ClosedParam;
            newia.ClosedPreprocessorParams = ClosedPreprocessorParams;
            newia.ClosedReroutePrompt.TargetId = ClosedReroutePrompt.TargetId;
            if (newia.ClosedPreprocessor != null && newia.ClosedPreprocessor.HasTarget && newia.ClosedPreprocessor.Target != null && newia.ClosedPreprocessor.Target.ConfigType != null)
            {
                newia.ClosedPreprocessorConfig = (BasePreprocessorConfig)Core.Create(newia.ClosedPreprocessor.Target.ConfigType, this, string.Concat("_", Id));
                newia.ClosedPreprocessorConfig.DeserializeFromText(newia.ClosedPreprocessorParams);
                newia.ClosedPreprocessorConfig.saveToTextStorage = ((t) => { newia.ClosedPreprocessorParams = t; });
            }

            newia.InitialProfit = InitialProfit;

            newia.NoAgentActionType = NoAgentActionType;
            newia.NoAgentParam = NoAgentParam;
            newia.NoAgentPreprocessorParams = NoAgentPreprocessorParams;
            newia.NoAgentReroutePrompt.TargetId = NoAgentReroutePrompt.TargetId;
            if (newia.NoAgentPreprocessor != null && newia.NoAgentPreprocessor.HasTarget && newia.NoAgentPreprocessor.Target != null && newia.NoAgentPreprocessor.Target.ConfigType != null)
            {
                newia.NoAgentPreprocessorConfig = (BasePreprocessorConfig)Core.Create(newia.NoAgentPreprocessor.Target.ConfigType, this, string.Concat("_", Id));
                newia.NoAgentPreprocessorConfig.DeserializeFromText(newia.NoAgentPreprocessorParams);
                newia.NoAgentPreprocessorConfig.saveToTextStorage = ((t) => { newia.NoAgentPreprocessorParams = t; });
            }



            newia.OverflowActionType = OverflowActionType;
            newia.OverflowParam = OverflowParam;
            newia.OverflowPreprocessorParams = OverflowPreprocessorParams;
            newia.OverflowReroutePrompt.TargetId = OverflowReroutePrompt.TargetId;
            if (newia.OverflowPreprocessor != null && newia.OverflowPreprocessor.HasTarget && newia.OverflowPreprocessor.Target != null && newia.OverflowPreprocessor.Target.ConfigType != null)
            {
                newia.OverflowPreprocessorConfig = (BasePreprocessorConfig)Core.Create(newia.OverflowPreprocessor.Target.ConfigType, this, string.Concat("_", Id));
                newia.OverflowPreprocessorConfig.DeserializeFromText(newia.OverflowPreprocessorParams);
                newia.OverflowPreprocessorConfig.saveToTextStorage = ((t) => { newia.OverflowPreprocessorParams = t; });
            }


            newia.Paused = Paused;

            newia.PreferredAgentQueueTime = PreferredAgentQueueTime;
            newia.PreferredAgentValidity = PreferredAgentValidity;
            newia.PreprocessorReplacesLanguages = PreprocessorReplacesLanguages;
            newia.PreprocessorReplacesSkills = PreprocessorReplacesSkills;
            newia.PromptForOverflow = PromptForOverflow;
            newia.OverflowPrompt.TargetId = OverflowPrompt.TargetId;
            newia.QueueMusicDelay = QueueMusicDelay;
            newia.Reject = Reject;
            newia.Ring = Ring;
            newia.SlaPercentageHandledInTime = SlaPercentageHandledInTime;
            newia.SlaPercentageToHandle = SlaPercentageToHandle;
            newia.SlaTime = SlaTime;
            newia.SlaMode = SlaMode;
            newia.SlaIncludeOverflow = SlaIncludeOverflow;

            foreach (InboundActivitySpecialDayAction iasda in SpecialDayActions)
            {
                newia.SpecialDayActions.Add(iasda.SpecialDay);
                newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowActionType = iasda.OverflowActionType;
                newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowParam = iasda.OverflowParam;
                newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowPreprocessorParams = iasda.OverflowPreprocessorParams;
                newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowReroutePrompt.TargetId = iasda.OverflowReroutePrompt.TargetId;

                if (newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowPreprocessor != null && newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowPreprocessor.HasTarget && newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowPreprocessor.Target != null && newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowPreprocessor.Target.ConfigType != null)
                {
                    newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowPreprocessorConfig = (BasePreprocessorConfig)Core.Create(newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowPreprocessor.Target.ConfigType, this, string.Concat("_", Id));
                    newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowPreprocessorConfig.DeserializeFromText(newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowPreprocessorParams);
                    newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowPreprocessorConfig.saveToTextStorage = ((t) => { newia.SpecialDayActions[newia.SpecialDayActions.Count() - 1].OverflowPreprocessorParams = t; });
                }

            }

            foreach (InboundActivityTimeSpanAction iatsa in TimeSpanActions)
            {
                newia.TimeSpanActions.Add(iatsa.PlanningTimeSpan);
                newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowActionType = iatsa.OverflowActionType;
                newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowParam = iatsa.OverflowParam;
                newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowPreprocessorParams = iatsa.OverflowPreprocessorParams;
                newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowReroutePrompt = iatsa.OverflowReroutePrompt;

                if (newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowPreprocessor != null && newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowPreprocessor.HasTarget && newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowPreprocessor.Target != null && newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowPreprocessor.Target.ConfigType != null)
                {
                    newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowPreprocessorConfig = (BasePreprocessorConfig)Core.Create(newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowPreprocessor.Target.ConfigType, this, string.Concat("_", Id));
                    newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowPreprocessorConfig.DeserializeFromText(newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowPreprocessorParams);
                    newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowPreprocessorConfig.saveToTextStorage = ((t) => { newia.TimeSpanActions[newia.TimeSpanActions.Count() - 1].OverflowPreprocessorParams = t; });
                }

            }

            newia.TransmitEWT = TransmitEWT;
            newia.TransmitPosition = TransmitPosition;
            newia.UsePreferredAgent = UsePreferredAgent;
            newia.WaitMusicLength = WaitMusicLength;

            if(attachToCampaign)
                newia.Campaign.Target.InboundActivities.Add(newia);
            

            newia.SecurityContext.TargetId = SecurityContext.TargetId;

            newia.AdminCore.InboundActivities.Add(newia);

            DuplicateSecurity(newia);

            return newia;
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
                    //SecurityContexts.RemoveAt(0);
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


        private AdminObjectList<DynamicParameterLink> m_DynamicParameters;

        [AdminLoad(Path = "/Admin/DynamicParameterLinks/DynamicParameterLink[@adminobjectid=\"{0}\"]")]
        public AdminObjectList<DynamicParameterLink> DynamicParameters
        {
            get
            {
                return m_DynamicParameters;
            }
            internal set
            {
                if (m_DynamicParameters != null)
                {
                    m_DynamicParameters.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_DynamicParameters_Collection_Changed);
                }

                m_DynamicParameters = value;

                if (m_DynamicParameters != null)
                {
                    m_DynamicParameters.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_DynamicParameters_Collection_Changed);
                }
            }
        }

        private void m_DynamicParameters_Collection_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
        }


        public object CheckedDynamicParameters
        {
            get
            {
                return new AdminCheckedLinkList<DynamicParameterDefinition, DynamicParameterLink>(m_Core.DynamicParameterDefinitions, DynamicParameters, this);
            }
        }


    }

    public class OutboundActivity : Activity
    {
        public void ClosedActionDeleted()
        {
            ClosedActionType = OutboundClosingAction.None;
            ClosedParam = DialingMode.Progressive;
            Planning.TargetId = null;
            Planning.DoneLoading();
            SetFieldLoaded("ClosedActionType");
            SetFieldLoaded("ClosedParam");
        }

        private AdminObjectReference<Prompt> m_AmdPrompt;
        private List<string> m_DeletedRelatedIds = new List<string>();
        private AdminObjectReference<SecurityContext> m_SecurityContext = null;
        private SingletonAdminObjectList<AdminObjectSecurityContext> m_SecurityContexts = null;


        public override void Clear()
        {
            foreach (SortField sf in SortFields)
                m_DeletedRelatedIds.Add(sf.Id);
            foreach (FilterPart sf in FilterParts)
                m_DeletedRelatedIds.Add(sf.Id);
            foreach (OutboundActivityCurrentActivityFilter sf in ActivityFilterParts)
                m_DeletedRelatedIds.Add(sf.Id);

            base.Clear();   
        }
        internal override void Delete()
        {
            base.Delete();

            foreach (string str in m_DeletedRelatedIds)
                Core.GetAdminObject(str).Delete();
        }
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

                if (Campaign!=null && Campaign.HasTarget)
                {
                    if (!Campaign.Target.OutboundActivities.Contains(this))
                        Campaign.Target.OutboundActivities.Add(this);
                }

                while (SortFields.Count() > 0)
                {
                    SortField sf = SortFields[0];
                    Core.Delete(sf);
                    SortFields.Remove(sf);
                }



                XmlNodeList sortfields = node.SelectNodes("OrderByFields/SortField");
                foreach (XmlNode xmlsf in sortfields)
                {
                    SortField tempSf = new SortField(SortFields);
                    tempSf.Load(xmlsf as XmlElement);

                    m_Core.SetAdminObject(tempSf);
                    SortFields.Add(tempSf);
                }


                while (FilterParts.Count > 0)
                {
                    FilterPart fp = FilterParts[0];
                    Core.Delete(fp);
                    FilterParts.Remove(fp);
                }

                XmlNodeList filterparts = node.SelectNodes("Filters/FilterPart");
                foreach (XmlNode xmlsf in filterparts)
                {
                    FilterPart tempSf = Core.Create<FilterPart>();
                    tempSf.Parent = FilterParts;
                    tempSf.Loading = true;
                    tempSf.Load(xmlsf as XmlElement);
                    FilterParts.Add(tempSf);
                    tempSf.Loading = false;
                }


                while (ActivityFilterParts.Count() > 0)
                {
                    OutboundActivityCurrentActivityFilter oacaf = ActivityFilterParts[0];
                    Core.Delete(oacaf);
                    ActivityFilterParts.Remove(oacaf);
                }

                XmlNodeList activityfilters = node.SelectNodes("CurrentActivityFilters/CurrentActivityFilterPart");
                foreach (XmlNode xmlsf in activityfilters)
                {
                    OutboundActivityCurrentActivityFilter tempSf = new OutboundActivityCurrentActivityFilter(ActivityFilterParts);
                    tempSf.Load(xmlsf as XmlElement);
                    m_Core.SetAdminObject(tempSf);
                    ActivityFilterParts.Add(tempSf);
                }

                SetFieldLoaded("DataSortOrder");
                SetFieldLoaded("SystemSortOrder");
                SetFieldLoaded("DataFilter");
                SetFieldLoaded("SystemFilter");
                SetFieldLoaded("CurrentActivityFilter");
                return true;
            }
            return false;
        }

        private AdminObjectReference<Location> m_Location;

        private AdminObjectReference<Carrier> m_Carrier;

        private AdminObjectReference<AmdSettings> m_AmdSettings;

        private AdminObjectReference<Language> m_InformationalLanguage;

        private AdminObjectReference<CallbackRuleset> m_CallbackRules;

        private AdminObjectList<FilterPart> m_FilterParts;

        private AdminObjectList<OutboundActivityCurrentActivityFilter> m_ActivityFilterParts;

        private AdminObjectList<SortField> m_SortFields;

        public override string DisplayText
        {
            get
            {
                if (State>0)
                {
                    switch (OutboundMode)
                    {
                        case DialingMode.Search:
                            return string.Concat(Translate("Search "), ShortDisplayText);
                        default:
                            if(Campaign!=null && Campaign.Target!=null && Campaign.Target.SystemCallbackActivity==this)
                                return string.Concat(Translate("Callbacks "), ShortDisplayText);
                            return string.Concat(Translate("Outbound "), ShortDisplayText);
                    }
                }
                else
                {
                    switch (OutboundMode)
                    {
                        case DialingMode.Search:
                            return string.Concat(Translate("[Search "), ShortDisplayText, "]");
                        default:
                            if (Campaign != null && Campaign.Target != null && Campaign.Target.SystemCallbackActivity == this)
                                return string.Concat(Translate("Callbacks "), ShortDisplayText);
                            return string.Concat(Translate("[Outbound "), ShortDisplayText, "]");
                    }
                }
            }
        }

        public OutboundActivity(AdminCore core)
            : base(core)
        {
            Init();
        }

        public OutboundActivity(AdminObject parent)
            : base(parent)
        {
            Init();
        }


        private void Init()
        {
            AmdPrompt = new AdminObjectReference<Prompt>(this);
            SortFields = new AdminObjectList<SortField>(this);
            FilterParts = new AdminObjectList<FilterPart>(this);
            ActivityFilterParts = new AdminObjectList<OutboundActivityCurrentActivityFilter>(this);
            m_Location = new AdminObjectReference<Admin.Location>(this);
            m_Carrier = new AdminObjectReference<Carrier>(this);
            m_AmdSettings = new AdminObjectReference<Admin.AmdSettings>(this);
            m_InformationalLanguage = new AdminObjectReference<Language>(this);
            this.MediaType = MediaType.Voice;
            OutboundMode = DialingMode.Progressive;
            RingTime = 20;            
            Paused = true;
            SetFieldValue<int>("MaxAbandons", 10);
            SetFieldValue<int>("TargetAbandons", 5);
            SecurityContext = new AdminObjectReference<SecurityContext>(this);
            SecurityContexts = new SingletonAdminObjectList<AdminObjectSecurityContext>(this);

        }

        internal protected override void LoadChildLists(System.Xml.XmlElement node, List<string> ignoreObjects)
        {
            base.LoadChildLists(node, ignoreObjects);
            if (node == null && ignoreObjects == null)
            {
                // a new activity is created
                
                SortField sf = (SortField)(SortFields.AddLinkItem(m_Core.SystemFields.Single((f) => (f.FieldMeaning == SystemFieldMeanings.Priority))));
                sf.SortOrder = SortOrder.Ascending;
                sf.Sequence = 1;
                
                sf = (SortField)(SortFields.AddLinkItem(m_Core.SystemFields.Single((f) => (f.FieldMeaning == SystemFieldMeanings.DialStartDate))));
                sf.SortOrder = SortOrder.Ascending;
                sf.Sequence = 2;

                sf = (SortField)(SortFields.AddLinkItem(m_Core.SystemFields.Single((f) => (f.FieldMeaning == SystemFieldMeanings.ExpectedProfit))));
                sf.SortOrder = SortOrder.Descending;
                sf.Sequence = 3;

                RecomputeSortOrder();

                ActivityFilterParts.AddLinkItem(m_Core.ActivityFilters.Single((f) => (f.Id == "_0this")));
                ActivityFilterParts.AddLinkItem(m_Core.ActivityFilters.Single((f) => (f.Id == "_1null")));
                
            }
        }

        public string Originator
        {
            get
            {
                return GetFieldValue<string>("Originator");
            }
            set
            {
                SetFieldValue("Originator", value);
            }
        }

        public AdminObjectReference<Language> InformationalLanguage
        {
            get
            {
                return m_InformationalLanguage;
            }
            internal set
            {
                m_InformationalLanguage = value;
            }
        }


        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public DialingMode OutboundMode
        {
            get
            {
                return GetFieldValue<DialingMode>("OutboundMode");
            }
            set
            {
                SetFieldValue("OutboundMode", value);
                FirePropertyChanged("DisplayText");
            }
        }

        public int TargetAbandons
        {
            get
            {
                return GetFieldValue<int>("TargetAbandons");
            }
            set
            {
                SetFieldValue<int>("TargetAbandons", value);
                if (Campaign!=null && Campaign.HasTarget && !Campaign.Target.Advanced)
                {
                    MaxAbandons = 2 * TargetAbandons;
                }
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public AbandonRateMode AbandonRateMode
        {
            get
            {
                return GetFieldValue<AbandonRateMode>("AbandonRateMode");
            }
            set
            {
                SetFieldValue<AbandonRateMode>("AbandonRateMode", value);
            }
        }

        public int MaxAbandons
        {
            get
            {
                return GetFieldValue<int>("MaxAbandons");
            }
            set
            {
                SetFieldValue<int>("MaxAbandons", value);
            }
        }

        public int MaxDialPerAgent
        {
            get
            {
                return GetFieldValue<int>("MaxDialPerAgent");
            }
            set
            {
                SetFieldValue<int>("MaxDialPerAgent", value);
            }
        }

        public int DialPerAgent
        {
            get
            {
                return GetFieldValue<int>("DialPerAgent");
            }
            set
            {
                SetFieldValue<int>("DialPerAgent", value);
            }
        }

        public int RingTime
        {
            get
            {
                return GetFieldValue<int>("RingTime");
            }
            set
            {
                SetFieldValue<int>("RingTime", value);
            }
        }

        public override bool Paused
        {
            get
            {
                return GetFieldValue<bool>("Paused");
            }
            set
            {
                SetFieldValue<bool>("Paused", value);
                if (Campaign != null && Campaign.HasTarget)
                {
                    Campaign.Target.FirePropertyChanged("OutboundStatus");
                }
            }
        }

        public bool TargetedCallbackNeverPaused
        {
            get
            {
                return GetFieldValue<bool>("TargetedCallbackNeverPaused");
            }
            set
            {
                SetFieldValue<bool>("TargetedCallbackNeverPaused", value);
            }
        }

        public bool EnqueueCallbacks
        {
            get
            {
                return GetFieldValue<bool>("EnqueueCallbacks");
            }
            set
            {
                SetFieldValue<bool>("EnqueueCallbacks", value);
            }
        }

        public bool TargetedCallbacksRequireTeamMembership
        {
            get
            {
                return GetFieldValue<bool>("TargetedCallbacksRequireTeamMembership");
            }
            set
            {
                SetFieldValue<bool>("TargetedCallbacksRequireTeamMembership", value);
            }
        }

        [AdminSave(SkipSave= true)]
        [AdminLoad(Path = "/Admin/OutboundActivities/OutboundActivity[@id=\"{0}\"]/OrderByFields/SortField")]
        public AdminObjectList<SortField> SortFields
        {
            get
            {
                return m_SortFields;
            }
            internal set
            {

                if(m_SortFields!=null)
                    m_SortFields.CollectionChanged -= new NotifyCollectionChangedEventHandler(m_SortFields_CollectionChanged);

                m_SortFields = value;

                if (m_SortFields != null)
                    m_SortFields.CollectionChanged += new NotifyCollectionChangedEventHandler(m_SortFields_CollectionChanged);
            }
        }

        void m_SortFields_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RecomputeSortOrder();
        }

        [AdminLoad(Path = "/Admin/OutboundActivities/OutboundActivity[@id=\"{0}\"]/Filters/FilterPart")]
        public AdminObjectList<FilterPart> FilterParts
        {
            get
            {
                return m_FilterParts;
            }
            internal set
            {
                if (m_FilterParts != null)
                    m_FilterParts.CollectionChanged -= new NotifyCollectionChangedEventHandler(m_FilterParts_CollectionChanged);

                m_FilterParts = value;

                if(m_FilterParts!=null)
                    m_FilterParts.CollectionChanged += new NotifyCollectionChangedEventHandler(m_FilterParts_CollectionChanged);
            }
        }

        void m_FilterParts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RecomputeFilters();
        }

        [AdminSave(SkipSave=true)]
        [AdminLoad(Path = "/Admin/OutboundActivities/OutboundActivity[@id=\"{0}\"]/CurrentActivityFilters/CurrentActivityFilterPart")]
        public AdminObjectList<OutboundActivityCurrentActivityFilter> ActivityFilterParts
        {
            get
            {
                return m_ActivityFilterParts;
            }
            internal set
            {
                if (m_ActivityFilterParts != null)
                    m_ActivityFilterParts.CollectionChanged -= new NotifyCollectionChangedEventHandler(m_ActivityFilterParts_CollectionChanged);

                m_ActivityFilterParts = value;

                if(m_ActivityFilterParts!=null)
                    m_ActivityFilterParts.CollectionChanged += new NotifyCollectionChangedEventHandler(m_ActivityFilterParts_CollectionChanged);
            }
        }

        void m_ActivityFilterParts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RecomputeFilters();
        }

        public object CheckedActivityFilters
        {
            get
            {
                return new AdminCheckedLinkList<CurrentActivityFilter, OutboundActivityCurrentActivityFilter>(Campaign.Target.CurrentActivityFilterKinds, ActivityFilterParts, this );
            }
        }

        // TODO:   BlackListCategory 

        public AdminObjectReference<Location> Location
        {
            get
            {
                return m_Location;
            }
            internal set
            {
                m_Location = value;
            }
        }

        public AdminObjectReference<Carrier> Carrier
        {
            get
            {
                return m_Carrier;
            }
            internal set
            {
                m_Carrier = value;
            }
        }


        public AdminObjectReference<AmdSettings> AmdSettings
        {
            get
            {
                return m_AmdSettings;
            }
            internal set
            {
                m_AmdSettings = value;
            }
        }

        public AdminObjectReference<CallbackRuleset> CallbackRules
        {
            get
            {
                return m_CallbackRules;
            }
            internal set
            {
                m_CallbackRules = value;
            }
        }

        public string PlanningSettings
        {
            get
            {
                if (Planning.Target != null)
                {
                    return string.Format(Translate("{0} is used; closed action is {1}"), Planning.Target.DisplayText, ClosedActionDescription);
                }
                return Translate("Please, select a planning...");
            }
        }

        override protected void PlanningPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.PlanningPropertyChanged(sender, e);
            FirePropertyChanged("PlanningSettings");
            FirePropertyChanged("ClosedActionDescription");
            FirePropertyChanged("CheckedTimeSpanActions");
            FirePropertyChanged("SpecialDayActions");
            FirePropertyChanged("CheckedSpecialDayActions");
        }

        public OutboundClosingAction ClosedActionType
        {
            get
            {
                return GetFieldValue<OutboundClosingAction>("ClosedActionType");
            }
            set
            {
                SetFieldValue<OutboundClosingAction>("ClosedActionType", value);
                FirePropertyChanged("ClosedActionDescription");
                FirePropertyChanged("PlanningSettings");
                ClosedParam  = DialingMode.Progressive;
            }
        }

        public string ClosedActionDescription
        {
            get
            {
                string strDesc = (new OutboundClosingActionsHelper()).Single((eh) => (eh.EnumValue == ClosedActionType)).Description;
                switch (ClosedActionType)
                {
                    case OutboundClosingAction.PauseActivity:
                        return strDesc;
                    case OutboundClosingAction.None:
                        return strDesc;
                    case OutboundClosingAction.ChangeDialingMode:
                        return string.Concat(strDesc, " ", ClosedParam);
                }

                return null;
            }
        }

        public DialingMode ClosedParam
        {
            get
            {
                return GetFieldValue<DialingMode>("ClosedParam");
            }
            set
            {
                SetFieldValue<DialingMode>("ClosedParam", value);

            }
        }

        [AdminLoad(Path = "/Admin/OutboundActivityTimeSpanActions/OutboundActivityTimeSpanAction[@outboundactivityid=\"{0}\"]")]
        public AdminObjectList<OutboundActivityTimeSpanAction> TimeSpanActions
        {
            get;
            internal set;
        }

        [AdminLoad(Path = "/Admin/OutboundActivitySpecialDayActions/OutboundActivitySpecialDayAction[@outboundactivityid=\"{0}\"]")]
        public AdminObjectList<OutboundActivitySpecialDayAction> SpecialDayActions
        {
            get;
            internal set;
        }

        public object CheckedTimeSpanActions
        {
            get
            {
                if (Planning.HasTarget)
                    return new AdminCheckedLinkList<PlanningTimeSpan, OutboundActivityTimeSpanAction>(Planning.Target.TimeSpans, TimeSpanActions, this);
                return null;
            }
        }

        public object CheckedSpecialDayActions
        {
            get
            {
                if (Planning.HasTarget)
                    return new AdminCheckedLinkList<SpecialDay, OutboundActivitySpecialDayAction>(Planning.Target.SpecialDays, SpecialDayActions, this);
                return null;
            }
        }

        public XmlDocument DataSortOrder
        {
            get
            {
                return GetFieldValue<XmlDocument>("DataSortOrder");
            }
            set
            {
                SetFieldValue<XmlDocument>("DataSortOrder", value);
            }
        }

        public XmlDocument SystemSortOrder
        {
            get
            {
                return GetFieldValue<XmlDocument>("SystemSortOrder");
            }
            set
            {
                SetFieldValue<XmlDocument>("SystemSortOrder", value);
            }
        }

        public XmlDocument DataFilter
        {
            get
            {
                return GetFieldValue<XmlDocument>("DataFilter");
            }
            set
            {
                SetFieldValue<XmlDocument>("DataFilter", value);
            }
        }

        public XmlDocument SystemFilter
        {
            get
            {
                return GetFieldValue<XmlDocument>("SystemFilter");
            }
            set
            {
                SetFieldValue<XmlDocument>("SystemFilter", value);
            }
        }

        public XmlDocument CurrentActivityFilter
        {
            get
            {
                return GetFieldValue<XmlDocument>("CurrentActivityFilter");
            }
            set
            {
                SetFieldValue<XmlDocument>("CurrentActivityFilter", value);
            }
        }

        public void RecomputeSortOrder()
        {
            XmlDocument docUserSortOrders = new XmlDocument();
            docUserSortOrders.AppendChild(docUserSortOrders.CreateElement("UserSortOrders"));

            XmlDocument docSystemSortOrders = new XmlDocument();
            docSystemSortOrders.AppendChild(docSystemSortOrders.CreateElement("SystemSortOrders"));


            if (SortFields == null || SortFields.Count == 0)
            {
                DataSortOrder = docUserSortOrders;
                SystemSortOrder = docSystemSortOrders;
                return;
            }


            foreach (SortField sf in SortFields.OrderBy( (a) => (a.Sequence) ) )
            {                
                if (sf.Field is UserField)
                {
                    sf.SaveNode(docUserSortOrders);
                }
                else if (sf.Field is SystemField)
                {
                    sf.SaveNode(docSystemSortOrders);
                }
                else if(sf.Field is QuotaField)
                {
                    sf.SaveNode(docSystemSortOrders);
                }
            }

            DataSortOrder = docUserSortOrders;

            SystemSortOrder = docSystemSortOrders;

        }

        public void RecomputeFilters()
        {

            XmlDocument docDataFilters = new XmlDocument();
            docDataFilters.AppendChild(docDataFilters.CreateElement("UserFilter"));

            XmlDocument docSystemFilters = new XmlDocument();
            docSystemFilters.AppendChild(docSystemFilters.CreateElement("SystemFilter"));

            XmlDocument docCurrentActivityFilters = new XmlDocument();
            docCurrentActivityFilters.AppendChild(docCurrentActivityFilters.CreateElement("CurrentActivityFilter"));

            if (FilterParts != null)
            {
                foreach (FilterPart fp in FilterParts.OrderBy((a) => (a.Sequence)))
                {
                    if (fp.Field.HasTarget && fp.Field.Target is UserField)
                    {
                        fp.SaveNode(docDataFilters);
                    }
                    else if (fp.Field.HasTarget && fp.Field.Target is SystemField)
                    {
                        fp.SaveNode(docSystemFilters);
                    }
                    else if(fp.Field.HasTarget && fp.Field.Target is QuotaField)
                    {
                        fp.SaveNode(docSystemFilters);
                    }
                }
            }

            if (ActivityFilterParts != null)
            {
                foreach (OutboundActivityCurrentActivityFilter oacaf in ActivityFilterParts)
                {
                    oacaf.SaveNode(docCurrentActivityFilters);
                }
            }

            DataFilter = docDataFilters;

            SystemFilter = docSystemFilters;

            CurrentActivityFilter = docCurrentActivityFilters;
        }

        public OutboundActivity Duplicate(bool attachToCampaign)
        {
            OutboundActivity newoa = AdminCore.Create<OutboundActivity>();

            newoa.CopyProperties(this, attachToCampaign);

            newoa.AbandonRateMode = AbandonRateMode;

            newoa.AmdSettings.TargetId = AmdSettings.TargetId;
            newoa.CallbackRules.TargetId = CallbackRules.TargetId;
            newoa.Carrier.TargetId = Carrier.TargetId;

            newoa.ClosedActionType = ClosedActionType;
            newoa.ClosedParam = ClosedParam;

            newoa.Description = string.Concat(Description, "*");
            newoa.DialPerAgent = DialPerAgent;

            newoa.InformationalLanguage.TargetId = InformationalLanguage.TargetId;
            newoa.Location.TargetId = Location.TargetId;
            newoa.MaxAbandons = MaxAbandons;
            newoa.MaxDialPerAgent = MaxDialPerAgent;
            newoa.Originator = Originator;
            newoa.OutboundMode = OutboundMode;
            newoa.Paused = Paused;
            newoa.RingTime = RingTime;
            newoa.TargetAbandons = TargetAbandons;
            newoa.TargetedCallbackNeverPaused = TargetedCallbackNeverPaused;
            newoa.TargetedCallbacksRequireTeamMembership = TargetedCallbacksRequireTeamMembership;


            if (attachToCampaign)
            {

                foreach (SortField sf in SortFields)
                {
                    SortField newsf = null;
                    newsf = (SortField)(newoa.SortFields.AddLinkItem(sf.Field));
                    newsf.Sequence = sf.Sequence;
                    newsf.SortOrder = sf.SortOrder;
                }


                foreach (FilterPart fp in FilterParts)
                {
                    FilterPart sf = fp.AdminCore.Create<FilterPart>(newoa);

                    sf.Field.TargetId = fp.Field.TargetId;
                    sf.Operator = fp.Operator;
                    sf.OperandField.TargetId = fp.OperandField.TargetId;
                    sf.OperandText = fp.OperandText;
                    sf.Activity.TargetId = newoa.Id;
                    sf.Sequence = fp.Sequence;

                    newoa.FilterParts.Add(sf);
                }


                foreach (OutboundActivityCurrentActivityFilter ocaf in ActivityFilterParts)
                {
                    if(ocaf.Object!=null)
                       newoa.ActivityFilterParts.Add(ocaf.Object);
                }


                newoa.SystemFilter = SystemFilter;
                newoa.SystemSortOrder = SystemSortOrder;
                newoa.CurrentActivityFilter = CurrentActivityFilter;
                newoa.DataFilter = DataFilter;
                newoa.DataSortOrder = DataSortOrder;
            }

            foreach (OutboundActivitySpecialDayAction oasda in SpecialDayActions)
            {
                newoa.SpecialDayActions.Add(oasda.SpecialDay);
                newoa.SpecialDayActions[newoa.SpecialDayActions.Count() - 1].ClosedActionType = oasda.ClosedActionType;
                newoa.SpecialDayActions[newoa.SpecialDayActions.Count() - 1].ClosedParam = oasda.ClosedParam;

            }

            foreach (OutboundActivityTimeSpanAction oatsa in TimeSpanActions)
            {
                newoa.TimeSpanActions.Add(oatsa.PlanningTimeSpan);
                newoa.TimeSpanActions[newoa.TimeSpanActions.Count() - 1].ClosedActionType = oatsa.ClosedActionType;
                newoa.TimeSpanActions[newoa.TimeSpanActions.Count() - 1].ClosedParam = oatsa.ClosedParam;
            }


            if (attachToCampaign)
                newoa.Campaign.Target.OutboundActivities.Add(newoa);

            newoa.SecurityContext.TargetId = SecurityContext.TargetId;

            newoa.AdminCore.OutboundActivities.Add(newoa);

            DuplicateSecurity(newoa);

            return newoa;
        }

        public IMediaProviderConfigurator BlackListProviderConfig
        {
            get
            {
                try
                {
                    string strProviderId = BlackListProvider.SelectSingleNode("Root/Config").Attributes["providerid"].Value;
                    MediaProvider mediaProvider = null;
                    foreach (MediaProvider mp in m_Core.MediaProviders)
                    {
                        if (mp.Id.Equals(strProviderId))
                        {
                            mediaProvider = mp;
                            break;
                        }
                    }
                    if (mediaProvider != null)
                    {
                        IMediaProviderConfigurator prov = Activator.CreateInstance(Type.GetType(mediaProvider.PluginType)) as IMediaProviderConfigurator;
                        prov.InitializeProvider(m_Core, mediaProvider);
                        prov.Config = BlackListProvider;
                        return prov;
                    }
                }
                catch
                {
                }
                return null;
            }
        }

        public string BlackListProviderId
        {
            get
            {
                try
                {
                    if (BlackListProvider == null)
                        return null;

                    return BlackListProvider.SelectSingleNode("Root/Config").Attributes["providerid"].Value;
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    string strProviderId = null;
                    try
                    {
                        strProviderId = BlackListProvider.SelectSingleNode("Root/Config").Attributes["providerid"].Value;
                    }
                    catch
                    {
                    }
                    if (strProviderId != value)
                    {
                        strProviderId = value;
                        MediaProvider mediaProvider = null;
                        foreach (MediaProvider mp in m_Core.MediaProviders)
                        {
                            if (mp.Id.Equals(strProviderId))
                            {
                                mediaProvider = mp;
                                break;
                            }
                        }
                        if (mediaProvider != null)
                        {
                            IMediaProviderConfigurator prov = Activator.CreateInstance(Type.GetType(mediaProvider.PluginType)) as IMediaProviderConfigurator;
                            prov.InitializeProvider(m_Core, mediaProvider);
                            BlackListProvider = prov.Config;
                        }
                        else
                        {
                            BlackListProvider = null;
                        }
                    }
                }
                catch
                {
                }

            }
        }

        public XmlDocument BlackListProvider
        {
            get
            {
                return GetFieldValue<XmlDocument>("BlackListProvider");
            }
            set
            {
                SetFieldValue<XmlDocument>("BlackListProvider", value);

            }
        }

        public AdminObjectReference<Prompt> AmdPrompt
        {
            get
            {
                return m_AmdPrompt;
            }
            set
            {
                if (m_AmdPrompt != null)
                {
                    m_AmdPrompt.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_AmdPrompt_PropertyChanged);
                    m_AmdPrompt.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_AmdPrompt_PropertyChanged);
                }

                m_AmdPrompt = value;

                if (m_AmdPrompt != null)
                {
                    m_AmdPrompt.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_AmdPrompt_PropertyChanged);
                    m_AmdPrompt.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_AmdPrompt_PropertyChanged);
                }

                m_AmdPrompt_PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("AmdPrompt"));
            }
        }

        protected virtual void m_AmdPrompt_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        public string SalesforceCampaign
        {
            get
            {
                if (Campaign.Target.CustomConfig == null)
                    return null;
                try
                {
                    XmlNode nde = Campaign.Target.CustomConfig.SelectSingleNode("/datasources/datasource/@source" + Id);
                    if(nde!=null)
                        return nde.Value;
                }
                catch
                {
                }
                return null;
            }
            set
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Campaign.Target.CustomConfig.OuterXml);
                Campaign.Target.CustomConfig = doc;


                XmlNode node = Campaign.Target.CustomConfig.SelectSingleNode("/datasources/datasource");
                XmlAttribute att = null;
                if (node.Attributes["source" + Id] != null)
                    att = node.Attributes["source" + Id];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("source" + Id);
                    node.Attributes.Append(att);
                }

                att.Value = value;


                Campaign.Target.FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesforceCampaign");

            }
        }

        public string SalesforceCampaignDescription
        {
            get
            {
                if (Campaign.Target.CustomConfig == null)
                    return null;
                try
                {
                    XmlNode nde = Campaign.Target.CustomConfig.SelectSingleNode("/datasources/datasource/@sourcedescription" + Id);
                    if(nde!=null)
                        return nde.Value;
                }
                catch
                {
                }
                return null;
            }
            set
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Campaign.Target.CustomConfig.OuterXml);
                Campaign.Target.CustomConfig = doc;

                XmlNode node = Campaign.Target.CustomConfig.SelectSingleNode("/datasources/datasource");
                XmlAttribute att = null;
                if (node.Attributes["sourcedescription" + Id] != null)
                    att = node.Attributes["sourcedescription" + Id];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("sourcedescription" + Id);
                    node.Attributes.Append(att);
                }

                att.Value = value;

                Campaign.Target.FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesforceCampaignDescription");
            }
        }

        public SalesforceCampaignMode SalesforceMode
        {
            get
            {
                if (Campaign.Target.CustomConfig == null)
                    return SalesforceCampaignMode.CampaignMembers;
                try
                {
                    XmlNode nde = Campaign.Target.CustomConfig.SelectSingleNode("/datasources/datasource/@type" + Id);
                    if (nde != null)
                    {
                        if (nde.Value == "ContactRoute.Dialer.SFContactsProvider, SFContactsProvider")
                            return SalesforceCampaignMode.CampaignMembers;
                        else
                            return SalesforceCampaignMode.Activities;
                    }
                }
                catch
                {
                }
                return SalesforceCampaignMode.CampaignMembers;
            }
            set
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Campaign.Target.CustomConfig.OuterXml);
                Campaign.Target.CustomConfig = doc;


                XmlNode node = Campaign.Target.CustomConfig.SelectSingleNode("/datasources/datasource");
                XmlAttribute att = null;
                if (node.Attributes["type" + Id] != null)
                    att = node.Attributes["type" + Id];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("type" + Id);
                    node.Attributes.Append(att);
                }

                if (value == SalesforceCampaignMode.CampaignMembers)
                {
                    att.Value = "ContactRoute.Dialer.SFContactsProvider, SFContactsProvider";
                }
                else
                {
                    att.Value = "ContactRoute.Dialer.SFContactsProvider2, SFContactsProvider";
                }


                Campaign.Target.FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesforceMode");

            }
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
