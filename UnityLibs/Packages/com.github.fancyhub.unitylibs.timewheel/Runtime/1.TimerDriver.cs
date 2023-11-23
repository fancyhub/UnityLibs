/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;


namespace FH
{
    public interface ITimerDriver
    {
        IClock Clock { get; }
        TimerId AddTimer(Action<TimerId> cb, long delay_ms);
        void Update();
        bool Cancel(TimerId timer_id);
    }

    public class TimerDriver : ITimerDriver
    {
        public TimerWheelGroup _time_wheel;
        public List<TimerWheelItem> _temp_list;
        public Dictionary<TimerId, Action<TimerId>> _dict;
        public IClock _clock;

        public static TimerDriver CreateNormal(IClock clock)
        {
            return new TimerDriver(
                clock,
                16, //间隔为16毫秒 
                new int[]
                {
                    64, // 间隔 16毫秒， 64个轮, ，1024毫秒，1秒以内
                    64, // 间隔 1024 (16 x 64) 毫秒，64个轮，65636毫秒，65秒，1分钟以内
                    64, // 间隔 65536 ( 1024 x 64) 毫秒，1分钟，64个轮，64分钟，1个小时
                    64, //  间隔 1个小时 (65536 x 64)，64个轮，64个小时，3天左右
                    32, // 间隔 3天左右，32个轮,时间跨度90天
                    // 8,  //间隔半年，16个轮，时间跨度4年
                });
        }

        public TimerDriver(IClock clock, int min_interval, int[] wheel_counts)
        {
            _clock = clock;
            _temp_list = new List<TimerWheelItem>();
            _dict = new Dictionary<TimerId, Action<TimerId>>(TimerId.EqualityComparer);

            _time_wheel = TimerWheelGroup.Create(_clock.GetTime(), min_interval, wheel_counts);
        }

        public IClock Clock { get => _clock; }

        public TimerId AddTimer(Action<TimerId> cb, long delay_ms)
        {
            if (delay_ms < 0)
                return TimerId.InvalidId;

            long time_exipire = _clock.GetTime() + delay_ms;

            TimerId timer_id = _time_wheel.AddTimer(time_exipire);

            if (timer_id != TimerId.InvalidId)
                _dict.Add(timer_id, cb);

            return timer_id;
        }

        public bool Cancel(ref TimerId timer_id)
        {
            bool ret = Cancel(timer_id);
            timer_id = TimerId.InvalidId;
            return ret;
        }

        public bool Cancel(TimerId timer_id)
        {
            if (timer_id == TimerId.InvalidId)
                return false;

            bool ret1 = _time_wheel.CancelTimer(timer_id);
            bool ret2 = _dict.Remove(timer_id);
            return ret1 && ret2;
        }

        public void Update()
        {
            _time_wheel.Tick(_clock.GetTime(), _temp_list);
            for (int i = 0; i < _temp_list.Count; ++i)
            {
                TimerId timer_id = _temp_list[i].Id;

                _dict.TryGetValue(timer_id, out Action<TimerId> cb);
                _dict.Remove(timer_id);
                if (cb == null)
                    continue;

                cb(timer_id);
            }
        }
    }
}
