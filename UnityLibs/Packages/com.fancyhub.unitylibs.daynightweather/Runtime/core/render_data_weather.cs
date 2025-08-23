/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/11/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;

namespace FH.DayNightWeather
{

    public struct DataSlotNodeVal
    {
        public int _id;
        public int _val; //优先级 或者 time
        public IRenderDataSlot _data;
    }

    /// <summary>
    /// 按天气分组
    /// </summary>
    public class RenderData_Weather
    {
        public Dictionary<RenderSlotKey, LinkedList<DataSlotNodeVal>> _dict;

        public RenderDataSlotGroup _dst_group;

        public RenderData_Weather()
        {
            _dict = new Dictionary<RenderSlotKey, LinkedList<DataSlotNodeVal>>(RenderSlotKey.EqualityComparer);
            _dst_group = new RenderDataSlotGroup();
        }

        public RenderDataSlotGroup Calc(int time)
        {
            _dst_group.ClearSlotData();

            foreach (var p in _dict)
            {
                var list = p.Value;
                _find_2(list, time, out var node_pre, out var node_next, out float t);
                if (node_pre == null)
                    continue;

                _dst_group.SetLerp(p.Key, node_pre.Value._data, node_next.Value._data, t);
            }
            return _dst_group;
        }

        public LinkedListNode<DataSlotNodeVal> Add(RenderSlotKey key, DataSlotNodeVal val)
        {
            _dict.TryGetValue(key, out var list);
            if (list == null)
            {
                list = new LinkedList<DataSlotNodeVal>();
                _dict.Add(key, list);
            }

            LinkedListNode<DataSlotNodeVal> node_pre = _find_less(list, val._val);
            if (node_pre == null)
                return list.ExtAddFirst(val);
            return list.ExtAddAfter(node_pre, val);
        }

        public static void _find_2(
            LinkedList<DataSlotNodeVal> list,
            int time_of_day,    //分钟
            out LinkedListNode<DataSlotNodeVal> pre,
            out LinkedListNode<DataSlotNodeVal> next,
            out float t)
        {
            //如果一个都没有, 都为空
            if (list.Count == 0)
            {
                pre = null;
                next = null;
                t = 0;
                return;
            }

            //如果只有一个, 前后都是相同的, 并且t =0
            if (list.Count == 1)
            {
                pre = list.First;
                next = pre;
                t = 0;
                return;
            }

            //找到当前时间的前面一个
            pre = _find_less(list, time_of_day);
            if (pre == null) //找不到, 就找最后一个,是一个环
            {
                pre = list.Last;
                next = list.First;
            }
            else //
            {
                next = pre.Next;
                if (next == null) //如果没有下一个, 是一个环, 就是头
                    next = list.First;
            }

            t = RenderTimeUtil.CalcPercent(pre.Value._val, next.Value._val, time_of_day);
        }

        //找到  time_of_day 的节点
        public static LinkedListNode<DataSlotNodeVal> _find_less(LinkedList<DataSlotNodeVal> list, int time_of_day)
        {
            var node = list.Last;
            for (; ; )
            {
                if (node == null)
                    return null;

                if (node.Value._val <= time_of_day)
                    return node;
                node = node.Previous;
            }
        }
    }


    /// <summary>
    /// 按照优先级
    /// </summary>
    public class RenderData_Priority
    {
        public Dictionary<RenderSlotKey, LinkedList<DataSlotNodeVal>> _dict;

        public RenderDataSlotGroup _dst_group;

        public RenderData_Priority()
        {
            _dict = new Dictionary<RenderSlotKey, LinkedList<DataSlotNodeVal>>(RenderSlotKey.EqualityComparer);
            _dst_group = new RenderDataSlotGroup();
        }

        public RenderDataSlotGroup Calc()
        {
            _dst_group.Clear();

            foreach (var p in _dict)
            {
                var list = p.Value;
                var last_node = list.Last;
                if (last_node == null || last_node.Value._data == null)
                    continue;

                _dst_group.Set(p.Key, last_node.Value._data);
            }
            return _dst_group;
        }

        public LinkedListNode<DataSlotNodeVal> Add(RenderSlotKey key, DataSlotNodeVal val)
        {
            _dict.TryGetValue(key, out var list);
            if (list == null)
            {
                list = new LinkedList<DataSlotNodeVal>();
                _dict.Add(key, list);
            }

            LinkedListNode<DataSlotNodeVal> node_pre = _find_less(list, val._val);
            if (node_pre == null)
                return list.ExtAddFirst(val);
            return list.ExtAddAfter(node_pre, val);
        }

        //找到  time_of_day 的节点
        public static LinkedListNode<DataSlotNodeVal> _find_less(LinkedList<DataSlotNodeVal> list, int time_of_day)
        {
            var node = list.Last;
            for (; ; )
            {
                if (node == null)
                    return null;

                if (node.Value._val <= time_of_day)
                    return node;
                node = node.Previous;
            }
        }
    }

}
