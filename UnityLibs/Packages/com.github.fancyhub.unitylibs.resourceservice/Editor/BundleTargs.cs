/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/25
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.ResManagement.Ed
{
    [Serializable]
    public class BundleTags
    {

        [Serializable]
        public class AssetItem
        {
            public AssetPath<UnityEngine.Object> Asset;

            [Header("eg:base;tag_a;tab_b")]
            public string Tags;
        }

        public List<AssetItem> AssetTags = new List<AssetItem>();

        private Dictionary<string, string[]> AssetPath2Tags = new Dictionary<string, string[]>();
        private Dictionary<string, string[]> AssetDir2Tags = new Dictionary<string, string[]>();


        public void BuildCache()
        {
            AssetPath2Tags.Clear();
            AssetDir2Tags.Clear();

            foreach (var p in AssetTags)
            {
                var obj = p.Asset.EdLoad();
                if (obj == null)
                    continue;

                string[] tags = p.Tags.Split(';', StringSplitOptions.RemoveEmptyEntries);
                if (tags == null || tags.Length == 0)
                    continue;

                if (obj is UnityEditor.DefaultAsset)
                {
                    AssetDir2Tags[p.Asset.Path] = tags;
                }
                else
                {
                    string path = p.Asset.Path;
                    if (!path.EndsWith('/'))
                        path += "/";
                    AssetPath2Tags[path] = tags;
                }
            }
        }

        public void GetTags(string asset_path, HashSet<string> tags)
        {
            AssetPath2Tags.TryGetValue(asset_path, out var tag_arrays);
            if (tag_arrays != null)
            {
                foreach (var p in tag_arrays)
                    tags.Add(p);
            }

            foreach (var p in AssetDir2Tags)
            {
                if (!asset_path.StartsWith(p.Key))
                    continue;
                foreach (var p2 in p.Value)
                    tags.Add(p2);
            }
        }
    }
}
