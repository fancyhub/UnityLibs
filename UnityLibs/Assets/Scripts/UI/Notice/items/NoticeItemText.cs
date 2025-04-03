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
    public sealed class NoticeItemText : CPoolItemBase, INoticeItem
    {
        public const float C_FADE_OUT_PERCENT = 0.8f;
        public string _Text;
        public UINoticeTextView _view;
        public NoticeItemDummy _dummy;
        public CPtr<IResInstHolder> _res_holder;

        public static NoticeItemText Create(IResInstHolder resHolder, string text)
        {
            NoticeItemText ret = GPool.New<NoticeItemText>();
            ret._Text = text;
            ret._res_holder = new CPtr<IResInstHolder>(resHolder);
            return ret;
        }

        public Vector2 GetViewSize()
        {
            if (_view == null)
                return Vector2.one;
            return _view.SelfRootTran.rect.size;
        }

        public bool IsValid()
        {
            return _view != null;
        }

        public bool TryMerge(INoticeItem other)
        {
            //NoticeItemText st = other as NoticeItemText;
            //if (st == null)
            //    return false;
            //if (st._Text == _Text)
            //    return true;
            return false;
        }

        public void Show(NoticeItemDummy dummy)
        {
            _dummy = dummy;

            _view = FH.UI.UIBaseView.CreateView<UINoticeTextView>(_dummy.Dummy, _res_holder.Val);
            if (_view == null)
                return;
            _view._txt.text = _Text;
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_view.SelfRootTran);
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



        public void Update(float progress)
        {
        }

        protected override void OnPoolRelease()
        {
            _dummy = default;
            _view?.Destroy();
            _view = null;
        }
    }
}
