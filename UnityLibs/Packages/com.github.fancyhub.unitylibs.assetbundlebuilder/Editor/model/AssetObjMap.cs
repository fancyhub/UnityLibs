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
    /// 描述了 Unity的 Assets 之间的依赖关系
    /// </summary>
    public class AssetObjMap
    {
        private IAssetDependency _bundle_collection;
        public bool _has_error_flag = false;
        private Dictionary<string, AssetObj> _objs_map;
        private AssetObjectCycleDep _cycle_dep;

        public AssetObjMap()
        {
            _objs_map = new Dictionary<string, AssetObj>();
            _cycle_dep = new AssetObjectCycleDep();
        }

        public void SetDepCollection(IAssetDependency collection)
        {
            _bundle_collection = collection;
        }

        public AssetObj AddObject(string path, string address_name = null)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            path = path.Replace('\\', '/');

            AssetObj obj = FindObject(path);
            if (obj != null)
            {
                obj.NeedExport = true;
                obj.AddressName = address_name;
                return obj;
            }

            _cycle_dep.Clear();
            obj = _CreateAssetObject(path, _cycle_dep);
            if (null == obj)
                return obj;

            obj.AddressName = address_name;
            obj.NeedExport = true;

            //把循环依赖变成一个组
            _cycle_dep.ProcessCycleDep();

            return obj;
        }

        public AssetObj FindObject(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            path = path.Replace('\\', '/');

            AssetObj obj_ret = null;
            _objs_map.TryGetValue(path, out obj_ret);
            return obj_ret;
        }

        public List<AssetObj> FindObjects(EAssetObjType obj_type)
        {
            List<AssetObj> ret = new List<AssetObj>();
            foreach (var p in _objs_map)
            {
                if (p.Value.AssetType == obj_type)
                {
                    ret.Add(p.Value);
                }
            }
            return ret;
        }

        public ICollection<AssetObj> GetAllObjects()
        {
            return _objs_map.Values;
        }

        public void CheckCycleDep()
        {
            AssetObjectCycleDepChecker _cycle_dep_checker = new AssetObjectCycleDepChecker();
            _cycle_dep_checker.HasCycleDep(_objs_map.Values);
        }
 
        private AssetObj _CreateAssetObject(string path, AssetObjectCycleDep cycle_dep)
        { 
            AssetObj ret_obj = new AssetObj(path, _bundle_collection.FileGuid(path));
            {

                _objs_map.Add(path, ret_obj);
                cycle_dep.Add(ret_obj);

                if (ret_obj.AssetType == EAssetObjType.none)
                {
                    BuilderLog.AssertFormat(false, "Unknow res type:{0}", path);
                }
            }


            List<string> deps_paths = _bundle_collection.CollectDirectDeps(ret_obj.FilePath, ret_obj.AssetType);
            foreach (string dep_path in deps_paths)
            {
                if (dep_path == null)
                {
                    string error_msg = "该对象的依赖里面包含空对象：" + path;
                    _has_error_flag = true;
                    BuilderLog.Error(error_msg);
                    continue;
                }

                //比较坑，有的时候，自己依赖自己
                if (dep_path == path)
                {
                    continue;
                }

                AssetObj dep_obj = FindObject(dep_path);
                if (dep_obj == null)
                {
                    dep_obj = _CreateAssetObject(dep_path, cycle_dep);
                    if (dep_obj == null)
                    {
                        BuilderLog.Warning("依赖文件创建失败:{0} 依赖的 {1} ", path, dep_path);
                        continue;
                    }
                }
                ret_obj.AddDirectDep(dep_obj);
            }
            return ret_obj;
        }
        #region 私有类
        /// <summary>
        /// 循环依赖的处理对象
        /// </summary>
        private class AssetObjectCycleDep
        {
            //当添加一个对象的时候，可能要把很多 他所依赖的object 添加进来        
            public List<AssetObj> _obj_list = new List<AssetObj>(100);
            public List<AssetObj> _stack = new List<AssetObj>(20);

            public void Clear()
            {
                _obj_list.Clear();
            }

            /// <summary>
            /// 获取新添加的对象列表
            /// </summary>        
            public List<AssetObj> GetObjListNew()
            {
                return _obj_list;
            }

            public void Add(AssetObj obj)
            {
                _obj_list.Add(obj);
            }

            //把循环依赖变成一个组
            public void ProcessCycleDep()
            {
                while (true)
                {
                    _stack.Clear();
                    bool found = false;
                    foreach (var obj in _obj_list)
                    {
                        bool find = _find_cycle_dep_objs(obj, _stack);
                        if (find)
                        {
                            _combine_cycle_objs(_stack);
                            _stack.Clear();
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        break;
                }
            }

            private bool _find_cycle_dep_objs(AssetObj obj, List<AssetObj> stack)
            {
                if (stack.Contains(obj))
                {
                    stack.Add(obj);
                    return true;
                }

                stack.Add(obj);
                foreach (var a in obj.GetDepObjs())
                {
                    if (_find_cycle_dep_objs(a, stack))
                    {
                        return true;
                    }
                }
                stack.RemoveAt(stack.Count - 1);
                return false;
            }


            private void _combine_cycle_objs(List<AssetObj> stack)
            {
                int count = stack.Count;
                AssetObj last_obj = stack[count - 1];
                int first_index = stack.IndexOf(last_obj);

                BuilderLog.Assert(first_index < (count - 1));

                //从first_index 到最后一个都算是一组，相互依赖

                HashSet<AssetObj> cycle_objs = new HashSet<AssetObj>();
                HashSet<AssetObj> dep_objs = new HashSet<AssetObj>();
                for (int i = first_index; i < count - 1; i++)
                {
                    AssetObj self_obj = stack[i];

                    cycle_objs.Add(self_obj);

                    if (self_obj.GetCycleDepObjGroup() != null)
                    {
                        foreach (var bb in self_obj.GetCycleDepObjGroup())
                        {
                            cycle_objs.Add(bb);
                        }
                    }

                    foreach (var bb in self_obj.GetDepObjs())
                    {
                        dep_objs.Add(bb);
                    }
                }

                foreach (var a in cycle_objs)
                {
                    dep_objs.Remove(a);
                }

                foreach (var a in cycle_objs)
                {
                    a.SetDepsObjs(dep_objs, cycle_objs);
                }
            }
        }


        /// <summary>
        /// 检查循环依赖的问题
        /// </summary>
        private class AssetObjectCycleDepChecker
        {
            HashSet<AssetObj> _checked_objs = new HashSet<AssetObj>();
            Stack<AssetObj> _stack = new Stack<AssetObj>();

            /// <summary>
            /// 没有循环依赖，就返回false
            /// 如果有循环依赖，返回true
            /// </summary>        
            public bool HasCycleDep(ICollection<AssetObj> objs_set)
            {
                //检查循环引用的问题            
                bool ret = false;
                foreach (AssetObj obj in objs_set)
                {
                    bool is_cycle = _CheckCycleDep(obj);
                    if (!is_cycle)
                        continue;

                    ret = true;
                    BuilderLog.Assert(!is_cycle);
                }
                return ret;
            }

            private bool _CheckCycleDep(AssetObj node)
            {
                bool ret = false;
                if (_stack.Contains(node))
                    return true;

                if (_checked_objs.Contains(node))
                {
                    return false;
                }

                _stack.Push(node);
                foreach (var child in node.GetDepObjs())
                {
                    if (_CheckCycleDep(child))
                        return true;
                }
                _checked_objs.Add(node);
                _stack.Pop();
                return ret;
            }
        }
        #endregion

    }

}
