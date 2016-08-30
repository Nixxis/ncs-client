using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Nixxis.Client.Controls
{
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
