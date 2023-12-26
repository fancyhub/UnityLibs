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
    public class TagRuler_AssetPattern : BuilderTagRuler
    {
        [Header("eg: base;tag_a;tag_b")]
        public string Tags;
        public List<PatternSearch> PatternList = new List<PatternSearch>();

        private List<string> _Tags = new List<string>();
        public override ITagRuler GetTagRuler()
        {
            string[] temps = Tags.Split(';', StringSplitOptions.RemoveEmptyEntries);
            _Tags.Clear();
            foreach (var p in temps)
            {
                string t = p.Trim().ToLower();
                if (string.IsNullOrEmpty(t))
                    continue;
                _Tags.Add(t);
            }
            if (_Tags.Count == 0)
                return null;

            return base.GetTagRuler();
        }

        public override void GetTags(string bundle_name, List<string> assets_list, HashSet<string> out_tags)
        {
            if (!_IsMatch(assets_list))
                return;

            foreach (var p in _Tags)
                out_tags.Add(p);
        }

        private bool _IsMatch(List<string> assets_list)
        {
            foreach(string asset_path in assets_list)
            {
                foreach (PatternSearch pattern in PatternList)
                {
                    if (pattern.IsMatch(asset_path))
                    {
                        return true;
                    }
                }
            }            
            return false;
        }
    }
}
