/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/5/14 
 * Title   : 
 * Desc    : 
*************************************************************************************/

//在这里写控制宏定义
#if UNITY_EDITOR
#define ENABLE_LOG_Debug

#define ENABLE_LOG_Warning 
#define ENABLE_LOG_Assert
#endif

#define ENABLE_LOG_Info

#define ENABLE_LOG_Error
#define ENABLE_LOG_Exception

//using ZString = Cysharp.Text.ZString; //https://github.com/Cysharp/ZString
using ZString = System.String;

namespace FH
{
    using System;
    using System.Diagnostics;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal static class LogConditional
    {
        private const string TRUE = "UNITY_5_3_OR_NEWER"; //这个名字只要保证 编译的时候指定就行了
        private const string FALSE = "__FALSE"; //随意写一些, 只要编译的时候不指定就行了

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

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        [UnityEditor.InitializeOnLoadMethod]
#endif
        public static void AutoInit()
        {
            //LogRecorder_Udp recorder_udp= new LogRecorder_Udp(LogRecorder_Udp.CreateRemoteIP("", 4));
            LogRecorder_File recorder_file = new LogRecorder_File(LogRecorder_File.GetLogFileDirPath());
            LogRecorderMgr.Init(new ILogRecorder[] { recorder_file });
        }

        public static void Init(bool enable_file = true, string upd_address = null, int udp_port = 1000)
        {
            List<ILogRecorder> recorder_list = new List<ILogRecorder>();
            if (enable_file)
            {
                recorder_list.Add(new LogRecorder_File(LogRecorder_File.GetLogFileDirPath()));
            }
            if (!string.IsNullOrEmpty(upd_address))
            {
                var remote = LogRecorder_Udp.CreateRemoteIP(upd_address, udp_port);
                if (remote != null)
                {
                    recorder_list.Add(new LogRecorder_Udp(remote));
                }
            }

            LogRecorderMgr.Init(recorder_list.ToArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ELogMask Lvl2Mask(ELogLvl lvl)
        {
            uint m = 1U << (int)lvl;
            ELogMask mask = (ELogMask)(~(m - 1));
            return mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _IsEnable(ELogLvl lvl)
        {
            ELogMask lvl_mask = (ELogMask)(1u << (int)lvl);

            if ((lvl_mask & AllowMask) == ELogMask.None)
                return false;
            return true;
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D(string format, params object[] args)
        {
#if ENABLE_LOG_Debug
            if (!_IsEnable(ELogLvl.Debug))
                return;

            LogPrinter.Print(ELogLvl.Debug, null, TraceMask, UnityMask, null, format, args);
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
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print1(ELogLvl.Debug, null, TraceMask, UnityMask, null, format, arg0);
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
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print2(ELogLvl.Debug, null, TraceMask, UnityMask, null, format, arg0, arg1);
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
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print3(ELogLvl.Debug, null, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
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
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print(ELogLvl.Debug, null, TraceMask, UnityMask, content, format, args);
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
            if (!_IsEnable(ELogLvl.Info))
                return;
            LogPrinter.Print(ELogLvl.Info, null, TraceMask, UnityMask, null, format, args);
#endif
        }

        /// <summary>
        /// Info
        /// </summary>
        [Conditional(LogConditional.COND_INFO)]
        [HideInCallstack]
        public static void I<T0>(string format, T0 arg0)
        {
#if ENABLE_LOG_Info
            if (!_IsEnable(ELogLvl.Info))
                return;
            LogPrinter.Print1(ELogLvl.Info, null, TraceMask, UnityMask, null, format, arg0);
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
            if (!_IsEnable(ELogLvl.Info))
                return;
            LogPrinter.Print(ELogLvl.Info, null, TraceMask, UnityMask, content, format, args);
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
            if (!_IsEnable(ELogLvl.Warning))
                return;
            LogPrinter.Print(ELogLvl.Warning, null, TraceMask, UnityMask, null, format, args);
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
            if (!_IsEnable(ELogLvl.Warning))
                return;
            LogPrinter.Print(ELogLvl.Warning, null, TraceMask, UnityMask, content, format, args);
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
            if (!_IsEnable(ELogLvl.Assert))
                return;

            LogPrinter.Print(ELogLvl.Assert, null, TraceMask, UnityMask, null, format, args);

#endif
        }

        /// <summary>
        /// assert
        /// </summary>
        [Conditional(LogConditional.COND_ASSERT)]
        [HideInCallstack]
        public static void Assert<T0>(bool cond, string format, T0 arg0)
        {
#if ENABLE_LOG_Assert            
            if (cond)
                return;

            if (format == null)
            {
                format = "Asset Error";                                
            }

            if (!_IsEnable(ELogLvl.Assert))
                return;

            LogPrinter.Print1(ELogLvl.Assert, null, TraceMask, UnityMask, null, format, arg0);

#endif
        }

        /// <summary>
        /// assert
        /// </summary>
        [Conditional(LogConditional.COND_ASSERT)]
        [HideInCallstack]
        public static void Assert<T0,T1>(bool cond, string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Assert            
            if (cond)
                return;

            if (format == null)
            {
                format = "Asset Error";
            }

            if (!_IsEnable(ELogLvl.Assert))
                return;

            LogPrinter.Print2(ELogLvl.Assert, null, TraceMask, UnityMask, null, format, arg0,arg1);

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
            if (!_IsEnable(ELogLvl.Assert))
                return;
            LogPrinter.Print(ELogLvl.Assert, null, TraceMask, UnityMask, content, format, args);
#endif
        }

        [Conditional(LogConditional.COND_EXCEPTION)]
        [HideInCallstack]
        public static void E(Exception e)
        {
#if ENABLE_LOG_Exception
            if (!_IsEnable(ELogLvl.Exception))
                return;

            LogPrinter.PrintE(UnityMask, e);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(string format, params object[] args)
        {
#if ENABLE_LOG_Error          
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print(ELogLvl.Error, null, TraceMask, UnityMask, null, format, args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print(ELogLvl.Error, null, TraceMask, UnityMask, content, format, args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0>(string format, T0 arg0)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print1(ELogLvl.Error, null, TraceMask, UnityMask, null, format, arg0);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0>(UnityEngine.Object content, string format, T0 arg0)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print1(ELogLvl.Error, null, TraceMask, UnityMask, content, format, arg0);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1>(string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print2(ELogLvl.Error, null, TraceMask, UnityMask, null, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1>(UnityEngine.Object content, string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print2(ELogLvl.Error, null, TraceMask, UnityMask, content, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print3(ELogLvl.Error, null, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1, T2>(UnityEngine.Object content, string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print3(ELogLvl.Error, null, TraceMask, UnityMask, content, format, arg0, arg1, arg2);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void ErrCode<T>(T code, string format = null, params object[] args) where T : Enum
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;

            if (code.GetHashCode() == 0)
                return;

            if (format == null)
                format = string.Empty;

            LogPrinter.Print(ELogLvl.Error, null, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void ErrCode<T, T0>(T code, string format, T0 arg0) where T : Enum
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print1(ELogLvl.Error, null, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0);
#endif
        }

        [HideInCallstack]
        public static void ErrCode<T, T0, T1>(T code, string format, T0 arg0, T1 arg1) where T : Enum
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;

            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print2(ELogLvl.Error, null, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0, arg1);
#endif
        }


        [HideInCallstack]
        public static void ErrCode<T, T0, T1, T2>(T code, string format, T0 arg0, T1 arg1, T2 arg2) where T : Enum
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print3(ELogLvl.Error, null, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0, arg1, arg2);
#endif
        }

    }

    public struct TagLog
    {
        public readonly string Tag;
        // allow mask, 对应的位,如果是true,就可以显示
        public ELogMask AllowMask;
        public ELogMask UnityMask;
        public ELogMask TraceMask;

        public TagLog(string tag, ELogMask allow_mask)
        {
            Tag = tag;
            AllowMask = allow_mask;
            UnityMask = allow_mask;
            TraceMask = allow_mask;
        }

        public void SetMasks(ELogLvl log_lvl)
        {
            AllowMask = Log.Lvl2Mask(log_lvl);
            UnityMask = AllowMask;
            TraceMask = AllowMask;
        }

        public static TagLog Create(string tag_name, ELogLvl log_lvl = ELogLvl.Info)
        {
            return new TagLog(tag_name, Log.Lvl2Mask(log_lvl));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _IsEnable(ELogLvl lvl)
        {
            ELogMask lvl_mask = (ELogMask)(1u << (int)lvl);

            if ((lvl_mask & AllowMask) == ELogMask.None)
                return false;

            if ((lvl_mask & Log.AllowMask) == ELogMask.None)
                return false;

            return true;
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public void D(string format, params object[] args)
        {
#if ENABLE_LOG_Debug
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print(ELogLvl.Debug, Tag, TraceMask, UnityMask, null, format, args);
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
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print1(ELogLvl.Debug, Tag, TraceMask, UnityMask, null, format, arg0);
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
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print2(ELogLvl.Debug, Tag, TraceMask, UnityMask, null, format, arg0, arg1);

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
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print3(ELogLvl.Debug, Tag, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
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
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print(ELogLvl.Debug, Tag, TraceMask, UnityMask, content, format, args);
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
            if (!_IsEnable(ELogLvl.Info))
                return;
            LogPrinter.Print(ELogLvl.Info, Tag, TraceMask, UnityMask, null, format, args);
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
            if (!_IsEnable(ELogLvl.Info))
                return;
            LogPrinter.Print(ELogLvl.Info, Tag, TraceMask, UnityMask, content, format, args);
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
            if (!_IsEnable(ELogLvl.Warning))
                return;
            LogPrinter.Print(ELogLvl.Warning, Tag, TraceMask, UnityMask, null, format, args);
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
            if (!_IsEnable(ELogLvl.Warning))
                return;
            LogPrinter.Print(ELogLvl.Warning, Tag, TraceMask, UnityMask, content, format, args);
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

            if (!_IsEnable(ELogLvl.Assert))
                return;
            LogPrinter.Print(ELogLvl.Assert, Tag, TraceMask, UnityMask, null, format, args);
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
            if (!_IsEnable(ELogLvl.Assert))
                return;
            LogPrinter.Print(ELogLvl.Assert, Tag, TraceMask, UnityMask, content, format, args);
#endif
        }

        [Conditional(LogConditional.COND_EXCEPTION)]
        [HideInCallstack]
        public void E(Exception e)
        {
#if ENABLE_LOG_Exception
            if (!_IsEnable(ELogLvl.Exception))
                return;
            LogPrinter.PrintE(UnityMask, e);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E(string format, params object[] args)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print(ELogLvl.Error, Tag, TraceMask, UnityMask, null, format, args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print(ELogLvl.Error, Tag, TraceMask, UnityMask, content, format, args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T0>(string format, T0 arg0)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print1(ELogLvl.Error, Tag, TraceMask, UnityMask, null, format, arg0);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T0>(UnityEngine.Object content, string format, T0 arg0)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print1(ELogLvl.Error, Tag, TraceMask, UnityMask, content, format, arg0);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T0, T1>(string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print2(ELogLvl.Error, Tag, TraceMask, UnityMask, null, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T0, T1>(UnityEngine.Object content, string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print2(ELogLvl.Error, Tag, TraceMask, UnityMask, content, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print3(ELogLvl.Error, Tag, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T0, T1, T2>(UnityEngine.Object content, string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print3(ELogLvl.Error, Tag, TraceMask, UnityMask, content, format, arg0, arg1, arg2);
#endif
        }


        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void ErrCode<T>(T code, string format = null, params object[] args) where T : Enum
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;

            if (code.GetHashCode() == 0)
                return;
            if (format == null)
                format = string.Empty;

            LogPrinter.Print(ELogLvl.Error, Tag, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void ErrCode<T, T0>(T code, string format, T0 arg0) where T : Enum
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print1(ELogLvl.Error, Tag, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0);
#endif
        }

        [HideInCallstack]
        public void ErrCode<T, T0, T1>(T code, string format, T0 arg0, T1 arg1) where T : Enum
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print2(ELogLvl.Error, Tag, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0, arg1);
#endif
        }


        [HideInCallstack]
        public void ErrCode<T, T0, T1, T2>(T code, string format, T0 arg0, T1 arg1, T2 arg2) where T : Enum
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print3(ELogLvl.Error, Tag, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0, arg1, arg2);
#endif
        }
    }

    public abstract class TagLogT<TSub> where TSub : new()
    {
        public static string Tag = typeof(TSub).Name;
        public static ELogMask AllowMask = ELogMask.Info | ELogMask.Warning | ELogMask.Assert | ELogMask.Error | ELogMask.Exception;
        public static ELogMask UnityMask = AllowMask;
        public static ELogMask TraceMask = AllowMask;


        public static void SetMasks(ELogLvl log_lvl)
        {
            AllowMask = Log.Lvl2Mask(log_lvl);
            UnityMask = AllowMask;
            TraceMask = AllowMask;
        }


        private static bool _IsEnable(ELogLvl lvl)
        {
            ELogMask lvl_mask = (ELogMask)(1u << (int)lvl);

            if ((lvl_mask & AllowMask) == ELogMask.None)
                return false;

            if ((lvl_mask & Log.AllowMask) == ELogMask.None)
                return false;

            return true;
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D(string format, params object[] args)
        {
#if ENABLE_LOG_Debug
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print(ELogLvl.Debug, Tag, TraceMask, UnityMask, null, format, args);
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
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print1(ELogLvl.Debug, Tag, TraceMask, UnityMask, null, format, arg0);
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
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print2(ELogLvl.Debug, Tag, TraceMask, UnityMask, null, format, arg0, arg1);

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
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print3(ELogLvl.Debug, Tag, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
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
            if (!_IsEnable(ELogLvl.Debug))
                return;
            LogPrinter.Print(ELogLvl.Debug, Tag, TraceMask, UnityMask, content, format, args);
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
            if (!_IsEnable(ELogLvl.Info))
                return;
            LogPrinter.Print(ELogLvl.Info, Tag, TraceMask, UnityMask, null, format, args);
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
            if (!_IsEnable(ELogLvl.Info))
                return;
            LogPrinter.Print(ELogLvl.Info, Tag, TraceMask, UnityMask, content, format, args);
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
            if (!_IsEnable(ELogLvl.Warning))
                return;
            LogPrinter.Print(ELogLvl.Warning, Tag, TraceMask, UnityMask, null, format, args);
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
            if (!_IsEnable(ELogLvl.Warning))
                return;
            LogPrinter.Print(ELogLvl.Warning, Tag, TraceMask, UnityMask, content, format, args);
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
            if (!_IsEnable(ELogLvl.Assert))
                return;
            LogPrinter.Print(ELogLvl.Assert, Tag, TraceMask, UnityMask, null, format, args);
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
            if (!_IsEnable(ELogLvl.Assert))
                return;
            LogPrinter.Print(ELogLvl.Assert, Tag, TraceMask, UnityMask, content, format, args);
#endif
        }

        [Conditional(LogConditional.COND_EXCEPTION)]
        [HideInCallstack]
        public static void E(Exception e)
        {
#if ENABLE_LOG_Exception
            if (!_IsEnable(ELogLvl.Exception))
                return;
            LogPrinter.PrintE(UnityMask, e);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(string format, params object[] args)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print(ELogLvl.Error, Tag, TraceMask, UnityMask, null, format, args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print(ELogLvl.Error, Tag, TraceMask, UnityMask, content, format, args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0>(string format, T0 arg0)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print1(ELogLvl.Error, Tag, TraceMask, UnityMask, null, format, arg0);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0>(UnityEngine.Object content, string format, T0 arg0)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print1(ELogLvl.Error, Tag, TraceMask, UnityMask, content, format, arg0);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1>(string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print2(ELogLvl.Error, Tag, TraceMask, UnityMask, null, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1>(UnityEngine.Object content, string format, T0 arg0, T1 arg1)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print2(ELogLvl.Error, Tag, TraceMask, UnityMask, content, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print3(ELogLvl.Error, Tag, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T0, T1, T2>(UnityEngine.Object content, string format, T0 arg0, T1 arg1, T2 arg2)
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            LogPrinter.Print3(ELogLvl.Error, Tag, TraceMask, UnityMask, content, format, arg0, arg1, arg2);
#endif
        }



        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void ErrCode<T>(T code, string format = null, params object[] args) where T : Enum
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            if (code.GetHashCode() == 0)
                return;
            if (format == null)
                format = string.Empty;

            LogPrinter.Print(ELogLvl.Error, Tag, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), args);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void ErrCode<T, T0>(T code, string format, T0 arg0) where T : Enum
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print1(ELogLvl.Error, Tag, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0);
#endif
        }

        [HideInCallstack]
        public void ErrCode<T, T0, T1>(T code, string format, T0 arg0, T1 arg1) where T : Enum
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print2(ELogLvl.Error, Tag, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0, arg1);
#endif
        }


        [HideInCallstack]
        public void ErrCode<T, T0, T1, T2>(T code, string format, T0 arg0, T1 arg1, T2 arg2) where T : Enum
        {
#if ENABLE_LOG_Error
            if (!_IsEnable(ELogLvl.Error))
                return;
            if (code.GetHashCode() == 0)
                return;

            LogPrinter.Print3(ELogLvl.Error, Tag, TraceMask, UnityMask, null, ZString.Format("ErrorCode: {0} {1}", code, format), arg0, arg1, arg2);
#endif
        }
    }
}
