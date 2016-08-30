using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nixxis;
using ContactRoute;

namespace Nixxis.Client.Admin
{
    public enum SystemMapping
    {
        None = 0,
        Fax = 1,
        Disturbed = 2,
        NoAnswer = 3,
        AnsweringMachine = 4,
        Busy = 5
    }

    public class Qualification: AdminObject
    {

        private static TranslationContext m_TranslationContext = new TranslationContext("Qualifications");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        internal override bool Reload(System.Xml.XmlNode node)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Reloading qualification {0}", node.Attributes["id"].Value), "###");
            if (base.Reload(node))
            {

                if (MainCampaign != null)
                    MainCampaign.Qualifications.Reload(node);

                if (ParentQualification != null)
                {
                    if (!ParentQualification.Children.Contains(this))
                    {
                        ParentQualification.Children.Add(this);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(ParentId))
                        return false;
                }

                return true;
            }
            return false;
        }

        internal override void Delete()
        {
            if (MainCampaign != null && MainCampaign.Qualifications!=null)
                MainCampaign.Qualifications.RemoveId(Id);

            if (ParentId!=null && ParentQualification != null)
            {
                if (!ParentQualification.Children.Contains(this))
                {
                    ParentQualification.Children.RemoveId(Id);
                }
            }

            base.Delete();



        }

        public Qualification(AdminCore core)
            : base(core)
        {
            Init();
        }


        public Qualification(AdminObject parent)
            : base(parent)
        {
            Init();
        }


