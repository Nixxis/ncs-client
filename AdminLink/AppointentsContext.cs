using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nixxis;
using System.Security.Cryptography;
using System.Xml;
using System.Collections.Specialized;

namespace Nixxis.Client.Admin
{
    public enum AppointmentRule
    {
        /// <summary>
        /// No strategy defined.
        /// </summary>
        None = 0,
        /// <summary>
        /// The fisrt free slot is used to determine the area to use.
        /// </summary>
        FirstFreeSlot,
        /// <summary>
        /// Try to fill the area having the smallest fill factor for a specific number of days.
        /// </summary>
        FillFactor,
        /// <summary>
        /// Try to fill the area having the smallest fill factor for a specific number of days and a specific fill factor target. 
        /// </summary>
        RestrictedFillFactor
    }

    public class AppointmentsContext : AdminObject
    {
        public AppointmentsContext(AdminCore core)
            : base(core)
        {
            Init();
        }

        public AppointmentsContext(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private void Init()
        {
            StartTime = 10 * 60;
            EndTime = 20 * 60;
            DefaultAppointmentDuration = 30;
            InitialDelay = 24;
            AllowedDays = 0;
            Granularity = 15;
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
                    return string.Concat("Appointment context ", Description);
            }
        }

        // TODO: check how the plannings are used at this level to ensure it is still working in V2...
        public AdminObjectReference<Planning> Planning
        {
            get;
            internal set;
        }

        public string Code
        {
            get{
                return GetFieldValue<string>("Code");
            }
            set{
                SetFieldValue<string>("Code", value);
            }
        }

        public string BaseUri
        {
            get{
                return GetFieldValue<string>("BaseUri");
            }
            set{
                SetFieldValue<string>("BaseUri", value);
            }
        }

        public int StartTime
        {
            get{
                return GetFieldValue<int>("StartTime");
            }
            set{
                SetFieldValue<int>("StartTime", value);
            }
        }

        public int EndTime
        {
            get{
                return GetFieldValue<int>("EndTime");
            }
            set{
                SetFieldValue<int>("EndTime", value);
            }
        }

        public int Granularity
        {
            get{
                return GetFieldValue<int>("Granularity");
            }
            set{
                SetFieldValue<int>("Granularity", value);
            }
        }

        public int InitialDelay
        {
            get{
                return GetFieldValue<int>("InitialDelay");
            }
            set{
                SetFieldValue<int>("InitialDelay", value);
            }
        }

        public bool InitialDelayLimited
        {
            get
            {
                return (InitialDelay != 0);
            }
            set
            {
                if (value)
                {
                    InitialDelay = 24;
                }
                else
                {
                    InitialDelay = 0;
                }
            }
        }
        
        public int DefaultAppointmentDuration
        {
            get{
                return GetFieldValue<int>("DefaultAppointmentDuration");
            }
            set{
                SetFieldValue<int>("DefaultAppointmentDuration", value);
            }
        }

        public int AllowedDays
        {
            get{
                return GetFieldValue<int>("AllowedDays");
            }
            set{
                SetFieldValue<int>("AllowedDays", value);
            }
        }

