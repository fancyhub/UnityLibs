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
    /// 基于服务器的时间,不可以暂停,不可以加速
    /// 毫秒
    /// 支持在子线程里面运行, 没有用到Unity的代码
    /// </summary>
    public sealed class ClockLocal : IClock
    {
        public int _frame_count;
        public bool _frame_update;
        public long _time;
        /// <summary>
        /// 是否基于frame 来更新
        /// </summary>        
        public ClockLocal(bool frame_update = false)
        {
            _frame_update = frame_update;
            _frame_count = -1;
            _time = 0;
        }

        /// <summary>
        /// 基于frame 来更新, 但是初始时间是给定的
        /// </summary>
        public ClockLocal(long init_time)
        {
            _frame_update = true;
            _frame_count = TimeUtil.FrameCount;
            //时间尽量只能往前增长
            _time = Math.Min(init_time, TimeUtil.UnixMilli);
        }

        public void Scale(float scale)
        {
            return;
        }

        public uint GetScale()
        {
            return IClock.ScaleOne;
        }

        public void Scale(uint scale)
        {
        }

        public void Pause()
        {
        }

        public bool IsPaused()
        {
            return false;
        }

        public void Resume()
        {
        }

        public long GetTime()
        {
            if (!_frame_update)
                return TimeUtil.UnixMilli;

            int frame_count = TimeUtil.FrameCount;
            if (_frame_count != frame_count)
            {
                _time = TimeUtil.UnixMilli;
                _frame_count = frame_count;
            }
            return _time;
        }
    }
}
