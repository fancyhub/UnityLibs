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
    public class AssetCollector_AssetsInputConfig : BuilderAssetCollector
    {
        public AssetsInputConfig Config;

        public override List<(string path, string address)> GetAllAssets()
        {
            if (Config == null)
                return new List<(string path, string address)>();
            return Config.GetResCollection();
        }

        public override IAssetCollector GetAssetCollector()
        {
            return this;
        }
    }
}