        public bool AllowedDaysLimited
        {
            get
            {
                return (AllowedDays != 0);
            }
            set
            {
                if (value)
                {
                    AllowedDays = 7;
                }
                else
                {
                    AllowedDays = 0;
                }
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public AppointmentRule OrderingRule
        {
            get{
                return GetFieldValue<AppointmentRule>("OrderingRule");
            }
            set{
                SetFieldValue<AppointmentRule>("OrderingRule", value);
            }
        }

        public int OrderingRuleNumberOfDays
        {
            get{
                return GetFieldValue<int>("OrderingRuleNumberOfDays");
            }
            set{
                SetFieldValue<int>("OrderingRuleNumberOfDays", value);
            }
        }
        
        public int OrderingRuleTargetFillFactor
        {
            get
            {
                return GetFieldValue<int>("OrderingRuleTargetFillFactor");
            }
            set
            {
                SetFieldValue<int>("OrderingRuleTargetFillFactor", value);
            }
        }

        [AdminLoad(Path = "/Admin/AppointmentsAreas/AppointmentsArea[@context=\"{0}\"]")]
        public AdminObjectList<AppointmentsArea> Areas
        {
            get;
            internal set;
        }


        [AdminLoad(Path = "/Admin/AppointmentsMembers/AppointmentsMember[@context=\"{0}\"]")]
        public AdminObjectList<AppointmentsMember> Members
        {
            get;
            internal set;
        }

        public AppointmentsContext Duplicate()
        {
            AppointmentsContext newctx = AdminCore.Create<AppointmentsContext>();

            newctx.AllowedDays = AllowedDays;
            newctx.AllowedDaysLimited = AllowedDaysLimited;
            Dictionary<string, string> areaMap = new Dictionary<string, string>();
            foreach(AppointmentsArea aa in Areas)
            {
                AppointmentsArea newaa = aa.AdminCore.Create<AppointmentsArea>();
                areaMap.Add(aa.Id, newaa.Id);
                newaa.Area = aa.Area;
                newaa.AreaWithoutMembers = aa.AreaWithoutMembers;
                newaa.AssociatedFieldMeaning = aa.AssociatedFieldMeaning;
                newaa.Description = string.Concat(aa.Description, "*");
                newaa.MaxAppointments = aa.MaxAppointments;
                newaa.MaxConcurentAppointments = aa.MaxConcurentAppointments;                
                newaa.PositiveMaxAppointments = aa.PositiveMaxAppointments;
                newaa.PositiveMaxConcurentAppointments = aa.PositiveMaxConcurentAppointments;
                newaa.Sequence = aa.Sequence;
                newctx.Areas.Add(newaa);
            }

            newctx.BaseUri = BaseUri;
            newctx.Code = Code;
            newctx.DefaultAppointmentDuration = DefaultAppointmentDuration;
            newctx.Description = string.Concat(Description, "*");
            newctx.EndTime = EndTime;
            newctx.Granularity = Granularity;
            newctx.GroupKey = GroupKey;
            newctx.InitialDelay = InitialDelay;

            foreach (AppointmentsMember am in Members)
            {
                AppointmentsMember newam = am.AdminCore.Create<AppointmentsMember>();

                foreach (AppointmentsRelation ar in am.Areas)
                {
                    newam.Areas.Add(am.AdminCore.GetAdminObject( areaMap[ar.AreaId]));
                }

                newam.Description = string.Concat(am.Description, "*");
                newam.MailboxId = am.MailboxId;                
                newam.Password = am.Password;
                newam.Planning.TargetId = am.Planning.TargetId;
                newctx.Members.Add(newam);
            }

            newctx.OrderingRule = OrderingRule;
            newctx.OrderingRuleNumberOfDays = OrderingRuleNumberOfDays;
            newctx.OrderingRuleTargetFillFactor = OrderingRuleTargetFillFactor;
            newctx.Planning.TargetId = Planning.TargetId;
            newctx.StartTime = StartTime;

            newctx.AdminCore.AppointmentsContexts.Add(newctx);

            return newctx;
        }

    }

    public class AppointmentsArea: AdminObject
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("AppointmentsArea");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        internal override bool Reload(System.Xml.XmlNode node)
        {
            if (base.Reload(node))
            {
                bool parentIsOk = false;
                if (Parent is AdminObjectList<AppointmentsArea>)
                {
                    AdminObjectList<AppointmentsArea> temp = (AdminObjectList < AppointmentsArea >)Parent;
                    if (temp.Parent is AppointmentsContext)
                    {
                        parentIsOk = true;
                    }
                }

                string context = node.Attributes["context"].Value;
                AppointmentsContext pl = m_Core.GetAdminObject(context) as AppointmentsContext;
                if (pl == null)
                {
                    return false;
                }
                if (!parentIsOk)
                {
                    Parent = pl.Areas;
                }

                if (!pl.Areas.ContainsId(Id))
                    pl.Areas.Add(this);
                return true;
            }
            return false;
        }

        
        public AppointmentsArea(AdminCore core)
            : base(core)
        {
            Init();
        }

        public AppointmentsArea(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private void Init()
        {
            Members = new AdminObjectList<AppointmentsRelation>(this);
            MaxAppointments = 0;
            MaxConcurentAppointments = 0;            
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
                    return string.Concat("Area ", Description);
            }
        }

        public string Area
        {
            get{
                return GetFieldValue<string>("Area");
            }
            set{
                SetFieldValue<string>("Area", value);
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public UserFieldMeanings  AssociatedFieldMeaning
        {
            get{
                return GetFieldValue<UserFieldMeanings>("AssociatedFieldMeaning");
            }
            set
            {
                SetFieldValue<UserFieldMeanings>("AssociatedFieldMeaning", value);
            }
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

        public int MaxAppointments
        {
            get
            {
                return GetFieldValue<int>("MaxAppointments");
            }
            set
            {
                SetFieldValue<int>("MaxAppointments", value);
                FirePropertyChanged("MaxAppointmentsLimited");
                FirePropertyChanged("MaxAppointmentsDescription");
                FirePropertyChanged("PositiveMaxAppointments");

            }
        }

        public int PositiveMaxAppointments
        {
            get
            {
                return Math.Abs(MaxAppointments);
            }
            set
            {
                if (MaxAppointmentsIsRelative)
                {
                    MaxAppointments = -value;
                }
                else
                {
                    MaxAppointments = value;
                }
            }
        }

        public bool MaxAppointmentsIsRelative
        {
            get
            {
                return (MaxAppointments < 0);
            }
            set
            {
                if (value)
                    MaxAppointments = -Math.Abs(MaxAppointments);
                else
                    MaxAppointments = Math.Abs(MaxAppointments);
            }
        }


        public bool MaxAppointmentsLimited
        {
            get
            {
                return (MaxAppointments!=0);
            }
            set
            {
                if (value)
                    MaxAppointments = 1;
                else
                    MaxAppointments = 0;
            }
        }

        public string MaxAppointmentsDescription
        {
            get
            {
                if (MaxAppointmentsIsRelative)
                {
                    return string.Format(Translate("is the number of members linked to this area minus {0}"), PositiveMaxAppointments);
                }
                else
                {
                    return string.Format(Translate("There is no more than {0} appointments per day on the area"), PositiveMaxAppointments);
                }
            }
        }


        public int MaxConcurentAppointments
        {
            get
            {
                return GetFieldValue<int>("MaxConcurentAppointments");
            }
            set
            {
                SetFieldValue<int>("MaxConcurentAppointments", value);
                FirePropertyChanged("MaxConcurentAppointmentsDescription");
                FirePropertyChanged("PositiveMaxConcurentAppointments");
                FirePropertyChanged("MaxConcurentAppointmentsLimited");
            }
        }

        public int PositiveMaxConcurentAppointments
        {
            get
            {
                return Math.Abs(MaxConcurentAppointments);
            }
            set
            {
                if (MaxConcurentAppointmentsIsRelative)
                {
                    MaxConcurentAppointments = -value;
                }
                else
                {
                    MaxConcurentAppointments = value;
                }
            }
        }

        public bool MaxConcurentAppointmentsLimited
        {
            get
            {
                return (MaxConcurentAppointments != 0);
            }
            set
            {
                if (value)
                    MaxConcurentAppointments = 1;
                else
                    MaxConcurentAppointments = 0;
            }
        }

        public bool MaxConcurentAppointmentsIsRelative
        {
            get
            {
                return (MaxConcurentAppointments < 0);
            }
            set
            {
                if (value)
                    MaxConcurentAppointments = -Math.Abs(MaxConcurentAppointments);
                else
                    MaxConcurentAppointments = Math.Abs(MaxConcurentAppointments);
            }
        }

        public string MaxConcurentAppointmentsDescription
        {
            get
            {
                if (MaxConcurentAppointmentsIsRelative)
                {
                    return string.Format(Translate("is the number of available appointments per day minus {0}"), PositiveMaxConcurentAppointments);
                }
                else
                {
                    return string.Format(Translate("There is no more than {0} concurent appointments on the area"), PositiveMaxConcurentAppointments);
                }
            }
        }

        public bool  AreaWithoutMembers
        {
            get
            {
                return GetFieldValue<bool>("AreaWithoutMembers");
            }
            set
            {
                SetFieldValue<bool>("AreaWithoutMembers", value);
                if (value)
                {
                    MaxAppointments = Math.Abs(MaxAppointments);
                    MaxConcurentAppointments = Math.Abs(MaxConcurentAppointments);
                    if(!MaxAppointmentsLimited)
                        MaxAppointmentsLimited = true;

                    if (Members != null)
                    {
                        while (Members.Count > 0)
                        {
                            int backup = Members.Count;
                            Core.Delete(Members[0]);
                            if (backup == Members.Count && backup > 0)
                                Members.RemoveAt(0);

                        }
                    }

                    FirePropertyChanged("CheckedMembers");
                    FirePropertyChanged("AreaWithoutMembersDescription");
                }
            }
        }

        public string AreaWithoutMembersDescription
        {
            get
            {
                if (Members != null && Members.Count > 0)
                {
                    return string.Format(Translate("Area work without members (enabling this will remove {0} members affectation)"), Members.Count);
                }
                else
                {
                    return string.Format(Translate("Area work without members"));
                }                
            }
        }

        private AdminObjectList<AppointmentsRelation> m_Members;

        [AdminLoad(Path = "/Admin/AppointmentsRelations/AppointmentsRelation[@appointmentsareaid=\"{0}\"]")]
        public AdminObjectList<AppointmentsRelation> Members
        {
            get
            {
                return m_Members;
            }
            internal set
            {
                if (m_Members != null)
                {
                    m_Members.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_Members_Collection_Changed);
                }

                m_Members = value;

                if (m_Members != null)
                {
                    m_Members.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_Members_Collection_Changed);
                }
                FirePropertyChanged("AreaWithoutMembersDescription");
            }
        }

        private void m_Members_Collection_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            FirePropertyChanged("AreaWithoutMembersDescription");
        }


        public object CheckedMembers
        {
            get
            {
                return new AdminCheckedLinkList<AppointmentsMember, AppointmentsRelation>(((AppointmentsContext)(((AdminObjectList<AppointmentsArea>)(this.Parent)).Parent)).Members, Members, this);
            }
        }

        public AppointmentsContext ParentContext
        {
            get
            {
                return (AppointmentsContext)(((AdminObjectList<AppointmentsArea>)Parent).Parent);
            }
        }


        public bool IsFirst
        {
            get
            {
                return ((AppointmentsContext)(((AdminObjectList<AppointmentsArea>)Parent).Parent)).Areas.Min((a) => (a.Sequence)) == Sequence;
            }
        }

        public bool IsLast
        {
            get
            {
                return ((AppointmentsContext)(((AdminObjectList<AppointmentsArea>)Parent).Parent)).Areas.Max((a) => (a.Sequence)) == Sequence;
            }
        }

        public AppointmentsArea Previous
        {
            get
            {
                if (Parent == null || Parent.Parent == null)
                    return null;

                int prevSequence = ((AppointmentsContext)(((AdminObjectList<AppointmentsArea>)Parent).Parent)).Areas.Max((a) => (a.Sequence < Sequence ? a.Sequence : -1));
                return ((AppointmentsContext)(((AdminObjectList<AppointmentsArea>)Parent).Parent)).Areas.First((a) => (a.Sequence == prevSequence));
            }
        }
        public AppointmentsArea Next
        {
            get
            {
                if (Parent == null || Parent.Parent == null)
                    return null;
                int nextSequence = ((AppointmentsContext)(((AdminObjectList<AppointmentsArea>)Parent).Parent)).Areas.Min((a) => (a.Sequence > Sequence ? a.Sequence : int.MaxValue));
                return ((AppointmentsContext)(((AdminObjectList<AppointmentsArea>)Parent).Parent)).Areas.First((a) => (a.Sequence == nextSequence));

            }
        }

        private string m_BackupContext = null;
        public override void Clear()
        {
            m_BackupContext = ParentContext.Id;
            base.Clear();
        }

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            try
            {
                ParentContext.EmptySave(doc);
            }
            catch
            {
            }

            XmlElement node = base.CreateSaveNode(doc, operation);

            XmlElement element = doc.CreateElement("Context");
            
            if(operation=="delete")
                element.InnerText = m_BackupContext;
            else
                element.InnerText = ParentContext.Id;

            node.AppendChild(element);

            return node;
        }

