/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/8/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    public class TimerTest
    {
        public static void Test()
        {
            ITimerDriver local_driver = TimerDriver.CreateNormal(new Clock(new ClockLocal(false)));

            local_driver.Clock.Scale(5.0f);

            Timer<string> timer = Timer.Create("Hello", (x) =>
            {
                string txt = x.UserData + " " + x.CurCount + " " + DateTime.Now.ToString();
                UnityEngine.Debug.Log(txt);
            }, local_driver);

            timer.Start(1000, 40);

            for (; ; )
            {
                local_driver.Update();
            }
        }
    }
}
