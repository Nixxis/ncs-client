using System;
namespace ContactRoute.Client
{
    internal interface IHttpNetworkClient
    {
        void Abort();
        void ClearCookies();
        System.Net.IPEndPoint ClientEndpoint { get; }
        bool TraceNetwork { get; set; }
        int ContentLength { get; }
        string ContentType { get; }
        System.Net.CookieContainer Cookies { get; }
        bool Connect();
        bool Get(string query);
        bool Post(string query, byte[] postData);
        bool Post(string query, byte[] postData, int offset, int count);
        bool Post(string query, byte[] postData, int offset, int count, string contentType);
        bool Post(string query, byte[] postData, string contentType);
        byte[] RawData { get; }
        int StatusCode { get; }
        int Timeout { get; set; }
        string UserAgent { get; set; }
    }
}
