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
        private const string CPath = "Packages/com.github.fancyhub.unitylibs.notice/Tests/Runtime/Res/UINoticeMarquee.prefab";

        public string _Text;

        public Text _TxtComp;
        public RectTransform _view;
        public NoticeItemDummy _dummy;

        public static NoticeItemTextMarquee Create(string text)
        {
            NoticeItemTextMarquee ret = GPool.New<NoticeItemTextMarquee>();
            ret._Text = text;
            return ret;
        }

        protected override void OnPoolRelease()
        {
            _dummy.ReleaseView(ref _view);
            _dummy = default;
            _TxtComp = null;
        }

        public Vector2 GetViewSize()
        {
            return _view.rect.size;
        }

        public bool TryMerge(INoticeItem other)
        {
            return false;
        }

        public void CreateView(NoticeItemDummy dummy)
        {
            _dummy = dummy;

            GameObject obj = dummy.CreateView(CPath);

            _view = obj.GetComponent<RectTransform>();
            var child = _view.Find("_txt");
            _TxtComp = child.GetComponent<Text>();
            _TxtComp.text = _Text;

            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_view);

            _SetProgress(0);
        }

        public void Update(NoticeItemTime time)
        {
            _SetProgress(time.GetCurPhaseProgress());
        }

        public void HideOut(NoticeItemTime time, List<NoticeEffectItemConfig> effect)
        {
            NoticeEffectPlayer.Play(_view, time, effect);
        }


        public void ShowUp(NoticeItemTime time, List<NoticeEffectItemConfig> effect)
        {
            NoticeEffectPlayer.Play(_view, time, effect);
        }

        private void _SetProgress(float progoress)
        {
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
