/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using UnityEngine;


namespace FH
{
    /// <summary>
    /// Notice 的数据包括了队列数据和item，item是下层去实现的
    /// </summary>
    public sealed class NoticeData
    {
        public ENoticeChannel _channel;

        //优先级
        public int _priority;

        /// <summary>
        /// 显示的时间
        /// </summary>
        public int _duration_show;

        /// <summary>
        /// 多少毫秒之后,过期, 只在排队过程中有用,一旦进入显示队列, 就不需要了
        /// 如果<=0 说明永远不过期
        /// </summary>
        public int _duration_expire = 0;

        //该item被添加到队列的时间戳, 这个外部不需要修改, push的时候,自动设置的
        public long _add_time_ms;

        public long ExpireTime
        {
            get
            {
                if (_duration_expire <= 0)
                    return long.MaxValue;
                return _duration_expire + _add_time_ms;
            }
        }

        public INoticeItem _item;

        public void Destroy()
        {
            _item?.Destroy();
        }
    }
}
