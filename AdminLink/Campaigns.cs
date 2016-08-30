using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml;
using System.Net;
using System.IO;
using Nixxis;
using System.Windows.Media;
using ContactRoute;


namespace Nixxis.Client.Admin
{
    public enum CampaignStatus
    {
        NotActive,
        Paused,
        Running,
        Mixed
    }


    public class Campaign : SecuredAdminObject
    {
        private static Cache cache = new Cache();

        private static TranslationContext m_TranslationContext = new TranslationContext("Campaigns");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }

        private bool m_Initiating = false;

        private AdminObjectReference<NumberFormat> m_NumberFormat = null;
        private AdminObjectList<Attachment> m_Attachments;
        private AdminObjectList<PredefinedText> m_PredefinedTexts;
        private AdminObjectList<InboundActivity> m_InboundActivities;
        private AdminObjectList<OutboundActivity> m_OutboundActivities;
        private AdminObjectList<UserField> m_UserFields;
        private AdminObjectReference<SecurityContext> m_SecurityContext = null;
        private SingletonAdminObjectList<AdminObjectSecurityContext> m_SecurityContexts = null;

        private bool m_Loaded = false;

        public Campaign(AdminCore core)
            : base(core)
        {
            Init();
        }

        public Campaign(AdminObject parent)
            : base(parent)
        {            
            Init();
        }

        internal override bool Reload(XmlNode node)
        {

            string selectedUserField = null;
            if (QuotasTimeRelatedFieldObject != null && QuotasTimeRelatedFieldObject is UserField)
            {
                selectedUserField = QuotasTimeRelatedFieldObject.Name;
            }

            if (base.Reload(node))
            {
                m_Loaded = true;




                while (UserFields.Count > 0)
                {
                    UserField uf = UserFields[0];
                    Core.Delete(uf);
                    UserFields.Remove(uf);
                    uf.Delete();
                }

                XmlNodeList filterparts = node.SelectNodes("FieldsConfig/FieldsConfig/FieldConfig");
                foreach (XmlNode xmlsf in filterparts)
                {
                    UserField tempSf = new UserField(UserFields);
                    tempSf.Load(xmlsf as XmlElement);
                    tempSf.Id = System.Guid.NewGuid().ToString("N");
                    Core.SetAdminObject(tempSf);                    
                    UserFields.Add(tempSf);
                    tempSf.DoneLoading();
                }

                FirePropertyChanged("DateTimeFields");

                SetFieldLoaded("FieldsConfig");


                foreach (Activity ac in Activities)
                    ac.SetDescriptionLoaded();

                if (Qualification.HasTarget)
                    Qualifications.Reload(Qualification.TargetId);


                XmlNode nde = node.SelectSingleNode("QuotasTimeRelatedField");
                if (nde != null)
                {
                    bool bFound = false;
                    if (nde.Attributes["xsi:nil"] == null || nde.Attributes["xsi:nil"].Value != "true")
                    {
                        foreach (SystemField sf in SystemFields)
                        {
                            if (sf.Id == nde.InnerText)
                            {
                                QuotasTimeRelatedField = nde.InnerText;
                                SetFieldLoaded("QuotasTimeRelatedField");
                                bFound = true;
                                break;
                            }
                        }

                        if (!bFound)
                        {
                            foreach (UserField uf in UserFields)
                            {
                                if (uf.Name == nde.InnerText)
                                {
                                    QuotasTimeRelatedField = uf.Id;
                                    SetFieldLoaded("QuotasTimeRelatedField");
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        QuotasTimeRelatedField = null;
                        SetFieldLoaded("QuotasTimeRelatedField");
                    }

                }
                else
                {
                    if (selectedUserField != null)
                    {
                        foreach (UserField uf in UserFields)
                        {
                            if (uf.Name == selectedUserField)
                            {
                                QuotasTimeRelatedField = uf.Id;
                                SetFieldLoaded("QuotasTimeRelatedField");
                                break;
                            }
                        }
                    }
                }


                FirePropertyChanged("DataSourceHasChanged");


                return true;
            }
            return false;
        }

        private void Init()
        {
            m_Initiating = true;
            FieldsConfig = null;
            Advanced = false;
            Attachments = new AdminObjectList<Attachment>(this);
            PredefinedTexts = new AdminObjectList<PredefinedText>(this);
            Prompts = new AdminObjectList<PromptLink>(this);
            UserFields = new AdminObjectList<UserField>(this);
            m_InboundActivities = new AdminObjectList<InboundActivity>(this);
            m_OutboundActivities = new AdminObjectList<OutboundActivity>(this);
            m_Qualifications = new Admin.QualificationsCollecion(this);

            NumberFormat = new AdminObjectReference<NumberFormat>(this);

            // TODO: double affectation should not be needed...
            AutomaticRecording = false;
            AutomaticRecording = null;

            m_InboundActivities.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_InboundActivities_CollectionChanged);
            m_OutboundActivities.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_OutboundActivities_CollectionChanged);
            
            CampaignCurrentActivityFilterKinds = new AdminObjectList<CurrentActivityFilter>(this);

            Description = string.Format("New campaign {0}", DateTime.Now);

            SystemTable = DefaultSystemTableName;
            QuotasTable = DefaultQuotasTableName;
            UserTable = "Data";

            SecurityContext = new AdminObjectReference<SecurityContext>(this);
            SecurityContexts = new SingletonAdminObjectList<AdminObjectSecurityContext>(this);

            QuotasTimeRelatedFieldIsNow = false;

            m_Initiating = false;
        }

        public override void Clear()
        {
            if(Qualification.HasTarget)
                RemoveQualification(Qualification.Target);

            if (!Advanced)
            {
                Core.Delete(SystemQueue);
                Core.Delete(SystemTeam);
            }

            if (Prompts != null)
            {
                while (Prompts.Count > 0)
                {
                    //TODO:  this one should remove the prompt for all referencing lists, including core.Prompts
                    if (Prompts[0].Prompt != null)
                        Core.Delete(Prompts[0].Prompt);
                    else
                        Core.Delete(Prompts[0]);
                }
            }
            if (InboundActivities != null)
            {
                while (InboundActivities.Count > 0)
                {
                    Core.Delete(InboundActivities[0]);
                }
            }
            if (OutboundActivities != null)
            {

                while (OutboundActivities.Count > 0)
                {
                    Core.Delete(OutboundActivities[0]);
                }
            }
            if (Attachments != null)
            {
                while (Attachments.Count > 0)
                {
                    Core.Delete(Attachments[0]);
                }
            }
            if (PredefinedTexts != null)
            {
                while (PredefinedTexts.Count > 0)
                {
                    Core.Delete(PredefinedTexts[0]);
                }
            }

            UserFields.Clear(true);

            base.Clear();
        }

        internal override void Delete()
        {
            base.Delete();
            // don't know why but sometimes FielsConfig is not part of campaign fields...
            try
            {
                SetFieldLoaded("FieldsConfig");
            }
            catch
            {
            }
        }

        internal void m_OutboundActivities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
                    
            FirePropertyChanged("HasSystemOutboundActivity");
            FirePropertyChanged("HasSystemSearchActivity");
            // not sure if it is necessary but it should not harm...
            FirePropertyChanged("SystemQueue");
            FirePropertyChanged("SystemOutboundActivity");
            FirePropertyChanged("SystemSearchActivity");
            FirePropertyChanged("HasOutboundActivities");
            FirePropertyChanged("OutboundStatus");
            FirePropertyChanged("HasActivities");
            FirePropertyChanged("Activities");
            FirePropertyChanged("SystemCallbackActivity");    

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach(Activity act in e.NewItems)
                    {
                        CurrentActivityFilter afh = m_Core.Create<CurrentActivityFilter>();
                        afh.Activity.TargetId = act.Id;
                        CampaignCurrentActivityFilterKinds.Add(afh);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (Activity act in e.OldItems)
                    {

                        for (int i=0; i<CampaignCurrentActivityFilterKinds.Count; i++)
                        {
                            CurrentActivityFilter afh = CampaignCurrentActivityFilterKinds[i];
                            if (afh.Activity.TargetId == act.Id)
                            {                                 
                                CampaignCurrentActivityFilterKinds.RemoveAt(i);
                                m_Core.Delete(afh);
                                i--;
                            }
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:

 

                    for (int i = 0; i < CampaignCurrentActivityFilterKinds.Count; i++)
                    {
                        CurrentActivityFilter afh = CampaignCurrentActivityFilterKinds[i];

                        CampaignCurrentActivityFilterKinds.RemoveAt(i);
                        m_Core.Delete(afh);
                        i--;
                    }

                    foreach (Activity act in (IEnumerable)sender)
                    {
                        CurrentActivityFilter afh = m_Core.Create<CurrentActivityFilter>("zz" + act.Id);
                        
                        afh.Activity.TargetId = act.Id;


                        CampaignCurrentActivityFilterKinds.Add(afh);
                    }

                    break;
            }

            FirePropertyChanged("CurrentActivityFilterKinds");

            FirePropertyChanged("PromptsTabIsVisible");
        }

        void m_InboundActivities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FirePropertyChanged("HasSystemInboundActivity");
            FirePropertyChanged("HasSystemChatActivity");
            FirePropertyChanged("HasSystemEmailActivity");
            // this is really important even if it seems strange
            FirePropertyChanged("SystemQueue");
            FirePropertyChanged("SystemInboundActivity");
            FirePropertyChanged("SystemChatActivity");
            FirePropertyChanged("SystemEmailActivity");
            FirePropertyChanged("HasInboundActivities");
            FirePropertyChanged("InboundStatus");
            FirePropertyChanged("MailStatus");
            FirePropertyChanged("ChatStatus");
            FirePropertyChanged("HasActivities");
            FirePropertyChanged("Activities");
            FirePropertyChanged("PromptsTabIsVisible");
            FirePropertyChanged("PredefinedTextsTabIsVisible");
            FirePropertyChanged("AttachmentsTabIsVisible");
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
                FirePropertyChanged("ShortDisplayText");
                FirePropertyChanged("DisplayText");
                FirePropertyChanged("TypedDisplayText");
                // this part is only needed for adminaccess part

                if (SystemCallbackActivity != null)
                    SystemCallbackActivity.Description = Description;
                if (SystemChatActivity != null)
                    SystemChatActivity.Description = Description;
                if (SystemEmailActivity != null)
                    SystemEmailActivity.Description = Description;
                if (SystemInboundActivity != null)
                    SystemInboundActivity.Description = Description;
                if (SystemOutboundActivity != null)
                    SystemOutboundActivity.Description = Description;
                if (SystemQueue != null)
                    SystemQueue.Description = Description;
                if (SystemSearchActivity != null)
                    SystemSearchActivity.Description = Description;
                if (SystemTeam != null)
                    SystemTeam.Description = Description;
                            
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
                    return string.Concat("Campaign ", Description);
            }
        }

        public XmlDocument CustomConfig
        {
            get
            {
                return GetFieldValue<XmlDocument>("CustomConfig");
            }
            set
            {
                SetFieldValue<XmlDocument>("CustomConfig", value);

                FirePropertyChanged("SalesforceConnection");
                FirePropertyChanged("SalesforceLogin");
                FirePropertyChanged("SalesforcePassword");

                FirePropertyChanged("SalesForceFieldPriority");
                FirePropertyChanged("SalesForceFieldStatus");
                FirePropertyChanged("SalesForceFieldTargetDestination");
                FirePropertyChanged("SalesForceFieldTargetHandler");
                FirePropertyChanged("SalesForceFieldDialStartDate");
                FirePropertyChanged("SalesForceFieldLastContactId");
                FirePropertyChanged("SalesForceFieldLastDialStatus");
                FirePropertyChanged("SalesForceFieldLastDialerTry");
                FirePropertyChanged("SalesForceFieldLastAgentId");
                FirePropertyChanged("SalesForceFieldLastAgentDescription");
                FirePropertyChanged("SalesForceFieldLastActivityId");
                FirePropertyChanged("SalesForceFieldLastActivityDescription");
                FirePropertyChanged("SalesForceFieldLastQualificationId");
                FirePropertyChanged("SalesForceFieldLastQualificationDescription");

                try
                {
                    foreach (OutboundActivity oa in this.OutboundActivities)
                    {
                        oa.FirePropertyChanged("SalesforceCampaign");
                        oa.FirePropertyChanged("SalesforceCampaignDescription");
                        oa.FirePropertyChanged("SalesforceMode");
                    }
                }
                catch
                {
                }
            }
        }

