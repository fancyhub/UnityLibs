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
    /// <summary>
    /// 1/6 ab打包前,预处理
    /// </summary>
    public interface IPreBuild
    {
        void OnPreBuild(AssetBundleBuilderConfig config, UnityEditor.BuildTarget target);
    }
    /// <summary>
    /// 1/6 ab打包前,预处理
    /// </summary>
    public abstract class BuilderPreBuild : ScriptableObject, IPreBuild
    {
        public bool Enable = true;
        public abstract void OnPreBuild(AssetBundleBuilderConfig config, UnityEditor.BuildTarget target);
    }


    /// <summary>
    /// 2/6 ab打包前, asset 收集
    /// </summary>
    public interface IAssetCollector
    {
        List<(string path, string address)> GetAllAssets();
    }
    /// <summary>
    /// 2/6 ab打包前, asset 收集
    /// </summary>
    public abstract class BuilderAssetCollector : ScriptableObject, IAssetCollector
    {
        public virtual IAssetCollector GetAssetCollector() { return this; }
        public abstract List<(string path, string address)> GetAllAssets();
    }

    /// <summary>
    /// 3/6 ab打包前, 资源依赖的收集
    /// </summary>
    public interface IAssetDependency
    {
        List<string> CollectDirectDeps(string path, EAssetObjType asset_type);
        string FileGuid(string path);
    }
    /// <summary>
    /// 3/6 ab打包前, 资源依赖的收集
    /// </summary>
    public abstract class BuilderAssetDependency : ScriptableObject, IAssetDependency
    {
        public virtual string FileGuid(string path)
        {
            return UnityEditor.AssetDatabase.AssetPathToGUID(path);
        }

        public abstract List<string> CollectDirectDeps(string asset_path, EAssetObjType asset_type);
        public virtual IAssetDependency GetAssetDependency() { return this; }
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
        public string GetBundleName(string asset_path, EAssetObjType asset_type, bool need_export);
    }
    /// <summary>
    /// 4/6 ab打包前, 确定每个asset对应的ab名字
    /// </summary>
    public abstract class BuilderBundleRuler : ScriptableObject, IBundleRuler
    {
        public bool Enable = true;
        public string RulerName;

        public abstract string GetBundleName(string asset_path, EAssetObjType asset_type, bool need_export);

        public virtual IBundleRuler GetBundleRuler()
        {
            return Enable ? this : null;
        }
    }


    /// <summary>
    /// 5/6 ab打包后, 给每个ab包增加tags
    /// </summary>
    public interface ITagRuler
    {
        public void GetTags(string bundle_name, List<string> assets_list, HashSet<string> out_tags);
    }

    /// <summary>
    /// 5/6 ab打包后, 给每个ab包增加tags
    /// </summary>
    public abstract class BuilderTagRuler : ScriptableObject, ITagRuler
    {
        public string Name;
        public bool Enable = true;

        public virtual ITagRuler GetTagRuler()
        {
            return Enable ? this : null;
        }

        public abstract void GetTags(string bundle_name, List<string> assets_list, HashSet<string> out_tags);
    }


    public class PostBuildContext
    {
        public BuildTarget Target;
        public AssetBundleBuilderConfig Config;
        public AssetGraph AssetGraph;        
    }
    /// <summary>
    /// 6/6 ab打包后, 后处理, 主要是处理manifest
    /// </summary>
    public interface IPostBuild
    {
        void OnPostBuild(PostBuildContext context);
    }
    /// <summary>
    /// 6/6 ab打包后, 后处理, 主要是处理manifest
    /// </summary>
    public abstract class BuilderPostBuild : ScriptableObject, IPostBuild
    {
        public bool Enable = true;
        public abstract void OnPostBuild(PostBuildContext context);
    }


   

}
