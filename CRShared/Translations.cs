using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
namespace Nixxis
{
    public class TranslationContext
    {
        private static TranslationContext m_TranslationContext = new TranslationContext();

        public static TranslationContext Default
        {
            get
            {
                return m_TranslationContext;
            }
        }

        private static SortedList<string, string> m_TranslationsCollection = new SortedList<string, string>();

        public static void LoadTranslations(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string line = null;
                string[] tempArray = null;
                while ((line = reader.ReadLine()) != null)
                {
                    tempArray = line.Split(',');

                    System.Diagnostics.Debug.Assert(tempArray.Length == 3);
                    string strTemp = string.Concat(Microsoft.JScript.GlobalObject.unescape(tempArray[0]), Microsoft.JScript.GlobalObject.unescape(tempArray[1]));
                    m_TranslationsCollection.Add(strTemp, Microsoft.JScript.GlobalObject.unescape(tempArray[2]));

                }
            }
        }

        public static void LoadTranslations(string str)
        {
            try
            {
                using (StringReader reader = new StringReader(str))
                {

                    string line = null;
                    string[] tempArray = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        tempArray = line.Split(',');

                        System.Diagnostics.Debug.Assert(tempArray.Length == 3);
                        string strTemp = string.Concat(Microsoft.JScript.GlobalObject.unescape(tempArray[0]), Microsoft.JScript.GlobalObject.unescape(tempArray[1]));
                        m_TranslationsCollection.Add(strTemp, Microsoft.JScript.GlobalObject.unescape(tempArray[2]));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        public string Context { get; set; }

        public TranslationContext()
        {
        }
        public TranslationContext(string context)
        {
            Context = context;
        }

        private static DataSet m_Translations = new DataSet();



        public string Translate(string value)
        {
            if (value == null)
                return value;

            string retValue = value.Replace("\n", "\\n").Replace("\t", "\\t").Replace("\"", "\\\"");
            string key = string.Concat(Context, retValue);
            if (m_TranslationsCollection.ContainsKey(key))
            {
                retValue = m_TranslationsCollection[key];
                retValue = retValue.Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\\"", "\"");
                return retValue;
            }
            return value;

        }
    }
}