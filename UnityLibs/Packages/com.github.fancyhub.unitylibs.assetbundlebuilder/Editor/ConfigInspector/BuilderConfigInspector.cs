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
using FH.Ed;

namespace FH.AssetBundleBuilder.Ed
{

    [CustomEditor(typeof(AssetBundleBuilderConfig))]
    public class BuilderConfigInspector : Editor
    {
        private InnerSOListDrawer<BuilderAssetCollector> _AssetCollectDrawer;
        private InnerSOListDrawer<BuilderAssetDependency> _AssetDependcyDrawer;
        private InnerSOListDrawer<BuilderBundleRuler> _BundleRulerDrawer;
        private InnerSOListDrawer<BuilderTagRuler> _TagRulerDrawer;
        private InnerSOListDrawer<BuilderPreBuild> _PreBuildDrawer;
        private InnerSOListDrawer<BuilderPostBuild> _PostBuildDrawer;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _BuildDrawers();

            EditorGUILayout.Space(10);
            _PreBuildDrawer.Draw("[Pre Build]");


            EditorGUILayout.Space(10);

            _AssetCollectDrawer.Draw("[Asset Collector](First Enable)");


            EditorGUILayout.Space(10);
            _AssetDependcyDrawer.Draw("[Asset Dependcy](First Enable)");


            EditorGUILayout.Space(10);
            _BundleRulerDrawer.Draw("[Bundle Rulers]");



            EditorGUILayout.Space(10);
            _TagRulerDrawer.Draw("[Tag Rulers]");


            EditorGUILayout.Space(10);
            _PostBuildDrawer.Draw("[Post Build]");
        }


        private void _BuildDrawers()
        {
            AssetBundleBuilderConfig config = (AssetBundleBuilderConfig)target;

            if (_AssetCollectDrawer == null)
                _AssetCollectDrawer = new InnerSOListDrawer<BuilderAssetCollector>(ref config.AssetCollector, AssetDatabase.GetAssetPath(target), "Add Asset Collector", null);

            if (_PreBuildDrawer == null)
            {
                _PreBuildDrawer = new InnerSOListDrawer<BuilderPreBuild>(ref config.PreBuild, AssetDatabase.GetAssetPath(target), "Add Pre Build", (a) =>
                {
                    return (a.Enable ? "√" : "X") + " : " + a.GetType().Name;
                });
            }

            if (_AssetDependcyDrawer == null)
                _AssetDependcyDrawer = new InnerSOListDrawer<BuilderAssetDependency>(ref config.AssetDependency, AssetDatabase.GetAssetPath(target), "Add Asset Dependency", null);

            if (_BundleRulerDrawer == null)
            {
                _BundleRulerDrawer = new InnerSOListDrawer<BuilderBundleRuler>(ref config.BundleRulers, AssetDatabase.GetAssetPath(target), "Add Bundle Rule", (a) =>
                {
                    return (a.Enable ? "√" : "X") + " : " + (string.IsNullOrEmpty(a.RulerName) ? a.GetType().Name : a.RulerName);
                });
            }

            if (_TagRulerDrawer == null)
            {
                _TagRulerDrawer = new InnerSOListDrawer<BuilderTagRuler>(ref config.TagRulers, AssetDatabase.GetAssetPath(target), "Add Tag Rule", (a) =>
                {
                    return (a.Enable ? "√" : "X") + " : " + (string.IsNullOrEmpty(a.Name) ? a.GetType().Name : a.Name);
                });
            }

            if (_PostBuildDrawer == null)
            {
                _PostBuildDrawer = new InnerSOListDrawer<BuilderPostBuild>(ref config.PostBuild, AssetDatabase.GetAssetPath(target), "Add Post Build", (a) =>
                {
                    return (a.Enable ? "√" : "X") + " : " + a.GetType().Name;
                });
            }
        }
    }
}
