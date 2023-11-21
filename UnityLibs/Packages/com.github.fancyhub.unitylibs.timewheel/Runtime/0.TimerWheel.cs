/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace FH
{
    public struct TimerId : IEquatable<TimerId>, IComparable<TimerId>
    {
        public static TimerId InvalidId = new TimerId(0);

        private static uint S_ID_GEN = 0;

        public static IEqualityComparer<TimerId> EqualityComparer = new TimerIdEqualityComparer();

        private uint _id;
        public static TimerId Create()
        {
            return new TimerId(++S_ID_GEN);
        }

        public bool IsValid()
        {
            return _id != 0;
        }

        public bool Equals(TimerId other)
        {
            return other._id == _id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_id);
        }

        public override bool Equals(object obj)
        {
            if (obj is TimerId other)
                return _id == other._id;
            return false;
        }

        public int CompareTo(TimerId other)
        {
            return _id.CompareTo(other._id);
        }

        private TimerId(uint id) { _id = id; }

        public static bool operator ==(TimerId a, TimerId b)
        {
            return a._id == b._id;
        }
        public static bool operator !=(TimerId a, TimerId b)
        {
            return a._id != b._id;
        }

        private class TimerIdEqualityComparer : IEqualityComparer<TimerId>
        {
            public bool Equals(TimerId x, TimerId y)
            {
                return x._id == y._id;
            }

            public int GetHashCode(TimerId obj)
            {
                return HashCode.Combine(obj._id);
            }
        }
    }

    //Time Id, 里面含有id 和过期时间
    public struct TimerWheelItem 
    {
        public readonly TimerId Id;
        public readonly long Time;
        public TimerWheelItem(long time)
        {
            Id = TimerId.Create();
            Time = time;
        }
    }

    //一个时间轮，里面包含了一组 Slots，添加的time 有上限和下限
    // TimeStamp => idx 的计算方法 idx = (TimeStamp >>_interval_shift) + _idx_offset
    // idx => slot_idx 的计算方法 slot_idx = idx & _slot_idx_mask
    public class TimerWheel 
    {
        public long _cur_idx;
        public long _slot_idx_mask;
        public byte _idx_offset; //根据时间搓算出的index 要加上这个偏移，要么0要么1
        public byte _interval_shift; //间隔, 是一个移位操作的数量 
        public LinkedList<TimerWheelItem >[] _slots;

        public TimerWheel(long init_time, byte interval_bit_shift, byte slot_count_shift, byte slot_idx_offset)
        {
            //1. 初始化
            _idx_offset = slot_idx_offset;
            _interval_shift = interval_bit_shift;
            _slot_idx_mask = (1L << slot_count_shift) - 1;
            _cur_idx = (init_time >> _interval_shift) + _idx_offset;

            //2. 初始化slots
            _slots = new LinkedList<TimerWheelItem >[1 << slot_count_shift];
            for (int i = 0; i < _slots.Length; ++i)
            {
                _slots[i] = new LinkedList<TimerWheelItem >();
            }
        }

        public void Clear()
        {
            foreach (var p in _slots)
            {
                p?.ExtClear();
            }
        }

        public LinkedListNode<TimerWheelItem > Add(TimerWheelItem  item)
        {
            var slot = _FindSlotToAdd(item);
            if (slot == null)
                return null;
            return slot.ExtAddLast(item);
        }

        public bool Add(LinkedListNode<TimerWheelItem > item_node)
        {
            if (item_node == null)
                return false;

            var slot = _FindSlotToAdd(item_node.Value);
            if (slot == null)
                return false;

            slot.AddLast(item_node);
            return true;
        }

        private LinkedList<TimerWheelItem > _FindSlotToAdd(TimerWheelItem  item)
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

        public void Tick(long time_now, LinkedList<TimerWheelItem > out_list)
        {
            //1. 清除
            out_list.Clear();

            //2. 计算time 对应的idx
            long idx_old = _cur_idx;
            _cur_idx = (time_now >> _interval_shift) + _idx_offset;

            //3. 开始循环, 要注意不能转多余一圈
            long dst_idx = idx_old + _slots.Length;
            dst_idx = System.Math.Min(_cur_idx, dst_idx);
            for (long idx = idx_old; idx < dst_idx; ++idx)
            {
                //3.1 根据 cur_idx 得到 slot_idx, 求余数
                long slot_idx = idx & _slot_idx_mask;

                //3.2 获取 list，并检查数量
                LinkedList<TimerWheelItem > list = _slots[slot_idx];
                if (list.Count == 0)
                    continue;

                //3.3. 处理所有的TimeId
                for (; ; )
                {
                    LinkedListNode<TimerWheelItem > node = list.First;
                    if (node == null)
                        break;
                    list.RemoveFirst();
                    out_list.AddLast(node);
                }
            }
        }
    }

    //时间轮组, time_stamp 可以是秒，也可以是毫秒
    public class TimerWheelGroup 
    {
        public static LinkedList<TimerWheelItem > _temp = new LinkedList<TimerWheelItem >();    //临时list，用来从一组slot 复制到另外一组slot的
        public long _time;

        public TimerWheel [] _slot_groups;
        public Dictionary<TimerId, LinkedListNode<TimerWheelItem >> _dict;
        //统计信息，第一次添加的时候，落在每个组上的数量
        public uint[] _stat;
        public long _max_dt;

        // interval 和 wheel_counts 里面的值必须是2的幂
        public static TimerWheelGroup  Create(long init_time, long interval, int[] wheel_counts)
        {
            //1. 检查
            if (init_time < 0)
                return null;
            if (wheel_counts == null || wheel_counts.Length == 0)
                return null;
            if (!BitUtil.IsPowOf2(interval))
                return null;
            foreach (int wheel_count in wheel_counts)
            {
                if (!BitUtil.IsPowOf2(wheel_count))
                    return null;
            }

            //2. 初始化
            int count = wheel_counts.Length;

            TimerWheelGroup  ret = new TimerWheelGroup 
            {
                _time = init_time,
                _slot_groups = new TimerWheel [count],
                _dict = new Dictionary<TimerId, LinkedListNode<TimerWheelItem >>(TimerId.EqualityComparer),
                _stat = new uint[count],
            };

            //3. 初始化slot
            int interval_shift = BitUtil.LastIndexOf1(interval);
            int idx_offset = 0;

            //4. 创建slots
            for (int i = 0; i < count; ++i)
            {
                int wheel_count_shift = BitUtil.LastIndexOf1(wheel_counts[i]);
                ret._slot_groups[i] = new TimerWheel (init_time, (byte)interval_shift, (byte)wheel_count_shift, (byte)idx_offset); 
                interval_shift += wheel_count_shift;
                idx_offset |= 1;
                ret._max_dt = (1L << interval_shift) - 1;
            }
            return ret;
        }

        //获取能添加的最大时间差
        public long GetMaxTimeDt()
        {
            return _max_dt;
        }

        public int Count { get { return _dict.Count; } }

        public void Clear()
        {
            foreach (var s in _slot_groups)
            {
                s?.Clear();
            }
            _dict.Clear();
        }

        public TimerId AddTimerByDt(long dt)
        {
            return AddTimer(dt + _time);
        }

        public TimerId AddTimer(long time)
        {
            //1. 检查，时间不能小于当前时间
            if (time < _time)
                return TimerId.InvalidId;

            //2. 创建 TimeId的节点
            TimerWheelItem  item = new TimerWheelItem(time);
            LinkedListNode<TimerWheelItem > node = null;

            //3. 添加到 slots_group里面
            for (int i = 0; i < _slot_groups.Length; ++i)
            {
                TimerWheel  group = _slot_groups[i];
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
                System.Diagnostics.Debug.Assert(false);
                return TimerId.InvalidId;
            }
        }

        public bool CancelTimer(TimerId id)
        {
            //1. 根据id 找到 Node
            _dict.TryGetValue(id, out LinkedListNode<TimerWheelItem > node);
            if (node == null)
                return false;

            //2. 把节点从对应的List上移除就行了
            _dict.Remove(id);
            node.List.ExtRemove(node);
            return true;
        }

        public void Tick(long time_stamp, List<TimerWheelItem > out_list)
        {
            //1. 清除 返回list
            out_list.Clear();

            //2. 检查时间，必须向前，如果相等，一定没有任何事件发生
            if (time_stamp < _time)
                return;
            _time = time_stamp;

            //3. 处理第一个，要把过期的TimeId 传出
            _slot_groups[0].Tick(time_stamp, _temp);
            for (; ; )
            {
                //3.1 永远取第一个节点
                if (!_temp.ExtPopFirst(out var item))
                    break;

                //3.2 把结果传出
                out_list.Add(item);

                //3.3 移除，并缓存
                _dict.Remove(item.Id);
            }

            //4. 迭代后续的 slot_groups
            for (int i = 1; i < _slot_groups.Length; ++i)
            {
                //4.1 更新，获取过期的列表
                _slot_groups[i].Tick(time_stamp, _temp);

                //4.2 处理 列表里面的 timeid
                for (; ; )
                {
                    if (!_temp.ExtPopFirstNode(out var item_node))
                        break;


                    //判断是否已经过期了，一般这种情况是 time的间隔比较大                
                    if (item_node.Value.Time <= time_stamp)
                    {
                        out_list.Add(item_node.Value);
                        _dict.Remove(item_node.Value.Id);
                        continue;
                    }

                    //把该timeid 添加到其他的时间轮里面
                    bool succ = false;
                    for (int k = i - 1; k >= 0; --k)
                    {
                        succ = _slot_groups[k].Add(item_node);
                        if (succ)
                            break;
                    }

                    if (!succ)
                    {
                        _dict.Remove(item_node.Value.Id);
                        System.Diagnostics.Debug.Assert(succ);
                    }
                }
            }

            //5. 排序
            if (out_list.Count > 0)
                out_list.Sort(_ItemCompare);
        }


        private static int _ItemCompare(TimerWheelItem  x, TimerWheelItem  y)
        {
            if (x.Time < y.Time)
                return -1;
            else if (x.Time > y.Time)
                return 1;
            return x.Id.CompareTo(y.Id);
        }
    }
}
