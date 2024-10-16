/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/26
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FH.AssetBundleBuilder.Ed
{
    [CreateAssetMenu(fileName = "AssetsInputConfig", menuName = "fancyhub/AssetsInputConfig")]
    [Serializable]
    public sealed class AssetsInputConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        public enum EAddressMode
        {
            None,
        }


        [Serializable]
        public sealed class Item
        {
            public AssetPath<UnityEngine.Object> Asset;
            public string SearchPattern = "*.*";
            public EAddressMode AddressMode;
            public string BundleName;
            public string Tags;


            public string[] GetTagArray()
            {
                if (string.IsNullOrEmpty(Tags))
                    return System.Array.Empty<string>();

                string[] temps = Tags.Split(';', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < temps.Length; i++)
                    temps[i] = temps[i].Trim().ToLower();
                return temps;
            }
        }

        public List<Item> Items = new List<Item>();

        private sealed class InnerItem
        {
            public string Path;
            public string AddressName;
            public string BundleName;
            public string[] Tags;
        }

        private Dictionary<string, InnerItem> _Dict = new Dictionary<string, InnerItem>();


        public List<(string path, string address)> GetResCollection()
        {
            List<(string path, string address)> ret = new List<(string path, string address)>(_Dict.Count);
            foreach (var p in _Dict)
            {
                ret.Add((p.Value.Path, p.Value.AddressName));
            }

            return ret;
        }

        public string GetBundleName(string asset_path)
        {
            _Dict.TryGetValue(asset_path, out var item);
            if (item == null)
                return null;

            return item.BundleName;
        }

        public void GetTags(string asset_path, HashSet<string> tags)
        {
            _Dict.TryGetValue(asset_path, out var item);
            if (item == null || item.Tags.Length == 0)
                return;
            foreach (var p in item.Tags)
            {

                tags.Add(p);
            }

        }

        public void BuildCache()
        {
            _Dict.Clear();

            foreach (var p in Items)
            {
                if (string.IsNullOrEmpty(p.Asset.Path))
                    continue;

                string[] tags = p.GetTagArray();

                if (System.IO.File.Exists(p.Asset.Path))
                {
                    _Dict[p.Asset.Path] = new InnerItem()
                    {
                        Path = p.Asset.Path,
                        AddressName = _GetAddressName(p.Asset.Path, p.AddressMode),
                        Tags = tags,
                        BundleName = p.BundleName,
                    };
                }
                else if (System.IO.Directory.Exists(p.Asset.Path))
                {
                    foreach (var file in System.IO.Directory.GetFiles(p.Asset.Path, p.SearchPattern, System.IO.SearchOption.AllDirectories))
                    {
                        if (file.EndsWith(".meta"))
                            continue;

                        string path = file.Replace('\\', '/');
                        _Dict[path] = new InnerItem()
                        {
                            Path = path,
                            AddressName = _GetAddressName(path, p.AddressMode),
                            Tags = tags,
                            BundleName = p.BundleName,
                        };
                    }
                }
                else
                    continue;
            }
        }


        private static string _GetAddressName(string path, EAddressMode mode)
        {
            return null;
        }


        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            BuildCache();
        }
    }
}
