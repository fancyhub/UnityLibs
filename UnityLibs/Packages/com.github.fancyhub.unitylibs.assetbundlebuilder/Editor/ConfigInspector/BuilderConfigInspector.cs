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


namespace FH.AssetBundleBuilder.Ed
{
    [CustomEditor(typeof(AssetBundleBuilderConfig))]
    public class BuilderConfigInspector : Editor
    {
        private Dictionary<BuilderFeature, Editor> _Dict = new Dictionary<BuilderFeature, Editor>();
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AssetBundleBuilderConfig config = (AssetBundleBuilderConfig)target;

            EditorGUILayout.HelpBox("Features", MessageType.Info);

            if (config.Features != null)
            {
                foreach (var p in config.Features)
                {
                    p.Show = EditorGUILayout.BeginFoldoutHeaderGroup(p.Show, p.GetType().Name, EditorStyles.foldout, _OnFeatureActionShow);
                    EditorGUILayout.EndFoldoutHeaderGroup();

                    if (p.Show)
                    {
                        EditorGUI.indentLevel++;
                        _Dict.TryGetValue(p, out var subEditor);
                        if(subEditor==null)
                        {
                            subEditor = Editor.CreateEditor(p);
                            _Dict.Add(p, subEditor);
                        }                        
                        subEditor.OnInspectorGUI();                        
                        EditorGUI.indentLevel--;
                    }
                }
            }
            if (GUILayout.Button("Add Feature"))
            {
                _GetGenericMenu().ShowAsContext();
            }
        }

        private void _OnFeatureActionShow(Rect position)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Move Up"), false, null);
            menu.DropDown(position);
        }


        private GenericMenu _GenericMenu;
        private GenericMenu _GetGenericMenu()
        {
            if (_GenericMenu != null)
                return _GenericMenu;

            _GenericMenu = new GenericMenu();
            var typeList = UnityEditor.TypeCache.GetTypesDerivedFrom<BuilderFeature>();
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
            AssetDatabase.AddObjectToAsset(obj, AssetDatabase.GetAssetPath(target));
            AssetBundleBuilderConfig config = (AssetBundleBuilderConfig)target;
            config.Features.Add(obj as BuilderFeature);
        }
    }
}
