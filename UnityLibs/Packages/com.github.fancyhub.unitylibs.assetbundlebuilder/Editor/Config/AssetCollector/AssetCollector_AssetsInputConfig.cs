/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/26
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// 收集器, 通过 AssetInputConfig 来收集资源
    /// </summary>
    public class AssetCollector_AssetsInputConfig : BuilderAssetCollector
    {
        public AssetsInputConfig Config;

        public override List<(string path, string address)> GetAllAssets()
        {
            if (Config == null)
                return new List<(string path, string address)>();
            Config.BuildCache();
            return Config.GetResCollection();
        }

        public override IAssetCollector GetAssetCollector()
        {
            return this;
        }
    }
}
