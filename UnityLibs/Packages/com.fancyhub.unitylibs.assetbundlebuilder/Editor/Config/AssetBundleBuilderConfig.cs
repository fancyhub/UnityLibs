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
    public enum EBuildPipeline
    {
        Legacy,
        SBP_Simple,
        SBP_Custom,
    }
    [CreateAssetMenu(fileName = "AssetBundleBuilderConfig", menuName = "fancyhub/AssetBundleBuilderConfig")]
    public class AssetBundleBuilderConfig : ScriptableObject
    {
        private static AssetBundleBuilderConfig _Default;
        public const string CPath = "Assets/fancyhub/AssetBundleBuilderConfig.asset";

        [SerializeField]
        private string _OutputDir = "ProjTemp/Build/Output/{Target}";

        public EBuildPipeline PipelineType = EBuildPipeline.Legacy;
        public BuilderParam Android;
        public BuilderParam IOS;
        public BuilderParam Windows;
        public BuilderParam OSX;


        public string GetOutputDir(UnityEditor.BuildTarget target)
        {
            return _OutputDir.Replace("{Target}", target.Ext2DirName());
        }

        [HideInInspector] public List<BuilderAssetCollector> AssetCollector = new List<BuilderAssetCollector>();
        [HideInInspector] public List<BuilderAssetDependency> AssetDependency = new List<BuilderAssetDependency>();

        [HideInInspector] public List<BuilderBundleRuler> BundleRulers = new List<BuilderBundleRuler>();
        [HideInInspector] public List<BuilderTagRuler> TagRulers = new List<BuilderTagRuler>();

        [HideInInspector] public List<BuilderPreBuild> PreBuild = new List<BuilderPreBuild>();

        [HideInInspector] public List<BuilderPostBuild> PostBuild = new List<BuilderPostBuild>();

        public IAssetCollector GetAssetCollector(ABBuildContext context)
        {
            return new AssetCollectorGroup(AssetCollector, context);
        }

        public IAssetDependency GetAssetDependency(ABBuildContext context)
        {
            foreach (var p in AssetDependency)
            {
                if (p.IsFake() == context.Fake)
                    return p.GetAssetDependency();
            }
            Debug.LogError("AssetDependency 是空的", this);
            throw new Exception($"AssetDependency 是空的 {AssetDatabase.GetAssetPath(this)} ");
        }

        public IBundleRuler GetBundleRuler(ABBuildContext context)
        {
            return new BuilderBundleRulerGroup(BundleRulers,context);
        }

        public ITagRuler GetTagRuler(ABBuildContext context)
        {
            return new BuilderTagRulerGroup(TagRulers,context);
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
                    throw new NotImplementedException($"平台 {target} 未实现");
            }
        }

        private sealed class BuilderTagRulerGroup : ITagRuler
        {
            private List<ITagRuler> _Rulers;
            public BuilderTagRulerGroup(List<BuilderTagRuler> rulers, ABBuildContext context)
            {
                _Rulers = new List<ITagRuler>();
                if (rulers != null)
                {
                    foreach (var p in rulers)
                    {
                        if (p == null || !p.Enable)
                            continue;
                        var r = p.GetTagRuler(context);
                        if (r != null)
                            _Rulers.Add(r);
                    }
                }
            }
            public void GetTags(ABBuildContext context, string bundle_name, List<string> assets_list, HashSet<string> out_tags)
            {
                foreach (var p in _Rulers)
                {
                    p.GetTags(context,bundle_name, assets_list, out_tags);
                }
            }
        }


        private sealed class BuilderBundleRulerGroup : IBundleRuler
        {
            private List<IBundleRuler> _Rulers;
            public BuilderBundleRulerGroup(List<BuilderBundleRuler> rulers, ABBuildContext context)
            {
                _Rulers = new List<IBundleRuler>();
                if (rulers != null)
                {
                    foreach (var p in rulers)
                    {
                        if (p == null || !p.Enable)
                            continue;
                        var r = p.GetBundleRuler(context);
                        if (r != null)
                            _Rulers.Add(r);
                    }
                }

                UnityEngine.Debug.Assert(_Rulers.Count > 0, "规则为空");
            }

            public string GetBundleName(ABBuildContext context, string asset_path, EAssetObjType asset_type, bool need_export)
            {
                foreach (var p in _Rulers)
                {
                    string ret = p.GetBundleName(context, asset_path, asset_type, need_export);
                    if (ret != null)
                        return ret;
                }
                return null;
            }

        }


        private sealed class AssetCollectorGroup : IAssetCollector
        {
            private List<IAssetCollector> _Collectors;
            public AssetCollectorGroup(List<BuilderAssetCollector> collectors, ABBuildContext context)
            {
                _Collectors = new List<IAssetCollector>();
                if (collectors != null)
                {
                    foreach (var p in collectors)
                    {
                        if (p == null || !p.Enable || p.IsFake() != context.Fake)
                            continue;
                        var r = p.GetAssetCollector(context);
                        if (r != null)
                            _Collectors.Add(r);
                    }
                }

                UnityEngine.Debug.Assert(_Collectors.Count > 0, "Collector为空");
            }

            public List<(string path, string address)> GetAllAssets(ABBuildContext context)
            {
                List<List<(string path, string address)>> temp = new List<List<(string path, string address)>>();
                int count = 0;
                foreach (var p in _Collectors)
                {
                    var t = p.GetAllAssets(context);
                    if (t == null || t.Count == 0)
                        continue;
                    temp.Add(t);
                    count += t.Count;
                }
                List<(string path, string address)> ret = new List<(string path, string address)>(count);
                foreach (var p in temp)
                {
                    ret.AddRange(p);
                }
                return ret;
            }
        }
    }

    [Serializable]
    public partial class BuilderParam
    {
        public BuildAssetBundleOptions BuildOptions = //BuildAssetBundleOptions.UncompressedAssetBundle
                                            BuildAssetBundleOptions.ChunkBasedCompression
                                           | BuildAssetBundleOptions.StrictMode;

        [Serializable]
        public partial class SBPParamConfig
        {
            public UnityEngine.CompressionType Compression = UnityEngine.CompressionType.Lz4HC;
            public UnityEditor.Build.Content.ContentBuildFlags BuildFlags;
            public UnityEditor.Build.Player.ScriptCompilationOptions ScriptOptions;
            public bool UseCache = true;
            public string CacheServerHost;
            public int CacheServerPort = 8126;
            public bool WriteLinkXML;
        }

        public SBPParamConfig SBPParam = new SBPParamConfig();
    }
}
