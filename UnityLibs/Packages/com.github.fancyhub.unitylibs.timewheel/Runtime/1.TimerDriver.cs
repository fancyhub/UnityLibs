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

    internal class TimerDriver : ITimerDriver
    {
        internal TimerWheelGroup _time_wheel;
        internal List<TimerWheelItem> _temp_list;
        public Dictionary<TimerId, Action<TimerId>> _dict;
        public IClock _clock;
 

        public TimerDriver(IClock clock, int min_interval, int[] wheel_counts)
        {
            _clock = clock;
            _temp_list = new List<TimerWheelItem>();
            _dict = new Dictionary<TimerId, Action<TimerId>>(TimerId.EqualityComparer);

            _time_wheel = TimerWheelGroup.Create(_clock.Time, min_interval, wheel_counts);
        }

        public IClock Clock { get => _clock; }

        public TimerId AddTimer(Action<TimerId> cb, long delay_ms)
        {
            if (delay_ms < 0)
                return TimerId.InvalidId;

            long time_exipire = _clock.Time + delay_ms;

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
            _time_wheel.Tick(_clock.Time, _temp_list);
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
