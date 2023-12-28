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
            MyEqualityComparer.Reg(StrEqualityComparer._);
        }

        public const int C_COUNT_MAX = 100;
        public Dictionary<Str, HashSet<Str>> _virtual_link;
        public UIRedDotTree _rd_tree;
        public UIRedDotVirual(UIRedDotTree tree)
        {
            _rd_tree = tree;
            _virtual_link = new Dictionary<Str, HashSet<Str>>(StrEqualityComparer._);
        }

        public HashSet<Str> Find(Str path)
        {
            _virtual_link.TryGetValue(path, out var ret);
            return ret;
        }

        public bool Add(Str real_path, Str virtual_path, List<(Str, int)> out_change_list)
        {
            if (real_path.IsEmpty() || virtual_path.IsEmpty())
                return false;

            _virtual_link.TryGetValue(real_path, out HashSet<Str> list);
            if (list == null)
            {
                list = new HashSet<Str>(StrEqualityComparer._);
                _virtual_link.Add(real_path, list);
            }
            list.Add(virtual_path);

            if (_rd_tree.TryGet(real_path, out var src_data))
            {
                _rd_tree.Set(virtual_path, src_data.Value, EUIRedDotDataType.VirtualNode);
                out_change_list.Add((virtual_path, src_data.Value));
                _rd_tree.UpdateParent(virtual_path, out_change_list);
            }
            else
            {
                _rd_tree.Set(virtual_path, 0, EUIRedDotDataType.VirtualNode);
                out_change_list.Add((virtual_path, 0));
                _rd_tree.UpdateParent(virtual_path, out_change_list);
            }
            return true;
        }

        public bool Remove(Str real_path, Str virtual_path, List<(Str, int)> out_change_list)
        {
            if (real_path.IsEmpty() || virtual_path.IsEmpty())
                return false;

            _virtual_link.TryGetValue(real_path, out HashSet<Str> list);
            if (list == null)
                return false;

            list.Remove(virtual_path);
            if (list.Count == 0)
            {
                _virtual_link.Remove(real_path);
            }

            _rd_tree.Delete(virtual_path, out_change_list);
            return true;
        }

        public void Update(List<(Str, int)> inout_change_list)
        {
            int count = inout_change_list.Count;
            //更新所有的虚拟节点
            for (int i = 0; ; i++)
            {
                if (i >= inout_change_list.Count)
                    break;

                var data = inout_change_list[i];
                HashSet<Str> vir_set = Find(data.Item1);
                if (vir_set == null)
                    continue;
                foreach (var vir_path in vir_set)
                {
                    if (!_rd_tree.Set(vir_path, data.Item2))
                        continue;

                    //虚拟节点不要发出消息
                    //_changed_list.Add((vir_path, data.Item2));
                    _rd_tree.UpdateParent(vir_path, inout_change_list);
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
