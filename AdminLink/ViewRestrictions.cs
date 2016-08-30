using System;
using System.Linq;
using System.Xml;

namespace Nixxis.Client.Admin
{
    public enum ViewRestrictionTargetType
    {
        Any = 0,
        Campaign,
        Inbound,
        Outbound,
        Mail,
        Chat,
        Queue,
        Team,
        Agent,
        MyTeam,
        MyGroup
    }

    [Flags]
    public enum ViewRestrictionInformationLevel
    {
        None = 0,
        Realtime = 1,
        History = 2,
        Production = 4,
        PeriodProduction = 8,
        ContactListInfo = 16,
        PeakRealTime = 32,
        PeakHistory = 64,
        PeakProduction = 128,
        All = 0xFFFF
    }

    public class ViewRestriction : AdminObject
    {
        public int Precedence
        {
            get
            {
                return GetFieldValue<int>("Precedence");
            }
            set
            {
                SetFieldValue<int>("Precedence", value);

                FirePropertyChanged("DisplayText");
            }
        }

        public bool Allowed
        {
            get
            {
                return GetFieldValue<bool>("Allowed");
            }
            set
            {
                SetFieldValue<bool>("Allowed", value);

                FirePropertyChanged("DisplayText");

            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public ViewRestrictionInformationLevel InformationLevel
        {
            get
            {
                return GetFieldValue<ViewRestrictionInformationLevel>("InformationLevel");
            }
            set
            {
                SetFieldValue<ViewRestrictionInformationLevel>("InformationLevel", value);
            }
        }

        [ConvertOptions(ConvertFlags.EnumValueAsInt)]
        public ViewRestrictionTargetType TargetType
        {
            get
            {
                return GetFieldValue<ViewRestrictionTargetType>("TargetType");
            }
            set
            {
                ViewRestrictionTargetType oldValue = TargetType;

                SetFieldValue<ViewRestrictionTargetType>("TargetType", value);

                if(value != oldValue)
                {
                    if (Target!=null && Target.HasTarget)
                    {
                        Target.TargetId = null;
                    }
                }

                FirePropertyChanged("DisplayText");
                FirePropertyChanged("Target");
                FirePropertyChanged("TargetDescription");
            }
        }

        public bool IncludeChildren
        {
            get
            {
                return GetFieldValue<bool>("IncludeChildren");
            }
            set
            {
                SetFieldValue<bool>("IncludeChildren", value);
            }
        }

        public AdminObjectReference<AdminObject> Target
        {
            get;
            internal set;
        }


        public string TargetDescription
        {
            get
            {
                if (Target != null && Target.TargetId!=null)
                    return m_Core.GetShortDisplayText(Target.TargetId);
                return string.Empty;
            }
        }

        public ViewRestriction(AdminObject parent)
            : base(parent)
        {
            InformationLevel = ViewRestrictionInformationLevel.All;
            IncludeChildren = true;
        }


        public ViewRestriction(AdminCore core)
            : base(core)
        {
            InformationLevel = ViewRestrictionInformationLevel.All;
            IncludeChildren = true;
        }


        public bool IsRestrictedToMyTeam
        {
            get
            {
                return( Allowed && TargetType == ViewRestrictionTargetType.MyTeam && IncludeChildren && InformationLevel == ViewRestrictionInformationLevel.All);
            }
        }
        public bool IsRestrictedToMyGroup
        {
            get
            {
                return (Allowed && TargetType == ViewRestrictionTargetType.MyGroup && IncludeChildren && InformationLevel == ViewRestrictionInformationLevel.All);
            }
        }
        public bool IsNotRestricted
        {
            get
            {
                return (Allowed && TargetType == ViewRestrictionTargetType.Any && IncludeChildren && InformationLevel == ViewRestrictionInformationLevel.All);
            }
        }

        public static ViewRestriction CreateRestrictedToMyTeam(AdminObject owner)
        {
            ViewRestriction vr = owner.Core.Create<ViewRestriction>(owner);
            vr.Allowed = true;
            vr.IncludeChildren = true;
            vr.InformationLevel = ViewRestrictionInformationLevel.All;
            vr.Precedence = 0;            
            vr.TargetType = ViewRestrictionTargetType.MyTeam;
            vr.Target.TargetId = owner.Id;
            return vr;
        }
        public static ViewRestriction CreateRestrictedToMyGroup(AdminObject owner)
        {
            ViewRestriction vr = owner.Core.Create<ViewRestriction>(owner);
            vr.Allowed = true;
            vr.IncludeChildren = true;
            vr.InformationLevel = ViewRestrictionInformationLevel.All;
            vr.Precedence = 0;
            vr.TargetType = ViewRestrictionTargetType.MyGroup;
            return vr;
        }
        public static ViewRestriction CreateNotRestricted(AdminObject owner)
        {
            ViewRestriction vr = owner.Core.Create<ViewRestriction>(owner);
            vr.Allowed = true;
            vr.IncludeChildren = true;
            vr.InformationLevel = ViewRestrictionInformationLevel.All;
            vr.Precedence = 0;
            vr.TargetType = ViewRestrictionTargetType.Any;
            return vr;
        }
        public static ViewRestriction CreateDisallow(AdminObject owner)
        {
            ViewRestriction vr = owner.Core.Create<ViewRestriction>(owner);
            vr.Allowed = false;
            vr.IncludeChildren = true;
            vr.InformationLevel = ViewRestrictionInformationLevel.All;
            vr.Precedence = 0;
            vr.TargetType = ViewRestrictionTargetType.Any;
            return vr;
        }

        public override string DisplayText
        {
            get
            {
                return string.Format("Restriction n°{0}: {1} {2}", Precedence, Allowed ? "allow" : "do not allow", TargetType);
            }
        }

        public bool IsFirst
        {
            get
            {
                return ParentAgent.ViewRestrictions.Min((a) => (a.Precedence)) == Precedence;
            }
        }

        public bool IsLast
        {
            get
            {
                return ParentAgent.ViewRestrictions.Max((a) => (a.Precedence)) == Precedence;
            }
        }

        public Agent ParentAgent
        {
            get
            {
                if (Parent is Agent)
                    return (Agent)Parent;
                else 
                    return ((Agent)(((AdminObjectList<ViewRestriction>)Parent).Parent));
            }
        }

        public ViewRestriction Previous
        {
            get
            {
                if (ParentAgent == null)
                    return null;
                int prevSequence = ParentAgent.ViewRestrictions.Max((a) => (a.Precedence < Precedence ? a.Precedence : -1));
                return ParentAgent.ViewRestrictions.First((a) => (a.Precedence == prevSequence));
            }
        }

        public ViewRestriction Next
        {
            get
            {
                if (ParentAgent == null)
                    return null;
                int nextSequence = ParentAgent.ViewRestrictions.Min((a) => (a.Precedence > Precedence ? a.Precedence : int.MaxValue));
                return ParentAgent.ViewRestrictions.First((a) => (a.Precedence == nextSequence));

            }
        }

        public Agent Agent
        {
            get
            {
                if (Parent == null)
                    return null;

                if (Parent is Agent)
                    return (Agent)Parent;

                return (Agent)(((AdminObjectList<ViewRestriction>)Parent).Parent);
            }
        }

        private string m_OriginalUserId = null;
        
        [AdminSave(SkipSave=true)]
        public string UserId
        {
            get
            {
                return GetFieldValue<string>("UserId");
            }
            set
            {
                SetFieldValue<string>("UserId", value);
                if (m_OriginalUserId == null)
                    m_OriginalUserId = value;
            }
        }

        public int m_BackupPrecedence =-1;
        public string m_BackupUserId = null;
        public override void Clear()
        {
            if(m_BackupPrecedence==-1)
                m_BackupPrecedence = Precedence;
            if (m_BackupUserId == null)
                m_BackupUserId = UserId;
            base.Clear();
        }

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            // TODO: check why called 2 times even when we add only one ViewRestriction
            XmlElement node = base.CreateSaveNode(doc, operation);

            XmlAttribute att = doc.CreateAttribute("userid");
            att.Value = (Agent == null) ? m_OriginalUserId : Agent.Id;
            node.Attributes.Append(att);


            att = doc.CreateAttribute("precedenceid");

            if (operation == "delete")
                att.Value = m_BackupPrecedence.ToString();
            else if (operation == "update")
                att.Value = this.GetOriginalFieldValue<int>("Precedence").ToString();
            else
                att.Value = Precedence.ToString();

            node.Attributes.Append(att);


            return node;
        }

        public ViewRestriction Duplicate()
        {
            ViewRestriction newvr = AdminCore.Create<ViewRestriction>();
            newvr.TargetType = TargetType;
            newvr.Target.TargetId = Target.TargetId;
            newvr.IncludeChildren = IncludeChildren;
            newvr.Allowed = Allowed;
            newvr.InformationLevel = InformationLevel;
            newvr.Precedence = Agent.ViewRestrictions.OrderBy((a) => (a.Precedence)).Last().Precedence + 1;
            Agent.ViewRestrictions.Add(newvr);
            return newvr;
        }
    }
}
