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
    public enum ENoticeEffect
    {
        None,
        Alpha,
        OffsetY,
        RightSlide,
        LeftSlide,
        ScaleY,
        ScaleX,
    }

    /// <summary>
    /// 不同的容器类型
    /// </summary>
    public enum ENoticeContainer
    {
        None,
        Single,
        Multi,
        Max,
    }

    [Serializable]
    public class NoticeTimeConfig
    {
        [Header("从不显示->到显示的时间")]
        public float ShowUpDuration = 0.3f;

        [Header("从显示 -> 完全隐藏的时间")]
        public float HideOutDuration = 0.3f;

        public NoticeItemTime CreateNoticeItemTime(NoticeData data)
        {
            return new NoticeItemTime(data.DurationShow, (int)(ShowUpDuration * 1000), (int)(HideOutDuration * 1000));
        } 
    }

    [System.Serializable]
    public sealed class NoticeEffectConfig
    {
        public List<NoticeEffectItemConfig> ShowUp = new List<NoticeEffectItemConfig>();
        public List<NoticeEffectItemConfig> HideOut = new List<NoticeEffectItemConfig>();
    }

    [Serializable]
    public sealed class NoticeContainerConfig
    {
        public ENoticeContainer ContainerType;
        [Tooltip("single Mode: 同等或者高优先级会把当前的顶掉,低优先级等待\n multi Mode: 忽略当前显示的优先级, 从队列里面直接获取最高优先级的数量")]
        public bool Immediate = false;
        public NoticeContainerMultiConfig Multi= new NoticeContainerMultiConfig();
        public NoticeTimeConfig Time = new NoticeTimeConfig();
        public NoticeEffectConfig Effect= new NoticeEffectConfig();
    }
}
