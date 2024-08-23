/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/31 17:09:02
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH.UI
{
    internal class UIViewLayerList
    {
        public const int C_NODE_INIT_COUNT = 10; //初始化得时候,一个layer多少个数量得node
        public const int C_NODE_INC_COUNT = 10;//数量不够得时候,一次增加多少node        

        public List<UIViewLayer> _layers;
        public Dictionary<int, LinkedListNode<UIViewNode>> _node_dict;
        public int _node_cap = 0;
        public int _node_cap_now = 0;

        public UIViewLayerList()
        {
            _layers = new List<UIViewLayer>();
            _node_cap = 0;
            _node_dict = new Dictionary<int, LinkedListNode<UIViewNode>>();
            _node_cap_now = 0;
        }

        public int LayerCount => _layers.Count;


        public void AddLayer(string name)
        {
            _layers.Add(new UIViewLayer()
            {
                _name = name,
                _nodes = new LinkedList<UIViewNode>(),
                _cap = C_NODE_INIT_COUNT,
                _start_idx = _layers.Count * C_NODE_INIT_COUNT
            });

            _node_cap_now += C_NODE_INIT_COUNT;
        }

        /// <summary>
        ///  返回得是 index, 如果是 &lt; 0  说明出错了
        /// </summary>
        public int AddView(int view_id, int layer_idx, UIViewWrapper extra, List<UIViewNode> changes)
        {
            //1 清除
            if (changes == null)
                return -1;
            changes.Clear();

            //2. 检查参数
            if (layer_idx < 0 || layer_idx >= _layers.Count)
                return -2;
            if (_node_dict.ContainsKey(view_id))
                return -3;

            //3. 如果有需要, 扩容该layer
            UIViewLayer layer = _layers[layer_idx];

            int check_rst = layer.CheckBeforeAdd();
            if (check_rst != 0)
            {
                if (check_rst == 1) //扩容
                {
                    //达到了上限, 没有可以扩容了
                    if ((_node_cap_now + C_NODE_INC_COUNT) > _node_cap)
                        return -4;
                    _extend(_layers, layer_idx, changes);
                    _node_cap_now += C_NODE_INC_COUNT;
                }
                else if (check_rst == 2)// 重新排序
                {
                    _sort_node_in_layer(layer, changes);
                }
                else
                {
                    return -1;
                }

                check_rst = layer.CheckBeforeAdd();
                if (check_rst != 0)
                    return -2;
            }

            //添加
            int new_idx = layer.GetNewIndex();
            UIViewNode data = new UIViewNode();
            data._id = view_id;
            data._node_idx = new_idx;
            data._layer_idx = layer_idx;
            data._extra_data = extra;
            var node = layer._nodes.ExtAddLast(data);
            _node_dict.Add(view_id, node);
            changes.Add(data);
            return new_idx;
        }

        public bool Del(int view_id, out UIViewNode val)
        {
            val = default;
            if (!_node_dict.TryGetValue(view_id, out var v))
                return false;

            val = v.Value;
            v.List.ExtRemove(v);
            _node_dict.Remove(view_id);
            return true;
        }

        public bool MoveInLayer(int view_id, int offset, List<UIViewNode> changes)
        {
            changes.Clear();
            if (offset == 0)
                return true;
            _node_dict.TryGetValue(view_id, out var node);
            if (node == null)
                return false;

            //2. 开始移动
            UIViewLayer layer = _layers[node.Value._layer_idx];
            LinkedList<UIViewNode> node_list = layer._nodes;

            if (offset > 0) //向上移动, 和链表的后面节点交换
            {
                //要判断自己是否为最后一个
                if (node.Next == null)
                    return true;

                var temp_node = node;
                for (int i = 0; i < offset; i++)
                {
                    temp_node = temp_node.Next;
                    if (temp_node == null)
                        break;
                }
                node_list.Remove(node);
                if (temp_node == null)
                    node_list.AddLast(node);
                else
                    node_list.AddAfter(temp_node, node);
            }
            else if (offset < 0) //向后移动, 和链表的前面节点交换
            {
                //已经是第一个了
                if (node.Previous == null)
                    return true;

                offset = -offset;
                var temp_node = node;
                for (int i = 0; i < offset; i++)
                {
                    temp_node = temp_node.Previous;
                    if (temp_node == null)
                        break;
                }
                node_list.Remove(node);
                if (temp_node == null)
                    node_list.AddFirst(node);
                else
                    node_list.AddBefore(temp_node, node);
            }

            _sort_node_in_layer(layer, changes);
            return true;
        }

        public bool FindLast(Func<UIViewNode, bool> func, out UIViewNode val)
        {
            for (int i = _layers.Count - 1; i >= 0; i--)
            {
                var layer = _layers[i];
                var last = layer._nodes.Last;
                for (; last != null; last = last.Previous)
                {
                    if (func == null || func(last.Value))
                    {
                        val = last.Value;
                        return true;
                    }
                }
            }

            val = default;
            return false;
        }

        public void GetList2<T2>(Func<UIViewNode, T2, bool> func, T2 v, List<UIViewNode> out_list)
        {
            if (func == null || out_list == null)
                return;
            out_list.Clear();
            for (int i = 0; i < _layers.Count; i++)
            {
                var layer = _layers[i];
                for (var node = layer._nodes.First; node != null; node = node.Next)
                {
                    if (func(node.Value, v))
                    {
                        out_list.Add(node.Value);
                    }
                }
            }
        }

        public void GetList(Func<UIViewNode, bool> func, List<UIViewNode> out_list)
        {
            if (func == null || out_list == null)
                return;
            out_list.Clear();
            for (int i = 0; i < _layers.Count; i++)
            {
                var layer = _layers[i];
                for (var node = layer._nodes.First; node != null; node = node.Next)
                {
                    if (func(node.Value))
                    {
                        out_list.Add(node.Value);
                    }
                }
            }
        }

        public bool GetVal(int id, out UIViewWrapper val)
        {
            if (!_node_dict.TryGetValue(id, out var v))
            {
                val = default;
                return false;
            }
            val = v.Value._extra_data;
            return true;
        }

        public bool SetVal(int id, UIViewWrapper ext_val)
        {
            if (!_node_dict.TryGetValue(id, out var v))
            {
                return false;
            }

            var vv = v.Value;
            vv._extra_data = ext_val;
            v.Value = vv;
            return true;
        }

        public static bool _extend(List<UIViewLayer> layers, int layer_index, List<UIViewNode> changes)
        {
            layers[layer_index]._cap += C_NODE_INC_COUNT;

            //后续的都要重新调整 order
            for (int i = layer_index + 1; i < layers.Count; i++)
            {
                UIViewLayer cur_layer = layers[i];
                cur_layer._start_idx += C_NODE_INC_COUNT;
                _sort_node_in_layer(cur_layer, changes);
            }

            return true;
        }

        public static void _sort_node_in_layer(UIViewLayer layer, List<UIViewNode> changes)
        {
            LinkedListNode<UIViewNode> node = layer._nodes.First;
            int index = layer._start_idx;
            for (; node != null; node = node.Next, index++)
            {
                UIViewNode data = node.Value;
                if (data._node_idx == index)
                    continue;

                data._node_idx = index;
                node.Value = data;
                changes.Add(data);
            }
        }
    }

}
