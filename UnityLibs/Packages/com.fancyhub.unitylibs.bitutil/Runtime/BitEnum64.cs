/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/

#if UNITY_EDITOR && ODIN_INSPECTOR
#define USE_ODIN
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
    [UnityEditor.CustomPropertyDrawer(typeof(BitEnum64<>), true)]
    public class EdBitEnum64Drawer : UnityEditor.PropertyDrawer
    {
        private List<(string name, int value)> _enum_name_values;

        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
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

            if (_enum_name_values == null)
                _enum_name_values = GetEnumNameValues(targetObjectType);

            UnityEditor.EditorGUI.BeginProperty(position, label, property);
            UnityEditor.EditorGUI.BeginChangeCheck();

            ShowBitEnum(position, label, v, _enum_name_values, newValue =>
            {
                propertyX.ulongValue = newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            if (UnityEditor.EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();
            UnityEditor.EditorGUI.EndProperty();
        }

        public static List<(string name, int value)> GetEnumNameValues(Type enum_type)
        {
            Array name_array = enum_type.GetEnumNames();
            Array value_array = enum_type.GetEnumValues();

            List<(string name, int value)> ret = new List<(string name, int value)>(value_array.Length);

            for (int i = 0; i < name_array.Length; i++)
            {
                ret.Add((name_array.GetValue(i).ToString(), value_array.GetValue(i).GetHashCode()));
            }
            return ret;
        }


        public static void ShowBitEnum(Rect position,
            GUIContent label,
            ulong value,
            List<(string name, int value)> name_values,
            Action<ulong> onChange)
        {
            if (label != null)
            {
                Rect fieldPos = UnityEditor.EditorGUI.PrefixLabel(position, label);
                string displayText = _GetDisplayText(value, name_values);

                if (GUI.Button(fieldPos, displayText, UnityEditor.EditorStyles.popup))
                {
                    _ShowMaskMenu(value, name_values, onChange);
                }
            }
            else
            {
                string displayText = _GetDisplayText(value, name_values);

                if (GUI.Button(position, displayText, UnityEditor.EditorStyles.popup))
                {
                    _ShowMaskMenu(value, name_values, onChange);
                }
            }
        }

        // 弹出多选菜单
        private static void _ShowMaskMenu(
            ulong current,
            List<(string name, int value)> enum_name_values,
            Action<ulong> onChanged)
        {
            var menu = new UnityEditor.GenericMenu();
            bool is_all = true;
            foreach (var p in enum_name_values)
            {
                bool on = (current & (1UL << p.value)) != 0 && p.value >= 0 && p.value < 63;
                if (!on)
                {
                    is_all = false;
                    break;
                }
            }

            // Nothing
            menu.AddItem(new GUIContent("Nothing"), current == 0, () => onChanged(0));
            menu.AddItem(new GUIContent("Everything"), is_all, () => onChanged(ulong.MaxValue));
            menu.AddSeparator("");

            // 每一位            
            foreach (var p in enum_name_values)
            {
                bool on = (current & (1UL << p.value)) != 0 && p.value >= 0 && p.value < 63;
                menu.AddItem(new GUIContent(p.name), on, () =>
                {
                    if (p.value >= 0 && p.value < 63)
                    {
                        ulong newValue = current ^ (1UL << p.value);
                        onChanged(newValue);
                    }
                });
            }

            menu.ShowAsContext();
        }

        // 生成显示文本
        private static string _GetDisplayText(ulong value, List<(string name, int value)> enum_values)
        {
            if (value == 0) return "Nothing";

            List<string> temp = new List<string>(enum_values.Count);

            foreach (var p in enum_values)
            {
                if (p.value >= 0 && p.value < 64 && (value & (1UL << p.value)) != 0)
                {
                    temp.Add(p.name);
                }
            }

            if (temp.Count == 0)
                return "Nothing";

            if (temp.Count == enum_values.Count)
                return "Everything";

            if (temp.Count > 3)
                return "Mixed...";

            return String.Join(',', temp.ToArray());
        }
    }
#endif


#if USE_ODIN
    public class EdOdinBitEnum64Drawer<T> : Sirenix.OdinInspector.Editor.OdinValueDrawer<BitEnum64<T>> where T : struct, IConvertible
    {
        private static List<(string name, int value)> _enum_name_values;
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect();

            BitEnum64<T> old_v = ValueEntry.SmartValue;
            Type tar_type = old_v.GetType().GetGenericArguments()[0];

            if (_enum_name_values == null)
                _enum_name_values = EdBitEnum64Drawer.GetEnumNameValues(typeof(T));

            EdBitEnum64Drawer.ShowBitEnum(rect, label, old_v.Value, _enum_name_values, newValue =>
            {
                ValueEntry.SmartValue = newValue;
            });
        }
    }
#endif

}
