/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    public enum ECoolDownTimer
    {
        Local,
        Server,
        UnityTime, //使用的是 UnityEngine.Time.time //收到暂停,timescale 影响
    }

    public struct CoolDownTimer
    {
        #region Creater
        public static CoolDownTimer CreateWithMillSec(long durationMs, ECoolDownTimer type = ECoolDownTimer.Local)
        {
            CoolDownTimer ret = new CoolDownTimer();
            ret._Type = type;
            ret._Duration = Math.Max(durationMs, 0);
            ret._EndTime = 0;
            return ret;
        }

        public static CoolDownTimer CreateWithSec(float durationSec, ECoolDownTimer type = ECoolDownTimer.Local)
        {
            CoolDownTimer ret = new CoolDownTimer();
            ret._Type = type;
            ret._Duration = Math.Max((long)(durationSec * 1000), 0);
            ret._EndTime = 0;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeStamp">毫秒者秒</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static CoolDownTimer CreateWithTimeStamp(long timeStamp, ECoolDownTimer type = ECoolDownTimer.Local)
        {
            switch (type)
            {
                default:
                case ECoolDownTimer.Local:
                case ECoolDownTimer.Server:
                    if (timeStamp < uint.MaxValue) //秒
                        timeStamp = timeStamp * 1000L;
                    break;
                case ECoolDownTimer.UnityTime:
                    //DoNothing
                    break;
            }
            
            CoolDownTimer ret = new CoolDownTimer();
            ret._Type = type;
            ret._EndTime = timeStamp;
            ret._Duration = Math.Max(timeStamp - _GetNowTime(type), 0L);
            return ret;
        }
        #endregion

        private ECoolDownTimer _Type;
        private long _EndTime;
        private long _Duration;

        public CoolDownTimer(ECoolDownTimer type, long endTime = 0, long duration = 0)
        {
            _Type = type;
            _EndTime = Math.Max(endTime, 0);
            _Duration = Math.Max(duration, 0);
        }

        public bool IsReady { get { return _EndTime <= _GetNowTime(); } }
        public long Duration { get { return _Duration; } set { _Duration = Math.Max(value, 0L); } }
        public long EndTime { get { return _EndTime; } set { _EndTime = Math.Max(value, 0L); } }
        public long RemainTime { get { return Math.Max(_EndTime - _GetNowTime(), 0); } }

        public void Start()
        {
            _EndTime = _GetNowTime() + _Duration;
        }

        public void Stop()
        {
            _EndTime = 0;
        }

        /// <summary>
        /// [0-1]
        /// 0: 说明刚刚启动
        /// 1: 说明ready
        /// </summary>
        public float Progress
        {
            get
            {
                var dt = _EndTime - _GetNowTime();
                if (dt <= 0)
                    return 1.0f;
                if (_Duration == 0)
                    return 0.0f;

                return 1.0f - UnityEngine.Mathf.Clamp01((float)((double)dt / _Duration));
            }
        }

        private long _GetNowTime()
        {
            return _GetNowTime(_Type);
        }

        private static long _GetNowTime(ECoolDownTimer type)
        {
            switch (type)
            {
                case ECoolDownTimer.Local:
                    return TimeUtil.UnixMilli;
                case ECoolDownTimer.Server:
                    return TimeUtil.SvrUnixMilli;
                case ECoolDownTimer.UnityTime:
                    return (long)(UnityEngine.Time.timeAsDouble * 1000);
                default:
                    return TimeUtil.UnixMilli;
            }
        }
    }
}
