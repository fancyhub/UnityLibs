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

namespace FH.NoticeSample
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
                    NoticeApi.ShowNotice(data, _CreateItem(view_type, txt + " " + i.ToString()));
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    NoticeApi.ShowNotice(data, _CreateItem(view_type, txt + " " + i.ToString()));
                    yield return new WaitForSeconds(time);
                }
            }
        }

        public static INoticeItem _CreateItem(ENoticeViewType view_type, string txt)
        {
            switch (view_type)
            {
                case ENoticeViewType.text:
                    return NoticeItemText.Create(txt);
                case ENoticeViewType.marquee:
                    return NoticeItemTextMarquee.Create(txt);
                case ENoticeViewType.message_box:
                    return NoticeItemMessageBox.Create(txt);

                default:
                    Debug.LogErrorFormat("未实现格式 {0}", view_type);
                    return null;
            }
        }
    }

    [CustomEditor(typeof(TestNoticeApi))]
    public class NoticeApiTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Show"))
            {
                ((TestNoticeApi)target).Show();
            }
        }
    }
}
#endif
