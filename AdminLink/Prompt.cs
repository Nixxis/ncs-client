using System.IO;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using System.Linq;

namespace Nixxis.Client.Admin
{
    public class Prompt : AdminObject
    {
        internal override bool Reload(System.Xml.XmlNode node)
        {
            base.Reload(node);
            m_LocalPath = null;
            foreach (XmlNode nde in node.SelectNodes("RelatedPrompts/Prompt"))
            {
                Prompt p = null;
                p = Core.GetAdminObject(nde.Attributes["id"].Value) as Prompt;
                if(p==null)
                {
                    p = new Prompt(Core);
                    p.Load(nde as XmlElement);
                    Core.SetAdminObject(p);
                }


                p.Reload(nde);

                if (!m_RelatedPrompts.Contains(p))
                    m_RelatedPrompts.Add(p);

            }
            return true;
        }

        private AdminObjectReference<Language> m_Language;
        private AdminObjectList<Prompt> m_RelatedPrompts;
        private string m_LocalPath;

        public string LocalPath
        {
            get
            {
                return m_LocalPath;
            }
            set
            {
                m_LocalPath = value;
                FirePropertyChanged("PathUri");
            }
        }

        public string Path
        {
            get
            {
                return GetFieldValue<string>("Path");
            }
            set
            {
                SetFieldValue<string>("Path", value);
                FirePropertyChanged("PathUri");
            }
        }

        public string Folder
        {
            get
            {
                if (Path == null)
                    return null;
                string[] parts = Path.Split('/');
                if (parts.Length >= 2)
                    return parts[parts.Length - 2];
                return null;
            }
        }

        public string RelativePath
        {
            get
            {
                return string.Concat(Folder, @"/", Id);
            }
        }

        public Uri PathUri
        {
            get
            {
                if (!string.IsNullOrEmpty(LocalPath))
                    return new Uri(LocalPath);

                if (!string.IsNullOrEmpty(Path))
                    return new Uri(string.Format(Path, m_Core.AdminUploadServer));

                return null;
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
                    return string.Concat("Prompt ", Description);
            }
        }

        [AdminLoad(Path = "/Admin/PromptsLinks/PromptLink[@promptid=\"{0}\"]")]
        public AdminObjectList<PromptLink> Links
        {
            get;
            internal set;
        }

