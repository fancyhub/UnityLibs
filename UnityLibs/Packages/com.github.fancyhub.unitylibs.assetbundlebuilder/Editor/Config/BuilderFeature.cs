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
    public interface IPreBuild
    {
        void OnPreBuild(AssetBundleBuilderConfig config, UnityEditor.BuildTarget target);
    }

    public class PostBuildContext
    {
        public BuildTarget Target;
        public AssetBundleBuilderConfig Config;
        public AssetGraph AssetGraph;
        public AssetBundleManifest Manifest;
        public AssetBundleBuild[] BundleBuildArray;
    }

    public interface IPostBuild
    {
        void OnPostBuild(PostBuildContext context);
    }

    public interface IAssetCollector
    {
        List<(string path, string address)> GetAllAssets();
    }

    public interface IBundleRuler
    {
        /// <summary>
        /// 获取 资源路径 对应的包名
        /// 如果返回空: 就找下一个 Ruler
        /// 如果最终为空, 就说明不需要明确指定, 通过自动分包, 如果该资源没有被任何 其他资源引用, 最终会报错
        /// </summary>        
        public string GetBundleName(string asset_path, EAssetObjType asset_type, bool need_export);
    }

    public abstract class BuilderFeature : ScriptableObject
    {
        public bool Enable = true;
    }

    public abstract class BuilderAssetCollector : ScriptableObject, IAssetCollector
    {
        public virtual IAssetCollector GetAssetCollector() { return this; }
        public abstract List<(string path, string address)> GetAllAssets();
    }

    public abstract class BuilderAssetDependency : ScriptableObject, IAssetDependency
    {
        public virtual string FileGuid(string path)
        {
            return UnityEditor.AssetDatabase.AssetPathToGUID(path);
        }

        public abstract List<string> CollectDirectDeps(string asset_path, EAssetObjType asset_type);
        public virtual IAssetDependency GetAssetDependency() { return this; }
    }

    public abstract class BuilderBundleRuler : ScriptableObject, IBundleRuler
    {
        public bool Enable = true;
        public string RulerName;

        public abstract string GetBundleName(string asset_path, EAssetObjType asset_type, bool need_export);

        public virtual IBundleRuler GetBundleRuler() { return this; }
    }

    public abstract class BuilderPreBuild : ScriptableObject, IPreBuild
    {
        public abstract void OnPreBuild(AssetBundleBuilderConfig config, UnityEditor.BuildTarget target);
    }

    public abstract class BuilderPostBuild : ScriptableObject, IPostBuild
    {
        public abstract void OnPostBuild(PostBuildContext context);
    }
}
