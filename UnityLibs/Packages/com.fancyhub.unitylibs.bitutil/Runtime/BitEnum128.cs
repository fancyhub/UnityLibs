/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR && ODIN_INSPECTOR
#define USE_ODIN
#endif

namespace FH
{
    /// <summary>
    /// 对应的枚举 不要本身就是 Flags类型的
    /// </summary>
    [System.Serializable]
    public struct BitEnum128<T> : IEquatable<BitEnum128<T>> where T : Enum
    {
        public const int LENGTH = 128;
        private const int C_COMP_LEN = 64;
        public static BitEnum128<T> All = new BitEnum128<T>(ulong.MaxValue, ulong.MaxValue);
        public static BitEnum128<T> Zero = new BitEnum128<T>(0, 0);

        public ulong ValueLo;
        public ulong ValueHi;

        public BitEnum128(ulong lo, ulong hi) { ValueLo = lo; ValueHi = hi; }

        public bool SetBit(T idx, bool state)
        {
            //1. check
            int index = BitUtil.Enum2Int(idx);
            if (index < 0 || index >= LENGTH)
            {
                Log.Assert(false, "idx:{1},{2} 要在 [0,{0})", LENGTH, idx, index);
                return false;
            }
            return SetBit(index, state);
        }

        public bool GetBit(T idx)
        {
            //1. check
            int index = BitUtil.Enum2Int(idx);
            if (index < 0 || index >= LENGTH)
            {
                Log.Assert(false, "idx:{1},{2} 要在 [0,{0})", LENGTH, idx, index);
                return false;
            }
            return GetBit(index);
        }

        public void SetValue(BitEnum128<T> mask, BitEnum128<T> value)
        {
            ValueLo = (ValueLo & (~mask.ValueLo)) | (value.ValueLo & mask.ValueLo);
            ValueHi = (ValueHi & (~mask.ValueHi)) | (value.ValueHi & mask.ValueHi);
        }

        public BitEnum128<T> GetValue(BitEnum128<T> mask)
        {
            ulong lo = ValueLo & mask.ValueLo;
            ulong hi = ValueHi & mask.ValueHi;
            return new BitEnum128<T>(lo, hi);
        }

        public static BitEnum128<T> operator &(BitEnum128<T> a, BitEnum128<T> b)
        {
            return new BitEnum128<T>(a.ValueLo & b.ValueLo, a.ValueHi & b.ValueHi);
        }

        public static BitEnum128<T> operator |(BitEnum128<T> a, BitEnum128<T> b)
        {
            return new BitEnum128<T>(a.ValueLo | b.ValueLo, a.ValueHi | b.ValueHi);
        }

        public static bool operator ==(BitEnum128<T> a, BitEnum128<T> b)
        {
            return a.ValueLo == b.ValueLo && a.ValueHi == b.ValueHi;
        }

        public static bool operator !=(BitEnum128<T> a, BitEnum128<T> b)
        {
            return a.ValueLo != b.ValueLo || a.ValueHi != b.ValueHi;
        }

        public bool IsZero()
        {
            return ValueLo == 0 && ValueHi == 0;
        }

        public bool SetBit(int idx, bool state)
        {
            //1. check            
            if (idx < 0 || idx >= LENGTH)
            {
                Log.Assert(false, "idx:{1} 要在 [0,{0})", LENGTH, idx);
                return false;
            }

            if (idx < C_COMP_LEN)
            {
                if (state)
                    ValueLo |= (1ul << idx);
                else
                    ValueLo &= ~(1ul << idx);
                return true;
            }

            idx -= C_COMP_LEN;
            if (state)
                ValueHi |= (1ul << idx);
            else
                ValueHi &= ~(1ul << idx);
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

            if (idx < C_COMP_LEN)
                return ((1ul << idx) & ValueLo) != 0;

            idx -= C_COMP_LEN;
            return ((1ul << idx) & ValueHi) != 0;
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
            ValueHi = state ? ulong.MaxValue : 0;
            ValueLo = ValueHi;
        }

        public int GetCount(bool v)
        {
            int ret = 0;
            ulong u64_v1 = v ? 1ul : 0ul;

            for (int i = 0; i < C_COMP_LEN; ++i)
            {
                ulong u64_v2 = (ValueLo >> i) & 0x1ul;
                if (u64_v2 == u64_v1)
                    ret++;
            }

            for (int i = 0; i < C_COMP_LEN; ++i)
            {
                ulong u64_v2 = (ValueHi >> i) & 0x1ul;
                if (u64_v2 == u64_v1)
                    ret++;
            }
            return ret;
        }

        public bool Equals(BitEnum128<T> other)
        {
            return ValueLo == other.ValueLo && ValueHi == other.ValueHi;
        }

