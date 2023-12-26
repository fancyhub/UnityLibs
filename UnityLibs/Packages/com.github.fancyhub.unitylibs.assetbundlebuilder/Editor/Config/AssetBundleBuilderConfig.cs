/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FH.Ed;

namespace FH.AssetBundleBuilder.Ed
{
    [CreateAssetMenu(fileName = "AssetBundleBuilderConfig", menuName = "fanchhub/AssetBundleBuilderConfig")]
    public class AssetBundleBuilderConfig : ScriptableObject
    {
        private static AssetBundleBuilderConfig _Default;
        public const string CPath = "Assets/fancyhub/AssetBundleBuilderConfig.asset";

        public string OutputDir;

        public BuilderParam Android;
        public BuilderParam IOS;
        public BuilderParam Windows;
        public BuilderParam OSX;

         
        public string GetOutputDir(UnityEditor.BuildTarget target)
        {
            return System.IO.Path.Combine(OutputDir, target.Ext2Name());
        }

        [HideInInspector] public List<BuilderAssetCollector> AssetCollector = new List<BuilderAssetCollector>();
        [HideInInspector] public List<BuilderAssetDependency> AssetDependency = new List<BuilderAssetDependency>();

        [HideInInspector] public List<BuilderBundleRuler> BundleRulers = new List<BuilderBundleRuler>();
        [HideInInspector] public List<BuilderTagRuler> TagRulers = new List<BuilderTagRuler>();

        [HideInInspector] public List<BuilderPreBuild> PreBuild = new List<BuilderPreBuild>();

        [HideInInspector] public List<BuilderPostBuild> PostBuild = new List<BuilderPostBuild>();

        public IAssetCollector GetAssetCollector()
        {
            if (AssetCollector.Count == 0)
                return null;
            return AssetCollector[0].GetAssetCollector();
        }

        public IAssetDependency GetAssetDependency()
        {
            if (AssetDependency.Count == 0)
                return null;
            return AssetDependency[0].GetAssetDependency();
        }

        public IBundleRuler GetBundleRuler()
        {
            return new BuilderBundleRulerGroup(BundleRulers);
        }

        public ITagRuler GetTagRuler()
        {
            return new BuilderTagRulerGroup(TagRulers);
        }

        public static AssetBundleBuilderConfig GetDefault()
        {
            if (_Default != null)
                return _Default;
            _Default = AssetDatabase.LoadAssetAtPath<AssetBundleBuilderConfig>(CPath);
            if (_Default == null)
            {
                Debug.LogError("加载 AssetBundleBuilderConfig 失败 " + CPath);
            }
            return _Default;
        }

        public BuilderParam GetBuilderParam(UnityEditor.BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return Android;
                case BuildTarget.iOS:
                    return IOS;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return Windows;
                case BuildTarget.StandaloneOSX:
                    return OSX;
                default:
                    return null;
            }
        }

        private sealed class BuilderTagRulerGroup : ITagRuler
        {
            private List<ITagRuler> _Rulers;
            public BuilderTagRulerGroup(List<BuilderTagRuler> rulers)
            {
                _Rulers = new List<ITagRuler>();
                if (rulers != null)
                {
                    foreach (var p in rulers)
                    {
                        if (p == null)
                            continue;
                        var r = p.GetTagRuler();
                        if (r != null)
                            _Rulers.Add(r);
                    }
                }
            }
            public void GetTags(string bundle_name, List<string> assets_list, HashSet<string> out_tags)
            {
                foreach (var p in _Rulers)
                {
                    p.GetTags(bundle_name, assets_list, out_tags);
                }
            }
        }


        private sealed class BuilderBundleRulerGroup : IBundleRuler
        {
            private List<IBundleRuler> _Rulers;
            public BuilderBundleRulerGroup(List<BuilderBundleRuler> rulers)
            {
                _Rulers = new List<IBundleRuler>();
                if (rulers != null)
                {
                    foreach (var p in rulers)
                    {
                        if (p == null)
                            continue;
                        var r = p.GetBundleRuler();
                        if (r != null)
                            _Rulers.Add(r);
                    }
                }

                UnityEngine.Debug.Assert(_Rulers.Count > 0, "规则为空");
            }

            public string GetBundleName(string asset_path, EAssetObjType asset_type, bool need_export)
            {
                foreach (var p in _Rulers)
                {
                    string ret = p.GetBundleName(asset_path, asset_type, need_export);
                    if (ret != null)
                        return ret;
                }
                return null;
            }
        }
    }

    [Serializable]
    public class BuilderParam
    {
        public BuildAssetBundleOptions BuildOptions = //BuildAssetBundleOptions.UncompressedAssetBundle
                                            BuildAssetBundleOptions.ChunkBasedCompression
                                           | BuildAssetBundleOptions.StrictMode;
    }
}
