using System;
using System.Collections.Generic;


namespace FH
{
    public struct TimerId : IEquatable<TimerId>
    {
        public static TimerId InvalidTimerId = new TimerId(TimeWheelId.InvalidId);
        public static IEqualityComparer<TimerId> EqualityComparer = new TimerIdEqualityComparer();
        internal readonly int Id;
        internal TimerId(int id) { Id = id; }

        public bool IsValid()
        {
            return Id != 0;
        }

        public bool Equals(TimerId other)
        {
            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public override bool Equals(object obj)
        {
            if (obj is TimerId other)
                return Id == other.Id;
            return false;
        }


        private class TimerIdEqualityComparer : IEqualityComparer<TimerId>
        {
            public bool Equals(TimerId x, TimerId y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(TimerId obj)
            {
                return HashCode.Combine(obj.Id);
            }
        }
    }

    public interface ITimerDriver : ICPtr
    {
        IClock Clock { get; }
        TimerId AddTimer(Action<TimerId> cb, long delay_ms);
        void Update();
        bool Cancel(TimerId timer_id);
    }

    public static class TimerMgr
    {
        private static ITimerDriver _Driver;


        public static ITimerDriver CreateSimple(IClock clock)
        {
            ///*
            return new TimerDriver(
               clock,
               16, //间隔为16毫秒 
               new int[]
               {
                    64, // 间隔 16毫秒， 64个slot, ，16*64 = 1024毫秒，1秒以内
                    64, // 间隔 1024 (16 x 64) 毫秒，64个slot，16*64*64 = 65636毫秒，65秒，1分钟以内
                    64, // 间隔 65536 ( 1024 x 64) 毫秒，1分钟，64个slot，16*64*64*64 = 64分钟，1个小时
                    64, //  间隔 1个小时 (65536 x 64)，64个slot，16*64*64*64*64 = 64个小时，3天左右
                    32, // 间隔 3天左右，32个slot,16*64*64*64*64*32 = 时间跨度99天
                        //4,  //间隔99天，4个slot，时间跨度1年
               });
            //*/

            /*
             // 16 * 32^6 = 198天
            return new TimerDriver(
                clock,
                16, //间隔为16毫秒 
                 32, // 32 slots per wheel
                 6  // 6 wheels
                 ); 
            //*/
        }

        public static void Init()
        {
            IClock clock = new ClockDecorator(new ClockLocal());
            _Driver = CreateSimple(clock);
        }

        public static TimerId AddTimer(Action<TimerId> cb, long time_ms)
        {
            if (_Driver == null)
                return TimerId.InvalidTimerId;

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
#if UNITY_2023_2_OR_NEWER
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
#endif
    }
}
