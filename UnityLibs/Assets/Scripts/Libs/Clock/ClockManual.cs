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
        public long _time;
        public ClockManual(long init_time)
        {
            _time = init_time;
        }

        public void SetTime(long time)
        {
            _time = time;
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
            return _time;
        }
    }
}
