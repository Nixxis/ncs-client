namespace Nixxis.Client.Admin
{
    public class AmdSettings : AdminObject
    {


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
                    return string.Concat("Amd settings ", Description);
            }
        }


        public AmdSettings(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public AmdSettings(AdminCore core)
            : base(core)
        {
            Init();
        }

        private void Init()
        {

            ServerUrl = "amd";
            InitialSilence = 2500;
            Greeting = 1500;
            AfterGreetingSilence = 800;
            TotalAnalysisTime = 5000;
            MinimumWordLength = 100;
            BetweenWordsSilence = 50;
            MaximumNumberOfWords = 3;
            SilenceTreshold = 256;
            DropMachine = true;
        }

        public string ServerUrl
        {
            get
            {
                return GetFieldValue<string>("ServerUrl");
            }
            set
            {
                SetFieldValue<string>("ServerUrl", value);
            }
        }

        public int InitialSilence
        {
            get
            {
                return GetFieldValue<int>("InitialSilence");
            }
            set
            {
                SetFieldValue<int>("InitialSilence", value);
            }
        }

        public int Greeting
        {
            get
            {
                return GetFieldValue<int>("Greeting");
            }
            set
            {
                SetFieldValue<int>("Greeting", value);
            }
        }

        public int AfterGreetingSilence
        {
            get
            {
                return GetFieldValue<int>("AfterGreetingSilence");
            }
            set
            {
                SetFieldValue<int>("AfterGreetingSilence", value);
            }
        }

        public int TotalAnalysisTime
        {
            get
            {
                return GetFieldValue<int>("TotalAnalysisTime");
            }
            set
            {
                SetFieldValue<int>("TotalAnalysisTime", value);
            }
        }

        public int MinimumWordLength
        {
            get
            {
                return GetFieldValue<int>("MinimumWordLength");
            }
            set
            {
                SetFieldValue<int>("MinimumWordLength", value);
            }
        }

        public int BetweenWordsSilence
        {
            get
            {
                return GetFieldValue<int>("BetweenWordsSilence");
            }
            set
            {
                SetFieldValue<int>("BetweenWordsSilence", value);
            }
        }

        public int MaximumNumberOfWords
        {
            get
            {
                return GetFieldValue<int>("MaximumNumberOfWords");
            }
            set
            {
                SetFieldValue<int>("MaximumNumberOfWords", value);
            }
        }

        public int SilenceTreshold
        {
            get
            {
                return GetFieldValue<int>("SilenceTreshold");
            }
            set
            {
                SetFieldValue<int>("SilenceTreshold", value);
            }
        }

        public bool DropMachine
        {
            get
            {
                return GetFieldValue<bool>("DropMachine");
            }
            set
            {
                SetFieldValue<bool>("DropMachine", value);
            }
        }

        public bool DropUnsure
        {
            get
            {
                return GetFieldValue<bool>("DropUnsure");
            }
            set
            {
                SetFieldValue<bool>("DropUnsure", value);
            }
        }

        public bool DropHuman
        {
            get
            {
                return GetFieldValue<bool>("DropHuman");
            }
            set
            {
                SetFieldValue<bool>("DropHuman", value);
            }
        }

        public bool PromptMachine
        {
            get
            {
                return GetFieldValue<bool>("PromptMachine");
            }
            set
            {
                SetFieldValue<bool>("PromptMachine", value);
            }
        }

        public bool PromptHuman
        {
            get
            {
                return GetFieldValue<bool>("PromptHuman");
            }
            set
            {
                SetFieldValue<bool>("PromptHuman", value);
            }
        }

        public bool PromptUnsure
        {
            get
            {
                return GetFieldValue<bool>("PromptUnsure");
            }
            set
            {
                SetFieldValue<bool>("PromptUnsure", value);
            }
        }


        public AmdSettings Duplicate()
        {
            AmdSettings newamd = AdminCore.Create<AmdSettings>();

            newamd.AfterGreetingSilence = AfterGreetingSilence;
            newamd.BetweenWordsSilence = BetweenWordsSilence;
            newamd.Description = string.Concat(Description, "*");
            newamd.DropHuman = DropHuman;
            newamd.DropMachine = DropMachine;
            newamd.DropUnsure = DropUnsure;
            newamd.GroupKey = GroupKey;
            newamd.InitialSilence = InitialSilence;
            newamd.MaximumNumberOfWords = MaximumNumberOfWords;
            newamd.MinimumWordLength = MinimumWordLength;
            newamd.PromptHuman = PromptHuman;
            newamd.PromptMachine = PromptMachine;
            newamd.PromptUnsure = PromptUnsure;
            newamd.ServerUrl = ServerUrl;
            newamd.SilenceTreshold = SilenceTreshold;
            newamd.TotalAnalysisTime = TotalAnalysisTime;

            newamd.AdminCore.AmdSettings.Add(newamd);

            return newamd;
        }

        public override void Save(System.Xml.XmlDocument doc)
        {
            base.Save(doc);
        }

    }
}