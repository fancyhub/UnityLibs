/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/26
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// 根据配置来确定该包所对应的tags
    /// </summary>
    public class TagRuler_AssetTagConfig : BuilderTagRuler
    {
        public AssetsInputConfig Config;

        public override ITagRuler GetTagRuler()
        {
            if (Config == null)
                return null;
            return base.GetTagRuler();
        }

        public override void GetTags(string bundle_name, List<string> assets_list, HashSet<string> out_tags)
        {            
            foreach (var p in assets_list)
            {
                Config.GetTags(p, out_tags);
            }
        }
    }
}
