using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixxis.Client.Admin
{
    public class PredefinedText: AdminObject
    {
        public PredefinedText(AdminCore core)
            : base(core)
        {
            Init();
        }

        public PredefinedText(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private void Init()
        {
            CompatibleMedias = MediaType.Chat | MediaType.Mail;
            Sequence = 1;
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

        public AdminObjectReference<Campaign> Campaign
        {
            get;
            internal set;
        }

        public AdminObjectReference<Language> Language
        {
            get;
            internal set;
        }

        public string TextContent
        {
            get
            {
                return GetFieldValue<string>("TextContent");
            }
            set
            {
                SetFieldValue<string>("TextContent", value);
            }
        }

        public string HtmlContent
        {
            get
            {
                return GetFieldValue<string>("HtmlContent");
            }
            set
            {
                SetFieldValue<string>("HtmlContent", value);
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public MediaType CompatibleMedias
        {
            get
            {
                return GetFieldValue<MediaType>("CompatibleMedias");
            }
            set
            {
                SetFieldValue<MediaType>("CompatibleMedias", value);
            }
        }

        public override string TypedDisplayText
        {
            get
            {
                return string.Concat("Predefined text ", Description);

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

        public bool IsFirst
        {
            get
            {
                return  ((AdminObjectList<PredefinedText>)Parent).Min((a) => (a.Sequence)) == Sequence;
            }
        }

        public bool IsLast
        {
            get
            {
                return ((AdminObjectList<PredefinedText>)Parent).Max((a) => (a.Sequence)) == Sequence;
            }
        }

        public PredefinedText Previous
        {
            get
            {
                if (Parent == null)
                    return null;
                int prevSequence = ((AdminObjectList<PredefinedText>)Parent).Max((a) => (a.Sequence < Sequence ? a.Sequence : -1));
                return ((AdminObjectList<PredefinedText>)Parent).First((a) => (a.Sequence == prevSequence));
            }
        }

        public PredefinedText Next
        {
            get
            {
                if (Parent == null)
                    return null;
                int nextSequence = ((AdminObjectList<PredefinedText>)Parent).Min((a) => (a.Sequence > Sequence ? a.Sequence : int.MaxValue));
                return ((AdminObjectList<PredefinedText>)Parent).First((a) => (a.Sequence == nextSequence));

            }
        }

        public bool IsMail
        {
            get
            {
                return (CompatibleMedias & MediaType.Mail) == MediaType.Mail;
            }
            set
            {
                if (value)
                {
                    CompatibleMedias = CompatibleMedias | MediaType.Mail;
                }
                else
                {
                    CompatibleMedias = (CompatibleMedias ^ MediaType.Mail) & CompatibleMedias;
                }
            }
        }

        public bool IsChat
        {
            get
            {
                return (CompatibleMedias & MediaType.Chat) == MediaType.Chat;
            }
            set
            {
                if (value)
                {
                    CompatibleMedias = CompatibleMedias | MediaType.Chat;
                }
                else
                {
                    CompatibleMedias = (CompatibleMedias ^ MediaType.Chat) & CompatibleMedias;
                }

            }
        }

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            try
            {
                if (Campaign.HasTarget)
                {
                    Campaign.Target.EmptySave(doc);
                }
            }
            catch
            {
            }
            return base.CreateSaveNode(doc, operation);
        }

    }
}
