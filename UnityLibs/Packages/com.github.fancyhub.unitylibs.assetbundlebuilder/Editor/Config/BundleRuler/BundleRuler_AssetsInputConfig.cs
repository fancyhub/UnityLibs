/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/26
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FH.AssetBundleBuilder.Ed
{
    public class BundleRuler_AssetsInputConfig : BuilderBundleRuler
    {
        public AssetsInputConfig Config;

        public override string GetBundleName(string asset_path, EAssetObjType asset_type, bool need_export)
        {
            if (Config == null)
                return null;

            return Config.GetBundleName(asset_path);
        }
    }
}
