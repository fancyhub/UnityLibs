/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.ABManagement
{
    internal class BundleMgrImplementEmpty : CPtrBase, IBundleMgr
    {
        public void Init()
        {

        }

        public IBundle FindBundleByAsset(string asset)
        {
            return null;
        }

        public EBundleFileStatus GetBundleStatus(IBundle bundle)
        {
            return EBundleFileStatus.None;
        }

        public void GetAllBundles(List<IBundle> bundles)
        {
            bundles?.Clear();
        }

        public IBundle LoadBundleByAsset(string asset)
        {
            return null;
        }

        protected override void OnRelease()
        {
        }

        public void Upgrade()
        {
        }

        public void GetBundleInfoByAssets(List<string> asset_list, List<BundleInfo> out_bundle_info_list)
        {
            out_bundle_info_list.Clear();
        }

        public BundleInfo GetBundleInfoByAsset(string asset)
        {
            return new BundleInfo(null, EBundleFileStatus.None);
        }
    }
}
