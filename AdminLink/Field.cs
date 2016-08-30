using System.Linq;
using System.Data;
using System;
using System.Xml;
using ContactRoute;
namespace Nixxis.Client.Admin
{
    public enum LookupTypes
    {
        None = 0,
        Agents,
        Activities,
        Qualifications,
        Area
    }

    public enum UserFieldMeanings
    {
        /// <summary>
        /// The field has no meaning.
        /// </summary>
        None = 0,
        /// <summary>
        /// The field is considered as a customer identification.
        /// </summary>
        CustomerId,
        /// <summary>
        /// The field represents a contact description.
        /// </summary>
        ContactDescription,
        /// <summary>
        /// The field is considered as potential profit.
        /// </summary>
        PotentialProfit,
        /// <summary>
        /// The field is considered as an home phone number.
        /// </summary>
        HomePhoneNumber,
        /// <summary>
        /// The field is considered as a business phone number.
        /// </summary>
        BusinessPhoneNumber,
        /// <summary>
        /// The field is considered as a private mobile phone number.
        /// </summary>
        HomeMobileNumber,
        /// <summary>
        /// The field is consisered as a business mobile phone number.
        /// </summary>
        BusinessMobileNumber,
        /// <summary>
        /// The field is considered as a private email address.
        /// </summary>
        HomeEmail,
        /// <summary>
        /// The field will be interpreted as a business email address.
        /// </summary>
        BusinessEmail,
        /// <summary>
        /// The field will be considered as a private chat address.
        /// </summary>
        HomeChat,
        /// <summary>
        /// The field is considered as a business chat address.
        /// </summary>
        BusinessChat,
        /// <summary>
        /// The field is considered as a private fax number.
        /// </summary>
        HomeFax,
        /// <summary>
        /// The field is considered as a business fax number.
        /// </summary>
        BusinessFax,
        /// <summary>
        /// The field contains the maximum number of dial attempts.
        /// </summary>
        MaxDial,
        /// <summary>
        /// The field contains an appointment identifier.
        /// </summary>
        AppointmentId,
        /// <summary>
        /// The field contains an apointement area.
        /// </summary>
        AppointmentArea,
        /// <summary>
        /// The field contains an appointment datetime.
        /// </summary>
        AppointmentDateTime,
        /// <summary>
        /// The field contains an address (part 1).
        /// </summary>
        Address1,
        /// <summary>
        /// The field contains an address (part 2).
        /// </summary>
        Address2,
        /// <summary>
        /// The field contains a postal code.
        /// </summary>
        PostalCode,
        /// <summary>
        /// The field represents a country name.
        /// </summary>
        Country,
        /// <summary>
        /// The field represents a city.
        /// </summary>
        City,
        /// <summary>
        /// The field represents a region.
        /// </summary>
        Region,
        /// <summary>
        /// The field contains latitude value.
        /// </summary>
        Latitude,
        /// <summary>
        /// The field contains longitude value.
        /// </summary>
        Longitude,
        OptInDate,
        OptInStatus,
        Quota
    }

    public enum DBTypes
    {
        /// <summary>
        /// Unknown type.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Boolean type.
        /// </summary>
        Boolean = 1,
        /// <summary>
        /// Integer type.
        /// </summary>
        Integer = 2,
        /// <summary>
        /// Datetime type.
        /// </summary>
        Datetime = 3,
        /// <summary>
        /// Float type.
        /// </summary>
        Float = 4,
        /// <summary>
        /// Char type.
        /// </summary>
        Char = 5,
        /// <summary>
        /// String type.
        /// </summary>
        String = 6,
        Time,
        AgentString,
        ActivityString,
        AreaString, 
        QualificationString
    }

