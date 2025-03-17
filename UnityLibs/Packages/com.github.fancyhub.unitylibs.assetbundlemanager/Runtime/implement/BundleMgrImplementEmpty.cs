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

        public IBundleMgr.EBundleFileStatus GetBundleStatus(IBundle bundle)
        {
            return IBundleMgr.EBundleFileStatus.None;
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
    }
}
