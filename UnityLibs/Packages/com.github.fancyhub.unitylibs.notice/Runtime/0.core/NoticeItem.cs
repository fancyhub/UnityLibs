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
    public struct NoticeItemDummy
    {
        public readonly Transform Dummy;
        public readonly INoticeChannelRoot ChannelRoot;

        public NoticeItemDummy(Transform dummy, INoticeChannelRoot channel_root)
        {
            Dummy = dummy;
            ChannelRoot = channel_root;
        }

        public NoticeItemDummy(GameObject dummy, INoticeChannelRoot channel_root)
        {
            Dummy = dummy.transform;
            ChannelRoot = channel_root;
        }
    }


    public interface INoticeItem : IDestroyable
    {
        void Show(NoticeItemDummy dummy);
        void FadeIn(NoticeItemTime time, NoticeEffectConfig effect);
        void FadeOut(NoticeItemTime time, NoticeEffectConfig effect);
        
        bool TryMerge(INoticeItem other);
        bool IsValid();
        Vector2 GetViewSize();

        void Update(NoticeItemTime time);
    }
}
