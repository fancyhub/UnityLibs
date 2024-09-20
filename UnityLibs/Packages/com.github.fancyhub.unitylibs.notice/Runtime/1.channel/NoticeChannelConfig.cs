/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;

namespace FH
{
    /// <summary>
    /// 控制提示显示和隐藏所使用的配置，因为显隐情况已经清除情况需要的数据比较多，所以抽出来封装成一个新类
    /// </summary>
    [System.Serializable]
    public sealed class NoticeVisibleCtrlConfig
    {
        // 获取显隐的控制情况
        public ENoticeVisiblePatternFlag VisiblePattern;

        // 获得显隐条件的配置
        public BitEnum32<ENoticeVisible> WhiteList;

        // 获取不可见的标记
        public BitEnum32<ENoticeVisible> BlackList;

        // 获取清除的标记
        public BitEnum32<ENoticeClearSignal> ClearSignal;


        public bool IsVisible(ENoticeVisible flag)
        {
            //白名单
            bool visible = _is_white_list_visible(flag);
            if (!visible)
                return false;

            //黑名单
            visible = _is_black_list_visible(flag);
            if (!visible)
                return false;

            return true;
        }

        private bool _is_white_list_visible(ENoticeVisible flag)
        {
            ENoticeVisiblePatternFlag conf_pattern = VisiblePattern;
            bool pattern_exist = (conf_pattern & ENoticeVisiblePatternFlag.WhiteList) != ENoticeVisiblePatternFlag.None;
            if (!pattern_exist)
                return true;

            return WhiteList[flag];

        }

        private bool _is_black_list_visible(ENoticeVisible flag)
        {
            ENoticeVisiblePatternFlag conf_pattern = VisiblePattern;
            bool pattern_exist = (conf_pattern & ENoticeVisiblePatternFlag.BlackList) != ENoticeVisiblePatternFlag.None;
            if (!pattern_exist)
                return true;

            return !BlackList[flag];
        }
    }

    [Serializable]
    public class NoticeTimeScaleConfig
    {
        [Header("堆叠的数量小于改值的时候,时间缩放=1")]
        public int PendingCountMin = 5;

        public int PendingCount = 10;
        public float TimeScale = 2;

        public float CalcScale(int count)
        {
            if (PendingCountMin <= 0 || count <= PendingCountMin)
                return 1;

            if (PendingCount <= PendingCountMin)
                return 1;

            float p = Mathf.InverseLerp(PendingCountMin, PendingCount, count);
            return Mathf.Lerp(1, TimeScale, p);
        }
    }

    [Serializable]
    public sealed class NoticeChannelConfig
    {
        public NoticeVisibleCtrlConfig Visible;
        public NoticeTimeScaleConfig TimeScale;
    }
}
