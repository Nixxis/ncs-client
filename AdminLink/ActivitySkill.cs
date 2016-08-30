using System;
using ContactRoute;

namespace Nixxis.Client.Admin
{
    [AdminObjectLinkCascadeAttribute(typeof(Activity), "Skills")]
    [AdminObjectLinkCascadeAttribute(typeof(Skill), "Activities")]
    public class ActivitySkill : AdminObjectLink<Activity, Skill>
    {
        private DoubleRange m_LevelRange;

        public ActivitySkill(AdminObject parent)
            : base(parent)
        {
            LevelRange = new DoubleRange(0, 100);
        }

        public string ActivityId
        {
            get
            {
                return Id1;
            }
        }

        public string SkillId
        {
            get
            {
                return Id2;
            }
        }

        public float Level
        {
            get { return GetFieldValue<float>("Level"); }
            set 
            { 
                SetFieldValue<float>("Level", value);
                m_LevelRange.End = value;
            }
        }

        public float MinimumLevel
        {
            get { return GetFieldValue<float>("MinimumLevel"); }
            set 
            { 
                SetFieldValue<float>("MinimumLevel", value); 
                m_LevelRange.Start = value;
            }
        }

        public DoubleRange LevelRange
        {
            get
            {
                return m_LevelRange;
            }
            internal set
            {
                if (m_LevelRange != null)
                {
                    m_LevelRange.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(m_LevelRange_PropertyChanged);
                }

                m_LevelRange = value;

                SetFieldValue<float>("MinimumLevel", (float)m_LevelRange.Start);
                SetFieldValue<float>("Level", (float)m_LevelRange.End);

                if (m_LevelRange != null)
                {
                    m_LevelRange.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_LevelRange_PropertyChanged);
                }
            }
        }

        void m_LevelRange_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetFieldValue<float>("MinimumLevel", (float)((DoubleRange)sender).Start);
            SetFieldValue<float>("Level", (float)((DoubleRange)sender).End);
            FirePropertyChanged("LevelRange");
        }

        public Activity Activity
        {
            get
            {
                if (m_Core == null)
                    return null;

                return (Activity)(m_Core.GetAdminObject(Id1));
            }
        }

        public Skill Skill
        {
            get 
            {
                if (m_Core == null)
                    return null;

                return (Skill)(m_Core.GetAdminObject(Id2)); 
            }
        }

    }
}