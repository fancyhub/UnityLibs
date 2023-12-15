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

    public sealed class UnityDependencyUtil
    {
        private HashSet<string> _IgnoreResSet = new HashSet<string>();
        private HashSet<string> _IgnoreExtSet = new HashSet<string>()
        {
            ".dll",//dll资源
            ".cs",//cs 资源
            ".giparams",// gi 生成的参数
        };

        public void AddIgnoreResList(List<string> ignore_res_list)
        {
            foreach (var p in ignore_res_list)
            {
                _IgnoreResSet.Add(p.ToLower());
            }
        }

        public void AddIgnoreExtList(List<string> ignoreExtList)
        {
            foreach (var p in ignoreExtList)
            {
                _IgnoreExtSet.Add(p.ToLower());
            }
        }

        public List<string> CollectDirectDeps(string path)
        {
            return _Filter(UnityEditor.AssetDatabase.GetDependencies(path, false));
        }

        public string FileGuid(string path)
        {
            return UnityEditor.AssetDatabase.AssetPathToGUID(path);
        }

        public List<string> _Filter(string[] in_data)
        {
            List<string> ret = new List<string>(in_data.Length);
            foreach (var a in in_data)
            {
                if (_ShouldIgnore(a))
                    continue;

                ret.Add(a);
            }
            return ret;
        }

        public bool _ShouldIgnore(string path)
        {
            //unity的内部资源
            if (path == "Resources/unity_builtin_extra")
                return true;
            else if (path == "Library/unity default resources") //unity的内部资源            
                return true;
            else if (path.EndsWith("LightingData.asset"))
                return true;
            else if (path.EndsWith(".unity")) //不可能有资源引用场景的            
                return true;

            path = path.ToLower();
            if (_IgnoreResSet.Count > 0 && _IgnoreResSet.Contains(path))
                return true;

            string ext = System.IO.Path.GetExtension(path);
            if (_IgnoreExtSet.Contains(ext))
                return true;

            return false;
        }
    }

    public class AssetDependency_Unity : BuilderAssetDependency
    {
        public List<string> IgnoreAssetPathList = new List<string>();
        public List<string> IngoreExtList = new List<string>() { ".dll", ".cs" };
        public UnityDependencyUtil _UnityDepCollection;

        private UnityDependencyUtil UnityDepCollection
        {
            get
            {
                if (_UnityDepCollection != null)
                    return _UnityDepCollection;
                _UnityDepCollection = new UnityDependencyUtil();

                _UnityDepCollection.AddIgnoreResList(IgnoreAssetPathList);

                _UnityDepCollection.AddIgnoreExtList(IngoreExtList);

                return _UnityDepCollection;
            }
        }

        public override List<string> CollectDirectDeps(string path)
        {
            return UnityDepCollection.CollectDirectDeps(path);
        }
    }
}
