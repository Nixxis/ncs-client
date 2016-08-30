using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nixxis;
using System.Security.Cryptography;

namespace Nixxis.Client.Admin
{

    public class Pause : AdminObject
    {
        public Pause(AdminCore core)
            : base(core)
        {
            Init();
        }

        public Pause(AdminObject parent)
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
                if(string.IsNullOrEmpty(value))
                    SetFieldValue<string>("GroupKey", null);
                else
                    SetFieldValue<string>("GroupKey", value);
                if (!string.IsNullOrEmpty(value))
                {
                    MD5 md5 = MD5.Create();
                    md5.Initialize();
                    byte[] encoded = md5.ComputeHash(Encoding.Default.GetBytes(value));
                    GroupId = System.Convert.ToBase64String(encoded);
                }
                else
                    GroupId = null;

                m_Core.FirePropertyChanged("PauseGroups");
            }
        }

        public string GroupId
        {
            get
            {
                return GetFieldValue<string>("GroupId");
            }
            set
            {
                SetFieldValue<string>("GroupId", value);
                m_Core.FirePropertyChanged("PauseGroups");
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
                    return string.Concat("Pause ", Description);
            }
        }

        public Pause Duplicate()
        {
            Pause newp = AdminCore.Create<Pause>();
            newp.Description = string.Concat(Description, "*");
            newp.GroupKey = GroupKey;
            newp.GroupId = GroupId;
            newp.AdminCore.Pauses.Add(newp);
            return newp;
        }
    }
}
