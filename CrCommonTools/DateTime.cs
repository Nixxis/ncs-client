using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Tools
{
    public static class DateTimeTools
    {
        /// <summary>
        /// Convert a datetime to a pipe separted string (YYYY|M|D|h|m|s)
        /// </summary>
        /// <param name="dateTime">Datetime to convert</param>
        /// <returns></returns>
        public static string ToArrayString(DateTime dateTime)
        {
            return string.Format("{0}|{1}|{2}|{3}|{4}|{5}", dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }
    }
}
