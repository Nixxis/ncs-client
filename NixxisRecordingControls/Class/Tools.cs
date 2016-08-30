using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Nixxis.Client.Recording
{
    public static class DataTypeCode
    {
        public const string BoolDataType = "b";
        public const string StringDataType = "s";
        public const string DateTimeDataType = "d";
        public const string IntDataType = "i";
        public const string NullValue = "n";
    }

    public static class Tools
    {
        public static bool DebugAllActive = false;

        public enum LogType
        {
            Info,
            Warning,
            Error,
            Debug,
            StatusInfo,
        }
        public static void Log(string msg, LogType type)
        {
            try
            {
                if (Tools.DebugAllActive || type != LogType.Debug)
                    System.Diagnostics.Trace.WriteLine(string.Format("{2} Nixxis Recording v2 {0}: {1}", type.ToString(), msg, DateTime.Now.ToString("yyyy MM dd hh:mm:ss.fff")));
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("yyyy MM dd hh:mm:ss.fff") + " Nixxis Recording v2 Err(function LOG): " + err.ToString());
            }
        }
    }

    public class chatInformation
    {
        public DateTime DateTime = DateTime.MinValue;
        public int EventType = 0;
        public string AgentId = "";
        public string DisplayName = "";
        public string Message = "";

        public chatInformation(string line)
        {
            string[] data = line.Split(new char[] { ',' });

            if (data.Length > 4)
            {
                DateTime = new DateTime((new DateTime(1970, 1, 1, 0, 0, 0)).Ticks + (long.Parse(data[0]) * 10000));
                EventType = int.Parse(data[1]);
                AgentId = data[2];
                DisplayName = Microsoft.JScript.GlobalObject.unescape(data[3]);
                Message = Microsoft.JScript.GlobalObject.unescape(data[4]);
            }
        }
    }

    public static class Format
    {
        public static string ToDisplayTimeSpan(TimeSpan time)
        {
            return string.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
        }
        public static string ToDisplayDuration(Duration duration)
        {
            if (duration.HasTimeSpan)
                return Format.ToDisplayTimeSpan(duration.TimeSpan);
            else
                return string.Format("{0:00}:{1:00}", duration.ToString(), duration.ToString());
        }
        public static string ToDisplayByte(long bytes)
        {
            string[] levelSubfix = new string[] { "Byte(s)", "KiB", "MiB", "GiB" };
            long[] levelValue = new long[] { 1, 1024, 1024 * 1024, 1024 * 1024 * 1024 };
            int level = 1;
            bool done = false;

            while (!done && level < levelValue.Length)
            {
                if (((double)bytes / (double)levelValue[level]) > 1)
                    level++;
                else
                {
                    done = true;
                    level--;
                }
            }
            if (level >= levelValue.Length) level = 3;
            return ((double)bytes / (double)levelValue[level]).ToString("F2") + " " + levelSubfix[level];
        }
    }

}
