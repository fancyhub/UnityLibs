/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace FH
{
    [Serializable]
    public class NoticeContainerMultiConfig
    {
        /// <summary>
        /// 获取最多多少步数
        /// 这个数据的数量代表了当前最多显示的item个数
        /// </summary>
        [Header("只有Container Type 是multi才生效")]
        public int MaxShowCount = 3;

        [Header("移动的时间")]
        public float StepMoveDuration = 0.3f;

        public bool DirUp = true;

        [Header("true, 会忽略优先级, 立刻把当前显示的删除掉")]
        public bool Immediate = false;
    }


    /// <summary>
    /// 正在显示的队列
    /// </summary>
    public class NoticeMultiShowingQueue
    {
        private const float CSpace = 5;
        public enum EState
        {
            Wait, //等待新的
            Moveing, //等待槽位移动
            Full, //满了
        }

        public NoticeItemWrapper[] _item_array;

        public float[] _item_pos_list;
        public EState _state;
        public INoticeChannelRoot _root;
        public IClock _clock;
        public int _interval;
        public long _wait_time_end;
        public int _max_count_show;
        public NoticeContainerConfig _config;

        public NoticeMultiShowingQueue(INoticeChannelRoot root, IClock clock, NoticeContainerConfig config)
        {
            _config = config;
            int max_count_show = Mathf.Max(1, config.Multi.MaxShowCount);
            _item_array = new NoticeItemWrapper[max_count_show];
            _item_pos_list = new float[max_count_show];
            _root = root;
            _interval = Mathf.Max(100, (int)(config.Multi.StepMoveDuration * 1000));
            _clock = clock;
            _state = EState.Wait;
        }

        public bool CanAddItem()
        {
            if (_state == EState.Wait)
                return true;
            return false;
        }

        public void Ensure(int count)
        {
            if (count <= 0)
                return;

            count = Math.Min(count, _item_array.Length);

            int now_empty_count = 0;
            foreach (var p in _item_array)
            {
                if (p == null)
                    now_empty_count++;
            }

            if (now_empty_count <= count)
                return;

            var dt = count - now_empty_count;
            for (int i = 0; i < _item_array.Length; i++)
            {
                if (_item_array[i] == null)
                    continue;
                _item_array[i].Destroy();
                _item_array[i] = null;
                dt--;
                if (dt <= 0)
                    break;
            }

            _state = EState.Wait;
        }

        public bool AddItem(NoticeData data)
        {
            if (null == data)
                return false;

            if (_state != EState.Wait)
            {
                Debug.Assert(false, "不能添加新的");
                return false;
            }

            NoticeItemWrapper item = NoticeItemWrapper.Create(_root, _clock, data, _config.Effect);
            if (item == null)
                return false;

            //先把item_list移动, 把0slot 保留下来
            int index_empty_slot = _IndexOfEmptySlot(_item_array);
            if (index_empty_slot != 0)
                _MoveItemsIndex(_item_array, index_empty_slot);

            _item_array[0] = item;

            //判断是否需要移动?
            bool need_move = _CalcItemsPos(_item_array, CSpace, _item_pos_list);
            if (!_config.Multi.DirUp)
                _InversePosList(_item_pos_list);

            NoticeItemTime notice_time = _config.Time.CreateNoticeItemTime(data);
            notice_time.Delay(_clock.Time);
            if (need_move)
            {
                notice_time.Delay(_interval);
                item.Show(notice_time);
                _MoveItems(_item_array, _item_pos_list, _interval);
                _state = EState.Moveing;
                _wait_time_end = _clock.Time + _interval + notice_time.GetFadeInDuration();
            }
            else if (_IsFull(_item_array))
            {
                item.Show(notice_time);
                _state = EState.Full;
            }
            else
            {
                item.Show(notice_time);
                _state = EState.Wait;
            }
            item.Update();
            return true;
        }

        public void Update()
        {
            int item_count = 0;
            for (int i = 0; i < _item_array.Length; i++)
            {
                var item = _item_array[i];
                if (item == null)
                    continue;
                if (!item.IsValid())
                {
                    item.Destroy();
                    _item_array[i] = null;
                    continue;
                }

                item.Update();
                if (item.IsTimeOut())
                {
                    item.Destroy();
                    _item_array[i] = null;
                }
                else
                    item_count++;
            }

            switch (_state)
            {
                case EState.Full:
                    if (item_count < _item_array.Length)
                        _state = EState.Wait;
                    break;

                case EState.Wait:
                    break;

                case EState.Moveing:
                    if (_clock.Time > _wait_time_end)
                    {
                        if (item_count < _item_array.Length)
                            _state = EState.Wait;
                        else
                            _state = EState.Full;
                    }
                    break;

                default:
                    break;
            }
        }

        public void ClearItems()
        {
            for (int i = 0; i < _item_array.Length; i++)
            {
                var item = _item_array[i];
                if (item == null)
                    continue;
                item.Destroy();
                _item_array[i] = null;
            }
            _state = EState.Wait;
        }

        public void Destroy()
        {
            ClearItems();
        }


        private static bool _IsFull(NoticeItemWrapper[] slots)
        {
            foreach (var p in slots)
            {
                if (p == null)
                    return false;
            }
            return true;
        }

        private static void _MoveItems(NoticeItemWrapper[] slots, float[] pos_list, long duration_ms)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var item = slots[i];
                if (item == null)
                    continue;

                item.MoveTo(pos_list[i], duration_ms);
            }
        }

        private static bool _CalcItemsPos(NoticeItemWrapper[] slots, float space, float[] out_pos_list)
        {
            bool need_move = false;
            float start_pos = -10000;
            for (int i = 0; i < slots.Length; i++)
            {
                var item = slots[i];
                if (item == null)
                    continue;

                float cur_pos = item.GetPos().y;
                float cur_size = item.GetSize().y;
                float pos_min = start_pos + cur_size * 0.5f;

                out_pos_list[i] = Mathf.Max(cur_pos, pos_min);
                if (cur_pos < pos_min)
                    need_move = true;
                start_pos = out_pos_list[i] + cur_size * 0.5f + space;
            }
            return need_move;
        }

        private static void _InversePosList(float[] pos_list)
        {
            for (int i = 0; i < pos_list.Length; i++)
                pos_list[i] = -pos_list[i];
        }

        private static void _MoveItemsIndex(NoticeItemWrapper[] slots, int slot)
        {
            for (int i = slot; i > 0; i--)
                slots[i] = slots[i - 1];
            slots[0] = null;
        }

        private static int _IndexOfEmptySlot(NoticeItemWrapper[] slots)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                    return i;
            }
            return -1;
        }
    }


    public class NoticeContainer_Multi : INoticeContainer
    {
        public NoticeMultiShowingQueue _show_queue;
        public NoticeContainerConfig _Config;
        private List<NoticeData> _ImmediateTempList;
        public NoticeContainer_Multi(NoticeContainerConfig config)
        {
            _Config = config;
        }

        /// <summary>
        /// 注释参照基类中的注释
        /// </summary>
        public void OnVisibleChange(bool visible)
        {
            if (!visible)
                _show_queue?.ClearItems();
        }

        /// <summary>
        /// 注释参照基类中的注释
        /// </summary>
        public void OnClear()
        {
            _show_queue?.ClearItems();
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        public void OnUpdate(NoticeContainerContext context)
        {
            if (_show_queue == null)
                _show_queue = new NoticeMultiShowingQueue(context._root, context._clock, _Config);

            _show_queue.Update();

            if (!context._Visible)
                return;

            //立即模式
            if (_Config.Multi.Immediate)
            {
                if (_ImmediateTempList == null)
                    _ImmediateTempList = new List<NoticeData>();
                _ImmediateTempList.Clear();
                for (int i = 0; i < _Config.Multi.MaxShowCount; i++)
                {
                    NoticeData data = context._data_queue.Pop(context._clock.Time);
                    if (data == null)
                        break;
                    _ImmediateTempList.Add(data);
                }
                context._data_queue.Clear();

                _show_queue.Ensure(_ImmediateTempList.Count);

                foreach (var p in _ImmediateTempList)
                {
                    _show_queue.AddItem(p);
                }
            }
            else
            {
                for (; ; )
                {
                    if (!_show_queue.CanAddItem())
                        break;

                    NoticeData data = context._data_queue.Pop(context._clock.Time);
                    if (data == null)
                        break;

                    _show_queue.AddItem(data);
                }
            }
        }

        public void OnDestroy()
        {
            _show_queue.Destroy();
        }
    }
}