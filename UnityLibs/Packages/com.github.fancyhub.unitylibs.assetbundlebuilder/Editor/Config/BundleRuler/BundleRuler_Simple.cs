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
    public class BundleRuler_Simple : BuilderBundleRuler
    {
        public string BundleName;
        public bool OnlyExportFile = true;
        public List<PatternSearch> PatternList = new List<PatternSearch>();

        public override string GetBundleName(string asset_path, EAssetObjType asset_type, bool need_export)
        {
            if (!need_export && OnlyExportFile)
                return null;

            if (!_IsMatch(asset_path))
                return null;
            return BundleName;
        }

        private bool _IsMatch(string file_path)
        {
            foreach (var a in PatternList)
            {
                if (a.IsMatch(file_path))                
                    return true;                
            }
            return false;
        }
    }
     
}
