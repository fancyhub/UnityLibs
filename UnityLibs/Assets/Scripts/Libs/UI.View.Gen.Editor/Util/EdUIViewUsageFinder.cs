using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.View.Gen.ED
{
    /// <summary>
    /// 用来查找 View 之间的引用关系
    /// </summary>
    public static class EdUIViewUsageFinder
    {
        public static Dictionary<Type, Type> Find(Type base_ui_view_type)
        {
            Dictionary<Type, Type> all_types = new();

            //1. 找到所有的View的子类
            var ui_view_type_collection = UnityEditor.TypeCache.GetTypesDerivedFrom(base_ui_view_type);
            foreach (var p in ui_view_type_collection)
            {
                all_types.Add(p, null);
            }

            //2. 分析View的所有成员变量, 获取他们之间的引用关系            
            List<Type> type_list = new List<Type>(all_types.Keys);
            foreach (var p in type_list)
            {
                //2.1 检查父类
                _AddClassRef(p.BaseType, p, ref all_types);

                //2.2 分析所有的成员变量
                System.Reflection.FieldInfo[] all_fields = p.GetFields();
                foreach (var f in all_fields)
                {
                    var field_type = f.FieldType;
                    if (_AddClassRef(field_type, p, ref all_types))
                        continue;

                    if (field_type.IsArray && _AddClassRef(field_type.GetElementType(), p, ref all_types))
                        continue;

                    if (field_type.IsGenericType)
                    {
                        foreach (Type t in field_type.GetGenericArguments())
                        {
                            _AddClassRef(t, p, ref all_types);
                        }
                    }
                }
            }

            return all_types;
        }

        private static bool _AddClassRef(Type type, Type container_type, ref Dictionary<Type, Type> all_types)
        {
            if (!all_types.ContainsKey(type))
                return false;

            all_types[type] = container_type;
            return true;
        }
    }
}
