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
    public static partial class OmiEditorGUI
    {
        public static void Draw(MyMemberInfo info)
        {
            if (info is MyFieldInfo field_info)
            {
                _DrawFieldInfo(field_info);
            }
            else if (info is MyMethodInfo methodInfo)
            {
                _DrawMethodInfo(methodInfo);
            }
        }

        private static void _DrawMemberList(List<MyMemberInfo> member_list)
        {
            if (member_list == null)
                return;
            foreach (var p in member_list)
                Draw(p);
        }

        private static void _DrawFieldInfo(MyFieldInfo field_info)
        {
            if (field_info.SerializedProperty != null)
            {
                EditorGUILayout.PropertyField(field_info.SerializedProperty);
            }
        }

        private static void _DrawMethodInfo(MyMethodInfo methodInfo)
        {
            foreach (var p in methodInfo.MethodInfo.GetCustomAttributes<BaseAttribute>())
            {
                if (p is ButtonAttribute button)
                {
                    string btn_name = button.Text;
                    if (string.IsNullOrEmpty(btn_name))
                    {
                        btn_name = methodInfo.MethodInfo.Name;
                    }

                    if (GUILayout.Button(btn_name))
                    {
                        methodInfo.MethodInfo.Invoke(methodInfo.Target.Target, null);
                    }
                }
            }
        }
    }
}