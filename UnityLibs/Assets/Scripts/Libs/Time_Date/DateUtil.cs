/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    public static class DateUtil
    {
        //tick的差距，从0001年到1970年
        private const long C_TICK_DT_1970_0001 = 621355968000000000L;

        private const long C_MICRO_SEC_TICKS = 10L; //1微秒 对应的Ticks
        private const long C_MILLI_SEC_TICKS = C_MICRO_SEC_TICKS * 1000L; //1毫秒 对应的ticks
        private const long C_SEC_TICKS = C_MILLI_SEC_TICKS * 1000L; //1 秒对应的 ticks
        private const long C_MINUTE_TICKS = C_SEC_TICKS * 60L;// 一分钟对应的 ticks
        private const long C_HOUR_TICKS = C_MINUTE_TICKS * 60L;//一小时对应的 ticks
        private const long C_DAY_TICKS = C_HOUR_TICKS * 24L;//一天内的ticks
        private const long C_SEC_2_MILLISEC = 1000L;//1秒对应的 毫秒
                                                    //
        #region Date
        private const string C_SVR_ZONE_NAME = "GameSvr";
        public static TimeZoneInfo SvrTimeZone = TimeZoneInfo.CreateCustomTimeZone(C_SVR_ZONE_NAME, new TimeSpan(8, 0, 0), C_SVR_ZONE_NAME, C_SVR_ZONE_NAME);
        /// <summary>
        /// 一周开始
        /// </summary>
        public static DayOfWeek FirstDayOfWeek = DayOfWeek.Monday;
        public static void SetSvrTimeZone(int hour, int min = 0)
        {
            SvrTimeZone = TimeZoneInfo.CreateCustomTimeZone(C_SVR_ZONE_NAME, new TimeSpan(hour, min, 0), C_SVR_ZONE_NAME, C_SVR_ZONE_NAME);
        }

        public static DateTime NowLocal { get { return DateTime.Now; } }

        public static DateTime NowSvr { get { return ToSvr(TimeUtil.SvrUnixMilli); } }


        /// <summary>
        /// 从时间戳 秒/毫秒 转成 DateTime Local
        /// </summary> 
        public static DateTime ToLocal(long time)
        {
            if (time <= 0)
                time = 0;
            else if (time < int.MaxValue)
                time = time * C_SEC_2_MILLISEC;

            long tick = time * C_MILLI_SEC_TICKS + C_TICK_DT_1970_0001;
            return new DateTime(tick, DateTimeKind.Utc).ToLocalTime();
        }

        /// <summary>
        /// 从时间戳 秒/毫秒 转成 DateTime UTC
        /// </summary> 
        public static DateTime ToUtc(long time)
        {
            if (time <= 0)
                time = 0;
            else if (time < int.MaxValue)
                time = time * C_SEC_2_MILLISEC;

            long tick = time * C_MILLI_SEC_TICKS + C_TICK_DT_1970_0001;
            return new DateTime(tick, DateTimeKind.Utc);
        }

        /// <summary>
        /// 从时间戳 秒/毫秒 转成 DateTime Svr
        /// </summary> 
        public static DateTime ToSvr(long time)
        {
            if (time <= 0)
                time = 0;
            else if (time < int.MaxValue)
                time = time * C_SEC_2_MILLISEC;

            long tick = time * C_MILLI_SEC_TICKS + C_TICK_DT_1970_0001;
            return new DateTime(tick, DateTimeKind.Utc).ToSvr();
        }

        public static long ToUnix(this DateTime date)
        {
            return (date.ToUtc().Ticks - C_TICK_DT_1970_0001) / C_SEC_TICKS;
        }

        public static long ToUnixMilli(this DateTime date)
        {
            return (date.ToUtc().Ticks - C_TICK_DT_1970_0001) / C_MILLI_SEC_TICKS;
        }

        /// <summary>
        /// 转成指定的本地时区
        /// </summary>
        public static DateTime ToLocal(this DateTime date)
        {
            switch (date.Kind)
            {
                case DateTimeKind.Utc:
                case DateTimeKind.Local:
                    return date.ToLocalTime();
                case DateTimeKind.Unspecified:
                    return TimeZoneInfo.ConvertTimeToUtc(date, SvrTimeZone).ToLocalTime();
                default:
                    return date;
            }
        }

        /// <summary>
        /// 转成指定的本地时区
        /// </summary>
        public static DateTime ToSvr(this DateTime date)
        {
            switch (date.Kind)
            {
                case DateTimeKind.Utc:
                case DateTimeKind.Local:
                    return TimeZoneInfo.ConvertTime(date, SvrTimeZone);
                case DateTimeKind.Unspecified:
                    return date;
                default:
                    return date;
            }
        }

        /// <summary>
        /// 转成UTC的时区
        /// </summary>
        public static DateTime ToUtc(this DateTime date)
        {
            switch (date.Kind)
            {
                case DateTimeKind.Utc:
                    return date;
                case DateTimeKind.Local:
                    return date.ToUniversalTime();
                case DateTimeKind.Unspecified:
                    return TimeZoneInfo.ConvertTimeToUtc(date, SvrTimeZone);
                default:
                    return date;
            }
        }

        public static TimeSpan GetTimeOfDay(this DateTime d)
        {
            return new TimeSpan(d.Ticks % C_DAY_TICKS);
        }

        public static TimeSpan GetTimeOfWeek(this DateTime d)
        {
            return GetTimeOfWeek(d, FirstDayOfWeek);
        }

        public static TimeSpan GetTimeOfWeek(this DateTime d, DayOfWeek first_day_of_week)
        {
            long time_of_day = d.Ticks % C_DAY_TICKS;
            DateTime d2 = d.AddTicks(-time_of_day);
            int dt_day = ((int)d2.DayOfWeek - (int)first_day_of_week + 7) % 7;
            return new TimeSpan(dt_day * C_DAY_TICKS + time_of_day);
        }

        public static TimeSpan GetTimeOfMonth(this DateTime d)
        {
            long time_of_day = d.Ticks % C_DAY_TICKS;
            DateTime d2 = d.AddTicks(-time_of_day);
            return new TimeSpan((d2.Day - 1) * C_DAY_TICKS + time_of_day);
        }

        /// <summary>
        /// 获取今天开始的时间, 返回今天0点0分0秒
        /// </summary>        
        public static DateTime GetThisDay(this DateTime d)
        {
            return d - d.GetTimeOfDay();
        }

        /// <summary>
        /// 获取这个星期开始的时间, 返回这个星期 0点0分0秒的时间
        /// </summary>       
        public static DateTime GetThisWeek(this DateTime d)
        {
            return d - d.GetTimeOfWeek(FirstDayOfWeek);
        }

        /// <summary>
        /// 获取这个星期开始的时间, 返回这个星期 0点0分0秒的时间
        /// </summary>       
        public static DateTime GetThisWeek(this DateTime d, DayOfWeek first_of_week)
        {
            return d - d.GetTimeOfWeek(first_of_week);
        }

        public static DateTime GetThisMonth(this DateTime d)
        {
            return d - d.GetTimeOfMonth();
        }

        /// <summary>
        /// 是否为同一天, 这个函数不管时区, 由外部处理
        /// </summary>
        public static bool IsSameDay(DateTime d1, DateTime d2)
        {
            return d1.GetThisDay() == d2.GetThisDay();
        }

        /// <summary>
        /// 时间戳是否为今天
        /// </summary>
        public static bool IsToday(long unix, bool svr_zone = true)
        {
            if (svr_zone)
                return ToSvr(unix).GetThisDay() == NowSvr.GetThisDay();
            else
                return ToLocal(unix).GetThisDay() == DateTime.Today;
        }
        #endregion
    }
}
