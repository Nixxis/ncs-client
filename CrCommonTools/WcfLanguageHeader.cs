using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace Common.Tools
{
    
    public class CustomMessageInspector : IDispatchMessageInspector, IClientMessageInspector
    {
        #region Message Inspector of the Service

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }

        #endregion

        #region Message Inspector of the Consumer

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            try
            {
                if (!request.Properties.ContainsKey(HttpRequestMessageProperty.Name))
                    request.Properties.Add(HttpRequestMessageProperty.Name, new HttpRequestMessageProperty());

                HttpRequestMessageProperty hrmp = request.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;

                if (hrmp != null)
                {
                    hrmp.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
                }

                International Intl = new International { Locale = CultureInfo.CurrentCulture.TwoLetterISOLanguageName };

                MessageHeader header = MessageHeader.CreateHeader("International", "http://www.w3.org/2005/09/ws-i18n", Intl);
                request.Headers.Add(header);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
            return null;
        }

        #endregion
    } 



    [AttributeUsage(AttributeTargets.Class)]
    public class WcfLanguageHeaderBehavior : Attribute, IEndpointBehavior
    {
        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            CustomMessageInspector inspector = new CustomMessageInspector();
            clientRuntime.MessageInspectors.Add(inspector);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
            ChannelDispatcher channelDispatcher = endpointDispatcher.ChannelDispatcher;
            if (channelDispatcher != null)
            {
                foreach (EndpointDispatcher ed in channelDispatcher.Endpoints)
                {
                    CustomMessageInspector inspector = new CustomMessageInspector();
                    ed.DispatchRuntime.MessageInspectors.Add(inspector);
                }
            }
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }



    public class WcfLanguageHeaderExtensionElement : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new WcfLanguageHeaderBehavior();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(WcfLanguageHeaderBehavior);
            }
        }
    }

    [DataContract(Name="International", Namespace="http://www.w3.org/2005/09/ws-i18n")]  
    public class International
    {
        private string locale;
        private string tz;
        private List<Preferences> preferences;

        public International()
        {
        }

        [DataMember(Name = "Locale")] 
        public string Locale
        {
            get { return locale; }
            set { locale = value; }
        }

        [DataMember(Name = "TZ", IsRequired = false, EmitDefaultValue = false)] 
        public string Tz
        {
            get { return tz; }
            set { tz = value; }
        }

        [DataMember(Name="Preferences", IsRequired=false, EmitDefaultValue=false)]
        public List<Preferences> Preferences
        {
            get { return preferences; }
            set { preferences = value; }
        }
    }

    public class Preferences : IXmlSerializable
    {
        private XmlNode anyElement;

        public XmlNode AnyElement
        {
            get { return anyElement; }
            set { anyElement = value; }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlDocument document = new XmlDocument();
            anyElement = document.ReadNode(reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            anyElement.WriteTo(writer);
        }
    }
}
