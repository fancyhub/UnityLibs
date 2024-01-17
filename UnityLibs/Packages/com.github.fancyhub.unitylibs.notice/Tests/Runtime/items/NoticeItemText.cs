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
        private const string CPath = "Packages/com.github.fancyhub.unitylibs.notice/Tests/Runtime/Res/UINoticeText.prefab";

        public string _Text;

        public Text _TxtComp;
        public RectTransform _view;
        public NoticeItemDummy _dummy;

        public static NoticeItemText Create(string text)
        {
            NoticeItemText ret = GPool.New<NoticeItemText>();
            ret._Text = text;
            return ret;
        }

        public Vector2 GetViewSize()
        {
            return _view.rect.size;
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

        public void CreateView(NoticeItemDummy dummy)
        {
            _dummy = dummy;

            GameObject obj = dummy.CreateView(CPath);

            _view = obj.GetComponent<RectTransform>();
            var child = _view.Find("_txt");
            _TxtComp = child.GetComponent<Text>();
            _TxtComp.text = _Text;
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_view);
        }

        public void HideOut(NoticeItemTime time, List<NoticeEffectItemConfig> effect)
        {
            NoticeEffectPlayer.Play(_view, time, effect);
        }


        public void ShowUp(NoticeItemTime time, List<NoticeEffectItemConfig> effect)
        {
            NoticeEffectPlayer.Play(_view, time, effect);
        }

        public void Update(NoticeItemTime time)
        {
        }

        protected override void OnPoolRelease()
        {
            _dummy.ReleaseView(ref _view);
            _dummy = default;
            _TxtComp = null;
        }
    }
}
