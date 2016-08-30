using System.Linq;
using System;
using Nixxis;
namespace Nixxis.Client.Admin
{


    public class Phone : AdminObject
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("Phones");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


        private SingletonAdminObjectList<PhoneLocation> m_Locations = null;

        private AdminObjectReference<Location> m_Location = null;

        private SingletonAdminObjectList<PhoneResource> m_Resources = null;

        private AdminObjectReference<Resource> m_Resource = null;

        private SingletonAdminObjectList<PhoneCarrier> m_Carriers = null;

        private AdminObjectReference<Carrier> m_Carrier = null;


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

        public string ShortCode
        {
            get
            {
                return GetFieldValue<string>("ShortCode");
            }
            set
            {
                Phone phone = m_Core.Phones.FirstOrDefault((a) => (a.ShortCode!= null && a.ShortCode.Equals(value) && a.State>0));
                if (phone != null && ShortCode!=null &&  !ShortCode.Equals(value))
                {
                    throw new InvalidOperationException();
                }


                SetFieldValue("ShortCode", value);
                FirePropertyChanged("DisplayText");
                FirePropertyChanged("TypedDisplayText");
            }
        }

        public string Address
        {
            get
            {
                return GetFieldValue<string>("Address");
            }
            set
            {
                if (Resource != null && Resource.TargetId != null && (value.StartsWith("sip:") || value.Contains("@")))
                    throw new InvalidOperationException();

                SetFieldValue("Address", value);
            }
        }

        public string MacAddress
        {
            get
            {
                return GetFieldValue<string>("MacAddress");
            }
            set
            {
                SetFieldValue("MacAddress", value);
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
                    if (string.IsNullOrEmpty(Description))
                        return ShortCode;
                    else
                        return string.Format("{0} ({1})", Description, ShortCode);
            }
        }

        public override string TypedDisplayText
        {
            get
            {
                if (IsSystemWithValidOwner)
                    return Owner.TypedDisplayText;
                else
                    if (string.IsNullOrEmpty(Description))
                        return string.Concat("Phone ", ShortCode);
                    else
                        return string.Format("Phone {0} ({1})", Description, ShortCode);
            }
        }


