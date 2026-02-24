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

namespace FH.NoticeSample
{
    public sealed class SampleNoticeItemMessageBox : CPoolItemBase, INoticeItem
    {
        public const float C_FADE_OUT_PERCENT = 0.8f;
        private const string CPath = "Packages/com.fancyhub.unitylibs.notice/Tests/Runtime/Res/UINoticeMessageBox.prefab";

        public string _Text;
        public string _BtnName;

        public RectTransform _view;
        public NoticeItemDummy _dummy;
        public CPtr<IResHolder> _ResHolder;

        public static SampleNoticeItemMessageBox Create(IResHolder resHolder, string text, string btn_name = null)
        {
            SampleNoticeItemMessageBox ret = GPool.New<SampleNoticeItemMessageBox>();
            ret._Text = text;
            ret._BtnName = btn_name;
            ret._ResHolder = new CPtr<IResHolder>(resHolder);

            if (string.IsNullOrEmpty(btn_name))
                ret._BtnName = "OK";

            return ret;
        }

        public Vector2 GetViewSize()
        {
            return _view.rect.size;
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

            _view = SampleNoticeFactory.CreateView(_ResHolder, CPath, dummy.Dummy);
            if (_view == null)
                return;

            _view.Find("_txt").GetComponent<Text>().text = _Text;
            var btn = _view.Find("_btn").GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(_OnBtnClick);
            btn.transform.Find("_txt_btn").GetComponent<Text>().text = _BtnName;

            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_view);
        }

        private void _OnBtnClick()
        {
            SampleNoticeFactory.ReleaseView(_ResHolder, ref _view);
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
            SampleNoticeFactory.ReleaseView(_ResHolder, ref _view);
            _dummy = default;
        }
    }
}
