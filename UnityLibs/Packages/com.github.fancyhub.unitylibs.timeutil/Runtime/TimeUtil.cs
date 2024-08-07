/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Runtime.CompilerServices;

namespace FH
{
    public static class SystemStartTimer
    {
        private const int CThresholdMS = 1000 * 60 * 60 * 24 * 2;//小于2天

        private static long _time_round_delta = 0;
        private static long _last_time = (uint)Environment.TickCount;
        private static object _locker = new object();
        /// <summary>
        /// 获取系统开启到现在的时间，毫秒,用户不可修改, 线程安全的
        /// 时间可能少 49.8天的倍数
        /// </summary>
        public static long Now
        {
            get
            {
                //因为是int, 对应的毫秒, 最大支持49.8 天
                // https://docs.microsoft.com/en-us/dotnet/api/system.environment.tickcount?view=net-6.0                
                long now = (uint)Environment.TickCount;

                //1. 先简单判断, 如果小于规定时间 ,就用旧值来计算时间
                long dt = now - _last_time;
                if (0 <= dt && dt <= CThresholdMS)
                    return now + _time_round_delta;

                //2.需要记录一下当前时间,以及判断是否超过了uint的最大值
                lock (_locker)
                {
                    if (now < _last_time) //出现了溢出, 记录一下
                        _time_round_delta += uint.MaxValue;
                    _last_time = now;
                }

                //3. 返回
                return now + _time_round_delta;
            }
        }
    }

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
        private const long C_SEC_2_MILLI = 1000L;//1秒对应的 毫秒        

        private static long _local_dt = (DateTime.UtcNow.Ticks - C_TICK_DT_1970_0001) / C_MILLI_SEC_TICKS - SystemStartTimer.Now;

        private static long _svr_dt = 0;
        private static int _frame_count = 0;

        //这个由外部设置,为了给多线程用的
        public static int FrameCount => _frame_count;

        public static void SetFrameCount(int frameCount)
        {
            _frame_count = frameCount;
        }

        /// <summary>
        /// 获取系统开启到现在的时间，毫秒,用户不可修改, 线程安全的
        /// </summary>
        public static long SystemStartTime => SystemStartTimer.Now;

        /// <summary>
        /// 本地时间戳,毫秒
        /// </summary>
        //public static long UnixMilli { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return (DateTime.UtcNow.Ticks - C_TICK_DT_1970_0001) / C_MILLI_SEC_TICKS; } }
        public static long UnixMilli { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return _local_dt + SystemStartTimer.Now; } }


        /// <summary>
        /// 本地时间戳,秒
        /// </summary>
        public static int Unix { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return (int)(UnixMilli / 1000L); } }


        /// <summary>
        /// 服务器时间戳,秒
        /// </summary>        
        public static int SvrUnix
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (int)((UnixMilli + _svr_dt) / C_SEC_2_MILLI); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _svr_dt = value * C_SEC_2_MILLI - UnixMilli; }
        }

        /// <summary>
        /// 服务器时间戳, 毫秒
        /// </summary>
        public static long SvrUnixMilli
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return UnixMilli + _svr_dt; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _svr_dt = value - UnixMilli; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Loc2SvrMilli(long local_time)
        {
            if (local_time <= 0) local_time = 0;
            return local_time + _svr_dt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Loc2Svr(int local_time)
        {
            return (int)(Loc2SvrMilli(local_time * C_SEC_2_MILLI) / C_SEC_2_MILLI);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Svr2LocMilli(long svr_time)
        {
            if (svr_time <= 0) svr_time = 0;
            return svr_time - _svr_dt;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Svr2Loc(int svr_time)
        {
            return (int)(Svr2LocMilli(svr_time * C_SEC_2_MILLI) / C_SEC_2_MILLI);
        }
    }
}
