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
    public class InnerSOListDrawer<T> where T : ScriptableObject
    {
        private string _ButtonName;
        private string _Path;
        private List<T> _List;
        private Func<T, string> _FuncNameGetter;
        private HashSet<int> TempSet = new HashSet<int>();
        private List<int> TempList = new List<int>();

        private Dictionary<int, InnerEditor> _EditorDict = new Dictionary<int, InnerEditor>();

        private class InnerEditor
        {
            private string _Path;
            private bool _Show;
            private T _Target;
            private List<T> _List;
            private Editor _SubEditor;
            private Func<T, string> _FuncNameGetter;

            public InnerEditor(List<T> list, T target, string path, Func<T, string> funcNameGetter)
            {
                _List = list;
                _Target = target;
                _SubEditor = Editor.CreateEditor(target);
                _Path = path;
                _FuncNameGetter = funcNameGetter;

            }

            public void Draw()
            {
                string name = null;
                if (_FuncNameGetter == null)
                    name = _Target.GetType().Name;
                else
                    name = _FuncNameGetter(_Target);
                
                _Show = EditorGUILayout.BeginFoldoutHeaderGroup(_Show, name, EditorStyles.foldout, _OnFeatureActionShow);
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (_Show)
                {
                    EditorGUI.indentLevel++;
                    _SubEditor.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }
            }

            private void _OnFeatureActionShow(Rect position)
            {
                int index = _List.IndexOf(_Target);
                if (index < 0)
                    return;

                var menu = new GenericMenu();
                if (index > 0)
                    menu.AddItem(new GUIContent("Move Up"), false, _OnActionMoveUp);
                if (index < _List.Count - 1)
                    menu.AddItem(new GUIContent("Move Down"), false, _OnActionMoveDown);
                menu.AddItem(new GUIContent("Remove"), false, _OnActionRemove);
                menu.DropDown(position);
            }

            private void _OnActionMoveUp()
            {
                int index = _List.IndexOf(_Target);
                if (index < 1)
                    return;
                var t = _List[index - 1];
                _List[index - 1] = _Target;
                _List[index] = t;
                AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(_Path));
            }

            private void _OnActionMoveDown()
            {
                int index = _List.IndexOf(_Target);
                if (index < 0 || index >= _List.Count - 1)
                    return;

                var t = _List[index + 1];
                _List[index + 1] = _Target;
                _List[index] = t;
                AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(_Path));
            }

            private void _OnActionRemove()
            {
                int index = _List.IndexOf(_Target);
                if (index >= 0)
                    _List.RemoveAt(index);

                AssetDatabase.RemoveObjectFromAsset(_Target);
                AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(_Path));
            }
        }

        public InnerSOListDrawer(ref List<T> list, string path, string button_name,Func<T,string> func_name_getter)
        {
            _ButtonName = button_name;
            if (list == null)
                list = new List<T>();
            _List = list;
            _Path = path;
            _FuncNameGetter = func_name_getter;
        }
        public void Draw(string title)
        {
            //1. 清除List里面空的对象, 清除重复
            {
                TempSet.Clear();
                for (int i = _List.Count - 1; i >= 0; i--)
                {
                    if (_List[i] == null || !TempSet.Add(_List[i].GetInstanceID()))
                        _List.RemoveAt(i);
                }
            }

            //2. 移除 Dict 里面已经不存在的
            {
                TempList.Clear();
                foreach (var p in _EditorDict)
                {
                    if (!TempSet.Contains(p.Key))
                        TempList.Add(p.Key);
                }
                foreach (var p in TempList)
                    _EditorDict.Remove(p);
                TempSet.Clear();
                TempList.Clear();
            }

            //3. Dict添加不存在的
            {
                foreach (var p in _List)
                {
                    if (_EditorDict.ContainsKey(p.GetInstanceID()))
                        continue;
                    _EditorDict.Add(p.GetInstanceID(), new InnerEditor(_List, p, _Path,_FuncNameGetter));
                }
            }


            //4. 渲染
            EditorGUILayout.TextField(title, EditorStyles.label);
            EditorGUI.indentLevel++;
            foreach (var p in _List)
            {
                _EditorDict.TryGetValue(p.GetInstanceID(), out var t);
                t?.Draw();
            }
            EditorGUI.indentLevel--;

            //5. 按钮
            if (GUILayout.Button(_ButtonName))
            {
                _GetGenericMenu().ShowAsContext();
            }
        }

        private GenericMenu _GenericMenu;
        private GenericMenu _GetGenericMenu()
        {
            if (_GenericMenu != null)
                return _GenericMenu;

            _GenericMenu = new GenericMenu();
            var typeList = UnityEditor.TypeCache.GetTypesDerivedFrom<T>();
            foreach (var p in typeList)
            {
                if (p.IsAbstract)
                    continue;
                _GenericMenu.AddItem(new GUIContent(p.Name), false, _AddFeatureCallBack, p);
            }

            return _GenericMenu;
        }

        private void _AddFeatureCallBack(object userData)
        {
            Type t = userData as Type;
            var obj = ScriptableObject.CreateInstance(t);
            obj.name = t.Name;
            AssetDatabase.AddObjectToAsset(obj, _Path);
            _List.Add(obj as T);

            AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(_Path));
        }
    }
}
