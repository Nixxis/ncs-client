using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace Nixxis.Client.Admin
{
    public class Attachment: AdminObject
    {
        public Attachment(AdminCore core) : base(core)
        {
            Init();
        }

        public Attachment(AdminObject parent) : base(parent)
        {
            Init();
        }

        private void Init()
        {
            CompatibleMedias = MediaType.Chat | MediaType.Mail;
            Sequence = 1;
            LocationIsLocal = false;
            InlineDisposition = false;
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

        public string ComputedLocation
        {
            get
            {
                if (LocationIsLocal)
                {
                    if(LocalPath==null)
                        return Path.GetFileName(Location);
                    else
                        return Path.GetFileName(LocalPath);
                }
                else
                {
                    return Location;
                }
            }
            set
            {
                Location = value;
            }
        }

        private string m_LocalPath = null;
        public string LocalPath
        {
            get
            {
                return m_LocalPath;
            }
            set
            {
                m_LocalPath = value;
            }
        }

        public string Location
        {
            get
            {
                return GetFieldValue<string>("Location");
            }
            set
            {
                SetFieldValue<string>("Location", value);
                FirePropertyChanged("ComputedLocation");
            }
        }

        public string UploadLocation
        {
            get
            {
                return GetFieldValue<string>("UploadLocation");
            }
            set
            {
                SetFieldValue<string>("UploadLocation", value);
                FirePropertyChanged("UploadLocation");
            }
        }

        public string Target
        {
            get
            {
                return GetFieldValue<string>("Target");
            }
            set
            {
                SetFieldValue<string>("Target", value);
            }
        }

        public bool LocationIsLocal
        {
            get
            {
                return GetFieldValue<bool>("LocationIsLocal");
            }
            set
            {
                SetFieldValue<bool>("LocationIsLocal", value);
                FirePropertyChanged("ComputedLocation");
            }
        }

        public bool InlineDisposition
        {
            get
            {
                return GetFieldValue<bool>("InlineDisposition");
            }
            set
            {
                SetFieldValue<bool>("InlineDisposition", value);
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
                return ((AdminObjectList<Attachment>)Parent).Min((a) => (a.Sequence)) == Sequence;
            }
        }

        public bool IsLast
        {
            get
            {
                return ((AdminObjectList<Attachment>)Parent).Max((a) => (a.Sequence)) == Sequence;
            }
        }

        public Attachment Previous
        {
            get
            {
                if (Parent == null)
                    return null;
                int prevSequence = ((AdminObjectList<Attachment>)Parent).Max((a) => (a.Sequence < Sequence ? a.Sequence : -1));
                return ((AdminObjectList<Attachment>)Parent).First((a) => (a.Sequence == prevSequence));
            }
        }

        public Attachment Next
        {
            get
            {
                if (Parent == null)
                    return null;
                int nextSequence = ((AdminObjectList<Attachment>)Parent).Min((a) => (a.Sequence > Sequence ? a.Sequence : int.MaxValue));
                return ((AdminObjectList<Attachment>)Parent).First((a) => (a.Sequence == nextSequence));

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

        public void ComputePath()
        {

            if (ConfigurationManager.AppSettings["PathUpload"] == null)
            {
                string temp = Core.AdminUploadUri.Split(new string[] { Core.AdminUploadServer }, StringSplitOptions.None)[1].Substring(1);
                Location = string.Concat(/*Core.AdminUploadUri,*/ temp , AdminCore.PathAttachments, @"/", Id, @"/", Path.GetFileName(LocalPath)).Replace(@"/", @"\");
                UploadLocation = string.Concat(Core.AdminUploadUri, AdminCore.PathAttachments, @"/", Id, @"/", Path.GetFileName(LocalPath));
            }
            else if (ConfigurationManager.AppSettings["PathUpload"].StartsWith("http://"))
            {
                string temp = Core.AdminUploadUri.Split(new string[] { ConfigurationManager.AppSettings["PathUpload"] }, StringSplitOptions.None)[1].Substring(1);
                Location = string.Concat(/*ConfigurationManager.AppSettings["PathUpload"],*/temp, @"/", AdminCore.PathAttachments, @"/", Id, @"/", Path.GetFileName(LocalPath)).Replace(@"/", @"\");
                UploadLocation = string.Concat(ConfigurationManager.AppSettings["PathUpload"], @"/", AdminCore.PathAttachments, @"/", Id, @"/", Path.GetFileName(LocalPath));
            }
            else
            {
                Location = string.Concat(ConfigurationManager.AppSettings["PathUpload"], @"\", AdminCore.PathAttachments, @"\", Id, @"/", Path.GetFileName(LocalPath));
                UploadLocation = string.Concat(ConfigurationManager.AppSettings["PathUpload"], @"\", AdminCore.PathAttachments, @"\", Id, @"/", Path.GetFileName(LocalPath));
            }
        }


        public override void Save(System.Xml.XmlDocument doc)
        {
            if (LocalPath != null)
            {
                ComputePath();
                AdminCore.UploadFile(LocalPath, UploadLocation);
            }
            base.Save(doc);
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

        public override string TypedDisplayText
        {
            get
            {
                return string.Concat("Attachment ", Description);

            }
        }

    }
}
