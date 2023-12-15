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
using JetBrains.Annotations;

namespace FH.AssetBundleBuilder.Ed
{
    [CustomEditor(typeof(AssetBundleBuilderConfig))]
    public class BuilderConfigInspector : Editor
    {
        private InnerSOListDrawer<BuilderBundleRuler> _BundleRullerDrawer;
        private InnerSODrawer<BuilderAssetCollector> _AssetCollectDrawer;
        private InnerSODrawer<BuilderAssetDependency> _AssetDependcyDrawer;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AssetBundleBuilderConfig config = (AssetBundleBuilderConfig)target;

            if (_AssetCollectDrawer == null)
                _AssetCollectDrawer = new InnerSODrawer<BuilderAssetCollector>(AssetDatabase.GetAssetPath(target));
            EditorGUILayout.Space(10);
            _AssetCollectDrawer.Draw("AssetCollector", ref config.AssetCollector);

            if (_AssetDependcyDrawer == null)
                _AssetDependcyDrawer = new InnerSODrawer<BuilderAssetDependency>(AssetDatabase.GetAssetPath(target));
            EditorGUILayout.Space(10);
            _AssetDependcyDrawer.Draw("AssetDependcy", ref config.AssetDependency);

            EditorGUILayout.Space(10);
            if (_BundleRullerDrawer == null)
            {
                _BundleRullerDrawer = InnerScriptableObjectDrawer.Create(ref config.BundleRulers, AssetDatabase.GetAssetPath(target), "Add Rule");
            }
            _BundleRullerDrawer.Draw();
        }
    }


    public static class InnerScriptableObjectDrawer
    {
        public static InnerSOListDrawer<T> Create<T>(ref List<T> targets, string path, string button_name) where T : ScriptableObject
        {
            if (targets == null)
                targets = new List<T>();
            return new InnerSOListDrawer<T>(targets, path, button_name);
        }
    }

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
                _Show = EditorGUILayout.Foldout(_Show, target.GetType().Name);
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

    public class InnerSOListDrawer<T> where T : ScriptableObject
    {
        private string _ButtonName;
        private string _Path;
        private List<T> _List;

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

            public InnerEditor(List<T> list, T target, string path)
            {
                _List = list;
                _Target = target;
                _SubEditor = Editor.CreateEditor(target);
                _Path = path;
            }

            public void Draw()
            {
                _Show = EditorGUILayout.BeginFoldoutHeaderGroup(_Show, _Target.GetType().Name, EditorStyles.foldout, _OnFeatureActionShow);
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

        public InnerSOListDrawer(List<T> list, string path, string button_name)
        {
            _ButtonName = button_name;
            _List = list;
            _Path = path;
        }
        public void Draw()
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
                    _EditorDict.Add(p.GetInstanceID(), new InnerEditor(_List, p, _Path));
                }
            }


            //4. 渲染
            foreach (var p in _List)
            {
                _EditorDict.TryGetValue(p.GetInstanceID(), out var t);
                t?.Draw();
            }

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
