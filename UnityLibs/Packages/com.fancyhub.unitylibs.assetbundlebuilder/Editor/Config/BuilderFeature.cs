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
    public class ABBuildContext
    {
        public BuildTarget Target;
        public EBuildPipeline PipelineType;
        public BuilderParam BuilderParam;
        public bool Fake;
        public string OutputDir;


        // 中途的输出
        public AssetGraph AssetGraph;
    }

    /// <summary>
    /// 1/6 ab打包前,预处理
    /// </summary>
    public interface IPreBuild
    {
        void OnPreBuild(ABBuildContext context);
    }
    /// <summary>
    /// 1/6 ab打包前,预处理
    /// </summary>
    public abstract class BuilderPreBuild : ScriptableObject, IPreBuild
    {
        public bool Enable = true;
        public abstract void OnPreBuild(ABBuildContext context);
    }


    /// <summary>
    /// 2/6 ab打包前, asset 收集
    /// </summary>
    public interface IAssetCollector
    {
        public List<(string path, string address)> GetAllAssets(ABBuildContext context);

    }
    /// <summary>
    /// 2/6 ab打包前, asset 收集
    /// </summary>
    public abstract class BuilderAssetCollector : ScriptableObject, IAssetCollector
    {
        public bool Enable;
        public virtual IAssetCollector GetAssetCollector(ABBuildContext context) { return this; }
        public abstract List<(string path, string address)> GetAllAssets(ABBuildContext context);
        public abstract bool IsFake();
    }

    /// <summary>
    /// 3/6 ab打包前, 资源依赖的收集
    /// </summary>
    public interface IAssetDependency
    {
        List<string> CollectDirectDeps(string path, EAssetObjType asset_type);
        string FileGuid(string path);
        void OnFinishCollect();
    }
    /// <summary>
    /// 3/6 ab打包前, 资源依赖的收集
    /// </summary>
    public abstract class BuilderAssetDependency : ScriptableObject, IAssetDependency
    {
        public abstract bool IsFake();
        public virtual IAssetDependency GetAssetDependency() { return this; }
        public abstract string FileGuid(string path);
        public abstract List<string> CollectDirectDeps(string asset_path, EAssetObjType asset_type);
        public virtual void OnFinishCollect() { }
    }

    /// <summary>
    /// 4/6 ab打包前, 确定每个asset对应的ab名字
    /// </summary>
    public interface IBundleRuler
    {
        /// <summary>
        /// 获取 资源路径 对应的包名
        /// 如果返回空: 就找下一个 Ruler
        /// 如果最终为空, 就说明不需要明确指定, 通过自动分包, 如果该资源没有被任何 其他资源引用, 最终会报错
        /// </summary>        
        public string GetBundleName(ABBuildContext context, string asset_path, EAssetObjType asset_type, bool need_export);
    }
    /// <summary>
    /// 4/6 ab打包前, 确定每个asset对应的ab名字
    /// </summary>
    public abstract class BuilderBundleRuler : ScriptableObject, IBundleRuler
    {
        public bool Enable = true;
        public string RulerName;

        public abstract string GetBundleName(ABBuildContext context, string asset_path, EAssetObjType asset_type, bool need_export);

        public virtual IBundleRuler GetBundleRuler(ABBuildContext context)
        {
            return this;
        }
    }


    /// <summary>
    /// 5/6 ab打包后, 给每个ab包增加tags
    /// </summary>
    public interface ITagRuler
    {
        public void GetTags(ABBuildContext context, string bundle_name, List<string> assets_list, HashSet<string> out_tags);
    }

    /// <summary>
    /// 5/6 ab打包后, 给每个ab包增加tags
    /// </summary>
    public abstract class BuilderTagRuler : ScriptableObject, ITagRuler
    {
        public string Name;
        public bool Enable = true;

        public virtual ITagRuler GetTagRuler(ABBuildContext context)
        {
            return this;
        }

        public abstract void GetTags(ABBuildContext context, string bundle_name, List<string> assets_list, HashSet<string> out_tags);
    }


    /// <summary>
    /// 6/6 ab打包后, 后处理, 主要是处理manifest
    /// </summary>
    public interface IPostBuild
    {
        void OnPostBuild(ABBuildContext context);
    }
    /// <summary>
    /// 6/6 ab打包后, 后处理, 主要是处理manifest
    /// </summary>
    public abstract class BuilderPostBuild : ScriptableObject, IPostBuild
    {
        public bool Enable = true;
        public abstract void OnPostBuild(ABBuildContext context);
    }

}
