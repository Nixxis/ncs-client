using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using Nixxis;
using System.Linq;
namespace Nixxis.Client.Admin
{
    public class Setting : SecuredAdminObject
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("Settings");

        private AdminObjectList<ObjectSecurity> m_Security = null;
        private ObservableCollection<ObjectSecurity> m_RolesOverview = null;


        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
        }


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
                    return string.Concat("Setting ", Description);
            }
        }

        public Setting(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public Setting(AdminCore core)
            : base(core)
        {
            Init();
        }

        private void Init()
        {
            Script = new AdminObjectReference<Admin.Preprocessor>(this);

            DynamicParameters = new AdminObjectList<DynamicParameterLink>(this);

        }

        public bool? AutomaticRecording
        {
            get
            {
                bool? retVal = (bool?)GetFieldValue<bool?>("AutomaticRecording");
                if (retVal == null)
                    return true; // to be synchronized with the server's behavior

                return retVal;
            }
            set
            {
                SetFieldValue<bool?>("AutomaticRecording", value);
                foreach (Campaign c in m_Core.Campaigns)
                {
                    foreach (Activity a in c.Activities)
                        a.FirePropertyChanged("AutomaticRecordingDescription");
                    c.FirePropertyChanged("AutomaticRecordingDescription");
                }
            }
        }


        public string Originator
        {
            get
            {
                string retVal = GetFieldValue<string>("Originator");
                return retVal;
            }
            set
            {
                SetFieldValue<string>("Originator", value);
            }
        }

        private AdminObjectReference<Preprocessor> m_Script;

        public AdminObjectReference<Preprocessor> Script
        {
            get
            {
                return m_Script;
            }
            internal set
            {
                if (m_Script != null)
                {
                    m_Script.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Script_TargetPropertyChanged);
                    m_Script.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Script_TargetPropertyChanged);
                }

                m_Script = value;

                if (m_Script != null)
                {
                    m_Script.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Script_TargetPropertyChanged);
                    m_Script.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Script_TargetPropertyChanged);
                }
            }
        }

        void m_Script_TargetPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        public override void Save(System.Xml.XmlDocument doc)
        {
            base.Save(doc);
        }

        public string ScriptUrl
        {
            get
            {
                return GetFieldValue<string>("ScriptUrl");
            }
            set
            {
                SetFieldValue<string>("ScriptUrl", value);
            }
        }

        #region Autoready
        public bool WrapupExtendable
        {
            get
            {
                return GetFieldValue<bool>("WrapupExtendable");
            }
            set
            {
                SetFieldValue<bool>("WrapupExtendable", value);
                FirePropertyChanged("PostWrapupBehaviorDescription");
                FirePropertyChanged("IsPostWrapupBehaviorDefined");
            }
        }

        public int WrapupTime
        {
            get
            {
                return GetFieldValue<int>("WrapupTime");
            }
            set
            {
                SetFieldValue<int>("WrapupTime", value);
                FirePropertyChanged("IsPostWrapupBehaviorDefined");
                FirePropertyChanged("PostWrapupBehaviorDescription");
            }
        }

        public bool IsWrapupTimeModified
        {
            get
            {
                return PostWrapupOptionsIncreaseWrapupDuration || PostWrapupOptionsDecreaseWrapupDuration || PostWrapupOptionsSetWrapupDuration;
            }
            set
            {
                if (value)
                {
                    PostWrapupOptionsIncreaseWrapupDuration = true;
                }
                else
                {
                    PostWrapupOptionsIncreaseWrapupDuration = false;
                    PostWrapupOptionsDecreaseWrapupDuration = false;
                    PostWrapupOptionsSetWrapupDuration = false;
                }
            }
        }

        public bool IsAutoReadyDelayModified
        {
            get
            {
                return PostWrapupOptionsIncreaseAutoReadyDelay || PostWrapupOptionsDecreaseAutoReadyDelay || PostWrapupOptionsSetAutoReadyDelay;
            }
            set
            {
                if (value)
                {
                    PostWrapupOptionsIncreaseAutoReadyDelay = true;
                }
                else
                {
                    PostWrapupOptionsIncreaseAutoReadyDelay = false;
                    PostWrapupOptionsDecreaseAutoReadyDelay = false;
                    PostWrapupOptionsSetAutoReadyDelay = false;
                }
            }
        }

        public int PostWrapupOptionsValue
        {
            get
            {
                return GetFieldValue<int>("PostWrapupOptionsValue");
            }
            set
            {
                SetFieldValue<int>("PostWrapupOptionsValue", value);
                FirePropertyChanged("IsPostWrapupBehaviorDefined");
                FirePropertyChanged("PostWrapupBehaviorDescription");

                FirePropertyChanged("PostWrapupOptionsIncreaseWrapupDuration");
                FirePropertyChanged("PostWrapupOptionsDecreaseWrapupDuration");
                FirePropertyChanged("PostWrapupOptionsSetWrapupDuration");
                FirePropertyChanged("PostWrapupOptionsIncreaseAutoReadyDelay");
                FirePropertyChanged("PostWrapupOptionsDecreaseAutoReadyDelay");
                FirePropertyChanged("PostWrapupOptionsSetAutoReadyDelay");
            }
        }

        public int InboundAutoReadyDelay
        {
            get
            {
                return GetFieldValue<int>("InboundAutoReadyDelay");
            }
            set
            {
                SetFieldValue<int>("InboundAutoReadyDelay", value);
                FirePropertyChanged("PostWrapupBehaviorDescription");
                FirePropertyChanged("IsPostWrapupBehaviorDefined");
            }
        }

        public int OutboundAutoReadyDelay
        {
            get
            {
                return GetFieldValue<int>("OutboundAutoReadyDelay");
            }
            set
            {
                SetFieldValue<int>("OutboundAutoReadyDelay", value);
                FirePropertyChanged("PostWrapupBehaviorDescription");
                FirePropertyChanged("IsPostWrapupBehaviorDefined");
            }
        }

        private bool? EvaluatePostWrapupOptions(PostWrapupOption option)
        {
            PostWrapupOption pwo = PostWrapupOptionHelper.AllowedOptions(PostWrapupOptionsValue);
            if ((pwo | option) == pwo)
            {
                return true;
            }
            pwo = PostWrapupOptionHelper.DeniedOptions(PostWrapupOptionsValue);
            if ((pwo | option) == pwo)
            {
                return false;
            }
            return null;

        }

        private void SetPostWrapupoptions(bool? val, PostWrapupOption option)
        {
            if (val.HasValue)
            {
                if (val.Value)
                {
                    PostWrapupOptionsValue = PostWrapupOptionHelper.ComputeEncodedForm(PostWrapupOptionsValue, option, PostWrapupOption.None);
                }
                else
                {
                    PostWrapupOptionsValue = PostWrapupOptionHelper.ComputeEncodedForm(PostWrapupOptionsValue, PostWrapupOption.None, option);
                }
            }
            else
            {
                PostWrapupOptionsValue = PostWrapupOptionHelper.ComputeEncodedForm(PostWrapupOptionsValue, option);
            }

        }

        public bool? PostWrapupOptionsSupAlert
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.AlertSup);
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.AlertSup);
            }
        }
        public bool? PostWrapupOptionsAlert
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.Alert);
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.Alert);
            }
        }
        public bool? PostWrapupOptionsCloseScript
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.CloseScript);
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.CloseScript);
            }
        }
        public bool? PostWrapupOptionsForceReady
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.ForceReady);
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.ForceReady);

            }
        }
        public bool PostWrapupOptionsReadyWhenScriptIsClosed
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.ReadyWhenScriptIsClosed).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.ReadyWhenScriptIsClosed);
            }
        }
        public bool PostWrapupOptionsIncreaseAutoReadyDelay
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.IncreaseAutoReadyDelay).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.IncreaseAutoReadyDelay);
            }
        }
        public bool PostWrapupOptionsDecreaseAutoReadyDelay
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.DecreaseAutoReadyDelay).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.DecreaseAutoReadyDelay);
            }
        }
        public bool PostWrapupOptionsSetAutoReadyDelay
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.SetAutoReadyDelay).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.SetAutoReadyDelay);
            }
        }
        public bool PostWrapupOptionsIncreaseWrapupDuration
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.IncreaseWrapupDuration).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.IncreaseWrapupDuration);
            }
        }
        public bool PostWrapupOptionsDecreaseWrapupDuration
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.DecreaseWrapupDuration).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.DecreaseWrapupDuration);
            }
        }
        public bool PostWrapupOptionsSetWrapupDuration
        {
            get
            {
                return EvaluatePostWrapupOptions(PostWrapupOption.SetWrapupDuration).GetValueOrDefault();
            }
            set
            {
                SetPostWrapupoptions(value, PostWrapupOption.SetWrapupDuration);
            }
        }


        #endregion


        internal override bool? Has_FullControlFlag(string rightid)
        {
            bool? tempResult = null;
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (os != null && roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.FullControl.HasValue && os.FullControl.Value)
                    {
                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }

                    if (os.FullControl.HasValue && !os.FullControl.Value)
                    {
                        // explictly denied
                        return false;
                    }

                }
            }

            return tempResult;
        }
        internal override bool? Has_PowerFlag(string rightid)
        {
            bool? tempResult = null;
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (os != null && roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.PowerUser.HasValue && os.PowerUser.Value)
                    {
                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }

                    if (os.PowerUser.HasValue && !os.PowerUser.Value)
                    {
                        // explictly denied
                        return false;
                    }

                }
            }

            return tempResult;
        }
        public override bool? Is_Visible(string rightid)
        {
            bool? tempResult = null;
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (os != null && roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.ReadAllowed.HasValue && os.ReadAllowed.Value)
                    {
                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }

                    if (os.ReadAllowed.HasValue && !os.ReadAllowed.Value)
                    {
                        // explictly denied
                        return false;
                    }

                }
            }

            return tempResult;
        }
        public override bool? Is_Listable(string rightid)
        {
            bool? tempResult = null;
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (os != null && roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.ListAllowed.HasValue && os.ListAllowed.Value)
                    {
                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }

                    if (os.ListAllowed.HasValue && !os.ListAllowed.Value)
                    {
                        // explictly denied
                        return false;
                    }

                }
            }

            return tempResult;
        }
        internal override bool? Is_Modifiable(string rightid)
        {
            bool? tempResult = null;
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (os != null && roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.WriteAllowed.HasValue && os.WriteAllowed.Value)
                    {
                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }
                    if (os.WriteAllowed.HasValue && !os.WriteAllowed.Value)
                    {
                        // explictly denied
                        return false;
                    }
                }
            }

            return tempResult;
        }
        internal override bool? Is_Deletable(string rightid)
        {
            bool? tempResult = null;
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (os != null && roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.DeleteAllowed.HasValue && os.DeleteAllowed.Value)
                    {
                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }
                    if (os.DeleteAllowed.HasValue && !os.DeleteAllowed.Value)
                    {
                        // explictly denied
                        return false;
                    }
                }
            }

            return tempResult;
        }
        internal override bool? Is_RighstHandlingAllowed(string rightid)
        {
            bool? tempResult = null;
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (os != null && roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.FullControl.HasValue && os.FullControl.Value)
                    {
                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }
                    if (os.FullControl.HasValue && !os.FullControl.Value)
                    {
                        // explictly denied
                        return false;
                    }
                }
            }

            return tempResult;
        }

        public bool? TypedComputedCreatability(string rightid)
        {
            bool? tempResult = null;
            IEnumerable<string> roleIds = m_Core.Agents[m_Core.m_CreatorId].Roles.Select((rm) => (rm.RoleId));

            // check if there is something set on this object...
            foreach (ObjectSecurity os in Security)
            {
                if (os != null && roleIds.Contains(os.RoleId) && os.RightId == rightid)
                {
                    if (os.CreateAllowed.HasValue && os.CreateAllowed.Value)
                    {
                        tempResult = true;
                    }
                    else if (m_Core.Roles[os.RoleId].NotAllowedMeansDenied)
                    {
                        // nothing set on this role
                        return false;
                    }

                    if (os.CreateAllowed.HasValue && !os.CreateAllowed.Value)
                    {
                        // explictly denied
                        return false;
                    }
                }
            }

            return tempResult;
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

    }

    public class DefaultRightsSetting : Setting
    {
        public DefaultRightsSetting(AdminCore core)
            : base(core)
        {
        }


        internal override bool? Has_FullControlFlag(string rightid)
        {
            return false;
        }
        internal override bool? Has_PowerFlag(string rightid)
        {
            return false;
        }
        public override bool? Is_Visible(string rightid)
        {
            return false;
        }
        public override bool? Is_Listable(string rightid)
        {
            return false;
        }
        internal override bool? Is_Modifiable(string rightid)
        {
            return false;
        }
        internal override bool? Is_Deletable(string rightid)
        {
            return false;
        }
        internal override bool? Is_RighstHandlingAllowed(string rightid)
        {
            return false;
        }
    }
}