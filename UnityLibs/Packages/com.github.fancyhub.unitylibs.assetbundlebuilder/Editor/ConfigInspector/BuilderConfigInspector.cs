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

        private InnerSODrawer<BuilderAssetCollector> _AssetCollectDrawer;
        private InnerSODrawer<BuilderAssetDependency> _AssetDependcyDrawer;
        private InnerSOListDrawer<BuilderBundleRuler> _BundleRullerDrawer;
        private InnerSOListDrawer<BuilderPreBuild> _PreBuildDrawer;
        private InnerSOListDrawer<BuilderPostBuild> _PostBuildDrawer;

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

            
            if (_BundleRullerDrawer == null)
            {
                _BundleRullerDrawer = new InnerSOListDrawer<BuilderBundleRuler>(ref config.BundleRulers, AssetDatabase.GetAssetPath(target), "Add Rule", (a) =>
                {
                    return a.RulerName;
                });
            }
            EditorGUILayout.Space(10);
            _BundleRullerDrawer.Draw("Bundle Rulers");

            if (_PreBuildDrawer == null)
            {
                _PreBuildDrawer = new InnerSOListDrawer<BuilderPreBuild>(ref config.PreBuild, AssetDatabase.GetAssetPath(target), "Add Pre Build", null);
            }
            EditorGUILayout.Space(10);
            _PreBuildDrawer.Draw("Pre Build");


            if (_PostBuildDrawer == null)
            {
                _PostBuildDrawer = new InnerSOListDrawer<BuilderPostBuild>(ref config.PostBuild, AssetDatabase.GetAssetPath(target), "Add Post Build", null);
            }
            EditorGUILayout.Space(10);
            _PostBuildDrawer.Draw("Post Build");        
        }
    }
}
