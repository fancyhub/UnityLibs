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


    internal class TimerDriver : CPtrBase, ITimerDriver
    {
        private TimerWheelGroup<Action<TimerId>> _time_wheel;
        private List<TimerWheelItem<Action<TimerId>>> _temp_list;
        private IClock _clock;


        public TimerDriver(IClock clock, int min_interval, int[] wheel_slots)
        {
            _clock = clock;
            _temp_list = new List<TimerWheelItem<Action<TimerId>>>();

            _time_wheel = TimerWheelGroup.Create<Action<TimerId>>(_clock.Time, min_interval, wheel_slots);
        }

        public TimerDriver(IClock clock, int min_interval, int slots_per_wheel, int wheel_count)
        {
            _clock = clock;
            _temp_list = new List<TimerWheelItem<Action<TimerId>>>();

            int[] wheel_slots = new int[wheel_count];
            for (int i = 0; i < wheel_count; i++)
                wheel_slots[i] = slots_per_wheel;
            _time_wheel = TimerWheelGroup.Create<Action<TimerId>>(_clock.Time, min_interval, wheel_slots);
        }

        public IClock Clock { get => _clock; }

        public TimerId AddTimer(Action<TimerId> cb, long delay_ms)
        {
            if (delay_ms < 0)
                return TimerId.InvalidTimerId;

            long time_exipire = _clock.Time + delay_ms;

            int timer_id = _time_wheel.AddTimer(time_exipire, cb);
            return new TimerId(timer_id);
        }

        public bool Cancel(ref TimerId timer_id)
        {
            bool ret = Cancel(timer_id);
            timer_id = TimerId.InvalidTimerId;
            return ret;
        }

        public bool Cancel(TimerId timer_id)
        {
            if (!timer_id.IsValid())
                return false;
            return _time_wheel.CancelTimer(timer_id.Id);
        }

        public void Update()
        {
            _temp_list.Clear();
            _time_wheel.Tick(_clock.Time, _temp_list);
            foreach (var p in _temp_list)
                p.UserData(new TimerId(p.Id));
            _temp_list.Clear();
        }

        protected override void OnRelease()
        {
        }
    }
}
