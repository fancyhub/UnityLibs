/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13
 * Title   : 
 * Desc    : 
*************************************************************************************/


namespace FH
{
    /// <summary>
    /// 不支持 暂停,加速
    /// 如果需要,在上面套接一个
    /// </summary>
    public class ClockManual : IClock
    {
        public long Time;
        public ClockManual(long init_time)
        {
            Time = init_time;
        }

        public void SetTime(long time)
        {
            Time = time;
        }

        public void Scale(float scale)
        {
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
            return Time;
        }
    }
}