        public override void EmptySave(XmlDocument doc)
        {
            try
            {
                ParentContext.EmptySave(doc);
            }
            catch
            {
            }

            base.EmptySave(doc);
        }

    }


    public class AppointmentsMember : AdminObject
    {
        internal override bool Reload(System.Xml.XmlNode node)
        {
            if (base.Reload(node))
            {
                bool parentIsOk = false;
                if (Parent is AdminObjectList<AppointmentsMember>)
                {
                    AdminObjectList<AppointmentsMember> temp = (AdminObjectList<AppointmentsMember>)Parent;
                    if (temp.Parent is AppointmentsContext)
                    {
                        parentIsOk = true;
                    }
                }

                string context = node.Attributes["context"].Value;
                AppointmentsContext pl = m_Core.GetAdminObject(context) as AppointmentsContext;
                if (pl == null)
                {
                    return false;
                }
                if (!parentIsOk)
                {
                    Parent = pl.Members;
                }

                if (!pl.Members.ContainsId(Id))
                    pl.Members.Add(this);
                return true;
            }
            return false;
        }

        public AppointmentsMember(AdminCore core)
            : base(core)
        {
            Init();
        }

        public AppointmentsMember(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private void Init()
        {
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
                    return string.Concat("Member ", Description);
            }
        }

