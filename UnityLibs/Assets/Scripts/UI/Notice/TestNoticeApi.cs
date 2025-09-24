/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23 
 * Title   : 
 * Desc    : 
*************************************************************************************/
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FH;

namespace Game
{
    public class TestNoticeApi : MonoBehaviour
    {
        public enum ENoticeViewType
        {
            text,
            marquee, //走马灯
            message_box,
        }


        [Serializable]
        public class TestConfig
        {
            public ENoticeChannel _channel = ENoticeChannel.Common;
            public ENoticeViewType _view_type = ENoticeViewType.text;
            public int _priority = 0;
            public float _duration = 2.0f;
            public string _img_name = "Props_icon_1";
            public string _text = "Hello World";
            public int _count = 10;
            public float _interval = 0;
        }

        [SerializeField] public TestConfig _testData = new TestConfig();

        [FH.Omi.Button]
        public void Show()
        {
            if (!Application.isPlaying)
                return;

            StartCoroutine(_ShowNotice());
        }

        private IEnumerator _ShowNotice()
        {
            string txt = _testData._text;
            float time = _testData._interval;
            int count = _testData._count;
            var view_type = _testData._view_type;
            NoticeData data = new NoticeData(_testData._channel, _testData._duration, _testData._priority);

            if (time <= 0)
            {
                for (int i = 0; i < count; i++)
                {
                    _Show(data, view_type, txt + " " + i.ToString());
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    _Show(data, view_type, txt + " " + i.ToString());
                    yield return new WaitForSeconds(time);
                }
            }
        }

        public static void _Show(NoticeData data, ENoticeViewType view_type, string txt)
        {
            var notice_api = NoticeApi.Inst;
            if (notice_api == null)
                return;

            switch (view_type)
            {
                case ENoticeViewType.text:
                    notice_api.ShowNotice(data, NoticeItemText.Create(notice_api.ResHolder, txt));
                    break;
                case ENoticeViewType.marquee:
                    notice_api.ShowNotice(data, NoticeItemTextMarquee.Create(notice_api.ResHolder, txt));
                    break;
                case ENoticeViewType.message_box:
                    notice_api.ShowNotice(data, NoticeItemMessageBox.Create(notice_api.ResHolder, txt));
                    break;

                default:
                    Debug.LogErrorFormat("未实现格式 {0}", view_type);
                    break;
            }
        }
    }
}
#endif
