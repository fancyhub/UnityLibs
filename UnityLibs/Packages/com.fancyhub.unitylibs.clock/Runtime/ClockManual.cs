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
    public sealed class ClockManual : IClock
    {
        private long _Time;
        public ClockManual(long init_time)
        {
            _Time = init_time;
        }

        public uint Scale
        {
            get => IClock.ScaleOne;
            set { }
        }

        public float ScaleFloat
        {
            get => 1.0f;
            set { }
        }
         
        public bool Pause
        {
            get { return false; }
            set { }
        }

        public long Time
        {
            get { return _Time; }
            set { _Time = value; }
        }
    }
}
