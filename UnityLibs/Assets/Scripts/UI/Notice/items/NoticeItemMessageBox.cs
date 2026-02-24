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
using FH.UI;

namespace Game
{
    public sealed class NoticeItemMessageBox : CPoolItemBase, INoticeItem
    {
        public const float C_FADE_OUT_PERCENT = 0.8f;

        public string _Text;
        public string _BtnName;

        public NoticeItemDummy _dummy;
        public UINoticeMessageBoxView _view;
        public CPtr<IResHolder> _res_holder;

        public static NoticeItemMessageBox Create(IResHolder resHolder, string text, string btn_name = null)
        {
            NoticeItemMessageBox ret = GPool.New<NoticeItemMessageBox>();
            ret._Text = text;
            ret._BtnName = btn_name;
            ret._res_holder = new CPtr<IResHolder>(resHolder);

            if (string.IsNullOrEmpty(btn_name))
                ret._BtnName = "OK";

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

            _view = FH.UI.UIBaseView.CreateView<UINoticeMessageBoxView>(_dummy.Dummy, _res_holder.Val);
            if (_view == null)
                return;
            _view._txt.text = _Text;
            _view._btn.onClick.RemoveAllListeners();
            _view._btn.onClick.AddListener(_OnBtnClick);
            _view._txt_btn.text = _BtnName;            
        }

        private void _OnBtnClick()
        {
            _view?.Destroy();
            _view = null;
            _dummy = default;
        }

        public void FadeOut(float progress, NoticeEffectConfig effect)
        {
            //NoticeEffectPlayer.Play(_view, progress, effect.HideOut);
        }


        public void FadeIn(float progress, NoticeEffectConfig effect)
        {
            //NoticeEffectPlayer.Play(_view, progress, effect.ShowUp);
        }

        public void Update(float progress)
        {
        }

        protected override void OnPoolRelease()
        {
            _view?.Destroy();
            _view = null;
            _dummy = default;
        }
    }
}
