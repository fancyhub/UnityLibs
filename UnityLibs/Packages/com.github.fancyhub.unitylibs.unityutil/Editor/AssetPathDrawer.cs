using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace FH.Ed
{
    [CustomPropertyDrawer(typeof(AssetPath<>), true)]
    public class AssetPathTDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type type_ref = fieldInfo.FieldType;
            if (typeof(System.Collections.IList).IsAssignableFrom(type_ref))
            {
                if (type_ref.IsArray)
                    type_ref = type_ref.GetElementType();
                else
                    type_ref = type_ref.GetGenericArguments()[0];
            }

            Type tar_type = type_ref.GetGenericArguments()[0];
            SerializedProperty prop = property.FindPropertyRelative("Path");
            string path = prop.stringValue;

            UnityEngine.Object cur_obj = AssetDatabase.LoadAssetAtPath(path, tar_type);
            UnityEngine.Object new_obj = cur_obj;
            if (label == null)
                new_obj = EditorGUI.ObjectField(position, cur_obj, tar_type, false);
            else
                new_obj = EditorGUI.ObjectField(position, label, cur_obj, tar_type, false);          

            if (new_obj == cur_obj)
                return;
            if (new_obj == null)
            {
                prop.stringValue = string.Empty;
            }
            else
            {
                prop.stringValue = AssetDatabase.GetAssetPath(new_obj);
            }
        }
    }


    // Odin
    /*
    public class OdinAssetPathDrawer<T> : OdinValueDrawer<AssetPath<T>> where T : UnityEngine.Object
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect();

            AssetPath<T> selfVal = ValueEntry.SmartValue;
            Type tar_type = selfVal.GetType().GetGenericArguments()[0];

            T old_obj = selfVal.EdLoad();
            UnityEngine.Object new_obj = old_obj;
            if (label != null)
            {
                new_obj = EditorGUI.ObjectField(rect, label, old_obj, tar_type, false);
            }
            else
                new_obj = EditorGUI.ObjectField(rect, old_obj, tar_type, false);

            if (old_obj != new_obj)
            {
                selfVal.EdSet((T)new_obj);
                ValueEntry.SmartValue = selfVal;
            }
        }
    }
    //*/
}