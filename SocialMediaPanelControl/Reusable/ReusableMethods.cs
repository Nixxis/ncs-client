using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;

namespace Nixxis.Client.Agent.Reusable
{
    public static class ReusableMethods
    {
        public static string MakeRequest(Uri uri, string contentLink, string action, string parameters = "")
        {
            string[] urlsplit = uri.AbsoluteUri.Split('/');
            string url = urlsplit[0] + "//" + urlsplit[2] + contentLink;

            HttpWebRequest wr = WebRequest.Create(url + action + parameters) as HttpWebRequest;
            if (wr != null)
            {
                wr.Method = "GET";

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)wr.GetResponse())
                    using (Stream objStream = response.GetResponseStream())
                        return new StreamReader(objStream).ReadToEnd();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    Console.WriteLine(e);
                }
            }

            return string.Empty;
        }

        public static BitmapImage GetBitmapImageThreadSafeOrDefault(string imagePath)
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri(@"pack://application:,,,/FacebookPanelControlS;component/Images/ImageNotFound.png"));
            
            try
            {
                bitmapImage = new BitmapImage(new Uri(string.Format(@"pack://application:,,,/FacebookPanelControlS;component/{0}", imagePath)));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Console.WriteLine(e);
            }

            return bitmapImage;
        }

        public static SortedDictionary<string, string> ExtractUrlParameters(Uri uri)
        {
            char[] querySeparator = new[] { '&' };                                                                              //Defines the separator for query's
            char[] valueSeparator = new[] { '=' };                                                                              //Defines the separator for value's of the query's

            string[] parameters = uri.Query.Substring(1).Split(querySeparator, StringSplitOptions.RemoveEmptyEntries);          //Removes the leading slash (/)
            SortedDictionary<string, string> commands = new SortedDictionary<string, string>();                                 //Extracts the parameter/value elements (in form parameter=value)

            foreach (string param in parameters)
            {
                string[] parts = param.Split(valueSeparator, 2);                                                                //Splits the parameter/value in parameter and value

                commands.Add(parts[0].ToLowerInvariant(), parts.Length > 1 ? parts[1] : null);                                  //Changes the parameter to lowerCase and gives it a corresponding value                                 
            }

            return commands;
        }
    }
}
