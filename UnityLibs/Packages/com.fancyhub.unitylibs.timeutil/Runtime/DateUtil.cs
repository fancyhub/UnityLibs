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
        #region Svr time zone
        private static TimeSpan _svr_time_zone = DateTimeOffset.Now.Offset;

        public static void SetSvrTimeZone(int hour, int min = 0)
        {
            _svr_time_zone = new TimeSpan(hour, min, 0);
        }
        #endregion


        /// <summary>
        /// 修复Culture, 解决不同区的 float.Parse("3.1415") , 某些地方(法国等) 用 逗号做小数点的问题
        /// 但是日期的格式化还是要用本地的格式
        /// </summary>
        public static void FixCultureInfo()
        {
            System.Globalization.CultureInfo newCultureInfo = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.CurrentUICulture.Clone();
            newCultureInfo.NumberFormat = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

            System.Globalization.CultureInfo.CurrentCulture = newCultureInfo;
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = newCultureInfo;
            System.Threading.Thread.CurrentThread.CurrentCulture = newCultureInfo;
        }

        public static DateTimeOffset NowLocal { get { return ToDateTimeLocal(TimeUtil.UnixMilli); } }

        public static DateTimeOffset NowSvr { get { return ToDateTimeSvr(TimeUtil.SvrUnixMilli); } }

        #region Convert
        /// <summary>
        /// 从时间戳 秒/毫秒 转成 DateTime UTC
        /// </summary> 
        public static DateTimeOffset ToDateTimeUtc(long time_stamp)
        {
            if (time_stamp <= 0)
                return DateTimeOffset.FromUnixTimeSeconds(0);
            else if (time_stamp < int.MaxValue)
                return DateTimeOffset.FromUnixTimeSeconds(time_stamp);
            else
                return DateTimeOffset.FromUnixTimeMilliseconds(time_stamp);
        }

        /// <summary>
        /// 从时间戳 秒/毫秒 转成 DateTime Local
        /// </summary> 
        public static DateTimeOffset ToDateTimeLocal(long time_stamp)
        {
            return ToDateTimeUtc(time_stamp).ToLocalTime();
        }

        /// <summary>
        /// 从时间戳 秒/毫秒 转成 DateTime Svr
        /// </summary> 
        public static DateTimeOffset ToDateTimeSvr(long time_stamp)
        {
            return ToDateTimeUtc(time_stamp).ToOffset(_svr_time_zone);
        }

        public static long ToUnix(this DateTimeOffset date)
        {
            return date.ToUnixTimeSeconds();
        }

        public static long ToUnixMilli(this DateTimeOffset date)
        {
            return date.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 转成本地时区
        /// </summary>
        public static DateTimeOffset ToLocal(this DateTimeOffset date)
        {
            return date.ToLocalTime();
        }

        /// <summary>
        /// 转成指定的本地时区
        /// </summary>
        public static DateTimeOffset ToSvr(this DateTimeOffset date)
        {
            return date.ToOffset(_svr_time_zone);
        }

        /// <summary>
        /// 转成UTC的时区
        /// </summary>
        public static DateTimeOffset ToUtc(this DateTimeOffset date)
        {
            return date.ToUniversalTime();
        }

        /// <summary>
        /// 把string格式的时间转换成服务器的时间
        /// </summary>        
        public static bool TryParseToSvrDateTime(string value, string format, out DateTimeOffset svr_time)
        {            
            IFormatProvider provider = System.Globalization.CultureInfo.InvariantCulture;
            System.Globalization.DateTimeStyles style = System.Globalization.DateTimeStyles.None;
            if (DateTime.TryParseExact(value, format, provider, style, out DateTime dt))
            {
                svr_time = new DateTimeOffset(dt, _svr_time_zone);
                return true;
            }
            svr_time = default;
            return false;
        }

        /// <summary>
        /// 把string格式的时间转换成服务器的时间
        /// </summary>
        public static bool TryParseToSvrDateTime(string value, out DateTimeOffset svr_time)
        {
            const string format = "yyyy-MM-dd HH:mm:ss";
            return TryParseToSvrDateTime(value, format, out svr_time);
        }

        #endregion


        #region Next Reset

        private const int DefaultResetHour = 4;
        private const DayOfWeek DefaultResetDayOfWeek = DayOfWeek.Monday;
        private const int DefaultResetDayOfMonth = 1;

        /// <summary>
        /// 获取下一次刷新日期,每天刷新
        /// </summary>
        public static DateTimeOffset GetNextDayReset(DateTimeOffset now, int resetHour = DefaultResetHour)
        {
            var today_reset_time = new DateTimeOffset(now.Year, now.Month, now.Day, resetHour, 0, 0, now.Offset);
            if (today_reset_time > now)
                return today_reset_time;
            return today_reset_time.AddDays(1);
        }

        /// <summary>
        /// 获取下一次刷新日期, 每天刷新
        /// </summary>
        public static DateTimeOffset GetNextDayResetSvr(long nowTimeStamp, int resetHour = DefaultResetHour)
        {
            return GetNextDayReset(ToDateTimeSvr(nowTimeStamp), resetHour);
        }


        /// <summary>
        /// 获取下一次刷新日期, 每周刷新
        /// </summary>
        public static DateTimeOffset GetNextWeekReset(DateTimeOffset now, DayOfWeek resetDayOfWeek = DefaultResetDayOfWeek, int resetHour = DefaultResetHour)
        {
            var temp = now.AddDays(resetDayOfWeek - now.DayOfWeek);
            var this_week_reset_time = new DateTimeOffset(temp.Year, temp.Month, temp.Day, resetHour, 0, 0, now.Offset);
            if (this_week_reset_time > now)
                return this_week_reset_time;
            return this_week_reset_time.AddDays(7);
        }

        /// <summary>
        /// 获取下一次刷新日期, 每周刷新
        /// </summary>
        public static DateTimeOffset GetNextWeekResetSvr(long nowTimeStamp, DayOfWeek resetDayOfWeek = DefaultResetDayOfWeek, int resetHour = DefaultResetHour)
        {
            return GetNextWeekReset(ToDateTimeSvr(nowTimeStamp), resetDayOfWeek, resetHour);
        }

        /// <summary>
        /// 获取下一次刷新日期, 每月刷新
        /// </summary>
        public static DateTimeOffset GetNextMonthReset(DateTimeOffset now, int resetDayOfMonth = DefaultResetDayOfMonth, int resetHour = DefaultResetHour)
        {
            var this_month_reset_time = new DateTimeOffset(now.Year, now.Month, resetDayOfMonth, resetHour, 0, 0, now.Offset);
            if (this_month_reset_time > now)
                return this_month_reset_time;
            return this_month_reset_time.AddMonths(1);
        }

        /// <summary>
        /// 获取下一次刷新日期, 每月刷新
        /// </summary>
        public static DateTimeOffset GetNextMonthResetSvr(long nowTimeStamp, int resetDayOfMonth = DefaultResetDayOfMonth, int resetHour = DefaultResetHour)
        {
            return GetNextMonthReset(ToDateTimeSvr(nowTimeStamp), resetDayOfMonth, resetHour);
        }
        #endregion


        /// <summary>
        /// 是否为同一天, 这个函数不管时区, 由外部处理
        /// </summary>
        public static bool IsSameDay(DateTime day1, DateTime day2)
        {
            return day1.Date == day2.Date;
        }

        /// <summary>
        /// 是否为同一天, 这个函数不管时区, 由外部处理
        /// </summary>
        public static bool IsSameDay(DateTimeOffset day1, DateTimeOffset day2)
        {
            return day1.Date == day2.Date;
        }
    }
}
