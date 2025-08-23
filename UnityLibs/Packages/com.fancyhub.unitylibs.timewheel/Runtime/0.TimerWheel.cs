/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    internal static class TimeWheelId
    {
        private static int S_ID_GEN = 0;
        internal const int InvalidId = 0;
        public static int NextId => ++S_ID_GEN;
    }

    //Time Id, 里面含有id 和过期时间
    internal struct TimerWheelItem<T>
    {
        public readonly int Id;
        public readonly long Time;
        public T UserData;
        public TimerWheelItem(long time, T user_data)
        {
            Id = TimeWheelId.NextId;
            Time = time;
            UserData = user_data;
        }
    }


    //一个时间轮，里面包含了一组 Slots，添加的time 有上限和下限
    // TimeStamp => idx 的计算方法 idx = (TimeStamp >>_interval_shift) + _idx_offset
    // idx => slot_idx 的计算方法 slot_idx = idx & _slot_idx_mask
    internal class TimerWheel<T>
    {
        public long _cur_idx;
        public long _slot_idx_mask;
        public int _idx_offset; //根据时间搓算出的index 要加上这个偏移，要么0要么1
        public byte _interval_shift; //间隔, 是一个移位操作的数量 
        public LinkedList<TimerWheelItem<T>>[] _slots;

        public TimerWheel(long init_time, byte interval_bit_shift, byte slot_count_shift, bool first_wheel)
        {
            //1. 初始化
            _idx_offset = first_wheel ? 0 : 1;
            _interval_shift = interval_bit_shift;
            _slot_idx_mask = (1L << slot_count_shift) - 1;
            _cur_idx = (init_time >> _interval_shift) + _idx_offset;

            //2. 初始化slots
            _slots = new LinkedList<TimerWheelItem<T>>[1 << slot_count_shift];
            for (int i = 0; i < _slots.Length; ++i)
            {
                _slots[i] = new LinkedList<TimerWheelItem<T>>();
            }
        }

        public void Clear()
        {
            foreach (var p in _slots)
            {
                p?.ExtClear();
            }
        }

        public LinkedListNode<TimerWheelItem<T>> Add(TimerWheelItem<T> item)
        {
            var slot = _FindSlotToAdd(item);
            if (slot == null)
                return null;
            return slot.ExtAddLast(item);
        }

        public bool MoveTo(LinkedListNode<TimerWheelItem<T>> item_node)
        {
            if (item_node == null)
                return false;

            var slot = _FindSlotToAdd(item_node.Value);
            if (slot == null)
                return false;

            item_node.List.Remove(item_node);
            slot.AddLast(item_node);
            return true;
        }

        private LinkedList<TimerWheelItem<T>> _FindSlotToAdd(TimerWheelItem<T> item)
        {
            //1. 计算出idx
            long idx = item.Time >> _interval_shift;

            //2. 检查
            if (idx < _cur_idx || idx >= (_cur_idx + _slots.Length))
                return null;

            //3. 计算出slot idx
            long slot_idx = idx & _slot_idx_mask;

            return _slots[slot_idx];
        }

        public void Tick(long time_now, LinkedList<TimerWheelItem<T>> out_list)
        {
            //1. 计算time 对应的idx
            long idx_old = _cur_idx;
            _cur_idx = (time_now >> _interval_shift) + _idx_offset;

            //2. 开始循环, 要注意不能转多余一圈
            long dst_idx = idx_old + _slots.Length;
            dst_idx = System.Math.Min(_cur_idx, dst_idx);
            for (long idx = idx_old; idx < dst_idx; ++idx)
            {
                //2.1 根据 cur_idx 得到 slot_idx, 求余数
                long slot_idx = idx & _slot_idx_mask;

                //2.2 获取 list，并检查数量
                LinkedList<TimerWheelItem<T>> list = _slots[slot_idx];
                if (list.Count == 0)
                    continue;

                //2.3. 处理所有的TimeId
                for (; ; )
                {
                    LinkedListNode<TimerWheelItem<T>> node = list.First;
                    if (node == null)
                        break;
                    list.RemoveFirst();
                    out_list.AddLast(node);
                }
            }
        }
    }

    //时间轮组, time_stamp 可以是秒，也可以是毫秒
    internal class TimerWheelGroup<T>
    {
        public static LinkedList<TimerWheelItem<T>> _temp = new LinkedList<TimerWheelItem<T>>();    //临时list，用来从一组slot 复制到另外一组slot的
        public long _time;

        public TimerWheel<T>[] _wheels;
        public Dictionary<int, LinkedListNode<TimerWheelItem<T>>> _dict;
        //统计信息，第一次添加的时候，落在每个组上的数量
        public uint[] _stat;
        public long _max_dt;

        //获取能添加的最大时间差
        public long GetMaxTimeDt()
        {
            return _max_dt;
        }

        public int Count { get { return _dict.Count; } }

        public void Clear()
        {
            foreach (var s in _wheels)
            {
                s?.Clear();
            }
            _dict.Clear();
        }

        public int AddTimerByDt(long delta_time_ms, T user_data)
        {
            return AddTimer(delta_time_ms + _time, user_data);
        }

        public int AddTimer(long expire_timestamp_ms, T user_data)
        {
            //1. 检查，时间不能小于当前时间
            if (expire_timestamp_ms < _time)
                return TimeWheelId.InvalidId;

            //2. 创建 TimeId的节点
            TimerWheelItem<T> item = new TimerWheelItem<T>(expire_timestamp_ms, user_data);
            LinkedListNode<TimerWheelItem<T>> node = null;

            //3. 添加到 slots_group里面
            for (int i = 0; i < _wheels.Length; ++i)
            {
                TimerWheel<T> group = _wheels[i];
                node = group.Add(item);
                if (node != null)
                {
                    _stat[i]++;
                    break;
                }
            }

            //4. 检查结果
            if (node != null)
            {
                _dict.Add(item.Id, node);
                return item.Id;
            }
            else
            {
                Log.Assert(false);
                return TimeWheelId.InvalidId;
            }
        }

        public bool CancelTimer(int timer_id)
        {
            //1. 根据id 找到 Node
            _dict.TryGetValue(timer_id, out LinkedListNode<TimerWheelItem<T>> node);
            if (node == null)
                return false;

            //2. 把节点从对应的List上移除就行了
            _dict.Remove(timer_id);
            node.List.ExtRemove(node);
            return true;
        }

        public void Tick(long time_stamp, List<TimerWheelItem<T>> out_list)
        {
            //1. 检查时间，必须向前，如果相等，一定没有任何事件发生
            if (time_stamp < _time)
                return;
            _time = time_stamp;

            //2. 处理第一个，要把过期的TimeId 传出
            _wheels[0].Tick(time_stamp, _temp);
            foreach (TimerWheelItem<T> p in _temp)
                out_list.Add(p);
            _temp.ExtClear();

            //3. 迭代后续的 slot_groups
            for (int i = 1; i < _wheels.Length; ++i)
            {
                //3.1 更新，获取过期的列表
                _wheels[i].Tick(time_stamp, _temp);

                //3.2 处理 列表里面的 timeid
                LinkedListNode<TimerWheelItem<T>> item_node = _temp.First;
                for (; ; )
                {
                    if (item_node == null)
                        break;
                    var curr_node = item_node;
                    item_node = item_node.Next;

                    //判断是否已经过期了，一般这种情况是 time的间隔比较大                
                    if (curr_node.Value.Time <= time_stamp)
                    {
                        out_list.Add(curr_node.Value);
                        _dict.Remove(curr_node.Value.Id);
                        continue;
                    }

                    //把该timeid 添加到其他的时间轮里面
                    bool succ = false;
                    for (int k = i - 1; k >= 0; --k)
                    {
                        succ = _wheels[k].MoveTo(curr_node);
                        if (succ)
                            break;
                    }

                    if (!succ)
                    {
                        _dict.Remove(curr_node.Value.Id);
                        Log.Assert(succ);
                    }
                }

                _temp.ExtClear();
            }

            //4. 排序
            if (out_list.Count > 0)
                out_list.Sort(_ItemCompare);
        }


        private static int _ItemCompare(TimerWheelItem<T> x, TimerWheelItem<T> y)
        {
            if (x.Time < y.Time)
                return -1;
            else if (x.Time > y.Time)
                return 1;
            return x.Id - y.Id;
        }
    }


    internal static class TimerWheelGroup
    {
        // interval 和 wheel_slots 里面的值必须是2的幂
        public static TimerWheelGroup<T> Create<T>(long init_time, long interval, int[] wheel_slots)
        {
            //1. 检查
            if (init_time < 0)
                return null;
            if (wheel_slots == null || wheel_slots.Length == 0)
                return null;
            if (!BitUtil.IsPowOf2(interval))
                return null;
            foreach (int wheel_slot in wheel_slots)
            {
                if (!BitUtil.IsPowOf2(wheel_slot))
                    return null;
            }

            //2. 初始化
            int count = wheel_slots.Length;

            TimerWheelGroup<T> ret = new TimerWheelGroup<T>
            {
                _time = init_time,
                _wheels = new TimerWheel<T>[count],
                _dict = new Dictionary<int, LinkedListNode<TimerWheelItem<T>>>(),
                _stat = new uint[count],
            };

            //3. 初始化slot
            int interval_shift = BitUtil.LastIndexOf1(interval);

            //4. 创建slots
            for (int i = 0; i < count; ++i)
            {
                int wheel_slot_shift = BitUtil.LastIndexOf1(wheel_slots[i]);
                ret._wheels[i] = new TimerWheel<T>(init_time, (byte)interval_shift, (byte)wheel_slot_shift, i==0);
                interval_shift += wheel_slot_shift;
                ret._max_dt = (1L << interval_shift) - 1;
            }
            return ret;
        }
    }
}