    public enum SystemFieldMeanings
    {
        Internal__Id__,
        CurrentActivity,
        PreviousActivity,
        LastHandlerActivity,
        LastContactId,
        LastActivityChange,
        LastHandler,
        LastHandlingTime,
        LastHandlingDuration,
        TotalHandlingDuration,
        TotalHandlers,
        State,
        PreviousState,
        SortInfo,
        CustomSortInfo,
        Priority,
        DialingModeOverride,
        DialStartDate,
        DialEndDate,
        CreationTime,
        ImportSequence,
        ImportTag,
        ExportSequence,
        RecycleCount,
        LastRecycle,
        ExportTime,
        TargetHandler,
        PreferredAgent,
        TargetDestination,
        TargetMedia,
        DialedCurrentActivity,
        TotalDialed,
        MaxDialAttempts,
        ExpectedProfit,
        LastDialStatus,
        LastDialStatusCount,
        LastDialedDestination,
        LastQualification,
        LastQualificationArgued,
        LastQualificationPositive,
        LastQualificationExportable,
        AreaId,
        AppointmentId,
        Excluded,
        VMFlagged
    }

    public enum QuotaFieldMeanings
    {
        MaxAbsoluteProgress,
        MaxAbsoluteRemaining,
        MaxRelativeProgress,
        MaxRelativeRemaining,
        MinAbsoluteProgress,
        MinAbsoluteRemaining,
        MinRelativeProgress,
        MinRelativeRemaining,
    }

    public class Field : AdminObject
    {
        protected string m_LoadedName;

        public void SetNewFieldName(string value)
        {
            // TODO: is it the right way? This allows the usage of invalid characters
            SetFieldValue<string>("Name", value);
        }
        public void SetOriginalFieldName(string value)
        {
            // TODO: is it the right way? This allows the usage of invalid characters
            SetFieldValue<string>("InitialName", value);
        }
        
        public string Name
        {
            get
            {
                return GetFieldValue<string>("Name") ;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if ("0123456789".IndexOf(value[0]) >= 0)
                        throw new NotSupportedException();

                    foreach (char c in value)
                    {
                        if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789".IndexOf(c) < 0)
                            throw new NotSupportedException();
                    }
                }

                if (Parent is Campaign)
                {
                    Campaign c = (Campaign)Parent;
                    if (c.UserFields.Select((uf) => (uf.Name)).Contains(value))
                        throw new NotSupportedException();
                }

                if (InitialName == null || InitialName.Equals(Name))
                    InitialName = value;

                SetFieldValue<string>("Name", value);
                   
            }
        }

        public string InitialName
        {
            get
            {
                return GetFieldValue<string>("InitialName") ;
            }
            set
            {
                SetFieldValue<string>("InitialName", value);
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public DBTypes DBType
        {
            get
            {
                return GetFieldValue<DBTypes>("DBType");
            }
            set
            {
                SetFieldValue<DBTypes>("DBType", value);
                if (value != DBTypes.String)
                {
                    Length = -1;
                }
            }
        }

        public int Length
        {
            get
            {
                return GetFieldValue<int>("Length");
            }
            set
            {
                SetFieldValue<int>("Length", value);
            }
        }

        public void SetUniqueConstraint(bool value)
        {
            IsUniqueConstraint = value;
        }

        public void SetIndexed(bool value)
        {
            IsIndexed = value;
        }

        public bool IsUniqueConstraint
        {
            get
            {
                return GetFieldValue<bool>("IsUniqueConstraint");
            }
            set
            {
                SetFieldValue<bool>("IsUniqueConstraint", value);
                if (value && !IsIndexed)
                    IsIndexed = true;
            }
        }

        public bool IsIndexed
        {
            get
            {
                return GetFieldValue<bool>("IsIndexed");
            }
            set
            {
                SetFieldValue<bool>("IsIndexed", value);
            }
        }

        public override string ShortDisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.ShortDisplayText;
                else
                    return Name;
            }
        }

