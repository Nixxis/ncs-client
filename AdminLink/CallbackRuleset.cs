using System.Linq;
using Nixxis;
using System.Xml;
using ContactRoute;

namespace Nixxis.Client.Admin
{
    public class CallbackRuleset : AdminObject
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
                return GetFieldValue<string>("Description");
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
                    return string.Concat("Callbacks ruleset ", Description);
            }
        }


        public CallbackRuleset(AdminObject parent)
            : base(parent)
        {
        }

        public CallbackRuleset(AdminCore core)
            : base(core)
        {
        }

        public int CallbackValidity
        {
            get
            {
                return GetFieldValue<int>("CallbackValidity");
            }
            set
            {
                SetFieldValue<int>("CallbackValidity", value);
            }
        }

        public int MaxDialAttempts
        {
            get
            {
                return GetFieldValue<int>("MaxDialAttempts");
            }
            set
            {
                SetFieldValue<int>("MaxDialAttempts", value);
            }
        }

        public bool MaxDialAttemptsLimited
        {
            get
            {
                return (MaxDialAttempts > 0);
            }
            set
            {
                if (value)
                {
                    MaxDialAttempts = 10;
                }
                else
                {
                    MaxDialAttempts = -1;
                }
            }
        }

        [AdminLoad(Path = "/Admin/CallbackRules/CallbackRule[@callbackrulesetid=\"{0}\"]")]
        public AdminObjectList<CallbackRule> Rules
        {
            get;
            internal set;
        }

        public CallbackRuleset Duplicate()
        {
            CallbackRuleset newcbrs = AdminCore.Create<CallbackRuleset>();

            newcbrs.Description = string.Concat(Description, "*");

            newcbrs.GroupKey = GroupKey;

            newcbrs.CallbackValidity = CallbackValidity;

            newcbrs.MaxDialAttempts = MaxDialAttempts;

            foreach (CallbackRule cbr in Rules)
            {
                CallbackRule newcbr = cbr.AdminCore.Create<CallbackRule>();

                newcbr.Action = cbr.Action;
                newcbr.Callback = cbr.Callback;
                newcbr.CallbackRuleset.TargetId = newcbrs.Id;
                newcbr.ConsecutiveStatusCount = cbr.ConsecutiveStatusCount;
                newcbr.EndReason = cbr.EndReason;
                newcbr.FixedTime = cbr.FixedTime;
                newcbr.ForceProgressive = cbr.ForceProgressive;
                newcbr.DialingModeOverride = cbr.DialingModeOverride;
                newcbr.LooseTarget = cbr.LooseTarget;
                newcbr.RelativeDelay = cbr.RelativeDelay;
                newcbr.Sequence = cbr.Sequence;
                newcbr.Validity = cbr.Validity;

                newcbrs.Rules.Add(newcbr);
            }

            newcbrs.AdminCore.CallbackRulesets.Add(newcbrs);

            return newcbrs;
        }
    }

    public class CallbackRule : AdminObject
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("CallbackRuleset");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        public string Description
        {
            get
            {
                return string.Format(Translate("When call is {0} for at least {1} times, then the action applied is '{2}'"), (new DialDisconnectionReasonHelper()).First((a) => (a.EnumValue == EndReason)).Description, ConsecutiveStatusCount, Action == QualificationAction.RetryAt || Action == QualificationAction.RetryNotBefore ? string.Concat(new QualificationActionHelper().First((a) => (a.EnumValue == Action)).Description, " ", DurationHelpers.GetDefaultDurationString( RelativeDelay * 60, false)) : new QualificationActionHelper().First((a) => (a.EnumValue == Action)).Description);
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
                    return string.Concat("Callbacks rule ", Description);
            }
        }


        public CallbackRule(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public CallbackRule(AdminCore core)
            : base(core)
        {
            Init();
        }

        private void Init()
        {
            FixedTime = 0;
            ForceProgressive = false;
        }

        public int Sequence
        {
            get
            {
                return GetFieldValue<int>("Sequence");
            }
            set
            {
                SetFieldValue<int>("Sequence", value);
                FirePropertyChanged("IsFirst");
                FirePropertyChanged("IsLast");                

            }
        }

        public int ConsecutiveStatusCount
        {
            get
            {
                return GetFieldValue<int>("ConsecutiveStatusCount");
            }
            set
            {
                SetFieldValue<int>("ConsecutiveStatusCount", value);
                FirePropertyChanged("Explanation");
                FirePropertyChanged("ConditionsDescription");
            }
        }

        public bool LooseTarget
        {
            get
            {
                return GetFieldValue<bool>("LooseTarget");
            }
            set
            {
                SetFieldValue<bool>("LooseTarget", value);
            }
        }

        public bool ForceProgressive
        {
            get
            {
                return GetFieldValue<bool>("ForceProgressive");
            }
            set
            {
                SetFieldValue<bool>("ForceProgressive", value);
            }
        }

        public int DialingModeOverride
        {
            get
            {
                return GetFieldValue<int>("DialingModeOverride");
            }
            set
            {
                SetFieldValue<int>("DialingModeOverride", value);
            }
        }


        public int Validity
        {
            get
            {
                return GetFieldValue<int>("Validity");
            }
            set
            {
                SetFieldValue<int>("Validity", value);
            }
        }

        public int RelativeDelay
        {
            get
            {
                return GetFieldValue<int>("RelativeDelay");
            }
            set
            {
                SetFieldValue<int>("RelativeDelay", value);
                FirePropertyChanged("Explanation");
                FirePropertyChanged("ActionDescription");

            }
        }

        public int FixedTime
        {
            get
            {
                return GetFieldValue<int>("FixedTime");
            }
            set
            {
                SetFieldValue<int>("FixedTime", value);
            }
        }

        public bool Callback
        {
            get
            {
                return GetFieldValue<bool>("Callback");
            }
            set
            {
                SetFieldValue<bool>("Callback", value);
            }
        }

        public DialDisconnectionReason EndReason
        {
            get
            {
                return GetFieldValue<DialDisconnectionReason>("EndReason");
            }
            set
            {
                SetFieldValue<DialDisconnectionReason>("EndReason", value);
                FirePropertyChanged("Explanation");
                FirePropertyChanged("ConditionsDescription");

            }
        }

        public QualificationAction Action
        {
            get
            {
                return GetFieldValue<QualificationAction>("Action");
            }
            set
            {
                SetFieldValue<QualificationAction>("Action", value);
                FirePropertyChanged("Explanation");
                FirePropertyChanged("ActionDescription");
            }
        }

        public string Explanation
        {
            get
            {
                return string.Format(Translate("When {0}, {1}"), ConditionsDescription, ActionDescription);
            }
        }
        public string ConditionsDescription
        {
            get
            {
                if (Callback)
                {
                    if (ConsecutiveStatusCount > 0)
                        return string.Format(Translate("callback and {0} for at least {1} times"), new DialDisconnectionReasonHelper().First((a) => (a.EnumValue == EndReason)).Description, ConsecutiveStatusCount);
                    else
                        return string.Format(Translate("callback and {0}"), new DialDisconnectionReasonHelper().First((a) => (a.EnumValue == EndReason)).Description);
                }
                else
                {
                    if (ConsecutiveStatusCount > 0)
                        return string.Format(Translate("{0} for at least {1} times"), new DialDisconnectionReasonHelper().First((a) => (a.EnumValue == EndReason)).Description, ConsecutiveStatusCount);
                    else
                        return new DialDisconnectionReasonHelper().First((a) => (a.EnumValue == EndReason)).Description;
                }
            }
        }
        public string ActionDescription
        {
            get
            {
                return string.Format("{0}{1}", 
                    new QualificationActionHelper().First( (a) => (a.EnumValue == Action) ).Description, 
                    Action==QualificationAction.RetryAt || Action== QualificationAction.RetryNotBefore || Action==QualificationAction.Callback ? string.Concat ( " ", DurationHelpers.GetDefaultDurationString(RelativeDelay * 60, false)): "");
            }
        }

        public CallbackRuleset ParentRuleSet
        {
            get
            {
                if (Parent == null)
                    return null;
                return (CallbackRuleset)(((AdminObjectList<CallbackRule>)Parent).Parent);
            }
        }

        public bool IsFirst
        {
            get
            {
                return ((CallbackRuleset)(((AdminObjectList<CallbackRule>)Parent).Parent)).Rules.Min((a) => (a.Sequence)) == Sequence;
            }
        }

        public bool IsLast
        {
            get
            {
                return ((CallbackRuleset)(((AdminObjectList<CallbackRule>)Parent).Parent)).Rules.Max((a) => (a.Sequence)) == Sequence;
            }
        }

        public CallbackRule Previous
        {
            get
            {
                if (Parent == null || Parent.Parent == null)
                    return null;
                int prevSequence = ((CallbackRuleset)(((AdminObjectList<CallbackRule>)Parent).Parent)).Rules.Max((a) => (a.Sequence < Sequence ? a.Sequence : -1));
                return ((CallbackRuleset)(((AdminObjectList<CallbackRule>)Parent).Parent)).Rules.First((a) => (a.Sequence == prevSequence));
            }
        }

        public CallbackRule Next
        {
            get
            {
                if (Parent == null || Parent.Parent == null)
                    return null;

                int nextSequence = ((CallbackRuleset)(((AdminObjectList<CallbackRule>)Parent).Parent)).Rules.Min((a) => (a.Sequence > Sequence ? a.Sequence : int.MaxValue));
                return ((CallbackRuleset)(((AdminObjectList<CallbackRule>)Parent).Parent)).Rules.First((a) => (a.Sequence == nextSequence));

            }
        }

        [AdminSave(SkipSave = true)]
        public AdminObjectReference<CallbackRuleset> CallbackRuleset
        {
            internal set;
            get;
        }

        public string m_BackupCallbackRule = null;
        public int m_BackupPrecedence = -1;
        public override void Clear()
        {
            if (m_BackupPrecedence == -1)
                m_BackupPrecedence = Sequence;
            if (m_BackupCallbackRule == null)
                m_BackupCallbackRule = CallbackRuleset.TargetId;
            base.Clear();
        }


        protected override XmlElement CreateSaveNode(XmlDocument doc, string operation)
        {
            try
            {
                CallbackRuleset.Target.EmptySave(doc);
            }
            catch
            {
            }

            XmlElement node = base.CreateSaveNode(doc, operation);

            node.RemoveAttribute("id");

            XmlAttribute att = doc.CreateAttribute("callbackrulesetid");
            if (Parent != null)
                att.Value = ParentRuleSet.Id;
            else if (CallbackRuleset.HasTarget)
                att.Value = CallbackRuleset.TargetId;
            else
                att.Value = CallbackRuleset.OriginalTargetId;


            node.Attributes.Append(att);

            // TODO: check, Sequence will be 0 when a delete has been done -> deletion of the wrong item!!!!
            att = doc.CreateAttribute("sequenceid");

            if (operation == "delete")
                att.Value = m_BackupPrecedence.ToString();
            else if (operation == "update")
                att.Value = this.GetOriginalFieldValue<int>("Sequence").ToString();
            else
                att.Value = Sequence.ToString();

            node.Attributes.Append(att);

            return node;
        }

        public CallbackRule Duplicate()
        {
            CallbackRule newcbr = AdminCore.Create<CallbackRule>();

            newcbr.Action = Action;
            newcbr.Callback = Callback;
            newcbr.CallbackRuleset.TargetId = CallbackRuleset.TargetId;
            newcbr.ConsecutiveStatusCount = ConsecutiveStatusCount;
            newcbr.EndReason = EndReason;
            newcbr.FixedTime = FixedTime;
            newcbr.ForceProgressive = ForceProgressive;
            newcbr.DialingModeOverride = DialingModeOverride;
            newcbr.LooseTarget = LooseTarget;
            newcbr.RelativeDelay = RelativeDelay;

            if (CallbackRuleset.Target.Rules.Count > 0)
                newcbr.Sequence = CallbackRuleset.Target.Rules.OrderBy((a) => (a.Sequence)).Last().Sequence + 1;

            newcbr.Validity = Validity;

            CallbackRuleset.Target.Rules.Add(newcbr);
            return newcbr;
        }
    }

}