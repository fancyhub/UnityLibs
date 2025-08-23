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
    }

    public class NoticeContainer_Multi : INoticeContainer
    {
        public NoticeMultiShowingQueue _show_queue;
        public NoticeContainerConfig _Config;
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
                _show_queue = new NoticeMultiShowingQueue(context.Root, context.Clock, _Config);

            _show_queue.Update();

            if (!context.Visible)
                return;

            //立即模式
            if (_Config.Immediate)
            {
                context.DataQueue.StripForMultiImmediate(_Config.Multi.MaxShowCount, context.Clock.Time);
                _show_queue.EnsureEmptySlots(context.DataQueue.Count);

                for (; ; )
                {
                    if (!_show_queue.CanAddItem())
                        break;

                    if (!context.DataQueue.Pop(out NoticeData data, context.Clock.Time))
                        break;

                    _show_queue.AddItem(data);
                }
            }
            else
            {
                for (; ; )
                {
                    if (!_show_queue.CanAddItem())
                        break;

                    if (!context.DataQueue.Pop(out NoticeData data, context.Clock.Time))
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