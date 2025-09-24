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
    public sealed class NoticeItemText : CPoolItemBase, INoticeItem
    {
        public const float C_FADE_OUT_PERCENT = 0.8f;
        private const string CPath = "Packages/com.fancyhub.unitylibs.notice/Tests/Runtime/Res/UINoticeText.prefab";

        public string _Text;

        public RectTransform _view;
        public NoticeItemDummy _dummy;
        public CPtr<IResHolder> _ResHolder;

        public static NoticeItemText Create(IResHolder resHolder, string text)
        {
            NoticeItemText ret = GPool.New<NoticeItemText>();
            ret._Text = text;
            ret._ResHolder = new CPtr<IResHolder>(resHolder);
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

            _view = NoticeFactory.CreateView(_ResHolder, CPath, dummy.Dummy);
            if (_view == null)
                return;
            _view.Find("_txt").GetComponent<Text>().text = _Text;

            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_view);
        }

        public void FadeIn(float progress, NoticeEffectConfig effect)
        {
            NoticeEffectPlayer.Play(_view, progress, effect.ShowUp);
        }

        public void FadeOut(float progress, NoticeEffectConfig effect)
        {
            NoticeEffectPlayer.Play(_view, progress, effect.HideOut);
        }


        public void Update(float progress)
        {
        }

        protected override void OnPoolRelease()
        {
            NoticeFactory.ReleaseView(_ResHolder, ref _view);
            _dummy = default;
        }
    }
}
