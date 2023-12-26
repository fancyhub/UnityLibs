/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.AssetBundleBuilder.Ed
{
    public class TagRuler_Bundle : BuilderTagRuler
    {
        [Header("eg: base;tag_a;tag_b")]
        public string Tags;
        [Tooltip("false: 任何包都会添加该 tags")]
        public bool EnablePattern = true;
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
            if (!_IsMatch(bundle_name))
                return;

            foreach (var p in _Tags)
                out_tags.Add(p);
        }

        private bool _IsMatch(string bundle_name)
        {
            if (!EnablePattern)
                return true;
            foreach (PatternSearch a in PatternList)
            {
                if (a.IsMatch(bundle_name))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
