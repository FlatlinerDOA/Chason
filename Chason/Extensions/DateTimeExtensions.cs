//--------------------------------------------------------------------------------------------------
// <copyright file="DateTimeExtensions.cs" company="Andrew Chisholm">
//   Copyright (c) 2013 Andrew Chisholm All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace Chason.Extensions
{
    using System;
    using System.Globalization;
    using System.Text;

    public static class DateTimeExtensions
    {
        /// <summary>
        /// The number of ticks that Javascript's epoch starts at (relative to .NET's epoch).
        /// </summary>
        internal static readonly long JavascriptEpoch = 621355968000000000;

        public static string ToJavascriptDate(this DateTime dateTime)
        {
            var sb = new StringBuilder(@"/Date(");
            var ticks = (dateTime.ToUniversalTime().Ticks - JavascriptEpoch) / 10000;
            sb.Append(ticks);
            switch (dateTime.Kind)
            {
                case DateTimeKind.Local:
                case DateTimeKind.Unspecified:
                    var offset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
                    // Offset code taken from NewtonSoft.Json
                    int absHours = Math.Abs(offset.Hours);
                    if (absHours < 10)
                    {
                        sb.Append(0);
                    }

                    sb.Append(absHours);
                    int absMinutes = Math.Abs(offset.Minutes);
                    if (absMinutes < 10)
                    {
                        sb.Append(0);
                    }

                    sb.Append(absMinutes);
                    break;
            }

            sb.Append(@")/");
            return sb.ToString();
        }

        public static DateTime FromJavascriptDate(this string dateTime)
        {
            var ticks = long.Parse(dateTime.Substring(7, dateTime.Length - 10)) * 10000L;
            return new DateTime(JavascriptEpoch + ticks, DateTimeKind.Utc);
        }

        public static string ToJavascriptDateOffset(this DateTimeOffset dateTime)
        {
            var sb = new StringBuilder(@"/Date(");
            var ticks = (dateTime.ToUniversalTime().Ticks - JavascriptEpoch) / 10000;
            sb.Append(ticks);
            sb.Append(@")/");
            return sb.ToString();
        }

        public static DateTimeOffset FromJavascriptDateOffset(this string dateTime)
        {
            var ticks = long.Parse(dateTime.Substring(7, dateTime.Length - 10)) * 10000L;
            return new DateTimeOffset(JavascriptEpoch + ticks, TimeSpan.Zero);
        }
    }
}
