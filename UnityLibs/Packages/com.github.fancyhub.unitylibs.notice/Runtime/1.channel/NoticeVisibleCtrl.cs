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

        public bool IsVisible(ENoticeVisible flag)
        {
            return _config.IsVisible(flag);
        }

        public bool NeedClear(ENoticeClearSignal signal)
        {
            return _config.ClearSignal[signal];
        }
    }
}
