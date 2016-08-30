using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ContactRoute.Config;

namespace ContactRoute.Recording.Config
{
    public class RecordingService : BaseModel
    {

        #region Constructor

        public RecordingService()
        {
            m_Name = "Recording config";
            m_Description = "For system and default user settings of the recording tool";
            m_Control = new ucConfig();
        }

        #endregion

        #region Members

        public override void LoadProfile()
        {
            m_Profile = new Profile();
            m_Profile.ConfigMode = m_ConfigMode;
            m_Profile.UserModeId = m_UserModeId;
            ((Profile)m_Profile).Location = m_Location;
            ((Profile)m_Profile).ConfigFile = m_ConfigFile;
            m_Profile.Load();
        }

        public override void Save()
        {
            if(m_Control != null)
            {
                m_Profile.GetProfileData(m_Control);
                ((Profile)m_Profile).Data = (ProfileData)m_Control.GetProfileData(((Profile)m_Profile).Data);
                m_Profile.Save();
            }
        }

        #endregion


    }
}
