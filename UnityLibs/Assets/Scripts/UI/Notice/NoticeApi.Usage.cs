/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using FH;
namespace Game
{
    public sealed partial class NoticeApi
    {
        
        //跑马灯
        public static void ShowMarquee(string txt, float duration_sec = 5.0f)
        {
            if (_ == null)
                return;

            NoticeData data = new NoticeData(ENoticeChannel.Common, duration_sec);
            _.ShowNotice(data, NoticeItemTextMarquee.Create(_.ResHolder, txt));
        }

        public static void ShowCommon(string txt, float duration_sec = 2.0f)
        {
            if (_ == null)
                return;
            NoticeData data = new NoticeData(ENoticeChannel.Common, duration_sec);
            _.ShowNotice(data, NoticeItemText.Create(_.ResHolder, txt));
        }
    }
}
