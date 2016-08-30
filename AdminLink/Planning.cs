using System.Collections;
using Nixxis;
namespace Nixxis.Client.Admin
{
    public class Planning : AdminObject
    {
        public override void Clear()
        {
            // clear the time spans
            while (TimeSpans.Count > 0)
                Core.Delete(TimeSpans[0]);
            while (SpecialDays.Count > 0)
                Core.Delete(SpecialDays[0]);
            base.Clear();
        }

        private static TranslationContext m_TranslationContext = new TranslationContext("Planning");

        public static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
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
                    return string.Concat("Planning ", Description);
            }
        }


        public Planning(AdminObject parent)
            : base(parent)
        {
        }

        public Planning(AdminCore core)
            : base(core)
        {
        }

        [AdminLoad(Path = "/Admin/PlanningTimeSpans/PlanningTimeSpan[@planningid=\"{0}\"]")]
        public AdminObjectList<PlanningTimeSpan> TimeSpans
        {
            get;
            internal set;
        }

        [AdminLoad(Path = "/Admin/SpecialDays/SpecialDay[@planningid=\"{0}\"]")]
        public AdminObjectList<SpecialDay> SpecialDays
        {
            get;
            internal set;
        }

        public Planning Duplicate()
        {
            Planning newplan = AdminCore.Create<Planning>();
            newplan.Description = string.Concat(Description, "*");
            newplan.GroupKey = GroupKey;
            foreach (SpecialDay sd in SpecialDays)
            {
                SpecialDay newsd = sd.AdminCore.Create<SpecialDay>();
                newsd.Closed = sd.Closed;
                newsd.Day = sd.Day;
                newsd.Description = string.Concat(sd.Description, "*");
                newsd.From = sd.From;
                newsd.Month = sd.Month;
                newsd.PlanningId = newplan.Id;
                newsd.To = sd.To;
                newsd.Year = sd.Year;
                newplan.SpecialDays.Add(newsd);
            }
            foreach (PlanningTimeSpan pts in TimeSpans)
            {
                PlanningTimeSpan newpts = pts.AdminCore.Create<PlanningTimeSpan>();
                newpts.Closed = pts.Closed;
                newpts.EndTime = pts.EndTime;
                newpts.PlanningId = newplan.Id;
                newpts.StartTime = pts.StartTime;
                newplan.TimeSpans.Add(newpts);
            }
            AdminCore.Plannings.Add(newplan);
            return newplan;
        }
    }

    public class PlanningTimeSpan: AdminObject
    {
        internal override bool Reload(System.Xml.XmlNode node)
        {
            if (base.Reload(node))
            {
                Planning pl = m_Core.GetAdminObject(PlanningId) as Planning;
                if (pl == null)
                {
                    return false;
                }
                if (!pl.TimeSpans.ContainsId(Id))
                    pl.TimeSpans.Add(this);
                return true;
            }
            return false;
        }
        
        public PlanningTimeSpan(AdminObject parent)
            : base(parent)
        {
        }

        public PlanningTimeSpan(AdminCore core)
            : base(core)
        {
        }


        public int StartTime
        {
            get
            {
                return GetFieldValue<int>("StartTime");
            }
            set
            {
                SetFieldValue<int>("StartTime", value);

                FirePropertyChanged("DisplayText");
                FirePropertyChanged("ShortDisplayText");
                FirePropertyChanged("TypedDisplayText");


            }
        }
        private int m_EndTime;
        public int EndTime
        {
            get
            {
                return m_EndTime;
            }
            set
            {
                m_EndTime = value;

                FirePropertyChanged("DisplayText");
                FirePropertyChanged("ShortDisplayText");
                FirePropertyChanged("TypedDisplayText");
            }
        }
        public bool Closed
        {
            get
            {
                return GetFieldValue<bool>("Closed");
            }
            set
            {
                SetFieldValue<bool>("Closed", value);

                FirePropertyChanged("DisplayText");
                FirePropertyChanged("ShortDisplayText");
                FirePropertyChanged("TypedDisplayText");
            }
        }
        public string PlanningId
        {
            get
            {
                return GetFieldValue<string>("PlanningId");
            }
            set
            {
                SetFieldValue<string>("PlanningId", value);
            }
        }

        private string TimeDescription
        {
            get
            {
                int day = StartTime / 1440;
                int HoursMinutes = StartTime - 1440 * day;
                string strDay = string.Empty;
                switch (day)
                {
                    case 0:
                        strDay = Planning.Translate("Monday");
                        break;
                    case 1:
                        strDay = Planning.Translate("Tuesday");
                        break;
                    case 2:
                        strDay = Planning.Translate("Wednesday");
                        break;
                    case 3:
                        strDay = Planning.Translate("Thursday");
                        break;
                    case 4:
                        strDay = Planning.Translate("Friday");
                        break;
                    case 5:
                        strDay = Planning.Translate("Saturday");
                        break;
                    case 6:
                        strDay = Planning.Translate("Sunday");
                        break;
                }
                int hours = HoursMinutes / 60;
                int minutes = HoursMinutes - 60 * hours;
                return string.Format("{0} {1:00}:{2:00}", strDay, hours, minutes);
            }
        }
        private string EndTimeDescription
        {
            get
            {
                int day = EndTime / 1440;
                int HoursMinutes = EndTime - 1440 * day;
                string strDay = string.Empty;
                switch (day)
                {
                    case 0:
                        strDay = Planning.Translate("Monday");
                        break;
                    case 1:
                        strDay = Planning.Translate("Tuesday");
                        break;
                    case 2:
                        strDay = Planning.Translate("Wednesday");
                        break;
                    case 3:
                        strDay = Planning.Translate("Thursday");
                        break;
                    case 4:
                        strDay = Planning.Translate("Friday");
                        break;
                    case 5:
                        strDay = Planning.Translate("Saturday");
                        break;
                    case 6:
                        strDay = Planning.Translate("Sunday");
                        break;
                }
                int hours = HoursMinutes / 60;
                int minutes = HoursMinutes - 60 * hours;
                return string.Format("{0} {1:00}:{2:00}", strDay, hours, minutes);
            }
        }
        private string Description
        {
            get
            {
                return string.Format(Planning.Translate("{0} from {1} to {2}"), Closed ? Planning.Translate("Closed") : Planning.Translate("Opened"), TimeDescription, EndTimeDescription);
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
        public override string ShortDisplayText
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
                    return Owner.DisplayText;
                else
                    return Description;
            }
        }

        [AdminLoad(Path = "/Admin/InboundActivitiesTimeSpanActions/InboundActivityTimeSpanAction[@planningtimespanid=\"{0}\"]")]
        public AdminObjectList<InboundActivityTimeSpanAction> InboundActivities
        {
            get;
            internal set;
        }

        [AdminLoad(Path = "/Admin/OutboundActivitiesTimeSpanActions/OutboundActivityTimeSpanAction[@planningtimespanid=\"{0}\"]")]
        public AdminObjectList<OutboundActivityTimeSpanAction> OutboundActivities
        {
            get;
            internal set;
        }

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            try
            {
                m_Core.GetAdminObject<Planning>(PlanningId).EmptySave(doc);
            }
            catch
            {
            }
            return base.CreateSaveNode(doc, operation);
        }
    }

    public class SpecialDay : AdminObject
    {
        internal override bool Reload(System.Xml.XmlNode node)
        {
            if (base.Reload(node))
            {
                Planning pl = m_Core.GetAdminObject(PlanningId) as Planning;
                if (pl == null)
                {
                    return false;
                }
                if (!pl.SpecialDays.ContainsId(Id))
                    pl.SpecialDays.Add(this);
                return true;
            }
            return false;
        }


        public SpecialDay(AdminObject parent)
            : base(parent)
        {
        }

        public SpecialDay(AdminCore core)
            : base(core)
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

        public int Day
        {
            get
            {
                return GetFieldValue<int>("Day");
            }
            set
            {
                SetFieldValue<int>("Day", value);
            }
        }
        public int Month
        {
            get
            {
                return GetFieldValue<int>("Month");
            }
            set
            {
                SetFieldValue<int>("Month", value);
            }
        }
        public int Year
        {
            get
            {
                return GetFieldValue<int>("Year");
            }
            set
            {
                SetFieldValue<int>("Year", value);
            }
        }

        public bool Closed
        {
            get
            {
                return GetFieldValue<bool>("Closed");
            }
            set
            {
                SetFieldValue<bool>("Closed", value);
            }
        }
        
        public int From
        {
            get
            {
                return GetFieldValue<int>("From");
            }
            set
            {
                SetFieldValue<int>("From", value);
            }
        }
        public int To
        {
            get
            {
                return GetFieldValue<int>("To");
            }
            set
            {
                SetFieldValue<int>("To", value);
            }
        }

        public string PlanningId
        {
            get
            {
                return GetFieldValue<string>("PlanningId");
            }
            set
            {
                SetFieldValue<string>("PlanningId", value);
            }
        }

        internal override void Load(System.Xml.XmlElement node)
        {
            base.Load(node);
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
        public override string ShortDisplayText
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
                    return Owner.DisplayText;
                else
                    return string.Concat("Special day ", Description);
            }
        }

        [AdminLoad(Path = "/Admin/InboundActivitySpecialDayActions/InboundActivitySpecialDayAction[@specialdayid=\"{0}\"]")]
        public AdminObjectList<InboundActivitySpecialDayAction> InboundActivities
        {
            get;
            internal set;
        }

        [AdminLoad(Path = "/Admin/OutboundActivitySpecialDayActions/OutboundActivitySpecialDayAction[@specialdayid=\"{0}\"]")]
        public AdminObjectList<OutboundActivitySpecialDayAction> OutboundActivities
        {
            get;
            internal set;
        }

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            try
            {
                m_Core.GetAdminObject<Planning>(PlanningId).EmptySave(doc);
            }
            catch
            {
            }

            return base.CreateSaveNode(doc, operation);
        }
    }
}