        public Prompt(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public Prompt(AdminCore core)
            : base(core)
        {
            Init();
        }

        private void Init()
        {
            Language = new AdminObjectReference<Admin.Language>(this);
            RelatedPrompts = new AdminObjectList<Prompt>(this);
        }


        public override void Clear()
        {
            base.Clear();
        }

        public void ComputePath(string folder, string extension)
        {
            if (ConfigurationManager.AppSettings["PathUpload"] == null)
            {
                Path = string.Concat(Core.AdminUploadUriPatern, AdminCore.PathSounds, @"/", folder, @"/", Id, System.IO.Path.GetExtension(extension));
            }
            else if (ConfigurationManager.AppSettings["PathUpload"].StartsWith("http://"))
            {
                Path = string.Concat(ConfigurationManager.AppSettings["PathUpload"], @"/", AdminCore.PathSounds, @"/", folder, @"/", Id, System.IO.Path.GetExtension(extension));
            }
            else
            {
                Path = string.Concat(ConfigurationManager.AppSettings["PathUpload"], @"\", AdminCore.PathSounds, @"\", folder, @"\", Id, System.IO.Path.GetExtension(extension));
            }
        }

        public AdminObjectReference<Language> Language
        {
            get
            {
                return m_Language;
            }
            internal set
            {
                if (m_Language != null)
                {
                    m_Language.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Language_PropertyChanged);
                    m_Language.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Language_PropertyChanged);
                }
                m_Language = value;

                if (m_Language != null)
                {
                    m_Language.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Language_PropertyChanged);
                    m_Language.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Language_PropertyChanged);
                }

            }
        }

        void m_Language_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AdminObject[] references = GetReferences();
            foreach (AdminObject obj in references)
            {
                if (obj is BasePreprocessorConfig)
                {
                    ((BasePreprocessorConfig)obj).Save();
                }
            }
            if (RelatedTo != null && RelatedTo.HasTarget)
                RelatedTo.Target.m_Language_PropertyChanged(sender, e);
        }

        [AdminLoad(Path = "/Admin/Prompts/Prompt[@id=\"{0}\"]/RelatedPrompts/Prompt")]
        public AdminObjectList<Prompt> RelatedPrompts
        {
            get
            {
                return m_RelatedPrompts;
            }
            internal set
            {
                if (m_RelatedPrompts != null)
                    m_RelatedPrompts.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_RelatedPrompts_CollectionChanged);

                m_RelatedPrompts = value;

                if (m_RelatedPrompts != null)
                    m_RelatedPrompts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_RelatedPrompts_CollectionChanged);
            }
        }

        void m_RelatedPrompts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FirePropertyChanged("HasRelatedPrompts");

            AdminObject[] references = GetReferences();
            foreach (AdminObject obj in references)
            {
                if (obj is BasePreprocessorConfig)
                {
                    ((BasePreprocessorConfig)obj).Save();
                }
            }
        }

        public bool HasRelatedPrompts
        {
            get
            {
                return RelatedPrompts.Count > 0;
            }
        }

        public AdminObjectReference<Prompt> RelatedTo
        {
            get;
            internal set;
        }

        public override void Save(System.Xml.XmlDocument doc)
        {
            if (LocalPath != null)
            {
                AdminCore.UploadFile(LocalPath, Path);
            }
            base.Save(doc);
        }


        // TODO, missing an event when refs r changing, to allow binding to refresh...
        public string References
        {
            get
            {
                AdminObject[] refs = GetReferences();
                List<string> txts = new List<string>();
                foreach (AdminObject ao in refs)
                {
                    if (ao != null)
                        txts.Add(ao.DisplayText);
                }

                return string.Join(",", txts);
            }
        }
    }


    [AdminObjectLinkCascadeAttribute(typeof(Prompt), "Links")]
    [AdminObjectLinkCascadeAttribute(typeof(AdminObject), "Prompts")]
    public class PromptLink : AdminObjectLink<AdminObject, Prompt>
    {
        internal override bool Reload(System.Xml.XmlNode node)
        {
            base.Reload(node);
            if (Link is Campaign)
            {
                Campaign camp = (Campaign)Link;
                if (!camp.Prompts.Contains(this))
                {
                    camp.Prompts.Add(this);
                }
            }
            return true;
        }

        public PromptLink(AdminObject parent)
            : base(parent)
        {
        }

        public string PromptId
        {
            get
            {
                return Id2;
            }
        }

        public string LinkId
        {
            get
            {
                return Id1;
            }
        }

        public Prompt Prompt
        {
            get
            {
                if (m_Core == null)
                    return null;
                return (Prompt)(m_Core.GetAdminObject(Id2));
            }
        }

        public AdminObject Link
        {
            get { return m_Core.GetAdminObject(Id1); }
        }

        protected override XmlElement CreateSaveNode(XmlDocument doc, string operation)
        {
            XmlElement returnValue = base.CreateSaveNode(doc, operation);

            if (returnValue.Attributes["adminobjectid"] != null && returnValue.Attributes["adminobjectid"].Value == PromptRepository.RepositoryId)
                returnValue.Attributes.Remove(returnValue.Attributes["adminobjectid"]);

            return returnValue;
        }

        internal override void Load(XmlElement node)
        {
            if (node.Attributes["adminobjectid"] == null)
            {
                this.SetIds(PromptRepository.RepositoryId, node.Attributes["promptid"].Value);
            }
            base.Load(node);
        }

        public static void PreProcessNode(XmlElement node)
        {
            if (node.Attributes["adminobjectid"] == null)
            {
                XmlAttribute att = node.OwnerDocument.CreateAttribute("adminobjectid");
                att.Value = PromptRepository.RepositoryId;
                node.Attributes.Append(att);
            }
        }

    }

    [AdminSave(SkipSave = true)]
    public class PromptRepository : AdminObject
    {
        public static string RepositoryId = "y1000000000000000000000000000000";

        public PromptRepository(AdminCore core)
            : base(core)
        {
            Id = RepositoryId;
            Prompts = new AdminObjectList<PromptLink>(this);
        }

        public PromptRepository(AdminObject parent)
            : base(parent)
        {
            Id = RepositoryId;
            Prompts = new AdminObjectList<PromptLink>(this);
        }

        private AdminObjectList<PromptLink> m_Prompts;

        [AdminLoad(Path = "/Admin/PromptsLinks/PromptLink[not(@adminobjectid)]")]
        public AdminObjectList<PromptLink> Prompts
        {
            get
            {
                return m_Prompts;
            }
            internal set
            {
                if (m_Prompts != null)
                {
                    m_Prompts.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_Prompts_Collection_Changed);
                }

                m_Prompts = value;

                if (m_Prompts != null)
                {
                    m_Prompts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_Prompts_Collection_Changed);
                }
                m_Prompts_Collection_Changed(this, null);
            }
        }

        private void m_Prompts_Collection_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Core.Campaigns != null)
            {
                foreach (Campaign camp in Core.Campaigns)
                {
                    foreach (Activity act in camp.Activities)
                    {
                        act.FirePropertyChanged("AllPrompts");
                        act.FirePropertyChanged("MusicPrompts");
                    }
                }
            }
            if (Core.Queues != null)
            {
                foreach (Queue q in Core.Queues)
                    q.FirePropertyChanged("AllPrompts");
            }
            FirePropertyChanged("SimplePrompts");
        }

        public IEnumerable<Prompt> SimplePrompts
        {
            get
            {
                return Prompts.Select((pl) => (pl.Prompt));
            }
        }

        protected override XmlElement CreateSaveNode(XmlDocument doc, string operation)
        {
            return base.CreateSaveNode(doc, operation);
        }

    }

}
