/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/28
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;

namespace FH
{
    /// <summary>
    /// 只能在主线程里面运行
    /// </summary>
    public class ClockUnityTime : IClock
    {
        public const long C_SEC_2_MS = 1000;
        public EType _type;
        public enum EType
        {
            /// <summary>
            /// 启动到现在的时间, 每帧更新一次
            /// 受 scale 影响, 受Editor Pause 影响
            /// </summary>
            Time,

            /// <summary>
            /// 启动到现在的时间,每帧更新一次
            /// 不受scale影响, 受Editor Pause 影响
            /// </summary>
            UnscaledTime,

            /// <summary>
            /// fixed time, 启动到当前fixedupdate 的时间,每次FixedUpdate 更新一次
            /// 受scale 影响, 受 Editor Pause 影响
            /// </summary>
            FixedTime,

            /// <summary>
            /// 启动到现在 fixed time
            /// 不受 scale 影响, 受 Editor Pause 影响
            /// </summary>
            UnscaledFixedTime,

            /// <summary>
            /// 从游戏启动到现在的时间, 不受scale, 不受Pause 影响
            /// </summary>
            StartTime,

            FrameCount,
        }

        public ClockUnityTime(EType t)
        {
            _type = t;
        }

        public uint GetScale()
        {
            return IClock.ScaleOne;
        }

        public long GetTime()
        {
            switch (_type)
            {
                case EType.FixedTime:
                    return (long)(Time.fixedTimeAsDouble * C_SEC_2_MS);

                case EType.Time:
                    return (long)(Time.timeAsDouble * C_SEC_2_MS);

                case EType.UnscaledFixedTime:
                    return (long)(Time.fixedUnscaledTimeAsDouble * C_SEC_2_MS);

                case EType.UnscaledTime:
                    return (long)(Time.unscaledTimeAsDouble * C_SEC_2_MS);

                case EType.StartTime:
                    return (long)(Time.realtimeSinceStartupAsDouble * C_SEC_2_MS);

                case EType.FrameCount:
                    return Time.frameCount;
                    //return TimeUtil.FrameCount;

                default:
                    return 0;
            }
        }

        public bool IsPaused()
        {
            return false;
        }

        public void Pause()
        {

        }

        public void Resume()
        {

        }

        public void Scale(float scale)
        {

        }

        public void Scale(uint scale)
        {

        }
    }
}