        private void Init()
        {
            IgnoreMaxDialAttempts = false;
            Exportable = true;
            TriggerHangup = false;
            Argued = false;
            Positive = 0;
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
            }
        }

        public bool Argued
        {
            get
            {
                return GetFieldValue<bool>("Argued");
            }
            set
            {
                SetFieldValue<bool>("Argued", value);
                FirePropertyChanged("Explanation");
            }
        }

        public int Positive
        {
            get
            {
                return GetFieldValue<int>("Positive");
            }
            set
            {
                SetFieldValue<int>("Positive", value);
                FirePropertyChanged("Explanation");
                FirePropertyChanged("IsStdPositive");
                FirePropertyChanged("IsStdNeutral");
                FirePropertyChanged("IsStdNegative");
                FirePropertyChanged("IsNotPositive");
                if (value > 0)
                {
                    Argued = true;
                }
            }
        }

        public bool PositiveUpdatable
        {
            get
            {
                return GetFieldValue<bool>("PositiveUpdatable");
            }
            set
            {
                SetFieldValue<bool>("PositiveUpdatable", value);
            }
        }

        public int DisplayOrder
        {
            get
            {
                return GetFieldValue<int>("DisplayOrder");
            }
            set
            {
                SetFieldValue<int>("DisplayOrder", value);
                FirePropertyChanged("IsFirst");
                FirePropertyChanged("IsLast");                
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public QualificationAction Action
        {
            get
            {
                return GetFieldValue<QualificationAction>("Action");
            }
            set
            {
                SetFieldValue<QualificationAction>("Action", value);
                if (value == QualificationAction.RetryAt || value == QualificationAction.RetryNotBefore)
                {
                    if (Delay == 0)
                        Delay = 7200;
                }
                FirePropertyChanged("Explanation");
                FirePropertyChanged("OtherDescription");
            }
        }

        public int Delay
        {
            get
            {
                return GetFieldValue<int>("Delay");
            }
            set
            {
                SetFieldValue<int>("Delay", value);
                FirePropertyChanged("Explanation");
                FirePropertyChanged("OtherDescription");
            }
        }

        public string ActionParameters
        {
            get
            {
                return GetFieldValue<string>("ActionParameters");
            }
            set
            {
                SetFieldValue<string>("ActionParameters", value);
                FirePropertyChanged("Explanation");
                FirePropertyChanged("OtherDescription");
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public SystemMapping SystemMapping
        {
            get
            {
                return GetFieldValue<SystemMapping>("SystemMapping");
            }
            set
            {
                SetFieldValue<SystemMapping>("SystemMapping", value);
            }
        }

        public Qualification Root
        {
            get
            {
                if (m_Core == null)
                    return null;
                Qualification qual = this;
                while (!string.IsNullOrEmpty(qual.ParentId))
                    qual = m_Core.GetAdminObject<Qualification>(qual.ParentId);
                return qual;
            }
        }

        public Campaign MainCampaign
        {
            get
            {
                try
                {
                    if (m_Core != null)
                    {
                        IEnumerable<Campaign> temp = m_Core.Campaigns.Where((camp) => (camp.Qualification.TargetId == Root.Id));
                        if (temp.Count() > 0)
                            return temp.First();
                    }
                }
                catch
                {
                }
                if(Parent!=null && Parent is Campaign)
                    return (Campaign)Parent;
                else if (Parent != null && Parent is AdminObjectList<Qualification>)
                {
                    if (Parent.Parent != null)
                    {
                        if (Parent.Parent is Qualification)
                            return ((Qualification)Parent.Parent).MainCampaign;
                        else if (Parent.Parent is Campaign)
                            return ((Campaign) Parent.Parent);
                    }
                }
                return null;
            }
        }
        public string CustomValue
        {
            get
            {
                return GetFieldValue<string>("CustomValue");
            }
            set
            {
                SetFieldValue<string>("CustomValue", value);
            }
        }

        public string NewActivity
        {
            get
            {
                return GetFieldValue<string>("NewActivity");
            }
            set
            {
                SetFieldValue<string>("NewActivity", value);
                FirePropertyChanged("Explanation");
                FirePropertyChanged("Explanation");
            }
        }

        public bool Exportable
        {
            get
            {
                return GetFieldValue<bool>("Exportable");
            }
            set
            {
                SetFieldValue<bool>("Exportable", value);
                FirePropertyChanged("OtherDescription");
            }
        }
        public bool TriggerHangup
        {
            get
            {
                return GetFieldValue<bool>("TriggerHangup");
            }
            set
            {
                SetFieldValue<bool>("TriggerHangup", value);
                FirePropertyChanged("OtherDescription");
            }
        }

        public bool IgnoreMaxDialAttempts
        {
            get
            {
                return GetFieldValue<bool>("IgnoreMaxDialAttempts");
            }
            set
            {
                SetFieldValue<bool>("IgnoreMaxDialAttempts", value);
                FirePropertyChanged("OtherDescription");
            }
        }

        public string ParentId
        {
            get
            {
                return GetFieldValue<string>("ParentId");
            }
            set
            {
                SetFieldValue<string>("ParentId", value);
            }
        }

        [AdminLoad(Path = "/Admin/Qualifications/Qualification[@parentid=\"{0}\"]")]
        public AdminObjectList<Qualification> Children
        {
            get;
            internal set;
        }

        [AdminLoad(Path = "/Admin/QualificationsExclusions/QualificationExclusion[@qualificationid=\"{0}\"]")]
        public AdminObjectList<QualificationExclusion> ActivitiesExclusions
        {
            get;
            internal set;
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
                FirePropertyChanged("OtherDescription");
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
                FirePropertyChanged("OtherDescription");
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
                FirePropertyChanged("OtherDescription");

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
                FirePropertyChanged("OtherDescription");
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


        public object CheckedActivitiesExclusions
        {
            get
            {
                if (MainCampaign != null)
                {
                    AdminCheckedLinkList<Activity, QualificationExclusion> retValue = new AdminCheckedLinkList<Activity, QualificationExclusion>(MainCampaign.Activities, ActivitiesExclusions, this);
                    return retValue;
                }
                return null;
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

        public override string  TypedDisplayText
        {
            get
            {
                return string.Concat("Qualification ", DisplayText);
            }
        }


        public string OtherDescription
        {
            get
            {
                List<string> lstAttributes = new List<string>();

                switch (Action)
                {
                    case QualificationAction.None:
                        break;
                    case QualificationAction.BlackList:
                        lstAttributes.Add(TranslationContext.Default.Translate("Using blacklist action"));
                        break;
                    case QualificationAction.Callback:
                        lstAttributes.Add(TranslationContext.Default.Translate("Requesting a callback"));
                        break;
                    case QualificationAction.ChangeActivity:
                        if (!string.IsNullOrWhiteSpace(ActionParameters))
                        {
                            lstAttributes.Add(string.Format(TranslationContext.Default.Translate("Changing activity to {0}"), m_Core.GetAdminObject(ActionParameters).DisplayText));
                        }
                        break;
                    case QualificationAction.DoNotRetry:
                        lstAttributes.Add(TranslationContext.Default.Translate("Not generating further retry"));
                        break;
                    case QualificationAction.RetryAt:
                        lstAttributes.Add(String.Format(TranslationContext.Default.Translate("Generating a retry {0} later"),  DurationHelpers.GetDefaultDurationString(Delay, false)));
                        break;
                    case QualificationAction.RetryNotBefore:
                        lstAttributes.Add(String.Format(TranslationContext.Default.Translate("Generating a retry not before {0}"), DurationHelpers.GetDefaultDurationString(Delay, false)));
                        break;
                    case QualificationAction.TargetedCallback:
                        lstAttributes.Add(TranslationContext.Default.Translate("Requesting a targeted callback"));
                        break;
                }

                if (!string.IsNullOrWhiteSpace(NewActivity))
                {
                    try
                    {
                        lstAttributes.Add(string.Format(TranslationContext.Default.Translate("Modifying activity to {0}"), m_Core.GetAdminObject(NewActivity).DisplayText));
                    }
                    catch
                    {
                    }
                }

                if (Exportable)
                    lstAttributes.Add(TranslationContext.Default.Translate("Exportable"));
                else
                    lstAttributes.Add(TranslationContext.Default.Translate("Not exportable"));
                
                if (TriggerHangup)
                    lstAttributes.Add(TranslationContext.Default.Translate("Hangup"));

                if (IgnoreMaxDialAttempts)
                    lstAttributes.Add(TranslationContext.Default.Translate("Ignore max. dial attempts"));

                if(!string.IsNullOrEmpty(PostWrapupBehaviorDescription))
                    lstAttributes.Add(PostWrapupBehaviorDescription);

                return string.Join(". ", lstAttributes);
            }
        }

        public string Explanation
        {
            get
            {
                List<string> lstAttributes = new List<string>();

                if (Argued)
                {
                    lstAttributes.Add(TranslationContext.Default.Translate("Argued"));
                }

                if (Positive > 0)
                {
                    lstAttributes.Add(TranslationContext.Default.Translate("Positive"));
                }
                else if (Positive < 0)
                {
                    lstAttributes.Add(TranslationContext.Default.Translate("Negative"));
                }
                else
                {
                    lstAttributes.Add(TranslationContext.Default.Translate("Neutral"));
                }

                switch (Action)
                {
                    case QualificationAction.None:
                        lstAttributes.Add(TranslationContext.Default.Translate("Using default action"));
                        break;
                    case QualificationAction.BlackList:
                        lstAttributes.Add(TranslationContext.Default.Translate("Using blacklist action"));
                        break;
                    case QualificationAction.Callback:
                        lstAttributes.Add(TranslationContext.Default.Translate("Requesting a callback"));
                        break;
                    case QualificationAction.ChangeActivity:
                        if (!string.IsNullOrWhiteSpace(ActionParameters))
                        {
                            lstAttributes.Add(string.Format(TranslationContext.Default.Translate("Changing activity to {0}"), m_Core.GetAdminObject(ActionParameters).DisplayText));
                        }
                        break;
                    case QualificationAction.DoNotRetry:
                        lstAttributes.Add(TranslationContext.Default.Translate("Not generating further retry"));
                        break;
                    case QualificationAction.RetryAt:
                        lstAttributes.Add(String.Format(TranslationContext.Default.Translate("Generating a retry {0} later"), DurationHelpers.GetDefaultDurationString(Delay*60, false)));
                        break;
                    case QualificationAction.RetryNotBefore:
                        lstAttributes.Add(String.Format(TranslationContext.Default.Translate("Generating a retry not before {0}"), DurationHelpers.GetDefaultDurationString(Delay * 60, false)));
                        break;
                    case QualificationAction.TargetedCallback:
                        lstAttributes.Add(TranslationContext.Default.Translate("Requesting a targeted callback"));
                        break;
                }

                if (!string.IsNullOrWhiteSpace(NewActivity))
                {
                    lstAttributes.Add(string.Format(TranslationContext.Default.Translate("Modifying activity to {0}"), m_Core.GetAdminObject(ActionParameters).DisplayText));
                }

                return string.Join(", ", lstAttributes);
            }
        }

        public bool IsStdPositive
        {
            get
            {
                return Positive == 1;
            }
            set
            {
                if (value)
                    Positive = 1;
            }
        }
        public bool IsStdNeutral
        {
            get
            {
                return Positive == 0;
            }
            set
            {
                if(value)
                    Positive = 0;
            }
        }
        public bool IsStdNegative
        {
            get
            {
                return Positive == -1;
            }
            set
            {
                if (value)
                    Positive = -1;
            }
        }
        public bool IsNotPositive
        {
            get
            {
                return (Positive <= 0);
            }
        }

        public Qualification ParentQualification
        {
            get
            {
                if(m_Core==null)
                    return null;
                return m_Core.GetAdminObject<Qualification>(ParentId);
            }
        }

        public bool IsFirst
        {
            get
            {
                return ParentQualification.Children.Min((a) => (a==null ? int.MaxValue : a.DisplayOrder)) == DisplayOrder;
            }
        }

        public bool IsLast
        {
            get
            {

                return ParentQualification.Children.Max((a) => (a == null ? int.MinValue : a.DisplayOrder)) == DisplayOrder;
            }
        }

        public Qualification Previous
        {
            get
            {
                if (ParentQualification == null)
                    return null;
                int prevSequence = ParentQualification.Children.Max((a) => (a.DisplayOrder < DisplayOrder ? a.DisplayOrder : -1));
                return ParentQualification.Children.FirstOrDefault((a) => (a.DisplayOrder == prevSequence));
            }
        }

        public Qualification Next
        {
            get
            {
                if (ParentQualification == null)
                    return null;
                int nextSequence = ParentQualification.Children.Min((a) => (a.DisplayOrder > DisplayOrder ? a.DisplayOrder : int.MaxValue));
                return ParentQualification.Children.FirstOrDefault((a) => (a.DisplayOrder == nextSequence));

            }
        }

        public void Remove()
        {
            // remove all children
            while (Children.Count!=0)
            {
                int countBefore = Children.Count;
                Children[0].Remove();
                if (Children.Count == countBefore)
                {
                    if (Children.Count > 0)
                        Children.RemoveAt(0);
                }
            }


            this.MainCampaign.Qualifications.Remove(this);
            m_Core.Delete(this);
        }
        
        public void Exclude(Activity act)
        {
            foreach (Qualification q in Children)
            {
                q.Exclude(act);
            }
            
            // TODO: check, this one will not work because unmatching of type Activity and OutboundActivity when executing "AddLinkSide"            
            // Workaround: add both sides           
            ActivitiesExclusions.Add(act);
            act.QualificationsExclusions.Add(this);

            Qualification qual = null;

            if (ParentId != null)
                qual = ParentQualification;

            while (qual != null)
            {
                bool temp = false;
                foreach(Qualification qu in qual.Children)
                {
                    if (qu.ActivitiesExclusions.Count((qe) => (qe.Activity == act)) != 1)
                    {
                        temp = true;
                        break;
                    }
                }
                if (!temp)
                {
                    qual.ActivitiesExclusions.Add(act);
                    act.QualificationsExclusions.Add(qual);
                }

                if (qual.ParentId != null)
                    qual = qual.ParentQualification;
                else
                    qual = null;
            }

        }

        public void Include(Activity act)
        {
            Qualification qual = this;
            while(qual!=null)
            {
                // see method Exclude(...)
                qual.ActivitiesExclusions.Remove(act);
                act.QualificationsExclusions.Remove(this);

                if (qual.ParentId != null)
                    qual = qual.ParentQualification;
                else
                    qual = null;
            }
        }

        internal override bool? Is_Deletable(string rightid)
        {
            try
            {
                return this.MainCampaign.Is_Deletable(rightid);
            }
            catch
            {
            }
            return base.Is_Deletable(rightid);
        }

        public override bool? Is_Listable(string rightid)
        {
            try
            {
                return this.MainCampaign.Is_Listable(rightid);
            }
            catch
            {
            }
            return base.Is_Listable(rightid);

        }

        internal override bool? Is_Modifiable(string rightid)
        {
            try
            {
                return this.MainCampaign.Is_Modifiable(rightid);
            }
            catch
            {
            }
            return base.Is_Modifiable(rightid);

        }

        internal override bool? Is_RighstHandlingAllowed(string rightid)
        {
            try
            {
                return this.MainCampaign.Is_RighstHandlingAllowed(rightid);
            }
            catch
            {
            }
            return base.Is_RighstHandlingAllowed(rightid);

        }
        public override bool? Is_Visible(string rightid)
        {
            try
            {
                return this.MainCampaign.Is_Visible(rightid);
            }
            catch
            {
            }
            return base.Is_Visible(rightid);

        }


        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            try
            {
                MainCampaign.EmptySave(doc);
            }
            catch
            {
            }
            return base.CreateSaveNode(doc, operation);
        }
    }

    [AdminListSort("SequenceNumber")]
    public class QualificationsCollecion : AdminObjectList<Qualification>
    {
        public QualificationsCollecion(AdminCore core)
            : base(core)
        {
        }

        public QualificationsCollecion(AdminObject parent)
            : base(parent)
        {
        }

        internal QualificationsCollecion(AdminObject parent, bool canClone, bool preloadChildren)
            : base(parent, canClone, preloadChildren)
        {
        }

        public void Reload(string str)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Reloading qualification collection from campaign ({0})", str), "###");
            if (!ContainsId(str))
            {
                AddId(str);
                Parent.FirePropertyChanged("Qualifications");
                Parent.FirePropertyChanged("Qualification");
            }

        }

        internal override bool Reload(System.Xml.XmlNode node)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Reloading qualificatios collection ({0})", node.Attributes["id"].Value), "###");
            if (base.Reload(node))
            {
                if (!ContainsId(node.Attributes["id"].Value))
                {
                    AddId(node.Attributes["id"].Value);
                    Parent.FirePropertyChanged("Qualifications");
                    Parent.FirePropertyChanged("Qualification");
                    Parent.FirePropertyChanged("Qualification");
                }
                return true;
            }
            return false;
            
        }

        internal override void Load(System.Xml.XmlElement node, System.Reflection.PropertyInfo pInfo)
        {
            if (node == null)
                return;
            System.Xml.XmlNode n = node.SelectSingleNode("Qualification");
            if (n == null)
                return;

            string qualificationid = n.InnerXml;

            if (string.IsNullOrEmpty(qualificationid))
                return;

            Qualification Root = m_Core.GetAdminObject<Qualification>(qualificationid);

            if (Root != null)
            {
                AddId(Root.Id);
                LoadChildren(/*node,*/ Root.Id);
            }
        }

        internal void LoadChildren(/*System.Xml.XmlElement node,*/ string qualificationId)
        {
            foreach (System.Xml.XmlElement nde in m_Core.GetXmlNodesByParentId(qualificationId).Where( (n) => (Int16.Parse( n.Attributes["state"].Value)>0)) )
            {
                Qualification qual = m_Core.GetAdminObject<Qualification>(nde.Attributes["id"].Value);

                if (qual != null)
                {
                    qual.ParentId = qualificationId;
                    AddId(qual.Id);
                    LoadChildren(/*node,*/ qual.Id);
                }

            }
        }
    }

}
