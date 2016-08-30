using System;
using System.IO;
using System.Text;
using System.Configuration;
using System.Xml;

namespace Nixxis.Client.Admin
{
    [AdminObjectLinkCascadeAttribute(typeof(InboundActivity), "TimeSpanActions")]
    [AdminObjectLinkCascadeAttribute(typeof(PlanningTimeSpan), "InboundActivities")]
    public class InboundActivityTimeSpanAction : AdminObjectLink<InboundActivity, PlanningTimeSpan>
    {
        public InboundActivityTimeSpanAction(AdminObject parent)
            : base(parent)
        {
            OverflowPreprocessor = new AdminObjectReference<Preprocessor>(this);
            OverflowMessage = new AdminObjectReference<Prompt>(this);
            OverflowReroutePrompt = new AdminObjectReference<Prompt>(this);
            OverflowActionType = OverflowActions.None;
        }

        internal override void Load(XmlElement node)
        {
            base.Load(node);

            if (OverflowPreprocessor.HasTarget && OverflowPreprocessor.Target.ConfigType != null)
            {
                OverflowPreprocessorConfig = (BasePreprocessorConfig)Core.Create(OverflowPreprocessor.Target.ConfigType, this, string.Concat(InboundActivityId, PlanningTimeSpanId));
                OverflowPreprocessorConfig.DeserializeFromText(OverflowPreprocessorParams);
                OverflowPreprocessorConfig.saveToTextStorage = ((t) => { OverflowPreprocessorParams = t; });
            }

        }

        public string InboundActivityId
        {
            get
            {
                return Id1;
            }
        }

        public string PlanningTimeSpanId
        {
            get
            {
                return Id2;
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public OverflowActions OverflowActionType
        {
            get
            {
                return GetFieldValue<OverflowActions>("OverflowActionType");
            }
            set
            {
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

        private AdminObjectReference<Prompt> m_OverflowMessage;

        private AdminObjectReference<Prompt> m_OverflowReroutePrompt;
        
        [AdminSave(SkipSave=true)]
        public AdminObjectReference<Prompt> OverflowMessage
        {
            get
            {
                return m_OverflowMessage;
            }
            internal set
            {
                if (m_OverflowMessage != null)
                {
                    m_OverflowMessage.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(OverflowMessagePropertyChanged);
                }

                m_OverflowMessage = value;

                if (m_OverflowMessage != null)
                {
                    m_OverflowMessage.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OverflowMessagePropertyChanged);
                }

            }
        }

        private AdminObjectReference<Preprocessor> m_OverflowPreprocessor;

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
            if (OverflowActionType == OverflowActions.IVR && OverflowParam != m_OverflowPreprocessor.TargetId)
                OverflowParam = m_OverflowPreprocessor.TargetId;

            FirePropertyChanged("OverflowPreprocessor");
        }

        protected void OverflowMessagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (OverflowActionType == OverflowActions.Message && OverflowParam != m_OverflowMessage.TargetId)
                OverflowParam = m_OverflowMessage.TargetId;

            FirePropertyChanged("OverflowMessage");
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

        public InboundActivity Activity
        {
            get
            {
                return (InboundActivity)(m_Core.GetAdminObject(Id1));
            }
        }

        public PlanningTimeSpan PlanningTimeSpan
        {
            get { return (PlanningTimeSpan)(m_Core.GetAdminObject(Id2)); }
        }

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            XmlElement node = base.CreateSaveNode(doc, operation);

            XmlAttribute att = doc.CreateAttribute("planningid");
            att.Value = PlanningTimeSpan.PlanningId;
            node.Attributes.Append(att);

            return node;

        }
    }

    [AdminObjectLinkCascadeAttribute(typeof(InboundActivity), "SpecialDayActions")]
    [AdminObjectLinkCascadeAttribute(typeof(SpecialDay), "InboundActivities")]
    public class InboundActivitySpecialDayAction : AdminObjectLink<InboundActivity, SpecialDay>
    {
        public InboundActivitySpecialDayAction(AdminObject parent)
            : base(parent)
        {
            OverflowActionType = OverflowActions.None;
            OverflowMessage = new AdminObjectReference<Prompt>(this);
            OverflowReroutePrompt = new AdminObjectReference<Prompt>(this);
            OverflowPreprocessor = new AdminObjectReference<Preprocessor>(this);
        }

        internal override void Load(XmlElement node)
        {
            base.Load(node);

            if (OverflowPreprocessor.HasTarget && OverflowPreprocessor.Target.ConfigType != null)
            {
                OverflowPreprocessorConfig = (BasePreprocessorConfig)Core.Create(OverflowPreprocessor.Target.ConfigType, this, string.Concat(InboundActivityId, SpecialDayId));
                OverflowPreprocessorConfig.DeserializeFromText(OverflowPreprocessorParams);
                OverflowPreprocessorConfig.saveToTextStorage = ((t) => { OverflowPreprocessorParams = t; });
            }

        }

        public string InboundActivityId
        {
            get
            {
                return Id1;
            }
        }

