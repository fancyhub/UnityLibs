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
    public enum EUIRedDotNodeType
    {
        AutoNode, // 自动
        ManualNode, //手动
        VirtualNode//虚拟的
    }

    public struct UIRedDotNodeData
    {
        public Str Path;
        public EUIRedDotNodeType NodeType;
        internal bool Inited;
        public int Value;
    }

    public class UIRedDotTree
    {
        public ValueTree<UIRedDotNodeData> _Root;
        public UIRedDotTree()
        {
            _Root = ValueTree<UIRedDotNodeData>.Create();
        }

        public void Destroy()
        {
            _Root.Destroy();
            _Root = null;
        }

        public bool Set(Str path, int v)
        {
            return Set(path, v, false, EUIRedDotNodeType.AutoNode);
        }

        public bool Set(Str path, int v, EUIRedDotNodeType type)
        {
            return Set(path, v, true, type);
        }

        public bool Set(Str path, int v, bool change_node_type, EUIRedDotNodeType type)
        {
            //1. 检查
            if (path.IsEmpty())
                return false;

            //2. 找到节点
            ValueTree<UIRedDotNodeData> node = _GetOrCreate(_Root, path, true);
            UIRedDotNodeData data = node.Data;

            //3. 修改节点类型
            bool changed = false;
            if (change_node_type && data.NodeType != type)
            {
                if (data.NodeType != EUIRedDotNodeType.AutoNode || type == EUIRedDotNodeType.AutoNode)
                {
                    Log.Assert(false, "节点类型不能从 {0} -> {1}, {2}", data.NodeType, type, path);
                    return false;
                }

                UIRedDotLog.D("ChangeType: [{0} : {1}], {2} -> {3}", data.NodeType, data.Path, data.NodeType, type);
                data.NodeType = type;
                changed = true;
            }

            //4. 修改值
            if (data.Value != v)
            {
                if (data.NodeType == EUIRedDotNodeType.AutoNode)
                {
                    Log.Assert(false, "节点类型{0},{1} 不能修改值", data.NodeType, path);
                    return false;
                }

                UIRedDotLog.D("ChangeValue: [{0} : {1}], {2} -> {3}", data.NodeType, data.Path, data.Value, v);
                data.Value = v;
                changed = true;
            }

            //5. 返回
            if (!changed)
                return false;
            node.Data = data;
            return true;
        }

        public bool Delete(Str path, List<(Str path, int value)> out_list)
        {
            //1. 找到对应的节点
            ValueTree<UIRedDotNodeData> node = _GetOrCreate(_Root, path, false);
            if (node == null)
                return false;
            out_list.Add((path, 0));

            //2. 删除所有的子节点            
            _FindAllChildren(node, out_list);

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
                if (current.Data.NodeType != EUIRedDotNodeType.AutoNode)
                    break;

                out_list.Add((current.Data.Path, 0));
                var temp_node = current;
                current = current.Parent;
                temp_node.Destroy();
            }
            return true;
        }

        public void UpdateParent(Str path, List<(Str path, int value)> out_list)
        {
            //更新所有的父节点
            Str temp = path;
            for (; ; )
            {
                //获取父路径
                if (!_GetParentPath(temp, out temp))
                    break;

                ValueTree<UIRedDotNodeData> parent_node = _GetOrCreate(_Root, temp, false);

                //如果为空,  比如 删除A.B.C,导致A.B 也被删除了, 还是要计算A路径的
                if (parent_node == null)
                    continue;

                //是根节点,直接结束
                if (parent_node.Parent == null)
                    break;

                if (!_UpdateParentsValue(parent_node))
                    break;

                out_list.Add((temp, parent_node.Data.Value));
            }
        }

        public bool TryGet(Str path, out UIRedDotNodeData v)
        {
            ValueTree<UIRedDotNodeData> node = _GetOrCreate(_Root, path, false);
            if (node != null)
            {
                v = node.Data;
                return true;
            }
            v = default;
            return false;
        }

        private void _FindAllChildren(ValueTree<UIRedDotNodeData> node, List<(Str, int)> out_list)
        {
            foreach (var p in node.GetChildren())
            {
                _FindAllChildren(p.Value, out_list);

                out_list.Add((p.Value.Data.Path, 0));
            }
        }

        private static ValueTree<UIRedDotNodeData> _GetOrCreate(ValueTree<UIRedDotNodeData> root, Str path, bool auto_create)
        {
            if (path.IsEmpty())
                return null;

            int count = 0;
            ValueTree<UIRedDotNodeData> temp_node = root;
            foreach (Str node_name in path.Split(ValueTree<UIRedDotNodeData>.CPathSeparator))
            {
                temp_node = temp_node.Get(node_name, auto_create);
                if (temp_node == null)
                    return null;

                if (count == 0)
                    count += node_name.Length;
                else
                    count += node_name.Length + 1;

                UIRedDotNodeData data = temp_node.Data;
                if (!data.Inited)
                {
                    Str node_path = path.Substr(0, count);
                    data.Inited = true;
                    data.Path = node_path;
                    data.NodeType = EUIRedDotNodeType.AutoNode;
                    data.Value = 0;
                    temp_node.Data = data;

                    UIRedDotLog.D("CreateNode: [{0} : {1}]", data.NodeType, data.Path);
                }
            }
            return temp_node;
        }

        private static bool _GetParentPath(Str path, out Str parent_path)
        {
            parent_path = Str.Empty;
            int index = path.LastIndexOf(ValueTree<UIRedDotNodeData>.CPathSeparator);
            if (index <= 0)
                return false;

            parent_path = path.Substr(0, index);
            return true;
        }

        private static bool _UpdateParentsValue(ValueTree<UIRedDotNodeData> node)
        {
            var data = node.Data;
            if (data.NodeType != EUIRedDotNodeType.AutoNode)
                return false;

            int final_val = 0;
            foreach (var p in node.GetChildren())
            {
                final_val += p.Value.Data.Value;
            }

            if (final_val == data.Value)
                return false;

            UIRedDotLog.D("ChangeParentValue: [{0} : {1}], {2} -> {3}", data.NodeType, data.Path, data.Value, final_val);
            data.Value = final_val;
            node.Data = data;
            return true;
        }
    }
}
