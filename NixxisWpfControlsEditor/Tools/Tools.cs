using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Nixxis.Client.Controls
{
    public static class Tools
    {
        public static string ApplicationDescription = AssemblyProduct;
        public static string DateTimeFormat = "yyyy MM dd hh:mm:ss.fff";
        public static bool DebugActive = false;

        public enum LogType
        {
            Info,
            Warning,
            Error,
            Debug,
        }
        public static void Log(string msg, LogType type)
        {
            try
            {
                if (DebugActive|| type== LogType.Error)
                    System.Diagnostics.Trace.WriteLine(string.Format("{2} {3} {0}: {1}", type.ToString(), msg, DateTime.Now.ToString(DateTimeFormat), ApplicationDescription));
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString(DateTimeFormat) + " " + ApplicationDescription + " Err(function LOG): " + err.ToString());
            }
        }
        public static string AssemblyProduct
        {
            get
            {
                // Get all Product attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                // If there aren't any Product attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Product attribute, return its value
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }
    }
}
