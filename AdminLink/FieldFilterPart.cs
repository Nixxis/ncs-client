using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using Nixxis;

namespace Nixxis.Client.Admin
{
    public enum ActivityFilterType
    {
        ThisActivity,
        OtherActivity,
        AnyActivity,
        NoActivity,
        DNRActivity
    }

    public enum Aggregator
    {
        None,
        AbsoluteAggregatedValue,
        AbsoluteRemainingAggregated,
        RelativeAggregatedValue,
        RelativeRemainingAggregated,
    }

    public enum Operator
    {
        /// <summary>
        /// Check if the field value is not null.
        /// </summary>
        IsNotNull,
        /// <summary>
        /// Check if the field value is null.
        /// </summary>
        IsNull,
        /// <summary>
        /// Check if the field value is true.
        /// </summary>
        IsTrue,
        /// <summary>
        /// Check if the field value is false.
        /// </summary>
        IsFalse,
        /// <summary>
        /// Check for equality.
        /// </summary>
        Equal,
        /// <summary>
        /// Check for inequality.
        /// </summary>
        NotEqual,
        /// <summary>
        /// Check if a value is inferior to another. 
        /// </summary>
        Inferior,
        /// <summary>
        /// Check if a value is inferior or equal to another.
        /// </summary>
        InferiorOrEqual,
        /// <summary>
        /// Check if a value is superior to another.
        /// </summary>
        Superior,
        /// <summary>
        /// Check if a value is superior or equal to another.
        /// </summary>
        SuperiorOrEqual,
        /// <summary>
        /// Check if a value is like another.
        /// </summary>
        Like,
        /// <summary>
        /// Check if a datetime value is before another.
        /// </summary>
        IsBefore,
        /// <summary>
        /// Check if a datetime value is after another.
        /// </summary>
        IsAfter,
        /// <summary>
        /// Check if a value is in the past.
        /// </summary>
        IsInThePast,
        /// <summary>
        /// Check if a value is in the future.
        /// </summary>
        IsInTheFuture,
        /// <summary>
        /// Check if a value is in the past while some delay expressed in minutes.
        /// </summary>
        IsXMinutesInThePast,
        /// <summary>
        /// Check if a value is in the future for some duration expressed in minutes.
        /// </summary>
        IsXMinutesInTheFuture,
        IsEmpty,
        IsNotEmpty,
        IsNullOrEmpty,
        IsNotNullAndNotEmpty
    }

    public enum CombineOperator
    {
        None,
        And,
        Or
    }

    [AdminSave(SkipSave=true)]
    public class FilterPart : AdminObject
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("FieldFilterParts");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        private string m_FieldName;
        private bool m_SequenceLoaded = false;


        public FilterPart(AdminObject parent)
            : base(parent)
        {
            Init();
            //TODO: use ActivityProperty instead...
            if (parent != null)
            {
                if(parent is AdminObjectList<FilterPart>)
                    ((AdminObjectList<FilterPart>)Parent).CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(FilterParts_CollectionChanged);
                else if (parent is OutboundActivity)
                    ((OutboundActivity)Parent).FilterParts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(FilterParts_CollectionChanged);
            }
        }

