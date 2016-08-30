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

    public class Carrier : AdminObject
    {
        private AdminObjectList<NumberingPlanEntry> m_NumberingPlanEntries;

        public Carrier(AdminCore core)
            : base(core)
        {
            Init();
        }

        public Carrier(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private void Init()
        {
            NumberingPlanEntries = new AdminObjectList<NumberingPlanEntry>(this);
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
                SetFieldValue<string>("GroupKey", null);
            }
        }

        public string Prefix
        {
            get
            {
                return GetFieldValue<string>("Prefix");
            }
            set
            {
                SetFieldValue<string>("Prefix", value);
            }            
        }

        public string Code
        {
            get
            {
                return GetFieldValue<string>("Code");
            }
            set
            {
                SetFieldValue<string>("Code", value);
            }
        }

        public string DefaultOriginator
        {
            get
            {
                return GetFieldValue<string>("DefaultOriginator");
            }
            set
            {
                SetFieldValue<string>("DefaultOriginator", value);
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
                    return string.Concat("Carrier ", Description);
            }
        }

        public Carrier Duplicate()
        {
            Carrier newca = AdminCore.Create<Carrier>();

            newca.Code = Code;
            newca.DefaultOriginator = DefaultOriginator;
            newca.Description = string.Concat(Description, "*");
            newca.GroupKey = GroupKey;
            newca.Prefix = Prefix;
            newca.AdminCore.Carriers.Add(newca);

            foreach (ResourceCarrier rc in Resources)
            {
                newca.Resources.Add(rc.Resource);
            }

            return newca;
        }

        [AdminLoad(Path = "/Admin/NumberingPlanEntries/NumberingPlanEntry[@carrierid=\"{0}\"]")]
        public AdminObjectList<NumberingPlanEntry> NumberingPlanEntries
        {
            get
            {
                return m_NumberingPlanEntries;
            }
            internal set
            {
                if (m_NumberingPlanEntries != null)
                    m_NumberingPlanEntries.CollectionChanged -= new NotifyCollectionChangedEventHandler(m_NumberingPlanEntries_CollectionChanged);

                m_NumberingPlanEntries = value;

                if (m_NumberingPlanEntries != null)
                    m_NumberingPlanEntries.CollectionChanged += new NotifyCollectionChangedEventHandler(m_NumberingPlanEntries_CollectionChanged);
            }
        }

        void m_NumberingPlanEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Core.FirePropertyChanged("NumberingPlanEntries");
            try
            {
                if (Core.InboundActivities != null)
                {
                    foreach (InboundActivity ia in Core.InboundActivities)
                        ia.FirePropertyChanged("NumberingPlanEntries");
                }
            }
            catch
            {
            }
        }

        public override void Clear()
        {
            while(NumberingPlanEntries.Count>0)
                Core.Delete(NumberingPlanEntries[0]);

            base.Clear();
        }

        public string DefaultNumberingPlan
        {
            get
            {
                try
                {
                    return (Int32.Parse(NumberingPlanEntries.OrderBy((np) => (np.EntryIsRegexp ? 0 : Int32.Parse(np.Entry))).Last().Entry)+1).ToString();
                }
                catch
                {
                }
                return "1000";
            }
        }

        public NumberingPlanEntry CreateNumberingPlanEntry(string destination)
        {
            NumberingPlanEntry duplicate = NumberingPlanEntries.FirstOrDefault((a) => (a.Entry!=null && a.Entry.Equals(destination)));

            if (duplicate != null)
            {
                if (!duplicate.Activity.HasTarget)
                    return duplicate;
                else
                    return null;
            }
            else
            {
                NumberingPlanEntry npe = Core.Create<NumberingPlanEntry>();

                npe.Carrier.Target = this;
                npe.Entry = destination;
                // a bit simplistic but will be just fine in most cases. For other cases, then it is still possible to go to carriers and define the numbering plan properly.
                npe.EntryIsRegexp = destination.Contains("$") || destination.Contains("^") || destination.Contains("[") || destination.Contains("]");
                npe.AutoCreated = true;
                NumberingPlanEntries.Add(npe);
                return npe;
            }
        }

        [AdminLoad(Path = "/Admin/ResourcesCarriers/ResourceCarrier[@carrierid=\"{0}\"]")]
        public AdminObjectList<ResourceCarrier> Resources
        {
            get;
            internal set;
        }

        public object CheckedResources
        {
            get
            {
                return new AdminCheckedLinkList<Resource, ResourceCarrier>(m_Core.Resources, Resources, this);
            }
        }


        [AdminLoad(SkipLoad = true)]
        public AdminObjectList<PhoneCarrier> Phones
        {
            get;
            internal set;
        }

        public object CheckedPhones
        {
            get
            {
                return new AdminCheckedLinkList<Phone, PhoneCarrier>(m_Core.Phones, Phones, this);
            }
        }

    }

    public class NumberingPlanEntry : AdminObject
    {

        private static TranslationContext m_TranslationContext = new TranslationContext("NumberingPlanEntry");

        private AdminObjectReference<InboundActivity> m_Activity;

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        public override string ShortDisplayText
        {
            get
            {
                return EntryIsRegexp ? string.Concat("[", Entry, "]") : Entry;
            }
        }

        public override string DisplayText
        {
            get
            {
                if (Carrier.HasTarget)
                    // happens upon deletion
                    return string.Format(Translate("{0} / {1}"), Carrier.Target.DisplayText, ShortDisplayText);
                else
                    return ShortDisplayText;
            }
        }

        internal string DisplayTextPrefix
        {
            get
            {
                if (Carrier.HasTarget)
                    // happens upon deletion
                    return string.Format(Translate("{0} /"), Carrier.Target.DisplayText);
                else
                    return string.Empty;
            }
        }

        public override string TypedDisplayText
        {
            get
            {
                return string.Format(Translate("Numbering plan entry {0}"), ShortDisplayText);
            }
        }


        public NumberingPlanEntry(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public NumberingPlanEntry(AdminCore core)
            : base(core)
        {
            Init();
        }

        private void Init()
        {
            Activity = new AdminObjectReference<InboundActivity>(this);
        }

        public bool EntryIsRegexp
        {
            get
            {
                return GetFieldValue<bool>("EntryIsRegexp");
            }
            set
            {
                SetFieldValue<bool>("EntryIsRegexp", value);
                FirePropertyChanged("Description");
            }
        }

        public bool AutoCreated
        {
            get
            {
                return GetFieldValue<bool>("AutoCreated");
            }
            set
            {
                SetFieldValue<bool>("AutoCreated", value);
                FirePropertyChanged("Description");
            }
        }

        public string Entry
        {
            get
            {
                return GetFieldValue<string>("Entry");
            }
            set
            {
                SetFieldValue<string>("Entry", value);
            }
        }

       
        public AdminObjectReference<Carrier> Carrier
        {
            internal set;
            get;
        }

        [AdminSave(SkipSave = true)]
        public AdminObjectReference<InboundActivity> Activity
        {
            get
            {
                return m_Activity;
            }
            internal set
            {

                if (m_Activity != null)
                {
                    m_Activity.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(ActivityPropertyChanged);
                }

                m_Activity = value;

                if (m_Activity != null)
                {
                    m_Activity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ActivityPropertyChanged);
                }
            }
        }

        protected void ActivityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (AutoCreated && !m_Activity.HasTarget && ! string.IsNullOrEmpty( m_Activity.LastTargetId ))
            {
                Core.Delete(this);
            }
        }

        public string Description
        {
            get
            {
                return string.Concat( EntryIsRegexp ? Translate("Regular expression"): Translate("Number"), AutoCreated ? Translate(" auto created"): string.Empty );
            }
        }
      
    }
}