        public override int GetHashCode()
        {
            int lo = (int)ValueLo ^ (int)(ValueLo >> 32);
            int hi = (int)ValueHi ^ (int)(ValueHi >> 32);
            return (lo * 397) ^ hi;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }



#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(BitEnum128<>), true)]
    public class EdBitEnum128Drawer : UnityEditor.PropertyDrawer
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

            var propertyLo = property.FindPropertyRelative("ValueLo");
            var propertyHi = property.FindPropertyRelative("ValueHi");
            ulong valueLo = propertyLo.ulongValue;
            ulong valueHi = propertyHi.ulongValue;

            if (_enum_name_values == null)
                _enum_name_values = GetEnumNameValues(targetObjectType);

            UnityEditor.EditorGUI.BeginProperty(position, label, property);
            UnityEditor.EditorGUI.BeginChangeCheck();

            ShowBitEnum(position, label, valueLo, valueHi, _enum_name_values, (newLo, newHi) =>
            {
                propertyLo.ulongValue = newLo;
                propertyHi.ulongValue = newHi;
                property.serializedObject.ApplyModifiedProperties();
            });

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
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
            ulong value_lo, ulong value_hi,
            List<(string name, int value)> name_values,
            Action<ulong, ulong> onChange)
        {
            if (label != null)
            {
                Rect fieldPos = UnityEditor.EditorGUI.PrefixLabel(position, label);
                string displayText = _GetDisplayText(value_lo, value_hi, name_values);

                if (GUI.Button(fieldPos, displayText, UnityEditor.EditorStyles.popup))
                {
                    _ShowMaskMenu(value_lo, value_hi, name_values, onChange);
                }
            }
            else
            {
                string displayText = _GetDisplayText(value_lo, value_hi, name_values);

                if (GUI.Button(position, displayText, UnityEditor.EditorStyles.popup))
                {
                    _ShowMaskMenu(value_lo, value_hi, name_values, onChange);
                }
            }
        }


        // 弹出多选菜单
        private static void _ShowMaskMenu(
            ulong current_lo,
            ulong current_hi,
            List<(string name, int value)> enum_name_values,
            Action<ulong, ulong> onChanged)
        {
            var menu = new UnityEditor.GenericMenu();
            bool is_all = true;
            foreach (var p in enum_name_values)
            {
                if (!_IsOn(current_lo, current_hi, p.value))
                {
                    is_all = false;
                    break;
                }
            }

            // Nothing
            menu.AddItem(new GUIContent("Nothing"), current_lo == 0, () => onChanged(0, 0));
            menu.AddItem(new GUIContent("Everything"), is_all, () => onChanged(ulong.MaxValue, ulong.MaxValue));
            menu.AddSeparator("");

            // 每一位            
            foreach (var p in enum_name_values)
            {
                bool on = _IsOn(current_lo, current_hi, p.value);
                menu.AddItem(new GUIContent(p.name), on, () =>
                {
                    if (p.value >= 0 && p.value < 63)
                    {
                        ulong newValue = current_lo ^ (1UL << p.value);
                        onChanged(newValue, current_hi);
                    }
                    else if (p.value >= 64 && p.value < 64)
                    {
                        ulong newValue = current_hi ^ (1UL << (p.value - 64));
                        onChanged(current_lo, newValue);
                    }
                });
            }

            menu.ShowAsContext();
        }

        private static bool _IsOn(ulong lo, ulong hi, int index)
        {
            if (index >= 0 && index < 64 && (lo & (1UL << index)) != 0)
            {
                return true;
            }
            else if (index >= 64 && index < 128 && (hi & (1UL << (index - 64))) != 0)
            {
                return true;
            }
            return false;
        }

        // 生成显示文本
        private static string _GetDisplayText(
            ulong value_lo,
            ulong value_hi,
            List<(string name, int value)> enum_values)
        {
            if (value_lo == 0 && value_hi == 0)
                return "Nothing";

            List<string> temp = new List<string>(enum_values.Count);


            foreach (var p in enum_values)
            {
                if (_IsOn(value_lo, value_hi, p.value))
                    temp.Add(p.name);
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
    public class EdOdinBitEnum128Drawer<T> : Sirenix.OdinInspector.Editor.OdinValueDrawer<BitEnum128<T>> where T :Enum
    {
        private static List<(string name, int value)> _enum_name_values;
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect();

            BitEnum128<T> old_v = ValueEntry.SmartValue;
            Type tar_type = old_v.GetType().GetGenericArguments()[0];

            if (_enum_name_values == null)
                _enum_name_values = EdBitEnum128Drawer.GetEnumNameValues(typeof(T));

            EdBitEnum128Drawer.ShowBitEnum(rect, label, old_v.ValueLo, old_v.ValueHi, _enum_name_values, (newLo,newHi)=>
            {
                ValueEntry.SmartValue = new BitEnum128<T>(newLo,newHi);
            });
        }
    }
#endif
}
