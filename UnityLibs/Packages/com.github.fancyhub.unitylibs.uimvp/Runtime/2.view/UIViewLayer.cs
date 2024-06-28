/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/31 17:09:02
 * Title   : 
 * Desc    : 
*************************************************************************************/

using FH;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Egg
{
    public interface IUILayerView : IUIElement
    {
        public void SetOrder(int order);
    }

    public interface IUILayerViewBGHandler : IUIElement
    {
        public void OnBgClick();
        public GameObject GetRoot();
    }

    public enum EUIBgClickMode
    {
        None,

        /// <summary>
        /// 普通的模式
        /// </summary>
        Common,

        /// <summary>
        /// 是弹tips的模式, 点击到tip外面都会收到click消息
        /// </summary>
        TipClick,

        /// <summary>
        /// 是弹tips的模式, 点击到tip外面都会收到click消息, 和tip一样,但是down的时候触发
        /// </summary>
        TipDown,
    }


    public static class UIViewLayerConst
    {
        public const int OrderMin = 100;
        public const int OrderMax = 32767;

        //层和层之间的间隔
        //每个view之间的 order 间隔
        public const int ViewOrderInterval = 100;

        //最多多少个view
        public const int ViewMaxCount = (OrderMax - OrderMin) / ViewOrderInterval;

        //BG 的order 比目标的小5
        public const int BgOrderInterval = 5;

        public static int CalcOrder(int view_idx)
        {
            return OrderMin + view_idx * ViewOrderInterval;
        }
    }

    public class UIViewLayerMgr
    {
        public sealed class ViewWrapper : CPoolItemBase
        {
            public static ViewWrapper Create()
            {
                return GPool.New<ViewWrapper>();
            }

            public bool _bg_mask;
            public IUILayerView _view;
            public EUIBgClickMode _click_mode;
            public IUILayerViewBGHandler _click_handler;
            public int _order;

            public void SetOrder(int order)
            {
                _order = order;
                _view?.SetOrder(order);
            }

            protected override void OnPoolRelease()
            {
                _view = null;
                _click_handler = null;
            }
        }
                
        private int _cur_click_view_id; //当前click的 view id
        private int _cur_mask_view_id; //当前显示mask的view id
        
        private int _count_of_tip_click;//记录当前需要tip click 的view 数量
        private int _count_of_tip_down; //记录当前需要tip down 的view 数量

        private ViewLayerList _layer_list;
        private List<ViewNode> _temp_list;
        private UIBG _ui_bg;

        public UIViewLayerMgr(UIBG ui_bg)
        {
            _layer_list = new ViewLayerList();
            _temp_list = new List<ViewNode>();
            _count_of_tip_click = 0;
            _count_of_tip_down = 0;
            _ui_bg = ui_bg;
            _ui_bg.EvntBgClick = _on_bg_click;
            UIBGEvent.GlobalEventClick = _on_global_click;
            UIBGEvent.GlobalEventDown = _on_global_down;
        }

        public bool AddView(IUILayerView view, int layer_index)
        {
            //1. 检查
            if (view == null)
                return false;
            if (layer_index < 0 || layer_index >= _layer_list.LayerCount)
            {
                Log.Assert(false, "LayerIndex 超过了范围 {0}", layer_index);
                return false;
            }

            ViewWrapper data = ViewWrapper.Create();
            data._view = view;
            data._click_mode = EUIBgClickMode.None;
            data._bg_mask = false;
            data._click_handler = null;

            int new_idx = _layer_list.Add(view.Id, layer_index, data, _temp_list);
            foreach (var p in _temp_list)
            {
                p._extra_data.SetOrder(UIViewLayerConst.CalcOrder(p._node_idx));
            }

            if (new_idx < 0)
                return false;
            _refresh_mask(_ui_bg, _layer_list, ref _cur_mask_view_id);
            _refresh_click(_ui_bg, _layer_list, ref _cur_click_view_id);
            return true;
        }

        public bool RemoveView(int view_id)
        {
            if (!_layer_list.Del(view_id, out var data))
                return false;

            if (data._extra_data._click_mode == EUIBgClickMode.TipClick)
                _count_of_tip_click--;
            else if (data._extra_data._click_mode == EUIBgClickMode.TipDown)
                _count_of_tip_down--;

            if (view_id == _cur_click_view_id)
            {
                _refresh_click(_ui_bg, _layer_list, ref _cur_click_view_id);
            }

            if (view_id == _cur_mask_view_id)
            {
                _refresh_mask(_ui_bg, _layer_list, ref _cur_mask_view_id);
            }

            data._extra_data.Destroy();
            return true;
        }

        public bool MoveView(int view_id, int count)
        {
            if (!_layer_list.MoveInLayer(view_id, count, _temp_list))
                return false;
            foreach (var p in _temp_list)
            {
                p._extra_data.SetOrder(UIViewLayerConst.CalcOrder(p._node_idx));
            }

            _refresh_mask(_ui_bg, _layer_list, ref _cur_mask_view_id);
            _refresh_click(_ui_bg, _layer_list, ref _cur_click_view_id);
            return true;
        }

        public bool SetBgMask(int view_id, bool enable)
        {
            if (!_layer_list.GetVal(view_id, out var data))
            {
                Log.Assert(false, "找不到 id 的 Layer {0}", view_id);
                return false;
            }

            if (data._bg_mask == enable)
                return true;

            data._bg_mask = enable;
            _layer_list.SetVal(view_id, data);
            _refresh_mask(_ui_bg, _layer_list, ref _cur_mask_view_id);
            return true;
        }

        public bool SetBgClick(int view_id, IUILayerViewBGHandler handler, EUIBgClickMode click_mode)
        {
            //1. 获取对应的val,并检查存在
            if (!_layer_list.GetVal(view_id, out var data))
            {
                Log.Assert(false, "找不到 id 的 Layer {0}", view_id);
                return false;
            }
            //2.如果全部相同,字节返回            
            if (data._click_handler == handler && data._click_mode == click_mode)
                return true;

            //3. 判断tips的类型是否计数增加,主要是为了后面的优化
            if (data._click_mode != click_mode)
            {
                if (click_mode == EUIBgClickMode.TipClick)
                    _count_of_tip_click++;
                else if (click_mode == EUIBgClickMode.TipDown)
                    _count_of_tip_down++;
            }

            //4. 修改 node里面的值
            data._click_mode = click_mode;
            data._click_handler = handler;
            _layer_list.SetVal(view_id, data);

            //5. 刷新当前需要普通click的对象
            _refresh_click(_ui_bg, _layer_list, ref _cur_click_view_id);
            return true;
        }

        private void _on_bg_click()
        {
            if (_cur_click_view_id == 0)
                return;
            if (!_layer_list.GetVal(_cur_click_view_id, out var v))
                return;
            IUILayerViewBGHandler handler = v._click_handler;
            if (handler == null)
                return;
            handler.OnBgClick();
        }

        private void _on_global_down(GameObject obj)
        {
            if (_count_of_tip_down == 0)
                return;

            int order = _ui_bg.GetOrder();
            if (!_ui_bg.IsEnable())
                order = int.MinValue;

            _layer_list.GetList2((p, v) =>
            {
                if (p._extra_data._order < v)
                    return false;
                return p._extra_data._click_mode == EUIBgClickMode.TipDown;

            }, order, _temp_list);

            //Log.Assert(_count_for_tip_down == _temp_list.Count, "计数不一致");

            foreach (var p in _temp_list)
            {
                IUILayerViewBGHandler handler = p._extra_data._click_handler;
                if (handler == null)
                    continue;

                if (_is_child(handler.GetRoot(), obj))
                    continue;
                handler.OnBgClick();
            }
        }

        private void _on_global_click(GameObject obj)
        {
            if (_count_of_tip_click == 0)
                return;

            int order = _ui_bg.GetOrder();
            if (!_ui_bg.IsEnable())
                order = int.MinValue;

            _layer_list.GetList2((p, v) =>
            {
                if (p._extra_data._order < v)
                    return false;
                return p._extra_data._click_mode == EUIBgClickMode.TipClick;
            }, order, _temp_list);

            Log.Assert(_count_of_tip_click == _temp_list.Count, "计数不一致");

            foreach (var p in _temp_list)
            {
                IUILayerViewBGHandler handler = p._extra_data._click_handler;
                if (handler == null)
                    continue;

                if (_is_child(handler.GetRoot(), obj))
                    continue;
                handler.OnBgClick();
            }
        }

        private static bool _is_child(GameObject root, GameObject target)
        {
            if (root == null)
                return false;
            if (target == null)
                return false;
            if (target == root)
                return true;

            Transform root_t = root.transform;
            Transform tar_t = target.transform;
            for (; ; )
            {
                if (tar_t == null)
                    return false;
                if (tar_t == root_t)
                    return true;
                tar_t = tar_t.parent;
            }
        }
            
        private static void _refresh_mask(UIBG ui_bg, ViewLayerList list, ref int cur_mask_id)
        {
            //1. 检查
            if (ui_bg == null || list == null)
                return;

            //2. 找到当前的mask对应的view
            //全屏遮罩才需要刷新uiroot
            bool found = list.FindLast((p) => { return (p._extra_data._bg_mask); }, out var data);

            int new_mask_id = 0;
            if (found)
                new_mask_id = data._id;
            if (new_mask_id == cur_mask_id)
                return;
            cur_mask_id = new_mask_id;


            //3. 如果当前没有需要mask
            if (cur_mask_id == 0)
            {
                ui_bg.DisableMask();
                return;
            }

            //4. 
            int order = UIViewLayerConst.CalcOrder(data._node_idx);
            ui_bg.EnableMask(order - UIViewLayerConst.BgOrderInterval);
        }

        private static void _refresh_click(UIBG ui_bg, ViewLayerList list, ref int cur_click_id)
        {
            //1. 检查
            if (ui_bg == null || list == null)
                return;

            //2. 找到当前的mask对应的view
            bool found = list.FindLast((p) =>
            {
                return p._extra_data._click_mode == EUIBgClickMode.Common;
            }, out var data);
            int new_click_id = 0;
            if (found)
                new_click_id = data._id;
            if (new_click_id == cur_click_id)
                return;
            cur_click_id = new_click_id;


            //3. 如果当前没有需要mask
            if (cur_click_id == 0)
            {
                ui_bg.DisableClick();
                return;
            }

            //4. 
            int order = UIViewLayerConst.CalcOrder(data._node_idx);
            ui_bg.EnableClick(order - UIViewLayerConst.BgOrderInterval);
        }

        public struct ViewNode
        {
            public int _id;
            public int _node_idx;
            public int _layer_idx;
            public ViewWrapper _extra_data;
        }


        private class ViewLayerList
        {
            public const int C_NODE_INIT_COUNT = 10; //初始化得时候,一个layer多少个数量得node
            public const int C_NODE_INC_COUNT = 10;//数量不够得时候,一次增加多少node
                       
            public class Layer
            {
                public int _start_idx;
                public int _cap;// 节点数量
                public LinkedList<ViewNode> _nodes;

                public int GetNewIndex()
                {
                    if (_nodes.Count == 0)
                        return _start_idx;

                    return _nodes.Last.Value._node_idx + 1;
                }

                /// <summary>
                /// 添加之前, 检查是否可以添加
                /// 0: ok
                /// 1: 需要扩容
                /// 2: 需要重新排序
                /// </summary>
                public int CheckBeforeAdd()
                {
                    //容量达到了上限, 需要扩容
                    if (_nodes.Count >= _cap)
                        return 1;

                    if (_nodes.Count == 0)
                        return 0;

                    int new_index = _nodes.Last.Value._node_idx + 1;
                    if ((new_index - _start_idx) >= _cap)
                        return 2;

                    return 0;
                }
            }

            public List<Layer> _layers;
            public Dictionary<int, LinkedListNode<ViewNode>> _node_dict;
            public int _node_cap = 0;
            public int _node_cap_now = 0;

            public ViewLayerList()
            {
                _layers = new List<Layer>();
                _node_cap = 0;
                _node_dict = new Dictionary<int, LinkedListNode<ViewNode>>();
                _node_cap_now = 0;
            }

            public int LayerCount => _layers.Count;


            public void AddLayer()
            {
                _layers.Add(new Layer()
                {
                    _nodes = new LinkedList<ViewNode>(),
                    _cap = C_NODE_INIT_COUNT,
                    _start_idx = _layers.Count * C_NODE_INIT_COUNT
                });

                _node_cap_now += C_NODE_INIT_COUNT;
            }

            /// <summary>
            ///  返回得是 index, 如果是 &lt; 0  说明出错了
            /// </summary>
            public int Add(int view_id, int layer_idx, ViewWrapper extra, List<ViewNode> changes)
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
                Layer layer = _layers[layer_idx];

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
                ViewNode data = new ViewNode();
                data._id = view_id;
                data._node_idx = new_idx;
                data._layer_idx = layer_idx;
                data._extra_data = extra;
                var node = layer._nodes.ExtAddLast(data);
                _node_dict.Add(view_id, node);
                changes.Add(data);
                return new_idx;
            }

            public bool Del(int view_id, out ViewNode val)
            {
                val = default;
                if (!_node_dict.TryGetValue(view_id, out var v))
                    return false;

                val = v.Value;
                v.List.ExtRemove(v);
                _node_dict.Remove(view_id);
                return true;
            }

            public bool MoveInLayer(int view_id, int count, List<ViewNode> changes)
            {
                changes.Clear();
                if (count == 0)
                    return true;
                _node_dict.TryGetValue(view_id, out var node);
                if (node == null)
                    return false;

                //2. 开始移动
                Layer layer = _layers[node.Value._layer_idx];
                LinkedList<ViewNode> node_list = layer._nodes;

                if (count > 0) //向上移动, 和链表的后面节点交换
                {
                    //要判断自己是否为最后一个
                    if (node.Next == null)
                        return true;

                    var temp_node = node;
                    for (int i = 0; i < count; i++)
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
                else if (count < 0) //向后移动, 和链表的前面节点交换
                {
                    //已经是第一个了
                    if (node.Previous == null)
                        return true;

                    count = -count;
                    var temp_node = node;
                    for (int i = 0; i < count; i++)
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

            public bool FindLast(Func<ViewNode, bool> func, out ViewNode val)
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

            public void GetList2<T2>(Func<ViewNode, T2, bool> func, T2 v, List<ViewNode> out_list)
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

            public void GetList(Func<ViewNode, bool> func, List<ViewNode> out_list)
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

            public bool GetVal(int id, out ViewWrapper val)
            {
                if (!_node_dict.TryGetValue(id, out var v))
                {
                    val = default;
                    return false;
                }
                val = v.Value._extra_data;
                return true;
            }

            public bool SetVal(int id, ViewWrapper ext_val)
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

            public static bool _extend(List<Layer> layers, int layer_index, List<ViewNode> changes)
            {
                layers[layer_index]._cap += C_NODE_INC_COUNT;

                //后续的都要重新调整 order
                for (int i = layer_index + 1; i < layers.Count; i++)
                {
                    Layer cur_layer = layers[i];
                    cur_layer._start_idx += C_NODE_INC_COUNT;
                    _sort_node_in_layer(cur_layer, changes);
                }

                return true;
            }

            public static void _sort_node_in_layer(Layer layer, List<ViewNode> changes)
            {
                LinkedListNode<ViewNode> node = layer._nodes.First;
                int index = layer._start_idx;
                for (; node != null; node = node.Next, index++)
                {
                    ViewNode data = node.Value;
                    if (data._node_idx == index)
                        continue;

                    data._node_idx = index;
                    node.Value = data;
                    changes.Add(data);
                }
            }
        }
    }
}
