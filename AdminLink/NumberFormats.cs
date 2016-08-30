using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nixxis;
using System.Security.Cryptography;

namespace Nixxis.Client.Admin
{

    public class NumberFormat : AdminObject
    {
        private INumberFormatEngine m_Engine = null;

        public NumberFormat(AdminCore core)
            : base(core)
        {
            Init();
        }

        public NumberFormat(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        private void Init()
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
                SetFieldValue<string>("GroupKey", null);
            }
        }

        public string InternationalDirectDialing
        {
            get
            {
                return GetFieldValue<string>("InternationalDirectDialing");
            }
            set
            {
                SetFieldValue<string>("InternationalDirectDialing", value);
            }            
        }

        public string CountryCode
        {
            get
            {
                return GetFieldValue<string>("CountryCode");
            }
            set
            {
                SetFieldValue<string>("CountryCode", value);
            }
        }

        public string TrunkPrefix
        {
            get
            {
                return GetFieldValue<string>("TrunkPrefix");
            }
            set
            {
                SetFieldValue<string>("TrunkPrefix", value);
            }
        }

        public string HandlerType
        {
            get
            {
                return GetFieldValue<string>("HandlerType");
            }
            set
            {
                SetFieldValue<string>("HandlerType", value);
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
                    return string.Concat("Format ", Description);
            }
        }

        public INumberFormatEngine Engine
        {
            get
            {
                if (m_Engine == null)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(HandlerType))
                            m_Engine = Activator.CreateInstance(Type.GetType(HandlerType)) as INumberFormatEngine;
                        else
                            m_Engine = new BaseNumberFormatEngine(Description, InternationalDirectDialing, CountryCode, TrunkPrefix);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(ex.ToString());
                    }
                }
                return m_Engine;
            }
        }

    }
}
