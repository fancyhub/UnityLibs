/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;


namespace FH
{
    /// <summary>
    /// 消息的频道, 代表了位置
    /// </summary>
    public enum ENoticeChannel
    {
        None = 0,        
        Common,
        Max,
    }

    

    /// <summary>
    /// 每个频道自己管理自己的可见情况
    /// 这个枚举实际上是标记游戏的各个阶段的
    /// 
    /// 只有三个状态：场景中，loading，过场动画
    /// </summary>
    [Flags]
    public enum ENoticeVisibleFlag
    {
        None = 0,
        Loading = 1 << 0,
        Cutscene = 1 << 1,
        All = -1
    }

    /// <summary>
    /// 和上一个枚举不同，这个是纯配置用的，用来配置提示是使用那种控制显示和隐藏的类型
    /// </summary>
    [Flags]
    public enum ENoticeVisiblePatternFlag
    {
        None = 0,

        WhiteList = 1 << 0,
        BlackList = 1 << 1,

        All = -1,
    }

    /// <summary>
    /// 清除信号量
    /// 
    /// 一共有三个信号量
    /// 
    /// 是否手动退出游戏
    /// 是否切换了场景
    /// 过场动画是否播放结束
    /// 
    /// </summary>
    [Flags]
    public enum ENoticeClearSignal
    {
        None = 0,
        BackToLogin = 1 << 0,
        ChangeScene = 1 << 2,
        All = -1,
    }

    public enum ENoticeEffect
    {
        None,
        FadeIn,
        FadeOut,
        MoveUp,
        SlideFromRight,
        SlideToRight,
    }
}
