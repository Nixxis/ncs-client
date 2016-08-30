using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Nixxis;
namespace Nixxis.Client.Admin
{
    public enum NumberingCallType
    {
        InboundGeneral,
        OutboundGeneral,
        OutboundActivity,
    }

    public class NumberingRule : AdminObject
    {
        private AdminObjectReference<Carrier> m_Carrier;
        private AdminObjectReference<Carrier> m_CarrierSelection;
        private static TranslationContext m_TranslationContext = new TranslationContext("NumberingRule");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
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
                FirePropertyChanged("Explanation");
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
                    return string.Concat("Numbering rule ", Description);
            }
        }

        public NumberingRule(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public NumberingRule(AdminCore core)
            : base(core)
        {
            Init();
        }

        public void Init()
        {
            Carrier = new AdminObjectReference<Carrier>(this);
            CarrierSelection = new AdminObjectReference<Carrier>(this);
            DestinationIsRegexp = false;
            SourceIsRegexp = false;
            Destination = "*";
            Source = "*";
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

        public string Explanation
        {
            get
            {
                if (string.IsNullOrEmpty(Description))
                {
                    return string.Format("{1} -> {0}", ActionDescription, ConditionsDescription);
                }
                else
                {
                    return string.Format("{0} ({2} -> {1})", Description, ActionDescription, ConditionsDescription);
                }
            }
        }

        public string ConditionsDescription
        {
            get
            {
                switch (NumberingCallType)
                {
                    case Admin.NumberingCallType.InboundGeneral:
                        return string.Format(Translate("Inbound call from {0}{1} to {2}{3}{4}"), SourceIsRegexp ? Translate("regexp ") : string.Empty, Source, DestinationIsRegexp ? Translate("regexp ") : string.Empty, Destination, Carrier.HasTarget ? Translate(" using carrier ") + Carrier.Target.DisplayText : string.Empty);

                    case Admin.NumberingCallType.OutboundActivity:
                        return string.Format(Translate("Automated call from {0}{1} to {2}{3}{4}"), SourceIsRegexp ? Translate("regexp ") : string.Empty, Source, DestinationIsRegexp ? Translate("regexp ") : string.Empty, Destination, Carrier.HasTarget ? Translate(" using carrier ") + Carrier.Target.DisplayText : string.Empty);

                    case Admin.NumberingCallType.OutboundGeneral:
                        return string.Format(Translate("Manual call from {0}{1} to {2}{3}{4}"), SourceIsRegexp ? Translate("regexp ") : string.Empty, Source, DestinationIsRegexp ? Translate("regexp ") : string.Empty, Destination, Carrier.HasTarget ? Translate(" using carrier ") + Carrier.Target.DisplayText : string.Empty);
                }
                return string.Empty;
            }
        }

        public string ActionDescription
        {
            get
            {
                if (Allowed)
                {
                    List<string> lst = new List<string>();

                    if (!string.IsNullOrEmpty( DestinationReplace))
                        lst.Add(string.Format(Translate("Replace destination with {0}"), DestinationReplace));

                    if (!string.IsNullOrEmpty(SourceReplace))
                        lst.Add(string.Format(Translate("Replace source with {0}"), SourceReplace));

                    if (/*!string.IsNullOrEmpty(CarrierSelection)*/ CarrierSelection.HasTarget)
                        lst.Add(string.Format(Translate("Select carrier {0}"), CarrierSelection.Target.DisplayText));

                    if (lst.Count == 0)
                        return Translate("Allow the call");
                    else
                        return string.Join(", ", lst.ToArray());
                }
                else
                {
                    return Translate("Do not allow the call");
                }
            }            
        }

        public NumberingCallType NumberingCallType
        {
            get
            {
                return GetFieldValue<NumberingCallType>("NumberingCallType");
            }
            set
            {
                SetFieldValue<NumberingCallType>("NumberingCallType", value);
                FirePropertyChanged("ConditionsDescription");
                FirePropertyChanged("Explanation");
            }
        }

        public bool SourceIsRegexp
        {
            get
            {
                return GetFieldValue<bool>("SourceIsRegexp");
            }
            set
            {
                SetFieldValue<bool>("SourceIsRegexp", value);
                FirePropertyChanged("ConditionsDescription");
                FirePropertyChanged("Explanation");
            }
        }

        public bool DestinationIsRegexp
        {
            get
            {
                return GetFieldValue<bool>("DestinationIsRegexp");
            }
            set
            {
                SetFieldValue<bool>("DestinationIsRegexp", value);
                FirePropertyChanged("ConditionsDescription");
                FirePropertyChanged("Explanation");
            }
        }

        public bool Allowed
        {
            get
            {
                return GetFieldValue<bool>("Allowed");
            }
            set
            {
                SetFieldValue<bool>("Allowed", value);
                FirePropertyChanged("ActionDescription");
                FirePropertyChanged("Explanation");
            }
        }

        public string Source
        {
            get
            {
                return GetFieldValue<string>("Source");
            }
            set
            {
                SetFieldValue<string>("Source", value);
                FirePropertyChanged("ConditionsDescription");
                FirePropertyChanged("Explanation");
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
                SetFieldValue<string>("Destination", value);
                FirePropertyChanged("ConditionsDescription");
                FirePropertyChanged("Explanation");
            }
        }

        public string SourceReplace
        {
            get
            {
                return GetFieldValue<string>("SourceReplace");
            }
            set
            {
                SetFieldValue<string>("SourceReplace", value);
                FirePropertyChanged("ActionDescription");
                FirePropertyChanged("Explanation");
            }
        }

        public string DestinationReplace
        {
            get
            {
                return GetFieldValue<string>("DestinationReplace");
            }
            set
            {
                SetFieldValue<string>("DestinationReplace", value);
                FirePropertyChanged("ActionDescription");
                FirePropertyChanged("Explanation");
            }
        }

        public AdminObjectReference<Carrier> CarrierSelection
        {
            get
            {
                return m_CarrierSelection;
            }
            internal set
            {
                if (m_CarrierSelection != null)
                {
                    m_CarrierSelection.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_CarrierSelection_PropertyChanged);
                    m_CarrierSelection.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_CarrierSelection_PropertyChanged);
                }

                m_CarrierSelection = value;

                if (m_CarrierSelection != null)
                {
                    m_CarrierSelection.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_CarrierSelection_PropertyChanged);
                    m_CarrierSelection.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_CarrierSelection_PropertyChanged);
                }

                FirePropertyChanged("ActionDescription");
                FirePropertyChanged("Explanation");

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
                if (m_Carrier != null)
                {
                    m_Carrier.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Carrier_PropertyChanged);
                    m_Carrier.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Carrier_PropertyChanged);
                }

                m_Carrier = value;

                if (m_Carrier != null)
                {
                    m_Carrier.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Carrier_PropertyChanged);
                    m_Carrier.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Carrier_PropertyChanged);
                }

                FirePropertyChanged("ConditionsDescription");
                FirePropertyChanged("Explanation");
            }
        }

        void m_Carrier_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FirePropertyChanged("ConditionsDescription");
            FirePropertyChanged("Explanation");

        }

        void m_CarrierSelection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FirePropertyChanged("ActionDescription");
            FirePropertyChanged("Explanation");

        }


        public Location ParentLocation
        {
            get
            {
                return (Location)(((AdminObjectList<NumberingRule>)Parent).Parent);
            }
        }

        public bool IsFirst
        {
            get
            {
                return ((Location)(((AdminObjectList<NumberingRule>)Parent).Parent)).NumberingRules.Min((a) => (a.Sequence)) == Sequence;
            }
        }

        public bool IsLast
        {
            get
            {
                return ((Location)(((AdminObjectList<NumberingRule>)Parent).Parent)).NumberingRules.Max((a) => (a.Sequence)) == Sequence;
            }
        }

        public NumberingRule Previous
        {
            get
            {
                if (Parent == null || Parent.Parent == null)
                    return null;
                int prevSequence = ((Location)(((AdminObjectList<NumberingRule>)Parent).Parent)).NumberingRules.Max((a) => ( a.Sequence < Sequence ? a.Sequence : -1 ));
                return ((Location)(((AdminObjectList<NumberingRule>)Parent).Parent)).NumberingRules.First((a) => (a.Sequence == prevSequence));
            }
        }
        
        public NumberingRule Next
        {
            get
            {
                if (Parent == null || Parent.Parent == null)
                    return null;
                int nextSequence = ((Location)(((AdminObjectList<NumberingRule>)Parent).Parent)).NumberingRules.Min((a) => (a.Sequence > Sequence ? a.Sequence : int.MaxValue));
                return ((Location)(((AdminObjectList<NumberingRule>)Parent).Parent)).NumberingRules.First((a) => (a.Sequence == nextSequence));

            }
        }

        private string m_BackupLocationId = null;
        public override void Clear()
        {
            m_BackupLocationId = ParentLocation.Id;
            base.Clear();            

        }

        protected override XmlElement CreateSaveNode(XmlDocument doc, string operation)
        {
            XmlElement node = base.CreateSaveNode(doc, operation);

            XmlAttribute att = doc.CreateAttribute("locationid");
            if(operation=="delete")
                att.Value = m_BackupLocationId;
            else
                att.Value = ParentLocation.Id;
            node.Attributes.Append(att);

            return node;
        }

        public NumberingRule Duplicate()
        {

            NumberingRule newnr = AdminCore.Create<NumberingRule>();
            newnr.Allowed = Allowed;
            newnr.Carrier.TargetId = Carrier.TargetId;
            newnr.CarrierSelection.TargetId = CarrierSelection.TargetId;
            newnr.Description = string.Concat(Description, "*");
            newnr.Destination = Destination;
            newnr.DestinationIsRegexp = DestinationIsRegexp;
            newnr.DestinationReplace = DestinationReplace;
            newnr.NumberingCallType = NumberingCallType;

            if (ParentLocation.NumberingRules.Count > 0)
                newnr.Sequence = ParentLocation.NumberingRules.OrderBy((a) => (a.Sequence)).Last().Sequence + 1;


            newnr.Source = Source;
            newnr.SourceIsRegexp = SourceIsRegexp;
            newnr.SourceReplace = SourceReplace;

            ParentLocation.NumberingRules.Add(newnr);
            return newnr;
        }
    }

}