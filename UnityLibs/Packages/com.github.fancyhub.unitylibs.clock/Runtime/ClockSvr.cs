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
    /// 服务器时间,不可以暂停,不可以加速
    /// 毫秒
    /// 支持在子线程里面运行, 没有用到Unity的代码
    /// </summary>
    public sealed class ClockSvr : IClock
    {
        public int _frame_count;
        public bool _frame_update;
        public long _time;

        /// <summary>
        /// 是否基于frame 来更新
        /// </summary>
        public ClockSvr(bool frame_update = false)
        {
            _frame_update = frame_update;
            _frame_count = -1;
            _time = 0;
        }

        /// <summary>
        /// 基于frame 来更新, 但是初始时间是给定的
        /// </summary>
        public ClockSvr(long init_time)
        {
            _frame_update = true;
            _frame_count = TimeUtil.FrameCount;
            //时间尽量只能往前增长
            _time = Math.Min(init_time, TimeUtil.SvrUnixMilli);
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
                if (!_frame_update)
                    return TimeUtil.SvrUnixMilli;

                int frame_count = TimeUtil.FrameCount;
                if (_frame_count != frame_count)
                {
                    _time = TimeUtil.SvrUnixMilli;
                    _frame_count = frame_count;
                }
                return _time;
            }
        }
    }
}
