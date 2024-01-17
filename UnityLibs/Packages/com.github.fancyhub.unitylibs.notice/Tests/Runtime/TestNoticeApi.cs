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
            StartCoroutine(_ShowNotice());
        }

        private IEnumerator _ShowNotice()
        {
            List<NoticeData> list = new List<NoticeData>(_testData._count);

            for (int i = 0; i < _testData._count; i++)
            {
                NoticeData data = new NoticeData()
                {
                    _channel = _testData._channel,
                    _duration_expire = -1,
                    _duration_show = (int)(_testData._duration * 1000),
                    _priority = _testData._priority,
                    _item = _create_item(_testData._text + " " + i.ToString())
                };
                list.Add(data);
            }

            float time = Math.Max(1.0f, _testData._interval);

            foreach (var p in list)
            {
                NoticeApi.ShowNotice(p);

                yield return new WaitForSeconds(time);
            }
        }

        public INoticeItem _create_item(string txt)
        {
            switch (_testData._view_type)
            {
                case ENoticeViewType.text:
                    return new NoticeItemText(txt);
                case ENoticeViewType.marquee:
                    return new NoticeItemTextMarquee(txt);
                default:
                    Debug.LogErrorFormat("未实现格式 {0}", _testData._view_type);
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
