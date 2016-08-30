using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixxis.Client.Recording.AppServerRequest
{
    public static class RelayAction
    {
        public const string Datetime = "?action=datetime";
        public const string Size = "?action=size";
        public const string DownloadFile = "";
    }

    public static class DataAction
    {
        public const string RemoteData = "?action=remotedata";
        public const string Listcontexts = "?action=listcontexts";
        public const string GetContextData = "?action=getcontextdata";
        public const string ListContextFields = "?action=listcontextfields";
    }

    public static class DataOptions
    {
        public static string IncludeSchema = "schema=true";
        public static string ExcludeSchema = "schema=false";
        public static string Timeout = "timeout={0}";
    }
}
