/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/6/2
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    [System.Serializable]
    public struct UITagItem
    {
        public EUITagIndex Id;
        public EUITag Tag;
        public BitEnum64<EUITag> HideMask;

        public UITagItem(EUITagIndex id, EUITag tag, BitEnum64<EUITag> hideMask)
        {
            this.Id = id;
            Tag = tag;
            HideMask = hideMask;
        }

        public UITagItem(EUITagIndex id, EUITag tag)
        {
            this.Id = id;
            Tag = tag;
            HideMask = new BitEnum64<EUITag>(0UL);
        }

        public UITagItem(EUITagIndex id, BitEnum64<EUITag> hideMask)
        {
            this.Id = id;
            Tag = EUITag.None;
            HideMask = hideMask;
        }

        public UITagItem(EUITagIndex id)
        {
            this.Id = id;
            Tag = EUITag.None;
            HideMask = new BitEnum64<EUITag>(0UL);
        }
    }

    /// <summary>
    /// Tag 的矩阵,  
    /// </summary>
    public sealed class UIPageTagMgr : IUIPageTagMgr
    {
        private Dictionary<EUITagIndex, UITagItem> _Dict = new();
        private UIPageTagMatrix _PageTagMatrix = new UIPageTagMatrix();
        public UIPageTagMgr(List<UITagItem> configs)
        {
            foreach (var p in configs)
            {
                _Dict.Add(p.Id, p);
            }
        }

        public void AddPage(IUITagPage page, EUITagIndex tagId)
        {
            if (page == null)
                return;
            page.SetTagPageInfo(new UITagPageInfo(this));

            if (!_Dict.TryGetValue(tagId, out var item))
                return;

            if (item.Tag != EUITag.None)
            {
                if (_PageTagMatrix.AddTag(page, (byte)item.Tag))
                {
                    UILog._.D("Page:{0}, add tag to page succ", page.Id);
                }
                else
                {
                    UILog._.D("Page:{0}, add tag to page failed, tag {1} is not supported", page.Id, item.Tag);
                }
            }

            if (item.HideMask.Value != 0)
            {
                if (_PageTagMatrix.AddMask(page.Id, item.HideMask.Value))
                {
                    UILog._.D("Page:{0}, add mask to page succ", page.Id);
                }
                else
                {
                    UILog._.D("Page:{0}, add mask to page succ, failed", page.Id);
                }
            }
        }

        public void RemovePage(int page_id)
        {
            if (_PageTagMatrix.RemoveTag(page_id))
            {
                UILog._.D("Page:{0}, remove page tag succ", page_id);
            }

            if(_PageTagMatrix.RemoveMask(page_id))
            {
                UILog._.D("Page:{0}, remove page mask succ", page_id);
            }
        }
    }
}
