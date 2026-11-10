/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// 
    /// </summary>
    public class BundleRuler_Shader : BuilderBundleRuler
    {
        public string BundleName;

        public bool IncludeShaderVariantCollection;

        public override string GetBundleName(string asset_path, EAssetObjType asset_type, bool need_export)
        {
            if (asset_type == EAssetObjType.shader)
                return BundleName;

            if (asset_type == EAssetObjType.shader_var_collection && IncludeShaderVariantCollection)
                return BundleName;

            return null;
        }
    }
}
