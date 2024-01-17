///*************************************************************************************
// * Author  : cunyu.fan
// * Time    : 2021/6/23
// * Title   : 
// * Desc    : 
//*************************************************************************************/

//using System;
//using UnityEngine;

//namespace FH
//{
//    [Serializable]
//    public class NoticeContainerMultiImmediateConfig
//    {
//        /// <summary>
//        /// 获取最多多少步数
//        /// 这个数据的数量代表了当前最多显示的item个数
//        /// </summary>
//        [Header("只有Container Type 是multi immediate才生效")]
//        public int _max_show_count;
//    }

//    public class NoticeContainer_MultiImmediate : INoticeContainer
//    {
//        public NoticeShowingQueue _show_queue;
//        public NoticeConfig _config;
//        public NoticeContainer_MultiImmediate(NoticeConfig config)
//        {
//            _config = config;
//        }

//        /// <summary>
//        /// 注释参照基类中的注释
//        /// </summary>
//        public void OnVisibleChange(bool visible)
//        {
//            if (!visible)
//                _show_queue?.ClearItems();
//        }

//        /// <summary>
//        /// 注释参照基类中的注释
//        /// </summary>
//        public void OnClear()
//        {
//            _show_queue?.ClearItems();
//        }

//        /// <summary>
//        /// 更新状态
//        /// </summary>
//        public void OnUpdate(NoticeContainerContext context)
//        {
//            if (_show_queue == null)
//            {
//                _show_queue = new NoticeShowingQueue(context._root, context._clock, _config);
//            }
//            _show_queue.Update();

//            if (!context._visible)
//                return;

//            for (; ; )
//            {
//                NoticeData data = context._data_queue.Pop(context._clock.Time);
//                if (data == null)
//                    break;

//                _show_queue.AddItem(data);
//            }
//        }

//        public void OnDestroy()
//        {
//            _show_queue?.Destroy();
//        }

//        /// <summary>
//        /// 正在显示的队列
//        /// </summary>
//        public class NoticeShowingQueue
//        {
//            public NoticeItemWrapper[] _item_list;
//            public float[] _item_pos_list;
//            public int _item_count;
//            public INoticeChannelRoot _root;
//            public IClock _clock;
//            public int _interval;
//            public long _wait_time_end;
//            public float _space = 5;
//            public int _max_count_show;
//            public int _top_max_count_show;

//            public NoticeConfig _config;
//            private NoticeItemWrapper notVisibleSlot;

//            public NoticeShowingQueue(
//                INoticeChannelRoot root,
//                IClock clock,
//                NoticeConfig config)
//            {
//                _config = config;
//                _max_count_show = Mathf.Max(1, config._Multi._MaxShowCount);
//                _item_list = new NoticeItemWrapper[_max_count_show];
//                _item_pos_list = new float[_max_count_show];
//                _item_count = 0;
//                _root = root;
//                _interval = Mathf.Max(100, (int)(config._Multi._StepMoveDuration * 1000));
//                _clock = clock;
//                _top_max_count_show = config._MultiImmediate._max_show_count;
//            }

//            public bool AddItem(NoticeData data)
//            {
//                if (null == data)
//                    return false;
//                NoticeItemWrapper item = NoticeItemWrapper.Create(_root, _clock, data, _config);
//                if (item == null)
//                    return false;

//                //先把item_list移动, 把0slot 保留下来
//                int index_empty_slot = _index_of_empty_slot(_item_list);
//                int emptyindex = 0;
//                if (index_empty_slot != 0)
//                {
//                    emptyindex = _move_items_index(_item_list, index_empty_slot, _top_max_count_show, data._priority);
//                }

//                _item_list[emptyindex] = item;
//                _item_count++;

//                _calc_item_pos(_item_list, _item_pos_list, _space);

//                if (!_config._Multi._DirUp)
//                    _inverse_pos_list(_item_pos_list);

//                NoticeTime notice_time = new NoticeTime(data._duration_show, _config._time);
//                notice_time.Delay(_clock.Time);

//                // notice_time.Delay(_interval);
//                item.Show(notice_time);
//                if (_item_count > 0)
//                {
//                    _move_items(_item_list, _item_pos_list, _interval);
//                    _wait_time_end = _clock.Time + _interval + notice_time.GetFadeInDuration();
//                }

