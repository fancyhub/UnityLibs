/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace FH.AssetBundleBuilder.Ed
{ 
    public partial class BundleNodeMap
    {
        private HashSet<BundleNode> _nodes_map = new HashSet<BundleNode>();
        private BundleNodeMapAdd _node_add;

        public BundleNodeMap()
        {
            _node_add = new BundleNodeMapAdd(_nodes_map);
        }

        public BundleNode Add(AssetObj obj, string ab_name)
        {
            return _node_add.Add(obj, ab_name);
        }

        public void Build()
        {
            new BundleNodeMapBuilder(_nodes_map).Build();
        }

        public UnityEditor.AssetBundleBuild[] GenAssetBundleBuildList()
        {
            HashSet<BundleNode> node_list = _nodes_map;
            if (node_list.Count == 0)
            {
                throw new System.Exception("没有任何资源被打包");
            }

            List<UnityEditor.AssetBundleBuild> builder_list = new List<UnityEditor.AssetBundleBuild>(node_list.Count);
            foreach (BundleNode node in node_list)
            {
                UnityEditor.AssetBundleBuild builder = node.GenAssetBundleBuild();

                if (builder.assetBundleName == null || builder.assetNames.Length == 0)
                {
                    throw new Exception("有node 为空 " + node.GetNodeName());
                }
                builder_list.Add(builder);
            }
            return builder_list.ToArray();
        }

        public void CheckAllAssetsInBundle(ICollection<AssetObj> allAssetObjs)
        {
            HashSet<string> file_list_from_obj = new HashSet<string>(allAssetObjs.Count);
            foreach (var p in allAssetObjs)
                file_list_from_obj.Add(p.FilePath);

            HashSet<string> file_list_from_node = new HashSet<string>(allAssetObjs.Count);
            foreach (var p in GetAllAssetObjects())
                file_list_from_node.Add(p.FilePath);

            if (file_list_from_node.Count == file_list_from_obj.Count)
                return;

            HashSet<string> file_list_not_include = new HashSet<string>();
            foreach (string a in file_list_from_obj)
            {
                if (file_list_from_node.Contains(a))
                    continue;
                file_list_not_include.Add(a);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("错误，有文件没有被打包 Count: " + file_list_not_include.Count);
            int index = 0;
            foreach (var a in file_list_not_include)
            {
                sb.AppendLine(a);
                index++;
                if (index > 10)
                {
                    //输出部分只取前面10个，要不然输出太多
                    break;
                }
            }

            throw new System.Exception(sb.ToString());
        }

        public HashSet<BundleNode> GetAllNodes()
        {
            return _nodes_map;
        }

        public HashSet<AssetObj> GetAllAssetObjects()
        {
            HashSet<AssetObj> ret = new HashSet<AssetObj>();
            foreach (BundleNode node in _nodes_map)
            {
                foreach (AssetObj obj in node)
                {
                    ret.Add(obj);
                }

                foreach (AssetObj obj in node._dep_objs)
                {
                    ret.Add(obj);
                }
            }
            return ret;
        }

        #region 私有类
        private class BundleNodeMapAdd
        {
            private HashSet<BundleNode> _BundleSet;
            private Dictionary<string, BundleNode> _Name2BundleMap = new Dictionary<string, BundleNode>();
            private Dictionary<AssetObj, BundleNode> _Asset2BundleMap = new Dictionary<AssetObj, BundleNode>();

            public BundleNodeMapAdd(HashSet<BundleNode> bundle_set)
            {
                _BundleSet = bundle_set;
            }

            public int AssetCount => _Asset2BundleMap.Count;

            public BundleNode Add(AssetObj assset, string ab_name)
            {
                BundleNode node = _FindBundleByName(ab_name);
                if (null == node)
                {
                    node = new BundleNode(ab_name);
                    _Name2BundleMap.Add(ab_name, node);
                    _BundleSet.Add(node);
                    node._IsSceneNode = assset.IsSceneObj();
                }

                _Add(assset, node);
                if (null != assset.GetCycleDepObjGroup())
                {
                    foreach (var a in assset.GetCycleDepObjGroup())
                    {
                        _Add(a, node);
                    }
                }
                return node;
            }

            private void _Add(AssetObj asset, BundleNode bundle)
            {
                //判断 该obj 是否已经加到了 node里面
                BundleNode orig_node = _FindBundleByAsset(asset);
                if (orig_node != null)
                {
                    //如果已经加进去了，就直接返回
                    if (bundle == orig_node)
                        return;

                    string msg = string.Format("File Is In {0},conflict with {1}, {2}"
                        , orig_node.GetNodeName()
                        , bundle.GetNodeName()
                        , asset.FilePath);

                    BuilderLog.Error(msg);
                    throw new System.Exception(msg);
                }

                if (bundle._IsSceneNode != asset.IsSceneObj())
                {
                    string msg = null;
                    if (bundle._IsSceneNode)
                        msg = string.Format("{0} is scene node, can't add {1}", bundle.GetNodeName(), asset.FilePath);
                    else
                        msg = string.Format("{0} is not scene node, can't add {1}", bundle.GetNodeName(), asset.FilePath);

                    BuilderLog.Error(msg);
                    throw new System.Exception(msg);
                }
                _Asset2BundleMap.Add(asset, bundle);
                bundle.AddMainObj(asset);
            }

            private BundleNode _FindBundleByAsset(AssetObj asset)
            {
                BundleNode ret = null;
                _Asset2BundleMap.TryGetValue(asset, out ret);
                return ret;
            }

            private BundleNode _FindBundleByName(string ab_name)
            {
                BundleNode ret = null;
                _Name2BundleMap.TryGetValue(ab_name, out ret);
                return ret;
            }
        }

        public class BundleNodeMapBuilder
        {
            private HashSet<BundleNode> _nodes_map;
            private HashSet<AssetObj> _all_deps_objs = new HashSet<AssetObj>();
            private BundleNodeDepChecker _cycle_dep_checker = new BundleNodeDepChecker();

            //这个属性和 node对象的 _main_objs 对应
            private Dictionary<AssetObj, BundleNode> _obj_main_nodes = new Dictionary<AssetObj, BundleNode>();

            //这个属性和node对象的_dep_objs 对应
            private Dictionary<AssetObj, HashSet<BundleNode>> _obj_owner_nodes = new Dictionary<AssetObj, HashSet<BundleNode>>();

            public BundleNodeMapBuilder(HashSet<BundleNode> node_set)
            {
                _nodes_map = node_set;
            }

            public void Build()
            {
                //step 1: 设置所有的main nodes
                foreach (BundleNode a in _nodes_map)
                {
                    foreach (var b in a._main_objs)
                    {
                        _set_main_node(b, a);
                    }
                }


                //step 2: 建立Node 之间的依赖关系
                foreach (BundleNode node in _nodes_map)
                {
                    foreach (AssetObj obj in node._main_objs)
                    {
                        foreach (AssetObj sub_obj in obj.GetDepObjs())
                        {
                            BundleNode main_node = _get_main_node(sub_obj);
                            if (main_node == null)
                                continue;
                            if (main_node == node)
                                continue;
                            node._dep_nodes.Add(main_node);
                        }
                    }
                }
                _cycle_dep_checker.HasCycleDep(_nodes_map);


                //Step3 : 把所有的依赖加到节点里面
                foreach (BundleNode self_node in _nodes_map)
                {
                    _fill_dep_objs(self_node);
                }
                _cycle_dep_checker.HasCycleDep(_nodes_map);

                //Step4: 分割节点
                bool result = _process_split();
                //_cycle_dep_checker.HasCycleDep(_nodes_map);
                while (result)
                {
                    result = _process_split();
                    //_cycle_dep_checker.HasCycleDep(_nodes_map);
                }

                //Step5: 把场景类型的节点下面的资源单独作为一个包
                HashSet<BundleNode> temp = new HashSet<BundleNode>();
                foreach (BundleNode self_node in _nodes_map)
                {
                    if (!self_node.IsSceneNode())
                        continue;

                    List<AssetObj> dep_objs = self_node.GetDepObjs();
                    if (dep_objs.Count == 0)
                        continue;

                    BundleNode new_node = new BundleNode(null);
                    temp.Add(new_node);

                    foreach (AssetObj obj in dep_objs)
                    {
                        HashSet<BundleNode> owner_nodes_set = _get_owner_node_set(obj);
                        BuilderLog.Assert(owner_nodes_set.Count == 1);
                        new_node.AddMainObj(obj);
                        _set_main_node(obj, new_node);
                        owner_nodes_set.Clear();
                    }

                    dep_objs.Clear();
                    HashSet<BundleNode> dep_nodes = self_node.GetDepNodes();
                    dep_nodes.Clear();
                }

                foreach (var a in temp)
                {
                    _nodes_map.Add(a);
                }

                //6. 重建依赖关系
                foreach (var a in _nodes_map)
                {
                    _build_node_dep_nodes(a);
                }

                _cycle_dep_checker.HasCycleDep(_nodes_map);
            }

            //建立 node 的依赖node
            private void _build_node_dep_nodes(BundleNode self_node)
            {
                HashSet<BundleNode> dep_nodes = self_node.GetDepNodes();
                dep_nodes.Clear();
                foreach (AssetObj a in self_node)
                {
                    foreach (var b in a.GetDepObjs())
                    {
                        var node = _get_main_node(b);
                        if (null != node)
                        {
                            if (node != self_node)
                            {
                                dep_nodes.Add(node);
                            }
                            continue;
                        }

                        var owner_nodes_set = _get_owner_node_set(b);
                        BuilderLog.Assert(owner_nodes_set.Count == 1);
                        foreach (var c in owner_nodes_set)
                        {
                            if (c != self_node)
                            {
                                dep_nodes.Add(c);
                            }
                        }
                    }
                }
            }

            private BundleNode _get_main_node(AssetObj obj)
            {
                BundleNode ret = null;
                _obj_main_nodes.TryGetValue(obj, out ret);
                return ret;
            }

            private void _set_main_node(AssetObj obj, BundleNode node)
            {
                _obj_main_nodes.Add(obj, node);
            }

            private void _replace_main_node(AssetObj obj, BundleNode node)
            {
                if (!_obj_main_nodes.ContainsKey(obj))
                    throw new System.Exception();

                _obj_main_nodes.Add(obj, node);
            }


            private HashSet<BundleNode> _get_owner_node_set(AssetObj obj)
            {
                HashSet<BundleNode> ret = null;
                _obj_owner_nodes.TryGetValue(obj, out ret);
                if (null == ret)
                {
                    ret = new HashSet<BundleNode>();
                    _obj_owner_nodes.Add(obj, ret);
                }
                return ret;
            }


            private void _fill_dep_objs(BundleNode self_node)
            {
                BuilderLog.Assert(self_node.GetDepObjs().Count == 0);

                _all_deps_objs.Clear();
                foreach (AssetObj self_obj in self_node.GetMainObjs())
                {
                    self_obj.GetAllDepObjs(_all_deps_objs);
                }

                foreach (AssetObj dep in _all_deps_objs)
                {
                    BundleNode node = _get_main_node(dep);
                    //如果不在 main_objs 里面，就加到自己节点里面的依赖objs里面
                    if (node == null)
                    {
                        self_node.AddDepObj(dep);
                        HashSet<BundleNode> dep_owner_nodes_set = _get_owner_node_set(dep);
                        dep_owner_nodes_set.Add(self_node);
                        continue;
                    }

                    if (node == self_node)
                        continue;

                    //如果已经在 某个Node 里面的 main_objs 了,那么该node 就是自己的依赖node
                    //一定要检查是否出现了 循环依赖
                    self_node._dep_nodes.Add(node);
                }
            }

            private bool _process_split()
            {
                HashSet<AssetObj> all_deps_objs = new HashSet<AssetObj>();

                //Step 1: 把节点里面的依赖obj，判断是否在依赖的节点里面，如果在就移除出去
                foreach (BundleNode self_node in _nodes_map)
                {
                    //如果标记位表示不需要处理，就ignore
                    if (!self_node.FlagNeedProcess) continue;

                    all_deps_objs.Clear();
                    foreach (AssetObj obj in self_node.GetDepObjs())
                    {
                        if (self_node.IsObjInDepNodes(obj))
                        {
                            all_deps_objs.Add(obj);
                        }
                    }

                    foreach (AssetObj obj in all_deps_objs)
                    {
                        HashSet<BundleNode> owner_node_set = _get_owner_node_set(obj);
                        self_node.RemoveDepObj(obj);
                        BuilderLog.Assert(owner_node_set.Count > 1);
                        owner_node_set.Remove(self_node);
                    }
                }

                //Step 2: 遍历每个node，检查所依赖的obj是否被多个node所依赖，如果是被多个的话，就变成一个独立的新节点
                Dictionary<string, HashSet<AssetObj>> temp = new Dictionary<string, HashSet<AssetObj>>();
                List<int> temp_ids = new List<int>();
                StringBuilder sb = new StringBuilder();

                //把所有的节点，按照被引用的node的ids的字符串作为key
                //ids按照从小到达排序
                //按照这个key来分组，一个组就是一个新的节点
                foreach (BundleNode self_node in _nodes_map)
                {
                    if (!self_node.FlagNeedProcess)
                        continue;

                    bool found = false;
                    foreach (AssetObj obj in self_node.GetDepObjs())
                    {
                        HashSet<BundleNode> obj_owner_nodes_set = _get_owner_node_set(obj);
                        BuilderLog.Assert(obj_owner_nodes_set.Count > 0);
                        //被多个依赖的，单独拎出来
                        if (obj_owner_nodes_set.Count > 1)
                        {
                            temp_ids.Clear();
                            foreach (BundleNode s in obj_owner_nodes_set)
                            {
                                temp_ids.Add(s.Id);
                            }
                            temp_ids.Sort();

                            sb.Clear();
                            for (int i = 0; i < temp_ids.Count; i++)
                            {
                                sb.Append(temp_ids[i].ToString());
                                sb.Append(',');
                            }
                            string key = sb.ToString();

                            HashSet<AssetObj> set = null;
                            temp.TryGetValue(key, out set);
                            if (set == null)
                            {
                                set = new HashSet<AssetObj>();
                                set.Add(obj);
                                temp.Add(key, set);
                            }
                            else
                                set.Add(obj);
                            found = true;
                        }
                    }

                    //如果该节点的状态没有发生改变，下次就不要继续处理了
                    if (found)
                    {
                        self_node.FlagNeedProcess = true;
                    }
                    else
                    {
                        self_node.FlagNeedProcess = false;
                    }
                }


                //Step3: 变成新的节点
                foreach (var p in temp)
                {
                    HashSet<AssetObj> obj_set = p.Value;
                    BundleNode new_node = new BundleNode(null);
                    new_node._IsSceneNode = false;
                    new_node.FlagNeedProcess = true;

                    foreach (AssetObj bo in obj_set)
                    {
                        new_node.AddMainObj(bo);
                        _set_main_node(bo, new_node);

                        //移除旧的
                        var bo_owner_nodes_set = _get_owner_node_set(bo);
                        foreach (BundleNode old_node in bo_owner_nodes_set)
                        {
                            old_node.FlagNeedProcess = true;
                            old_node.RemoveDepObj(bo);
                            old_node._dep_nodes.Add(new_node);
                        }
                        bo_owner_nodes_set.Clear();
                    }
                    _fill_dep_objs(new_node);
                    _nodes_map.Add(new_node);
                }

                if (temp.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 检查循环依赖的问题
        /// </summary>
        private class BundleNodeDepChecker
        {
            public HashSet<BundleNode> _checked_nodes = new HashSet<BundleNode>();
            public Stack<BundleNode> _stacks = new Stack<BundleNode>();

            /// <summary>
            /// 没有循环依赖，就返回false
            /// 如果有循环依赖，返回true
            /// </summary>
            public bool HasCycleDep(ICollection<BundleNode> nodes_set)
            {
                _checked_nodes.Clear();
                //检查循环引用的问题            
                bool ret = false;
                foreach (BundleNode node in nodes_set)
                {
                    _stacks.Clear();
                    bool is_cycle = _check_cycle_dep(node);
                    if (!is_cycle)
                        continue;

                    ret = true;
                    break;
                }

                //出现了循环依赖
                if (!ret)
                    return !ret;


                var last_one = _stacks.Peek();
                List<BundleNode> b_list = new List<BundleNode>();
                foreach (var p in _stacks)
                {
                    if (p == last_one)
                        b_list.Add(p);
                    else if (b_list.Count > 0)
                        b_list.Add(p);
                }
                b_list.Add(last_one);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Node Dep Cycle ");
                foreach (var a in b_list)
                {
                    sb.AppendLine("\t" + a.GetNodeName());
                }
                sb.AppendLine();

                for (int i = 0; i < b_list.Count - 1; ++i)
                {
                    var node_a = b_list[i];
                    var node_b = b_list[i + 1];

                    sb.AppendFormat("Node {0} 依赖 {1} 的文件列表\n", node_a.GetNodeName(), node_b.GetNodeName());
                    foreach (var p in node_a.FindDep(node_b))
                    {
                        sb.AppendFormat("\t {0} -> {1} \n", p.Key, p.Value);
                    }
                }

                string msg = sb.ToString();
                BuilderLog.Error(msg);
                throw new System.Exception("循环依赖了");
            }

            public bool _check_cycle_dep(BundleNode node)
            {
                bool ret = false;
                if (_stacks.Contains(node))
                    return true;

                if (_checked_nodes.Contains(node))
                {
                    return false;
                }

                _stacks.Push(node);
                foreach (var child in node._dep_nodes)
                {
                    if (_check_cycle_dep(child))
                    {
                        return true;
                    }
                }

                _checked_nodes.Add(node);
                _stacks.Pop();
                return ret;
            }
        }

        #endregion
    }
}
