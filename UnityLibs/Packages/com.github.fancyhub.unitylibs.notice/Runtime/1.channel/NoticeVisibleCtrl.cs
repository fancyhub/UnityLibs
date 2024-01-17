/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Collections.Generic;


namespace FH
{
    /// <summary>
    /// 可见的标记位控制
    /// 
    /// 这里原本像写成模板函数，但是值类型的模板函数不能用 and 运算符
    /// </summary>
    public sealed class NoticeVisibleCtrl
    {
        public NoticeVisibleCtrlConfig _config;
        public NoticeVisibleCtrl(NoticeVisibleCtrlConfig config)
        {
            _config = config;
        }

        public bool IsVisible(ENoticeVisibleFlag flag)
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

        public bool NeedClear(ENoticeClearSignal signal)
        {
            bool clear = (signal & _config.ClearSignal) != ENoticeClearSignal.None;
            return clear;
        }

        public ENoticeClearSignal GetClearSignal()
        {
            return _config.ClearSignal;
        }

        public bool _is_white_list_visible(ENoticeVisibleFlag tar_flag)
        {
            ENoticeVisiblePatternFlag conf_pattern = _config.VisiblePattern;
            bool pattern_exist = (conf_pattern & ENoticeVisiblePatternFlag.WhiteList) != ENoticeVisiblePatternFlag.None;
            if (!pattern_exist)
                return true;

            bool visible = (_config.VisibleFlag & tar_flag) != ENoticeVisibleFlag.None;
            return visible;
        }

        public bool _is_black_list_visible(ENoticeVisibleFlag tar_flag)
        {
            ENoticeVisiblePatternFlag conf_pattern = _config.VisiblePattern;
            bool pattern_exist = (conf_pattern & ENoticeVisiblePatternFlag.BlackList) != ENoticeVisiblePatternFlag.None;
            if (!pattern_exist)
                return true;

            bool visible = (_config.VisibleFlag & tar_flag) == ENoticeVisibleFlag.None;
            return visible;
        }
    }
}
