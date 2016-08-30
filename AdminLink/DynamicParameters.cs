using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixxis.Client.Admin
{

    public enum DynamicParameterType
    {
        Unknown,
        Numeric,
        Boolean,
        Text,
        Prompt,
        Queue
    }

    public class DynamicParameterDefinition : AdminObject
    {

        public DynamicParameterDefinition(AdminCore core)
            : base(core)
        {
            Init();
        }

        public DynamicParameterDefinition(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public void Init()
        {
        }

        public override string DisplayText
        {
            get
            {
                return Name;
            }
        }

        public string Name
        {
            get
            {
                return GetFieldValue<string>("Name");
            }
            set
            {
                SetFieldValue("Name", value);
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
                SetFieldValue("Description", value);
            }
        }
        public DynamicParameterType Type
        {
            get
            {
                return GetFieldValue<DynamicParameterType>("Type");
            }
            set
            {
                SetFieldValue("Type", value);
            }
        }

        [AdminLoad(Path = "/Admin/DynamicParameterLinks/DynamicParameterLink[@dynamicparameterid=\"{0}\"]")]
        public AdminObjectList<DynamicParameterLink> Links
        {
            get;
            internal set;
        }
    }


    public enum DynamicParameterAction
    {
        None = 0,
        Initialize,
        Overwrite,
        Clear
    }

    [AdminObjectLinkCascadeAttribute(typeof(DynamicParameterDefinition), "Links")]
    [AdminObjectLinkCascadeAttribute(typeof(AdminObject), "DynamicParameters")]
    public class DynamicParameterLink : AdminObjectLink<AdminObject, DynamicParameterDefinition>
    {
        private AdminObjectReference<Queue> m_Queue;
        private AdminObjectReference<Prompt> m_Prompt;

        public DynamicParameterLink(AdminObject parent)
            : base(parent)
        {
            Action = DynamicParameterAction.Initialize;
            Queue = new AdminObjectReference<Queue>(this);
            Prompt = new AdminObjectReference<Prompt>(this);
        }

        public string DynamicParameterId
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

        public DynamicParameterDefinition DynamicParameter
        {
            get
            {
                if (m_Core == null)
                    return null;
                return (DynamicParameterDefinition)(m_Core.GetAdminObject(Id2));
            }
        }

        public AdminObject Link
        {
            get { return m_Core.GetAdminObject(Id1); }
        }

        public string Value
        {
            get
            {
                return GetFieldValue<string>("Value");
            }
            set
            {
                SetFieldValue<string>("Value", value);
                if (DynamicParameter.Type == DynamicParameterType.Queue)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        if(Queue.TargetId!=null)
                            Queue.TargetId = null;
                    }
                    else
                    {
                        if(Queue.TargetId != value)
                            Queue.TargetId = value;
                    }
                }
                if (DynamicParameter.Type == DynamicParameterType.Prompt)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        if (Prompt.TargetId != null)
                            Prompt.TargetId = null;
                    }
                    else
                    {
                        if (Prompt.TargetId != value)
                            Prompt.TargetId = value;
                    }
                }
            }
        }

        [AdminSave(SkipSave = true)]
        public AdminObjectReference<Queue> Queue
        {
            get
            {
                return m_Queue;
            }
            set
            {
                if (m_Queue != null)
                {
                    m_Queue.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Queue_PropertyChanged);
                    m_Queue.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Queue_PropertyChanged);
                }

                m_Queue = value;

                if (m_Queue != null)
                {
                    m_Queue.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Queue_PropertyChanged);
                    m_Queue.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Queue_PropertyChanged);
                }
            }
        }

        void m_Queue_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Value != Queue.TargetId)
            {
                Value = Queue.TargetId;
            }
        }

        [AdminSave(SkipSave = true)]
        public AdminObjectReference<Prompt> Prompt
        {
            get
            {
                return m_Prompt;
            }
            set
            {
                if (m_Prompt != null)
                {
                    m_Prompt.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Prompt_PropertyChanged);
                    m_Prompt.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_Prompt_PropertyChanged);
                }

                m_Prompt = value;

                if (m_Prompt != null)
                {
                    m_Prompt.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Prompt_PropertyChanged);
                    m_Prompt.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Prompt_PropertyChanged);
                }
            }
        }

        void m_Prompt_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Value != Prompt.TargetId)
            {
                Value = Prompt.TargetId;
            }
        }


        public DynamicParameterAction Action
        {
            get
            {
                return GetFieldValue<DynamicParameterAction>("Action");
            }
            set
            {
                SetFieldValue<DynamicParameterAction>("Action", value);
            }
        }

    }


}