        public string SpecialDayId
        {
            get
            {
                return Id2;
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public OverflowActions OverflowActionType
        {
            get
            {
                return GetFieldValue<OverflowActions>("OverflowActionType");
            }
            set
            {
                SetFieldValue<OverflowActions>("OverflowActionType", value);
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

        private AdminObjectReference<Prompt> m_OverflowMessage;

        private AdminObjectReference<Prompt> m_OverflowReroutePrompt;

        [AdminSave(SkipSave = true)]
        public AdminObjectReference<Prompt> OverflowMessage
        {
            get
            {
                return m_OverflowMessage;
            }
            internal set
            {
                if (m_OverflowMessage != null)
                {
                    m_OverflowMessage.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(OverflowMessagePropertyChanged);
                }

                m_OverflowMessage = value;

                if (m_OverflowMessage != null)
                {
                    m_OverflowMessage.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OverflowMessagePropertyChanged);
                }

            }
        }

        private AdminObjectReference<Preprocessor> m_OverflowPreprocessor;

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
            if (OverflowActionType == OverflowActions.IVR && OverflowParam != m_OverflowPreprocessor.TargetId)
                OverflowParam = m_OverflowPreprocessor.TargetId;

            FirePropertyChanged("OverflowPreprocessor");
        }

        protected void OverflowMessagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (OverflowActionType == OverflowActions.Message && OverflowParam != m_OverflowMessage.TargetId)
                OverflowParam = m_OverflowMessage.TargetId;

            FirePropertyChanged("OverflowMessage");
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

        public Activity Activity
        {
            get
            {
                return (InboundActivity)(m_Core.GetAdminObject(Id1));
            }
        }

        public SpecialDay SpecialDay
        {
            get { return (SpecialDay)(m_Core.GetAdminObject(Id2)); }
        }

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            XmlElement node = base.CreateSaveNode(doc, operation);

            XmlAttribute att = doc.CreateAttribute("planningid");
            att.Value = SpecialDay.PlanningId;
            node.Attributes.Append(att);

            return node;
        }
    }






    [AdminObjectLinkCascadeAttribute(typeof(OutboundActivity), "TimeSpanActions")]
    [AdminObjectLinkCascadeAttribute(typeof(PlanningTimeSpan), "OutboundActivities")]
    public class OutboundActivityTimeSpanAction : AdminObjectLink<OutboundActivity, PlanningTimeSpan>
    {
        public OutboundActivityTimeSpanAction(AdminObject parent)
            : base(parent)
        {
            ClosedActionType = OutboundClosingAction.None;
        }

        public string OutboundActivityId
        {
            get
            {
                return Id1;
            }
        }

        public string PlanningTimeSpanId
        {
            get
            {
                return Id2;
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public OutboundClosingAction ClosedActionType
        {
            get
            {
                return GetFieldValue<OutboundClosingAction>("ClosedActionType");
            }
            set
            {
                SetFieldValue<OutboundClosingAction>("ClosedActionType", value);
                ClosedParam = DialingMode.Progressive;
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
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

        public OutboundActivity Activity
        {
            get
            {
                return (OutboundActivity)(m_Core.GetAdminObject(Id1));
            }
        }

        public PlanningTimeSpan PlanningTimeSpan
        {
            get { return (PlanningTimeSpan)(m_Core.GetAdminObject(Id2)); }
        }

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            XmlElement node = base.CreateSaveNode(doc, operation);

            XmlAttribute att = doc.CreateAttribute("planningid");
            att.Value = PlanningTimeSpan.PlanningId;
            node.Attributes.Append(att);

            return node;

        }
    }

    [AdminObjectLinkCascadeAttribute(typeof(OutboundActivity), "SpecialDayActions")]
    [AdminObjectLinkCascadeAttribute(typeof(SpecialDay), "OutboundActivities")]
    public class OutboundActivitySpecialDayAction : AdminObjectLink<OutboundActivity, SpecialDay>
    {
        public OutboundActivitySpecialDayAction(AdminObject parent)
            : base(parent)
        {
            ClosedActionType = OutboundClosingAction.None;
        }

        public string OutboundActivityId
        {
            get
            {
                return Id1;
            }
        }

        public string SpecialDayId
        {
            get
            {
                return Id2;
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public OutboundClosingAction ClosedActionType
        {
            get
            {
                return GetFieldValue<OutboundClosingAction>("ClosedActionType");
            }
            set
            {
                SetFieldValue<OutboundClosingAction>("ClosedActionType", value);
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
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

        public Activity Activity
        {
            get
            {
                return (OutboundActivity)(m_Core.GetAdminObject(Id1));
            }
        }

        public SpecialDay SpecialDay
        {
            get { return (SpecialDay)(m_Core.GetAdminObject(Id2)); }
        }

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            XmlElement node = base.CreateSaveNode(doc, operation);

            XmlAttribute att = doc.CreateAttribute("planningid");
            att.Value = SpecialDay.PlanningId;
            node.Attributes.Append(att);

            return node;

        }
    }
}