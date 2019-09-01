using System;

namespace iRaidTools
{
    public static class DateTimeHelper
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

        /// <summary>
        /// Returns total seconds (UnixTimestamp) since Unix Epoch for given DateTime.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ToUnixTimestamp(this DateTime time)
        {
            return (long)(time - UnixEpoch).TotalSeconds;
        }

        /// <summary>
        /// Returns total milliseconds (UnixTimestamp) since Unix Epoch for given DateTime.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ToUnixTimestampInMilliseconds(this DateTime time)
        {
            return (long)(time - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        /// <summary>
        /// Return DateTime from Unix Epoch in seconds.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime SecondsToDateTime(this long seconds)
        {
            return UnixEpoch.AddSeconds(seconds);
        }

        /// <summary>
        /// Return DateTime from Unix Epoch in milliseconds.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime MillisecondsToUnixDateTime(this long milliseconds)
        {
            return UnixEpoch.AddMilliseconds(milliseconds);
        }
    }
}
