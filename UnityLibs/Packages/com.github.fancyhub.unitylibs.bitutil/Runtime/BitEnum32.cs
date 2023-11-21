/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

#if UNITY_EDITOR
//#define BEHAVIOUR_DESIGNER
//#define USE_ODIN
using UnityEditor;
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    internal static class BitEnumUtil
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        internal static int ToInt32<T>(T v) where T : struct, IConvertible
        {
            //return v.GetHashCode();
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.EnumToInt(v);
        }
    }
    /// <summary>
    /// 对应的枚举 不要本身就是 Flags类型的
    /// </summary>
    [Serializable]
    //[Unity.Burst.BurstCompile]
    public struct BitEnum32<T> : IEquatable<BitEnum32<T>> where T : struct, IConvertible
    {
        public const int LENGTH = 32;
        public static BitEnum32<T> Zero = new BitEnum32<T>(0);
        public static BitEnum32<T> All = new BitEnum32<T>(uint.MaxValue);

#if BEHAVIOUR_DESIGNER
        [BDBitEnum32]
#endif
        public uint Value;
        public BitEnum32(uint value) { Value = value; }
        public BitEnum32(int value) { Value = (uint)value; }

        public BitEnum32(params T[] arrays)
        {
            Value = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                int idx = BitEnumUtil.ToInt32(arrays[i]);
                Value |= 1u << idx;
            }
        }

        public bool SetBit(T idx, bool state)
        {
            //1. check
            int index = BitEnumUtil.ToInt32(idx);
            return SetBit(index, state);
        }

        public bool GetBit(T idx)
        {
            //1. check
            int index = BitEnumUtil.ToInt32(idx);
            return GetBit(index);
        }

        public void SetValue(BitEnum32<T> mask, BitEnum32<T> value)
        {
            Value = (Value & (~mask.Value)) | (value.Value & mask.Value);
        }

        public void SetValue(BitEnum32<T> mask, bool value)
        {
            if (value)
                Value |= mask.Value;
            else
                Value &= ~mask.Value;
        }

        public BitEnum32<T> GetValue(BitEnum32<T> mask)
        {
            return Value & mask.Value;
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
                Value = (1u << idx) | Value;
            else
                Value = ~(1u << idx) & Value;
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
            return ((1u << idx) & Value) != 0;
        }

        public bool this[T idx]
        {
            set
            {
                int index = BitEnumUtil.ToInt32(idx);
                SetBit(index, value);
            }
            get
            {
                int index = BitEnumUtil.ToInt32(idx);
                return GetBit(index);
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
            uint u32_v1 = v ? 1u : 0u;
            for (int i = 0; i < LENGTH; ++i)
            {
                uint u32_v2 = (Value >> i) & 0x1u;
                if (u32_v2 == u32_v1)
                    ret++;
            }
            return ret;
        }

        public override bool Equals(object obj)
        {
            return obj is BitEnum32<T> @enum && Equals(@enum);
        }

        public bool Equals(BitEnum32<T> other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode() { return HashCode.Combine(Value); }


        public static BitEnum32<T> operator &(BitEnum32<T> a, BitEnum32<T> b) { return new BitEnum32<T>(a.Value & b.Value); }
        public static BitEnum32<T> operator |(BitEnum32<T> a, BitEnum32<T> b) { return new BitEnum32<T>(a.Value | b.Value); }
        public static bool operator ==(BitEnum32<T> a, BitEnum32<T> b) { return a.Value == b.Value; }
        public static bool operator !=(BitEnum32<T> a, BitEnum32<T> b) { return a.Value != b.Value; }

        public static implicit operator BitEnum32<T>(T v) { int idx = BitEnumUtil.ToInt32(v); return new BitEnum32<T>(1u << idx); }
        public static implicit operator BitEnum32<T>(int v) { return new BitEnum32<T>(v); }
        public static implicit operator BitEnum32<T>(uint v) { return new BitEnum32<T>(v); }
        public static implicit operator uint(BitEnum32<T> v) { return v.Value; }
        public static implicit operator int(BitEnum32<T> v) { return (int)v.Value; }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(BitEnum32<>), true)]
    public class EdBitEnum32Drawer : PropertyDrawer
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
            uint v = propertyX.uintValue;
            int mask = Value2Mask(v, targetObjectType);
            int new_mask = mask;
            if (label != null)
                new_mask = EditorGUI.MaskField(position, label, mask, targetObjectType.GetEnumNames());
            else
                new_mask = EditorGUI.MaskField(position, mask, targetObjectType.GetEnumNames());

            if (mask == new_mask)
                return;

            uint new_v = Mask2Value(new_mask, targetObjectType);
            propertyX.uintValue = new_v;
        }

        public static int Value2Mask(uint v, Type enum_type)
        {
            // 根据V 转换成 index_mask
            Array value_array = enum_type.GetEnumValues();
            int mask = 0;
            for (int i = 0; i < value_array.Length; i++)
            {
                int index = value_array.GetValue(i).GetHashCode();
                if ((v & (1U << index)) != 0)
                    mask |= 1 << i;
            }
            return mask;
        }

        public static uint Mask2Value(int mask, Type enum_type)
        {
            uint v = 0;
            Array value_array = enum_type.GetEnumValues();
            for (int i = 0; i < value_array.Length; i++)
            {
                int index = value_array.GetValue(i).GetHashCode();
                if ((mask & (1 << i)) != 0)
                {
                    v |= 1U << index;
                }
            }
            return v;
        }
    }
#endif

#if USE_ODIN
    public class EdOdinBitEnum32Drawer<T> : OdinValueDrawer<BitEnum32<T>> where T : struct, IConvertible
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect();

            BitEnum32<T> old_v = ValueEntry.SmartValue;
            Type tar_type = old_v.GetType().GetGenericArguments()[0];

            int mask = EdBitEnum32Drawer.Value2Mask(old_v.Value, tar_type);
            int new_mask = mask;

            if (label != null)
                new_mask = EditorGUI.MaskField(rect, label, mask, tar_type.GetEnumNames());
            else
                new_mask = EditorGUI.MaskField(rect, mask, tar_type.GetEnumNames());


            if (new_mask != mask)
            {
                ValueEntry.SmartValue = EdBitEnum32Drawer.Mask2Value(new_mask, tar_type);
            }
        }
    }
#endif

#if BEHAVIOUR_DESIGNER
    //这个类是解决 行为树的Inspector使用MaskField的功能
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class BDBitEnum32Attribute : BehaviorDesigner.Runtime.Tasks.ObjectDrawerAttribute
    {
        public BDBitEnum32Attribute()
        {
        }
    }

    [BehaviorDesigner.Editor.CustomObjectDrawer(typeof(BDBitEnum32Attribute))]
    public class EdBDBitEnum32AttributeDrawer : BehaviorDesigner.Editor.ObjectDrawer
    {
        public override void OnGUI(GUIContent label)
        {
            Type enum_type = FieldInfo.DeclaringType.GetGenericArguments()[0];

            uint v = (uint)Value;
            int mask = EdBitEnum32Drawer.Value2Mask(v, enum_type);
            int new_mask = mask;
            if (label != null)
                new_mask = EditorGUILayout.MaskField(label, mask, enum_type.GetEnumNames());
            else
                new_mask = EditorGUILayout.MaskField(mask, enum_type.GetEnumNames());

            if (mask == new_mask)
                return;

            uint new_v = EdBitEnum32Drawer.Mask2Value(new_mask, enum_type);
            Value = new_v;
        }
    }
#endif
}
