/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
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

        public void StripForMultiImmediate(int max_count, long now_time)
        {
            //1. 先删除过期的
            {
                var node = _queue.First;
                for (; ; )
                {
                    if (node == null)
                        break;
                    if (node.Value.IsExpire(now_time))
                    {
                        node.Value.Destroy();
                        var t = node.Next;
                        _queue.ExtRemove(node);
                        node = t;
                    }
                    else
                    {
                        node = node.Next;
                    }
                }
            }

            //2. 如果数量够了
            if (_queue.Count <= max_count || max_count <= 0)
                return;

            //3. 删除低优先级的            
            for (; ; )
            {
                //找到相同优先级的开始节点
                var start_node = _FindStartNodeWithSampePriority(_queue.Last);
                //删除相同优先级的, 从头开始删除, 也就是说, 后加的保留
                _RemoveNodes(start_node, max_count);

                //够了
                if (_queue.Count <= max_count)
                    return;
            }
        }

        /// <summary>
        /// 向数据队列中插入数据，根据优先级来判定
        /// 优先级大的在前面, 相同优先级, 后来的在后面
        /// </summary>
        public void Push(NoticeData data)
        {
            if (null == data.Item)
            {
                NoticeLog.E("data is null, operation failed, plis check!");
                return;
            }

            //剔除掉重复的 notice，主要是针对common类型，避免弹太多太久太慢
            if (_TryMerge(_queue, data))
                return;

            int priority = data.Priority;

            //从大到小排序
            var node = _queue.First;
            for (; node != null; node = node.Next)
            {
                if (node.Value.Priority < priority)
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
        public bool Peek(out NoticeData data)
        {
            data = default;
            if (_queue.Count == 0)
                return false;
            data = _queue.First.Value;
            return true;
        }

        // <summary>
        /// single立即模式下, 弹出一个数据
        /// </summary>        
        public bool PopSingleImmediate(out NoticeData out_data, long time_now, int priority)
        {
            out_data = default;
            bool ret = false;
            for (; ; )
            {
                if (!Peek(out var temp_peek_data))
                    break;
                if (temp_peek_data.Priority < priority)
                    break;

                Pop(out out_data, time_now, int.MinValue);
                priority = out_data.Priority;
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// 弹出一个数据
        /// </summary>
        public bool Pop(out NoticeData data, long time_now, int priority = int.MinValue)
        {
            data = default;
            for (; ; )
            {
                LinkedListNode<NoticeData> node = _queue.First;
                if (node == null)
                    return false;

                NoticeData ret = node.Value;
                if (ret.Priority <= priority)
                    return false;

                if (!ret.IsExpire(time_now))
                {
                    _queue.ExtRemove(node);
                    data = ret;
                    return true;
                }

                node.Value.Destroy();
                _queue.ExtRemove(node);
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

        private static bool _TryMerge(LinkedList<NoticeData> data_list, NoticeData new_data)
        {
            foreach (var data in data_list)
            {
                if (data.Item.TryMerge(new_data.Item))
                    return true;
            }
            return false;
        }

        private static void _RemoveNodes(LinkedListNode<NoticeData> node, int remain_max_count)
        {
            var list = node.List;
            for (; ; )
            {
                if (node == null || list.Count <= remain_max_count)
                    return;

                var t = node.Next;
                node.Value.Destroy();
                list.ExtRemove(node);
                node = t;
            }
        }

        private static LinkedListNode<NoticeData> _FindStartNodeWithSampePriority(LinkedListNode<NoticeData> end_node)
        {
            var start_node = end_node;
            int priority = end_node.Value.Priority;
            for (; ; )
            {
                var pre_node = start_node.Previous;
                if (pre_node == null)
                    break;
                if (pre_node.Value.Priority != priority)
                    break;
                start_node = pre_node;
            }
            return start_node;
        }
    }
}
