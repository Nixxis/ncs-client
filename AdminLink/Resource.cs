using System.Collections.Generic;
namespace Nixxis.Client.Admin
{
    public class Resource : AdminObject
    {
        private SingletonAdminObjectList<ResourceLocation> m_Locations = null;
        private AdminObjectReference<Location> m_Location = null;


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
                    return string.Concat("Resource ", Description);
            }
        }


        public Resource(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public Resource(AdminCore core)
            : base(core)
        {
            Init();
        }

        private void Init()
        {
            Locations = new SingletonAdminObjectList<ResourceLocation>(this);
            Location = new AdminObjectReference<Location>(this);
            Phones = new AdminObjectList<PhoneResource>(this, false, false);

            MaxInLinesLimited = false;
            MaxOutLinesLimited = false;
            MaxLinesLimited = false;
        }

        public string BaseUri
        {
            get
            {
                return GetFieldValue<string>("BaseUri");
            }
            set
            {
                string oldBaseUriIP = BaseUriIP;

                SetFieldValue<string>("BaseUri",  value.Trim());

                if (HoldMusicPlayer!=null && oldBaseUriIP!=null && HoldMusicPlayer.EndsWith(oldBaseUriIP))
                    HoldMusicPlayer = string.Concat(HoldMusicPlayer.Substring(0, HoldMusicPlayer.Length - oldBaseUriIP.Length), BaseUriIP);
                if (Ringer != null && oldBaseUriIP != null && Ringer.EndsWith(oldBaseUriIP))
                    Ringer = string.Concat(Ringer.Substring(0, Ringer.Length - oldBaseUriIP.Length), BaseUriIP);
                if (Announcer != null && oldBaseUriIP != null && Announcer.EndsWith(oldBaseUriIP))
                    Announcer = string.Concat(Announcer.Substring(0, Announcer.Length - oldBaseUriIP.Length), BaseUriIP);
                if (QueueLoopPlayer != null && oldBaseUriIP != null && QueueLoopPlayer.EndsWith(oldBaseUriIP))
                    QueueLoopPlayer = string.Concat(QueueLoopPlayer.Substring(0, QueueLoopPlayer.Length - oldBaseUriIP.Length), BaseUriIP);
                if (OutboundGateway != null && oldBaseUriIP != null && OutboundGateway.EndsWith(oldBaseUriIP))
                    OutboundGateway = string.Concat(OutboundGateway.Substring(0, OutboundGateway.Length - oldBaseUriIP.Length), BaseUriIP);
                if (AnsweringMachineDetector != null && oldBaseUriIP != null && AnsweringMachineDetector.EndsWith(oldBaseUriIP))
                    AnsweringMachineDetector = string.Concat(AnsweringMachineDetector.Substring(0, AnsweringMachineDetector.Length - oldBaseUriIP.Length), BaseUriIP);
                if (ConferenceBridge != null && oldBaseUriIP != null && ConferenceBridge.EndsWith(oldBaseUriIP))
                    ConferenceBridge = string.Concat(ConferenceBridge.Substring(0, ConferenceBridge.Length - oldBaseUriIP.Length), BaseUriIP);
                if (IvrPlayer != null && oldBaseUriIP != null && IvrPlayer.EndsWith(oldBaseUriIP))
                    IvrPlayer = string.Concat(IvrPlayer.Substring(0, IvrPlayer.Length - oldBaseUriIP.Length), BaseUriIP);
                if (Monitoring != null && oldBaseUriIP != null && Monitoring.EndsWith(oldBaseUriIP))
                    Monitoring = string.Concat(Monitoring.Substring(0, Monitoring.Length - oldBaseUriIP.Length), BaseUriIP);
                if (Recording != null && oldBaseUriIP != null && Recording.Contains(oldBaseUriIP))
                    if(!string.IsNullOrEmpty(oldBaseUriIP))
                        Recording = Recording.Replace(oldBaseUriIP, BaseUriIP);
                if (RecordingPlayer != null && oldBaseUriIP != null && RecordingPlayer.EndsWith(oldBaseUriIP))
                    RecordingPlayer = string.Concat(RecordingPlayer.Substring(0, RecordingPlayer.Length - oldBaseUriIP.Length), BaseUriIP);
                if (SSHSettings != null && oldBaseUriIP != null && SSHSettings.Contains(oldBaseUriIP))
                    if (!string.IsNullOrEmpty(oldBaseUriIP))
                        SSHSettings = SSHSettings.Replace(oldBaseUriIP, BaseUriIP);

            }
        }

        public string BaseUriIP
        {
            get
            {
                if (BaseUri!=null && BaseUri.StartsWith("sip:"))
                    return BaseUri.Substring("sip:".Length);
                else
                    return BaseUri;
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

        
        public string HoldMusicPlayer
        {
            get
            {
                return GetFieldValue<string>("HoldMusicPlayer");
            }
            set
            {
                
                SetFieldValue<string>("HoldMusicPlayer", value==null ? null : value.Trim());
            }
        }
        
        public string Ringer
        {
            get
            {
                return GetFieldValue<string>("Ringer");
            }
            set
            {
                SetFieldValue<string>("Ringer", value == null ? null : value.Trim());
            }
        }
        
        public string Announcer
        {
            get
            {
                return GetFieldValue<string>("Announcer");
            }
            set
            {
                SetFieldValue<string>("Announcer", value == null ? null : value.Trim());
            }
        }
        
        public string QueueLoopPlayer
        {
            get
            {
                return GetFieldValue<string>("QueueLoopPlayer");
            }
            set
            {
                SetFieldValue<string>("QueueLoopPlayer", value == null ? null : value.Trim());
            }
        }
        
        public string OutboundGateway
        {
            get
            {
                return GetFieldValue<string>("OutboundGateway");
            }
            set
            {
                SetFieldValue<string>("OutboundGateway", value == null ? null : value.Trim());
            }
        }
        
        public string AnsweringMachineDetector
        {
            get
            {
                return GetFieldValue<string>("AnsweringMachineDetector");
            }
            set
            {
                SetFieldValue<string>("AnsweringMachineDetector", value == null ? null : value.Trim());
            }
        }

        public string ConferenceBridge
        {
            get
            {
                return GetFieldValue<string>("ConferenceBridge");
            }
            set
            {
                SetFieldValue<string>("ConferenceBridge", value == null ? null : value.Trim());
            }
        }

        public string IvrPlayer
        {
            get
            {
                return GetFieldValue<string>("IvrPlayer");
            }
            set
            {
                SetFieldValue<string>("IvrPlayer", value == null ? null : value.Trim());
            }
        }

        public string Monitoring
        {
            get
            {
                return GetFieldValue<string>("Monitoring");
            }
            set
            {
                SetFieldValue<string>("Monitoring", value == null ? null : value.Trim());
            }
        }

        public string Recording
        {
            get
            {
                return GetFieldValue<string>("Recording");
            }
            set
            {
                SetFieldValue<string>("Recording", value == null ? null : value.Trim());
            }
        }

        public string RecordingPlayer
        {
            get
            {
                return GetFieldValue<string>("RecordingPlayer");
            }
            set
            {
                SetFieldValue<string>("RecordingPlayer", value == null ? null : value.Trim());
            }
        }

        public string SSHSettings
        {
            get
            {
                return GetFieldValue<string>("SSHSettings");
            }
            set
            {
                SetFieldValue<string>("SSHSettings", value == null ? null : value.Trim());
            }
        }


        public bool HoldMusicPlayerEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(HoldMusicPlayer);
            }
            set
            {
                if (value)
                {
                    HoldMusicPlayer = string.Concat("sip:hold-v2@", BaseUriIP);
                }
                else
                {
                    HoldMusicPlayer = null;
                }
            }
        }

        public bool RingerEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(Ringer);
            }
            set
            {
                if (value)
                {
                    Ringer = string.Concat("sip:ringing@", BaseUriIP);
                }
                else
                {
                    Ringer = null;
                }
            }
        }

        public bool AnnouncerEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(Announcer);
            }
            set
            {
                if (value)
                {
                    Announcer = string.Concat("sip:beep@", BaseUriIP);
                }
                else
                {
                    Announcer = null;
                }
            }
        }

        public bool QueueLoopPlayerEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(QueueLoopPlayer);
            }
            set
            {
                if (value)
                {
                    QueueLoopPlayer = string.Concat("sip:{0}@", BaseUriIP);
                }
                else
                {
                    QueueLoopPlayer = null;
                }
            }
        }

        public bool OutboundGatewayEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(OutboundGateway);
            }
            set
            {
                if (value)
                {
                    OutboundGateway = string.Concat("sip:{0}@", BaseUriIP);
                }
                else
                {
                    OutboundGateway = null;
                }
            }
        }

        public bool AnsweringMachineDetectorEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(AnsweringMachineDetector);
            }
            set
            {
                if (value)
                {
                    AnsweringMachineDetector = string.Concat("sip:{0}@", BaseUriIP);
                }
                else
                {
                    AnsweringMachineDetector = null;
                }
            }
        }

        public bool ConferenceBridgeEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(ConferenceBridge);
            }
            set
            {
                if (value)
                {
                    ConferenceBridge = string.Concat("sip:conf@", BaseUriIP);
                }
                else
                {
                    ConferenceBridge = null;
                }
            }
        }

        public bool IvrPlayerEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(IvrPlayer);
            }
            set
            {
                if (value)
                {
                    IvrPlayer = string.Concat("sip:{0}@", BaseUriIP);
                }
                else
                {
                    IvrPlayer = null;
                }
            }
        }

        public bool MonitoringEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(Monitoring);
            }
            set
            {
                if (value)
                {
                    Monitoring = string.Concat("sip:monitoring@", BaseUriIP);
                }
                else
                {
                    Monitoring = null;
                }
            }
        }

        public bool RecordingEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(Recording);
            }
            set
            {
                if (value)
                {
                    Recording = string.Concat("AsteriskManager;", BaseUriIP, ":5038;nixxis;nixxis00");
                }
                else
                {
                    Recording = null;
                }
            }
        }

        public bool RecordingPlayerEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(RecordingPlayer);
            }
            set
            {
                if (value)
                {
                    RecordingPlayer = string.Concat("sip:playback_recording@", BaseUriIP);
                }
                else
                {
                    RecordingPlayer = null;
                }
            }
        }
        public bool SSHSettingsEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(SSHSettings);
            }
            set
            {
                if (value)
                {
                    SSHSettings = BaseUriIP;
                }
                else
                {
                    SSHSettings = null;
                }
            }
        }


        [AdminLoad(SkipLoad = true)]
        public AdminObjectList<PhoneResource> Phones
        {
            get;
            internal set;
        }

        public object CheckedPhones
        {
            get
            {
                return new AdminCheckedLinkList<Phone, PhoneResource>(m_Core.Phones, Phones, this);
            }
        }


        [AdminLoad(SkipLoad=true)]
        public SingletonAdminObjectList<ResourceLocation> Locations
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

        void m_Locations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SyncLocationFromLocations();
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

        void m_Location_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SyncLocationsFromLocation();
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


        public string FtpUrl
        {
            get
            {
                return GetFieldValue<string>("FtpUrl");
            }
            set
            {
                SetFieldValue<string>("FtpUrl", value);
            }
        }

        public string FtpUser
        {
            get
            {
                return GetFieldValue<string>("FtpUser");
            }
            set
            {
                SetFieldValue<string>("FtpUser", value);
            }
        }

        public string FtpPassword
        {
            get
            {
                return GetFieldValue<string>("FtpPassword");
            }
            set
            {
                SetFieldValue<string>("FtpPassword", value);
            }
        }

        // TODO
        public string AddonsCompatibility
        {
            get
            {
                return GetFieldValue<string>("AddonsCompatibility");
            }
            set
            {
                SetFieldValue<string>("AddonsCompatibility", value);
            }
        }

        public bool Enabled
        {
            get
            {
                return GetFieldValue<bool>("Enabled");
            }
            set
            {
                SetFieldValue<bool>("Enabled", value);
            }
        }

        public float Cost
        {
            get
            {
                return GetFieldValue<float>("Cost");
            }
            set
            {
                SetFieldValue<float>("Cost", value);
            }
        }

        public int MaxLines
        {
            get
            {
                return GetFieldValue<int>("MaxLines");
            }
            set
            {
                SetFieldValue<int>("MaxLines", value);
            }
        }

        public int MaxOutLines
        {
            get
            {
                return GetFieldValue<int>("MaxOutLines");
            }
            set
            {
                SetFieldValue<int>("MaxOutLines", value);
            }
        }

        public int MaxInLines
        {
            get
            {
                return GetFieldValue<int>("MaxInLines");
            }
            set
            {
                SetFieldValue<int>("MaxInLines", value);
            }
        }

        public bool MaxLinesLimited
        {
            get
            {
                return (MaxLines != -1);
            }
            set
            {
                if (value)
                {
                    MaxLines = 0;
                }
                else
                {
                    MaxLines = -1;
                }
            }
        }

        public bool MaxOutLinesLimited
        {
            get
            {
                return (MaxOutLines !=-1);
            }
            set
            {
                if (value)
                {
                    MaxOutLines = 0;
                }
                else
                {
                    MaxOutLines = -1;
                }
            }
        }

        public bool MaxInLinesLimited
        {
            get
            {
                return (MaxInLines != -1);
            }
            set
            {
                if (value)
                {
                    MaxInLines = 0;
                }
                else
                {
                    MaxInLines = -1;
                }
            }
        }

        public Resource Duplicate()
        {
            Resource res = AdminCore.Create<Resource>();
            res.AddonsCompatibility = AddonsCompatibility;
            res.Announcer = Announcer;
            res.AnsweringMachineDetector = AnsweringMachineDetector;
            res.BaseUri = BaseUri;
            res.ConferenceBridge = ConferenceBridge;
            res.Cost = Cost;
            res.Description = string.Concat(Description, "*");
            res.Enabled = Enabled;
            res.FtpPassword = FtpPassword;
            res.FtpUrl = FtpUrl;
            res.FtpUser = FtpUser;
            res.GroupKey = GroupKey;
            res.HoldMusicPlayer = HoldMusicPlayer;
            res.IvrPlayer = IvrPlayer;
            res.Location.TargetId = Location.TargetId;
            res.MaxInLines = MaxInLines;
            res.MaxLines = MaxLines;
            res.MaxOutLines = MaxOutLines;
            res.Monitoring = Monitoring;
            res.OutboundGateway = OutboundGateway;
            res.QueueLoopPlayer = QueueLoopPlayer;
            res.Recording = Recording;
            res.RecordingPlayer = RecordingPlayer;
            res.SSHSettings = SSHSettings;
            res.Ringer = Ringer;
            res.UserAgent = UserAgent;

            res.AdminCore.Resources.Add(res);

            foreach (ResourceCarrier rc in Carriers)
            {
                res.Carriers.Add(rc.Carrier);
            }

            return res;
        }

        [AdminLoad(Path = "/Admin/ResourcesCarriers/ResourceCarrier[@resourceid=\"{0}\"]")]
        public AdminObjectList<ResourceCarrier> Carriers
        {
            get;
            internal set;
        }

        public object CheckedCarriers
        {
            get
            {
                return new AdminCheckedLinkList<Carrier, ResourceCarrier>( m_Core.Carriers , Carriers, this);
            }
        }

    }
}