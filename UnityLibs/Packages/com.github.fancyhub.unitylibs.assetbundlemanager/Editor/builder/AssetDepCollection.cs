using System;
using System.Collections.Generic;

namespace FH.AssetBundleManager.Builder
{
    public class AssetDepCollection : IAssetDepCollection
    {
        public HashSet<string> _ignore_res_list = new HashSet<string>()
        {
        };
        public List<string> _temp = new List<string>();
        public string[] CollectDirectDeps(string path)
        {
            return _Filter(UnityEditor.AssetDatabase.GetDependencies(path, false));
        }

        public string FileGuid(string path)
        {
            return UnityEditor.AssetDatabase.AssetPathToGUID(path);
        }

        public string FileHash(string path)
        {
            //if (path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            //    return string.Empty;
            //return _get_file_modified_time(path).ToString();

            // 下面的太慢了
            UnityEngine.Hash128 obj = UnityEditor.AssetDatabase.GetAssetDependencyHash(path);
            string ret = obj.ToString();
            if (null == ret)
                return string.Empty;
            return ret;
        }

        public EFileStatus FileStatus(string path)
        {
            if (System.IO.File.Exists(path))
                return EFileStatus.exist;
            if (System.IO.Directory.Exists(path))
                return EFileStatus.folder;
            return EFileStatus.not_exist;
        }

        public string[] _Filter(string[] in_data)
        {
            _temp.Clear();
            bool changed = false;
            foreach (var a in in_data)
            {
                if (_ShouldIgnore(a))                
                    changed = true;                
                else
                    _temp.Add(a);
            }

            if (changed)
                return _temp.ToArray();
            return in_data;
        }

        public bool _ShouldIgnore(string path)
        {
            //unity的内部资源
            if (path == "Resources/unity_builtin_extra")
            {
                return true;
            }
            else if (path == "Library/unity default resources") //unity的内部资源
            {
                return true;
            }
            else if (path.EndsWith("LightingData.asset"))
            {
                return true;
            }
            else if (_ignore_res_list.Contains(path))
            {
                return true;
            }
            else if (path.EndsWith(".unity")) //不可能有资源引用场景的
            {
                return true;
            }
            else
            {
                string lower = path.ToLower();
                if (lower.EndsWith(".dll")) //dll资源
                {
                    return true;
                }
                else if (lower.EndsWith(".cs")) //cs 资源
                {
                    return true;
                }
                else if (lower.EndsWith(".giparams")) // gi 生成的参数
                {
                    return true;
                }
            }
            return false;
        }
    }
}