        public AdminObjectReference<Planning> Planning
        {
            get;
            internal set;
        }

        public string MailboxId
        {
            get
            {
                return GetFieldValue<string>("MailboxId");
            }
            set
            {
                SetFieldValue<string>("MailboxId", value);
            }
        }

        public string Password
        {
            get
            {
                return GetFieldValue<string>("Password");
            }
            set
            {
                SetFieldValue<string>("Password", value);
            }
        }

        [AdminLoad(Path = "/Admin/AppointmentsRelations/AppointmentsRelation[@appointmentsmemberid=\"{0}\"]")]
        public AdminObjectList<AppointmentsRelation> Areas
        {
            get;
            internal set;
        }

        public object CheckedAreas
        {
            get
            {
                return new AdminCheckedLinkList<AppointmentsArea, AppointmentsRelation>( ((AppointmentsContext)(((AdminObjectList<AppointmentsMember>)(this.Parent)).Parent)).Areas, Areas, this);
            }
        }

        public AppointmentsContext ParentContext
        {
            get
            {
                return (AppointmentsContext)(((AdminObjectList<AppointmentsMember>)Parent).Parent);
            }
        }

        private string m_BackupContext = null;
        public override void Clear()
        {
            m_BackupContext = ParentContext.Id;
            base.Clear();
        }


        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            try
            {
                ParentContext.EmptySave(doc);
            }
            catch
            {
            }

