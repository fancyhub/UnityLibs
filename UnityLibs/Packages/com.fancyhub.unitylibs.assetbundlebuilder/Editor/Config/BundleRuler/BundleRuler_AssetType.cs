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
    public class BundleRuler_AssetType: BuilderBundleRuler
    {
        public string BundleName;

        public List<EAssetObjType> AssetTypes = new List<EAssetObjType>();

        private HashSet<EAssetObjType> _HashAssetTypes;

        public override string GetBundleName(ABBuildContext context, string asset_path, EAssetObjType asset_type, bool need_export)
        {
            if (_HashAssetTypes.Contains(asset_type))
                return BundleName;

            return null;
        }

        public override IBundleRuler GetBundleRuler(ABBuildContext context)
        {            
            if(_HashAssetTypes==null)
                _HashAssetTypes= new HashSet<EAssetObjType>();
            _HashAssetTypes.Clear();
            foreach (var p in AssetTypes)
                _HashAssetTypes.Add(p);
            return this;
        }
    }
}
