using System;
using System.Linq;
using System.Xml;

namespace Nixxis.Client.Admin
{
    public enum SortOrder
    {
        Ascending = 0,
        Descending = 1
    }

    [Admin.AdminSave(SkipSave = true)]
    [AdminObjectLinkCascadeAttribute(typeof(Field), "ActivitiesSortFields")]
    [AdminObjectLinkCascadeAttribute(typeof(OutboundActivity), "SortFields")]
    public class SortField : AdminObjectLink<Field, OutboundActivity>
    {
        private string m_FieldName;
        private bool m_SequenceLoaded = false;

        public SortField(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public SortField(AdminCore core)
            : base(core)
        {
            Init();
        }

        internal override bool Reload(XmlNode node)
        {
            if (base.Reload(node))
            {
                SetFieldLoaded("Sequence");
                return true;
            }
            return false;
        }

        internal override void Load(System.Xml.XmlElement node)
        {
            base.Load(node);

            if (node.Attributes["type"] != null && node.Attributes["type"].Value == "USR")
            {
                AdminObjectList<UserField> userFields = ((OutboundActivity)(((AdminObjectList<SortField>)Parent).Parent)).Campaign.Target.UserFields;
                if (userFields != null)
                {
                    UserField uf = userFields.FirstOrDefault((a) => (a.Name.Equals(m_FieldName)));
                    if (uf != null)
                    {
                        FieldId = uf.Id;
                    }
                }
            }
            else
            {
                AdminObjectList<SystemField> sysFields = ((OutboundActivity)(((AdminObjectList<SortField>)Parent).Parent)).Campaign.Target.SystemFields;
                if (sysFields != null)
                {
                    SystemField sf = sysFields.FirstOrDefault((a) => (a.Name.Equals(m_FieldName)));
                    if (sf != null)
                    {
                        FieldId = sf.Id;
                    }
                    else
                    {
                        AdminObjectList<QuotaField> quotFields = ((OutboundActivity)(((AdminObjectList<SortField>)Parent).Parent)).Campaign.Target.CombinedQuotaFields;
                        if (quotFields != null)
                        {
                            QuotaField qf = quotFields.FirstOrDefault((a) => (a.Name.Equals(m_FieldName)));
                            if (qf != null)
                            {
                                FieldId = qf.Id;
                            }
                        }
                    }

                }
            }

            if (!m_SequenceLoaded)
            {
                m_SequenceLoaded = true;
                Sequence = ((OutboundActivity)(((AdminObjectList<SortField>)Parent).Parent)).SortFields.Count + 1;
            }

            Id = GetCombinedId(Id1, Id2);

            DoneLoading();
        }

        private void Init()
        {
            Sequence = 1;
            SortOrder = Admin.SortOrder.Ascending;
        }

        public string FieldId
        {
            get
            {
                return Id1;
            }
            set
            {
                Id1 = value;
            }
        }

        public string OutboundActivityId
        {
            get
            {
                return Id2;
            }
            set
            {
                Id2 = value;
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

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public SortOrder SortOrder
        {
            get
            {
                return GetFieldValue<SortOrder>("SortOrder");
            }
            set
            {
                SetFieldValue<SortOrder>("SortOrder", value);
            }
        }

        public void SetAggregation(int value)
        {
            Aggregator = (Aggregator)value;
        }

        public Aggregator Aggregator
        {
            get
            {
                return GetFieldValue<Aggregator>("Aggregator");
            }
            set
            {
                SetFieldValue<Aggregator>("Aggregator", value);
            }
        }


        public Field Field
        {
            get
            {
                if (m_Core == null)
                    return null;
                return (Field)(m_Core.GetAdminObject(Id1));
            }
        }

        public string Details
        {
            get
            {
                if (Aggregator == Admin.Aggregator.None)
                    return string.Format("{0} order on {1}", SortOrder,  Field.TypedDisplayText);
                else
                    return string.Format("{0} order on {1}({2})", SortOrder, Aggregator, Field.TypedDisplayText);
            }
        }

        public OutboundActivity Activity
        {
            get 
            { 
                if(Id2 == null || m_Core == null)
                    return null;
                return (OutboundActivity)(m_Core.GetAdminObject(Id2));
            }
        }

        protected override void SetFieldValue<T>(string index, T value)
        {
            base.SetFieldValue<T>(index, value);
            RecomputeSortOrder();
        }

        private void RecomputeSortOrder()
        {
            if (Activity != null)
                Activity.RecomputeSortOrder();
        }

        public override string TypedDisplayText
        {
            get
            {
                return string.Format("Sort order on {0}", Field.TypedDisplayText);
            }
        }

        public void SetFieldName(string value)
        {
            m_FieldName = value;
        }

        public bool IsFirst
        {
            get
            {
                return Activity.SortFields.Min((a) => (a == null ? int.MaxValue : a.Sequence)) == Sequence;
            }
        }

        public bool IsLast
        {
            get
            {

                return Activity.SortFields.Max((a) => (a == null ? int.MinValue : a.Sequence)) == Sequence;
            }
        }

        public SortField Previous
        {
            get
            {
                if (Activity == null)
                    return null;
                int prevSequence = Activity.SortFields.Max((a) => (a.Sequence < Sequence ? a.Sequence : -1));
                return Activity.SortFields.First((a) => (a.Sequence == prevSequence));
            }
        }

        public SortField Next
        {
            get
            {
                if (Activity == null)
                    return null;
                int nextSequence = Activity.SortFields.Min((a) => (a.Sequence > Sequence ? a.Sequence : int.MaxValue));
                return Activity.SortFields.First((a) => (a.Sequence == nextSequence));

            }
        }


        public void SaveNode(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("SortPart");

            XmlAttribute att = doc.CreateAttribute("FieldName");
            att.Value = Field.Name;
            node.Attributes.Append(att);

            att = doc.CreateAttribute("Asc");
            att.Value = (this.SortOrder == Admin.SortOrder.Ascending) ? "True": "False" ;
            node.Attributes.Append(att);

            att = doc.CreateAttribute("AppointmentFunction");
            att.Value = "False";
            node.Attributes.Append(att);

            if (Aggregator != Admin.Aggregator.None)
            {
                att = doc.CreateAttribute("Aggregation");
                att.Value = XmlConvert.ToString((int)Aggregator);
                node.Attributes.Append(att);
            }

            doc.DocumentElement.AppendChild(node);
        }
    }
}