            XmlElement node = base.CreateSaveNode(doc, operation);

            XmlElement element = doc.CreateElement("Context");
            if(operation=="delete")
                element.InnerText = m_BackupContext;
            else 
                element.InnerText = ParentContext.Id;

            node.AppendChild(element);

            return node;
        }

        public override void EmptySave(XmlDocument doc)
        {
            try
            {
                ParentContext.EmptySave(doc);
            }
            catch
            {
            }


            base.EmptySave(doc);
        }

    }

    [AdminObjectLinkCascadeAttribute(typeof(AppointmentsMember), "Areas")]
    [AdminObjectLinkCascadeAttribute(typeof(AppointmentsArea), "Members")]
    public class AppointmentsRelation : AdminObjectLink<AppointmentsMember, AppointmentsArea>
    {
        public AppointmentsRelation(AdminObject parent)
            : base(parent)
        {
        }

        public string MemberId
        {
            get
            {
                return Id1;
            }
        }

        public string AreaId
        {
            get
            {
                return Id2;
            }
        }


        public AppointmentsMember Member
        {
            get
            {
                return (AppointmentsMember)(m_Core.GetAdminObject(Id1));
            }
        }

        public AppointmentsArea Area
        {
            get { return (AppointmentsArea)(m_Core.GetAdminObject(Id2)); }
        }

    }
}
