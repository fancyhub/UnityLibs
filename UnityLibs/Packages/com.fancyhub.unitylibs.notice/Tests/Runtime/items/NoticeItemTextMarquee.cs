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
    public sealed class NoticeItemTextMarquee : CPoolItemBase, INoticeItem
    {
        private const string CPath = "Packages/com.fancyhub.unitylibs.notice/Tests/Runtime/Res/UINoticeMarquee.prefab";

        public string _Text;

        public Text _TxtComp;
        public RectTransform _view;
        public NoticeItemDummy _dummy;
        public CPtr<IResHolder> _ResHolder;

        public static NoticeItemTextMarquee Create(IResHolder resholder, string text)
        {
            NoticeItemTextMarquee ret = GPool.New<NoticeItemTextMarquee>();
            ret._Text = text;
            ret._ResHolder = new CPtr<IResHolder>(resholder);
            return ret;
        }

        protected override void OnPoolRelease()
        {
            NoticeFactory.ReleaseView(_ResHolder, ref _view);
            _dummy = default;
            _TxtComp = null;
        }
        public bool IsValid()
        {
            return _view != null;
        }

        public Vector2 GetViewSize()
        {
            return _view.rect.size;
        }

        public bool TryMerge(INoticeItem other)
        {
            return false;
        }

        public void Show(NoticeItemDummy dummy)
        {
            _dummy = dummy;

            _view = NoticeFactory.CreateView(_ResHolder, CPath, _dummy.Dummy);
            if (_view == null)
                return;
            var child = _view.Find("_txt");
            _TxtComp = child.GetComponent<Text>();
            _TxtComp.text = _Text;

            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_view);

            _SetProgress(0);
        }

        public void Update(float progress)
        {
            if (_TxtComp == null)
                return;
            _SetProgress(progress);
        }

        public void FadeOut(float progress, NoticeEffectConfig effect)
        {
            NoticeEffectPlayer.Play(_view, progress, effect.HideOut);
        }

        public void FadeIn(float progress, NoticeEffectConfig effect)
        {
            NoticeEffectPlayer.Play(_view, progress, effect.ShowUp);
        }

        private void _SetProgress(float progoress)
        {
            if (_TxtComp == null)
                return;

            RectTransform txt_tran = _TxtComp.rectTransform;
            RectTransform view_tran = _view;
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
