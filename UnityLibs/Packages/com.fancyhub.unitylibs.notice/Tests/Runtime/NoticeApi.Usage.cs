/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.NoticeSample
{
    public static partial class NoticeApi
    {          
        public static void ShowNotice(NoticeData data, INoticeItem item)
        {
            if (item == null)
                return;
            if (_Mgr == null)
            {
                item.Destroy();
                return;
            }
            _Mgr.ShowNotice(data, item);
        }

        //跑马灯
        public static void ShowMarquee(string txt, float duration_sec = 5.0f)
        {
            NoticeData data = new NoticeData(ENoticeChannel.Common, duration_sec);
            ShowNotice(data, NoticeItemTextMarquee.Create(_ResHolder, txt));
        }

        public static void ShowCommon(string txt, float duration_sec = 2.0f)
        {
            NoticeData data = new NoticeData(ENoticeChannel.Common, duration_sec);
            ShowNotice(data, NoticeItemText.Create(_ResHolder, txt));
        }         
    }
}
