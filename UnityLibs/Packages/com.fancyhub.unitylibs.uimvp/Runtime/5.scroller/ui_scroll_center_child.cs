
/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/2 16:11:09
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
    public class UIScrollCenterChild
    {
        public CPtr<EUIScroll> _scroller;
        public float _speed;
        public Vector3 _start_pos;
        public Vector3 _end_pos;
        public bool _moving = false;
        public bool _ver = true;
        public float _timer = 0;
        public float _total_time = 0;
        public int _index_selected = -1;
        public Action<int> _select_cb;
        public bool _invoke_cb = false;
        public float _max_move_time = 100;
        public UIScrollCenterChild(EUIScroll scroll, float speed, float max_time, Action<int> OnSelectCb)
        {
            _scroller = scroll;
            _speed = speed;
            scroll.EventMoveEnd += _on_scroll_move_end;
            scroll.EventDragStart += _on_drag_start;
            if (!scroll.UnityScroll.vertical)
                _ver = false;
            _speed = Mathf.Max(speed, 1);
            _select_cb = OnSelectCb;
            _max_move_time = max_time;
            UIScrollEventBehaviour b = UIScrollEventBehaviour.Get(scroll.UnityScroll);
            b._threshold = speed;
        }

        public void MoveTo(int index, bool invoke_cb = false)
        {
            EUIScroll scroller = _scroller;
            if (scroller == null)
                return;
            scroller.UnityScroll.StopMovement();
            _invoke_cb = invoke_cb;

            List<IScrollerItem> item_list = scroller.GetItemList();
            if (index < 0 || index >= item_list.Count)
            {
                _moving = false;
                return;
            }
            IScrollerItem item = item_list[index];

            _index_selected = index;
            Vector2 pos = scroller.ViewSize * 0.5f + scroller.ContentPos;
            Vector2 item_pos = item.Pos + item.Size * 0.5f;

            if (_index_selected < 0)
            {
                _moving = false;
                return;
            }

            Vector3 dt = item_pos - pos;
            if (_ver)
            {
                dt.x = 0;
                _total_time = dt.y / _speed;
            }
            else
            {
                dt.y = 0;
                _total_time = dt.x / _speed;
            }
            _total_time = Mathf.Abs(_total_time);

            if (_total_time < 0.1f)
            {
                _moving = false;
                if (invoke_cb)
                    _select_cb?.Invoke(_index_selected);
                return;
            }

            _total_time = Mathf.Min(_total_time, _max_move_time);
            _start_pos = scroller.UnityScroll.content.localPosition;
            _end_pos = _start_pos + dt;
            _timer = 0;
            _moving = true;

            UISceneMgr.AddUpdate(_on_update);            
        }

        public void _on_scroll_move_end()
        {
            EUIScroll scroller = _scroller;
            if (scroller == null)
                return;

            List<IScrollerItem> item_list = scroller.GetItemList();

            Vector2 pos = scroller.ViewSize * 0.5f + scroller.ContentPos;
            int index = _find_closest_item(item_list, pos);
            MoveTo(index, true);
        }

        public EUIUpdateResult _on_update(float dt)
        {
            if (!_moving)
            {
                return EUIUpdateResult.Stop;
            }
            EUIScroll scroller = _scroller;
            if (scroller == null)
                return EUIUpdateResult.Stop;

            _timer += dt;
            float r = _timer / _total_time;
            bool is_end = (_timer >= _total_time);

            Vector3 pos = Vector3.Lerp(_start_pos, _end_pos, r);
            if (is_end)
                pos = _end_pos;
            scroller.UnityScroll.content.localPosition = pos;

            if (is_end)
            {
                _moving = false;
                if (_invoke_cb)
                    _select_cb?.Invoke(_index_selected);
                return EUIUpdateResult.Stop;
            }

            return EUIUpdateResult.Continue;
        }

        public void _on_drag_start()
        {
            _moving = false;
        }

        public int _find_closest_item(List<IScrollerItem> item_list, Vector2 pos)
        {
            float min_dist = float.MaxValue;
            int index = -1;
            for (int i = 0; i < item_list.Count; i++)
            {
                var item = item_list[i];
                Vector2 item_pos = item.Pos + item.Size * 0.5f;
                Vector2 pos_dt = item_pos - pos;
                float dist = pos_dt.sqrMagnitude;
                if (min_dist > dist)
                {
                    index = i;
                    min_dist = dist;
                }
            }
            return index;
        }
    }
}