//                Update();
//                return true;
//            }

//            public void Update()
//            {
//                _item_count = 0;
//                for (int i = 0; i < _item_list.Length && i < _max_count_show; i++)
//                {
//                    var item = _item_list[i];
//                    if (item == null)
//                        continue;

//                    if (!item.IsValid())
//                    {
//                        _item_list[i] = null;
//                        item.Destroy();
//                        continue;
//                    }

//                    item.Update();
//                    if (item.IsTimeOut())
//                    {
//                        item.Destroy();
//                        _item_list[i] = null;
//                    }
//                    else
//                        _item_count++;
//                }
//            }

//            public void ClearItems()
//            {
//                for (int i = 0; i < _item_list.Length; i++)
//                {
//                    var item = _item_list[i];
//                    if (item == null)
//                        continue;
//                    item.Destroy();
//                    _item_list[i] = null;
//                }

//                _item_count = 0;
//            }

//            public void Destroy()
//            {
//                ClearItems();
//            }

//            private static void _move_items(NoticeItemWrapper[] slots, float[] pos_list, long duration_ms)
//            {
//                for (int i = 0; i < slots.Length; i++)
//                {
//                    var item = slots[i];
//                    if (item == null)
//                        continue;

//                    item.MoveTo(pos_list[i], duration_ms);
//                }
//            }

//            private static bool _calc_item_pos(
//                NoticeItemWrapper[] slots,
//                float[] pos_list,
//                float space)
//            {
//                bool need_move = false;
//                float start_pos = -1000;
//                for (int i = 0; i < slots.Length; i++)
//                {
//                    var item = slots[i];
//                    if (item == null)
//                        continue;

//                    float cur_pos = item.GetPos().y;
//                    float cur_size = item.GetSize().y;
//                    float pos_min = start_pos + cur_size * 0.5f;

//                    pos_list[i] = Mathf.Max(cur_pos, pos_min);
//                    if (cur_pos < pos_min)
//                        need_move = true;
//                    start_pos = pos_list[i] + cur_size * 0.5f + space;
//                }

//                return need_move;
//            }

//            private static void _inverse_pos_list(float[] pos_list)
//            {
//                for (int i = 0; i < pos_list.Length; i++)
//                    pos_list[i] = -pos_list[i];
//            }

//            private static int _move_items_index(NoticeItemWrapper[] slots, int slot, int top_max_count, int priority)
//            {
//                if (priority == 0)
//                {
//                    if (slot == slots.Length - 1 && slots[slot] != null)
//                    {
//                        slots[slot].Destroy();
//                        slots[slot] = null;
//                    }

//                    for (int i = slot; i > 0; i--)
//                    {
//                        if (slots[i - 1].Priority > 0)
//                        {
//                            slots[i] = null;
//                            return i;
//                        }

//                        slots[i] = slots[i - 1];
//                    }
//                }

//                if (priority > 0)
//                {
//                    int current_top_count = 0;
//                    for (int i = 0; i < slots.Length; i++)
//                    {
//                        if (slots[i] != null && slots[i].Priority > 0)
//                        {
//                            current_top_count++;
//                        }
//                    }

//                    if (top_max_count > 0 && current_top_count == top_max_count)
//                    {
//                        slots[top_max_count - 1].Destroy();
//                        slots[top_max_count - 1] = null;
//                        for (int i = top_max_count - 1; i > 0; i--)
//                        {
//                            slots[i] = slots[i - 1];
//                        }
//                    }
//                    else
//                    {
//                        if (slot == slots.Length - 1 && slots[slot] != null)
//                        {
//                            slots[slot].Destroy();
//                            slots[slot] = null;
//                        }

//                        for (int i = slot; i > 0; i--)
//                        {
//                            slots[i] = slots[i - 1];
//                        }
//                    }
//                }

//                // if (slots[0] != null)
//                // {
//                //     slots[0].Destroy();
//                // }

//                slots[0] = null;
//                return 0;
//            }

//            private static int _index_of_empty_slot(NoticeItemWrapper[] slots)
//            {
//                for (int i = 0; i < slots.Length; i++)
//                {
//                    if (slots[i] == null)
//                        return i;
//                }

//                return slots.Length - 1;
//            }
//        }
//    }       
//}