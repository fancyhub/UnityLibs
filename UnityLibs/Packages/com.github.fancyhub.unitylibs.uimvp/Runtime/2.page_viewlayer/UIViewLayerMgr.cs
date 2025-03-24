/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/31 17:09:02
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    public interface IUISharedBG
    {
        public void DisableMask();
        public void EnableMask(int order);

        public void DisableClick();
        public void EnableClick(int order);
        public int GetClickOrder();
        public bool IsClickEnable();
    }

    public sealed class UIViewLayerMgr : IUIViewLayerMgr
    {
        private int _cur_click_view_id; //当前click的 view id
        private int _cur_mask_view_id; //当前显示mask的view id

        private int _count_of_tip_click;//记录当前需要tip click 的view 数量
        private int _count_of_tip_down; //记录当前需要tip down 的view 数量

        private UIViewLayerList _layer_list;
        private List<UIViewNode> _temp_list;
        private IUISharedBG _ui_shared_bg;

        public UIViewLayerMgr(IUISharedBG ui_bg)
        {
            _layer_list = new UIViewLayerList();

            _temp_list = new List<UIViewNode>();
            _count_of_tip_click = 0;
            _count_of_tip_down = 0;
            _ui_shared_bg = ui_bg;

            UIBGEvent.GlobalBGClick = _on_bg_click;
            UIBGEvent.GlobalEventClick = _on_global_click;
            UIBGEvent.GlobalEventDown = _on_global_down;
        }

        public void AddLayer(string layer_name)
        {
            _layer_list.AddLayer(layer_name);
        }

        public bool AddView(IUILayerView view, int layer_index, IUILayerViewBGHandler handler, EUIBgClickMode click_mode)
        {
            if (!AddView(view, layer_index))
                return false;

            return SetBgClick(view.Id, handler, click_mode);
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

            UIViewWrapper data = UIViewWrapper.Create();
            data._view = view;
            data._click_mode = EUIBgClickMode.None;
            data._bg_mask = false;
            data._click_handler = null;

            int new_idx = _layer_list.AddView(view.Id, layer_index, data, _temp_list);
            foreach (var p in _temp_list)
            {
                p._extra_data.SetOrder(UIViewLayerConst.CalcOrder(p._node_idx));
            }

            if (new_idx < 0)
                return false;
            _refresh_mask(_ui_shared_bg, _layer_list, ref _cur_mask_view_id);
            _refresh_click(_ui_shared_bg, _layer_list, ref _cur_click_view_id);
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
                _refresh_click(_ui_shared_bg, _layer_list, ref _cur_click_view_id);
            }

            if (view_id == _cur_mask_view_id)
            {
                _refresh_mask(_ui_shared_bg, _layer_list, ref _cur_mask_view_id);
            }

            data._extra_data.Destroy();
            return true;
        }

        /// <summary>
        /// 移动View的层次, offset 是要移动的便宜
        /// </summary>
        /// <param name="view_id"></param>
        /// <param name="offset">0: 不移动<br/> &gt;0: 向上移动 <br/> &lt;0: 向下移动</param>
        public bool MoveView(int view_id, int offset)
        {
            if (!_layer_list.MoveInLayer(view_id, offset, _temp_list))
                return false;

            if (offset == 0)
                return true;

            foreach (var p in _temp_list)
            {
                p._extra_data.SetOrder(UIViewLayerConst.CalcOrder(p._node_idx));
            }

            _refresh_mask(_ui_shared_bg, _layer_list, ref _cur_mask_view_id);
            _refresh_click(_ui_shared_bg, _layer_list, ref _cur_click_view_id);
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
            _refresh_mask(_ui_shared_bg, _layer_list, ref _cur_mask_view_id);
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
            _refresh_click(_ui_shared_bg, _layer_list, ref _cur_click_view_id);
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

            int order = _ui_shared_bg.GetClickOrder();
            if (!_ui_shared_bg.IsClickEnable())
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

                if (_is_child(p._extra_data._view.GetRoot(), obj))
                    continue;
                handler.OnBgClick();
            }
        }

        private void _on_global_click(GameObject obj)
        {
            if (_count_of_tip_click == 0)
                return;

            int order = _ui_shared_bg.GetClickOrder();
            if (!_ui_shared_bg.IsClickEnable())
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

                if (_is_child(p._extra_data._view.GetRoot(), obj))
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

        private static void _refresh_mask(IUISharedBG ui_bg, UIViewLayerList list, ref int cur_mask_id)
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

        private static void _refresh_click(IUISharedBG ui_bg, UIViewLayerList list, ref int cur_click_id)
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
    }
}
