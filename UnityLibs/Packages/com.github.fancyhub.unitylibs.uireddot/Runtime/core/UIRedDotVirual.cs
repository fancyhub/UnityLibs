/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/17 18:30:36
 * Title   : 
 * Desc    : 红点的虚拟节点管理
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    /// <summary>
    /// 管理所有虚拟节点
    /// </summary>
    public sealed class UIRedDotVirual
    {
        static UIRedDotVirual()
        {
            MyEqualityComparer.Reg(Str.EqualityComparer);
        }

        public const int C_COUNT_MAX = 100;
        private Dictionary<Str, HashSet<Str>> _RealPath2VirtualPathMap;
        private UIRedDotTree _Tree;
        public UIRedDotVirual(UIRedDotTree tree)
        {
            _Tree = tree;
            _RealPath2VirtualPathMap = new Dictionary<Str, HashSet<Str>>(Str.EqualityComparer);
        }

        public HashSet<Str> Find(Str real_path)
        {
            _RealPath2VirtualPathMap.TryGetValue(real_path, out var ret);
            return ret;
        }

        public bool Link(string real_path, string virtual_path, List<(Str, UIRedDotValue)> out_change_list)
        {
            if (string.IsNullOrEmpty(real_path) || string.IsNullOrEmpty(virtual_path))
                return false;

            _RealPath2VirtualPathMap.TryGetValue(real_path, out HashSet<Str> list);
            if (list == null)
            {
                list = new HashSet<Str>(Str.EqualityComparer);
                _RealPath2VirtualPathMap.Add(real_path, list);
            }
            list.Add(virtual_path);

            if (_Tree.TryGet(real_path, out var src_data))
            {
                _Tree.Set(virtual_path, src_data.Value.Count, UIRedDotTree.ENodeType.VirtualNode);
                out_change_list.Add((virtual_path, src_data.Value));
                _Tree.UpdateParent(virtual_path, out_change_list);
            }
            else
            {
                _Tree.Set(virtual_path, 0, UIRedDotTree.ENodeType.VirtualNode);
                out_change_list.Add((virtual_path, default));
                _Tree.UpdateParent(virtual_path, out_change_list);
            }
            return true;
        }

        public bool Unlink(string real_path, string virtual_path, List<(Str path, UIRedDotValue value)> out_change_list)
        {
            if (string.IsNullOrEmpty(real_path) || string.IsNullOrEmpty(virtual_path))
                return false;

            _RealPath2VirtualPathMap.TryGetValue(real_path, out HashSet<Str> virtual_path_set);
            if (virtual_path_set == null || !virtual_path_set.Remove(virtual_path))
                return false;

            if (virtual_path_set.Count == 0)
                _RealPath2VirtualPathMap.Remove(real_path);

            _Tree.Delete(virtual_path, out_change_list);
            return true;
        }

        public void Update(List<(Str path, UIRedDotValue value)> inout_change_list)
        {
            int count = inout_change_list.Count;
            //更新所有的虚拟节点
            for (int i = 0; ; i++)
            {
                if (i >= inout_change_list.Count)
                    break;

                var item = inout_change_list[i];
                HashSet<Str> virtual_path_set = Find(item.path);
                if (virtual_path_set == null)
                    continue;

                foreach (Str virtual_path in virtual_path_set)
                {
                    if (!_Tree.Set(virtual_path, item.value.Count))
                        continue;

                    //虚拟节点不要发出消息
                    //inout_change_list.Add((virtual_path, item.value));

                    _Tree.UpdateParent(virtual_path, inout_change_list);
                }

                if (count > (inout_change_list.Count + C_COUNT_MAX))
                {
                    Log.Assert(false, "虚拟节点太多了,可能出现了循环");
                    break;
                }
            }
        }
    }
}
