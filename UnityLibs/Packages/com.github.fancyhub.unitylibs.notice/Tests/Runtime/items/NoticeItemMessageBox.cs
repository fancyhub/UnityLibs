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
    public sealed class NoticeItemMessageBox : CPoolItemBase, INoticeItem
    {
        public const float C_FADE_OUT_PERCENT = 0.8f;
        private const string CPath = "Packages/com.github.fancyhub.unitylibs.notice/Tests/Runtime/Res/UINoticeMessageBox.prefab";

        public string _Text;
        public string _BtnName;

        public RectTransform _view;
        public NoticeItemDummy _dummy;

        public static NoticeItemMessageBox Create(string text, string btn_name = null)
        {
            NoticeItemMessageBox ret = GPool.New<NoticeItemMessageBox>();
            ret._Text = text;
            ret._BtnName = btn_name;

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

        public void CreateView(NoticeItemDummy dummy)
        {
            _dummy = dummy;

            GameObject obj = dummy.CreateView(CPath);

            _view = obj.GetComponent<RectTransform>();

            _view.Find("_txt").GetComponent<Text>().text = _Text;
            var btn = _view.Find("_btn").GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(_OnBtnClick);
            btn.transform.Find("_txt_btn").GetComponent<Text>().text = _BtnName;

            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_view);
        }

        private void _OnBtnClick()
        {
            _dummy.ReleaseView(ref _view);
            _dummy = default;
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
        }
    }
}
