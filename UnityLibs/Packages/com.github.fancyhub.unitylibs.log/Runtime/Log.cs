/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/5/14 
 * Title   : 
 * Desc    : 
*************************************************************************************/

//在这里写控制宏定义
//#if UNITY_EDITOR
#define ENABLE_LOG_Debug
#define ENABLE_LOG_Info
#define ENABLE_LOG_Warning
#define ENABLE_LOG_Assert
//#endif

#define ENABLE_LOG_Error
#define ENABLE_LOG_Exception

namespace FH
{
    using System;
    using System.Diagnostics;
    using UnityEngine;
    using System.Runtime.CompilerServices;
    using ZString = System.String;


    internal static class LogConditional
    {
        private const string TRUE = "CSHARP_7_3_OR_NEWER"; //这个名字只要保证 编译的时候指定就行了
        private const string FALSE = "FALSE"; //随意写一些, 只要编译的时候不指定就行了

#if ENABLE_LOG_Debug
        public const string COND_DEBUG = TRUE;
#else
        public const string COND_DEBUG = FALSE;
#endif

#if ENABLE_LOG_Info
        public const string COND_INFO = TRUE;
#else
        public const string COND_INFO= FALSE;
#endif

#if ENABLE_LOG_Warning
        public const string COND_WARNING = TRUE;
#else
        public const string COND_WARNING= FALSE;
#endif

#if ENABLE_LOG_Assert
        public const string COND_ASSERT = TRUE;
#else
        public const string COND_ASSERT= FALSE;
#endif

#if ENABLE_LOG_Error
        public const string COND_ERROR = TRUE;
#else
        public const string COND_ERROR= FALSE;
#endif

#if ENABLE_LOG_Exception
        public const string COND_EXCEPTION = TRUE;
#else
        public const string COND_EXCEPTION= FALSE;
#endif
    }

    public static class Log
    {
        // allow mask, 对应的位,如果是true,就可以显示
        public static ELogMask AllowMask = ELogMask.All;

        public static ELogMask TraceMask = ELogMask.Warning | ELogMask.Assert | ELogMask.Error | ELogMask.Exception;

