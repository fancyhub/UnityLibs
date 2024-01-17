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
    public sealed class NoticeItemDummy
    {
        public Transform Dummy;
        public INoticeChannelRoot ChannelRoot;

        public GameObject CreateView(string path)
        {
            return ChannelRoot.CreateView(path, Dummy);
        }

        public void ReleaseView(GameObject view)
        {
            ChannelRoot.ReleaseView(view);
        }

        public void ReleaseView(ref GameObject view)
        {
            ChannelRoot.ReleaseView(view);
            view = null;
        }

        public void ReleaseView(ref Transform view)
        {
            if (view == null)
                return;

            ChannelRoot.ReleaseView(view.gameObject);
            view = null;
        }

        public void ReleaseView(ref RectTransform view)
        {
            if (view == null)
                return;

            ChannelRoot.ReleaseView(view.gameObject);
            view = null;
        }
    }


    public interface INoticeItem : IDestroyable
    {
        void CreateView(NoticeItemDummy dummy);

        void ShowUp(NoticeItemTime time, List<NoticeEffectItemConfig> effect);
        void Update(NoticeItemTime time);
        void HideOut(NoticeItemTime time, List<NoticeEffectItemConfig> effect);

        bool Merge(INoticeItem other);

        Vector2 GetSize();
    }
}
