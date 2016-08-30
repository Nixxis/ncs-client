using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Nixxis.Client.Admin
{
    public interface INumberFormatEngine
    {
        string Description { get; }
        bool CanConvertTo(INumberFormatEngine format);
        bool CanConvertFrom(INumberFormatEngine format);
        string ConvertTo(string phone, INumberFormatEngine format);
        string ConvertFrom(string phone, INumberFormatEngine format);
    }

    public class RegexNumberFormatEngine: INumberFormatEngine
    {
        private string m_Description;
        private System.Text.RegularExpressions.Regex m_IsConvertible = null;
        private List<System.Text.RegularExpressions.Regex> m_LocalMatches = new List<System.Text.RegularExpressions.Regex>();
        private List<string> m_LocalReplaces = new List<string>();
        private List<System.Text.RegularExpressions.Regex> m_GlobalMatches = new List<System.Text.RegularExpressions.Regex>();
        private List<string> m_GlobalReplaces = new List<string>();

        public RegexNumberFormatEngine(string description, string isConvertibleExpression, string[] localMatchAndReplaceExpressions, string[] globalMatchAndReplaceExpressions)
        {
            m_Description = description;
            m_IsConvertible = new System.Text.RegularExpressions.Regex(isConvertibleExpression);
            foreach (string str in localMatchAndReplaceExpressions)
            {
                string[] strSplit = str.Split(';');
                m_LocalMatches.Add(new System.Text.RegularExpressions.Regex(strSplit[0]));
                m_LocalReplaces.Add(strSplit[1]);
            }
            foreach (string str in globalMatchAndReplaceExpressions)
            {
                string[] strSplit = str.Split(';');
                m_GlobalMatches.Add(new System.Text.RegularExpressions.Regex(strSplit[0]));
                m_GlobalReplaces.Add(strSplit[1]);
            }
        }

        public string ConvertTo(string phone, INumberFormatEngine format)
        {
            if (phone == null)
                return null;

            if (format == this || format is NeutralFormatEngine)
                return phone;

            if (format is GlobalFormatEngine)
            {
                if (m_IsConvertible.IsMatch(phone))
                {
                    for (int i = 0; i < m_LocalMatches.Count; i++)
                    {
                        if (m_LocalMatches[i].IsMatch(phone))
                            return m_LocalMatches[i].Replace(phone, m_LocalReplaces[i]);
                    }
                }
                return null;
            }

            if (format.CanConvertFrom(GlobalFormatEngine.Engine))
                return format.ConvertFrom(ConvertTo(phone, GlobalFormatEngine.Engine), GlobalFormatEngine.Engine); 

            return null;
        }

        public string ConvertFrom(string phone, INumberFormatEngine format)
        {
            if (phone == null)
                return null;

            if (format == this || format is NeutralFormatEngine)
                return phone;

            if (format is GlobalFormatEngine)
            {
                for (int i = 0; i < m_GlobalMatches.Count; i++)
                {
                    if (m_GlobalMatches[i].IsMatch(phone))
                        return m_GlobalMatches[i].Replace(phone, m_GlobalReplaces[i]);
                }
                return null;
            }

            if (format.CanConvertTo(GlobalFormatEngine.Engine))
                return ConvertFrom(format.ConvertTo(phone, GlobalFormatEngine.Engine), GlobalFormatEngine.Engine);
            
            return null;
        }

        public bool CanConvertTo(INumberFormatEngine format)
        {
            return format == this || format is NeutralFormatEngine || format is GlobalFormatEngine || format.CanConvertFrom(GlobalFormatEngine.Engine);
        }

        public bool CanConvertFrom(INumberFormatEngine format)
        {
            return format == this || format is NeutralFormatEngine || format is GlobalFormatEngine || format.CanConvertTo(GlobalFormatEngine.Engine);
        }

        public string Description
        {
            get
            {
                return m_Description;
            }
        }
    }

    public class BaseNumberFormatEngine : RegexNumberFormatEngine
    {
        public BaseNumberFormatEngine(string description, string idd, string countryCode, string trunkPrefix)
            : base(
                description,
                "^" + trunkPrefix + "[0123456789]*$|^\\+[0123456789]*$|^" + idd + "[0123456789]*$",
                new string[] { "^" + idd + "([0123456789]*)$;+$1", "^" + trunkPrefix + "([0123456789]*)$;+" + countryCode + "$1", "^\\+([0123456789]*)$;+$1" },
                new string[] { "^\\+" + countryCode + "([0123456789]*)$;" + trunkPrefix + "$1", "^\\+([0123456789]*)$;" + idd + "$1" })
        {
        }
    }

    public class NeutralFormatEngine : INumberFormatEngine
    {
        private static NeutralFormatEngine m_Engine = new NeutralFormatEngine();
        public static NeutralFormatEngine Engine
        {
            get
            {
                return m_Engine;
            }
        }

        public bool CanConvertTo(INumberFormatEngine format)
        {
            return true;
        }

        public bool CanConvertFrom(INumberFormatEngine format)
        {
            return true;
        }

        public string ConvertTo(string phone, INumberFormatEngine format)
        {
            return phone;
        }

        public string ConvertFrom(string phone, INumberFormatEngine format)
        {
            return phone;
        }

        public string Description
        {
            get
            {
                return "Neutral";
            }
        }
    }

    public class GlobalFormatEngine : INumberFormatEngine
    {
        private static GlobalFormatEngine m_Engine = new GlobalFormatEngine();
        public static GlobalFormatEngine Engine
        {
            get
            {
                return m_Engine;
            }
        }

        public bool CanConvertTo(INumberFormatEngine format)
        {
            return (format is GlobalFormatEngine || format is NeutralFormatEngine || format.CanConvertFrom(GlobalFormatEngine.Engine));
        }

        public bool CanConvertFrom(INumberFormatEngine format)
        {
            return (format is GlobalFormatEngine || format is NeutralFormatEngine || format.CanConvertTo(GlobalFormatEngine.Engine));
        }

        public string ConvertTo(string phone, INumberFormatEngine format)
        {
            if (format is GlobalFormatEngine || format is NeutralFormatEngine)
                return phone;

            if (format.CanConvertFrom(GlobalFormatEngine.Engine))
                return format.ConvertFrom(ConvertTo(phone, GlobalFormatEngine.Engine), GlobalFormatEngine.Engine);

            return null;
        }

        public string ConvertFrom(string phone, INumberFormatEngine format)
        {
            if (format is GlobalFormatEngine || format is NeutralFormatEngine)
                return phone;

            if (format.CanConvertTo(GlobalFormatEngine.Engine))
                return ConvertFrom(format.ConvertTo(phone, GlobalFormatEngine.Engine), GlobalFormatEngine.Engine);

            return null;
        }

        public string Description
        {
            get
            {
                return "Global";
            }
        }

    }


}