        public static ELogMask UnityMask = ELogMask.All;

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D(string format, params object[] args)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print(ELogLvl.Debug, null, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D<T0>(string format, T0 arg0)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print1(ELogLvl.Debug, null, AllowMask, TraceMask, UnityMask, null, format, arg0);
#endif
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D<T0, T1>(string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print2(ELogLvl.Debug, null, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1);
#endif
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print3(ELogLvl.Debug, null, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
#endif
        }

        /// <summary>
        /// Debug Info
        /// </summary>        
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print(ELogLvl.Debug, null, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }

        /// <summary>
        /// Info
        /// </summary>
        [Conditional(LogConditional.COND_INFO)]
        [HideInCallstack]
        public static void I(string format, params object[] args)
        {
#if ENABLE_LOG_Info
            LogPrinter.Print(ELogLvl.Info, null, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }

        /// <summary>
        /// Info
        /// </summary>
        [Conditional(LogConditional.COND_INFO)]
        [HideInCallstack]
        public static void I(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Info
            LogPrinter.Print(ELogLvl.Info, null, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }

        /// <summary>
        /// warning
        /// </summary>
        [Conditional(LogConditional.COND_WARNING)]
        [HideInCallstack]
        public static void W(string format, params object[] args)
        {
#if ENABLE_LOG_Warning
            LogPrinter.Print(ELogLvl.Warning, null, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }


        /// <summary>
        /// warning
        /// </summary>
        [Conditional(LogConditional.COND_WARNING)]
        [HideInCallstack]
        public static void W(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Warning
            LogPrinter.Print(ELogLvl.Warning, null, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }


        /// <summary>
        /// assert
        /// </summary>
        [Conditional(LogConditional.COND_ASSERT)]
        [HideInCallstack]
        public static void Assert(bool cond, string format = null, params object[] args)
        {
#if ENABLE_LOG_Assert            
            if (cond)
                return;

            if (format == null)
            {
                format = "Asset Error";
                args = null;
            }
            LogPrinter.Print(ELogLvl.Assert, null, AllowMask, TraceMask, UnityMask, null, format, args);

#endif
        }

        /// <summary>
        /// assert
        /// </summary>
        [Conditional(LogConditional.COND_ASSERT)]
        [HideInCallstack]
        public static void Assert(UnityEngine.Object content, bool cond, string format = null, params object[] args)
        {
#if ENABLE_LOG_Assert
            if (cond)
                return;

            if (format == null)
            {
                format = "Asset Error";
                args = null;
            }
            LogPrinter.Print(ELogLvl.Assert, null, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }

        [Conditional(LogConditional.COND_EXCEPTION)]
        [HideInCallstack]
        public static void E(Exception e)
        {
#if ENABLE_LOG_Exception
            LogPrinter.PrintE(AllowMask, UnityMask, e);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(string format, params object[] args)
        {
#if ENABLE_LOG_Error          
            LogPrinter.Print(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0>(string format, T0 arg0)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print1(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, null, format, arg0);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0>(UnityEngine.Object content, string format, T0 arg0)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print1(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, content, format, arg0);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1>(string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print2(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1>(UnityEngine.Object content, string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print2(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, content, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print3(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1, T2>(UnityEngine.Object content, string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print3(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, content, format, arg0, arg1, arg2);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void ErrCode<T>(T code, string format = null, params object[] args) where T : Enum
        {
#if ENABLE_LOG_Error
            if (code.GetHashCode() == 0)
                return;

            if (format == null)
                format = string.Empty;

            LogPrinter.Print(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void ErrCode<T, T0>(T code, string format, T0 arg0) where T : Enum
        {
#if ENABLE_LOG_Error
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print1(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0);
#endif
        }

        [HideInCallstack]
        public static void ErrCode<T, T0, T1>(T code, string format, T0 arg0, T1 arg1) where T : Enum
        {
#if ENABLE_LOG_Error
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print2(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0, arg1);
#endif
        }


        [HideInCallstack]
        public static void ErrCode<T, T0, T1, T2>(T code, string format, T0 arg0, T1 arg1, T2 arg2) where T : Enum
        {
#if ENABLE_LOG_Error
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print3(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0, arg1, arg2);
#endif
        }

    }

    public struct TagLogger
    {
        public readonly string Tag;
        // allow mask, 对应的位,如果是true,就可以显示
        public ELogMask AllowMask;
        public ELogMask UnityMask;
        public ELogMask TraceMask;

        public TagLogger(string tag, ELogMask allow_mask)
        {
            Tag = tag;
            AllowMask = allow_mask;
            UnityMask = allow_mask;
            TraceMask = allow_mask;
        }

        public static TagLogger Create(string tag_name, ELogLvl log_lvl = ELogLvl.Info)
        {
            uint m = 1U << (int)log_lvl;
            ELogMask mask = (ELogMask)(~(m - 1));
            return new TagLogger(tag_name, mask);
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public void D(string format, params object[] args)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print(ELogLvl.Debug, Tag, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public void D<T0>(string format, T0 arg0)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print1(ELogLvl.Debug, Tag, AllowMask, TraceMask, UnityMask, null, format, arg0);
#endif
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public void D<T0, T1>(string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print2(ELogLvl.Debug, Tag, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1);

#endif
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public void D<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print3(ELogLvl.Debug, Tag, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
#endif
        }

        /// <summary>
        /// Debug Info
        /// </summary>        
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public void D(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print(ELogLvl.Debug, Tag, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }

        /// <summary>
        /// Info
        /// </summary>
        [Conditional(LogConditional.COND_INFO)]
        [HideInCallstack]
        public void I(string format, params object[] args)
        {
#if ENABLE_LOG_Info
            LogPrinter.Print(ELogLvl.Info, Tag, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }

        /// <summary>
        /// Info
        /// </summary>
        [Conditional(LogConditional.COND_INFO)]
        [HideInCallstack]
        public void I(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Info
            LogPrinter.Print(ELogLvl.Info, Tag, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }

        /// <summary>
        /// warning
        /// </summary>
        [Conditional(LogConditional.COND_WARNING)]
        [HideInCallstack]
        public void W(string format, params object[] args)
        {
#if ENABLE_LOG_Warning
            LogPrinter.Print(ELogLvl.Warning, Tag, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }


        /// <summary>
        /// warning
        /// </summary>
        [Conditional(LogConditional.COND_WARNING)]
        [HideInCallstack]
        public void W(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Warning
            LogPrinter.Print(ELogLvl.Warning, Tag, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }


        /// <summary>
        /// assert
        /// </summary>
        [Conditional(LogConditional.COND_ASSERT)]
        [HideInCallstack]
        public void Assert(bool cond, string format = null, params object[] args)
        {
#if ENABLE_LOG_Assert
            if (cond) return;
            if (format == null)
            {
                format = "Asset Error";
                args = null;
            }

            LogPrinter.Print(ELogLvl.Assert, Tag, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }

        /// <summary>
        /// assert
        /// </summary>
        [Conditional(LogConditional.COND_ASSERT)]
        [HideInCallstack]
        public void Assert(UnityEngine.Object content, bool cond, string format = null, params object[] args)
        {
#if ENABLE_LOG_Assert
            if (cond) return;
            if (format == null)
            {
                format = "Asset Error";
                args = null;
            }
            LogPrinter.Print(ELogLvl.Assert, Tag, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }

        [Conditional(LogConditional.COND_EXCEPTION)]
        [HideInCallstack]
        public void E(Exception e)
        {
#if ENABLE_LOG_Exception
            LogPrinter.PrintE(AllowMask, UnityMask, e);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E(string format, params object[] args)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T0>(string format, T0 arg0)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print1(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, format, arg0);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T0>(UnityEngine.Object content, string format, T0 arg0)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print1(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, content, format, arg0);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T0, T1>(string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print2(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T0, T1>(UnityEngine.Object content, string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print2(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, content, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print3(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T0, T1, T2>(UnityEngine.Object content, string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print3(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, content, format, arg0, arg1, arg2);
#endif
        }


        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void ErrCode<T>(T code, string format = null, params object[] args) where T : Enum
        {
#if ENABLE_LOG_Error
            if (code.GetHashCode() == 0)
                return;
            if (format == null)
                format = string.Empty;

            LogPrinter.Print(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void ErrCode<T, T0>(T code, string format, T0 arg0) where T : Enum
        {
#if ENABLE_LOG_Error
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print1(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0);
#endif
        }

        [HideInCallstack]
        public void ErrCode<T, T0, T1>(T code, string format, T0 arg0, T1 arg1) where T : Enum
        {
#if ENABLE_LOG_Error
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print2(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0, arg1);
#endif
        }


        [HideInCallstack]
        public void ErrCode<T, T0, T1, T2>(T code, string format, T0 arg0, T1 arg1, T2 arg2) where T : Enum
        {
#if ENABLE_LOG_Error
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print3(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0, arg1, arg2);
#endif
        }
    }

    public abstract class TagLoggerT<TSub> where TSub : new()
    {
        public static string Tag = nameof(TSub);
        public static ELogMask AllowMask = ELogMask.Info | ELogMask.Warning | ELogMask.Assert | ELogMask.Error | ELogMask.Exception;
        public static ELogMask UnityMask = AllowMask;
        public static ELogMask TraceMask = AllowMask;
        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D(string format, params object[] args)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print(ELogLvl.Debug, Tag, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D<T0>(string format, T0 arg0)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print1(ELogLvl.Debug, Tag, AllowMask, TraceMask, UnityMask, null, format, arg0);
#endif
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D<T0, T1>(string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print2(ELogLvl.Debug, Tag, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1);

#endif
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print3(ELogLvl.Debug, Tag, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
#endif
        }

        /// <summary>
        /// Debug Info
        /// </summary>        
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Debug
            LogPrinter.Print(ELogLvl.Debug, Tag, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }

        /// <summary>
        /// Info
        /// </summary>
        [Conditional(LogConditional.COND_INFO)]
        [HideInCallstack]
        public static void I(string format, params object[] args)
        {
#if ENABLE_LOG_Info
            LogPrinter.Print(ELogLvl.Info, Tag, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }

        /// <summary>
        /// Info
        /// </summary>
        [Conditional(LogConditional.COND_INFO)]
        [HideInCallstack]
        public static void I(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Info
            LogPrinter.Print(ELogLvl.Info, Tag, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }

        /// <summary>
        /// warning
        /// </summary>
        [Conditional(LogConditional.COND_WARNING)]
        [HideInCallstack]
        public static void W(string format, params object[] args)
        {
#if ENABLE_LOG_Warning
            LogPrinter.Print(ELogLvl.Warning, Tag, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }


        /// <summary>
        /// warning
        /// </summary>
        [Conditional(LogConditional.COND_WARNING)]
        [HideInCallstack]
        public static void W(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Warning
            LogPrinter.Print(ELogLvl.Warning, Tag, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }


        /// <summary>
        /// assert
        /// </summary>
        [Conditional(LogConditional.COND_ASSERT)]
        [HideInCallstack]
        public static void Assert(bool cond, string format = null, params object[] args)
        {
#if ENABLE_LOG_Assert
            if (cond) return;
            if (format == null)
            {
                format = "Asset Error";
                args = null;
            }

            LogPrinter.Print(ELogLvl.Assert, Tag, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }

        /// <summary>
        /// assert
        /// </summary>
        [Conditional(LogConditional.COND_ASSERT)]
        [HideInCallstack]
        public static void Assert(UnityEngine.Object content, bool cond, string format = null, params object[] args)
        {
#if ENABLE_LOG_Assert
            if (cond) return;
            if (format == null)
            {
                format = "Asset Error";
                args = null;
            }
            LogPrinter.Print(ELogLvl.Assert, Tag, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }

        [Conditional(LogConditional.COND_EXCEPTION)]
        [HideInCallstack]
        public static void E(Exception e)
        {
#if ENABLE_LOG_Exception
            LogPrinter.PrintE(AllowMask, UnityMask, e);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(string format, params object[] args)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, format, args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, content, format, args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0>(string format, T0 arg0)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print1(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, format, arg0);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0>(UnityEngine.Object content, string format, T0 arg0)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print1(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, content, format, arg0);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1>(string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print2(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1>(UnityEngine.Object content, string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print2(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, content, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print3(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1, T2>(UnityEngine.Object content, string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print3(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, content, format, arg0, arg1, arg2);
#endif
        }



        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void ErrCode<T>(T code, string format = null, params object[] args) where T : Enum
        {
#if ENABLE_LOG_Error
            if (code.GetHashCode() == 0)
                return;
            if (format == null)
                format = string.Empty;

            LogPrinter.Print(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void ErrCode<T, T0>(T code, string format, T0 arg0) where T : Enum
        {
#if ENABLE_LOG_Error
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print1(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0);
#endif
        }

        [HideInCallstack]
        public void ErrCode<T, T0, T1>(T code, string format, T0 arg0, T1 arg1) where T : Enum
        {
#if ENABLE_LOG_Error
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print2(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0, arg1);
#endif
        }


        [HideInCallstack]
        public void ErrCode<T, T0, T1, T2>(T code, string format, T0 arg0, T1 arg1, T2 arg2) where T : Enum
        {
#if ENABLE_LOG_Error
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print3(ELogLvl.Error, Tag, AllowMask, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0, arg1, arg2);
#endif
        }
    }
}
