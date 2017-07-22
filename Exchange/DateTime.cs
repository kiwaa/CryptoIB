using System;

namespace CIB.Exchange
{
    public static class DateTimeUtilities
    {
        private static readonly DateTime UnixMinValue = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return (long)Math.Truncate((dateTime - UnixMinValue).TotalSeconds);
        }

        public static DateTime FromUnixTimestamp(long timestamp)
        {
            return UnixMinValue.AddSeconds(timestamp);
        }
    }
}