using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Nixxis.Client.Admin
{
 
    public class Language: AdminObject
    {
        public class SimpleLanguage
        {
            public string Isocode { get;set;}
            public string Description { get; set; }
            public string GroupKey { get; set; }

        }

        public static List<SimpleLanguage> Languages
        {
            get
            {
                List<SimpleLanguage> result = new List<SimpleLanguage>();

                if (System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.Equals("en"))
                {
                    foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
                    {
                        if (ci.IetfLanguageTag.Length > 0)
                        {
                            SimpleLanguage lng = new SimpleLanguage();

                            lng.Isocode = ci.IetfLanguageTag;
                            lng.Description = ci.EnglishName;
                            if (!ci.IsNeutralCulture)
                                lng.GroupKey = ci.Parent.EnglishName;
                            else
                                lng.GroupKey = ci.EnglishName;

                            result.Add(lng);
                        }
                    }
                }
                else
                {
                    foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
                    {
                        if (ci.IetfLanguageTag.Length > 0)
                        {
                            SimpleLanguage lng = new SimpleLanguage();

                            lng.Isocode = ci.IetfLanguageTag;

                            lng.Description = ci.NativeName.Replace('(', ' ').Replace(')', ' ') + " - " + ci.EnglishName.Replace('(', ' ').Replace(')', ' ');

                            if (!ci.IsNeutralCulture)
                            {
                                lng.GroupKey = ci.Parent.NativeName;                               
                            }
                            else
                            {
                                lng.GroupKey = ci.NativeName;
                            }

                            result.Add(lng);
                        }
                    }
                }
                result.Sort( (sl1, sl2) => (sl1.Description.CompareTo(sl2.Description) ) );
                return result;
            }
        }

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            if(operation=="create" && m_Core.Hidden.Languages.ContainsId(this.Id))
            {
                System.Xml.XmlElement elm = base.CreateSaveNode(doc, "update");
                System.Xml.XmlAttribute att = elm.OwnerDocument.CreateAttribute("state");
                att.Value = System.Xml.XmlConvert.ToString(10);
                elm.Attributes.Append(att);
                return elm;
            }
            else
            {
                return base.CreateSaveNode(doc, operation);
            }
        }

        public Language(string iscode, AdminCore core)
            : base(core)
        {
            Id = iscode;
        }

        public Language(AdminCore core)
            : base(core)
        {
        }

        public Language(AdminObject parent): base(parent)
        {
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
                SetFieldValue<string>("GroupKey", value);
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
                    return string.Concat("Language ", Description);
            }
        }


        [AdminLoad(Path = "/Admin/AgentsLanguages/AgentLanguage[@languageid=\"{0}\"]")]
        public AdminObjectList<AgentLanguage> Agents
        {
            get;
            internal set;
        }

        public object CheckedAgents
        {
            get
            {
                return new AdminCheckedLinkList<Agent, AgentLanguage>(m_Core.Agents, Agents, this);
            }
        }

        [AdminLoad(Path = "/Admin/ActivitiesLanguages/ActivityLanguage[@languageid=\"{0}\"]")]
        public AdminObjectList<ActivityLanguage> Activities
        {
            get;
            internal set;
        }

        public object CheckedActivities
        {
            get
            {
                return new AdminCheckedLinkList<Activity, ActivityLanguage>(m_Core.InboundActivities, Activities, this);
            }
        }

    }
}
