using System;
using System.Collections.Generic;


namespace FH
{
    public static class TimerMgr
    {
        private static ITimerDriver _Driver;


        public static ITimerDriver CreateSimple(IClock clock)
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

        public static void Init()
        {
            IClock clock = new ClockDecorator(new ClockLocal());
            _Driver = CreateSimple(clock);
        }

        public static TimerId AddTimer(Action<TimerId> cb, long time_ms)
        {
            if (_Driver == null)
                return TimerId.InvalidId;

            return _Driver.AddTimer(cb, time_ms);
        }

        public static void Update()
        {
            if (_Driver == null)
                return;

            _Driver.Update();
        }

        public static bool Cancel(TimerId timer_id)
        {
            if (_Driver == null)
                return false;

            return _Driver.Cancel(timer_id);
        }

        public static UnityEngine.Awaitable Wait(long time_ms)
        {
            if (_Driver == null)
                return default;

            
            UnityEngine.AwaitableCompletionSource source = new UnityEngine.AwaitableCompletionSource();
            _Driver.AddTimer((x) =>
            {
                source.TrySetResult();
            }, time_ms);

            return source.Awaitable;
        }
    }
}
