using System.Collections.ObjectModel;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using ContactRoute;
using System.Xml;
namespace Nixxis.Client.Admin
{
    [EnumDefaultValue(AdministrationLevel.Full)]
    public enum AdministrationLevel
    {
        None = 0,
        ReadOnly = 1,
        ModifyOnly = 2,
        Full = 3,
        Restricted = 4
    }

    [Flags]
    public enum AdministrationRestrictions
    {
        None = 0,
        HumanResources = 1,
        DataManagement = 2
    }

    public class Agent : AdminObject
    {

        private static TranslationContext m_TranslationContext = new TranslationContext("Agents");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        private AdminObjectList<ViewRestriction> m_ViewRestrictions;

        public Agent(AdminCore core)
            : base(core)
        {
            Init();
        }

        public Agent(AdminObject parent)
            : base(parent)
        {
            Init();
        }


        private void Init()
        {
            m_ViewRestrictions = new AdminObjectList<ViewRestriction>(this);
            m_ViewRestrictions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_ViewRestrictions_CollectionChanged);
            PassKey = Core.LoginEncrypter.EncryptPassword(string.Empty, LoginEncryption.Purpose.Storage);
            CustomerVisibilityLevel = 1;
            GuiLanguage = null;// string.Empty;
        }

        void m_ViewRestrictions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FirePropertyChanged("ViewRestrictionsDescription");
            FirePropertyChanged("ViewRestrictionsNeedExpertMode");
            FirePropertyChanged("ViewRestrictionsIsNoRestriction");
            FirePropertyChanged("ViewRestrictionsIsMyTeam");
       }

        public string Account
        {
            get
            {
                return GetFieldValue<string>("Account");
            }
            set
            {
                Agent agt = m_Core.Agents.FirstOrDefault((a) => (a.Account!=null && a.Account.Equals(value) && a.State>0));
                if (agt != null && Account!=null && !Account.Equals(value))
                {
                    throw new InvalidOperationException();
                }

                SetFieldValue("Account", value);

                FirePropertyChanged("TypedDisplayText");
                FirePropertyChanged("DisplayText");
                FirePropertyChanged("ShortDisplayText");
            }
        }

        public string PassKey
        {
            get
            {
                return GetFieldValue<string>("PassKey");
            }
            set
            {
                SetFieldValue("PassKey", value);
            }
        }
        
        public string Description
        {
            get
            {
                return GetFieldValue<string>("Description");
            }
            set
            {
                SetFieldValue("Description", value);

                FirePropertyChanged("DisplayText");
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

        public string FirstName
        {
            get
            {
                return GetFieldValue<string>("FirstName");
            }
            set
            {
                SetFieldValue("FirstName", value);

                FirePropertyChanged("TypedDisplayText");
                FirePropertyChanged("DisplayText");
                FirePropertyChanged("ShortDisplayText");

            }
        }

        public string LastName
        {
            get
            {
                return GetFieldValue<string>("LastName");
            }
            set
            {
                SetFieldValue("LastName", value);

                FirePropertyChanged("TypedDisplayText");
                FirePropertyChanged("DisplayText");
                FirePropertyChanged("ShortDisplayText");

            }
        }

        public string GuiLanguage
        {
            get
            {
                return GetFieldValue<string>("GuiLanguage");
            }
            set
            {
                SetFieldValue("GuiLanguage", value);
            }
        }

        public string CallerIdentification
        {
            get
            {
                return GetFieldValue<string>("CallerIdentification");
            }
            set
            {
                SetFieldValue("CallerIdentification", value);
            }
        }

        public override bool IsDeletable
        {
            get
            {

                if ("defaultagent++++++++++++++++++++".Equals(Id))
                    return false;

                return base.IsDeletable;
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
                return m_IsPostWrapupBehaviorDefined || WrapupTime!=0 || PostWrapupOptionsValue != 0 || AutoReadyDelay!=0 || WrapupExtendable ;
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
                else if(PostWrapupOptionsDecreaseAutoReadyDelay)
                {
                    builder.AppendFormat(Translate("The delay before being ready is decreased by {0}. "), DurationHelpers.GetDefaultDurationString(AutoReadyDelay / 1000, false));
                }
                else if(PostWrapupOptionsSetAutoReadyDelay)
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

        public int AdministrationLevel
        {
            get
            {
                return (int)GetFieldValue<int>("AdministrationLevel");
            }
            set
            {
                SetFieldValue<int>("AdministrationLevel", value);
                FirePropertyChanged("RightsDescription");
                FirePropertyChanged("AdminRightsDescription");
                FirePropertyChanged("AdministrationLevelValue");
                FirePropertyChanged("AdministrationRestrictions");
                FirePropertyChanged("AdministrationRestrictionsHR");
                FirePropertyChanged("AdministrationRestrictionsDM");
            }
        }

        public int ReportingLevel
        {
            get
            {
                return GetFieldValue<int>("ReportingLevel");
            }
            set
            {
                SetFieldValue<int>("ReportingLevel", value);
            }
        }

        public override string TypedDisplayText
        {
            get
            {
                return string.Format("Agent {0}", ShortDisplayText);
            }
        }

        public override string DisplayText
        {
            get
            {
                string retVal = null;
                if (!string.IsNullOrEmpty(Description) && (Description.Contains("{0}") || Description.Contains("{1}") || Description.Contains("{2}")))
                {
                    retVal = string.Format(Description, Account, FirstName, LastName);
                }
                else
                {
                    retVal = string.Format("{0} {1} {2}{3}", Account, FirstName, LastName, string.IsNullOrEmpty(Description) ? string.Empty : string.Format(" :{0}", Description));
                }

                if (State>0)
                {
                    return retVal;
                }
                else
                {
                    return string.Concat("[", retVal, "]");
                }
            }
        }

        public override string ShortDisplayText
        {
            get
            {
                return string.Format("{0} {1} {2}", Account, FirstName, LastName);
            }
        }

        public AdministrationLevel AdministrationLevelValue
        {
            get
            {
                if (AdministrationLevel < ((int)(Nixxis.Client.Admin.AdministrationLevel.Restricted)))
                {
                    return (Nixxis.Client.Admin.AdministrationLevel)AdministrationLevel;
                }
                else
                {
                    return Nixxis.Client.Admin.AdministrationLevel.Restricted;
                }
            }
            set
            {
                AdministrationLevel = (int)value;
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public AdministrationRestrictions AdministrationRestrictions
        {
            get
            {
                if (AdministrationLevel < ((int)(Nixxis.Client.Admin.AdministrationLevel.Restricted)))
                {
                    return Nixxis.Client.Admin.AdministrationRestrictions.None;
                }
                else
                {
                    return (Nixxis.Client.Admin.AdministrationRestrictions)(AdministrationLevel - ((int)Nixxis.Client.Admin.AdministrationLevel.Full));
                }
            }
            set
            {
                if (value != Admin.AdministrationRestrictions.None)
                {
                    AdministrationLevel = ((int)Nixxis.Client.Admin.AdministrationLevel.Full) + ((int)value);
                }
            }
        }

        public string RightsDescription
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                // TODO
                if (true /*|| this.AgentLevel > 0*/)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.Append(TranslationContext.Default.Translate("Agent"));
                }
                if (this.SupervisionLevel > 0)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.Append(TranslationContext.Default.Translate("Supervisor"));
                }
                switch (this.AdministrationLevelValue)
                {
                    case Nixxis.Client.Admin.AdministrationLevel.None:
                        break;
                    case Nixxis.Client.Admin.AdministrationLevel.ReadOnly:
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(TranslationContext.Default.Translate("Read only administrator"));
                        break;
                    case Nixxis.Client.Admin.AdministrationLevel.ModifyOnly:
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(TranslationContext.Default.Translate("Read and modify administrator"));
                        break;
                    case Nixxis.Client.Admin.AdministrationLevel.Restricted:
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(TranslationContext.Default.Translate("Restricted administrator"));
                        break;
                    default:
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(TranslationContext.Default.Translate("Administrator"));
                        break;
                }
                return sb.ToString();
            }
        }

        public string AdminRightsDescription
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                switch (this.AdministrationLevelValue)
                {
                    case Nixxis.Client.Admin.AdministrationLevel.None:
                        break;
                    case Nixxis.Client.Admin.AdministrationLevel.ReadOnly:
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(TranslationContext.Default.Translate("Read only"));
                        break;
                    case Nixxis.Client.Admin.AdministrationLevel.ModifyOnly:
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(TranslationContext.Default.Translate("Read and modify"));
                        break;
                    case Nixxis.Client.Admin.AdministrationLevel.Restricted:
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(TranslationContext.Default.Translate("Restricted"));
                        break;
                    default:
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(TranslationContext.Default.Translate("Full control"));
                        break;
                }
                return sb.ToString();
            }
        }

        public bool AdministrationRestrictionsHR
        {
            get
            {
                return (AdministrationRestrictions & Admin.AdministrationRestrictions.HumanResources) == Admin.AdministrationRestrictions.HumanResources;
            }
            set
            {
                if (value)
                {
                    AdministrationRestrictions = AdministrationRestrictions | Admin.AdministrationRestrictions.HumanResources;
                }
                else
                {
                    AdministrationRestrictions = (AdministrationRestrictions ^ Admin.AdministrationRestrictions.HumanResources) & AdministrationRestrictions;
                }
            }
        }
        public bool AdministrationRestrictionsDM
        {
            get
            {
                return (AdministrationRestrictions & Admin.AdministrationRestrictions.DataManagement) == Admin.AdministrationRestrictions.DataManagement;
            }
            set
            {
                if (value)
                {
                    AdministrationRestrictions = AdministrationRestrictions | Admin.AdministrationRestrictions.DataManagement;
                }
                else
                {
                    AdministrationRestrictions = (AdministrationRestrictions ^ Admin.AdministrationRestrictions.DataManagement) & AdministrationRestrictions;
                }
            }
        }


        public int SupervisionLevel
        {
            get
            {
                return GetFieldValue<int>("SupervisionLevel");
            }
            set
            {
                SetFieldValue<int>("SupervisionLevel", value);
                FirePropertyChanged("RightsDescription");
            }
        }

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

        public int CustomerVisibilityLevel
        {
            get
            {
                return GetFieldValue<int>("CustomerVisibilityLevel");
            }
            set
            {
                SetFieldValue<int>("CustomerVisibilityLevel", value);
            }
        }

        [AdminLoad(Path = "/Admin/RoleMembers/RoleMember[@agentid=\"{0}\"]")]
        public AdminObjectList<RoleMember> Roles
        {
            get;
            internal set;
        }

        [AdminLoad(Path = "/Admin/AgentsTeams/AgentTeam[@agentid=\"{0}\"]")]
        public AdminObjectList<AgentTeam> Teams
        {
            get; internal set;
        }

        [AdminLoad(Path = "/Admin/AgentsSkills/AgentSkill[@agentid=\"{0}\"]")]
        public AdminObjectList<AgentSkill> Skills
        {
            get;
            internal set;
        }

        [AdminLoad(Path = "/Admin/AgentsLanguages/AgentLanguage[@agentid=\"{0}\"]")]
        public AdminObjectList<AgentLanguage> Languages
        {
            get;
            internal set;
        }

        [AdminLoad(Path = "/Admin/ViewRestrictions/ViewRestriction[@agentid=\"{0}\"]")]
        public AdminObjectList<ViewRestriction> ViewRestrictions
        {
            get
            {
                return m_ViewRestrictions;
            }
            internal set
            {
                m_ViewRestrictions = value;
            }
        }



        public string ViewRestrictionsDescription
        {
            get
            {
                if (ViewRestrictionsIsNoRestriction)
                    return TranslationContext.Default.Translate("Without restrictions");
                else if (ViewRestrictionsIsMyTeam)
                    return TranslationContext.Default.Translate("Restricted to own affectations");
                else if (ViewRestrictionsIsMyTeam)
                    return TranslationContext.Default.Translate("Restricted to group");
                else 
                    return TranslationContext.Default.Translate("With restrictions");
            }
        }

        public bool ViewRestrictionsNeedExpertMode
        {
            get
            {
                return !ViewRestrictionsIsNoRestriction && !ViewRestrictionsIsMyTeam && !ViewRestrictionsIsMyGroup;
            }
        }

        public bool ViewRestrictionsIsNoRestriction
        {
            get
            {
                if (ViewRestrictions.Count == 0)
                    return true;
                if (ViewRestrictions.Count == 1)
                {
                    if (ViewRestrictions[0].IsNotRestricted)
                        return true;
                }
                return false;
            }
            set
            {
                if(value)
                    ViewRestrictions.Clear();
            }
        }

        public bool ViewRestrictionsIsMyTeam
        {
            get
            {
                if (ViewRestrictions.Count == 2)
                {
                    ViewRestriction vr1 = ViewRestrictions.OrderBy((a) => (a.Precedence)).First();
                    ViewRestriction vr2 = ViewRestrictions.OrderBy((a) => (a.Precedence)).Last();
                    if ((vr1.IsRestrictedToMyTeam && vr1.Allowed && vr2.TargetType == ViewRestrictionTargetType.Any && !vr2.Allowed) ||
                         (vr2.IsRestrictedToMyTeam && !vr2.Allowed && vr1.TargetType == ViewRestrictionTargetType.Any && vr1.Allowed))
                        return true;
                }
                return false;
            }
            set
            {
                if (value)
                {
                    ViewRestrictions.Clear();
                    ViewRestrictions.Add(ViewRestriction.CreateRestrictedToMyTeam(this));
                    ViewRestriction vr = ViewRestriction.CreateDisallow(this);
                    vr.Precedence = 1;
                    ViewRestrictions.Add(vr);
                }
            }
        }

        public bool ViewRestrictionsIsMyGroup
        {
            get
            {
                if (ViewRestrictions.Count == 1)
                {
                    if (ViewRestrictions[0].IsRestrictedToMyGroup)
                        return true;
                }
                return false;
            }
            set
            {
                if (value)
                {
                    ViewRestrictions.Clear();
                    ViewRestrictions.Add(ViewRestriction.CreateRestrictedToMyGroup(this));
                }
            }
        }

        public object CheckedSkills
        {
            get
            {
                return new AdminCheckedLinkList<Skill, AgentSkill>(m_Core.Skills, Skills, this);
            }
        }

        public object CheckedRoles
        {
            get
            {
                return new AdminCheckedLinkList<Role, RoleMember>(m_Core.Roles, Roles, this);
            }
        }

        public object CheckedTeams
        {
            get
            {
                return new AdminCheckedLinkList<Team, AgentTeam>(m_Core.Teams, Teams, this);
            }
        }

        public object CheckedLanguages
        {
            get
            {
                return new AdminCheckedLinkList<Language, AgentLanguage>(m_Core.Languages, Languages, this);
            }
        }

        internal override bool Reload(System.Xml.XmlNode node)
        {
            if (base.Reload(node))
            {
                SetFieldLoaded("PassKey");
                return true;
            }
            return false;
        }

        public Agent Duplicate()
        {
            Agent agt = AdminCore.Create<Agent>();

            agt.Account = AdminCore.DefaultAgentAccount;
            agt.FirstName = FirstName;
            agt.LastName = LastName;
            agt.Description = Description;
            agt.AdministrationLevel = AdministrationLevel;
            agt.CallerIdentification = CallerIdentification;
            agt.CustomerVisibilityLevel = CustomerVisibilityLevel;
            agt.GroupKey = GroupKey;
            agt.GuiLanguage = GuiLanguage;
            agt.PassKey = PassKey;
            agt.RecordingPlaybackLevel = RecordingPlaybackLevel;
            agt.ReportingLevel = ReportingLevel;
            agt.SupervisionLevel = SupervisionLevel;
            agt.WrapupExtendable = WrapupExtendable;
            agt.WrapupTime = WrapupTime;
            agt.AutoReadyDelay = AutoReadyDelay;
            agt.PostWrapupOptionsValue = PostWrapupOptionsValue;

            agt.Core.Agents.Add(agt);

            foreach (AgentTeam t in Teams)
            {
                agt.Teams.Add(t.Team);
                agt.Teams[agt.Teams.Count - 1].BaseLevel = t.BaseLevel;
            }

            foreach (AgentSkill s in Skills)
            {
                agt.Skills.Add(s.Skill);
                agt.Skills[agt.Skills.Count - 1].Level = s.Level;
            }

            foreach (AgentLanguage l in Languages)
            {
                agt.Languages.Add(l.Language);
                agt.Languages[agt.Languages.Count - 1].Level = l.Level;
            }

            foreach (ViewRestriction vr in ViewRestrictions)
            {
                ViewRestriction newvr = agt.Core.Create<ViewRestriction>();
                newvr.TargetType = vr.TargetType;
                newvr.Target.TargetId = vr.Target.TargetId;
                newvr.IncludeChildren = vr.IncludeChildren;
                newvr.Allowed = vr.Allowed;
                newvr.InformationLevel = vr.InformationLevel;
                newvr.Precedence = vr.Precedence;
                agt.ViewRestrictions.Add(newvr);
            }

            foreach (RoleMember rm in Roles)
            {
                agt.Roles.Add(rm.Role);
            }

            return agt;
        }




        protected override XmlElement CreateSaveNode(XmlDocument doc, string operation)
        {
            if (GuiLanguage == "NullLanguage++++++++++++++++++++")
                GuiLanguage = null;
            return base.CreateSaveNode(doc, operation);
        }

        internal override void Load(XmlElement node)
        {
            base.Load(node);
            if (string.IsNullOrEmpty(GuiLanguage))
            {
                GuiLanguage = "NullLanguage++++++++++++++++++++";
                DoneLoading();
            }

        }



        public override bool? Is_Visible(string rightid)
        {
            bool? tempResult = base.Is_Visible(rightid);

            foreach (AgentTeam t in this.Teams)
            {
                bool? result = t.Team.Is_Visible(rightid);
                if (result != null)
                {
                    if (result.Value)
                    {
                        tempResult = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return tempResult;
        }

        public override bool? Is_Listable(string rightid)
        {
            bool? tempResult = base.Is_Listable(rightid);

            foreach (AgentTeam t in this.Teams)
            {
                bool? result = t.Team.Is_Listable(rightid);
                if (result != null)
                {
                    if (result.Value)
                    {
                        tempResult = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return tempResult;
        }

        internal override bool? Has_FullControlFlag(string rightid)
        {
            bool? tempResult = base.Has_FullControlFlag(rightid);

            foreach (AgentTeam t in this.Teams)
            {
                bool? result = t.Team.Has_FullControlFlag(rightid);
                if (result != null)
                {
                    if (result.Value)
                    {
                        tempResult = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return tempResult;
        }

        internal override bool? Has_PowerFlag(string rightid)
        {
            bool? tempResult = base.Has_PowerFlag(rightid);

            foreach (AgentTeam t in this.Teams)
            {
                bool? result = t.Team.Has_PowerFlag(rightid);
                if (result != null)
                {
                    if (result.Value)
                    {
                        tempResult = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return tempResult;
        }

        internal override bool? Is_Modifiable(string rightid)
        {
            bool? tempResult = base.Is_Modifiable(rightid);

            foreach (AgentTeam t in this.Teams)
            {
                bool? result = t.Team.Is_Modifiable(rightid);
                if (result != null)
                {
                    if (result.Value)
                    {
                        tempResult = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return tempResult;
        }

        internal override bool? Is_Deletable(string rightid)
        {
            bool? tempResult = base.Is_Deletable(rightid);

            foreach (AgentTeam t in this.Teams)
            {
                bool? result = t.Team.Is_Deletable(rightid);
                if (result != null)
                {
                    if (result.Value)
                    {
                        tempResult = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return tempResult;
        }

        internal override bool? Is_RighstHandlingAllowed(string rightid)
        {
            bool? tempResult = base.Is_RighstHandlingAllowed(rightid);

            foreach (AgentTeam t in this.Teams)
            {
                bool? result = t.Team.Is_RighstHandlingAllowed(rightid);
                if (result != null)
                {
                    if (result.Value)
                    {
                        tempResult = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return tempResult;
        }


    }

}