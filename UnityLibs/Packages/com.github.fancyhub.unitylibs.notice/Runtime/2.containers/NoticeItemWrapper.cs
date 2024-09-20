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
    /// Item 的包装类
    /// </summary>
    public sealed class NoticeItemWrapper : CPoolItemBase, IDestroyable
    {
        private NoticeEffectConfig _effectConfig;
        private IClock _clock;
        private INoticeChannelRoot _root;
        private INoticeItem _item;
        private GameObject _move_obj;
        private NoticeItemTime _time;
        private NoticeItemMove _move;
        private InitableValue<Vector2> _size;
        private int _priority;

        public static NoticeItemWrapper Create(
            INoticeChannelRoot root,
            IClock clock,
            NoticeData data,
            NoticeEffectConfig config)
        {
            GameObject self_root = root.CreateItemDummy();

            if (self_root == null)
                return null;
            //self_root.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            //self_root.transform.SetParent(channel_root.transform, false);

            NoticeItemWrapper ret = GPool.New<NoticeItemWrapper>();
            ret._effectConfig = config;
            ret._priority = data.Priority;
            ret._clock = clock;
            ret._item = data.Item;
            ret._root = root;
            ret._size.Clear();
            ret._move_obj = self_root;
            ret._item.Show(new NoticeItemDummy(self_root, root));
            return ret;
        }

        public int Priority => _priority;

        public Vector2 GetSize()
        {
            if (!_size.Inited && _item.IsValid())
                _size.Value = _item.GetViewSize();
            return _size.Value;
        }

        public Vector2 GetPos()
        {
            return _move_obj.transform.localPosition;
        }

        public void MoveTo(float pos, long duration_ms)
        {
            Vector3 start_pos = _move_obj.transform.localPosition;
            Vector3 end_pos = new Vector3(0, pos, 0);
            long now = _clock.Time;
            Range64 time_range = new Range64(now, now + duration_ms);
            _move = new NoticeItemMove(start_pos, end_pos, time_range);
        }

        protected override void OnPoolRelease()
        {
            _item?.Destroy();
            _item = null;

            _root.ReleaseItemDummy(_move_obj);
            _root = null;
            _move_obj = null;

            _effectConfig = null;

            _size.Clear();

            _clock = null;
        }

        public void Show(NoticeItemTime notice_time)
        {
            _time = notice_time;
            _time.SetTimeNow(_clock.Time);
            if (_time.Phase == ENoticeItemPhase.Wait)
                _move_obj.SetActive(false);
            else
                _move_obj.SetActive(true);
        }

        public bool IsValid()
        {
            if (_move_obj == null)
                return false;
            if (_item == null)
                return false;
            return _item.IsValid();
        }

        public void Update()
        {
            //1. 获取时间
            long now = _clock.Time;

            //2. 检查移动
            if (_move._moving)
            {
                Vector3 new_pos = _move.GetPos(now, out bool end);
                _move_obj.transform.localPosition = new_pos;
                _move._moving = !end;
            }

            //3. 根据阶段来调用不同的逻辑
            _time.SetTimeNow(now);
            switch (_time.Phase)
            {
                case ENoticeItemPhase.Wait:
                    return;

                case ENoticeItemPhase.ShowIn:
                    _move_obj.SetActive(true);
                    _item.FadeIn(_time, _effectConfig);
                    break;

                case ENoticeItemPhase.HideOut:
                    _move_obj.SetActive(true);
                    _item.FadeOut(_time, _effectConfig);
                    break;

                case ENoticeItemPhase.Showing:
                    _move_obj.SetActive(true);
                    _item.Update(_time);
                    break;

                case ENoticeItemPhase.End:
                    break;
            }
        }

        public bool IsTimeOut()
        {
            if (_time.IsEnd(_clock.Time))
                return true;
            return false;
        }


        public struct NoticeItemMove
        {
            public bool _moving;
            public Vector3 _start_pos;
            public Vector3 _end_pos;
            public Range64 _time_range;

            public NoticeItemMove(Vector3 start_pos, Vector3 end_pos, Range64 time_range)
            {
                _moving = true;
                _start_pos = start_pos;
                _end_pos = end_pos;
                _time_range = time_range;
            }

            public void Clear()
            {
                _moving = false;
            }

            public Vector3 GetPos(long time_now, out bool timeout)
            {
                if (time_now >= _time_range.Max)
                {
                    timeout = true;
                    return _end_pos;
                }

                float p = _time_range.GetClampPercent(time_now);
                timeout = false;
                return Vector3.Lerp(_start_pos, _end_pos, p);
            }
        }

        public struct InitableValue<T>
        {
            private bool _inited;
            private T _v;

            public bool Inited { get { return _inited; } }
            public T Value
            {
                get
                {
                    Debug.Assert(_inited, "值还没有初始化");
                    return _v;
                }
                set
                {
                    _v = value;
                    _inited = true;
                }
            }

            public void Clear()
            {
                _inited = false;
                _v = default;
            }
        }
    }
}