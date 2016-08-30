using System;
using System.Xml;

namespace Nixxis.Client.Admin
{
    public class LocationCost : AdminObject
    {
        internal override void Load(System.Xml.XmlElement node)
        {
            // TODO: check if it is a great idea...
            Id = AdminObjectLink.GetCombinedId(node.Attributes["fromlocation"].Value, node.Attributes["tolocation"].Value);
            base.Load(node);
        }

        public LocationCost(AdminObject parent)
            : base(parent)
        {
        }

        public AdminObjectReference<Location> FromLocation
        {
            get;
            internal set;
        }

        public AdminObjectReference<Location> ToLocation
        {
            get;
            internal set;
        }

        public float Cost
        {
            get
            {
                return GetFieldValue<float>("Cost");
            }
            set
            {
                SetFieldValue<float>("Cost", value);
                FirePropertyChanged("Allowed");
            }
        }
        public bool Allowed
        {
            get
            {
                return Cost >= 0;
            }
            set
            {
                if (value)
                {
                    if (Cost < 0)
                        Cost = 0;
                }
                else
                {
                    Cost = -1;
                }
                FirePropertyChanged("Allowed");
                FirePropertyChanged("Cost");
            }
        }

        protected override System.Xml.XmlElement CreateSaveNode(System.Xml.XmlDocument doc, string operation)
        {
            XmlElement node = base.CreateSaveNode(doc, operation);

            node.Attributes.RemoveNamedItem("id");

            string[] tempArray = Id.Split(new char[]{'/'});
            XmlAttribute att = doc.CreateAttribute("fromlocationid");
            if (FromLocation.TargetId != null)
                att.Value = FromLocation.TargetId;
            else
                att.Value = tempArray[0];


            node.Attributes.Append(att);

            att = doc.CreateAttribute("tolocationid");
            if (ToLocation.TargetId != null)
                att.Value = ToLocation.TargetId;
            else
                att.Value = tempArray[1];

            node.Attributes.Append(att);

            return node;    
        }
    }
}