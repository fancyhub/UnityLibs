/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FH.Ed
{
   
    public class InnerSODrawer<T> where T : ScriptableObject
    {
        private string _Path;
        public List<Type> _TypeList;
        public string[] _NameList;

        private T _LastTarget;
        private Editor _Editor;
        private bool _Show;

        public InnerSODrawer(string path)
        {
            _Path = path;
            var typeList = UnityEditor.TypeCache.GetTypesDerivedFrom<T>();
            _TypeList = new List<Type>(typeList.Count);

            foreach (var p in typeList)
            {
                if (p.IsAbstract)
                    continue;
                _TypeList.Add(p);
            }

            _NameList = new string[_TypeList.Count + 1];
            _NameList[0] = "[None]";

            for (int i = 0; i < _TypeList.Count; i++)
            {
                _NameList[i + 1] = _TypeList[i].Name;
            }
        }

        public void Draw(string lable, ref T target)
        {
            
            int selectIndex = -1;
            if (target != null)
            {
                selectIndex = _TypeList.IndexOf(target.GetType());
                if (selectIndex >= 0)
                    selectIndex += 1;
            }

            int index = EditorGUILayout.Popup(lable, selectIndex, _NameList);
            if (index != selectIndex)
            {
                if (target != null)
                {
                    AssetDatabase.RemoveObjectFromAsset(target);
                    target = null;
                }

                if (index > 0)
                {
                    Type t = _TypeList[index - 1];
                    target = ScriptableObject.CreateInstance(t) as T;
                    target.name = t.Name;
                    AssetDatabase.AddObjectToAsset(target, _Path);
                }
                AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(_Path));
            }

            if (target == null)
                return;

            if (_Editor == null || target != _LastTarget)
            {
                _Editor = Editor.CreateEditor(target);
                _LastTarget = target;
            }
             
            if (_Editor != null)
            {
                EditorGUI.indentLevel++;
                _Show = EditorGUILayout.InspectorTitlebar(_Show, target);
                if (_Show)
                {
                    EditorGUI.indentLevel++;
                    _Editor.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }
    }    
}
