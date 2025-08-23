/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/

#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    /// <summary>
    /// 对应的枚举 不要本身就是 Flags类型的
    /// </summary>
    [Serializable]
    //[Unity.Burst.BurstCompile]
    public struct BitEnum64<T> : IEquatable<BitEnum64<T>> where T : struct, IConvertible
    {
        public const int LENGTH = 64;
        public static BitEnum64<T> Zero = new BitEnum64<T>(0);
        public static BitEnum64<T> All = new BitEnum64<T>(ulong.MaxValue);

        public ulong Value;
        public BitEnum64(ulong value) { Value = value; }
        public BitEnum64(long value) { Value = (ulong)value; }

        public BitEnum64(params T[] arrays)
        {
            Value = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                int idx = BitUtil.Struct2Int(arrays[i]);
                Value |= 1u << idx;
            }
        }

        public bool SetBit(T idx, bool state)
        {
            //1. check
            int index = BitUtil.Struct2Int(idx);
            if (index < 0 || index >= LENGTH)
            {
                Log.Assert(false, "idx:{1},{2} 要在 [0,{0})", LENGTH, idx, index);
                return false;
            }

            if (state)
                Value = (1ul << index) | Value;
            else
                Value = ~(1ul << index) & Value;
            return true;
        }

        public bool GetBit(T idx)
        {
            //1. check
             int index = BitUtil.Struct2Int(idx);
            if (index < 0 || index >= LENGTH)
            {
                Log.Assert(false, "idx:{1},{2} 要在 [0,{0})", LENGTH, idx, index);
                return false;
            }
            return ((1ul << index) & Value) != 0;
        }

        public void SetValue(BitEnum64<T> mask, BitEnum64<T> v)
        {
            Value = (Value & (~mask.Value)) | (v.Value & mask.Value);
        }

        public void SetValue(BitEnum64<T> mask, bool v)
        {
            if (v)
                Value |= mask.Value;
            else
                Value &= ~mask.Value;
        }

        public bool SetBit(int idx, bool state)
        {
            //1. check            
            if (idx < 0 || idx >= LENGTH)
            {
                Log.Assert(false, "idx:{1} 要在 [0,{0})", LENGTH, idx);
                return false;
            }

            if (state)
                Value = (1ul << idx) | Value;
            else
                Value = ~(1ul << idx) & Value;
            return true;
        }

        public bool GetBit(int idx)
        {
            //1. check            
            if (idx < 0 || idx >= LENGTH)
            {
                Log.Assert(false, "idx:{1} 要在 [0,{0})", LENGTH, idx);
                return false;
            }
            return ((1ul << idx) & Value) != 0;
        }

        public bool this[T idx]
        {
            set
            {
                SetBit(idx, value);
            }
            get
            {
                return GetBit(idx);
            }
        }

        public bool this[int idx]
        {
            set { SetBit(idx, value); }
            get { return GetBit(idx); }
        }

        public int Length { get { return LENGTH; } }

        /// <summary>
        /// 清零或是置为最大值
        /// </summary>
        public void Clear(bool state)
        {
            Value = state ? uint.MaxValue : 0;
        }

        public int GetCount(bool v)
        {
            int ret = 0;
            ulong u64_v1 = v ? 1ul : 0ul;
            for (int i = 0; i < LENGTH; ++i)
            {
                ulong u64_v2 = (Value >> i) & 1ul;
                if (u64_v2 == u64_v1)
                    ret++;
            }
            return ret;
        }

        public override bool Equals(object obj) { if (obj is BitEnum64<T> other) return other.Value == Value; return false; }

        public bool Equals(BitEnum64<T> other) { return Value == other.Value; }

        public override int GetHashCode() { return HashCode.Combine(Value); }


        public static BitEnum64<T> operator &(BitEnum64<T> a, BitEnum64<T> b) { return new BitEnum64<T>(a.Value & b.Value); }
        public static BitEnum64<T> operator |(BitEnum64<T> a, BitEnum64<T> b) { return new BitEnum64<T>(a.Value | b.Value); }
        public static bool operator ==(BitEnum64<T> a, BitEnum64<T> b) { return a.Value == b.Value; }
        public static bool operator !=(BitEnum64<T> a, BitEnum64<T> b) { return a.Value != b.Value; }

        public static implicit operator BitEnum64<T>(T v) { int idx = BitUtil.Struct2Int(v); return new BitEnum64<T>(1ul << idx); }
        public static implicit operator BitEnum64<T>(long v) { return new BitEnum64<T>(v); }
        public static implicit operator BitEnum64<T>(ulong v) { return new BitEnum64<T>(v); }
        public static implicit operator ulong(BitEnum64<T> v) { return v.Value; }
        public static implicit operator long(BitEnum64<T> v) { return (long)v.Value; }

    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(BitEnum64<>), true)]
    public class EdBitEnum64Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var type_ref = fieldInfo.FieldType;
            if (typeof(System.Collections.IList).IsAssignableFrom(type_ref))
            {
                if (type_ref.IsArray)
                    type_ref = type_ref.GetElementType();
                else
                    type_ref = type_ref.GetGenericArguments()[0];
            }

            var targetObjectType = type_ref.GetGenericArguments()[0];

            var propertyX = property.FindPropertyRelative("Value");
            ulong v = propertyX.ulongValue;
            int mask = Value2Mask(v, targetObjectType);
            int new_mask = mask;
            if (label != null)
                new_mask = EditorGUI.MaskField(position, label, mask, targetObjectType.GetEnumNames());
            else
                new_mask = EditorGUI.MaskField(position, mask, targetObjectType.GetEnumNames());

            if (mask == new_mask)
                return;

            ulong new_v = Mask2Value(new_mask, targetObjectType);
            propertyX.ulongValue = new_v;
        }

        public static int Value2Mask(ulong v, Type enum_type)
        {
            // 根据V 转换成 index_mask
            Array value_array = enum_type.GetEnumValues();
            int mask = 0;
            for (int i = 0; i < value_array.Length; i++)
            {
                int index = value_array.GetValue(i).GetHashCode();
                if ((v & (1Ul << index)) != 0)
                    mask |= 1 << i;
            }
            return mask;
        }

        public static ulong Mask2Value(int mask, Type enum_type)
        {
            ulong v = 0;
            Array value_array = enum_type.GetEnumValues();
            for (int i = 0; i < value_array.Length; i++)
            {
                int index = value_array.GetValue(i).GetHashCode();
                if ((mask & (1 << i)) != 0)
                {
                    v |= 1UL << index;
                }
            }
            return v;
        }
    }
#endif
}
