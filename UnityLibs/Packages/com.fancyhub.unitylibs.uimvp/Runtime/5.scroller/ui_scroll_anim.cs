/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/31 17:23:53
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    public class UIScrollAnim
    {
        //动画播放的方向
        public static Vector4 IN_DIR_LEFT_2_RIGHT = new Vector4(-1.5f, 0, 0, 0);
        public static Vector4 IN_DIR_RIGHT_2_LEFT = -IN_DIR_LEFT_2_RIGHT;
        public static Vector4 IN_DIR_BOTTOM_2_TOP = new Vector4(0, 1.5f, 0, 0);
        public static Vector4 IN_DIR_TOP_2_BOTTOM = -IN_DIR_BOTTOM_2_TOP;

        public static Vector4 OUT_DIR_LEFT_2_RIGHT = new Vector4(0, 0, -1.5f, 0);
        public static Vector4 OUT_DIR_RIGHT_2_LEFT = -OUT_DIR_LEFT_2_RIGHT;
        public static Vector4 OUT_DIR_BOTTOM_2_TOP = new Vector4(0, 0, 0, 1.5f);
        public static Vector4 OUT_DIR_TOP_2_BOTTOM = -OUT_DIR_BOTTOM_2_TOP;


        //动画的curve，ease in out，如果需要其他的，可以增加
        public static AnimationCurve _s_easy_in_out;

        public EUIScroll _scroller;
        public AnimationCurve _anim_curve;
        public float _single_time;
        public float _interval;
        public float _total_time;
        public double _time;
        public Vector2 _start_anim_pos;
        public Vector2 _end_anim_pos;
        public bool _playing = false;
        public Action _end_cb = null;
        public Func<int, int, int> _index_map;

        public UIScrollAnim(EUIScroll scroller)
        {
            _scroller = scroller;
            if (null == _s_easy_in_out)
            {
                _s_easy_in_out = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }
        }

        public void Play(Vector4 dir,
            float single_time,
            float interval_time,
            float total_time,
            Action end_cb = null,
            Func<int, int, int> index_map = null)
        {
            _end_cb = end_cb;
            _index_map = index_map;
            _single_time = Mathf.Max(single_time, 0.01f);
            _interval = Mathf.Max(interval_time, 0);
            _total_time = total_time;
            _time = 0;
            _anim_curve = _s_easy_in_out;

            //1. 计算初始化位置
            Vector2 view_size = _scroller.ViewSize;
            _start_anim_pos = new Vector2(view_size.x * dir.x, view_size.y * dir.y);
            _end_anim_pos = new Vector2(view_size.x * dir.z, view_size.y * dir.w);


            //2. 更新一次
            _update(0);

            //3. 挂载更新
            if (!_playing)
                UISceneMgr.AddUpdate(_update);
            _playing = true;
        }

        public void Play(Vector4 dir,
            float single_time,
            float interval_time,
            Action end_cb = null,
            Func<int, int, int> index_map = null)
        {
            float total_time = CalcTotalTime(_scroller, single_time, interval_time);
            Play(dir, single_time, interval_time, total_time, end_cb, index_map);
        }

        public static float CalcTotalTime(
            EUIScroll scroller
            , float single_time
            , float interval_time)
        {
            //1. 计算total time
            float total_time;
            List<IScrollerItem> item_list = scroller.GetItemList();
            int count = item_list.Count;
            if (count == 0)
                total_time = 0.01f;
            else
                total_time = single_time + (count - 1) * interval_time;
            return total_time;
        }

        public static int RowMultiIndexMap(int cell_count_per_row, int item_list_count, int index)
        {
            int row_index = index / cell_count_per_row;

            //计算出该行的范围
            int row_start_idx = row_index * cell_count_per_row;
            int row_end_idx = row_start_idx + cell_count_per_row - 1;

            //计算该index 所在的col index
            int col_index = index - row_start_idx;

            int ret = row_start_idx + (cell_count_per_row - col_index - 1);

            //如果当前行是完整的
            if (row_end_idx < item_list_count)
                return ret;

            int dt = row_end_idx - item_list_count + 1;
            return ret - dt;
        }

        public void Stop()
        {
            if (!_playing)
                return;
            _playing = false;

            _reset_anim_pos(_scroller);
            _end_cb?.Invoke();
            _end_cb = null;
        }

        public EUIUpdateResult _update(float dt)
        {
            _time += dt;

            //1. 检查状态
            if (!_playing)
                return EUIUpdateResult.Stop;

            //2.检查时间，如果结束，直接重置动画位置            
            if (_time > _total_time)
            {
                _playing = false;
                _reset_anim_pos(_scroller);
                _end_cb?.Invoke();
                return EUIUpdateResult.Stop;
            }

            //3. 更新 item
            List<IScrollerItem> item_list = _scroller.GetItemList();
            int count = item_list.Count;
            for (int i = 0; i < count; ++i)
            {
                //1. 计算item 对应的curve值
                float start_time = i * _interval;
                float curve_time = (float)((_time - start_time) / _single_time);
                float curve_value = _anim_curve.Evaluate(Mathf.Clamp01(curve_time));

                //2. 计算anim的位置
                int index = i;
                if (_index_map != null)
                    index = _index_map(count, i);
                IScrollerItem item = item_list[index];
                item.AnimPos = Vector2.Lerp(_start_anim_pos, _end_anim_pos, curve_value);
            }

            _scroller.Refresh();
            return EUIUpdateResult.Continue;
        }

        public void _reset_anim_pos(EUIScroll scroller)
        {
            List<IScrollerItem> item_list = scroller.GetItemList();
            for (int i = 0; i < item_list.Count; ++i)
            {
                var item = item_list[i];
                item.AnimPos = Vector2.zero;
            }

            scroller.Refresh();
        }
    }
}
