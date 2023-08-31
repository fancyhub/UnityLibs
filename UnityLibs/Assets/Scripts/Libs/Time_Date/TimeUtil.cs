/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    public static class TimeUtil
    {
        //tick的差距，从0001年到1970年
        private const long C_TICK_DT_1970_0001 = 621355968000000000L;
        /// <summary>
        /// 获取timestamp，以秒为单位, 从1970-01-01T00:00:00Z. 开始到现在<para/>
        /// 1秒 = 1000 毫秒 ms，millisecond <para/>
        /// 1毫秒 = 1000 微秒 μs，microsecond <para/>
        /// 1微秒 = 1000 纳秒 ns，nanosecond<para/>        
        /// 1 微秒 = 10 tick <para/>
        /// 1 毫秒 = 10，000 tick<para/>
        /// 1 秒 = 10，000,000 tick<para/>
        /// 1 tick = 100 纳秒，0.1微秒 <para/>
        /// </summary>     
        private const long C_MICRO_SEC_TICKS = 10L; //1微秒 对应的Ticks
        private const long C_MILLI_SEC_TICKS = C_MICRO_SEC_TICKS * 1000L; //1毫秒 对应的ticks
        private const long C_SEC_TICKS = C_MILLI_SEC_TICKS * 1000L; //1 秒对应的 ticks
        private const long C_MINUTE_TICKS = C_SEC_TICKS * 60L;// 一分钟对应的 ticks
        private const long C_HOUR_TICKS = C_MINUTE_TICKS * 60L;//一小时对应的 ticks
        private const long C_DAY_TICKS = C_HOUR_TICKS * 24L;//一天内的ticks
        private const long C_SEC_2_MILLISEC = 1000L;//1秒对应的 毫秒
        private static long _system_time = 0;
        private static long _last_system_time = (uint)Environment.TickCount;

        private static long _svr_dt = 0;
        private static int _frame_count = 0;

        //这个由外部设置,为了给多线程用的
        public static int FrameCount => _frame_count;

        //只能在主线程调用
        public static void UpdateFrameCount()
        {
            _frame_count = UnityEngine.Time.frameCount;
        }

        /// <summary>
        /// 获取系统开启到现在的时间，毫秒,用户不可修改, 非多线程安全的
        /// </summary>
        public static long SystemTime
        {
            get
            {
                //因为是int, 对应的毫秒, 最大支持49.8 天
                // https://docs.microsoft.com/en-us/dotnet/api/system.environment.tickcount?view=net-6.0                
                long now = (uint)Environment.TickCount;
                if (now < _last_system_time)
                    _system_time += uint.MaxValue;
                _last_system_time = now;
                return now + _system_time;
            }
        }

        /// <summary>
        /// 本地时间戳,秒
        /// </summary>
        public static int Unix { get { return (int)((DateTime.UtcNow.Ticks - C_TICK_DT_1970_0001) / C_SEC_TICKS); } }

        /// <summary>
        /// 本地时间戳,毫秒
        /// </summary>
        public static long UnixMilli { get { return (DateTime.UtcNow.Ticks - C_TICK_DT_1970_0001) / C_MILLI_SEC_TICKS; } }

        /// <summary>
        /// 服务器时间戳,秒
        /// </summary>
        public static int SvrUnix
        {
            get { return (int)((UnixMilli + _svr_dt) / C_SEC_2_MILLISEC); }
            set { _svr_dt = value * C_SEC_2_MILLISEC - UnixMilli; }
        }

        /// <summary>
        /// 服务器时间戳, 毫秒
        /// </summary>
        public static long SvrUnixMilli
        {
            get { return UnixMilli + _svr_dt; }
            set { _svr_dt = value < int.MaxValue ? (value * C_SEC_2_MILLISEC - UnixMilli) : (value - UnixMilli); }
        }

        /// <summary>
        /// 把本地的时间戳转换成服务器的时间戳 <para/>
        /// 参数: 本地时间戳, 可以是秒,或者是毫秒
        /// </summary>
        /// <param name="local_time">本地的时间戳,秒/毫秒</param>
        /// <returns>服务器的时间戳, 毫秒</returns>
        public static long Loc2Svr(long local_time)
        {
            if (local_time <= 0)
                local_time = 0;
            else if (local_time < int.MaxValue)
                local_time = local_time * C_SEC_2_MILLISEC;
            return local_time + _svr_dt;
        }

        /// <summary>
        /// 把服务器的时间戳,转换成本地的时间戳 <para/>
        /// 参数: 服务器时间戳, 可以是秒,或者是毫秒
        /// </summary>
        /// <param name="svr_time">服务器的时间戳, 秒/毫秒</param>
        /// <returns>本地的时间戳, 毫秒</returns>
        public static long Svr2Loc(long svr_time)
        {
            if (svr_time <= 0)
                svr_time = 0;
            else if (svr_time < int.MaxValue)
                svr_time = svr_time * C_SEC_2_MILLISEC;
            return svr_time - _svr_dt;
        }

    }
}
