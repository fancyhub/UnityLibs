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
    public class NoticeItemText : INoticeItem
    {
        public const float C_FADE_OUT_PERCENT = 0.8f;        
        private const string CPath = "Packages/com.github.fancyhub.unitylibs.notice/Tests/Runtime/Res/UINoticeText.prefab";
        
        public string _Text;

        public Text _TxtComp;
        public RectTransform _view;
        public NoticeItemDummy _dummy;

        public NoticeItemText(string text)
        {
            _Text = text;
        }

        public Vector2 GetSize()
        {
            return _view.rect.size;
        }

        public void Destroy()
        {
            _dummy?.ReleaseView(ref _view);
            _dummy = null;
        }

        public bool Merge(INoticeItem other)
        {
            NoticeItemText st = other as NoticeItemText;
            if (st == null)
                return false;
            if (st._Text == _Text)
                return true;
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
    }
}
