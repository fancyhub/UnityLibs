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

    /// <summary>
    /// 正在显示的队列
    /// </summary>
    public class NoticeMultiShowingQueue
    {
        private const float CSpace = 5;

        public enum EState
        {
            Wait, //等待新的
            Moving, //等待槽位移动
            Full, //满了
        }

        public NoticeContainerConfig _Config;
        public NoticeItemWrapper[] _Slots;
        public float[] _SlotsPosArray;

        public EState _State;
        public INoticeChannelRoot _Root;
        public IClock _Clock;
        public int _Interval;
        public long _MovingTimeEnd;

        public NoticeMultiShowingQueue(INoticeChannelRoot root, IClock clock, NoticeContainerConfig config)
        {
            _Config = config;
            int max_count_show = Mathf.Max(1, config.Multi.MaxShowCount);
            _Slots = new NoticeItemWrapper[max_count_show];
            _SlotsPosArray = new float[max_count_show];
            _Root = root;
            _Interval = Mathf.Max(100, (int)(config.Multi.StepMoveDuration * 1000));
            _Clock = clock;
            _State = EState.Wait;
        }

        public bool CanAddItem()
        {
            if (_State == EState.Wait)
                return true;
            return false;
        }

        public void EnsureEmptySlots(int count)
        {
            if (count <= 0)
                return;

            count = Math.Min(count, _Slots.Length);

            int now_empty_count = 0;
            foreach (var p in _Slots)
            {
                if (p == null)
                    now_empty_count++;
            }

            if (now_empty_count >= count)
                return;

            var dt = count - now_empty_count;
            for (int i = _Slots.Length - 1; i >= 0; i--)
            {
                if (_Slots[i] == null)
                    continue;
                _Slots[i].Destroy();
                _Slots[i] = null;
                dt--;
                if (dt <= 0)
                    break;
            }
            _State = EState.Wait;
        }

        public bool AddItem(NoticeData data)
        {
            if (null == data.Item)
            {
                NoticeLog.Assert(false, "param data is null");
                return false;
            }

            if (_State != EState.Wait)
            {
                NoticeLog.Assert(false, "不能添加新的 {0}", _State);
                return false;
            }

            NoticeItemWrapper item = NoticeItemWrapper.Create(_Root, _Clock, data, _Config.Effect);
            if (item == null)
                return false;

            //先把item_list移动, 把0slot 保留下来
            int index_empty_slot = _IndexOfEmptySlot(_Slots);
            if (index_empty_slot != 0)
                _MoveItemsIndex(_Slots, index_empty_slot);

            _Slots[0] = item;

            //判断是否需要移动?
            bool need_move = _CalcItemsPos(_Slots, CSpace, _SlotsPosArray);
            if (!_Config.Multi.DirUp)
                _InversePosList(_SlotsPosArray);

            NoticeItemTime notice_time = _Config.Time.CreateNoticeItemTime(data);
            notice_time.Delay(_Clock.Time);
            if (need_move)
            {
                notice_time.Delay(_Interval);
                item.Show(notice_time);
                _MoveItems(_Slots, _SlotsPosArray, _Interval);
                _State = EState.Moving;
                _MovingTimeEnd = _Clock.Time + _Interval + notice_time.GetFadeInDuration();
            }
            else if (_IsFull(_Slots))
            {
                item.Show(notice_time);
                _State = EState.Full;
            }
            else
            {
                item.Show(notice_time);
                _State = EState.Wait;
            }
            item.Update();
            return true;
        }

        public void Update()
        {
            int item_count = 0;
            for (int i = 0; i < _Slots.Length; i++)
            {
                var item = _Slots[i];
                if (item == null)
                    continue;
                if (!item.IsValid())
                {
                    item.Destroy();
                    _Slots[i] = null;
                    continue;
                }

                item.Update();
                if (item.IsTimeOut())
                {
                    item.Destroy();
                    _Slots[i] = null;
                }
                else
                    item_count++;
            }

            switch (_State)
            {
                case EState.Full:
                    if (item_count < _Slots.Length)
                        _State = EState.Wait;
                    break;

                case EState.Wait:
                    break;

                case EState.Moving:
                    if (_Clock.Time > _MovingTimeEnd)
                    {
                        if (item_count < _Slots.Length)
                            _State = EState.Wait;
                        else
                            _State = EState.Full;
                    }
                    break;

                default:
                    break;
            }
        }

        public void ClearItems()
        {
            for (int i = 0; i < _Slots.Length; i++)
            {
                var item = _Slots[i];
                if (item == null)
                    continue;
                item.Destroy();
                _Slots[i] = null;
            }
            _State = EState.Wait;
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
}