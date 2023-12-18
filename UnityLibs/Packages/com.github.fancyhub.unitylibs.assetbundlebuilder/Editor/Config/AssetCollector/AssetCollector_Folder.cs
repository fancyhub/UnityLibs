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
    public class AssetCollector_Folder : BuilderAssetCollector
    {
        public List<string> Folders;

        public override List<(string path, string address)> GetAllAssets()
        {
            List<(string, string)> ret = new List<(string, string)>();

            foreach (var p in this.Folders)
            {
                foreach (var p2 in System.IO.Directory.GetFiles(p, "*.*", System.IO.SearchOption.AllDirectories))
                {
                    if (p2.EndsWith(".meta"))
                        continue;

                    ret.Add((p2.Replace('\\', '/'), null));
                }
            }

            return ret;
        }

        public override IAssetCollector GetAssetCollector()
        {
            return this;
        }
    }
}