        public Phone(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public Phone(AdminCore core)
            : base(core)
        {
            Init();
        }

        private void Init()
        {
            Locations = new SingletonAdminObjectList<PhoneLocation>(this);
            Location = new AdminObjectReference<Location>(this);
            KeepConnected = false;
        }

        [AdminLoad(SkipLoad = true)]
        public SingletonAdminObjectList<PhoneLocation> Locations
        {
            get
            {
                return m_Locations;
            }
            internal set
            {
                if (m_Locations != null)
                    m_Locations.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_Locations_CollectionChanged);

                m_Locations = value;

                if (m_Locations != null)
                    m_Locations.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_Locations_CollectionChanged);

                SyncLocationFromLocations();
            }

        }

        [AdminLoad(SkipLoad = true)]
        public SingletonAdminObjectList<PhoneResource> Resources
        {
            get
            {
                return m_Resources;
            }
            internal set
            {
                if (m_Resources != null)
                    m_Resources.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_Resources_CollectionChanged);

                m_Resources = value;

                if (m_Resources != null)
                    m_Resources.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_Resources_CollectionChanged);

                SyncResourceFromResources();
            }

        }

        void m_Resources_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SyncResourceFromResources();
        }


        void m_Locations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SyncLocationFromLocations();
        }

        [AdminLoad(SkipLoad = true)]
        public SingletonAdminObjectList<PhoneCarrier> Carriers
        {
            get
            {
                return m_Carriers;
            }
            internal set
            {
                if (m_Carriers != null)
                    m_Carriers.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_Carriers_CollectionChanged);

                m_Carriers = value;

                if (m_Carriers != null)
                    m_Carriers.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_Carriers_CollectionChanged);

                SyncResourceFromResources();
            }

        }

        void m_Carriers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SyncCarrierFromCarriers();
        }


        public AdminObjectReference<Location> Location
        {
            get
            {
                return m_Location;
            }
            internal set
            {
                if (m_Location != null)
                {
                    m_Location.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Location_PropertyChanged);
                    m_Location.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Location_PropertyChanged);
                }

                m_Location = value;

                if (m_Location != null)
                {
                    m_Location.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Location_PropertyChanged);
                    m_Location.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Location_PropertyChanged);
                }

                SyncLocationsFromLocation();
            }
        }

        public AdminObjectReference<Resource> Resource
        {
            get
            {
                return m_Resource;
            }
            internal set
            {
                if (m_Resource != null)
                {
                    m_Resource.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Resource_PropertyChanged);
                    m_Resource.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Resource_PropertyChanged);
                }

                m_Resource = value;

                if (m_Resource != null)
                {
                    m_Resource.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Resource_PropertyChanged);
                    m_Resource.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Resource_PropertyChanged);
                }

                SyncResourcesFromResource();
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

                SyncCarriersFromCarrier();
            }
        }


        void m_Location_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SyncLocationsFromLocation();
        }

        void m_Resource_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SyncResourcesFromResource();
            FirePropertyChanged("ServerUrlPart");
        }

        void m_Carrier_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SyncCarriersFromCarrier();
        }


        private bool Syncing = false;

        private void SyncLocationFromLocations()
        {
            lock (this)
            {
                if (Locations == null || Location == null || Syncing)
                    return;

                Syncing = true;
            }

            if (Locations.Count == 0)
            {
                if (Location.HasTarget)
                {
                    Location.TargetId = null;
                }
            }
            else if (Locations.Count == 1)
            {
                if (Location.HasTarget)
                {
                    if (Location.TargetId != Locations[0].LocationId)
                        Location.TargetId = Locations[0].LocationId;
                }
                else
                {
                    Location.TargetId = Locations[0].LocationId;
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
            }

            lock (this)
            {
                Syncing = false;
            }
        }

        private void SyncLocationsFromLocation()
        {
            lock (this)
            {
                if (Locations == null || Location == null || Syncing)
                    return;

                Syncing = true;
            }

            if (!Location.HasTarget)
            {
                if (Locations.Count == 0)
                {
                }
                else if (Locations.Count == 1)
                {
                    Locations.RemoveAt(0);
                }
                else if (Locations.Count > 1)
                {
                    System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
                }
            }
            else
            {
                if (Locations.Count == 0)
                {
                    Locations.Add(Location.Target);
                }
                else if (Locations.Count == 1)
                {
                    if (Locations[0].LocationId != Location.TargetId)
                    {
                        Locations.RemoveAt(0);
                        Locations.Add(Location.Target);
                    }
                }
                else if (Locations.Count > 1)
                {
                    System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
                }
            }

            lock (this)
            {
                Syncing = false;
            }
        }

        private void SyncResourceFromResources()
        {
            lock (this)
            {
                if (Resources == null || Resource == null || Syncing)
                    return;

                Syncing = true;
            }

            if (Resources.Count == 0)
            {
                if (Resource.HasTarget)
                {
                    Resource.TargetId = null;
                }
            }
            else if (Resources.Count == 1)
            {
                if (Resource.HasTarget)
                {
                    if (Resource.TargetId != Resources[0].ResourceId)
                        Resource.TargetId = Resources[0].ResourceId;
                }
                else
                {
                    Resource.TargetId = Resources[0].ResourceId;
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
            }

            lock (this)
            {
                Syncing = false;
            }
        }

        private void SyncResourcesFromResource()
        {
            lock (this)
            {
                if (Resources == null || Resource == null || Syncing)
                    return;

                Syncing = true;
            }

            if (!Resource.HasTarget)
            {
                if (Resources.Count == 0)
                {
                }
                else if (Resources.Count == 1)
                {
                    Resources.RemoveAt(0);
                }
                else if (Resources.Count > 1)
                {
                    System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
                }
            }
            else
            {
                if (Resources.Count == 0)
                {
                    Resources.Add(Resource.Target);
                }
                else if (Resources.Count == 1)
                {
                    if (Resources[0].ResourceId != Resource.TargetId)
                    {
                        Resources.RemoveAt(0);
                        Resources.Add(Resource.Target);
                    }
                }
                else if (Resources.Count > 1)
                {
                    System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
                }
            }

            lock (this)
            {
                Syncing = false;
            }
        }

        private void SyncCarrierFromCarriers()
        {
            lock (this)
            {
                if (Carriers == null || Carrier == null || Syncing)
                    return;

                Syncing = true;
            }

            if (Carriers.Count == 0)
            {
                if (Carrier.HasTarget)
                {
                    Carrier.TargetId = null;
                }
            }
            else if (Carriers.Count == 1)
            {
                if (Carrier.HasTarget)
                {
                    if (Carrier.TargetId != Carriers[0].CarrierId)
                        Carrier.TargetId = Carriers[0].CarrierId;
                }
                else
                {
                    Carrier.TargetId = Carriers[0].CarrierId;
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
            }

            lock (this)
            {
                Syncing = false;
            }
        }

        private void SyncCarriersFromCarrier()
        {
            lock (this)
            {
                if (Carriers == null || Carrier == null || Syncing)
                    return;

                Syncing = true;
            }

            if (!Carrier.HasTarget)
            {
                if (Carriers.Count == 0)
                {
                }
                else if (Carriers.Count == 1)
                {
                    Carriers.RemoveAt(0);
                }
                else if (Carriers.Count > 1)
                {
                    System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
                }
            }
            else
            {
                if (Carriers.Count == 0)
                {
                    Carriers.Add(Carrier.Target);
                }
                else if (Carriers.Count == 1)
                {
                    if (Carriers[0].CarrierId != Carrier.TargetId)
                    {
                        Carriers.RemoveAt(0);
                        Carriers.Add(Carrier.Target);
                    }
                }
                else if (Carriers.Count > 1)
                {
                    System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
                }
            }

            lock (this)
            {
                Syncing = false;
            }
        }


        public bool KeepConnected
        {
            get
            {
                return GetFieldValue<bool>("KeepConnected");
            }
            set
            {
                SetFieldValue<bool>("KeepConnected", value);
            }
        }

        public bool Register
        {
            get
            {
                return GetFieldValue<bool>("Register");
            }
            set
            {
                SetFieldValue<bool>("Register", value);
                FirePropertyChanged("ServerUrlPart");
            }
        }

        public bool ExternalLine
        {
            get
            {
                return GetFieldValue<bool>("ExternalLine");
            }
            set
            {
                SetFieldValue<bool>("ExternalLine", value);
            }
        }

        public bool AutoAnswer
        {
            get
            {
                return GetFieldValue<bool>("AutoAnswer");
            }
            set
            {
                SetFieldValue<bool>("AutoAnswer", value);
            }
        }

        public string UserAgent
        {
            get
            {
                return GetFieldValue<string>("UserAgent");
            }
            set
            {
                SetFieldValue<string>("UserAgent", value);
            }
        }

        public AdminObjectReference<Agent> AgentAssociation
        {
            get;
            internal set;
        }

        public string CallerIdentification
        {
            get
            {
                return GetFieldValue<string>("CallerIdentification");
            }
            set
            {
                SetFieldValue<string>("CallerIdentification", value);
            }
        }

        public Phone Duplicate()
        {
            Phone newPhone = AdminCore.Create<Phone>();

            newPhone.Address = Address;
            newPhone.AgentAssociation.TargetId = AgentAssociation.TargetId;
            newPhone.AutoAnswer = AutoAnswer;
            newPhone.CallerIdentification = CallerIdentification;
            newPhone.Description = string.Concat(Description, "*");
            newPhone.ExternalLine = ExternalLine;
            newPhone.GroupKey = GroupKey;
            newPhone.KeepConnected = KeepConnected;
            newPhone.Location.TargetId = Location.TargetId;
            newPhone.Resource.TargetId = Resource.TargetId;
            newPhone.ShortCode = newPhone.AdminCore.DefaultPhoneShortCode;
            newPhone.Register = Register;
            newPhone.UserAgent = UserAgent;
            newPhone.AdminCore.Phones.Add(newPhone);
            return newPhone;
        }


        public string PhoneRegisterType
        {
            get
            {
                if (Register)
                    return Translate("Phone registers on application server");
                else if (Resource.TargetId != null)
                    return Translate("Phone registers on resource");
                else
                    return Translate("Phone is external");
            }
        }

        public string ServerUrlPart
        {
            get
            {
                try
                {
                    if (Register)
                        return "@1.2.3.4";
                    else if (Resource.HasTarget)
                        return Resource.Target.OutboundGateway.Substring(Resource.Target.OutboundGateway.IndexOf('@'));
                }
                catch
                {
                }
                return string.Empty;
            }
        }
    }

    [AdminSave(SkipSave = true)]
    [AdminObjectLinkCascadeAttribute(typeof(Phone), "Resources")]
    [AdminObjectLinkCascadeAttribute(typeof(Resource), "Phones")]
    public class PhoneResource : AdminObjectLink<Phone, Resource>
    {
        public PhoneResource(AdminObject parent)
            : base(parent)
        {
        }

        public string PhoneId
        {
            get
            {
                return Id1;
            }
        }

        public string ResourceId
        {
            get
            {
                return Id2;
            }
        }


        public Phone Phone
        {
            get
            {
                if (m_Core == null)
                    return null;
                return (Phone)(m_Core.GetAdminObject(Id1));
            }
        }

        public Resource Resource
        {
            get
            {
                if (m_Core == null)
                    return null;

                return (Resource)(m_Core.GetAdminObject(Id2));
            }
        }

    }


    [AdminSave(SkipSave = true)]
    [AdminObjectLinkCascadeAttribute(typeof(Phone), "Carriers")]
    [AdminObjectLinkCascadeAttribute(typeof(Carrier), "Phones")]
    public class PhoneCarrier : AdminObjectLink<Phone, Carrier>
    {
        public PhoneCarrier(AdminObject parent)
            : base(parent)
        {
        }

        public string PhoneId
        {
            get
            {
                return Id1;
            }
        }

        public string CarrierId
        {
            get
            {
                return Id2;
            }
        }


        public Phone Phone
        {
            get
            {
                if (m_Core == null)
                    return null;
                return (Phone)(m_Core.GetAdminObject(Id1));
            }
        }

        public Carrier Carrier
        {
            get
            {
                if (m_Core == null)
                    return null;

                return (Carrier)(m_Core.GetAdminObject(Id2));
            }
        }

    }

}