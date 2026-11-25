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
    public enum EMessageBoxResult
    {
        OK,
        Cancel,
    }

    public enum EMessageBoxButton
    {
        One,
        Two,
    }

    public struct MessageBoxData
    {
        public string Title;
        public string Message;

        public EMessageBoxButton Button;
        public string BtnNameOK;
        public string BtnNameCancel;

        public Action<EMessageBoxResult> CallBack;
    }

    public sealed class NoticeItemMessageBox : CPoolItemBase, INoticeItem
    {
        public const float C_FADE_OUT_PERCENT = 0.8f;

        private MessageBoxData _data;

        public NoticeItemDummy _dummy;
        public UINoticeMessageBoxView _view;
        public CPtr<IResHolder> _res_holder;

        public static NoticeItemMessageBox Create(
            IResHolder resHolder,
            string title,
            string message,
            Action<EMessageBoxResult> action = null,
            EMessageBoxButton button = EMessageBoxButton.One,
            string btn_name_ok = null,
            string btn_name_cancel = null)
        {
            return Create(resHolder, new MessageBoxData
            {
                Title = title,
                Message = message,

                Button = button,
                BtnNameOK = btn_name_ok,
                BtnNameCancel = btn_name_cancel,

                CallBack = action,
            });
        }

        public static NoticeItemMessageBox Create(
            IResHolder resHolder,
            MessageBoxData data)
        {
            NoticeItemMessageBox ret = GPool.New<NoticeItemMessageBox>();
            ret._data = data;
            ret._res_holder = new CPtr<IResHolder>(resHolder);
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
            _view._title.text = string.IsNullOrEmpty(_data.Title) ? string.Empty : _data.Title;
            _view._message.text = string.IsNullOrEmpty(_data.Message) ? string.Empty : _data.Message;

            _view._btnOK.ButtonName = string.IsNullOrEmpty(_data.BtnNameOK) ? "OK" : _data.BtnNameOK;
            _view._btnOK.OnClick = _OnBtnOKClick;

            if (_data.Button == EMessageBoxButton.Two)
            {
                _view._btnCancel.Active = true;
                _view._btnCancel.OnClick = _OnBtnCancelClick;
                _view._btnCancel.ButtonName = string.IsNullOrEmpty(_data.BtnNameCancel) ? "Cancel" : _data.BtnNameCancel;
            }
            else
            {
                _view._btnCancel.OnClick = null;
                _view._btnCancel.Active = false;
            }

        }

        private void _OnBtnOKClick()
        {
            _OnResult(EMessageBoxResult.OK);
        }

        private void _OnBtnCancelClick()
        {
            _OnResult(EMessageBoxResult.Cancel);
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
            _OnResult(EMessageBoxResult.Cancel);
        }

        private void _OnResult(EMessageBoxResult result)
        {
            var action = _data.CallBack;
            _view?.Destroy();
            _view = null;
            _dummy = default;
            _data = default;

            action?.Invoke(result);
        }
    }
}