        void FilterParts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (FilterPart fp in (IEnumerable<FilterPart>)sender)
            {
                fp.CheckCombineOperator();
            }
        }

        public FilterPart(AdminCore core)
            : base(core)
        {
            Init();
        }

        private void Init()
        {
            Activity = new AdminObjectReference<OutboundActivity>(this);
            OperandField = new AdminObjectReference<AdminObject>(this);
            Id = System.Guid.NewGuid().ToString("N");
            Sequence = 1;
        }

        internal override bool Reload(XmlNode node)
        {
            if (base.Reload(node))
            {
                SetFieldLoaded("Sequence");
                return true;
            } return false;
        }

        internal override void Load(System.Xml.XmlElement node)
        {
            Activity.TargetId = node.SelectSingleNode("ancestor::OutboundActivity").Attributes["id"].Value;
            Activity.DoneLoading();

            base.Load(node);

            if (node.Attributes["type"] != null && node.Attributes["type"].Value == "USR")
            {
                AdminObjectList<UserField> userFields = ((OutboundActivity)(((AdminObjectList<FilterPart>)Parent).Parent)).Campaign.Target.UserFields;
                if (userFields != null)
                {
                    UserField uf = userFields.FirstOrDefault((a) => (a.Name.Equals(m_FieldName)));
                    if (uf != null)
                    {
                        Field.TargetId = uf.Id;
                    }
                }
            }
            else
            {
                AdminObjectList<SystemField> sysFields = ((OutboundActivity)(((AdminObjectList<FilterPart>)Parent).Parent)).Campaign.Target.SystemFields;
                if (sysFields != null)
                {
                    SystemField sf = sysFields.FirstOrDefault((a) => (a.Name.Equals(m_FieldName)));
                    if (sf != null)
                    {
                        Field.TargetId = sf.Id;
                    }
                    else
                    {
                        AdminObjectList<QuotaField> quotaFields = ((OutboundActivity)(((AdminObjectList<FilterPart>)Parent).Parent)).Campaign.Target.CombinedQuotaFields;
                        if (quotaFields != null)
                        {
                            QuotaField qf = quotaFields.FirstOrDefault((a) => (a.Name.Equals(m_FieldName)));
                            if (qf != null)
                            {
                                Field.TargetId = qf.Id;
                            }
                        }
                    }

                }
            }

            if (!m_SequenceLoaded)
            {
                m_SequenceLoaded = true;
                Sequence = ((OutboundActivity)(((AdminObjectList<FilterPart>)Parent).Parent)).FilterParts.Count +1;
            }
        }

        public void SetSequence(int value)
        {
            Sequence = value;
            m_SequenceLoaded = true;
        }

        public void SetOperand(string value)
        {
            OperandText = value;
        }

        public void SetFieldOperand(string value)
        {

            if (value.StartsWith("SD.[") && value.EndsWith("]"))
            {
                AdminObjectList<SystemField> sysFields = ((OutboundActivity)(((AdminObjectList<FilterPart>)Parent).Parent)).Campaign.Target.SystemFields;
                if (sysFields != null)
                {
                    SystemField sf = sysFields.FirstOrDefault((a) => (a.Name.Equals(value.Substring(4, value.Length - 5))));
                    OperandField.TargetId = sf.Id;
                }

            }
            else if (value.StartsWith("CD.[") && value.EndsWith("]"))
            {
                AdminObjectList<UserField> userFields = ((OutboundActivity)(((AdminObjectList<FilterPart>)Parent).Parent)).Campaign.Target.UserFields;
                if (userFields != null)
                {
                    UserField uf = userFields.FirstOrDefault((a) => (a.Name.Equals(value.Substring(4, value.Length - 5))));
                    if (uf != null)
                    {
                        OperandField.TargetId = uf.Id;
                    }
                }
            }
            else
            {
                AdminObjectList<UserField> userFields = ((OutboundActivity)(((AdminObjectList<FilterPart>)Parent).Parent)).Campaign.Target.UserFields;
                if (userFields != null)
                {
                    UserField uf = userFields.FirstOrDefault((a) => (a.Name.Equals(value)));
                    if (uf != null)
                    {
                        OperandField.TargetId = uf.Id;
                    }
                }
            }



        }

        public void SetFieldName(string value)
        {
            m_FieldName = value;
        }

        public void SetLookupOperand(string value)
        {
            OperandField.TargetId = value;
        }


        public void SetOperator(Operator value)
        {
            SetFieldValue<Operator>("Operator", value);
        }

        public void SetNextPart(int value)
        {
            if (value == 0)
            {
                CombineOperator = Admin.CombineOperator.And;
            }
            else
            {
                CombineOperator = Admin.CombineOperator.Or;
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
                CheckCombineOperator();

                FirePropertyChanged("IsFirst");
                FirePropertyChanged("IsLast");                

            }
        }

        internal bool Loading { get; set; }

        private void CheckCombineOperator()
        {
            if (Parent != null && !Loading )
            {
                // TODO: use Activity instead
                OutboundActivity oact = null;

                if (Parent is AdminObjectList<FilterPart>)
                {
                    oact = ((OutboundActivity)(((AdminObjectList<FilterPart>)Parent).Parent));
                }
                else if (Parent is OutboundActivity)
                {
                    oact = (OutboundActivity)Parent;
                }

                if (oact.FilterParts.Count == Sequence)
                {
                    if (CombineOperator != Admin.CombineOperator.None)
                    {
                        CombineOperator = Admin.CombineOperator.None;
                    }
                }
                else
                {
                    if (CombineOperator == Admin.CombineOperator.None)
                    {
                        CombineOperator = Admin.CombineOperator.And;
                    }
                }
            }
        }

        public AdminObjectReference<Field> Field
        {
            get;
            internal set;
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

        public string Details
        {
            get
            {
                if(Aggregator== Admin.Aggregator.None)
                    return string.Format(Translate("Filter on {0}"), Field.Target.TypedDisplayText);
                else
                    return string.Format(Translate("Filter on {0} aggregated by {1}"), Field.Target.TypedDisplayText, (new AggregatorHelper()).First((a) => (a.EnumValue == Aggregator)).Description);

            }
        }

        public AdminObjectReference<OutboundActivity> Activity
        {
            get;
            internal set;
        }

        public AdminObjectReference<Campaign> Campaign
        {
            get;
            internal set;
        }

        public Operator Operator
        {
            get
            {
                return GetFieldValue<Operator>("Operator");
            }
            set
            {
                OperandType old = new OperatorHelper().First((a) => (a.EnumValue == Operator)).OperandType;

                SetFieldValue<Operator>("Operator", value);

                if (old != new OperatorHelper().First((a) => (a.EnumValue == Operator)).OperandType)
                {
                    OperandField.TargetId = null;
                    OperandText = null;
                }
            }
        }

        public AdminObjectReference<AdminObject> OperandField
        {
            get;
            internal set;
        }

        public string OperandText
        {
            get
            {
                if (OperandField.HasTarget)
                    return OperandField.Target.TypedDisplayText;

                return GetFieldValue<string>("OperandText");
            }
            set
            {
                SetFieldValue<string>("OperandText", value);
            }
        }

        public CombineOperator CombineOperator
        {
            get
            {
                return GetFieldValue<CombineOperator>("CombineOperator");
            }
            set
            {
                SetFieldValue<CombineOperator>("CombineOperator", value);
            }
        }

        public override string TypedDisplayText
        {
            get
            {
                if(Field.HasTarget)
                    return string.Format("Filter on {0}", Field.Target.TypedDisplayText);
                return string.Empty;
            }
        }




        public bool IsFirst
        {
            get
            {
                if (!Activity.HasTarget)
                    return false;

                return Activity.Target.FilterParts.Min((a) => (a == null ? int.MaxValue : a.Sequence)) == Sequence;
            }
        }

        public bool IsLast
        {
            get
            {
                if (!Activity.HasTarget)
                    return false;
                return Activity.Target.FilterParts.Max((a) => (a == null ? int.MinValue : a.Sequence)) == Sequence;
            }
        }

        public FilterPart Previous
        {
            get
            {
                if (Activity.Target == null)
                    return null;

                int prevSequence = Activity.Target.FilterParts.Max((a) => (a.Sequence < Sequence ? a.Sequence : -1));
                return Activity.Target.FilterParts.First((a) => (a.Sequence == prevSequence));
            }
        }

        public FilterPart Next
        {
            get
            {
                if (Activity.Target == null)
                    return null;

                int nextSequence = Activity.Target.FilterParts.Min((a) => (a.Sequence > Sequence ? a.Sequence : int.MaxValue));
                return Activity.Target.FilterParts.First((a) => (a.Sequence == nextSequence));

            }
        }


        public void SaveNode(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("FilterPart");

            XmlAttribute att = doc.CreateAttribute("FieldName");
            att.Value = Field.Target.Name;
            node.Attributes.Append(att);

            att = doc.CreateAttribute("Operator");
            att.Value = Operator.ToString();
            node.Attributes.Append(att);

            if(OperandField.HasTarget)
            {
                if (OperandField.Target is Field)
                {
                    att = doc.CreateAttribute("FieldOperand");
                    if (OperandField.Target is UserField)
                    {
                        att.Value = string.Concat("CD.[", ((Field)OperandField.Target).Name, "]");
                    }
                    else
                    {
                        att.Value = string.Concat("SD.[", ((Field)OperandField.Target).Name, "]");
                    }
                    node.Attributes.Append(att);
                }
                else
                {
                    att = doc.CreateAttribute("LookupOperand");
                    att.Value = OperandField.TargetId;
                    node.Attributes.Append(att);
                }
            }
            else
            {
                att = doc.CreateAttribute("Operand");
                att.Value = OperandText;
                node.Attributes.Append(att);
            }

            att = doc.CreateAttribute("DBType");
            if (Field.Target.DBType == DBTypes.ActivityString || Field.Target.DBType == DBTypes.AgentString || Field.Target.DBType == DBTypes.AreaString || Field.Target.DBType == DBTypes.QualificationString)
                att.Value = DBTypes.String.ToString();
            else
                att.Value = Field.Target.DBType.ToString();
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


            switch (CombineOperator)
            {
                case Admin.CombineOperator.And:
                    att = doc.CreateAttribute("NextPart");
                    att.Value = "0";
                    node.Attributes.Append(att);
                    break;
                case Admin.CombineOperator.Or:
                    att = doc.CreateAttribute("NextPart");
                    att.Value = "1";
                    node.Attributes.Append(att);
                    break;
                case Admin.CombineOperator.None:
                    break;
            }

            doc.DocumentElement.AppendChild(node);
        }

        public void SaveNodeForDataManage(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("FilterPart");

            XmlAttribute att = doc.CreateAttribute("FieldName");
            if(Field.Target is SystemField)
                att.Value = string.Concat("s.[", Field.Target.Name, "]");
            else
                att.Value = string.Concat("d.[", Field.Target.Name, "]");
            node.Attributes.Append(att);

            att = doc.CreateAttribute("Operator");
            att.Value = Operator.ToString();
            node.Attributes.Append(att);

            if (OperandField.HasTarget)
            {
                if (OperandField.Target is Field)
                {
                    att = doc.CreateAttribute("FieldOperand");
                    if (OperandField.Target is UserField)
                    {
                        att.Value = string.Concat("d.[", ((Field)OperandField.Target).Name, "]");
                    }
                    else
                    {
                        att.Value = string.Concat("s.[", ((Field)OperandField.Target).Name, "]");
                    }
                    node.Attributes.Append(att);
                }
                else
                {
                    att = doc.CreateAttribute("LookupOperand");
                    att.Value = OperandField.TargetId;
                    node.Attributes.Append(att);
                }
            }
            else
            {
                att = doc.CreateAttribute("Operand");
                att.Value = OperandText;
                node.Attributes.Append(att);
            }

            if (Aggregator == Admin.Aggregator.None)
            {
                att = doc.CreateAttribute("DBType");
                if (Field.Target.DBType == DBTypes.ActivityString || Field.Target.DBType == DBTypes.AgentString || Field.Target.DBType == DBTypes.AreaString || Field.Target.DBType == DBTypes.QualificationString)
                    att.Value = DBTypes.String.ToString();
                else
                    att.Value = Field.Target.DBType.ToString();
                node.Attributes.Append(att);

            }
            else
            {
                att.Value = DBTypes.Integer.ToString();
                node.Attributes.Append(att);

                att = doc.CreateAttribute("Aggregation");
                att.Value = XmlConvert.ToString((int)Aggregator);
                node.Attributes.Append(att);
            }


            att = doc.CreateAttribute("AppointmentFunction");
            att.Value = "False";
            node.Attributes.Append(att);



            switch (CombineOperator)
            {
                case Admin.CombineOperator.And:
                    att = doc.CreateAttribute("NextPart");
                    att.Value = "0";
                    node.Attributes.Append(att);
                    break;
                case Admin.CombineOperator.Or:
                    att = doc.CreateAttribute("NextPart");
                    att.Value = "1";
                    node.Attributes.Append(att);
                    break;
                case Admin.CombineOperator.None:
                    break;
            }

            doc.DocumentElement.AppendChild(node);
        }

        protected override void SetFieldValue<T>(string index, T value)
        {
            base.SetFieldValue<T>(index, value);
            RecomputeFilters();
        }

        private void RecomputeFilters()
        {
            if (Activity != null && Activity.HasTarget)
                Activity.Target.RecomputeFilters();
        }


    }

}