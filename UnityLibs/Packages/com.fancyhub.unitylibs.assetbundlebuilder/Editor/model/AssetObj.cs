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
        private readonly int Id;
        public readonly string FilePath = null;
        public readonly string GUID = null;
        public readonly EAssetObjType AssetType;

        public bool NeedExport = false;
        public string AddressName = null;

        //这个里面是循环依赖的objs
        //如果该对象和某些对象是循环依赖，这个属性，就会共享，包含这一组循环依赖的所有对象
        //并且这一组内的所有对象的 _DepObjs 也共享实例
        //_cycle_dep_objs 是共享实例
        private HashSet<AssetObj> _CycleDepObjs;
        private HashSet<AssetObj> _DepObjs = new HashSet<AssetObj>();

        private HashSet<AssetObj> _DirectDepObjs = new HashSet<AssetObj>();

        public AssetObj(string path, string guid)
        {
            FilePath = path;
            GUID = guid;
            Id = ++C_ID_GEN;
            AssetType = AssetObjType.GetObjType(path);
        }

        public bool IsSceneObj() { return AssetType == EAssetObjType.scene; }

        public HashSet<AssetObj> GetCycleDepObjGroup()
        {
            return _CycleDepObjs;
        }
        public HashSet<AssetObj> GetDirectDepObjs()
        {
            return _DirectDepObjs;
        }
        public HashSet<AssetObj> GetDepObjs()
        {
            return _DepObjs;
        }

        public void SetDepsObjs(HashSet<AssetObj> depObjs, HashSet<AssetObj> cycleDepObjs)
        {
            _DepObjs = depObjs;
            _CycleDepObjs = cycleDepObjs;
        }

        public void AddDirectDep(AssetObj obj)
        {
            _DepObjs.Add(obj);
            _DirectDepObjs.Add(obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return FilePath;
        }

        public bool IsDep(AssetObj obj)
        {
            if (_DepObjs.Contains(obj))
                return true;

            foreach (var a in _DepObjs)
            {
                if (a.IsDep(obj))
                    return true;
            }
            return false;
        }

        public void GetAllDepObjs(HashSet<AssetObj> out_deps_objs)
        {
            foreach (AssetObj dep_obj in _DepObjs)
            {
                if (out_deps_objs.Contains(dep_obj))
                    continue;

                if (_CycleDepObjs != null)
                {
                    foreach (AssetObj cycle_dep_obj in _CycleDepObjs)
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
