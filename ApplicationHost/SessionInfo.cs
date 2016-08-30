using System;
using System.Xml;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using ContactRoute;

namespace Nixxis.Client
{
    public class SessionInfo : ISession
    {
        public class ServiceDescription : IServiceDescription
        {
            public ServiceDescription(XmlNode node)
            {
                Settings = new NameValueCollection();

                if (node.Attributes["id"].Value.EndsWith("$"))
                    ServiceID = node.Attributes["id"].Value.Substring(0, node.Attributes["id"].Value.Length - 1);
                else
                    ServiceID = node.Attributes["id"].Value;

                Location = node.InnerText;

                foreach (XmlAttribute Attr in node.Attributes)
                {
                    Settings.Set(Attr.Name, Attr.Value);
                }
            }

            public string ServiceID
            {
                get;  set;
            }

            public string Location
            {
                get;
                 set;
            }

            public NameValueCollection Settings
            {
                get;
                 set;
            }
        }

        XmlDocument m_ServicesDoc;
        XmlDocument m_Authentication;
        List<ServiceDescription> m_Services;

        private SessionInfo()
        { }

        public SessionInfo(XmlDocument servicesDoc, XmlDocument authentication)
        {
            m_ServicesDoc = servicesDoc;
            m_Authentication = authentication;

            m_Services = new List<ServiceDescription>(20);

            if (m_ServicesDoc != null)
            {
                foreach (XmlNode Node in m_ServicesDoc.SelectNodes("//services/service"))
                {
                    ServiceDescription Desc = new ServiceDescription(Node);
                    m_Services.Add(Desc);
                }
            }
        }

        public bool Connect()
        {
            return true;
        }

        public Uri BaseUri
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public string DomainName
        {
            get
            {
                try
                {
                    return m_ServicesDoc.DocumentElement.Attributes["name"].Value;
                }
                catch
                {
                }

                return string.Empty;
            }
        }
        public System.Net.NetworkCredential Credential
        {
            get 
            {
                if (m_Authentication == null)
                    return null;

                XmlNode LoginNode = m_Authentication.SelectSingleNode("AgentLogin/agent");

                if (LoginNode == null)
                    return null;

                return new System.Net.NetworkCredential(LoginNode.SelectSingleNode("account").InnerText, LoginNode.Attributes["token"].Value);
            }
        }

        public string Extension
        {
            get 
            {
                if (m_Authentication == null)
                    return null;

                XmlNode LoginNode = m_Authentication.SelectSingleNode("AgentLogin/agent");

                if (LoginNode == null)
                    return null;

                return LoginNode.Attributes["extension"].Value;
            }
        }

        public int DebugMode
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public IServiceDescription this[int index]
        {
            get 
            {
                return m_Services[index];
            }
        }

        public IServiceDescription this[string index]
        {
            get 
            {
                return m_Services.Find(s => s.ServiceID == index);
            }
        }

        public Uri MakeUri(string relativeUri)
        {
            throw new NotImplementedException();
        }


        public void Trace(string strTace)
        {
            System.Diagnostics.Trace.WriteLine(strTace);

            HttpWebRequest webRequest = WebRequest.Create(this["trace"].Location) as HttpWebRequest;
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.AllowWriteStreamBuffering = true;

            using (Stream webRequestStream = webRequest.GetRequestStream())
            {
                using (StreamWriter tw = new StreamWriter(webRequestStream))
                {
                    try
                    {
                        tw.Write(string.Format("({0}):", m_Authentication.SelectSingleNode("/AgentLogin/agent").Attributes["id"].Value));
                    }
                    catch
                    {
                    }
                    tw.Write(strTace);
                    tw.Close();
                }

                webRequestStream.Close();
            }

            try
            {
                webRequest.BeginGetResponse(null, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }
    }

}
