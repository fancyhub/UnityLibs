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
    public sealed class ClockUnityTime : IClock
    {
        public const long C_SEC_2_MS = 1000;
        private long _StartTime;
        private EType _Type;
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

        public ClockUnityTime(EType t, bool from_zero = false)
        {
            _Type = t;
            if (from_zero)
                _StartTime = _GetTime(t);
            else
                _StartTime = 0;
        }

        public uint Scale
        {
            get => IClock.ScaleOne;
            set { }
        }

        public float ScaleFloat
        {
            get => 1.0f;
            set { }
        }

        public bool Pause
        {
            get { return false; }
            set { }
        }

        public long Time
        {
            get
            {
                return _GetTime(_Type) - _StartTime;
            }
        }

        private static long _GetTime(EType type)
        {
            switch (type)
            {
                case EType.FixedTime:
                    return (long)(UnityEngine.Time.fixedTimeAsDouble * C_SEC_2_MS);

                case EType.Time:
                    return (long)(UnityEngine.Time.timeAsDouble * C_SEC_2_MS);

                case EType.UnscaledFixedTime:
                    return (long)(UnityEngine.Time.fixedUnscaledTimeAsDouble * C_SEC_2_MS);

                case EType.UnscaledTime:
                    return (long)(UnityEngine.Time.unscaledTimeAsDouble * C_SEC_2_MS);

                case EType.StartTime:
                    return (long)(UnityEngine.Time.realtimeSinceStartupAsDouble * C_SEC_2_MS);

                case EType.FrameCount:
                    return UnityEngine.Time.frameCount;
                //return TimeUtil.FrameCount;

                default:
                    return 0;
            }
        }
    }

    public struct ClockUnityTimeScaleable : IClock
    {
        public readonly ClockUnityTime BaseClock;
        private readonly ClockDecorator.ClockData _data;
        private ClockDecorator.ClockPauseScale _pause_scale;
        private ClockDecorator.ClockTransformer _transformer;

        public ClockUnityTimeScaleable(ClockUnityTime.EType type, bool from_zero = false, uint mul_factor = 1, uint div_factor = 1)
        {
            this.BaseClock = new ClockUnityTime(type, from_zero);
            this._data = ClockDecorator.ClockData.Default;
            this._pause_scale = ClockDecorator.ClockPauseScale.Default;
            this._transformer = new ClockDecorator.ClockTransformer(mul_factor, div_factor);
        }


        public uint Scale
        {
            get => _pause_scale.Scale;
            set
            {
                uint v = System.Math.Max(value, 0);
                if (_pause_scale.Scale == v)
                    return;
                _pause_scale.Scale = v;
                _data.SetScale(_pause_scale.GetFinalScale(), BaseClock.Time);
            }
        }

        public float ScaleFloat
        {
            get => (float)(_pause_scale.Scale * ClockDecorator.CScaleInt2Float);
            set
            {
                Scale = (uint)(value * IClock.ScaleOne);
            }
        }

        public bool Pause
        {
            get
            {
                return _pause_scale.Pause;
            }
            set
            {
                if (value == _pause_scale.Pause)
                    return;
                _pause_scale.Pause = value;

                if (value)
                {
                    _data.SetScale(_pause_scale.GetFinalScale(), BaseClock.Time);
                }
                else
                {
                    _data.SetScale(_pause_scale.GetFinalScale(), BaseClock.Time);
                }
            }
        }

        public long Time
        {
            get
            {
                long src_time = BaseClock.Time;
                long scaled_time = _data.GetTime(src_time);
                long ret = _transformer.Transform(scaled_time);
                return ret;
            }
        }
    }
}
