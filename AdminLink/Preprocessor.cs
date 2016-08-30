using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Linq;
using ContactRoute;
using System.Collections.Specialized;
namespace Nixxis.Client.Admin
{
    public class Preprocessor : AdminObject
    {
        protected override void SetFieldValue<T>(string index, T value)
        {
            base.SetFieldValue<T>(index, value);
            Core.PropagatePreprocessorsChanges();
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
                    return string.Concat("Preprocessor ", Description);
            }
        }

        public string EditorUrl
        {
            get
            {
                return GetFieldValue<string>("EditorUrl");
            }
            set
            {
                SetFieldValue<string>("EditorUrl", value);
            }
        }

        public string ConfigurationAssemblyType
        {
            get
            {
                return GetFieldValue<string>("ConfigurationAssemblyType");
            }
            set
            {
                SetFieldValue<string>("ConfigurationAssemblyType", value);
            }
        }

        public string Resource
        {
            get
            {
                return GetFieldValue<string>("Resource");
            }
            set
            {
                SetFieldValue<string>("Resource", value);
            }
        }

        public bool DropAfter
        {
            get
            {
                return GetFieldValue<bool>("DropAfter");
            }
            set
            {
                SetFieldValue<bool>("DropAfter", value);
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public MediaType MediaType
        {
            get
            {
                return GetFieldValue<MediaType>("MediaType");
            }
            set
            {
                SetFieldValue<MediaType>("MediaType", value);
            }
        }

        public Preprocessor(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public Preprocessor(AdminCore core)
            : base(core)
        {
            Init();
        }

        private void Init()
        {
            DynamicParameters = new AdminObjectList<DynamicParameterLink>(this);
        }

        public Type DialogType
        {
            get
            {
                if (string.IsNullOrEmpty(EditorUrl))
                    return null;

                try
                {
                    Type tpe = ConfigType;
                    if (tpe != null)
                    {
                        foreach (Attribute att in tpe.GetCustomAttributes(typeof(DialogTypeAttribute), true))
                        {
                            return Type.GetType((att as DialogTypeAttribute).Type);
                        }
                    }
                }
                catch
                {

                }
                return null;
            }
        }

        public Type ConfigType
        {
            get
            {
                if (string.IsNullOrEmpty(EditorUrl))
                    return null;
                try
                {
                    if ("ContactRoute.IvrAdmin, IvrAdmin".Equals(EditorUrl))
                    {
                        return Type.GetType("Nixxis.Client.Admin.SimplePreprocessorConfig, AdminLink");
                    }
                    else if ("ContactRoute.CallbackIvrAdmin, IvrAdmin".Equals(EditorUrl))
                    {
                        return Type.GetType("Nixxis.Client.Admin.CallbackPreprocessorConfig, NixxisAdminControls");
                    }
                    else
                        return Type.GetType(EditorUrl);
                }
                catch
                {
                }
                return null;
            }
        }

        public Preprocessor Duplicate()
        {
            Preprocessor prep = AdminCore.Create<Preprocessor>();
            prep.ConfigurationAssemblyType = ConfigurationAssemblyType;
            prep.Description = string.Concat(Description, "*");
            prep.DropAfter = DropAfter;
            prep.EditorUrl = EditorUrl;
            prep.GroupKey = GroupKey;
            prep.MediaType = MediaType;
            prep.Resource = Resource;

            prep.AdminCore.Preprocessors.Add(prep);

            return prep;
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


    /// <summary>
    /// Base class defining the <see cref="AdminObject"/> specifying the configuration of a processor.
    /// </summary>
    public abstract class BasePreprocessorConfig: AdminObject
    {
        /// <summary>
        /// Defines the delegate called when saving the configuration.
        /// </summary>
        /// <param name="text"></param>
        public delegate void SaveToTextStorageDelegate(string text);

        /// <summary>
        /// Create a new <see cref="BasePreprocessorConfig"/> object.
        /// </summary>
        /// <param name="core">The <see cref="AdminCore"/> instance.</param>
        public BasePreprocessorConfig(AdminCore core)
            : base(core)
        {
        }

        /// <summary>
        /// Create a new <see cref="BasePreprocessorConfig"/> object as a logical children of another <see cref="AdminObject"/> object.
        /// </summary>
        /// <param name="parent">The <see cref="AdminObject"/> parent object.</param>
        public BasePreprocessorConfig(AdminObject parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Delegate allowing the user to specify the way the configuration will be stored. A call to method <see cref="Save"/> is triggering the call to this delegate.
        /// </summary>
        public SaveToTextStorageDelegate saveToTextStorage { get; set; }
        
        /// <summary>
        /// Called by the framework when loading the configuration. 
        /// </summary>
        /// <param name="text">The text containing the serialization of the configuration.</param>
        public abstract void DeserializeFromText(string text);

        /// <summary>
        /// Called by the framework when the configuration must be saved to a text storage.
        /// </summary>
        /// <returns>The text containing the serialization of the configuration.</returns>
        protected abstract string SerializeToText();

        /// <summary>
        /// Called by the framework when the configuration needs to be saved. The way the configuration is serialized is controlled by the <see cref="SerializeToText"/> method while the storage itself will be specified by <see cref="saveToTextStorage"/> delegate.
        /// </summary>
        public void Save()
        {
            if (saveToTextStorage != null)
                saveToTextStorage(SerializeToText());
            else
                if(!m_Core.IsLoading)
                    throw new NotSupportedException();
        }
    }
}