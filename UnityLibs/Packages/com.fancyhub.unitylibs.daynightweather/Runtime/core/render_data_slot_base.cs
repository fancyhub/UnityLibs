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
    public abstract class RenderDataSlotBase<T> : IRenderDataSlot<T>
    {
        public T _data;
        public bool _has_data;
        public bool _dirty;
        public Type _data_type = typeof(T);

        public RenderDataSlotBase()
        {
            _data = default;
            _has_data = false;
            _dirty = false; //如果是空的, dirty = false
        }

        public virtual bool HasData { get => _has_data; set { _has_data = value; } }
        public Type DataType { get => _data_type; }
        public virtual bool Dirty { get => _dirty; set => _dirty = value; }
        public virtual T Val
        {
            get => _data;
            set
            {
                if (!_has_data)
                {
                    _data = value;
                    _dirty = true;
                    _has_data = true;
                    return;
                }

                if (IsDataEuqal(ref _data, ref value))
                    return;
                _data = value;
                _dirty = true;
            }
        }

        public virtual void Clear()
        {
            _data = default;
            _has_data = false;
            _dirty = false;
        }

        public virtual bool CopyFrom(IRenderDataSlot src)
        {
            if (src == null)
            {
                Log.Assert(false, "复制的时候, src ==null ");
                return false;
            }

            if (!src.HasData)
                return true;

            IRenderDataSlot<T> src2 = src as IRenderDataSlot<T>;
            if (src2 == null)
            {
                Log.Assert(false, "复制的时候, Src的类型 {0} 和 目标 {1} 不一致 ", src.DataType, DataType);
                return false;
            }

            Val = src2.Val;
            return true;
        }

        public bool Lerp(IRenderDataSlot from, IRenderDataSlot to, float t)
        {
            if (from == null || to == null)
            {
                Log.Assert(from != null, "数据源 To 为空");
                Log.Assert(to != null, "数据源 To 为空");
                return false;
            }

            IRenderDataSlot<T> from2 = from as IRenderDataSlot<T>;
            IRenderDataSlot<T> to2 = to as IRenderDataSlot<T>;

            if (from2 == null || to2 == null)
            {
                Log.Assert(from2 != null, "数据源类型不对 From: {0} Dst:{1}", from.DataType, DataType);
                Log.Assert(to2 != null, "数据源类型不对 To: {0} Dst:{1}", to.DataType, DataType);
                return false;
            }

            if (from2.HasData && to2.HasData)
            {
                Val = LerpData(from2.Val, to2.Val, t);
            }
            else if (!from2.HasData && !to2.HasData)
            {
                return true; //如果两个源都是无数据, 直接返回
            }
            else
            {
                Val = from2.HasData ? from2.Val : to2.Val;
            }
            return true;
        }

        public abstract T LerpData(T from, T to, float t);
        public abstract bool IsDataEuqal(ref T x, ref T y);
        public abstract IRenderDataSlot Clone();
    }
}
