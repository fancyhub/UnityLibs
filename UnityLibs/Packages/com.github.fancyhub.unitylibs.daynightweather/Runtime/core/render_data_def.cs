/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/11/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;


/**
 * 主要是关于 Render Slot  Key的操作
 * 天气的定义 & 主要枚举类型的定义
 */
namespace FH.DayNightWeather
{
    public enum EWeather
    {
        sunny,
        cloudy,
        rain,
        snow,
        max,
    }

    public enum ERenderSlot
    {
        light, //光源
        env,  //环境
        post, //后期
    }

    public struct RenderSlotKey : IEquatable<RenderSlotKey>, IEqualityComparer<RenderSlotKey>
    {
        public static IEqualityComparer<RenderSlotKey> EqualityComparer = new RenderSlotKey();

        public ERenderSlot _type;
        public int _sub_type;

        public static RenderSlotKey Create<T>(ERenderSlot type, T sub_type) where T : Enum
        {
            return new RenderSlotKey() { _type = type, _sub_type = sub_type.GetHashCode() };
        }

        public void SetMainType(ERenderSlot type)
        {
            _type = type;
        }

        public void SetSubType<T>(T sub_type) where T : Enum
        {
            _sub_type = sub_type.GetHashCode();
        }

        public static bool operator !=(RenderSlotKey self, RenderSlotKey other)
        {
            return !(self == other);
        }

        public static bool operator ==(RenderSlotKey self, RenderSlotKey other)
        {
            return (self._type == other._type && self._sub_type == other._sub_type);
        }

        public override int GetHashCode()
        {
            uint hi = ((uint)_type) << 16;
            uint lo = (uint)_sub_type;
            uint com = hi | lo;
            return (int)com;
        }

        public override bool Equals(object obj)
        {
            if (obj is RenderSlotKey)
                return this == ((RenderSlotKey)obj);
            return false;
        }

        public bool Equals(RenderSlotKey other)
        {
            return this == other;
        }

        bool IEqualityComparer<RenderSlotKey>.Equals(RenderSlotKey x, RenderSlotKey y)
        {
            return x == y;
        }

        int IEqualityComparer<RenderSlotKey>.GetHashCode(RenderSlotKey obj)
        {
            return obj.GetHashCode();
        }
    }
}
