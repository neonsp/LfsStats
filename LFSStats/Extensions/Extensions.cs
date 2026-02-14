using System;

namespace LFSStatistics
{
    public static class Extensions
    {
        public static long TotalMillisecondsLong(this TimeSpan time)
        {
            try
            {
                return (long)time.TotalMilliseconds;
            }
            catch (Exception)
            {
                return (long)0;
            }
        }
    }
}