        public Field(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public Field(AdminCore core)
            : base(core)
        {
            Init();
        }

        protected virtual void Init()
        {
            Length = -1;
            DBType = DBTypes.String;
            Campaign = new AdminObjectReference<Campaign>(this);
        }

        internal override void Load(XmlElement node)
        {
            Campaign.TargetId = node.SelectSingleNode("ancestor::Campaign").Attributes["id"].Value;
            Campaign.DoneLoading();

            base.Load(node);

            m_LoadedName = Name;
        }

        internal void Load(string campaignid, XmlElement node)
        {
            Campaign.TargetId = campaignid;
            Campaign.DoneLoading();

            base.Load(node);

            m_LoadedName = Name;
        }

        public AdminObjectReference<Campaign> Campaign
        {
            get;
            internal set;
        }

        public AdminObjectList<SortField> ActivitiesSortFields
        {
            get;
            internal set;
        }

        public AdminObjectList<FilterPart> ActivitiesFilterParts
        {
            get;
            internal set;
        }

        protected void RecomputeFieldsConfig()
        {
            if (Campaign != null && Campaign.HasTarget)
                Campaign.Target.RecomputeFieldsConfig();
        }

        protected void RecomputeQuotaFields()
        {
            if (Campaign != null && Campaign.HasTarget)
                Campaign.Target.FirePropertyChanged("QuotaFields");
        }
    }

    [AdminSave(SkipSave = true)]
    public class SystemField : Field
    {
        public SystemField(AdminObject parent) : base(parent)
        {
        }

        public SystemField(AdminCore core)  : base(core)
        {
        }

        public SystemField(AdminCore core, SystemFieldMeanings meaning)
            : base(core)
        {
            Init();
            FieldMeaning = meaning;
            Name = meaning.ToString();
            Id = string.Concat("z1000000000000000000000000000", (100 + (int)meaning).ToString());
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public SystemFieldMeanings FieldMeaning
        {
            get
            {
                return GetFieldValue<SystemFieldMeanings>("FieldMeaning");
            }
            set
            {
                SetFieldValue<SystemFieldMeanings>("FieldMeaning", value);
            }
        }

        public override string TypedDisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.TypedDisplayText;
                else
                    return string.Concat("System field ", DisplayText);
            }
        }

