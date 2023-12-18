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

namespace FH.AssetBundleBuilder.Ed
{
    [CreateAssetMenu(fileName = "AssetBundleBuilderConfig", menuName = "fanchhub/AssetBundleBuilderConfig")]
    public class AssetBundleBuilderConfig : ScriptableObject
    {
        private static AssetBundleBuilderConfig _Default;
        public const string CPath = "Assets/fancyhub/AssetBundleBuilderConfig.asset";
        public BuilderParam Android;
        public BuilderParam IOS;
        public BuilderParam Windows;
        public BuilderParam OSX;


        [HideInInspector] public BuilderAssetCollector AssetCollector;
        [HideInInspector] public BuilderAssetDependency AssetDependency;

        [HideInInspector] public List<BuilderBundleRuler> BundleRulers = new List<BuilderBundleRuler>();

        [HideInInspector] public List<BuilderPreBuild> PreBuild = new List<BuilderPreBuild>();

        [HideInInspector] public List<BuilderPostBuild> PostBuild = new List<BuilderPostBuild>();

        public IAssetCollector GetAssetCollector()
        {
            return AssetCollector.GetAssetCollector();
        }

        public IAssetDependency GetAssetDependency()
        {
            return AssetDependency.GetAssetDependency();
        }

        public IBundleRuler GetBundleRuler()
        {
            return new BuilderBundleRulerGroup(BundleRulers);
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
                        if (p == null || !p.Enable)
                            continue;
                        _Rulers.Add(p.GetBundleRuler());
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
        public string OutputDir;
    }
}
