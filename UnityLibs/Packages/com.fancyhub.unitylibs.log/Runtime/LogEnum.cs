/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/5/14 
 * Title   : 
 * Desc    : 
*************************************************************************************/

//在这里写控制宏定义
#if UNITY_EDITOR
#define ENABLE_LOG_Debug
#define ENABLE_LOG_Info
#define ENABLE_LOG_Warning
#define ENABLE_LOG_Assert
#endif

#define ENABLE_LOG_Error
#define ENABLE_LOG_Exception

namespace FH
{
    using System;
     
    public enum ELogLvl
    {
        Debug,
        Info,
        Warning,
        Assert,
        Error,
        Exception,
        Off,
    }

    [Flags]
    public enum ELogMask : byte
    {
        None = 0,
        Debug = 1 << (int)ELogLvl.Debug,
        Info = 1 << (int)ELogLvl.Info,
        Warning = 1 << (int)ELogLvl.Warning,
        Assert = 1 << (int)ELogLvl.Assert,
        Error = 1 << (int)ELogLvl.Error,
        Exception = 1 << (int)ELogLvl.Exception,
        All = byte.MaxValue,
    }      
}
