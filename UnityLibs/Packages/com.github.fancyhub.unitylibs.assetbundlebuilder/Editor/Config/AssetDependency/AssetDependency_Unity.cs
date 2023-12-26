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

    public class AssetDependency_Unity : BuilderAssetDependency, IAssetDependency
    {
        public List<string> IgnoreAssetPathList = new List<string>();
        public List<string> IngoreExtList = new List<string>() { ".dll", ".cs" };
        public AssetPath<UnityEditor.DefaultAsset>[] AtlasDirs = new AssetPath<UnityEditor.DefaultAsset>[0];

        public UnityDependencyUtil _UnityDepCollection;
        public AtlasUtil _AtlasUtil;
        public Dictionary<string, List<string>> _ShaderDeps;

        public override IAssetDependency GetAssetDependency()
        {
            _UnityDepCollection = new UnityDependencyUtil();
            _UnityDepCollection.AddIgnoreResList(IgnoreAssetPathList);
            _UnityDepCollection.AddIgnoreExtList(IngoreExtList);

            _AtlasUtil = new AtlasUtil();
            List<string> atlas_dir = new List<string>();
            foreach (var p in AtlasDirs)
            {
                atlas_dir.Add(p.Path);
            }
            _AtlasUtil.BuildCache(atlas_dir.ToArray());

            var config = ShaderDBAsset.Load();
            if (config == null)
                _ShaderDeps = new Dictionary<string, List<string>>();
            else
                _ShaderDeps = config.EdGetShaderMaterialDict();
            return this;
        }

        public override List<string> CollectDirectDeps(string asset_path, EAssetObjType asset_type)
        {
            if (asset_type == EAssetObjType.atlas)
            {
                return new List<string>();
            }
            else if (asset_type == EAssetObjType.texture)
            {
                var ret = new List<string>();
                _AtlasUtil.GetSpriteDependency(asset_path, ret);
                return ret;
            }
            else if (asset_type == EAssetObjType.shader)
            {
                var ret = _UnityDepCollection.CollectDirectDeps(asset_path, asset_type);
                _ShaderDeps.TryGetValue(asset_path, out var list);
                ret.AddRange(list);
                return ret;
            }
            else
            {
                return _UnityDepCollection.CollectDirectDeps(asset_path, asset_type);
            }
        }
    }
}