        public string SalesforceConnection
        {
            get
            {
                if (CustomConfig == null)
                    return null;
                try
                {
                    XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/@connection");
                    if (nde != null)
                        return nde.Value;
                }
                catch
                {
                }
                return null;
            }
            set
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource");
                XmlAttribute att = null;
                if (node.Attributes["connection"] != null)
                    att = node.Attributes["connection"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("connection");
                    node.Attributes.Append(att);
                }

                att.Value = value;

                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesforceConnection");
            }
        }
        public string SalesforceLogin
        {
            get
            {
                if (CustomConfig == null)
                    return null;
                try
                {
                    XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Credentials/@Login");
                    if (nde != null)
                        return nde.Value;
                }
                catch
                {
                }
                return null;
            }
            set
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Credentials");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Credentials"));
                }
                XmlAttribute att = null;
                if (node.Attributes["Login"] != null)
                    att = node.Attributes["Login"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("Login");
                    node.Attributes.Append(att);
                }

                att.Value = value;

                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesforceLogin");
            }
        }
        public string SalesforcePassword
        {
            get
            {
                if (CustomConfig == null)
                    return null;

                try
                {
                    XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Credentials/@Password");
                    if (nde != null)
                        return nde.Value;
                }
                catch
                {
                }
                return null;
            }
            set
            {

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Credentials");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Credentials"));
                }
                XmlAttribute att = null;
                if (node.Attributes["Password"] != null)
                    att = node.Attributes["Password"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("Password");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesforcePassword");
            }
        }

        public string SalesForceFieldPriority
        {
            get
            {
                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@PriorityField");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                if (string.IsNullOrEmpty(strReturnVal))
                    strReturnVal = "nixxis__Dialer_priority__c";
                
                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["PriorityField"] != null)
                    att = node.Attributes["PriorityField"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("PriorityField");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldPriority");
            }
        }
        public string SalesForceFieldStatus
        {
            get
            {
                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@Status");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                if (string.IsNullOrEmpty(strReturnVal))
                    strReturnVal = "nixxis__Dialer_status__c";

                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["Status"] != null)
                    att = node.Attributes["Status"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("Status");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldStatus");
            }
        }
        public string SalesForceFieldTargetDestination
        {
            get
            {
                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@TargetDestination");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                if (string.IsNullOrEmpty(strReturnVal))
                    strReturnVal = "nixxis__Dialer_target_destination__c";

                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["TargetDestination"] != null)
                    att = node.Attributes["TargetDestination"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("TargetDestination");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldTargetDestination");
            }
        }
        public string SalesForceFieldTargetHandler
        {
            get
            {
                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@TargetHandler");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                if (string.IsNullOrEmpty(strReturnVal))
                    strReturnVal = "nixxis__Dialer_target_handler__c";

                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["TargetHandler"] != null)
                    att = node.Attributes["TargetHandler"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("TargetHandler");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldTargetHandler");
            }
        }
        public string SalesForceFieldDialStartDate
        {
            get
            {
                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@DialStartDate");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                if (string.IsNullOrEmpty(strReturnVal))
                    strReturnVal = "nixxis__Dial_start_date__c";

                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["DialStartDate"] != null)
                    att = node.Attributes["DialStartDate"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("DialStartDate");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldDialStartDate");
            }
        }
        public string SalesForceFieldLastContactId
        {
            get
            {
                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@LastContactId");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                if (string.IsNullOrEmpty(strReturnVal))
                    strReturnVal = "nixxis__Last_contact_id__c";

                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["LastContactId"] != null)
                    att = node.Attributes["LastContactId"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("LastContactId");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldLastContactId");
            }
        }
        public string SalesForceFieldLastDialStatus
        {
            get
            {
                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@LastDialStatus");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                if (string.IsNullOrEmpty(strReturnVal))
                    strReturnVal = "nixxis__Last_dial_status__c";

                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["LastDialStatus"] != null)
                    att = node.Attributes["LastDialStatus"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("LastDialStatus");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldLastDialStatus");
            }
        }
        public string SalesForceFieldLastDialerTry
        {
            get
            {
                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@LastDialerTry");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                if (string.IsNullOrEmpty(strReturnVal))
                    strReturnVal = "nixxis__Last_dialer_try__c";

                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["LastDialerTry"] != null)
                    att = node.Attributes["LastDialerTry"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("LastDialerTry");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldLastDialerTry");
            }
        }

        public string SalesForceFieldLastAgentId
        {
            get
            {
                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@LastAgentId");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["LastAgentId"] != null)
                    att = node.Attributes["LastAgentId"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("LastAgentId");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldLastAgentId");
            }
        }
        public string SalesForceFieldLastAgentDescription
        {
            get
            {

                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@LastAgentDescription");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                return strReturnVal;

            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["LastAgentDescription"] != null)
                    att = node.Attributes["LastAgentDescription"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("LastAgentDescription");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldLastAgentDescription");
            }
        }
        public string SalesForceFieldLastActivityId
        {
            get
            {
                
                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@LastActivityId");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["LastActivityId"] != null)
                    att = node.Attributes["LastActivityId"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("LastActivityId");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldLastActivityId");
            }
        }
        public string SalesForceFieldLastActivityDescription
        {
            get
            {

                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@LastActivityDescription");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["LastActivityDescription"] != null)
                    att = node.Attributes["LastActivityDescription"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("LastActivityDescription");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldLastActivityDescription");
            }
        }
        public string SalesForceFieldLastQualificationId
        {
            get
            {

                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@LastQualificationId");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["LastQualificationId"] != null)
                    att = node.Attributes["LastQualificationId"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("LastQualificationId");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldLastQualificationId");
            }
        }
        public string SalesForceFieldLastQualificationDescription
        {
            get
            {

                string strReturnVal = null;

                if (CustomConfig != null)
                {
                    try
                    {
                        XmlNode nde = CustomConfig.SelectSingleNode("/datasources/datasource/Fields/@LastQualificationDescription");
                        if (nde != null)
                            strReturnVal = nde.Value;
                    }
                    catch
                    {
                    }
                }

                return strReturnVal;
            }
            set
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(CustomConfig.OuterXml);
                CustomConfig = doc;

                XmlNode node = CustomConfig.SelectSingleNode("/datasources/datasource/Fields");
                if (node == null)
                {
                    node = CustomConfig.SelectSingleNode("/datasources/datasource");
                    node = node.AppendChild(CustomConfig.CreateElement("Fields"));
                }
                XmlAttribute att = null;
                if (node.Attributes["LastQualificationDescription"] != null)
                    att = node.Attributes["LastQualificationDescription"];
                else
                {
                    att = node.OwnerDocument.CreateAttribute("LastQualificationDescription");
                    node.Attributes.Append(att);
                }

                att.Value = value;


                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("SalesForceFieldLastQualificationDescription");
            }
        }


        public bool? AutomaticRecording
        {
            get
            {
                return (bool?)GetFieldValue<bool?>("AutomaticRecording");
            }
            set
            {
                SetFieldValue<bool?>("AutomaticRecording", value);
                foreach (Activity a in this.Activities)
                    a.FirePropertyChanged("AutomaticRecordingDescription");
                FirePropertyChanged("AutomaticRecordingDescription");
            }
        }

        public string AutomaticRecordingDescription
        {
            get
            {
                if (!AutomaticRecording.HasValue)
                {
                    
                    if (m_Core.Settings[0].AutomaticRecording.GetValueOrDefault())
                        return Translate("Conversations are recorded (inherited from global settings; if no override)");
                    else
                        return Translate("Conversations are not recorded (inherited from global settings; if no override)");
                }
                else
                {
                    if (AutomaticRecording.Value)
                        return Translate("Conversations are recorded (if no activity override)");
                    else
                        return Translate("Conversations are not recorded (if no activity override)");
                }
            }

        }


        public string CustomConfigText
        {
            get
            {
                if (CustomConfig == null)
                    return null;

                return CustomConfig.InnerXml;
            }
            set
            {
                CustomConfig = new XmlDocument();
                CustomConfig.LoadXml(value);
            }
        }

        public bool SpecifySystemTable
        {
            get
            {
                return SystemTable != null && SystemTable != DefaultSystemTableName;
            }
            set
            {
                if (value)
                {
                    SystemTable = string.Empty;
                }
                else
                {
                    SystemTable = DefaultSystemTableName;
                }

            }

        }

        public bool SpecifyQuotasTable
        {
            get
            {
                return QuotasTable != null && QuotasTable != DefaultQuotasTableName;
            }
            set
            {
                if (value)
                {
                    QuotasTable = string.Empty;
                }
                else
                {
                    QuotasTable = DefaultQuotasTableName;
                }

            }

        }


        public bool SpecifyDatabase
        {
            get
            {
                return DatabaseName!=null && DatabaseName != DefaultDbName;
            }
            set
            {
                if (value)
                {
                    DatabaseName = string.Empty;
                }
                else
                {
                    DatabaseName = DefaultDbName;
                    UserTable = "Data";
                }
                SystemTable = DefaultSystemTableName;                    
                QuotasTable = DefaultQuotasTableName;
            }
        }

        public string DatabaseName
        {
            get
            {
                return GetFieldValue<string>("DatabaseName");
            }
            set
            {
                if (value == null && DatabaseName != null)
                    ((ISession)AppDomain.CurrentDomain.GetData("SessionInfo")).Trace(string.Format("DatabaseName set to null on campaign {0} ({1})", Id, (new System.Diagnostics.TraceEventCache()).Callstack));

                SetFieldValue<string>("DatabaseName", value);
                FirePropertyChanged("DataSourceHasChanged");
                FirePropertyChanged("SpecifyDatabase");
                FirePropertyChanged("TablesList");
            }
        }

        public bool DataSourceHasChanged
        {
            get
            {
                if (!m_Loaded || !HasBeenLoaded)
                    return false;

                return this.GetFieldIsModified("DatabaseName") || this.GetFieldIsModified("UserTable") || this.GetFieldIsModified("SystemTable") || this.GetFieldIsModified("QuotasTable");
            }
        }

        public string UserTable
        {
            get
            {
                return GetFieldValue<string>("UserTable");
            }
            set
            {
                if(value==null && UserTable!=null)
                    ((ISession)AppDomain.CurrentDomain.GetData("SessionInfo")).Trace(string.Format("UserTable set to null on campaign {0} ({1})", Id, (new System.Diagnostics.TraceEventCache()).Callstack));

                SetFieldValue<string>("UserTable", value);
                FirePropertyChanged("DataSourceHasChanged");
                if(!m_Initiating && !m_Loaded && !HasBeenLoaded)
                {
                    if (ConfigurationManager.ConnectionStrings["admin"] == null || string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["admin"].ConnectionString))
                    {
                        try
                        {
                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=getfieldsconfig&database={1}&usertable={2}", Core.AdminUri, DatabaseName, UserTable));
                        webRequest.Method = WebRequestMethods.Http.Get;
                        webRequest.Timeout = 15 * 60000;
                        webRequest.AllowWriteStreamBuffering = true;

                            using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                            {
                                webRequest.Timeout = 720000;
                                using (Stream respStream = response.GetResponseStream())
                                {
                                    using (StreamReader sr = new StreamReader(respStream))
                                    {
                                        XmlDocument doc = new XmlDocument();
                                        doc.LoadXml(sr.ReadToEnd());
                                        FieldsConfig = doc;



                                        while (UserFields.Count > 0)
                                        {
                                            UserField uf = UserFields[0];
                                            Core.Delete(uf);
                                            UserFields.Remove(uf);
                                            uf.Delete();
                                        }

                                        XmlNodeList filterparts = FieldsConfig.SelectNodes("FieldsConfig/FieldConfig");
                                        foreach (XmlNode xmlsf in filterparts)
                                        {
                                            UserField tempSf = new UserField(UserFields);
                                            tempSf.Load(Id, xmlsf as XmlElement);
                                            tempSf.Id = System.Guid.NewGuid().ToString("N");
                                            Core.SetAdminObject(tempSf);
                                            UserFields.Add(tempSf);
                                            tempSf.DoneLoading();
                                        }

                                        SetFieldLoaded("FieldsConfig");

                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine(ex.ToString());
                        }
                    }
                }
            }
        }


        public string SystemTable
        {
            get
            {
                return GetFieldValue<string>("SystemTable");
            }
            set
            {
                if (value == null && SystemTable != null)
                    ((ISession)AppDomain.CurrentDomain.GetData("SessionInfo")).Trace(string.Format("SystemTable set to null on campaign {0} ({1})", Id, (new System.Diagnostics.TraceEventCache()).Callstack));
                    


                SetFieldValue<string>("SystemTable", value);
                FirePropertyChanged("DataSourceHasChanged");
                FirePropertyChanged("SpecifySystemTable");
            }
        }

        public string QuotasTable
        {
            get
            {
                return GetFieldValue<string>("QuotasTable");
            }
            set
            {
                if (value == null && QuotasTable != null)
                    ((ISession)AppDomain.CurrentDomain.GetData("SessionInfo")).Trace(string.Format("QuotasTable set to null on campaign {0} ({1})", Id, (new System.Diagnostics.TraceEventCache()).Callstack));
                    

                SetFieldValue<string>("QuotasTable", value);
                FirePropertyChanged("DataSourceHasChanged");
                FirePropertyChanged("SpecifyQuotasTable");
        }
        }

        public DataSourceType CustomDataSource
        {
            get
            {

                if (CustomConfig == null)
                    return DataSourceType.Standard;
                else
                {
                    try
                    {
                        if(CustomConfig.SelectSingleNode("datasources/datasource").Attributes["type"].Value == "ContactRoute.Dialer.SFContactsProvider, SFContactsProvider")
                        {
                            return DataSourceType.Salesforce;
                        }
                    }
                    catch
                    {
                    }
                    return DataSourceType.Custom;
                }


            }
            set
            {
                switch (value)
                {
                    case DataSourceType.Salesforce:
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(string.Format("<datasources><datasource id=\"{0}\" connection=\"https://login.salesforce.com/services/Soap/u/29.0\" source=\"\" sourcedescription=\"\" type=\"ContactRoute.Dialer.SFContactsProvider, SFContactsProvider\"></datasource></datasources>", System.Guid.NewGuid().ToString("N")));
                        if (!this.Advanced)
                            HasSystemSearchActivity = false;
                        CustomConfig = doc;
                        break;
                    case DataSourceType.Custom:
                        XmlDocument doc1 = new XmlDocument();
                        doc1.LoadXml("<datasources><datasource id=\"\" connection=\"\" source=\"\" type=\"\"></datasource></datasources>");
                        if (!this.Advanced)
                            HasSystemSearchActivity = false;
                        CustomConfig = doc1;
                        break;
                    case DataSourceType.Standard:
                        CustomConfig = null;
                        break;
                }
                FirePropertyChanged("CustomConfig");
                FirePropertyChanged("CustomConfigText");
                FirePropertyChanged("CustomDataSource");
                FirePropertyChanged("SearchAllowed");
            }
        }

        public bool SearchAllowed
        {
            get
            {
                return (CustomDataSource == DataSourceType.Standard);
            }
        }

        [AdminLoad(Path = "/Admin/Campaigns/Campaign[@id=\"{0}\"]/FieldsConfig/FieldsConfig/FieldConfig")]
        public AdminObjectList<UserField> UserFields
        {
            get
            {
                return m_UserFields;
            }
            internal set
            {
                if (m_UserFields != null)
                    m_UserFields.CollectionChanged -= new NotifyCollectionChangedEventHandler(m_UserFields_CollectionChanged);

                m_UserFields = value;

                if(m_UserFields!=null)
                    m_UserFields.CollectionChanged += new NotifyCollectionChangedEventHandler(m_UserFields_CollectionChanged);
            }
        }


        void m_UserFields_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RecomputeFieldsConfig();
            FirePropertyChanged("QuotaFields");
        }

        public IEnumerable<UserField> QuotaFields
        {
            get
            {
                return UserFields.Where((a) => (a.FieldMeaning == UserFieldMeanings.Quota));
            }
        }

        public string QuotasTimeRelatedDescription
        {
            get
            {
                if (QuotasGranularity == TimeRelatedQuotaGranularity.None)
                    return string.Empty;

                if(QuotasTimeRelatedFieldObject==null)
                    return string.Format(Translate("no field selected yet!"));

                return string.Format(Translate("based on {0} with {1} granularity"), QuotasTimeRelatedFieldObject.TypedDisplayText, Translate(new TimeRelatedQuotaGranularitiesHelper().First( (h)=>(h.EnumValue == QuotasGranularity)).Description));
            }            
        }

        public bool QuotasAreTimeRelated
        {
            get
            {
                return QuotasGranularity != TimeRelatedQuotaGranularity.None;
            }
            set
            {
                if (value)
                {
                    QuotasGranularity = TimeRelatedQuotaGranularity.Day;
                    if(string.IsNullOrEmpty(QuotasTimeRelatedField))
                    {
                        QuotasTimeRelatedField = SystemFields.First((f) => (f.FieldMeaning == SystemFieldMeanings.LastHandlingTime)).Id;
                    }
                }
                else
                    QuotasGranularity = TimeRelatedQuotaGranularity.None;
            }
        }

        public TimeRelatedQuotaGranularity QuotasGranularity
        {
            get
            {
                return GetFieldValue<TimeRelatedQuotaGranularity>("QuotasGranularity");
            }
            set
            {
                SetFieldValue<TimeRelatedQuotaGranularity>("QuotasGranularity", value);
                FirePropertyChanged("QuotasTimeRelatedDescription");
                FirePropertyChanged("QuotasAreTimeRelated");
            }
        }

        public bool QuotasTimeRelatedFieldIsNow
        {
            get
            {
                return GetFieldValue<bool>("QuotasTimeRelatedFieldIsNow");
            }
            set
            {
                SetFieldValue<bool>("QuotasTimeRelatedFieldIsNow", value);
            }
        }

        public int QuotasMinimumDelay
        {
            get
            {
                return GetFieldValue<int>("QuotasMinimumDelay");
            }
            set
            {
                SetFieldValue<int>("QuotasMinimumDelay", value);
                FirePropertyChanged("QuotaMinimumDelayLimited");
            }
        }

        public int QuotasMaximumDelay
        {
            get
            {
                return GetFieldValue<int>("QuotasMaximumDelay");
            }
            set
            {
                SetFieldValue<int>("QuotasMaximumDelay", value);
                FirePropertyChanged("QuotaMaximumDelayLimited");
            }
        }

        public bool QuotaMaximumDelayLimited
        {
            get
            {
                return QuotasMaximumDelay > 0;
            }
            set
            {
                if (value)
                    QuotasMaximumDelay = 30 * 24 * 60;
                else
                    QuotasMaximumDelay = 0;
            }
        }

        public bool QuotaMinimumDelayLimited
        {
            get
            {
                return QuotasMinimumDelay > 0;
            }
            set
            {
                if (value)
                    QuotasMinimumDelay = 24 * 60;
                else
                    QuotasMinimumDelay = 0;
            }
        }

        [AdminLoad(SkipLoad=true)]
        public string QuotasTimeRelatedField
        {
            get
            {
                return GetFieldValue<string>("QuotasTimeRelatedField");
            }
            set
            {
                SetFieldValue<string>("QuotasTimeRelatedField", value);                
                FirePropertyChanged("QuotasTimeRelatedFieldObject");
                FirePropertyChanged("QuotasTimeRelatedDescription");
            }
        }

        public Field QuotasTimeRelatedFieldObject
        {
            get
            {
                foreach (SystemField sf in SystemFields)
                    if (sf.Id == QuotasTimeRelatedField)
                        return sf;
                foreach (UserField uf in UserFields)
                    if (uf.Id == QuotasTimeRelatedField)
                        return uf;
                return null;
            }
        }

        [AdminLoad(Path = "/Admin/InboundActivities/InboundActivity[@campaignid=\"{0}\"]")]
        public AdminObjectList<InboundActivity> InboundActivities
        {
            get
            {
                return m_InboundActivities;
            }
            internal set
            {
                m_InboundActivities = value;
            }
        }

        [AdminLoad(Path = "/Admin/OutboundActivities/OutboundActivity[@campaignid=\"{0}\"]")]
        public AdminObjectList<OutboundActivity> OutboundActivities
        {
            get
            {
                return m_OutboundActivities;
            }
            internal set
            {

                m_OutboundActivities = value;
            }
        }

        public IEnumerable<OutboundActivity> AllOutboundActivities
        {
            get
            {
                return OutboundActivities.Union(Core.Hidden.OutboundActivities);
            }
        }

        private QualificationsCollecion m_Qualifications;
        private bool m_QualificationsLoaded;
        [AdminLoad(SkipLoad=true)]
        public QualificationsCollecion Qualifications
        {
            get
            {
                if (!m_QualificationsLoaded && Core!=null && !Core.IsSaving)
                {
                    XmlNode QNode = m_Core.GetXmlNodesById(m_Id);
                    m_Qualifications.Load((XmlElement)QNode, (System.Reflection.PropertyInfo)null);

                    m_QualificationsLoaded = true;
                }

                return m_Qualifications;
            }
            internal set
            {
                m_Qualifications = value;
            }
        }

        public AdminObjectReference<Qualification> Qualification
        {
            get;
            internal set;
        }

        public AdminObjectReference<AppointmentsContext> AppointmentsContext
        {
            get;
            internal set;
        }

        [AdminLoad(Path = "/Admin/PredefinedTexts/PredefinedText[@campaignid=\"{0}\"]")]
        public AdminObjectList<PredefinedText> PredefinedTexts
        {
            get
            {
                return m_PredefinedTexts;
            }
            internal set
            {
                if (m_PredefinedTexts != null)
                    m_PredefinedTexts.CollectionChanged -= new NotifyCollectionChangedEventHandler(m_PredefinedTexts_CollectionChanged);

                m_PredefinedTexts = value;

                if(m_PredefinedTexts!=null)
                    m_PredefinedTexts.CollectionChanged += new NotifyCollectionChangedEventHandler(m_PredefinedTexts_CollectionChanged);
            }
        }

        void m_PredefinedTexts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FirePropertyChanged("PredefinedTextsTabIsVisible");
        }

        [AdminLoad(Path = "/Admin/Attachments/Attachment[@campaignid=\"{0}\"]")]
        public AdminObjectList<Attachment> Attachments
        {
            get
            {
                return m_Attachments;
            }
            internal set
            {
                if (m_Attachments != null)
                    m_Attachments.CollectionChanged -= new NotifyCollectionChangedEventHandler(m_Attachments_CollectionChanged);
                
                m_Attachments = value;

                if(m_Attachments!=null)
                    m_Attachments.CollectionChanged += new NotifyCollectionChangedEventHandler(m_Attachments_CollectionChanged);
            }
        }

        void m_Attachments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FirePropertyChanged("AttachmentsTabIsVisible");
        }


        public InboundActivity SystemInboundActivity
        {
            get
            {
                foreach (InboundActivity inact in InboundActivities)
                    if (inact.IsSystemWithValidOwner && inact.MediaType == MediaType.Voice)
                        return inact;


                return null;
            }
        }

        public InboundActivity SystemChatActivity
        {
            get
            {
                foreach (InboundActivity inact in InboundActivities)
                    if (inact.IsSystemWithValidOwner && inact.MediaType == MediaType.Chat)
                        return inact;


                return null;
            }
        }

        public InboundActivity SystemEmailActivity
        {
            get
            {
                foreach (InboundActivity inact in InboundActivities)
                    if (inact.IsSystemWithValidOwner && inact.MediaType == MediaType.Mail)
                        return inact;


                return null;
            }
        }

        public OutboundActivity SystemOutboundActivity
        {
            get
            {
                foreach (OutboundActivity outact in OutboundActivities)
                    if (outact.IsSystemWithValidOwner && outact.OutboundMode != DialingMode.Search && (SystemInboundActivity == null || SystemInboundActivity.CallbackActivity.Target != outact))
                        return outact;

                return null;
            }
        }

        public OutboundActivity SystemSearchActivity
        {
            get
            {
                foreach (OutboundActivity outact in OutboundActivities)
                    if (outact.IsSystemWithValidOwner && outact.OutboundMode == DialingMode.Search && (SystemInboundActivity==null || SystemInboundActivity.CallbackActivity.Target!=outact) )
                        return outact;

                return null;
            }
        }

        public OutboundActivity SystemCallbackActivity
        {
            get
            {
                foreach (OutboundActivity outact in OutboundActivities)
                    if (outact.IsSystemWithValidOwner && SystemInboundActivity!=null && SystemInboundActivity.CallbackActivity.Target == outact)
                        return outact;

                return null;
            }
        }

        public bool HasSystemInboundActivity
        {
            get
            {
                return SystemInboundActivity != null;
            }
            set
            {
                if (value)
                {
                    // TODO
                    // take a look in the inactive objects... and reactivate the object
                    // if not there, create it.
                    if (false)
                    {
                    }
                    else
                    {                        

                        InboundActivity newAct = m_Core.Create<InboundActivity>();

                        newAct.OwnerId = Id;
                        newAct.MediaType = MediaType.Voice;
                        newAct.Campaign.TargetId= Id;
                        newAct.Description = string.Concat("Inbound ", Description);

                        m_Core.InboundActivities.Add(newAct);
                        InboundActivities.Add(newAct);


                        newAct.Queue.Target = SystemQueue;

                        OutboundActivity newCB = m_Core.Create<OutboundActivity>();
                        newCB.OwnerId = Id;
                        newCB.OutboundMode = DialingMode.Progressive;
                        newCB.Location.TargetId = "defaultlocation+++++++++++++++++";
                        newCB.Carrier.TargetId = "defaultcarrier++++++++++++++++++";
                        newCB.CallbackRules.TargetId = "defaultcbrules++++++++++++++++++";
                        newCB.Campaign.TargetId = Id;
                        newCB.ActivityFilterParts.Remove("_1null");
                        newCB.Description = String.Concat("Callbacks ", Description);
                        FilterPart fp = m_Core.Create<FilterPart>();
                        fp.Field.TargetId = m_Core.SystemFields.First( (f) => (f.FieldMeaning == SystemFieldMeanings.State) ).Id;
                        fp.Operator = Operator.Equal;
                        fp.OperandText = "15";
                        newCB.FilterParts.Add(fp);
                        newAct.CallbackActivity.TargetId = newCB.Id;
                        m_Core.OutboundActivities.Add(newCB);
                        OutboundActivities.Add(newCB);

                        newCB.Queue.Target = SystemQueue;
                        
                        FirePropertyChanged("Prompts");
                    }
                }
                else
                {
                    if (SystemInboundActivity != null)
                    {
                        AdminObject ao = SystemInboundActivity;

                        m_Core.Delete(ao);

                        AdminObject cb = SystemOutboundActivity;
 
                        m_Core.Delete(cb);

                    }
                }
                FirePropertyChanged("PromptsTabIsVisible");
            }
        }

        public bool HasSystemChatActivity
        {
            get
            {
                return SystemChatActivity != null;
            }
            set
            {
                if (value)
                {


                        InboundActivity newAct = m_Core.Create<InboundActivity>();

                        newAct.OwnerId = Id;
                        newAct.MediaType = MediaType.Chat;
                        newAct.Campaign.TargetId = Id;
                        newAct.Description = string.Concat("Chat ", Description);


                        m_Core.InboundActivities.Add(newAct);
                        InboundActivities.Add(newAct);

                        newAct.Queue.Target = SystemQueue;
              
                }
                else
                {
                    if (SystemChatActivity != null)
                    {

                        AdminObject ao = SystemChatActivity;

                        m_Core.Delete(ao);
                    }
                }
                FirePropertyChanged("AttachmentsTabIsVisible");
                FirePropertyChanged("PredefinedTextsTabIsVisible");
            }
        }

        public bool HasSystemEmailActivity
        {
            get
            {
                return SystemEmailActivity != null;
            }
            set
            {
                if (value)
                {
                    // TODO
                    // take a look in the inactive objects... and reactivate the object
                    // if not there, create it.
                    if (false)
                    {
                    }
                    else
                    {
                        InboundActivity newAct = m_Core.Create<InboundActivity>();

                        newAct.OwnerId = Id;
                        newAct.MediaType = MediaType.Mail;
                        newAct.Campaign.TargetId = Id;
                        newAct.Description = string.Concat("Email ", Description);
                        m_Core.InboundActivities.Add(newAct);
                        InboundActivities.Add(newAct);

                        newAct.Queue.Target = SystemQueue;

                    }
                }
                else
                {
                    if (SystemEmailActivity != null)
                    {
                        AdminObject ao = SystemEmailActivity;

                        m_Core.Delete(ao);
                    }
                }
                FirePropertyChanged("AttachmentsTabIsVisible");
                FirePropertyChanged("PredefinedTextsTabIsVisible");


            }
        }

        public bool HasSystemOutboundActivity
        {
            get
            {
                return SystemOutboundActivity != null;
            }
            set
            {
                if (value)
                {
                    // TODO
                    // take a look in the inactive objects... and reactivate the object
                    // if not there, create it.
                    if (false)
                    {
                    }
                    else
                    {
                        OutboundActivity newAct = m_Core.Create<OutboundActivity>();

                        newAct.OwnerId = Id;
                        newAct.OutboundMode = DialingMode.Progressive;
                        newAct.Location.TargetId = "defaultlocation+++++++++++++++++";
                        newAct.Carrier.TargetId = "defaultcarrier++++++++++++++++++";
                        newAct.CallbackRules.TargetId = "defaultcbrules++++++++++++++++++";
                        newAct.Campaign.TargetId = Id;
                        newAct.Description = String.Concat("Outbound ", Description);
                        m_Core.OutboundActivities.Add(newAct);
                        OutboundActivities.Add(newAct);

                        newAct.Queue.Target = SystemQueue;

                    }
                }
                else
                {
                    if (SystemOutboundActivity != null)
                    {
                        AdminObject ao = SystemOutboundActivity;

                        m_Core.Delete(ao);
                    }
                }

                FirePropertyChanged("PromptsTabIsVisible");
            }
        }

        public bool HasSystemSearchActivity
        {
            get
            {
                return SystemSearchActivity != null;
            }
            set
            {
                if (value)
                {
                    // TODO
                    // take a look in the inactive objects... and reactivate the object
                    // if not there, create it.
                    if (false)
                    {
                    }
                    else
                    {
                        OutboundActivity newAct = m_Core.Create<OutboundActivity>();

                        newAct.OwnerId = Id;
                        newAct.OutboundMode = DialingMode.Search;
                        newAct.Campaign.TargetId = Id;
                        newAct.Description = String.Concat("Search ", Description);
                        newAct.Location.TargetId = "defaultlocation+++++++++++++++++";
                        newAct.Carrier.TargetId = "defaultcarrier++++++++++++++++++";
                        newAct.CallbackRules.TargetId = "defaultcbrules++++++++++++++++++";
                        m_Core.OutboundActivities.Add(newAct);
                        OutboundActivities.Add(newAct);

                        newAct.Queue.Target = SystemQueue;

                    }
                }
                else
                {
                    if (SystemSearchActivity != null)
                    {
                        AdminObject ao = SystemSearchActivity;

                        m_Core.Delete(ao);
                    }
                }
            }
        }

        public bool HasInboundActivities
        {
            get
            {
                return InboundActivities != null && InboundActivities.Count > 0;
            }
        }

        public bool HasOutboundActivities
        {
            get
            {
                return OutboundActivities != null && OutboundActivities.Count > 0;
            }
        }

        public bool HasActivities
        {
            get
            {
                return HasInboundActivities || HasOutboundActivities;
            }
        }

        public bool AttachmentsTabIsVisible
        {
            get
            {
                return Attachments.Count>0 || 
                    (!Advanced &&  (HasSystemEmailActivity || HasSystemChatActivity) ) ||  
                    ( Advanced &&  InboundActivities.Count>0  && 
                        ( InboundActivities.Count( (a) => (a.MediaType==MediaType.Chat) )>0 || InboundActivities.Count( (a) => (a.MediaType==MediaType.Mail) )>0 ) );
            }
        }
        
        public bool PredefinedTextsTabIsVisible
        {
            get
            {
                return PredefinedTexts.Count > 0 ||
                    (!Advanced && (HasSystemEmailActivity || HasSystemChatActivity)) ||
                    (Advanced && InboundActivities.Count > 0 &&
                        (InboundActivities.Count((a) => (a.MediaType == MediaType.Chat)) > 0 || InboundActivities.Count((a) => (a.MediaType == MediaType.Mail)) > 0));
            }
        }
        
        public bool PromptsTabIsVisible
        {
            get
            {
                return Prompts.Count > 0 ||
                    (!Advanced && (HasSystemInboundActivity || HasSystemOutboundActivity) ||
                    ( Advanced && Activities.Count() > 0 && Activities.Count( (a) => (a.MediaType== MediaType.Voice))>0) );
            }
        }

        public Team SystemTeam
        {
            get
            {
                if (m_Core == null)
                    return null;
                foreach (Team team in m_Core.Teams)
                {
                    if (team.Owner == this)
                    {
                        return team;
                    }
                }

                return null;
            }
        }

        public Queue SystemQueue
        {
            get
            {
                if (m_Core==null || m_Core.Queues == null)
                    return null;

                foreach (Queue queue in m_Core.Queues)
                {
                    if (queue.Owner == this)
                    {
                        return queue;
                    }
                }

                return null;
            }
        }

        public bool Advanced
        {
            get
            {
                return GetFieldValue<bool>("Advanced");
            }
            set
            {
                SetFieldValue<bool>("Advanced", value);
                if (value)
                {
                    if (SystemTeam == null)
                        return;

                    foreach (Activity act in Activities)
                    {
                        if (act.IsSystemWithValidOwner)
                        {
                            foreach (PromptLink p in act.Prompts)
                                SystemQueue.Prompts.Add(p.Prompt);
                        }
                    }

                    foreach (PromptLink p in Prompts)
                        SystemQueue.Prompts.Add(p.Prompt);

                    SystemTeam.Description = string.Concat(Translate("Team "), SystemTeam.DisplayText);
                    SystemTeam.OwnerId = null;

                    SystemQueue.Description = string.Concat(Translate("Queue "), SystemQueue.DisplayText);
                    SystemQueue.OwnerId = null;

                    Activity sca = SystemCallbackActivity;
                    Activity search = SystemSearchActivity;

                    foreach (Activity act in Activities)
                    {
                        if (act.IsSystemWithValidOwner)
                        {
                            OutboundActivity oa = act as OutboundActivity;

                            if (oa != null)
                            {
                                if (act == sca)
                                    act.Description = string.Concat(Translate("Callbacks "), act.ShortDisplayText);
                                else if (act == search)
                                    act.Description = string.Concat(Translate("Search "), act.ShortDisplayText);
                                else
                                    act.Description = string.Concat(Translate("Outbound "), act.ShortDisplayText);
                            }
                            else
                            {
                                InboundActivity ia = act as InboundActivity;
                                if(ia!=null)
                                {
                                    if(ia.MediaType == MediaType.Voice)
                                    {
                                        act.Description = string.Concat(Translate("Inbound "), act.ShortDisplayText);
                                    }
                                    else if(ia.MediaType == MediaType.Mail)
                                    {
                                        act.Description = string.Concat(Translate("Mail "), act.ShortDisplayText);
                                    }
                                    else if(ia.MediaType == MediaType.Chat)
                                    {
                                        act.Description = string.Concat(Translate("Chat "), act.ShortDisplayText);
                                    }
                                }
                            }
                            act.OwnerId = null;
                        }
                    }

                    m_Core.Teams.Refresh();
                    m_Core.Queues.Refresh();
                    InboundActivities.Refresh();
                    OutboundActivities.Refresh();
                }
            }
        }

        public IEnumerable<Activity> Activities
        {
            get 
            {
                if (InboundActivities != null && OutboundActivities != null)
                {
                    IEnumerator<InboundActivity> Inbound = this.InboundActivities.GetEnumerator();

                    while (Inbound.MoveNext())
                        yield return Inbound.Current;

                    IEnumerator<OutboundActivity> Outbound = this.OutboundActivities.GetEnumerator();

                    while (Outbound.MoveNext())
                        yield return Outbound.Current;
                }
            }
        }

        public void PauseChat(bool pause)
        {
            if (HasInboundActivities)
            {
                for (int i = 0; i < InboundActivities.Count; i++)
                {
                    if (InboundActivities[i].MediaType == MediaType.Chat)
                    {
                        if (InboundActivities[i].Paused != pause)
                            InboundActivities[i].Paused = pause;
                    }
                }
            }
        }

        public void PauseMail(bool pause)
        {
            if (HasInboundActivities)
            {
                for (int i = 0; i < InboundActivities.Count; i++)
                {
                    if (InboundActivities[i].MediaType == MediaType.Mail)
                    {
                        if (InboundActivities[i].Paused != pause)
                            InboundActivities[i].Paused = pause;
                    }
                }
            }
        }

        public void PauseInbound(bool pause)
        {
            if (HasInboundActivities)
            {
                for (int i = 0; i < InboundActivities.Count; i++)
                {
                    if (InboundActivities[i].MediaType == MediaType.Voice)
                    {
                        if (InboundActivities[i].Paused != pause)
                            InboundActivities[i].Paused = pause;
                    }
                }
            }
        }

        public void PauseOutbound(bool pause)
        {
            if (HasOutboundActivities && OutboundActivities.Count > 0)
            {
                foreach (OutboundActivity inact in OutboundActivities)
                {
                    if (inact.Paused!=pause)
                        inact.Paused = pause;
                }
            }
        }

        public CampaignStatus InboundStatus
        {
            get
            {
                if(!Advanced && IsReadOnly)
                    return CampaignStatus.NotActive;

                if (HasInboundActivities)
                {
                    int count = 0;
                    int activeCount = 0;
                    for (int i = 0; i < InboundActivities.Count; i++)
                    {
                        if (InboundActivities[i].MediaType == MediaType.Voice && !InboundActivities[i].IsReadOnly)
                        {
                            count++;
                            if (!InboundActivities[i].Paused)
                                activeCount++;
                        }
                    }
                    if (count == 0)
                        return CampaignStatus.NotActive;
                    if (activeCount == 0)
                        return CampaignStatus.Paused;
                    if (activeCount == count)
                        return CampaignStatus.Running;
                    return CampaignStatus.Mixed;
                }
                else
                {
                    return CampaignStatus.NotActive;
                }
            }
        }

        public CampaignStatus OutboundStatus
        {
            get
            {
                if (!Advanced && IsReadOnly)
                    return CampaignStatus.NotActive;

                OutboundActivity callback = SystemCallbackActivity;

                if (HasOutboundActivities && OutboundActivities.Count > 0 && ( OutboundActivities.Count!=1 || OutboundActivities[0]!=callback) )
                {
                    int runningCount = 0;
                    int totalCount = 0;
                    foreach (OutboundActivity outact in OutboundActivities)
                    {
                        if (!outact.IsReadOnly)
                        {
                        if (outact != callback)
                        {
                            totalCount++;
                            if (!outact.Paused)
                                runningCount++;
                        }
                    }
                    }
                    if(totalCount == 0)
                        return CampaignStatus.NotActive;
                    if (runningCount == 0)
                        return CampaignStatus.Paused;
                    if (runningCount == totalCount)
                        return CampaignStatus.Running;

                    return CampaignStatus.Mixed;
                }
                else
                {
                    return CampaignStatus.NotActive;
                }
            }
        }

        public CampaignStatus MailStatus
        {
            get
            {
                if (!Advanced && IsReadOnly)
                    return CampaignStatus.NotActive;

                if (HasInboundActivities)
                {
                    int count = 0;
                    int activeCount = 0;
                    for (int i = 0; i < InboundActivities.Count; i++)
                    {
                        if (InboundActivities[i].MediaType == MediaType.Mail && !InboundActivities[i].IsReadOnly)
                        {
                            count++;
                            if (!InboundActivities[i].Paused)
                                activeCount++;
                        }
                    }
                    if(count==0)
                        return CampaignStatus.NotActive;
                    if (activeCount == 0)
                        return CampaignStatus.Paused;
                    if (activeCount == count)
                        return CampaignStatus.Running;
                    return CampaignStatus.Mixed;
                }
                else
                {
                    return CampaignStatus.NotActive;
                }
            }
        }

        public CampaignStatus ChatStatus
        {
            get
            {
                if (!Advanced && IsReadOnly)
                    return CampaignStatus.NotActive;

                if (HasInboundActivities)
                {
                    int count = 0;
                    int activeCount = 0;
                    for (int i = 0; i < InboundActivities.Count; i++)
                    {
                        if (InboundActivities[i].MediaType == MediaType.Chat && !InboundActivities[i].IsReadOnly)
                        {
                            count++;
                            if (!InboundActivities[i].Paused)
                                activeCount++;
                        }
                    }
                    if (count == 0)
                        return CampaignStatus.NotActive;
                    if (activeCount == 0)
                        return CampaignStatus.Paused;
                    if (activeCount == count)
                        return CampaignStatus.Running;
                    return CampaignStatus.Mixed;
                }
                else
                {
                    return CampaignStatus.NotActive;
                }
            }
        }

        public object CheckedTeams
        {
            get
            {
                if (SystemQueue != null)
                    return new AdminCheckedLinkList<Team, QueueTeam>(m_Core.Teams /* TODO: allow passing delegate (t) => (!t.IsSystem || t.Owner == this) */, SystemQueue.Teams, SystemQueue);
                else
                    return null;
            }
        }

        public AdminCheckedLinkList<Agent, AgentTeam> CheckedAgents
        {
            get
            {
                if (SystemQueue == null || SystemTeam == null)
                    return null;

                return new AdminCheckedLinkList<Agent, AgentTeam>(m_Core.Agents, new AdminObjectList[]{ SystemQueue.Teams}, "Team.Agents" , SystemTeam.Agents, SystemTeam, null);
            }
        }

        [AdminLoad(SkipLoad = true)]
        [AdminDelete(SkipDelete = true)]
        public AdminObjectList<SystemField> SystemFields
        {
            get
            {
                if (m_Core == null)
                    return null;
                return m_Core.SystemFields;
            }
        }

        [AdminLoad(SkipLoad = true)]
        [AdminDelete(SkipDelete = true)]
        public AdminObjectList<QuotaField> CombinedQuotaFields
        {
            get
            {
                if (m_Core == null)
                    return null;

                return m_Core.QuotaFields;
            }
        }


        [AdminLoad(SkipLoad = true)]
        public AdminObjectList<CurrentActivityFilter> CampaignCurrentActivityFilterKinds
        {
            get;
            internal set;
        }

        [AdminDelete(SkipDelete = true)]
        [AdminLoad(SkipLoad = true)]
        public AdminObjectList<CurrentActivityFilter> DefaultCurrentActivityFilterKinds
        {
            get
            {
                if (m_Core == null)
                    return null;
                return m_Core.ActivityFilters;
            }
        }

        public IEnumerable<CurrentActivityFilter> CurrentActivityFilterKinds
        {
            get
            {
                return DefaultCurrentActivityFilterKinds.Union(CampaignCurrentActivityFilterKinds);
            }
        }

        public IEnumerable<Field> Fields
        {
            get
            {
                if(UserFields.FirstOrDefault( (a) => (a.FieldMeaning == UserFieldMeanings.Quota)  ) == null)
                    return UserFields.Union<Field>(SystemFields).OrderBy((f) => (f.TypedDisplayText));
                else
                    return UserFields.Union<Field>(CombinedQuotaFields).Union<Field>(SystemFields).OrderBy( (f) => (f.TypedDisplayText) );
            }
        }

        public string ExportFields
        {
            get
            {
                return GetFieldValue<string>("ExportFields");
            }
            set
            {
                SetFieldValue<string>("ExportFields", value);
                FirePropertyChanged("ExportFieldsCollection");
            }
        }

        public IEnumerable<string> ExportFieldsCollection
        {
            get
            {
                return ExportFields.Split(',');
            }
        }

        [AdminLoad(SkipLoad = true)]
        public IEnumerable<AdminObject> LookupAgents
        {
            get
            {
                return m_Core.LookupAgents.OrderBy( (a) => (a.DisplayText) );
            }
        }

        [AdminLoad(SkipLoad = true)]
        public IEnumerable<AdminObject> LookupActivities
        {
            get
            {
                return Activities.OrderBy ( (a) => (a.DisplayText) ) ;
            }
        }

        [AdminLoad(SkipLoad = true)]
        public IEnumerable<AdminObject> LookupAreas
        {
            get
            {

                if (AppointmentsContext!=null && AppointmentsContext.HasTarget)
                {
                    return AppointmentsContext.Target.Areas.OrderBy((a) => (a == null ? string.Empty : a.DisplayText));
                }
                return new AdminObject[] { };
            }
        }

        [AdminLoad(SkipLoad = true)]
        public IEnumerable<AdminObject> LookupQualifications
        {
            get
            {
                return Qualifications.Where((a) => (a.Children.Count==0)).OrderBy( (a) => (a.DisplayText) );
            }
        }

        [AdminLoad(SkipLoad = true)]
        public IEnumerable<AdminObject> StringAgentsFields
        {
            get
            {
                IEnumerable<AdminObject> retValue = LookupAgents.Union(Fields.Where((a) => (a.DBType == DBTypes.Char || a.DBType == DBTypes.String)));
                return retValue;
            }
        }

        [AdminLoad(SkipLoad = true)]
        public IEnumerable<AdminObject> StringActivitiesFields
        {
            get
            {
                IEnumerable<AdminObject> retValue = LookupActivities.Union(Fields.Where((a) => (a.DBType == DBTypes.Char || a.DBType == DBTypes.String)));
                return retValue;
            }
        }

        [AdminLoad(SkipLoad = true)]        
        public IEnumerable<AdminObject> StringAreasFields
        {
            get
            {
                IEnumerable<AdminObject> retValue = LookupAreas.Union(Fields.Where((a) => (a.DBType == DBTypes.Char || a.DBType == DBTypes.String)));
                return retValue;
            }
        }

        [AdminLoad(SkipLoad = true)]
        public IEnumerable<AdminObject> StringQualificationsFields
        {
            get
            {
                IEnumerable<AdminObject> retValue = LookupQualifications.Union(Fields.Where((a) => (a.DBType == DBTypes.Char || a.DBType == DBTypes.String)));
                return retValue;
            }
        }

        [AdminLoad(SkipLoad = true)]
        public IEnumerable<Field> StringFields
        {
            get
            {
                return Fields.Where( (a) => (a.DBType == DBTypes.Char || a.DBType == DBTypes.String) );
            }
        }

        [AdminLoad(SkipLoad = true)]
        public IEnumerable<Field> IntFields
        {
            get
            {
                return Fields.Where((a) => (a.DBType == DBTypes.Integer || a.DBType == DBTypes.Float));
            }
        }

        [AdminLoad(SkipLoad = true)]
        public IEnumerable<Field> DateTimeFields
        {
            get
            {
                return Fields.Where((a) => (a.DBType == DBTypes.Datetime));
            }
        }

        [AdminLoad(SkipLoad = true)]
        public IEnumerable<Field> BoolFields
        {
            get
            {
                return Fields.Where((a) => (a.DBType == DBTypes.Boolean));
            }
        }


        private AdminObjectList<PromptLink> m_Prompts;

        [AdminLoad(Path = "/Admin/PromptsLinks/PromptLink[@adminobjectid=\"{0}\"]")]
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
                PromptsChangedOnChildActivities();
            }
        }

        private void m_Prompts_Collection_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (m_Loaded)
            {
                PromptsChangedOnChildActivities();
                FirePropertyChanged("PromptsTabIsVisble");
            }
        }

        private void PromptsChangedOnChildActivities()
        {
            if (Activities != null)
            {
                foreach (Activity inact in Activities)
                {
                    inact.FirePropertyChanged("AllPrompts");
                    inact.FirePropertyChanged("MusicPrompts");
                }
            }
        }

        public void RemoveQualification(Qualification qual)
        {
            qual.Remove();
        }
        
        public XmlDocument FieldsConfig
        {
            get
            {
                return GetFieldValue<XmlDocument>("FieldsConfig");
            }
            set
            {
                SetFieldValue<XmlDocument>("FieldsConfig", value);
            }
        }

        internal override void Load(XmlElement node)
        {
            base.Load(node);

            XmlNode nde = node.SelectSingleNode("QuotasTimeRelatedField");
            if (nde != null && !string.IsNullOrEmpty(nde.InnerText))
            {
                bool bFound = false;
                foreach (SystemField sf in SystemFields)
                {
                    if (sf.Id == nde.InnerText)
                    {
                        QuotasTimeRelatedField = nde.InnerText;
                        SetFieldLoaded("QuotasTimeRelatedField");
                        bFound = true;
                        break;
                    }
                }

                if (!bFound)
                {
                    foreach (UserField uf in UserFields)
                    {
                        if (uf.Name == nde.InnerText)
                        {
                            QuotasTimeRelatedField = uf.Id;
                            SetFieldLoaded("QuotasTimeRelatedField");
                            break;
                        }
                    }
                }
            }

            QualificationsCollecion temp = Qualifications;
            
            m_Loaded = true;
            RecomputeFieldsConfig();
            FirePropertyChanged("QuotaFields");


            DoneLoading();
        }
        public override void Save(XmlDocument doc)
        {
            if (CustomDataSource == DataSourceType.Standard)
            {
                if (string.IsNullOrEmpty(DatabaseName))
                {
                    SystemTable = getDefaultSystemTableName(null, null);
                    QuotasTable = getDefaultQuotasTableName(null, null);
                    UserTable = "Data";
                    DatabaseName = DefaultDbName;
                }

                    
            }



            base.Save(doc);

            if (QuotasTimeRelatedFieldObject!=null && QuotasTimeRelatedFieldObject is UserField)
            {
                XmlNode nde = doc.SelectSingleNode(string.Format("admin/Campaign[@id='{0}']/QuotasTimeRelatedField", Id));
                if (nde != null)
                    nde.InnerText = QuotasTimeRelatedFieldObject.Name;
            }
            
        }


        public void RecomputeFieldsConfig()
        {
            if (HasBeenLoaded && !m_Loaded)
                return;

            if (UserFields == null || UserFields.Count == 0)
            {
                FieldsConfig = null;
                return;
            }

            StringBuilder sb = new StringBuilder();
            XmlDocument doc = new XmlDocument();
            
            doc.AppendChild(doc.CreateElement("FieldsConfig"));
            
            foreach (UserField uf in UserFields.OrderBy( (ua)=>(ua.Name) ) )
            {
                uf.SaveNode(doc);
            }

            FieldsConfig = doc;
        }


        public DataSet GetData(bool onlyTop, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=getData", Core.AdminUri));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Post;

            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    tw.WriteLine(string.Format("campaignId={0}", Id));
                    tw.WriteLine(string.Format("onlyTop={0}", onlyTop));
                    tw.WriteLine(string.Format("qualifications={0}", qualifications == null ? string.Empty : string.Join(",", qualifications)));
                    tw.WriteLine(string.Format("negativeonly={0}", negativeonly));
                    tw.WriteLine(string.Format("excludedonly={0}", excludedonly));
                    tw.WriteLine(string.Format("exportableonly={0}", exportableonly));
                    tw.WriteLine(string.Format("batchnumber={0}", batchnumber));
                    tw.WriteLine(string.Format("tag={0}", string.IsNullOrEmpty(tag) ? null : Uri.EscapeDataString(tag)));
                    tw.WriteLine(string.Format("exportnumber={0}", exportnumber));
                    tw.WriteLine(string.Format("notexportedyet={0}", notexportedyet));
                    tw.WriteLine(string.Format("disconnectReasons={0}", disconnectReasons == null ? string.Empty : string.Join(",", disconnectReasons)));
                    tw.WriteLine(string.Format("dialStates={0}", dialStates == null ? string.Empty : string.Join(",", dialStates)));
                    tw.WriteLine(string.Format("agentids={0}", agentids == null ? string.Empty : string.Join(",", agentids)));
                    tw.WriteLine(string.Format("activities={0}", activities == null ? string.Empty : string.Join(",", activities)));
                    tw.WriteLine(string.Format("from={0}", From == null ? string.Empty : From.GetValueOrDefault().ToBinary().ToString() ));
                    tw.WriteLine(string.Format("to={0}", To == null ? string.Empty : To.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("advancedFilter={0}", advancedFilter == null ? string.Empty : Uri.EscapeDataString(advancedFilter.InnerXml)));
                    tw.Close();
                }

                webRequestStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    DataSet ds = new DataSet();
                    using (Stream respStream = response.GetResponseStream())
                    {      
                        ds.ReadXml(respStream);
                    }
                    response.Close();
                    return ds;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
            return null;
        }
        public int GetDataCount(string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=getDataCount", Core.AdminUri));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Post;

            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    tw.WriteLine(string.Format("campaignId={0}", Id));
                    tw.WriteLine(string.Format("qualifications={0}", qualifications==null ? string.Empty: string.Join(",", qualifications)));
                    tw.WriteLine(string.Format("negativeonly={0}", negativeonly));
                    tw.WriteLine(string.Format("excludedonly={0}", excludedonly));
                    tw.WriteLine(string.Format("exportableonly={0}", exportableonly));
                    tw.WriteLine(string.Format("batchnumber={0}", batchnumber));
                    tw.WriteLine(string.Format("tag={0}", string.IsNullOrEmpty(tag) ? null : Uri.EscapeDataString(tag)));
                    tw.WriteLine(string.Format("exportnumber={0}", exportnumber));
                    tw.WriteLine(string.Format("notexportedyet={0}", notexportedyet));

                    tw.WriteLine(string.Format("disconnectReasons={0}", disconnectReasons==null ? string.Empty : string.Join(",", disconnectReasons)));
                    tw.WriteLine(string.Format("dialStates={0}", dialStates==null ? string.Empty : string.Join(",", dialStates)));
                    tw.WriteLine(string.Format("agentids={0}", agentids==null? string.Empty : string.Join(",", agentids)));
                    tw.WriteLine(string.Format("activities={0}", activities==null ? string.Empty : string.Join(",", activities)));
                    tw.WriteLine(string.Format("from={0}", From == null ? string.Empty : From.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("to={0}", To == null ? string.Empty : To.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("advancedFilter={0}", advancedFilter == null ? string.Empty : Uri.EscapeDataString(advancedFilter.InnerXml)));

                    tw.Close();
                }
                    
                webRequestStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                        
                    string strResponse = null;
                    using (Stream respStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            strResponse = sr.ReadToEnd();
                        }
                    }
                    response.Close();
                    if(strResponse!=null)
                        return Int32.Parse(strResponse);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
            return -1;
        }
        public int DataManageDelete(string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=dataManageDelete", Core.AdminUri));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.Timeout = 15 * 60000;
            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    tw.WriteLine(string.Format("user={0}", Core.m_CreatorId));
                    tw.WriteLine(string.Format("campaignId={0}", Id));
                    tw.WriteLine(string.Format("qualifications={0}", qualifications == null ? string.Empty : string.Join(",", qualifications)));
                    tw.WriteLine(string.Format("negativeonly={0}", negativeonly));
                    tw.WriteLine(string.Format("excludedonly={0}", excludedonly));
                    tw.WriteLine(string.Format("exportableonly={0}", exportableonly));
                    tw.WriteLine(string.Format("batchnumber={0}", batchnumber));
                    tw.WriteLine(string.Format("tag={0}", string.IsNullOrEmpty(tag) ? null : Uri.EscapeDataString(tag)));
                    tw.WriteLine(string.Format("exportnumber={0}", exportnumber));
                    tw.WriteLine(string.Format("notexportedyet={0}", notexportedyet));

                    tw.WriteLine(string.Format("disconnectReasons={0}", disconnectReasons == null ? string.Empty : string.Join(",", disconnectReasons)));
                    tw.WriteLine(string.Format("dialStates={0}", dialStates == null ? string.Empty : string.Join(",", dialStates)));
                    tw.WriteLine(string.Format("agentids={0}", agentids == null ? string.Empty : string.Join(",", agentids)));
                    tw.WriteLine(string.Format("activities={0}", activities == null ? string.Empty : string.Join(",", activities)));
                    tw.WriteLine(string.Format("from={0}", From == null ? string.Empty : From.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("to={0}", To == null ? string.Empty : To.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("advancedFilter={0}", advancedFilter == null ? string.Empty : Uri.EscapeDataString(advancedFilter.InnerXml)));

                    tw.Close();
                }

                webRequestStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {

                    string strResponse = null;
                    using (Stream respStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            strResponse = sr.ReadToEnd();
                        }
                    }
                    response.Close();
                    if (strResponse != null)
                        return Int32.Parse(strResponse);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return -1;
        }
        public int DataManageInclude(string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=dataManageInclude", Core.AdminUri));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.Timeout = 15 * 60000;

            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    tw.WriteLine(string.Format("user={0}", Core.m_CreatorId));
                    tw.WriteLine(string.Format("campaignId={0}", Id));
                    tw.WriteLine(string.Format("qualifications={0}", qualifications == null ? string.Empty : string.Join(",", qualifications)));
                    tw.WriteLine(string.Format("negativeonly={0}", negativeonly));
                    tw.WriteLine(string.Format("excludedonly={0}", excludedonly));
                    tw.WriteLine(string.Format("exportableonly={0}", exportableonly));
                    tw.WriteLine(string.Format("batchnumber={0}", batchnumber));
                    tw.WriteLine(string.Format("tag={0}", string.IsNullOrEmpty(tag) ? null : Uri.EscapeDataString(tag)));
                    tw.WriteLine(string.Format("exportnumber={0}", exportnumber));
                    tw.WriteLine(string.Format("notexportedyet={0}", notexportedyet));

                    tw.WriteLine(string.Format("disconnectReasons={0}", disconnectReasons == null ? string.Empty : string.Join(",", disconnectReasons)));
                    tw.WriteLine(string.Format("dialStates={0}", dialStates == null ? string.Empty : string.Join(",", dialStates)));
                    tw.WriteLine(string.Format("agentids={0}", agentids == null ? string.Empty : string.Join(",", agentids)));
                    tw.WriteLine(string.Format("activities={0}", activities == null ? string.Empty : string.Join(",", activities)));
                    tw.WriteLine(string.Format("from={0}", From == null ? string.Empty : From.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("to={0}", To == null ? string.Empty : To.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("advancedFilter={0}", advancedFilter == null ? string.Empty : Uri.EscapeDataString(advancedFilter.InnerXml)));

                    tw.Close();
                }

                webRequestStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {

                    string strResponse = null;
                    using (Stream respStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            strResponse = sr.ReadToEnd();
                        }
                    }
                    response.Close();
                    if (strResponse != null)
                        return Int32.Parse(strResponse);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return -1;
        }
        public int DataManageExclude(string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=dataManageExclude",Core.AdminUri));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.Timeout = 15 * 60000;

            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    tw.WriteLine(string.Format("user={0}", Core.m_CreatorId));
                    tw.WriteLine(string.Format("campaignId={0}", Id));
                    tw.WriteLine(string.Format("qualifications={0}", qualifications == null ? string.Empty : string.Join(",", qualifications)));
                    tw.WriteLine(string.Format("negativeonly={0}", negativeonly));
                    tw.WriteLine(string.Format("excludedonly={0}", excludedonly));
                    tw.WriteLine(string.Format("exportableonly={0}", exportableonly));
                    tw.WriteLine(string.Format("batchnumber={0}", batchnumber));
                    tw.WriteLine(string.Format("tag={0}", string.IsNullOrEmpty(tag) ? null : Uri.EscapeDataString(tag)));
                    tw.WriteLine(string.Format("exportnumber={0}", exportnumber));
                    tw.WriteLine(string.Format("notexportedyet={0}", notexportedyet));

                    tw.WriteLine(string.Format("disconnectReasons={0}", disconnectReasons == null ? string.Empty : string.Join(",", disconnectReasons)));
                    tw.WriteLine(string.Format("dialStates={0}", dialStates == null ? string.Empty : string.Join(",", dialStates)));
                    tw.WriteLine(string.Format("agentids={0}", agentids == null ? string.Empty : string.Join(",", agentids)));
                    tw.WriteLine(string.Format("activities={0}", activities == null ? string.Empty : string.Join(",", activities)));
                    tw.WriteLine(string.Format("from={0}", From == null ? string.Empty : From.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("to={0}", To == null ? string.Empty : To.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("advancedFilter={0}", advancedFilter == null ? string.Empty : Uri.EscapeDataString(advancedFilter.InnerXml)));

                    tw.Close();
                }

                webRequestStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {

                    string strResponse = null;
                    using (Stream respStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            strResponse = sr.ReadToEnd();
                        }
                    }
                    response.Close();
                    if (strResponse != null)
                        return Int32.Parse(strResponse);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return -1;
        }
        public int DataManageRemoveTimeConstraints(string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=dataManageRemoveTimeConstraints", Core.AdminUri));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.Timeout = 15 * 60000;

            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    tw.WriteLine(string.Format("user={0}", Core.m_CreatorId));
                    tw.WriteLine(string.Format("campaignId={0}", Id));
                    tw.WriteLine(string.Format("qualifications={0}", qualifications == null ? string.Empty : string.Join(",", qualifications)));
                    tw.WriteLine(string.Format("negativeonly={0}", negativeonly));
                    tw.WriteLine(string.Format("excludedonly={0}", excludedonly));
                    tw.WriteLine(string.Format("exportableonly={0}", exportableonly));
                    tw.WriteLine(string.Format("batchnumber={0}", batchnumber));
                    tw.WriteLine(string.Format("tag={0}", string.IsNullOrEmpty(tag) ? null : Uri.EscapeDataString(tag)));
                    tw.WriteLine(string.Format("exportnumber={0}", exportnumber));
                    tw.WriteLine(string.Format("notexportedyet={0}", notexportedyet));

                    tw.WriteLine(string.Format("disconnectReasons={0}", disconnectReasons == null ? string.Empty : string.Join(",", disconnectReasons)));
                    tw.WriteLine(string.Format("dialStates={0}", dialStates == null ? string.Empty : string.Join(",", dialStates)));
                    tw.WriteLine(string.Format("agentids={0}", agentids == null ? string.Empty : string.Join(",", agentids)));
                    tw.WriteLine(string.Format("activities={0}", activities == null ? string.Empty : string.Join(",", activities)));
                    tw.WriteLine(string.Format("from={0}", From == null ? string.Empty : From.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("to={0}", To == null ? string.Empty : To.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("advancedFilter={0}", advancedFilter == null ? string.Empty : Uri.EscapeDataString(advancedFilter.InnerXml)));

                    tw.Close();
                }

                webRequestStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {

                    string strResponse = null;
                    using (Stream respStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            strResponse = sr.ReadToEnd();
                        }
                    }
                    response.Close();
                    if (strResponse != null)
                        return Int32.Parse(strResponse);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return -1;
        }
        public int DataManageRecycle(string activityid, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=dataManageRecycle", Core.AdminUri));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.Timeout = 15 * 60000;

            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    tw.WriteLine(string.Format("user={0}", Core.m_CreatorId));
                    tw.WriteLine(string.Format("campaignId={0}", Id));
                    tw.WriteLine(string.Format("activityId={0}", activityid));
                    tw.WriteLine(string.Format("qualifications={0}", qualifications == null ? string.Empty : string.Join(",", qualifications)));
                    tw.WriteLine(string.Format("negativeonly={0}", negativeonly));
                    tw.WriteLine(string.Format("excludedonly={0}", excludedonly));
                    tw.WriteLine(string.Format("exportableonly={0}", exportableonly));
                    tw.WriteLine(string.Format("batchnumber={0}", batchnumber));
                    tw.WriteLine(string.Format("tag={0}", string.IsNullOrEmpty(tag) ? null : Uri.EscapeDataString(tag)));
                    tw.WriteLine(string.Format("exportnumber={0}", exportnumber));
                    tw.WriteLine(string.Format("notexportedyet={0}", notexportedyet));

                    tw.WriteLine(string.Format("disconnectReasons={0}", disconnectReasons == null ? string.Empty : string.Join(",", disconnectReasons)));
                    tw.WriteLine(string.Format("dialStates={0}", dialStates == null ? string.Empty : string.Join(",", dialStates)));
                    tw.WriteLine(string.Format("agentids={0}", agentids == null ? string.Empty : string.Join(",", agentids)));
                    tw.WriteLine(string.Format("activities={0}", activities == null ? string.Empty : string.Join(",", activities)));
                    tw.WriteLine(string.Format("from={0}", From == null ? string.Empty : From.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("to={0}", To == null ? string.Empty : To.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("advancedFilter={0}", advancedFilter == null ? string.Empty : Uri.EscapeDataString(advancedFilter.InnerXml)));

                    tw.Close();
                }

                webRequestStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {

                    string strResponse = null;
                    using (Stream respStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            strResponse = sr.ReadToEnd();
                        }
                    }
                    response.Close();
                    if (strResponse != null)
                        return Int32.Parse(strResponse);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return -1;
        }
        public int DataManageReaffectCB(string agentId, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=dataManageReaffectCB", Core.AdminUri));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.Timeout = 15 * 60000;

            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    tw.WriteLine(string.Format("user={0}", Core.m_CreatorId));
                    tw.WriteLine(string.Format("campaignId={0}", Id));
                    tw.WriteLine(string.Format("qualifications={0}", qualifications == null ? string.Empty : string.Join(",", qualifications)));
                    tw.WriteLine(string.Format("negativeonly={0}", negativeonly));
                    tw.WriteLine(string.Format("excludedonly={0}", excludedonly));
                    tw.WriteLine(string.Format("exportableonly={0}", exportableonly));
                    tw.WriteLine(string.Format("batchnumber={0}", batchnumber));
                    tw.WriteLine(string.Format("tag={0}", string.IsNullOrEmpty(tag) ? null : Uri.EscapeDataString(tag)));
                    tw.WriteLine(string.Format("exportnumber={0}", exportnumber));
                    tw.WriteLine(string.Format("notexportedyet={0}", notexportedyet));

                    tw.WriteLine(string.Format("disconnectReasons={0}", disconnectReasons == null ? string.Empty : string.Join(",", disconnectReasons)));
                    tw.WriteLine(string.Format("dialStates={0}", dialStates == null ? string.Empty : string.Join(",", dialStates)));
                    tw.WriteLine(string.Format("agentids={0}", agentids == null ? string.Empty : string.Join(",", agentids)));
                    tw.WriteLine(string.Format("agentid={0}", agentId));
                    tw.WriteLine(string.Format("activities={0}", activities == null ? string.Empty : string.Join(",", activities)));
                    tw.WriteLine(string.Format("from={0}", From == null ? string.Empty : From.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("to={0}", To == null ? string.Empty : To.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("advancedFilter={0}", advancedFilter == null ? string.Empty : Uri.EscapeDataString(advancedFilter.InnerXml)));

                    tw.Close();
                }

                webRequestStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {

                    string strResponse = null;
                    using (Stream respStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            strResponse = sr.ReadToEnd();
                        }
                    }
                    response.Close();
                    if (strResponse != null)
                        return Int32.Parse(strResponse);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return -1;
        }
        public int[] ImportData(string file, char separator, char stringDelimiter, bool firstRowContainsHeader, DataTable table, List<string> fields, string importTag, string activityid, int maxdialAttemps, bool removeNonNumeric, string prefixToRemove, string prefixToAdd, IEnumerable<string> preferredAgents, Nixxis.Client.Admin.progressReportDelegate progress, string srcNumberFormat, string decimalSeparator)
        {
            string newFileName = string.Concat(DateTime.Now.ToString("yyyyMMddHHmmss"), "-", Path.GetFileName(file));
            AdminCore.UploadFile(file, string.Concat(Core.AdminUploadUri, AdminCore.PathImportExports, @"/", newFileName), ((a, b) => { if (progress != null) progress(a, TranslationContext.Default.Translate("Uploading file..."), b); }));

            if (progress != null)
                progress(-1, TranslationContext.Default.Translate( "Processing file..."), string.Empty);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=importData",Core.AdminUri));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.Timeout = 15 * 60000;
            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    tw.WriteLine(string.Format("user={0}", Core.m_CreatorId));
                    tw.WriteLine(string.Concat("file=", Uri.EscapeDataString( string.Concat( AdminCore.PathUpload, @"/", AdminCore.PathImportExports, @"/", newFileName))) );
                    tw.WriteLine(string.Format("separator={0}", separator));
                    tw.WriteLine(string.Format("decimalSeparator={0}", decimalSeparator));
                    tw.WriteLine(string.Format("stringDelimiter={0}", stringDelimiter));
                    tw.WriteLine(string.Format("firstRowContainsHeader={0}", firstRowContainsHeader));
                    tw.WriteLine(string.Format("campaignId={0}", Id));
                    tw.WriteLine(string.Format("srcNumberFormat={0}", srcNumberFormat));
                    tw.WriteLine(string.Format("dstNumberFormat={0}", NumberFormat.TargetId));
                       

                    StringBuilder sb = new StringBuilder();
                    using (XmlWriter xw = XmlWriter.Create(sb))
                    {
                        table.WriteXmlSchema(xw);
                        tw.WriteLine(string.Format("table={0}", sb.ToString().Replace('\n', (char)0).Replace('\r', (char)0)));
                    }
                        
                    tw.WriteLine(string.Format("fields={0}", fields == null ? string.Empty : string.Join(",", fields)));
                    tw.WriteLine(string.Format("importTag={0}", importTag == null ? string.Empty : Uri.EscapeDataString( importTag)));
                    tw.WriteLine(string.Format("activityId={0}", activityid));
                    tw.WriteLine(string.Format("maxDialAttempts={0}", maxdialAttemps));
                    tw.WriteLine(string.Format("removeNonNumeric={0}", removeNonNumeric));
                    tw.WriteLine(string.Format("prefixToRemove={0}", prefixToRemove));
                    tw.WriteLine(string.Format("prefixToAdd={0}", prefixToAdd));
                    tw.WriteLine(string.Format("preferredAgents={0}", preferredAgents == null ? string.Empty: string.Join(",", preferredAgents)));

                    tw.Close();
                }

                webRequestStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    webRequest.Timeout = 720000;
                    string strResponse = null;
                    using (Stream respStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            string strProgress = null;
                            while ((strProgress = sr.ReadLine()) != null)
                            {
                                // strProgress in format percent,description                                    
                                string[] splitResult = strProgress.Split(new string[] { "," }, 3, StringSplitOptions.None);
                                if (progress != null)
                                {
                                    if (splitResult.Length == 3)
                                    {
                                        progress(Int32.Parse(splitResult[0]), splitResult[1], splitResult[2]);
                                    }
                                }
                                strResponse = splitResult[0];
                            }
                        }
                    }
                    if (strResponse != null)
                    {
                        string[] strResp = strResponse.Split(';');
                        return new int[]{ Int32.Parse(strResp[0]), Int32.Parse(strResp[1]) };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
            return new int[] { -1, -1};
        }
        public string PreviewPhoneNumbers(string file, long bytes, char separator, char stringDelimiter, bool firstRowContainsHeader, DataTable table,  bool removeNonNumeric, string prefixToRemove, string prefixToAdd, string dstNumberFormat, string decimalSeparator)
        {
            string newFileName = string.Concat(DateTime.Now.ToString("yyyyMMddHHmmss"), "-", Path.GetFileName(file));
                
            AdminCore.UploadFile(file, string.Concat(Core.AdminUploadUri, AdminCore.PathImportExports, @"/", newFileName), bytes);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=previewPhoneNumbers", Core.AdminUri));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.Timeout = 15 * 60000;
            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    tw.WriteLine(string.Concat("file=", Uri.EscapeDataString(string.Concat(AdminCore.PathUpload, @"/", AdminCore.PathImportExports, @"/", newFileName))));
                    tw.WriteLine(string.Format("separator={0}", separator));
                    tw.WriteLine(string.Format("decimalSeparator={0}", decimalSeparator));
                    tw.WriteLine(string.Format("stringDelimiter={0}", stringDelimiter));
                    tw.WriteLine(string.Format("firstRowContainsHeader={0}", firstRowContainsHeader));
                    tw.WriteLine(string.Format("srcNumberFormat={0}", dstNumberFormat));


                    StringBuilder sb = new StringBuilder();
                    using (XmlWriter xw = XmlWriter.Create(sb))
                    {
                        table.WriteXmlSchema(xw);
                        tw.WriteLine(string.Format("table={0}", sb.ToString().Replace('\n', (char)0).Replace('\r', (char)0)));
                    }

                    tw.WriteLine(string.Format("removeNonNumeric={0}", removeNonNumeric));
                    tw.WriteLine(string.Format("prefixToRemove={0}", prefixToRemove));
                    tw.WriteLine(string.Format("prefixToAdd={0}", prefixToAdd));
                    tw.WriteLine(string.Format("dstNumberFormat={0}",  NumberFormat.TargetId ));

                    tw.Close();
                }

                webRequestStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    webRequest.Timeout = 720000;
                    using (Stream respStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
            return "";
        }
        public int ExportData(string file, char separator, char stringDelimiter, bool firstRowContainsHeader, List<int> exportablefields, string[] qualifications, bool negativeonly, bool excludedonly, bool exportableonly, int batchnumber, string tag, int exportnumber, bool notexportedyet, IEnumerable<DialDisconnectionReason> disconnectReasons, IEnumerable<QualificationAction> dialStates, IEnumerable<string> agentids, IEnumerable<string> activities, DateTime? From, DateTime? To, XmlDocument advancedFilter, Nixxis.Client.Admin.progressReportDelegate progress, string decimalSeparator)
        {
            System.Diagnostics.Trace.WriteLine("Exporting!");
            System.Diagnostics.Trace.WriteLine(string.Format("Export request {0}", file),"Export");


            string newFileName = string.Concat(DateTime.Now.ToString("yyyyMMddHHmmss"), "-", Path.GetFileName(file));
            System.Diagnostics.Trace.WriteLine(string.Format("newFileName = {0}", newFileName), "Export");
            if(progress!=null)
                progress(-1, TranslationContext.Default.Translate("Generating export file..."), string.Empty);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=exportData", Core.AdminUri));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.Timeout = 15 * 60000;
            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    tw.WriteLine(string.Concat("file=", Uri.EscapeDataString(string.Concat( AdminCore.PathUpload, @"/", AdminCore.PathImportExports, @"/", newFileName) )));
                    tw.WriteLine(string.Format("separator={0}", separator));
                    tw.WriteLine(string.Format("decimalSeparator={0}", decimalSeparator));
                    tw.WriteLine(string.Format("stringDelimiter={0}", stringDelimiter));
                    tw.WriteLine(string.Format("firstRowContainsHeader={0}", firstRowContainsHeader));
                    tw.WriteLine(string.Format("campaignId={0}", Id));
                    tw.WriteLine(string.Format("exportableFields={0}", exportablefields == null ? string.Empty : string.Join(",", exportablefields)));
                    tw.WriteLine(string.Format("qualifications={0}", qualifications == null ? string.Empty : string.Join(",", qualifications)));
                    tw.WriteLine(string.Format("negativeonly={0}", negativeonly));
                    tw.WriteLine(string.Format("excludedonly={0}", excludedonly));
                    tw.WriteLine(string.Format("exportableonly={0}", exportableonly));
                    tw.WriteLine(string.Format("batchnumber={0}", batchnumber));
                    tw.WriteLine(string.Format("tag={0}", string.IsNullOrEmpty(tag) ? null : Uri.EscapeDataString(tag)));
                    tw.WriteLine(string.Format("exportnumber={0}", exportnumber));
                    tw.WriteLine(string.Format("notexportedyet={0}", notexportedyet));

                    tw.WriteLine(string.Format("disconnectReasons={0}", disconnectReasons == null ? string.Empty : string.Join(",", disconnectReasons)));
                    tw.WriteLine(string.Format("dialStates={0}", dialStates == null ? string.Empty : string.Join(",", dialStates)));
                    tw.WriteLine(string.Format("agentids={0}", agentids == null ? string.Empty : string.Join(",", agentids)));
                    tw.WriteLine(string.Format("activities={0}", activities == null ? string.Empty : string.Join(",", activities)));
                    tw.WriteLine(string.Format("from={0}", From == null ? string.Empty : From.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("to={0}", To == null ? string.Empty : To.GetValueOrDefault().ToBinary().ToString()));
                    tw.WriteLine(string.Format("advancedFilter={0}", advancedFilter == null ? string.Empty : Uri.EscapeDataString(advancedFilter.InnerXml)));

                    tw.Close();
                }

                webRequestStream.Close();
            }

            try
            {
                System.Diagnostics.Trace.WriteLine("Request prepared, will request response", "Export");
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    webRequest.Timeout = 720000;

                    string strResponse = null;
                    using (Stream respStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            System.Diagnostics.Trace.WriteLine("Reading response", "Export");
                            string strProgress = null;
                            while ((strProgress = sr.ReadLine()) != null)
                            {
                                    
                                // strProgress in format percent,description                                    
                                string[] splitResult = strProgress.Split(new string[] { "," }, 3, StringSplitOptions.None);
                                if (progress != null)
                                {
                                    System.Diagnostics.Trace.WriteLine(string.Format("Progress={0}", strProgress), "Export");
                                    if (splitResult.Length == 3)
                                        progress(Int32.Parse(splitResult[0]), splitResult[1], splitResult[2]);
                                }
                                strResponse = splitResult[0];
                            }
                        }
                    }

                    System.Diagnostics.Trace.WriteLine(string.Format("strResponse = {0}", strResponse), "Export");
                    if (strResponse != null)
                    {
                        if(progress!=null)
                            progress(-1, TranslationContext.Default.Translate("Downloading resulting file..."), string.Empty);

                        HttpWebRequest webGetRequest = (HttpWebRequest)WebRequest.Create(string.Concat(Core.AdminUploadUri, AdminCore.PathImportExports, @"/", newFileName));
                        webGetRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                        webRequest.Method = WebRequestMethods.Http.Get;
                        using (HttpWebResponse getResponse = (HttpWebResponse)webGetRequest.GetResponse())
                        {
                            long totalLength = 0;
                            long currentProgress = 0;
                            int lastProgress = 0;
                            DateTime lastProgressTime = DateTime.Now;
                            string progressFormat = string.Empty;
                            double multiplicator = 1;

                            if(progress!=null)
                                long.TryParse(getResponse.Headers["x-cr-length"], out totalLength);

                            if (totalLength > 1024*1024)
                            {
                                progressFormat = TranslationContext.Default.Translate("{0:N}/{1:N} Mb");
                                multiplicator = 1024*1024;

                            }
                            else if (totalLength > 1024)
                            {
                                progressFormat = TranslationContext.Default.Translate("{0:N}/{1:N} Kb");
                                multiplicator = 1024;
                            }
                            else
                            {
                                progressFormat = TranslationContext.Default.Translate("{0}/{1} bytes");
                            }


                            using (Stream respStream = getResponse.GetResponseStream())
                            {
                                using (StreamReader sr = new StreamReader(respStream))
                                {
                                    using (Stream writes = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.Write))
                                    {
                                        byte[] buffer = new byte[1024];
                                        int count = 0;
                                        while ( (count = respStream.Read(buffer, 0, buffer.Length) ) > 0 )
                                        {
                                            writes.Write(buffer, 0, count);
                                            if(progress!=null)
                                            {
                                                currentProgress += count;
                                                long temp = currentProgress * 100 / totalLength;
                                                if (temp > lastProgress || DateTime.Now.Subtract(lastProgressTime).TotalMilliseconds>2000)
                                                {
                                                    lastProgress = (int)temp;
                                                    lastProgressTime = DateTime.Now;
                                                    progress(lastProgress, TranslationContext.Default.Translate("Downloading resulting file..."), string.Format(progressFormat, currentProgress/multiplicator, totalLength/multiplicator));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return Int32.Parse(strResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return -1;
        }
        public void MaintainDatabase(bool reorganizeIndexes, bool updateStatistics, Nixxis.Client.Admin.progressReportDelegate progress)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?action=maintainDatabase", Core.AdminUri));
            webRequest.Headers.Add("Accept-Language", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.Timeout = 15 * 60000;
            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    tw.WriteLine(string.Format("user={0}", Core.m_CreatorId));
                    tw.WriteLine(string.Format("campaignId={0}", Id));
                    tw.WriteLine(string.Format("reorganizeIndexes={0}", reorganizeIndexes));
                    tw.WriteLine(string.Format("updateStatistics={0}", updateStatistics));
                    tw.Close();
                }

                webRequestStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    webRequest.Timeout = 720000;
                    string strResponse = null;
                    using (Stream respStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(respStream))
                        {
                            string strProgress = null;
                            while ((strProgress = sr.ReadLine()) != null)
                            {
                                // strProgress in format percent,description                                    
                                string[] splitResult = strProgress.Split(new string[] { "," }, 3, StringSplitOptions.None);
                                if (progress != null)
                                {
                                    if (splitResult.Length == 3)
                                    {
                                        progress(Int32.Parse(splitResult[0]), splitResult[1], splitResult[2]);
                                    }
                                }
                                strResponse = splitResult[0];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        public string[] DatabasesList
        {
            get
            {
                try
                {

                    string strTemp = cache.GetEntry(string.Format("{0}?action=listdatabases", Core.AdminUri), (strReq) =>
                    {
                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(strReq);
                webRequest.Method = WebRequestMethods.Http.Get;
                webRequest.Timeout = 15 * 60000;
                webRequest.AllowWriteStreamBuffering = true;

                    using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                    {
                        webRequest.Timeout = 720000;
                        using (Stream respStream = response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(respStream))
                            {

                                    return sr.ReadToEnd();
                                }
                            }
                        }
                    },
                    TimeSpan.FromSeconds(60));
                    if (strTemp != null && strTemp.Length > 2)
                    {
                        strTemp = strTemp.Substring(1, strTemp.Length - 2);
                        return strTemp.Split(new string[] { "];[" }, StringSplitOptions.RemoveEmptyEntries);
                    }

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }

                return new string[]{};
            }
        }

        public class TableCommented
        {
            public string Description
            {
                get
                {
                    if (Comment != null)
                        return string.Format("{0} ({1})", TableName, Comment);
                    else
                        return TableName;
                }               
            }
            private bool m_DirtyFlag = false;
            public bool DirtyFlag
            {
                get
                {
                    return m_DirtyFlag;
                }
                set
                {
                    m_DirtyFlag = value;
                }
            }
            public string TableName { get; set; }
            public string Comment { get; set; }
        }

        private ObservableCollection<TableCommented> m_TablesList = new ObservableCollection<TableCommented>();

        public ObservableCollection<TableCommented> TablesList
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(DatabaseName))
                        return null;

                    string strTemp = cache.GetEntry(string.Format("{0}?action=listtables&database={1}", Core.AdminUri, DatabaseName), (strReq) =>
                        {
                            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(strReq);
                webRequest.Method = WebRequestMethods.Http.Get;
                webRequest.Timeout = 15 * 60000;
                webRequest.AllowWriteStreamBuffering = true;

                    using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                    {
                        webRequest.Timeout = 720000;
                        using (Stream respStream = response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(respStream))
                            {
                                        return sr.ReadToEnd();
                                    }
                                }
                            }
                        },TimeSpan.FromSeconds(60));


                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(strTemp);

                                foreach (TableCommented tc in m_TablesList)
                                {
                                    tc.DirtyFlag = true;
                                }

                                foreach (XmlNode nde in doc.SelectNodes("Tables/Table"))
                                {
                                    TableCommented tc = null;
                                    if ((tc = m_TablesList.FirstOrDefault((a) => (a.TableName == nde.SelectSingleNode("name").InnerText))) != null)
                                    {
                                        tc.Comment = nde.SelectSingleNode("remarks") == null ? null : nde.SelectSingleNode("remarks").InnerText;
                                        tc.DirtyFlag = false;
                                    }
                                    else
                                    {
                                        m_TablesList.Add(new TableCommented() { TableName = nde.SelectSingleNode("name").InnerText, Comment = nde.SelectSingleNode("remarks") == null ? null : nde.SelectSingleNode("remarks").InnerText });
                                    }
                                }


                                for (int i = 0; i < m_TablesList.Count; i++)
                                {
                                    if (m_TablesList[i].DirtyFlag)
                                    {
                                        m_TablesList.RemoveAt(i);
                                        i--;
                                    }
                                }

                            }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }

                return m_TablesList;
            }
        }

        public string DefaultDbName
        {
            get
            {

                try
                {

                    return cache.GetEntry(string.Format("{0}?action=getcampaigndefaultdbname&campaign={1}", Core.AdminUri, this.Id), (strReq) =>
                        {
                            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(strReq);
                webRequest.Method = WebRequestMethods.Http.Get;
                webRequest.Timeout = 15 * 60000;
                webRequest.AllowWriteStreamBuffering = true;

                    using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                    {
                        webRequest.Timeout = 720000;
                        using (Stream respStream = response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(respStream))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                        }, TimeSpan.FromSeconds(60));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }
                return string.Empty;
            }
        }

        public string DefaultSystemTableName
        {
            get
            {
                return getDefaultSystemTableName(this.Id, DatabaseName);
            }
        }

        private string getDefaultSystemTableName(string id, string databaseName)
        {

            try
            {

                return cache.GetEntry(string.Format("{0}?action=getsystemtabledefaultname&campaign={1}&dbname={2}", Core.AdminUri,id, databaseName), (strReq) =>
                {
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(strReq);
                webRequest.Method = WebRequestMethods.Http.Get;
                webRequest.Timeout = 15 * 60000;
                webRequest.AllowWriteStreamBuffering = true;

                    using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                    {
                        webRequest.Timeout = 720000;
                        using (Stream respStream = response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(respStream))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                },
                    TimeSpan.FromSeconds(60));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return string.Empty;

        }


        public string DefaultQuotasTableName
        {
            get
            {
                return getDefaultQuotasTableName(this.Id, this.DatabaseName);
            }
        }

        private string getDefaultQuotasTableName(string id, string  databaseName)
        {

                try
                {
                    return cache.GetEntry(string.Format("{0}?action=getquotastabledefaultname&campaign={1}&dbname={2}", Core.AdminUri, id, databaseName), (strReq) =>
                    {
                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(strReq);
                        webRequest.Method = WebRequestMethods.Http.Get;
                        webRequest.Timeout = 15 * 60000;
                        webRequest.AllowWriteStreamBuffering = true;

                    using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                    {
                        webRequest.Timeout = 720000;
                        using (Stream respStream = response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(respStream))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                    },

                        TimeSpan.FromSeconds(60));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                }
                return string.Empty;

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

        public Campaign Duplicate( bool duplicateQualifications, bool  duplicateAttachments, bool duplicatePredefinedTexts, bool duplicateInboundActivities, bool duplicateoutboundActivities, bool duplicateDataStructure, bool copyAffectations)
        {
            Campaign newcamp = AdminCore.Create<Campaign>();

            newcamp.Description = string.Concat(Description, "*");
            newcamp.Advanced = Advanced;
            newcamp.AppointmentsContext.TargetId = AppointmentsContext.TargetId;
            newcamp.CustomConfig = CustomConfig;
            newcamp.CustomDataSource = CustomDataSource;
            newcamp.ExportFields = ExportFields;
            newcamp.GroupKey = GroupKey;
            newcamp.NumberFormat.TargetId = NumberFormat.TargetId;

            Dictionary<string, string> qualificationsMappings = new Dictionary<string, string>();

            if (duplicateQualifications)
            {
                #region Qualifications
                List<Qualification> toCopy = new List<Qualification>();

                toCopy.Add(Qualification.Target);

                Qualification newqual = toCopy[0].AdminCore.Create<Qualification>();
                newqual.Description = toCopy[0].Description;
                newqual.Action = toCopy[0].Action;
                newqual.ActionParameters = toCopy[0].ActionParameters;
                newqual.Argued = toCopy[0].Argued;
                newqual.CustomValue = toCopy[0].CustomValue;
                newqual.DisplayOrder = toCopy[0].DisplayOrder;
                newqual.Exportable = toCopy[0].Exportable;
                newqual.IgnoreMaxDialAttempts = toCopy[0].IgnoreMaxDialAttempts;
                newqual.NewActivity = toCopy[0].NewActivity;
                newqual.Positive = toCopy[0].Positive;
                newqual.PositiveUpdatable = toCopy[0].PositiveUpdatable;
                newqual.SystemMapping = toCopy[0].SystemMapping;
                newqual.TriggerHangup = toCopy[0].TriggerHangup;

                newcamp.Qualifications.Add(newqual);

                newcamp.Qualification.TargetId = newqual.Id;

                foreach (Qualification q in toCopy[0].Children)
                    toCopy.Add(q);

                qualificationsMappings.Add(toCopy[0].Id, newqual.Id);

                toCopy.RemoveAt(0);

                while (toCopy.Count > 0)
                {
                    newqual = toCopy[0].AdminCore.Create<Qualification>();
                    newqual.Description = toCopy[0].Description;
                    newqual.Action = toCopy[0].Action;
                    


                    newqual.ActionParameters = toCopy[0].ActionParameters;

                    newqual.Argued = toCopy[0].Argued;
                    newqual.CustomValue = toCopy[0].CustomValue;
                    newqual.DisplayOrder = toCopy[0].DisplayOrder;
                    newqual.Exportable = toCopy[0].Exportable;
                    newqual.IgnoreMaxDialAttempts = toCopy[0].IgnoreMaxDialAttempts;

                    newqual.NewActivity = toCopy[0].NewActivity;

                    newqual.Positive = toCopy[0].Positive;
                    newqual.PositiveUpdatable = toCopy[0].PositiveUpdatable;
                    newqual.SystemMapping = toCopy[0].SystemMapping;
                    newqual.TriggerHangup = toCopy[0].TriggerHangup;

                    newqual.ParentId = qualificationsMappings[toCopy[0].ParentId];
                    newqual.ParentQualification.Children.Add(newqual);

                    newcamp.Qualifications.Add(newqual);

                    foreach (Qualification q in toCopy[0].Children)
                        toCopy.Add(q);

                    qualificationsMappings.Add(toCopy[0].Id, newqual.Id);

                    toCopy.RemoveAt(0);
                }
                #endregion
            }

            if (duplicateAttachments)
            {
                #region Attachments
                foreach (Attachment att in Attachments)
                {
                    Attachment newatt = att.AdminCore.Create<Attachment>();
                    newatt.Description = att.Description;
                    newatt.Campaign.TargetId = newcamp.Id;
                    newatt.CompatibleMedias = att.CompatibleMedias;
                    newatt.LocalPath = att.LocalPath;
                    newatt.Location = att.Location;
                    newatt.InlineDisposition = att.InlineDisposition;
                    newatt.LocationIsLocal = att.LocationIsLocal;
                    newatt.Target = att.Target;
                    newatt.Sequence = att.Sequence;

                    newcamp.Attachments.Add(newatt);
                }
                #endregion
            }

            if (duplicatePredefinedTexts)
            {
                #region PredefinedTexts
                foreach (PredefinedText pt in PredefinedTexts)
                {
                    PredefinedText newpt = pt.AdminCore.Create<PredefinedText>();
                    newpt.Campaign.TargetId = newcamp.Id;
                    newpt.CompatibleMedias = pt.CompatibleMedias;
                    newpt.Description = pt.Description;
                    newpt.HtmlContent = pt.HtmlContent;
                    newpt.Language.TargetId = pt.Language.TargetId;
                    newpt.Sequence = pt.Sequence;
                    newpt.TextContent = pt.TextContent;

                    newcamp.PredefinedTexts.Add(newpt);
                }
                #endregion
            }

            if (duplicateInboundActivities || duplicateoutboundActivities)
            {
                // we move campaigns prompts to global level...
                while (Prompts.Count > 0)
                {
                    PromptLink pl = Prompts[0];
                    Campaign camp = ((Campaign)(pl.Link));
                    pl.Prompt.Description = string.Concat(pl.Prompt.Description, "*");
                    camp.Core.GlobalPrompts.Prompts.Add(pl.Prompt);
                    camp.Prompts.Remove(pl.Prompt);
                }
            }

            if (duplicateInboundActivities)
            {
                #region Inbound
                foreach (InboundActivity ia in InboundActivities)
                {
                    // we move activity prompts to global level
                    while (ia.Prompts.Count > 0)
                    {
                        PromptLink pl = ia.Prompts[0];
                        Activity act = ((Activity)(pl.Link));
                        pl.Prompt.Description = string.Concat(pl.Prompt.Description, "*");
                        ia.Core.GlobalPrompts.Prompts.Add(pl.Prompt);
                        ia.Prompts.Remove(pl.Prompt);
                    }


                    InboundActivity newia = ia.Duplicate(false);
                    newia.Campaign.TargetId = newcamp.Id;
                    if (ia.OwnerId != null)
                        newia.OwnerId = newcamp.Id; 
                    newcamp.InboundActivities.Add(newia);

                    if (duplicateQualifications)
                    {
                        foreach (QualificationExclusion qe in ia.QualificationsExclusions)
                        {
                            newia.QualificationsExclusions.Add(newia.AdminCore.GetAdminObject<Qualification>(qualificationsMappings[qe.QualificationId]));
                        }
                    }

                }

                #endregion
            }

            Dictionary<string, string> fieldMap = new Dictionary<string, string>();

            if (duplicateDataStructure)
            {
                #region DataStructure
                newcamp.FieldsConfig = FieldsConfig;

                foreach (UserField uf in UserFields)
                {
                    UserField newuf = uf.Duplicate(newcamp);
                    newuf.Campaign.TargetId = newcamp.Id;
                    fieldMap.Add(uf.Id, newuf.Id);
                    newcamp.UserFields.Add(newuf);
                }

                #endregion
            }

            Dictionary<string, string> outboundMap = new Dictionary<string, string>();
            if (duplicateoutboundActivities)
            {
                #region Outbound


                foreach (OutboundActivity oa in OutboundActivities)
                {
                    // we move activity prompts to global level
                    while (oa.Prompts.Count > 0)
                    {
                        PromptLink pl = oa.Prompts[0];
                        Activity act = ((Activity)(pl.Link));
                        pl.Prompt.Description = string.Concat(pl.Prompt.Description, "*");
                        oa.Core.GlobalPrompts.Prompts.Add(pl.Prompt);
                        oa.Prompts.Remove(pl.Prompt);
                    }


                    OutboundActivity newoa = oa.Duplicate(false);
                    outboundMap.Add(oa.Id, newoa.Id);


                    foreach (SortField sf in oa.SortFields)
                    {                 
                        SortField newsf = null;

                        if(sf.Field is SystemField)
                        {
                            newsf = (SortField)(newoa.SortFields.AddLinkItem(sf.Field));
                            newsf.Sequence = sf.Sequence;
                            newsf.SortOrder = sf.SortOrder;
                        }
                        else
                        {
                            if(fieldMap.ContainsKey(sf.FieldId))
                            {
                                newsf = (SortField)(newoa.SortFields.AddLinkItem( oa.AdminCore.GetAdminObject(fieldMap[sf.FieldId])));
                                newsf.Sequence = sf.Sequence;
                                newsf.SortOrder = sf.SortOrder;
                            }
                        }
                    }



                    foreach (FilterPart fp in oa.FilterParts)
                    {
                        if (fp.Field.Target is SystemField)
                        {
                            if (fp.OperandField.TargetId == null || fp.OperandField.Target is SystemField)
                            {
                                FilterPart sf = newoa.AdminCore.Create<FilterPart>(newoa);
                                sf.Field.TargetId = fp.Field.TargetId;
                                sf.Operator = fp.Operator;

                                sf.OperandField.TargetId = fp.OperandField.TargetId;
                                sf.OperandText = fp.OperandText;
                                sf.Activity.TargetId = newoa.Id;
                                sf.Sequence = fp.Sequence;

                                newoa.FilterParts.Add(sf);
                            }
                            else
                            {
                                if (fieldMap.ContainsKey(fp.OperandField.TargetId))
                                {
                                    FilterPart sf = newoa.AdminCore.Create<FilterPart>(newoa);
                                    sf.Field.TargetId = fp.Field.TargetId;
                                    sf.Operator = fp.Operator;

                                    sf.OperandField.TargetId = fieldMap[fp.OperandField.TargetId];
                                    sf.OperandText = fp.OperandText;
                                    sf.Activity.TargetId = newoa.Id;
                                    sf.Sequence = fp.Sequence;

                                    newoa.FilterParts.Add(sf);
                                }
                            }
                        }
                        else
                        {
                            if (fieldMap.ContainsKey(fp.Field.TargetId))
                            {
                                if (fp.OperandField.Target == null || fp.OperandField.Target is SystemField)
                                {
                                    FilterPart sf = newoa.AdminCore.Create<FilterPart>(newoa);
                                    sf.Field.TargetId = fieldMap[fp.Field.TargetId];
                                    sf.Operator = fp.Operator;

                                    sf.OperandField.TargetId = fp.OperandField.TargetId;
                                    sf.OperandText = fp.OperandText;
                                    sf.Activity.TargetId = newoa.Id;
                                    sf.Sequence = fp.Sequence;

                                    newoa.FilterParts.Add(sf);
                                }
                                else
                                {
                                    if (fieldMap.ContainsKey(fp.OperandField.TargetId))
                                    {
                                        FilterPart sf = newoa.AdminCore.Create<FilterPart>(newoa);
                                        sf.Field.TargetId = fieldMap[fp.Field.TargetId];
                                        sf.Operator = fp.Operator;

                                        sf.OperandField.TargetId = fieldMap[fp.OperandField.TargetId];
                                        sf.OperandText = fp.OperandText;
                                        sf.Activity.TargetId = newoa.Id;
                                        sf.Sequence = fp.Sequence;

                                        newoa.FilterParts.Add(sf);
                                    }
                                }
                            }
                        }
                    }


                    newoa.Campaign.TargetId = newcamp.Id;
                    if (oa.OwnerId != null)
                        newoa.OwnerId = newcamp.Id;

                    newcamp.OutboundActivities.Add(newoa);

                    if (duplicateQualifications)
                    {
                        foreach (QualificationExclusion qe in oa.QualificationsExclusions)
                        {
                            newoa.QualificationsExclusions.Add(newoa.AdminCore.GetAdminObject<Qualification>(qualificationsMappings[qe.QualificationId]));
                        }
                    }

                }

                #endregion
            }

            if (!newcamp.Advanced)
            {
                Team newTeam = newcamp.AdminCore.Create<Team>();
                newTeam.GroupKey = GroupKey;
                newTeam.OwnerId = newcamp.Id;
                newTeam.Description = newcamp.Description;
                newcamp.AdminCore.Teams.Add(newTeam);

                Queue q = SystemQueue.Duplicate();

                q.Teams.Remove(SystemTeam);

                q.Description = newcamp.Description;
                q.GroupKey = GroupKey;
                q.OwnerId = newcamp.Id;

                newTeam.Queues.Add(q);

                if (copyAffectations)
                {
                    foreach (AgentTeam at in SystemTeam.Agents)
                    {
                        newTeam.Agents.Add(at.Agent);
                    }
                }

                foreach (OutboundActivity oa in newcamp.OutboundActivities)
                {
                    oa.Queue.TargetId = newcamp.SystemQueue.Id;
                }
              
            }


            foreach (InboundActivity ia in newcamp.InboundActivities)
            {
                if (!newcamp.Advanced)
                {
                    ia.Queue.TargetId = newcamp.SystemQueue.Id;
                }

                if (ia.CallbackActivity.HasTarget && ia.CallbackActivity.Target != null)
                {
                    if (ia.CallbackActivity.Target.Campaign.Target != newcamp)
                    {
                        if (outboundMap.ContainsKey(ia.CallbackActivity.TargetId))
                            ia.CallbackActivity.TargetId = outboundMap[ia.CallbackActivity.TargetId];
                        else
                            ia.CallbackActivity.TargetId = null;
                    }
                }
            }

            foreach (Qualification q in newcamp.Qualifications)
            {
                if (!string.IsNullOrEmpty(q.NewActivity))
                {
                    if (outboundMap.ContainsKey(q.NewActivity))
                        q.NewActivity = outboundMap[q.NewActivity];
                    else
                        q.NewActivity = null;
                }
                if (!string.IsNullOrEmpty(q.ActionParameters))
                {
                    AdminObject ao = q.AdminCore.GetAdminObject(q.ActionParameters);
                    if (ao is OutboundActivity)
                    {
                        if (outboundMap.ContainsKey(q.ActionParameters))
                            q.ActionParameters = outboundMap[q.ActionParameters];
                        else
                            q.ActionParameters = null;
                    }
                }
            }


            newcamp.SecurityContext.TargetId = SecurityContext.TargetId;

            newcamp.AdminCore.Campaigns.Add(newcamp);

            DuplicateSecurity(newcamp);

            return newcamp;
        }

        public AdminObjectReference<SecurityContext> SecurityContext
        {
            get
            {
                return m_SecurityContext;
            }
            internal set
            {
                if (m_SecurityContext != null)
                {
                    m_SecurityContext.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_SecurityContext_PropertyChanged);
                    m_SecurityContext.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_SecurityContext_PropertyChanged);
                }

                m_SecurityContext = value;

                if (m_SecurityContext != null)
                {
                    m_SecurityContext.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_SecurityContext_PropertyChanged);
                    m_SecurityContext.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_SecurityContext_PropertyChanged);
                }

                SyncSecurityContextsFromSecurityContext();
            }
        }

        void m_SecurityContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SyncSecurityContextsFromSecurityContext();

            if (!this.Advanced && m_Loaded)
            {
                if(SystemQueue !=null)
                    SystemQueue.SecurityContext.TargetId = SecurityContext.TargetId;
                if(SystemTeam !=null)
                    SystemTeam.SecurityContext.TargetId = SecurityContext.TargetId;
            }
        }

        [AdminLoad(SkipLoad = true)]
        public SingletonAdminObjectList<AdminObjectSecurityContext> SecurityContexts
        {
            get
            {
                return m_SecurityContexts;
            }
            internal set
            {
                if (m_SecurityContexts != null)
                    m_SecurityContexts.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_SecurityContexts_CollectionChanged);

                m_SecurityContexts = value;

                if (m_SecurityContexts != null)
                    m_SecurityContexts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(m_SecurityContexts_CollectionChanged);

                SyncSecurityContextFromSecurityContexts();
            }

        }

        void m_SecurityContexts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SyncSecurityContextFromSecurityContexts();
        }

        private bool Syncing = false;

        private void SyncSecurityContextsFromSecurityContext()
        {
            lock (this)
            {
                if (SecurityContexts == null || SecurityContext == null || Syncing)
                    return;

                Syncing = true;
            }

            if (!SecurityContext.HasTarget)
            {
                if (SecurityContexts.Count == 0)
                {
                }
                else if (SecurityContexts.Count == 1)
                {
                    SecurityContexts[0].SecurityContext.SecuredAdminObjects.Remove(this);
                }
                else if (SecurityContexts.Count > 1)
                {
                    System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
                }
            }
            else
            {
                if (SecurityContexts.Count == 0)
                {
                    SecurityContexts.Add(SecurityContext.Target);
                }
                else if (SecurityContexts.Count == 1)
                {
                    if (SecurityContexts[0].SecurityContextId != SecurityContext.TargetId)
                    {
                        SecurityContexts.RemoveAt(0);
                        SecurityContexts.Add(SecurityContext.Target);
                    }
                }
                else if (SecurityContexts.Count > 1)
                {
                    System.Diagnostics.Debug.Assert(false, "Incorrect collection count");
                }
            }

            lock (this)
            {
                Syncing = false;
            }


        }

        private void SyncSecurityContextFromSecurityContexts()
        {
            lock (this)
            {
                if (SecurityContexts == null || SecurityContext == null || Syncing)
                    return;

                Syncing = true;
            }

            if (SecurityContexts.Count == 0)
            {
                if (SecurityContext.HasTarget)
                {
                    SecurityContext.TargetId = null;
                }
            }
            else if (SecurityContexts.Count == 1)
            {
                if (SecurityContext.HasTarget)
                {
                    if (SecurityContext.TargetId != SecurityContexts[0].SecurityContextId)
                        SecurityContext.TargetId = SecurityContexts[0].SecurityContextId;
                }
                else
                {
                    SecurityContext.TargetId = SecurityContexts[0].SecurityContextId;
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


        public override bool? Is_Visible(string rightid)
        {
            bool? tempResult1 = base.Is_Visible(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Is_Visible(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }

            }

            return tempResult1;
        }

        public override bool? Is_Listable(string rightid)
        {
            bool? tempResult1 = base.Is_Listable(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Is_Listable(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }

            }

            return tempResult1;
        }

        internal override bool? Has_FullControlFlag(string rightid)
        {
            bool? tempResult1 = base.Has_FullControlFlag(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Has_FullControlFlag(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }

            }

            return tempResult1;
        }

        internal override bool? Has_PowerFlag(string rightid)
        {
            bool? tempResult1 = base.Has_PowerFlag(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Has_PowerFlag(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }

            }

            return tempResult1;
        }

        internal override bool? Is_Modifiable(string rightid)
        {
            bool? tempResult1 = base.Is_Modifiable(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Is_Modifiable(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }

            }

            return tempResult1;
        }

        internal override bool? Is_Deletable(string rightid)
        {
            bool? tempResult1 = base.Is_Deletable(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                // we cannot allow a campaign deletion if it would delete an activity that cannot be deleted
                foreach (Activity a in Activities)
                {
                    if (!a.IsDeletable)
                        return false;
                }


                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Is_Deletable(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }

            }

            return tempResult1;
        }

        internal override bool? Is_RighstHandlingAllowed(string rightid)
        {
            bool? tempResult1 = base.Is_RighstHandlingAllowed(rightid);
            if (tempResult1 == null || tempResult1.Value)
            {
                if (SecurityContext.HasTarget)
                {
                    bool? tempResult2 = SecurityContext.Target.Is_RighstHandlingAllowed(rightid);

                    if (tempResult2 != null && !tempResult2.Value)
                        return false;
                    else if (tempResult1 == null && tempResult2 != null)
                        return tempResult2;
                }

            }

            return tempResult1;
        }


        public void Delete(Field field)
        {
            foreach(OutboundActivity a in OutboundActivities)
            {
                for(int i=0; i< a.FilterParts.Count; i++)
                {
                    FilterPart fp = a.FilterParts[i];
                    if(fp.Field.TargetId == field.Id)
                    {
                        a.FilterParts.RemoveAt(i);
                        i--;
                    }
                }
            }
            Core.Delete(field);
        }
    }
}
