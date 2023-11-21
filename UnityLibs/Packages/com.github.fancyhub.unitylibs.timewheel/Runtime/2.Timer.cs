/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{

    public struct TimerData
    {
        public IClock _clock;

        public long _interval_ms;

        public int _repeat_count;

        public int _cur_count;
        public long _start_time;
        /// <summary>
        /// 如果这个时间 >0 说明是update 模式
        /// </summary>
        public long _expire_time;

        public void Start(IClock clock, long interval_ms, int repeat_count, long duration = 0)
        {
            _clock = clock;
            _interval_ms = interval_ms;
            _repeat_count = repeat_count;
            _cur_count = 0;

            _start_time = clock.GetTime();
            if (duration > 0)
                _expire_time = _start_time + duration;
            else
                _expire_time = 0;
        }

        public void Clear()
        {
            _clock = null;
            _interval_ms = 0;
            _repeat_count = 0;
            _cur_count = 0;
            _start_time = 0;
            _expire_time = 0;
        }

        public long GetStartTime()
        {
            return _start_time;
        }

        public int GetCurCount()
        {
            return _cur_count;
        }

        public long GetElapsedTime()
        {
            if (_clock == null)
                return 0;

            return _clock.GetTime() - _start_time;
        }

        /// <summary>
        /// Update 模式才有效果
        /// </summary>
        public long GetDuration()
        {
            return _expire_time - _start_time;
        }

        //true: 还需要继续
        //false: timer 结束
        public bool Next()
        {
            _cur_count++;

            if (_clock == null)
                return false;

            //如果次数到了,结束
            if (_repeat_count > 0 && _cur_count >= _repeat_count)
                return false;

            //如果总时间到了
            if (_expire_time > 0 && _expire_time < _clock.GetTime())
                return false;

            //如果次数和总时间都没结束, 那就是没有结束
            return true;
        }
    }


    public interface ITimer : ICPtr
    {
        public bool IsRunning { get; }

        /// <summary>
        /// 经过的时间
        /// </summary>
        public long ElapsedTime { get; }


        public void Stop();
    }

    public sealed class Timer : CPoolItemBase, ITimer
    {
        public TimerId _timer_id;
        public Action<Timer> _call_back;
        public ITimerDriver _driver;

        public TimerData _data;

        /// <summary>
        /// driver = null 的时候,挂在默认的 local driver 上
        /// </summary>        
        public static Timer Create(Action<Timer> call_back, ITimerDriver driver)
        {
            if (call_back == null)
                return null;

            if (driver == null)
                return null;
            var ret = GPool.New<Timer>(() => new Timer());
            ret._driver = driver;
            ret._call_back = call_back;
            return ret;
        }

        /// <summary>
        /// driver = null 的时候,挂在默认的 local driver 上
        /// </summary>        
        public static Timer<T> Create<T>(T user_data, Action<Timer<T>> call_back, ITimerDriver driver)
        {
            if (call_back == null)
                return null;

            return Timer<T>.Create(user_data, call_back, driver);
        }

        private Timer()
        {
        }

        public bool IsRunning { get { return _timer_id.IsValid(); } }

        /// <summary>
        /// 经过的时间
        /// </summary>
        public long ElapsedTime { get { return _data.GetElapsedTime(); } }

        public Action<Timer> CallBack
        {
            get { return _call_back; }
        }

        public int CurCount { get { return _data.GetCurCount(); } }

        /// <summary>       
        /// repeat_count &lt;= 0 无限循环  &gt;0 循环次数 <br/>
        /// duration_ms >0, 如果总时间或者循环次数达到条件, 结束timer
        /// </summary>        
        public bool Start(long interval_ms, int repeat_count, long duration_ms = 0)
        {
            if (_timer_id.IsValid())
                return false;
            if (interval_ms < 0)
                return false;
            if (_driver == null)
                return false;
            if (_call_back == null)
                return false;

            _data.Start(_driver.Clock, interval_ms, repeat_count, duration_ms);
            _timer_id = _driver.AddTimer(_on_time_out, interval_ms);
            return true;
        }

        public void Stop()
        {
            if (!_timer_id.IsValid())
                return;
            if (_driver == null)
                return;
            _driver.Cancel(_timer_id);
            _timer_id = TimerId.InvalidId;
        }

        protected override void OnPoolRelease()
        {
            if (_call_back == null)
                return;

            Stop();
            _driver = null;
            _call_back = null;
            _data.Clear();
        }

        public void _on_time_out(TimerId timer_id)
        {
            _timer_id = TimerId.InvalidId;

            if (_data.Next())
            {
                _timer_id = _driver.AddTimer(_on_time_out, _data._interval_ms);
            }

            CPtr<ICPtr> timer_ptr = this;
            _call_back?.Invoke(this);
            if (!_timer_id.IsValid())
                timer_ptr.Destroy();
        }
    }

    public sealed class Timer<T> : CPoolItemBase, ITimer
    {
        public TimerId _timer_id;
        public T _user_data;
        public Action<Timer<T>> _call_back;
        public TimerData _data;
        public ITimerDriver _driver;

        public static Timer<T> Create(T user_data, Action<Timer<T>> call_back, ITimerDriver driver)
        {
            if (call_back == null)
                return null;
            if (driver == null)
                return null;
            var ret = GPool.New<Timer<T>>(() => new Timer<T>());
            ret._driver = driver;
            ret._call_back = call_back;
            ret.UserData = user_data;
            return ret;
        }

        private Timer()
        {
        }

        public T UserData
        {
            get { return _user_data; }
            set { _user_data = value; }
        }

        public int CurCount { get { return _data.GetCurCount(); } }

        /// <summary>
        /// 经过的时间
        /// </summary>
        public long ElapsedTime { get { return _data.GetElapsedTime(); } }


        public Action<Timer<T>> CallBack
        {
            get { return _call_back; }
        }
        public bool IsRunning { get { return _timer_id.IsValid(); } }


        /// <summary>       
        /// repeat_count &lt;= 0 无限循环  &gt;0 循环次数 <br/>
        /// duration_ms >0, 如果总时间或者循环次数达到条件, 结束timer
        /// </summary>        
        public bool Start(long interval_ms, int repeat_count, long duration_ms = 0)
        {
            if (_timer_id.IsValid())
                return false;
            if (interval_ms < 0)
                return false;
            if (_driver == null)
                return false;
            if (_call_back == null)
                return false;

            _data.Start(_driver.Clock, interval_ms, repeat_count, duration_ms);
            _timer_id = _driver.AddTimer(_on_time_out, interval_ms);
            return true;
        }

        public void Stop()
        {
            if (!_timer_id.IsValid())
                return;
            if (_driver == null)
                return;
            _driver.Cancel(_timer_id);
            _timer_id = TimerId.InvalidId;
        }

        protected override void OnPoolRelease()
        {
            Stop();
            _driver = null;
            _call_back = null;
            _user_data = default;
            _data.Clear();
        }

        public void _on_time_out(TimerId timer_id)
        {
            _timer_id = TimerId.InvalidId;

            if (_data.Next())
            {
                _timer_id = _driver.AddTimer(_on_time_out, _data._interval_ms);
            }

            CPtr<ICPtr> timer_ptr = this;
            _call_back?.Invoke(this);
            if (!_timer_id.IsValid())
                timer_ptr.Destroy();
        }
    }


    public sealed class TimerSeq : CPoolItemBase, ITimer
    {
        private struct InnerData
        {
            public long _interval_ms;
            public int _repeat_count;
            public long _duration_ms;
            public Action<TimerSeq> _call_back;
        }

        private LinkedList<InnerData> _link_list = new LinkedList<InnerData>();
        private bool _is_infinite = false; //是否已经无限了
        private long _start_time_ms;

        private CPtr<Timer> _cur_timer;
        private Action<TimerSeq> _cur_call_back;
        private ITimerDriver _timer_driver;

        public static TimerSeq Create(ITimerDriver driver)
        {
            if (driver == null)
                return null;
            var ret = GPool.New<TimerSeq>(() => new TimerSeq());
            ret._timer_driver = driver;
            return ret;
        }

        private TimerSeq() { }

        public TimerSeq AddDelay(long interval_ms)
        {
            _Add(null, interval_ms, 1, 0);
            return this;
        }

        public TimerSeq AddOnce(long interval_ms, Action<TimerSeq> call_back)
        {
            _Add(call_back, interval_ms, 1, 0);
            return this;
        }

        public TimerSeq AddRepeat(long interval_ms, int repeat_count, Action<TimerSeq> call_back)
        {
            _Add(call_back, interval_ms, repeat_count, 0);
            return this;
        }

        public TimerSeq AddUpdate(long duration, Action<TimerSeq> call_back)
        {
            _Add(call_back, 0, 0, duration);
            return this;
        }

        public bool IsRunning { get { return !_cur_timer.Null; } }

        /// <summary>
        /// 经过的时间
        /// </summary>
        public long ElapsedTime
        {
            get
            {
                if (_timer_driver == null || _timer_driver.Clock == null)
                    return 0;
                return _timer_driver.Clock.GetTime() - _start_time_ms;
            }
        }

     
        public void Start()
        {
            if (!_cur_timer.Null)
                return;
            if (_link_list.Count == 0)
                return;

            _start_time_ms = _timer_driver.Clock.GetTime();
            _link_list.ExtPopFirst(out var data);
            _cur_call_back = data._call_back;
            _cur_timer = Timer.Create(_OnTimerOut, _timer_driver);
            _cur_timer.Val.Start(0, data._repeat_count, data._duration_ms);
        }

        public void Stop()
        {
            _cur_timer.Destroy();
            _cur_call_back = null;
            _link_list.ExtClear();
            _is_infinite = false;
        }

        protected override void OnPoolRelease()
        {
            if (_timer_driver == null)
                return;
            _timer_driver = null;
            Stop();
        }

        private bool _Add(Action<TimerSeq> call_back, long interval_ms, int repeat_count, long duration_ms)
        {
            if (!_cur_timer.Null)
            {
                Log.Assert(false, "已经开始运行了,不能添加");
                return false;
            }

            _link_list.ExtAddLast(new InnerData()
            {
                _interval_ms = interval_ms,
                _repeat_count = repeat_count,
                _call_back = call_back,
                _duration_ms = duration_ms
            });

            if (repeat_count <= 0 && duration_ms <= 0)
                _is_infinite = true;
            return true;
        }


        private void _OnTimerOut(Timer t)
        {
            //开始下一个Timer
            Action<TimerSeq> action = _cur_call_back;
            if (!t.IsRunning)
            {
                _cur_timer.Destroy();
                _cur_call_back = null;


                if (_link_list.ExtPopFirst(out var data))
                {
                    _cur_call_back = data._call_back;
                    _cur_timer = Timer.Create(_OnTimerOut, _timer_driver);
                    _cur_timer.Val.Start(data._interval_ms, data._repeat_count, data._duration_ms);
                }
            }

            //调用回调
            CPtr<ICPtr> timer_ptr = this;
            action?.Invoke(this);
            if (_cur_timer.Val == null)
                timer_ptr.Destroy();
        }
    }
}
