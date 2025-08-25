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
    public static class TimeUtil
    {
        private const long C_SEC_2_MILLI = 1000L;//1秒对应的 毫秒        

        #region Unity FrameCount
        private static int _frame_count = 0;
        //这个由外部设置,为了给多线程用的
        public static int FrameCount => _frame_count;
        public static void SetFrameCount(int frameCount) { _frame_count = frameCount; }
        #endregion

        #region System Start Time
        internal static class SystemBootTime
        {
            /// <summary>
            /// 获取系统启动时间, 通过 Environment.TickCount
            /// </summary>
            private static class EnvironmentTick32
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
                            now = (uint)Environment.TickCount;
                            dt = now - _last_time;
                            if (dt > CThresholdMS)
                            {
                                _last_time = now;
                            }
                            else if (dt < 0)
                            {
                                _time_round_delta += uint.MaxValue;
                                _last_time = now;
                            }
                        }
                        //3. 返回
                        return now + _time_round_delta;
                    }
                }
            }


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            [System.Runtime.InteropServices.DllImport("kernel32.dll")]
            private static extern ulong GetTickCount64();
            public static long Now { get { return (long)GetTickCount64(); } }



#elif UNITY_EDITOR_OSX
            private const int CLOCK_MONOTONIC = 6;

            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
            private struct timespec
            {
                public long tv_sec;
                public long tv_nsec;
            }

            
            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern int clock_gettime(int clockId, out timespec tp);

            public static long Now
            {
                get
                { 
                    if (clock_gettime(CLOCK_MONOTONIC, out timespec tp) == 0)
                        return tp.tv_sec * 1000 + tp.tv_nsec / 1000000;       
                    return 0; 
                }
            }
#elif  UNITY_IOS || UNITY_STANDALONE_OSX

            [System.Runtime.InteropServices.DllImport("__Internal")]
            private static extern long clock_gettime_ios();

            public static long Now
            {
                get
                { 
                    return clock_gettime_ios();                    
                }
            }
#elif UNITY_STANDALONE_LINUX || UNITY_ANDROID
            private const int CLOCK_BOOTTIME = 7;    
            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
            private struct timespec
            {
                public IntPtr tv_sec;   
                public IntPtr tv_nsec;
            }

            [System.Runtime.InteropServices.DllImport("c")]
            private static extern int clock_gettime(int clockId, out timespec tp);

            public static long Now
            {
                get
                {
                    if (clock_gettime(CLOCK_BOOTTIME, out timespec tp) != 0)
                        return 0;

                    long sec = tp.tv_sec.ToInt64();
                    long nsec = tp.tv_nsec.ToInt64();
                    return sec * 1000 + nsec / 1000000;
                }
            }
#else
            public static long Now
            {
                get
                { 
                    return EnvironmentTick32.Now;
                }
            }
#endif
        }



        /// <summary>
        /// 获取系统开启到现在的时间，毫秒,用户不可修改, 线程安全的
        /// </summary>
        public static long SystemStartTime => SystemBootTime.Now;
#endregion

        #region Local time
        private static readonly long _local_dt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - SystemBootTime.Now;
        /// <summary>
        /// 本地时间戳,毫秒
        /// </summary>
        //public static long UnixMilli { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); } }
        public static long UnixMilli { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return SystemBootTime.Now + _local_dt; } }


        /// <summary>
        /// 本地时间戳,秒
        /// </summary>
        public static int Unix { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return (int)(UnixMilli / C_SEC_2_MILLI); } }

        #endregion

        #region Server Time
        private static long _svr_dt = 0;
        /// <summary>
        /// 服务器时间戳,秒
        /// </summary>        
        public static int SvrUnix
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (int)(SvrUnixMilli / C_SEC_2_MILLI); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { SvrUnixMilli = value * C_SEC_2_MILLI; }
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
        #endregion

        #region Local & Server Time Converter
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Loc2SvrMilli(long local_time_milli)
        {
            if (local_time_milli <= 0) local_time_milli = 0;
            return local_time_milli + _svr_dt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Svr2LocMilli(long svr_time_milli)
        {
            if (svr_time_milli <= 0) svr_time_milli = 0;
            return svr_time_milli - _svr_dt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Loc2Svr(int local_time)
        {
            return (int)(Loc2SvrMilli(local_time * C_SEC_2_MILLI) / C_SEC_2_MILLI);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Svr2Loc(int svr_time)
        {
            return (int)(Svr2LocMilli(svr_time * C_SEC_2_MILLI) / C_SEC_2_MILLI);
        }
        #endregion
    }
}
