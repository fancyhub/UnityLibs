/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{

    /// <summary>
    /// 基于的clock系统 <para/>
    /// 最终时间 = base_clock.Time * mul_factor / div_factor <para/>
    /// 可以暂停,可以加速     <para/>
    /// div_factor: 是为了解决 从 毫秒 -> 秒 的转换 <para/>
    /// mul_factor: 是为了解决 从 秒 -> 毫秒 的转换 <para/>
    /// </summary>
    public sealed class ClockDecorator : IClock
    {
        private const float CScaleInt2Float = 1.0f / IClock.ScaleOne;

        public struct ClockData
        {
            //开始记录时间的 时间戳
            public long _src_ts;

            //每次状态变化之后，就要改变
            public long _virtual_ts;

            //缩放值, 千分位
            public uint _scale;

            public void Init()
            {
                _scale = IClock.ScaleOne;
                _src_ts = 0;
                _virtual_ts = 0;
            }

            public void SetScale(uint scale, long now_time)
            {
                if (_scale == scale)
                    return;

                _virtual_ts = GetTime(now_time);
                _src_ts = now_time;
                _scale = scale;
            }

            public long GetTime(long now_time)
            {
                //如果 归到一起，可以不用区分是否是暂停状态
                //不过这种 暂停，或者缩放值为0 的情况下，可以少调用一次 获取时间戳
                if (_scale == 0)
                    return _virtual_ts;

                long dt = now_time - _src_ts;
                if (_scale != IClock.ScaleOne)
                    dt = (dt * _scale) / IClock.ScaleOne;
                return _virtual_ts + dt;
            }
        }

        public struct ClockPauseScale
        {
            public uint _scale;
            public bool _pause;

            public void Init()
            {
                _scale = IClock.ScaleOne;
                _pause = false;
            }

            public void Scale(uint scale)
            {
                _scale = scale;
            }

            public void Pause()
            {
                _pause = true;
            }

            public void Resume()
            {
                _pause = false;
            }

            public uint GetFinalScale()
            {
                if (_pause)
                    return 0;
                return _scale;
            }
        }

        public struct ClockTransformer
        {
            public int _mul_factor;
            public int _div_factor;

            public ClockTransformer(int mul_factor = 1,
                        int div_factor = 1)
            {
                _mul_factor = Math.Max(1, mul_factor);
                _div_factor = Math.Max(1, div_factor);
            }

            public long Transform(long time)
            {
                if (_mul_factor == _div_factor)
                    return time;
                return (time * _mul_factor) / _div_factor;
            }
        }


        public IClock _base_clock;
        public ClockPauseScale _pause_scale;
        public ClockData _data;
        public ClockTransformer _transformer;

        public ClockDecorator(IClock base_clock, int mul_factor = 1, int div_factor = 1)
        {
            _transformer = new ClockTransformer(mul_factor, div_factor);
            _base_clock = base_clock;
            _data = new ClockData();
            _pause_scale = new ClockPauseScale();
            _data.Init();
            _pause_scale.Init();
        }

        public uint GetScale()
        {
            return _pause_scale._scale;
        }


        public uint Scale
        {
            get => _pause_scale._scale;
            set
            {
                if (_pause_scale._scale == value)
                    return;

                _pause_scale.Scale(value);
                _data.SetScale(_pause_scale.GetFinalScale(), _base_clock.Time);
            }
        }

        public float ScaleFloat
        {
            get => (float)(_pause_scale._scale * CScaleInt2Float);
            set
            {
                Scale = (uint)(value * IClock.ScaleOne);
            }
        }
         

        public bool Pause
        {
            get
            {
                return _pause_scale._pause;
            }
            set
            {
                if (value == _pause_scale._pause)
                    return;

                if (value)
                {
                    _pause_scale.Pause();
                    _data.SetScale(_pause_scale.GetFinalScale(), _base_clock.Time);
                }
                else
                {
                    _pause_scale.Resume();
                    _data.SetScale(_pause_scale.GetFinalScale(), _base_clock.Time);
                }
            }
        }

        public long Time
        {
            get
            {
                long src_time = _base_clock.Time;
                long scaled_time = _data.GetTime(src_time);
                long ret = _transformer.Transform(scaled_time);
                return ret;
            }
        }
    }
}
