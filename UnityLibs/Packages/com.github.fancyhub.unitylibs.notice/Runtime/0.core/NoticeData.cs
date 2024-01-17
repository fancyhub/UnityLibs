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
    public struct NoticeData
    {
        public readonly ENoticeChannel Channel;

        //优先级
        public readonly int Priority;

        /// <summary>
        /// 显示的时间, ms
        /// </summary>
        public readonly int DurationShow;

        /// <summary>
        /// 多少毫秒之后,过期, 只在排队过程中有用,一旦进入显示队列, 就不需要了
        /// 如果<=0 说明永远不过期
        /// </summary>
        public readonly int DurationExpire;

        //该item被添加到队列的时间戳, 这个外部不需要修改, push的时候,自动设置的
        internal readonly long AddTimeStampMs;

        private INoticeItem _item;

        public NoticeData(ENoticeChannel channel, float duration, int priority = 0, int duration_expire = 0)
        {
            Channel = channel;
            Priority = priority;
            DurationExpire = duration_expire;
            DurationShow = (int)(duration * 1000);
            AddTimeStampMs = 0;
            _item = null;
        }

        internal NoticeData(NoticeData orig, INoticeItem item, long now_time)
        {
            Channel = orig.Channel;
            Priority = orig.Priority;
            DurationExpire = orig.DurationExpire;
            DurationShow = orig.DurationShow;
            AddTimeStampMs = now_time;
            _item = item;
        }

        public INoticeItem Item => _item;

        public bool IsExpire(long now_time)
        {
            if (DurationExpire <= 0)
                return false;
            return (now_time - AddTimeStampMs) > DurationExpire;
        }

        public void Destroy()
        {
            _item?.Destroy();
            _item = null;
        }
    }
}