        public override string DisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.DisplayText;
                else
                    return string.Format("{0} ({1})", Name, new SystemFieldMeaningHelper().Single( (a) => (a.EnumValue==FieldMeaning ) ).Description);
            }
        }

        public LookupTypes Lookup
        {
            get
            {
                return GetFieldValue<LookupTypes>("Lookup");
            }
            set
            {
                SetFieldValue<LookupTypes>("Lookup", value);
            }
        }

    }



    [AdminSave(SkipSave = true)]
    public class QuotaField : Field
    {
        public QuotaField(AdminObject parent) : base(parent)
        {
        }

        public QuotaField(AdminCore core)  : base(core)
        {
        }

        public QuotaField(AdminCore core, QuotaFieldMeanings meaning)
            : base(core)
        {
            Init();
            FieldMeaning = meaning;
            Name = meaning.ToString();
            Id = string.Concat("w1000000000000000000000000000", (100 + (int)meaning).ToString());
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public QuotaFieldMeanings FieldMeaning
        {
            get
            {
                return GetFieldValue<QuotaFieldMeanings>("FieldMeaning");
            }
            set
            {
                SetFieldValue<QuotaFieldMeanings>("FieldMeaning", value);
            }
        }

        public override string TypedDisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.TypedDisplayText;
                else
                    return string.Concat("Quota combination ", DisplayText);
            }
        }

        public override string DisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.DisplayText;
                else
                    return string.Format("{0} ({1})", Name, new QuotaFieldMeaningHelper().Single( (a) => (a.EnumValue==FieldMeaning ) ).Description);
            }
        }

        public LookupTypes Lookup
        {
            get
            {
                return GetFieldValue<LookupTypes>("Lookup");
            }
            set
            {
                SetFieldValue<LookupTypes>("Lookup", value);
            }
        }

    }

  
    public class UserField : Field
    {
        public bool AllowRecomputations = true;

        protected override void SetFieldValue<T>(string index, T value)
        {

            base.SetFieldValue<T>(index, value);

            if (AllowRecomputations && index!="QuotaDefinition")
            {
                RecomputeFieldsConfig();
                RecomputeQuotaFields();
            }
        }

        public UserField(AdminObject parent) : base(parent)
        {
        }

        public UserField(AdminCore core)  : base(core)
        {
        }

        protected override void Init()
        {
            base.Init();
            FieldMeaning = UserFieldMeanings.None;
        }

        public void SetMeaning(int meaning)
        {
            FieldMeaning = (UserFieldMeanings)meaning;
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public UserFieldMeanings FieldMeaning
        {
            get
            {
                return GetFieldValue<UserFieldMeanings>("FieldMeaning");
            }
            set
            {
                SetFieldValue<UserFieldMeanings>("FieldMeaning", value);
                if (value == UserFieldMeanings.Quota)
                {
                    QuotaComputationMethod = QuotaComputationMethod.Unknown;
                    QuotaTargetComputationMethod = Admin.QuotaTargetComputationMethod.CountQualified;
                    QuotaTarget = 0;
                }
            }
        }

        public override string TypedDisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.TypedDisplayText;
                else
                    return string.Concat("User field ", DisplayText);
            }
        }

        public override string DisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.DisplayText;
                else
                    if (FieldMeaning != UserFieldMeanings.None)
                        return string.Format("{0} ({1})", Name, new UserFieldMeaningHelper().Single((a) => (a.EnumValue == FieldMeaning)).Description);
                    else
                        return Name;
            }
        }

        public System.Xml.XmlElement SaveNode(System.Xml.XmlDocument doc)
        {
            XmlElement elm = doc.CreateElement("FieldConfig");

            XmlElement temp = doc.CreateElement("OriginalFieldName");

            temp.InnerText = InitialName;
            elm.AppendChild(temp);

            temp = doc.CreateElement("NewFieldName");
            temp.InnerText = Name;
            elm.AppendChild(temp);

            temp = doc.CreateElement("Skip");
            temp.InnerText = XmlConvert.ToString(false) ;
            elm.AppendChild(temp);

            temp = doc.CreateElement("ForceValue");
            temp.InnerText = XmlConvert.ToString(false);
            elm.AppendChild(temp);
            
            temp = doc.CreateElement("ForcedValue");           
            elm.AppendChild(temp);

            temp = doc.CreateElement("DBType");
            temp.InnerText = XmlConvert.ToString((int)(DBType));
            elm.AppendChild(temp);

            temp = doc.CreateElement("Length");
            temp.InnerText = XmlConvert.ToString(Length);
            elm.AppendChild(temp);

            temp = doc.CreateElement("Meaning");
            temp.InnerText = XmlConvert.ToString((int)(FieldMeaning));
            elm.AppendChild(temp);

            temp = doc.CreateElement("Description");
            elm.AppendChild(temp);

            temp = doc.CreateElement("MultiSelection");
            temp.InnerText = XmlConvert.ToString(false);
            elm.AppendChild(temp);

            temp = doc.CreateElement("ChoicesAreLookups");
            temp.InnerText = XmlConvert.ToString(false);
            elm.AppendChild(temp);

            temp = doc.CreateElement("AppointmentFunction");
            temp.InnerText = XmlConvert.ToString(false);
            elm.AppendChild(temp);

            temp = doc.CreateElement("IsUniqueConstraint");
            temp.InnerText = XmlConvert.ToString(IsUniqueConstraint);
            elm.AppendChild(temp);

            temp = doc.CreateElement("Indexed");
            temp.InnerText = XmlConvert.ToString(IsIndexed);
            elm.AppendChild(temp);

            if (QuotaDefinition != null && QuotaDefinition.DocumentElement!=null)
            {
                temp = doc.CreateElement("Quota");
                temp.AppendChild(doc.ImportNode(QuotaDefinition.DocumentElement, true));
                elm.AppendChild(temp);
            }


            doc.DocumentElement.AppendChild(elm);

            return elm;
        }

        protected override XmlElement CreateSaveNode(XmlDocument doc, string operation)
        {

            XmlElement node = base.CreateSaveNode(doc, operation);

            XmlAttribute att = doc.CreateAttribute("campaignid");
            att.Value = Campaign.TargetId ?? Campaign.OriginalTargetId;
            node.Attributes.Append(att);

            // TODO: that is enough for deletion but it will be problematic for field rename... 
            // should get the "original" field name but even then then an alter table will not be possible...
            att = doc.CreateAttribute("name");
            att.Value = Name ?? m_LoadedName;            
            node.Attributes.Append(att);

            return node;
        }

        public UserField Duplicate(Campaign newCamp)
        {
            UserField newField = AdminCore.Create<UserField>(newCamp);

            newField.FieldMeaning = FieldMeaning;
            newField.InitialName = InitialName;
            newField.IsUniqueConstraint = IsUniqueConstraint;
            newField.Length = Length;
            newField.Name = Name;
            newField.DBType = DBType;
            newField.QuotaDefinition = QuotaDefinition;
            return newField;
        }

        public QuotaComputationMethod QuotaComputationMethod
        {
            get
            {
                if (QuotaDefinition == null)
                    return QuotaComputationMethod.DirrectValue;
                try
                {
                    return (QuotaComputationMethod)int.Parse(QuotaDefinition.SelectSingleNode("QuotaDefinition/ComputationMethod").InnerText);
                }
                catch
                {
                    return QuotaComputationMethod.Unknown;
                }
            }
            set
            {
                XmlNode tempNode = null;


                if (QuotaDefinition == null)
                    QuotaDefinition = new XmlDocument();

                XmlDocument newDoc = new XmlDocument();
                if(!string.IsNullOrEmpty(QuotaDefinition.OuterXml))
                    newDoc.LoadXml(QuotaDefinition.OuterXml);

                tempNode = newDoc.SelectSingleNode("QuotaDefinition");

                if(tempNode==null)
                    tempNode = newDoc.AppendChild(newDoc.CreateElement("QuotaDefinition"));

                tempNode = tempNode.SelectSingleNode("ComputationMethod");

                if (tempNode == null)
                    tempNode = newDoc.DocumentElement.AppendChild(newDoc.CreateElement("ComputationMethod"));

                if(tempNode !=null)
                    tempNode.InnerText = XmlConvert.ToString((int)value);

                // will trigger the change...
                
                QuotaDefinition = newDoc;

                FirePropertyChanged("QuotaComputationMethod");
                FirePropertyChanged("QuotaComputationMethodReadOnly");
                
            } 
        }

        public bool QuotaComputationMethodReadOnly
        {
            get
            {
                if (QuotaComputationMethod == ContactRoute.QuotaComputationMethod.Unknown)
                    return false;
                if (GetOriginalFieldValue<XmlDocument>("QuotaDefinition") == null || (QuotaComputationMethod)int.Parse(GetOriginalFieldValue<XmlDocument>("QuotaDefinition").SelectSingleNode("QuotaDefinition/ComputationMethod").InnerText) == ContactRoute.QuotaComputationMethod.Unknown)
                    return false;
                else
                    return true;
            }
        }

        public QuotaTargetComputationMethod QuotaTargetComputationMethod
        {
            get
            {
                if (QuotaDefinition == null)
                    return Admin.QuotaTargetComputationMethod.CountQualified;
                try
                {
                    return (QuotaTargetComputationMethod)int.Parse(QuotaDefinition.SelectSingleNode("QuotaDefinition/TargetComputationMethod").InnerText);
                }
                catch
                {
                    return QuotaTargetComputationMethod.CountQualified;
                }
            }
            set
            {
                XmlNode tempNode = null;


                if (QuotaDefinition == null)
                    QuotaDefinition = new XmlDocument();

                XmlDocument newDoc = new XmlDocument();
                if (!string.IsNullOrEmpty(QuotaDefinition.OuterXml))
                    newDoc.LoadXml(QuotaDefinition.OuterXml);


                tempNode = newDoc.SelectSingleNode("QuotaDefinition");

                if (tempNode == null)
                    tempNode = newDoc.AppendChild(newDoc.CreateElement("QuotaDefinition"));

                tempNode = tempNode.SelectSingleNode("TargetComputationMethod");

                if (tempNode == null)
                    tempNode = newDoc.DocumentElement.AppendChild(newDoc.CreateElement("TargetComputationMethod"));

                if (tempNode != null)
                    tempNode.InnerText = XmlConvert.ToString((int)value);

                // will trigger the change...
                QuotaDefinition = newDoc;

                FirePropertyChanged("QuotaTargetComputationMethod");

            }
        }

        public string QuotaFormula
        {
            get
            { 
            if (QuotaDefinition == null)
                    return null;
                try
                {
                    return QuotaDefinition.SelectSingleNode("QuotaDefinition/QuotaFormula").InnerText;
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                XmlNode tempNode = null;


                if (QuotaDefinition == null)
                    QuotaDefinition = new XmlDocument();

                XmlDocument newDoc = new XmlDocument();
                if (!string.IsNullOrEmpty(QuotaDefinition.OuterXml))
                    newDoc.LoadXml(QuotaDefinition.OuterXml);


                tempNode = newDoc.SelectSingleNode("QuotaDefinition");

                if (tempNode == null)
                    tempNode = newDoc.AppendChild(newDoc.CreateElement("QuotaDefinition"));

                tempNode = tempNode.SelectSingleNode("QuotaFormula");

                if (tempNode == null)
                    tempNode = newDoc.DocumentElement.AppendChild(newDoc.CreateElement("QuotaFormula"));

                if (tempNode != null)
                    tempNode.InnerText = value;

                // will trigger the change...
                
                
                QuotaDefinition = newDoc;

                FirePropertyChanged("QuotaFormula");
            }

        }

        public int QuotaTarget
        {
            get
            {
                if (QuotaDefinition == null)
                    return 0;
                try
                {
                    return int.Parse(QuotaDefinition.SelectSingleNode("QuotaDefinition/Target").InnerText);
                }
                catch
                {
                    return 0;
                }

            }
            set
            {

                XmlNode tempNode = null;

                if (QuotaDefinition == null)
                    QuotaDefinition = new XmlDocument();

                XmlDocument newDoc = new XmlDocument();
                if (!string.IsNullOrEmpty(QuotaDefinition.OuterXml))
                    newDoc.LoadXml(QuotaDefinition.OuterXml);


                tempNode = newDoc.SelectSingleNode("QuotaDefinition");



                if (tempNode == null)
                    tempNode = newDoc.AppendChild(newDoc.CreateElement("QuotaDefinition"));

                tempNode = tempNode.SelectSingleNode("Target");

                if (tempNode == null)
                    tempNode = newDoc.DocumentElement.AppendChild(newDoc.CreateElement("Target"));
                            
                if (tempNode != null)
                    tempNode.InnerText = XmlConvert.ToString(value);

                // will trigger the change...
                QuotaDefinition = newDoc;

                FirePropertyChanged("QuotaTarget");
            }
        }

        public XmlDocument QuotaDefinition
        {
            get
            {
                return GetFieldValue<XmlDocument>("QuotaDefinition");
            }
            set
            {
                SetFieldValue<XmlDocument>("QuotaDefinition", value);
            } 

        }

        public void SetQuota(XmlDocument value)
        {
            QuotaDefinition = value;
        }

        public override void Save(XmlDocument doc)
        {
            base.Save(doc);
        }

    }

    public enum QuotaTargetComputationMethod
    {
        CountQualified,
        CountPositive,
        CountNegative,
        CountNeutral,
        CountArgued,
        SumQualificationValue,
        SumPositiveQualificationValue
    }

}