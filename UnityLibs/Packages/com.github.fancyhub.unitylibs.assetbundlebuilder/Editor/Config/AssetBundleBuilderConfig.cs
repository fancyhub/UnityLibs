/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FH.AssetBundleBuilder.Ed
{
    [CreateAssetMenu(fileName = "AssetBundleBuilderConfig", menuName = "fanchhub/AssetBundleBuilderConfig")]
    public class AssetBundleBuilderConfig : ScriptableObject
    {
        private static AssetBundleBuilderConfig _Default;
        public const string CPath = "Assets/fancyhub/AssetBundleBuilderConfig.asset";
        public BuilderParam Android;
        public BuilderParam IOS;
        public BuilderParam Windows;
        public BuilderParam OSX;

        [HideInInspector]
        public List<BuilderFeature> Features = new List<BuilderFeature>();

        public IAssetCollection GetAssetCollection()
        {
            return new AssetCollectionGroup(Features);
        }

        public IAssetDepCollection GetAssetDepCollection()
        {
            foreach (var p in Features)
            {
                if (!p.Enable)
                    continue;

                if (p is IAssetDepCollection c)
                    return c;
            }

            Debug.LogError("找不到 IAssetDepCollection");
            return null;
        }

        public static AssetBundleBuilderConfig GetDefault()
        {
            if (_Default != null)
                return _Default;
            _Default = AssetDatabase.LoadAssetAtPath<AssetBundleBuilderConfig>(CPath);
            if (_Default == null)
            {
                Debug.LogError("加载 AssetBundleBuilderConfig 失败 " + CPath);
            }
            return _Default;
        }

        public BuilderParam GetBuilderParam(UnityEditor.BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return Android;
                case BuildTarget.iOS:
                    return IOS;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return Windows;
                case BuildTarget.StandaloneOSX:
                    return OSX;
                default:
                    return null;
            }
        }

        private class AssetCollectionGroup : IAssetCollection
        {
            public List<IAssetCollection> List = new List<IAssetCollection>();

            public AssetCollectionGroup(List<BuilderFeature> features)
            {
                foreach (var f in features)
                {
                    if (!f.Enable)
                        continue;

                    if (f is IAssetCollection c)
                    {
                        List.Add(c);
                    }
                }
            }

            public List<(string path, string address)> GetAllAssets()
            {
                List<(string path, string address)> ret = new List<(string path, string address)>();
                foreach (var p in List)
                {
                    ret.AddRange(p.GetAllAssets());
                }

                return ret;
            }
        }
    }

    [Serializable]
    public class BuilderParam
    {
        public BuildAssetBundleOptions BuildOptions = //BuildAssetBundleOptions.UncompressedAssetBundle
                                            BuildAssetBundleOptions.ChunkBasedCompression
                                           | BuildAssetBundleOptions.StrictMode;
        public string OutputDir;
    }
}
