using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Data;

namespace Nixxis.Client.Reporting
{
    public class ContextDataClient : IDisposable
    {
        private static int m_RequestsTimeout = 900000;

        private Uri m_BaseLocation;

        public ContextDataClient(Uri baseLocation)
        {
            m_BaseLocation = baseLocation;
        }

        public ContextDataClient(string baseLocation)
            : this(new Uri(baseLocation))
        {
        }

        public HttpWebResponse DoRequest(string request)
        {
            return DoRequest(request, null, 0, 0);
        }

        public HttpWebResponse DoRequest(string request, byte[] postData, int offset, int count)
        {
            Uri RequestUri = (string.IsNullOrEmpty(request)) ? m_BaseLocation : new Uri(m_BaseLocation, request);
            HttpWebResponse Response = null;

            for (int Retry = 0; Retry < 2; Retry++)
            {
                HttpWebRequest Request = WebRequest.Create(RequestUri) as HttpWebRequest;
                Request.Timeout = m_RequestsTimeout;
                try
                {
                    if (postData != null)
                    {
                        using (Stream RequestStream = Request.GetRequestStream())
                        {
                            RequestStream.Write(postData, offset, count);
                        }
                    }

                    Response = (HttpWebResponse)Request.GetResponse();

                    break;
                }
                catch (WebException WEx)
                {
                    if (WEx.Response is HttpWebResponse)
                    {
                        Response = (HttpWebResponse)WEx.Response;

                        System.Diagnostics.Trace.WriteLine(string.Format("Received http status: {0}", Response.StatusCode), "DataClient");
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine(WEx.GetBaseException().ToString(), "DataClient");
                    }
                }
                catch (Exception Ex)
                {
                    System.Diagnostics.Trace.WriteLine(Ex.GetBaseException().ToString(), "DataClient");
                }
            }

            return Response;
        }

        public XmlDocument LoadXmlDocument(string queryString)
        {
            using (HttpWebResponse Response = DoRequest(queryString))
            {
                if (Response != null && Response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream ResponseStream = Response.GetResponseStream())
                    {
                        XmlDocument Doc = new XmlDocument();

                        Doc.Load(ResponseStream);

                        return Doc;
                    }
                }
            }

            return null;
        }

        public DataSet LoadDataSet(string source, string procedure, IDictionary<string, object> parameters)
        {
            StringBuilder SB = new StringBuilder(m_BaseLocation.ToString());

            SB.Append("?action=remotedata&v1=true&source=").Append(source).Append("&exec=").Append(procedure);

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> Pair in parameters)
                {
                    SB.Append("&").Append(Pair.Key);

                    if (Pair.Value == null)
                    {
                        SB.Append("=n");
                    }
                    else if (Pair.Value is DateTime)
                    {
                        SB.Append("=d,").Append(((DateTime)Pair.Value).ToString("yyyyMMdd"));
                    }
                    else if (Pair.Value is int || Pair.Value is long)
                    {
                        SB.Append("=i,").Append(Pair.Value.ToString());
                    }
                    else if (Pair.Value is float || Pair.Value is double)
                    {
                        SB.Append("=f,").Append(Pair.Value.ToString());
                    }
                    else if (Pair.Value is bool)
                    {
                        SB.Append("=b,").Append(Pair.Value.ToString());
                    }
                    else
                    {
                        SB.Append("=s,").Append(Pair.Value.ToString());
                    }
                }
            }

            using (HttpWebResponse Response = DoRequest(SB.ToString()))
            {
                if (Response != null && Response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream ResponseStream = Response.GetResponseStream())
                    {
                        DataSet DS = new DataSet();

                        DS.ReadXml(ResponseStream);

                        return DS;
                    }
                }
            }

            return null;
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
