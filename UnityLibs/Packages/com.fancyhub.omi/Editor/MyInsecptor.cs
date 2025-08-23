/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace FH.Omi.Editor
{
    //[CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.MonoBehaviour), true)]
    public class MyInspector : UnityEditor.Editor
    {
        protected MyTargetInfo _TargetInfo;

        protected virtual void OnEnable()
        {
            _TargetInfo = new MyTargetInfo(target, serializedObject);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

            OmiEditorGUI.DrawGroup(_TargetInfo.Group);

            serializedObject.ApplyModifiedProperties();
        }
    }

    public sealed class MyTargetInfo
    {
        public readonly UnityEngine.Object Target;
        public readonly SerializedObject SerializedObject;
        private List<MyMemberInfo> _MemberList;
        private GroupNode _Group;

        public MyTargetInfo(UnityEngine.Object target, SerializedObject so)
        {
            this.Target = target;
            this.SerializedObject = so;
        }

        public GroupNode Group
        {
            get
            {
                if (_Group != null)
                    return _Group;

                List<MyMemberInfo> member_list = MemberList;
                _Group = new GroupNode(null);

                foreach (var p in member_list)
                {
                    string path = null;
                    foreach (var attr in p.MemberInfo.GetCustomAttributes<GroupAttribute>())
                    {
                        path = attr.Path;
                        _Group.AddChildWithAttribute(attr);
                    }
                    _Group.AddMember(path, p);
                }

                return _Group;
            }
        }

        public List<MyMemberInfo> MemberList
        {
            get
            {
                if (_MemberList != null)
                    return _MemberList;

                _MemberList = _CreateMemberInfoList();
                return _MemberList;
            }
        }

        private List<MyMemberInfo> _CreateMemberInfoList()
        {
            var ret = new List<MyMemberInfo>();

            Type type = Target.GetType();

            Type tt = type;
            List<Type> all_parents = new List<Type>();
            for (; ; )
            {
                if (tt == typeof(UnityEngine.Object))
                    break;
                if (tt == typeof(System.Object))
                    break;

                all_parents.Add(tt);
                tt = tt.BaseType;
            }

            all_parents.Reverse();

            foreach (var t3 in all_parents)
            {
                var all_fields = t3.GetFields(System.Reflection.BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (var m in all_fields)
                {
                    var my_member_info = MyMemberInfo.CreateMember(this, m);
                    if (my_member_info != null)
                    {
                        ret.Add(my_member_info);
                    }
                }
            }

            {
                var all_prop = type.GetProperties(System.Reflection.BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (var m in all_prop)
                {
                    var my_member_info = MyMemberInfo.CreateMember(this, m);
                    if (my_member_info != null)
                    {

                        ret.Add(my_member_info);
                    }
                }
            }

            {
                var all_methods = type.GetMethods(System.Reflection.BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (var m in all_methods)
                {
                    var my_member_info = MyMemberInfo.CreateMember(this, m);
                    if (my_member_info != null)
                    {

                        ret.Add(my_member_info);
                    }
                }
            }

            return ret;
        }
    }

    public abstract class MyMemberInfo
    {
        public readonly MyTargetInfo Target;
        public readonly System.Reflection.MemberInfo MemberInfo;
        internal MyMemberInfo(MyTargetInfo target, System.Reflection.MemberInfo member_info)
        {
            this.Target = target;
            this.MemberInfo = member_info;
        }

        public string Name { get { return MemberInfo.Name; } }

        public static MyMemberInfo CreateMember(MyTargetInfo target_info, System.Reflection.MemberInfo member)
        {
            if (member is System.Reflection.FieldInfo field_info)
            {
                string ser_name = field_info.Name;
                UnityEngine.Serialization.FormerlySerializedAsAttribute attr = field_info.GetCustomAttribute<UnityEngine.Serialization.FormerlySerializedAsAttribute>();
                if (attr != null)
                    ser_name = attr.oldName;
                var ser_p = target_info.SerializedObject.FindProperty(ser_name);
                if (ser_p != null)
                    return new MyFieldInfo(target_info, field_info, ser_p);

                var cust_attr = field_info.GetCustomAttribute<BaseAttribute>();
                if (cust_attr == null)
                    return null;
                return new MyFieldInfo(target_info, field_info, null);
            }
            else if (member is System.Reflection.PropertyInfo prop_info)
            {
                var cust_attr = prop_info.GetCustomAttribute<BaseAttribute>();
                if (cust_attr == null)
                    return null;
                return new MyPropertyInfo(target_info, prop_info);
            }
            else if (member is System.Reflection.MethodInfo method_info)
            {
                var cust_attr = method_info.GetCustomAttribute<BaseAttribute>();
                if (cust_attr == null)
                    return null;
                return new MyMethodInfo(target_info, method_info);
            }
            return null;
        }

    }

    public sealed class MyFieldInfo : MyMemberInfo
    {
        public readonly System.Reflection.FieldInfo FieldInfo;
        public readonly SerializedProperty SerializedProperty;

        internal MyFieldInfo(MyTargetInfo target, System.Reflection.FieldInfo field_info, SerializedProperty prop) : base(target, field_info)
        {
            this.FieldInfo = field_info;
            this.SerializedProperty = prop;
        }

    }

    public sealed class MyPropertyInfo : MyMemberInfo
    {
        public readonly System.Reflection.PropertyInfo PropertyInfo;

        internal MyPropertyInfo(MyTargetInfo target, System.Reflection.PropertyInfo prop_info) : base(target, prop_info)
        {
            PropertyInfo = prop_info;
        }


    }

    public sealed class MyMethodInfo : MyMemberInfo
    {
        public readonly System.Reflection.MethodInfo MethodInfo;

        internal MyMethodInfo(MyTargetInfo target, System.Reflection.MethodInfo method_info) : base(target, method_info)
        {
            MethodInfo = method_info;
        }
    }
}