/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    //同一时间,只能显示一个
    public class NoticeContainer_Single : INoticeContainer
    {
        public NoticeItemWrapper _cur_item;
        public NoticeContainerConfig _config;
        public NoticeContainer_Single(NoticeContainerConfig config)
        {
            _config = config;
        }

        public void OnClear()
        {
            _cur_item?.Destroy();
            _cur_item = null;
        }

        public void OnDestroy()
        {
            OnClear();
        }

        public void OnUpdate(NoticeContainerContext context)
        {
            if (_cur_item != null)
            {
                if (_cur_item.IsValid())
                {
                    _cur_item.Update();
                    if (_cur_item.IsTimeOut())
                    {
                        _cur_item.Destroy();
                        _cur_item = null;
                    }
                }
                else
                {
                    _cur_item.Destroy();
                    _cur_item = null;
                }
            }
            NoticeData next_data = null;

            if (_cur_item == null)
                next_data = context._data_queue.Pop(context._clock.Time);
            else
                next_data = context._data_queue.Pop(context._clock.Time, _cur_item.Priority);

            if (next_data == null)
                return;

            _cur_item?.Destroy();
            _cur_item = NoticeItemWrapper.Create(context._root, context._clock, next_data, _config.Effect);
            if (_cur_item == null)
                return;

            NoticeItemTime notice_time = _config.Time.CreateNoticeItemTime(next_data);
            notice_time.Delay(context._clock.Time);
            _cur_item.Show(notice_time);
            _cur_item.Update();
        }

        public void OnVisibleChange(bool visible)
        {
            if (!visible)
            {
                _cur_item?.Destroy();
                _cur_item = null;
            }
        }
    }
}
