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
        public UnityDependencyUtil _UnityDepCollection;

        public override IAssetDependency GetAssetDependency()
        {
            _UnityDepCollection = new UnityDependencyUtil();
            _UnityDepCollection.AddIgnoreResList(IgnoreAssetPathList);
            _UnityDepCollection.AddIgnoreExtList(IngoreExtList);
            return this;
        }

        public override List<string> CollectDirectDeps(string asset_path, EAssetObjType asset_type)
        {
            return _UnityDepCollection.CollectDirectDeps(asset_path, asset_type);
        }
    }
}
