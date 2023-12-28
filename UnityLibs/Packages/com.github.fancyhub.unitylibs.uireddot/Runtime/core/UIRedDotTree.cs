/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/17 18:30:07
 * Title   : 
 * Desc    :  红点的树状数据
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    public enum EUIRedDotDataType
    {
        AutoNode, // 自动
        ManualNode, //手动
        VirtualNode//虚拟的
    }

    public struct UIRedDotItemData
    {
        public EUIRedDotDataType DataType;
        public bool Inited;
        public Str Path;
        public int Value;
    }

    public class UIRedDotTree
    {
        public ValueTree<UIRedDotItemData> _Root;
        public UIRedDotTree()
        {
            _Root = ValueTree<UIRedDotItemData>.Create();
        }

        public void Destroy()
        {
            _Root.Destroy();
            _Root = null;
        }

        public bool Set(Str path, int v)
        {
            return Set(path, v, false, EUIRedDotDataType.AutoNode);
        }

        public bool Set(Str path, int v, EUIRedDotDataType type)
        {
            return Set(path, v, true, type);
        }

        public bool Set(Str path, int v, bool change_node_type, EUIRedDotDataType type)
        {
            //1. 检查
            if (path.IsEmpty())
                return false;

            //2. 找到节点
            ValueTree<UIRedDotItemData> node = _get(_Root, path, true);
            var data = node.Data;

            //3. 修改节点类型
            bool changed = false;
            if (change_node_type && data.DataType != type)
            {
                if (data.DataType != EUIRedDotDataType.AutoNode || type == EUIRedDotDataType.AutoNode)
                {
                    Log.Assert(false, "节点类型不能从 {0} -> {1}, {2}", data.DataType, type, path);
                    return false;
                }

                data.DataType = type;
                changed = true;
            }

            //4. 修改值
            if (data.Value != v)
            {
                if (data.DataType == EUIRedDotDataType.AutoNode)
                {
                    Log.Assert(false, "节点类型{0},{1} 不能修改值", data.DataType, path);
                    return false;
                }
                data.Value = v;
                changed = true;
            }

            //5. 返回
            if (!changed)
                return false;
            node.Data = data;
            return true;
        }

        public bool Delete(Str path, List<(Str, int)> out_list)
        {
            //1. 找到对应的节点
            ValueTree<UIRedDotItemData> node = _get(_Root, path, false);
            if (node == null)
                return false;
            out_list.Add((path, 0));

            //2. 删除所有的子节点            
            _find_all_child(node, out_list);

            //3. 删除空的自动类型的父节点
            var current = node.Parent;
            node.Destroy();
            for (; ; )
            {
                //如果自己是根节点, 不能删除
                if (current == null || current.Parent == null)
                    break;

                //如果还有直接点,也不能删除
                if (current.GetChildren().Count > 0)
                    break;

                //如果自己不是自动节点,删除
                if (current.Data.DataType != EUIRedDotDataType.AutoNode)
                    break;

                out_list.Add((current.Data.Path, 0));
                var temp_node = current;
                current = current.Parent;
                temp_node.Destroy();
            }
            return true;
        }

        public void UpdateParent(Str path, List<(Str, int)> out_list)
        {
            //更新所有的父节点
            Str temp = path;
            for (; ; )
            {
                //获取父路径
                if (!_get_parent_path(temp, out temp))
                    break;

                ValueTree<UIRedDotItemData> parent_node = _get(_Root, temp, false);

                //如果为空,  比如 删除A.B.C,导致A.B 也被删除了, 还是要计算A路径的
                if (parent_node == null)
                    continue;

                //是根节点,直接结束
                if (parent_node.Parent == null)
                    break;

                if (!_update_parent(parent_node))
                    break;

                out_list.Add((temp, parent_node.Data.Value));
            }
        }

        public bool TryGet(Str path, out UIRedDotItemData v)
        {
            ValueTree<UIRedDotItemData> node = _get(_Root, path, false);
            if (node != null)
            {
                v = node.Data;
                return true;
            }
            v = default;
            return false;
        }

        public void _find_all_child(ValueTree<UIRedDotItemData> node, List<(Str, int)> out_list)
        {
            foreach (var p in node.GetChildren())
            {
                _find_all_child(p.Value, out_list);

                out_list.Add((p.Value.Data.Path, 0));
            }
        }

        public ValueTree<UIRedDotItemData> _get(ValueTree<UIRedDotItemData> root, Str path, bool auto_create)
        {
            if (path.IsEmpty())
                return null;

            int count = 0;
            ValueTree<UIRedDotItemData> temp = root;
            foreach (var sub in path.Split(ValueTree<UIRedDotItemData>.C_PATH_SPLIT))
            {
                temp = temp.Get(sub, auto_create);
                if (temp == null)
                    return null;

                if (count == 0)
                    count += sub.Length;
                else
                    count += sub.Length + 1;

                var data = temp.Data;
                if (!data.Inited)
                {
                    data.Inited = true;
                    data.Path = path.Substr(0, count);
                    data.DataType = EUIRedDotDataType.AutoNode;
                    data.Value = 0;
                    temp.Data = data;
                }
            }
            return temp;
        }

        public static bool _get_parent_path(Str path, out Str parent_path)
        {
            parent_path = Str.Empty;
            int index = path.LastIndexOf(ValueTree<UIRedDotItemData>.C_PATH_SPLIT);
            if (index <= 0)
                return false;

            parent_path = path.Substr(0, index);
            return true;
        }

        public static bool _update_parent(ValueTree<UIRedDotItemData> node)
        {
            var data = node.Data;
            if (data.DataType != EUIRedDotDataType.AutoNode)
                return false;

            int count = 0;
            foreach (var p in node.GetChildren())
            {
                count += p.Value.Data.Value;
            }

            data.Value = count;
            node.Data = data;
            return true;
        }
    }
}
