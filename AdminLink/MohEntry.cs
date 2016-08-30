using System.Linq;
using System;
using Nixxis;
namespace Nixxis.Client.Admin
{

    public class MohEntry : AdminObject
    {
        private static TranslationContext m_TranslationContext = new TranslationContext("MohEntry");

        protected static string Translate(string text)
        {
            return m_TranslationContext.Translate(text);
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

        public MohEntry(AdminObject parent)
            : base(parent)
        {
            Init();
        }

        public MohEntry(AdminCore core)
            : base(core)
        {
            Init();
        }

        private void Init()
        {
        }

    }

}