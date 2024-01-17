/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;

namespace FH
{
    /// <summary>
    /// 数据的频道
    /// 
    /// 这个类是用链表实现的，因为这里的数据没有排序，只有插入和删除
    /// </summary>
    public sealed class NoticeDataQueue
    {
        public LinkedList<NoticeData> _queue;
        public NoticeDataQueue()
        {
            _queue = new LinkedList<NoticeData>();
        }

        public int Count { get { return _queue.Count; } }

        public void Clear()
        {
            foreach (var p in _queue)
            {
                p.Destroy();
            }
            _queue.ExtClear();
        }

        /// <summary>
        /// 向数据队列中插入数据，根据优先级来判定
        /// </summary>
        public void Push(NoticeData data)
        {
            if (null == data)
            {
                NoticeLog.E("data is null, operation failed, plis check!");
                return;
            }

            //剔除掉重复的 notice，主要是针对common类型，避免弹太多太久太慢
            if (_TryMerge(_queue, data))
                return;

            int priority = data._priority;

            //从大大小排序
            var node = _queue.First;
            for (; node != null; node = node.Next)
            {
                if (node.Value._priority < priority)
                    break;
            }

            if (node != null)
                _queue.ExtAddBefore(node, data);
            else
                _queue.ExtAddLast(data);
        }

        /// <summary>
        /// 查看第一个数据
        /// </summary>
        public NoticeData Peek()
        {
            if (_queue.Count == 0)
                return null;
            return _queue.First.Value;
        }

        /// <summary>
        /// 弹出一个数据
        /// </summary>
        public NoticeData Pop(long time_now, int priority = int.MinValue)
        {
            for (; ; )
            {
                LinkedListNode<NoticeData> node = _queue.First;
                if (node == null)
                    return null;

                NoticeData ret = node.Value;
                if (ret._priority <= priority)
                    return null;

                _queue.ExtRemoveFirst();
                if (ret.ExpireTime > time_now)
                    return ret;
            }
        }

        /// <summary>
        /// 如果要退出游戏，这个时候应该清除所有的数据
        /// 调用的就是这个接口
        /// </summary>
        public void Destroy()
        {
            LinkedListNode<NoticeData> node = _queue.First;
            while (null != node)
            {
                node = node.Next;
            }
            _queue.ExtClear();
        }

        private bool _TryMerge(LinkedList<NoticeData> data_list, NoticeData new_data)
        {
            foreach (var data in data_list)
            {
                if (data._item.Merge(new_data._item))
                    return true;
            }
            return false;
        }
    }
}
