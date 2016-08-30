using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nixxis.Client.Admin
{
    
    [AdminSave(SkipSave = true)]
    public class CurrentActivityFilter : AdminObject
    {
        private AdminObjectReference<OutboundActivity> m_Activity;

        public CurrentActivityFilter(AdminCore core)
            : base(core)
        {
            Activity = new AdminObjectReference<OutboundActivity>(this);
        }

        public CurrentActivityFilter(AdminCore core, string id)
            : base(core)
        {
            Id = id;
            Activity = new AdminObjectReference<OutboundActivity>(this);
        }

        public AdminObjectReference<OutboundActivity> Activity
        {
            get
            {
                return m_Activity;
            }
            internal set
            {
                if (m_Activity != null)
                {
                    m_Activity.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(ActivityPropertyChanged);
                    m_Activity.TargetPropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(ActivityPropertyChanged);
                }

                m_Activity = value;

                if (m_Activity != null)
                {
                    m_Activity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ActivityPropertyChanged);
                    m_Activity.TargetPropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ActivityPropertyChanged);
                }
            }
        }

        virtual protected void ActivityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FirePropertyChanged("DisplayText");
        }


        public override string DisplayText
        {
            get
            {
                if (Activity.HasTarget)
                {
                    return string.Format("Records linked to {0}", Activity.Target.DisplayText);
                }
                else
                {
                    if (Id.Equals("_0this"))
                    {
                        return "Records linked to this activity";
                    }
                    else if (Id.Equals("_2any"))
                    {
                        return "Records linked to any activity";
                    }
                    else if (Id.Equals("_1null"))
                    {
                        return "Records not linked to an activity";
                    }
                    else if (Id.Equals("_3dnr"))
                    {
                        return "Records not to be called again";
                    }
                }
                return string.Empty;
            }
        }
    }

    [AdminSave(SkipSave = true)]
    [AdminObjectLinkCascadeAttribute(typeof(OutboundActivity), "ActivityFilterParts")]
    public class OutboundActivityCurrentActivityFilter : AdminObjectLink<OutboundActivity, CurrentActivityFilter>
    {
        public OutboundActivityCurrentActivityFilter(AdminObject parent)
            : base(parent)
        {
        }

        public OutboundActivityCurrentActivityFilter(AdminCore core)
            : base(core)
        {
        }

        internal override void Load(System.Xml.XmlElement node)
        {

            base.Load(node);

            if (Id2 !=null && !Id2.Equals("_0this") && !Id2.Equals("_2any") && !Id2.Equals("_1null") && !Id2.Equals("_3dnr"))
            {
                // This is a temporry solution beacuse the CampaignCurrentActivityFilterKinds collection is not yet available at this time.
                // By naming convention, I avoid relying on that collection...
                Id2 = "zz" + Id2;
                Id = AdminObjectLink.GetCombinedId(Id1, Id2);
            }

        }

        public string ActivityId
        {
            get
            {
                return Id1;
            }
            set
            {
                Id1 = value;
            }
        }

        public string ActivityFilterHelperId
        {
            get
            {
                return Id2;
            }
            set
            {
                Id2 = value;
            }
        }

        public OutboundActivity Activity
        {
            get 
            {
                if (m_Core == null)
                    return null;
                return (OutboundActivity)(m_Core.GetAdminObject(Id1)); 
            }
        }

        public CurrentActivityFilter Object
        {
            get
            {
                if (m_Core == null)
                    return null;
                return (CurrentActivityFilter)(m_Core.GetAdminObject(Id2));
            }
        }

        //linked to this activity
        //<CurrentActivityFilter>
        //  <FilterPart FieldName="" Operator="IsNotNull" Operand="" DBType="Unknown" AppointmentFunction="False" />
        //</CurrentActivityFilter>

        //not linked to any act
        //<CurrentActivityFilter>
        //  <FilterPart FieldName="" Operator="IsNull" Operand="" DBType="Unknown" AppointmentFunction="False" />
        //</CurrentActivityFilter>
        
        //not to be called again
        //<CurrentActivityFilter>
        //  <FilterPart FieldName="" Operator="Like" Operand="_%" DBType="Unknown" AppointmentFunction="False" />
        //</CurrentActivityFilter>

        //linked to any act
        //<CurrentActivityFilter>
        //  <FilterPart FieldName="" Operator="Superior" Operand="0" DBType="Unknown" AppointmentFunction="False" />
        //</CurrentActivityFilter>

        //linked to specific activity
        //<CurrentActivityFilter>
        //  <FilterPart FieldName="" Operator="Equal" Operand="92f9247bd23e40ff82398cbf0602b2e7" DBType="Unknown" AppointmentFunction="False" />
        //</CurrentActivityFilter>
        public void SaveNode(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("FilterPart");

            XmlAttribute att = doc.CreateAttribute("FieldName");
            att.Value = string.Empty;
            node.Attributes.Append(att);

            if (ActivityFilterHelperId == null)
            {
                return;
            }
            if (ActivityFilterHelperId.Equals("_0this"))
            {
                att = doc.CreateAttribute("Operator");
                att.Value = Operator.IsNotNull.ToString();
                node.Attributes.Append(att);
            }
            else if (ActivityFilterHelperId.Equals("_1null"))
            {
                att = doc.CreateAttribute("Operator");
                att.Value = Operator.IsNull.ToString();
                node.Attributes.Append(att);
            }
            else if (ActivityFilterHelperId.Equals("_2any"))
            {
                att = doc.CreateAttribute("Operator");
                att.Value = Operator.Superior.ToString();
                node.Attributes.Append(att);

                att = doc.CreateAttribute("Operand");
                att.Value = "0";
                node.Attributes.Append(att);
            }
            else if (ActivityFilterHelperId.Equals("_3dnr"))
            {
                att = doc.CreateAttribute("Operator");
                att.Value = Operator.Like.ToString();
                node.Attributes.Append(att);

                att = doc.CreateAttribute("Operand");
                att.Value = "_%";
                node.Attributes.Append(att);
            }
            else
            {
                att = doc.CreateAttribute("Operator");
                att.Value = Operator.Equal.ToString();
                node.Attributes.Append(att);

                att = doc.CreateAttribute("Operand");
                if(ActivityFilterHelperId.StartsWith("zz"))
                    att.Value = ActivityFilterHelperId.Substring(2);// this.Object.Activity.TargetId;// ActivityFilterHelperId;// oa.Id;
                else
                    att.Value = ActivityFilterHelperId;
                node.Attributes.Append(att);
            }


            att = doc.CreateAttribute("DBType");
            att.Value = DBTypes.Unknown.ToString();
            node.Attributes.Append(att);

            att = doc.CreateAttribute("AppointmentFunction");
            att.Value = "False";
            node.Attributes.Append(att);

            doc.DocumentElement.AppendChild(node);
        }

        protected override void SetFieldValue<T>(string index, T value)
        {
            base.SetFieldValue<T>(index, value);
            RecomputeFilters();
        }

        private void RecomputeFilters()
        {
            if (Activity != null)
                Activity.RecomputeFilters();
        }

    }
}
