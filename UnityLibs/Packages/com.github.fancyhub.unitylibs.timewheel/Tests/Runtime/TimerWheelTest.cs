using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using System;
 
public class TimerWheelTest : MonoBehaviour
{
    public ITimerDriver _timer_driver;

    public CPtr<ITimer> _timer_repeater;
    public CPtr<ITimer> _timer_seq;

    public void Awake()
    {
        _timer_driver = TimerDriver.CreateNormal(new ClockDecorator(new ClockLocal(true)));
        //_timer_driver.Clock.Scale(5.0f);

        UpdateBehaviour t = this.gameObject.AddComponent<UpdateBehaviour>();
        t._timer_driver = _timer_driver;
    }

    public void OnEnable()
    {
        var timer = Timer.Create("Hello", (x) =>
        {
            string txt = x.UserData + " " + x.CurCount + " " + DateTime.Now.ToString();
            UnityEngine.Debug.Log(txt);
        }, _timer_driver);
        timer.Start(1000, 10);


        var timer_seq = TimerSeq.Create(_timer_driver);
        timer_seq
            .AddOnce(1000, (x) => { UnityEngine.Debug.Log($"TimeSeq Once,  {DateTime.Now}"); })
            .AddRepeat(1000, 3, (x) => { UnityEngine.Debug.Log($"TimeSeq Repeat,  {DateTime.Now}"); })
            .AddDelay(3000)
            .AddUpdate(30 * 1000, (x) => { UnityEngine.Debug.Log($"TimeSeq Update,  {DateTime.Now}"); });
        timer_seq.Start();

        _timer_repeater = timer;
        _timer_seq = timer_seq;
    }

    public void OnDisable()
    {
        _timer_repeater.Destroy();
        _timer_seq.Destroy();
    }

    public class UpdateBehaviour : MonoBehaviour
    {
        public ITimerDriver _timer_driver;

        public void Update()
        {
            _timer_driver.Update();
        }
    }
}
