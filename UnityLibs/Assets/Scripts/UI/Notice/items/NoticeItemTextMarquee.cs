/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FH;
namespace Game
{
    public sealed class NoticeItemTextMarquee : CPoolItemBase, INoticeItem
    {
        public string _Text;
        public NoticeItemDummy _dummy;

        public UINoticeMarqueeView _view;
        public CPtr<IResInstHolder> _res_holder;
        public static NoticeItemTextMarquee Create(IResInstHolder resholder, string text)
        {
            NoticeItemTextMarquee ret = GPool.New<NoticeItemTextMarquee>();
            ret._Text = text;
            ret._res_holder = new CPtr<IResInstHolder>(resholder);
            return ret;
        }

        protected override void OnPoolRelease()
        {
            _dummy = default;
            _view?.Destroy();
            _view = null;
        }
        public bool IsValid()
        {
            return _view != null;
        }

        public Vector2 GetViewSize()
        {
            if (_view == null)
                return Vector2.one;
            return _view.SelfRootTran.rect.size;
        }

        public bool TryMerge(INoticeItem other)
        {
            return false;
        }

        public void Show(NoticeItemDummy dummy)
        {
            _dummy = dummy;

            _view = FH.UI.UIBaseView.CreateView<UINoticeMarqueeView>(_dummy.Dummy, _res_holder.Val);
            if (_view == null)
                return;
            _view._txt.text = _Text;
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_view.SelfRootTran);
            _SetProgress(0);
        }

        public void Update(float progress)
        {
            _SetProgress(progress);
        }

        public void FadeOut(float progress, NoticeEffectConfig effect)
        {
            if (_view != null)
                NoticeEffectPlayer.Play(_view.SelfRootTran, progress, effect.HideOut);
        }

        public void FadeIn(float progress, NoticeEffectConfig effect)
        {
            if (_view != null)
                NoticeEffectPlayer.Play(_view.SelfRootTran, progress, effect.ShowUp);
        }

        private void _SetProgress(float progoress)
        {
            if (_view == null)
                return;

            RectTransform txt_tran = _view._txt.rectTransform;
            RectTransform view_tran = _view.SelfRootTran;
            float txt_width = txt_tran.rect.width;
            float view_width = view_tran.rect.width;

            float width_total = txt_width + view_width;

            float pos_min = width_total * 0.5f;
            float pos_max = -width_total * 0.5f;

            float pos = Mathf.Lerp(pos_min, pos_max, progoress);

            txt_tran.anchoredPosition = new Vector2(pos, 0);
        }
    }
}
