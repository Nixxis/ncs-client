using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
namespace Nixxis.Client.Admin
{
    public class Location : AdminObject
    {
        public override void Clear()
        {
            for(int i=0; i< m_Core.LocationCosts.Count; i++)
                if ((m_Core.LocationCosts[i].ToLocation.HasTarget && m_Core.LocationCosts[i].ToLocation.TargetId.Equals(Id)) ||
                    (m_Core.LocationCosts[i].FromLocation.HasTarget && m_Core.LocationCosts[i].FromLocation.TargetId.Equals(Id)))
                {
                    LocationCost lc = m_Core.LocationCosts[i];
                    m_Core.LocationCosts.Remove(lc);
                    m_Core.Delete(lc);
                    i--;
                }

                base.Clear();
        }

        private AdminObjectReference<NumberFormat> m_NumberFormat = null;

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
                    return string.Concat("Location ", Description);
            }
        }

        public Location(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public Location(AdminCore core)
            : base(core)
        {
            Init();
        }

        private void Init()
        {
            DefaultCost = 0;
            DynamicParameters = new AdminObjectList<DynamicParameterLink>(this);
            NumberFormat = new AdminObjectReference<NumberFormat>(this);
            Phones = new AdminObjectList<PhoneLocation>(this , false, false);
            Resources = new AdminObjectList<ResourceLocation>(this, false, false);
        }


        private AdminObjectList<DynamicParameterLink> m_DynamicParameters;

        [AdminLoad(Path = "/Admin/DynamicParameterLinks/DynamicParameterLink[@adminobjectid=\"{0}\"]")]
        public AdminObjectList<DynamicParameterLink> DynamicParameters
        {
            get
            {
                return m_DynamicParameters;
            }
            internal set
            {
                if (m_DynamicParameters != null)
                {
                    m_DynamicParameters.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_DynamicParameters_Collection_Changed);
                }

                m_DynamicParameters = value;

                if (m_DynamicParameters != null)
                {
                    m_DynamicParameters.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_DynamicParameters_Collection_Changed);
                }
            }
        }

        private void m_DynamicParameters_Collection_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
        }


        public object CheckedDynamicParameters
        {
            get
            {
                return new AdminCheckedLinkList<DynamicParameterDefinition, DynamicParameterLink>(m_Core.DynamicParameterDefinitions, DynamicParameters, this);
            }
        }

        [AdminLoad(SkipLoad = true)]
        public AdminObjectList<PhoneLocation> Phones
        {
            get;
            internal set;
        }


        [AdminLoad(SkipLoad = true)]
        public AdminObjectList<ResourceLocation> Resources
        {
            get;
            internal set;
        }

        public object CheckedPhones
        {
            get
            {
                return new AdminCheckedLinkList<Phone, PhoneLocation>(m_Core.Phones, Phones, this);
            }
        }

        public object CheckedResources
        {
            get
            {
                return new AdminCheckedLinkList<Resource, ResourceLocation>(m_Core.Resources, Resources, this);
            }
        }

        [AdminLoad(Path = "/Admin/NumberingRules/NumberingRule[@locationid=\"{0}\"]")]
        public AdminObjectList<NumberingRule> NumberingRules
        {
            get;
            internal set;
        }
       
        public IEnumerable<LocationCost> SourceCosts
        {
            get
            {
                ObservableCollection<LocationCost> temp = new ObservableCollection<LocationCost>();

                foreach( LocationCost locost in m_Core.LocationCosts.Where( (a) => (a.FromLocation.TargetId == Id) ) )
                    temp.Add(locost);

                return temp;
            }
        }

        public IEnumerable<LocationCost> DestinationCosts
        {
            get
            {
                ObservableCollection<LocationCost> temp = new ObservableCollection<LocationCost>();

                foreach( LocationCost locost in m_Core.LocationCosts.Where( (a) => (a.ToLocation.TargetId == Id) ) )
                    temp.Add(locost);

                return temp;
            }
        }

        public AdminObjectReference<NumberFormat> NumberFormat
        {
            get
            {
                return m_NumberFormat;
            }
            internal set
            {
                if (m_NumberFormat != null)
                {
                    m_NumberFormat.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_NumberFormat_PropertyChanged);
                    m_NumberFormat.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_NumberFormat_PropertyChanged);
                }

                m_NumberFormat = value;

                if (m_NumberFormat != null)
                {
                    m_NumberFormat.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_NumberFormat_PropertyChanged);
                    m_NumberFormat.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_NumberFormat_PropertyChanged);
                }
            }
        }

        void m_NumberFormat_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }


        public float DefaultCost
        {
            get
            {
                return GetFieldValue<float>("DefaultCost");
            }
            set
            {
                SetFieldValue<float>("DefaultCost", value);
            }
        }

        public Location Duplicate()
        {
            Location newLoc = AdminCore.Create<Location>();

            newLoc.DefaultCost = DefaultCost;
            newLoc.Description = string.Concat(Description, "*");
            newLoc.GroupKey = GroupKey;
            newLoc.NumberFormat.TargetId = NumberFormat.TargetId;

            foreach (NumberingRule nr in NumberingRules)
            {
                NumberingRule newnr = nr.AdminCore.Create<NumberingRule>();
                newnr.Allowed = nr.Allowed;
                newnr.Carrier.TargetId = nr.Carrier.TargetId;
                newnr.CarrierSelection.TargetId = nr.CarrierSelection.TargetId;
                newnr.Description = string.Concat(nr.Description, "*");
                newnr.Destination = nr.Destination;
                newnr.DestinationIsRegexp = nr.DestinationIsRegexp;
                newnr.DestinationReplace = nr.DestinationReplace;
                newnr.NumberingCallType = nr.NumberingCallType;
                newnr.Sequence = nr.Sequence;
                newnr.Source = nr.Source;
                newnr.SourceIsRegexp = nr.SourceIsRegexp;
                newnr.SourceReplace = nr.SourceReplace;                
                newLoc.NumberingRules.Add(newnr);
            }

            foreach (Location l in AdminCore.Locations)
            {
                LocationCost locost = AdminCore.Create<LocationCost>();
                locost.FromLocation.TargetId = newLoc.Id;
                locost.ToLocation.TargetId = l.Id;
                locost.Cost = 0;
                AdminCore.LocationCosts.Add(locost);

                locost = AdminCore.Create<LocationCost>();
                locost.ToLocation.TargetId = newLoc.Id;
                locost.FromLocation.TargetId = l.Id;
                locost.Cost = 0;
                AdminCore.LocationCosts.Add(locost);
            }

            AdminCore.Locations.Add(newLoc);
            return newLoc;
        }

    }

}