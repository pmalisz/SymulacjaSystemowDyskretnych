using Backend.Consts;
using System;
using System.Linq;

namespace Backend.Commons.Helpers
{
    public static class TimeHelper
    {
        public static DateTime GetTimeFromString(string time)
        {
            var parts = time.Split(ApplicationConsts.TimeSeparator).Select(a => int.Parse(a)).ToArray();
            return new DateTime().AddHours(parts[0]).AddMinutes(parts[1]);
        }

        public static string GetShortTimeAsString(DateTime dateTime) => dateTime.ToString(ApplicationConsts.ShortTimeFormat);

        public static string GetLongTimeAsString(DateTime dateTime) => dateTime.ToString(ApplicationConsts.LongTimeFormat);

        public static int CompareTimes(DateTime time1, DateTime time2)
        {
            int time1Value = time1.Hour * 3600 + time1.Minute * 60 + time1.Second;
            int time2Value = time2.Hour * 3600 + time2.Minute * 60 + time2.Second;

            if (time1Value < time2Value)
                return -1;
            if (time1Value > time2Value)
                return 1;

            return 0;
        }
    }
}
