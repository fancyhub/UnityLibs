/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/14
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.AssetBundleBuilder.Ed
{
    public interface IAssetCollection
    {
        List<(string path, string address)> GetAllAssets();
    }

    public abstract class BuilderFeature : ScriptableObject
    {
        [HideInInspector]
        public bool Show = false;
        public bool Enable = true;
        
    }
}
