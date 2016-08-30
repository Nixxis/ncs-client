using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ContactRoute.Config
{
    //Should be abstract but a problem in the framework doesn't allow the designer to use it.
    public class baseUcConfig : UserControl
    {
        protected ConfigMode m_ConfigMode = ConfigMode.System;

        public ConfigMode ConfigMode
        {
            get { return m_ConfigMode; }
            set { m_ConfigMode = value; SetConfigMode(); }
        }

        public virtual void SetProfileData(object data) { throw new NotImplementedException("This is an abastract class. There for you need to override this class."); }
        public virtual object GetProfileData(object data) { throw new NotImplementedException("This is an abastract class. There for you need to override this class."); }

        public virtual void SetConfigMode() { throw new NotImplementedException("This is an abastract class. There for you need to override this class."); }
    }
}
