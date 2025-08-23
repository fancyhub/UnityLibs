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
    public enum ENoticeVisible
    {
        None = 0,
        Loading,
        Cutscene,
    }

    /// <summary>
    /// 和上一个枚举不同，这个是纯配置用的，用来配置提示是使用那种控制显示和隐藏的类型
    /// </summary>
    [Flags]
    public enum ENoticeVisiblePatternFlag
    {
        None = 0,

        WhiteList , 
        BlackList ,

        All = -1,
    }

    /// <summary>
    /// 清除信号量        
    /// 
    /// </summary>    
    public enum ENoticeClearSignal
    {
        BackToLogin,
        ChangeScene,
    }
}
