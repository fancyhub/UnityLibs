/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// 代表了一个 asset
    /// </summary>
    public class AssetObj
    {
        public static int C_ID_GEN = 100000;
        public int _id;
        readonly int _hash_code;

        public EAssetObjType _obj_type;
        public string _file_path = null;
        public string _guid = null;
        public string _file_hash = string.Empty;
        public bool _need_export = false;
        public string _address_name = null;

        //这个里面是循环依赖的objs
        //如果该对象和某些对象是循环依赖，这个属性，就会共享，包含这一组循环依赖的所有对象
        //并且这一组内的所有对象的 _dep_objs 也共享实例
        //_cycle_dep_objs 是共享实例
        public HashSet<AssetObj> _cycle_deps_objs;
        public HashSet<AssetObj> _dep_objs = new HashSet<AssetObj>();

        public AssetObj()
        {
            _hash_code = C_ID_GEN.GetHashCode();
            _id = C_ID_GEN;
            C_ID_GEN++;
        }

        public bool IsSceneObj()
        {
            return _obj_type == EAssetObjType.scene;
        }

        public EAssetObjType GetAssetType()
        {
            return _obj_type;
        }

        public string GetFilePath()
        {
            return _file_path;
        }

        public HashSet<AssetObj> GetCycleDepObjGroup()
        {
            return _cycle_deps_objs;
        }

        public override int GetHashCode()
        {
            return _hash_code;
        }

        public override string ToString()
        {
            return _file_path;
        }


        public void SetPath(string path, string guid, string file_hash)
        {
            _obj_type = AssetObjType.GetObjType(path);
            _file_path = path;
            _guid = guid;
            _file_hash = file_hash;
        }

        public bool IsDep(AssetObj obj)
        {
            if (_dep_objs.Contains(obj))
                return true;

            foreach (var a in _dep_objs)
            {
                if (a.IsDep(obj))
                    return true;
            }
            return false;
        }

        public void GetAllDepObjs(HashSet<AssetObj> out_deps_objs)
        {
            foreach (AssetObj dep_obj in _dep_objs)
            {
                if (out_deps_objs.Contains(dep_obj))
                    continue;

                if (_cycle_deps_objs != null)
                {
                    foreach (AssetObj cycle_dep_obj in _cycle_deps_objs)
                    {
                        out_deps_objs.Add(cycle_dep_obj);
                    }
                }

                out_deps_objs.Add(dep_obj);
                dep_obj.GetAllDepObjs(out_deps_objs);
            }
        }
    }

}
