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
    /// <summary>
    /// 数据slot的group
    /// </summary>
    public class RenderDataSlotGroup
    {
        public Dictionary<RenderSlotKey, IRenderDataSlot> _slots;
        public Dictionary<ERenderSlot, bool> _dirty_flags;

        public RenderDataSlotGroup()
        {
            _slots = new Dictionary<RenderSlotKey, IRenderDataSlot>(RenderSlotKey.EqualityComparer);
            _dirty_flags = new Dictionary<ERenderSlot, bool>();
        }

        public void ClearSlotData()
        {
            foreach (var p in _slots)
            {
                p.Value.Clear();
            }
        }

        public void Set(RenderSlotKey key, IRenderDataSlot slot)
        {
            _slots[key] = slot;
        }

        public void Clear()
        {
            _slots.Clear();
            _dirty_flags.Clear();
        }

        public void SetLerp(RenderSlotKey key, IRenderDataSlot from, IRenderDataSlot to, float t)
        {
            bool contain = _slots.TryGetValue(key, out var slot);
            RDSUtil.LerpSlot(from, to, t, ref slot);
            if (!contain && slot != null)
            {
                _slots.Add(key, slot);
            }

            if (slot != null && slot.Dirty)
            {
                _dirty_flags[key._type] = true;
            }
        }

        public bool IsDirty()
        {
            return _dirty_flags.Count > 0;
        }

        public bool IsDirty<TEnum>(ERenderSlot type, TEnum sub_type) where TEnum : Enum
        {
            if (!_dirty_flags.TryGetValue(type, out var ret))
                return false;

            RenderSlotKey key = RenderSlotKey.Create(type, sub_type);
            if (!_slots.TryGetValue(key, out var slot))
                return false;
            if (slot == null)
                return false;
            return slot.Dirty;
        }

        public bool IsDirty(ERenderSlot type)
        {
            if (!_dirty_flags.TryGetValue(type, out var ret))
                return false;
            return ret;
        }

        public void ClearDirty()
        {
            _dirty_flags.Clear();
            foreach (var p in _slots)
                p.Value.Dirty = false;
        }

        public bool Get<TVal>(
            RenderSlotKey key,
            bool dirty_flag, //如果dirty_flag ==true, 并且slot.Dirty=false, 返回false
            out TVal v)
        {
            _slots.TryGetValue(key, out var slot);

            if (slot == null || !slot.HasData)
            {
                v = default;
                return false;
            }

            if (dirty_flag && !slot.Dirty)
            {
                v = default;
                return false;
            }

            IRenderDataSlot<TVal> type_slot = slot as IRenderDataSlot<TVal>;
            if (type_slot == null)
            {
                Log.Assert(false, "类型转换失败 从{0} -> {1}", slot.DataType, typeof(TVal));
                v = default;
                return false;
            }

            v = type_slot.Val;
            return true;
        }

        public bool Get<TEnum, TVal>(
            ERenderSlot slot_type,
            TEnum sub_type,
            bool dirty_flag, //如果dirty_flag ==true, 并且slot.Dirty=false, 返回false
            out TVal v) where TEnum : Enum
        {
            var key = new RenderSlotKey()
            {
                _type = slot_type,
                _sub_type = sub_type.GetHashCode(),
            };
            return Get(key, dirty_flag, out v);
        }

        public void Lerp(RenderDataSlotGroup from, RenderDataSlotGroup to, float t)
        {
            if (from == null || to == null)
                return;

            Dictionary<RenderSlotKey, IRenderDataSlot> from_2 = from._slots;
            Dictionary<RenderSlotKey, IRenderDataSlot> to_2 = to._slots;
            foreach (var p in from_2)
            {
                IRenderDataSlot slot_from = p.Value;
                to_2.TryGetValue(p.Key, out IRenderDataSlot slot_to);

                _slots.TryGetValue(p.Key, out var slot);
                if (slot == null)
                {
                    slot = slot_from.Clone();
                    _slots.Add(p.Key, slot);
                }

                RDSUtil.LerpSlot(slot_from, slot_to, t, ref slot);

                if (slot.Dirty)
                    _dirty_flags[p.Key._type] = true;
            }

            foreach (var p in to_2)
            {
                if (from_2.ContainsKey(p.Key))
                    continue;

                _slots.TryGetValue(p.Key, out var slot);
                if (slot == null)
                {
                    slot = p.Value.Clone();
                    _slots.Add(p.Key, slot);
                }

                slot.CopyFrom(p.Value);
                if (slot.Dirty)
                    _dirty_flags[p.Key._type] = true;
            }
        }

        public void Copy(RenderDataSlotGroup src)
        {
            if (src == null)
                return;

            foreach (var p in src._slots)
            {
                if (!p.Value.HasData)
                    continue;
                _slots.TryGetValue(p.Key, out var slot);
                if (slot == null)
                {
                    slot = p.Value.Clone();
                    _slots.Add(p.Key, slot);
                }
                slot.CopyFrom(p.Value);

                if (slot.Dirty)
                    _dirty_flags[p.Key._type] = true;
            }
        }

    }
}
