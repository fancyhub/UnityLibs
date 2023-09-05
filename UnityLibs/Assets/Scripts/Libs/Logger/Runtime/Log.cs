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

    internal static class LogPrinter
    {
        private const bool C_Need_File_Info = false;
        private const int C_Buffer_Size = 2048;
        private const string C_Timer_Formater = "[{0:yy_MM_dd HH:mm:ss:fff} {1}]";
        private const int C_Stack_Start_Index = 3;
        private static int[] C_Stack_Depth = new int[]
        {
            8, //Debug
            8, //Info
            1000, //Warning,
            1000,//Assert,
            1000,//Error,
            1000,//Exception,
            1000,//Off,
        };

        [HideInCallstack]
        public static void Print(ELogLvl log_lvl, string tag, ELogMask allow_mask, ELogMask trace_mask, ELogMask unity_mask, UnityEngine.Object context, string format, object[] args)
        {
            ELogMask lvl_mask = _ToMask(log_lvl);

            if ((lvl_mask & allow_mask) == ELogMask.None)
                return;


            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_Buffer_Size]);
            _AppendTime(ref sb);
            _AppendLvlTag(ref sb, log_lvl, tag);
            _AppendFormat(ref sb, format, args);

            bool with_trace = (trace_mask & lvl_mask) != ELogMask.None;

            string log_msg = null;
            if ((unity_mask & lvl_mask) != ELogMask.None)
            {
                log_msg = sb.ToString();
                UnityEngine.Debug.LogFormat(_ToUnityLogType(log_lvl), with_trace ? LogOption.None : LogOption.NoStacktrace, context, log_msg);
            }

            if (log_msg != null)
            {
                sb = new ValueStringBuilder(stackalloc char[C_Buffer_Size]);
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                LogRecorderMgr.Record(log_msg, sb.ToString());
            }
            else
            {
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                LogRecorderMgr.Record(sb.ToString());
            }
        }

        [HideInCallstack]
        public static void PrintE(ELogMask enable_mask, ELogMask unity_mask, Exception e)
        {
            ELogLvl log_lvl = ELogLvl.Exception;
            ELogMask lvl_mask = _ToMask(log_lvl);

            if ((lvl_mask & enable_mask) == ELogMask.None)
                return;

            if ((unity_mask & lvl_mask) != ELogMask.None)
                UnityEngine.Debug.LogException(e);


            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_Buffer_Size]);
            _AppendTime(ref sb);
            _AppendLvlTag(ref sb, log_lvl, null);
            sb.Append(e.Message);
            sb.Append(e.StackTrace);
            LogRecorderMgr.Record(sb.ToString());
        }

        [HideInCallstack]
        public static void Print1<T0>(ELogLvl log_lvl, string tag, ELogMask enable_mask, ELogMask trace_mask, ELogMask unity_mask, UnityEngine.Object context, string format, T0 arg0)
        {
            ELogMask lvl_mask = _ToMask(log_lvl);

            if ((lvl_mask & enable_mask) == ELogMask.None)
                return;


            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_Buffer_Size]);
            _AppendTime(ref sb);
            _AppendLvlTag(ref sb, log_lvl, tag);
            sb.Append(ZString.Format(format, arg0));

            bool with_trace = (trace_mask & lvl_mask) != ELogMask.None;

            string log_msg = null;
            if ((unity_mask & lvl_mask) != ELogMask.None)
            {
                log_msg = sb.ToString();
                UnityEngine.Debug.LogFormat(_ToUnityLogType(log_lvl), with_trace ? LogOption.None : LogOption.NoStacktrace, context, log_msg);
            }

            if (log_msg != null)
            {
                sb = new ValueStringBuilder(stackalloc char[C_Buffer_Size]);
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                LogRecorderMgr.Record(log_msg, sb.ToString());
            }
            else
            {
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                LogRecorderMgr.Record(sb.ToString());
            }
        }

        [HideInCallstack]
        public static void Print2<T0, T1>(ELogLvl log_lvl, string tag, ELogMask enable_mask, ELogMask trace_mask, ELogMask unity_mask, UnityEngine.Object context, string format, T0 arg0, T1 arg1)
        {
            ELogMask lvl_mask = _ToMask(log_lvl);

            if ((lvl_mask & enable_mask) == ELogMask.None)
                return;


            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_Buffer_Size]);
            _AppendTime(ref sb);
            _AppendLvlTag(ref sb, log_lvl, tag);
            sb.Append(ZString.Format(format, arg0, arg1));

            bool with_trace = (trace_mask & lvl_mask) != ELogMask.None;

            string log_msg = null;
            if ((unity_mask & lvl_mask) != ELogMask.None)
            {
                log_msg = sb.ToString();
                UnityEngine.Debug.LogFormat(_ToUnityLogType(log_lvl), with_trace ? LogOption.None : LogOption.NoStacktrace, context, log_msg);
            }

            if (log_msg != null)
            {
                sb = new ValueStringBuilder(stackalloc char[C_Buffer_Size]);
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                LogRecorderMgr.Record(log_msg, sb.ToString());
            }
            else
            {
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                LogRecorderMgr.Record(sb.ToString());
            }
        }

        [HideInCallstack]
        public static void Print3<T0, T1, T2>(ELogLvl log_lvl, string tag, ELogMask enable_mask, ELogMask trace_mask, ELogMask unity_mask, UnityEngine.Object context, string format, T0 arg0, T1 arg1, T2 arg2)
        {
            ELogMask lvl_mask = _ToMask(log_lvl);

            if ((lvl_mask & enable_mask) == ELogMask.None)
                return;


            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_Buffer_Size]);
            _AppendTime(ref sb);
            _AppendLvlTag(ref sb, log_lvl, tag);
            sb.Append(ZString.Format(format, arg0, arg1, arg2));

            bool with_trace = (trace_mask & lvl_mask) != ELogMask.None;

            string log_msg = null;
            if ((unity_mask & lvl_mask) != ELogMask.None)
            {
                log_msg = sb.ToString();
                UnityEngine.Debug.LogFormat(_ToUnityLogType(log_lvl), with_trace ? LogOption.None : LogOption.NoStacktrace, context, log_msg);
            }

            if (log_msg != null)
            {
                sb = new ValueStringBuilder(stackalloc char[C_Buffer_Size]);
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                LogRecorderMgr.Record(log_msg, sb.ToString());
            }
            else
            {
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                LogRecorderMgr.Record(sb.ToString());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static LogType _ToUnityLogType(ELogLvl lvl)
        {
            switch (lvl)
            {
                case ELogLvl.Debug: return LogType.Log;
                case ELogLvl.Info: return LogType.Log;
                case ELogLvl.Warning: return LogType.Warning;
                case ELogLvl.Assert: return LogType.Assert;
                case ELogLvl.Error: return LogType.Error;
                case ELogLvl.Exception: return LogType.Exception;
                default: return LogType.Error;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ELogMask _ToMask(ELogLvl lvl)
        {
            return (ELogMask)(1U << (int)lvl);
        }

        private static void _AppendFormat(ref ValueStringBuilder sb, string format, object[] args)
        {
            if (format == null)
                return;
            if (args != null && args.Length > 0)
            {
                sb.Append(string.Format(format, args));
            }
            else
                sb.Append(format);
        }

        private static void _AppendTime(ref ValueStringBuilder sb)
        {
            sb.Append(ZString.Format(C_Timer_Formater, System.DateTime.Now, TimeUtil.FrameCount));
        }

        private static void _AppendTrace(ref ValueStringBuilder sb, int max_step, bool need_file_info)
        {
            var trace = new System.Diagnostics.StackTrace(need_file_info);
            int start_index = C_Stack_Start_Index;
            int end_index = System.Math.Min(trace.FrameCount, (max_step + start_index));

            for (int i = start_index; i < end_index; ++i)
            {
                System.Diagnostics.StackFrame frame = trace.GetFrame(i);
                System.Reflection.MethodBase mb = frame.GetMethod();


                sb.Append("\t");
                sb.Append(mb.DeclaringType.Name);
                sb.Append(":");
                sb.Append(mb.Name);
                sb.Append("(");
                System.Reflection.ParameterInfo[] param_list = mb.GetParameters();
                int c = param_list.Length;
                for (int j = 0; j < c; ++j)
                {
                    sb.Append(param_list[j].ParameterType.Name);
                    if (j < (c - 1))
                    {
                        sb.Append(",");
                    }
                }
                sb.Append(")");

                if (need_file_info)
                {
                    string file_name = frame.GetFileName();
                    int line_num = frame.GetFileLineNumber();
                    sb.Append(" (at ");
                    sb.Append(file_name);
                    sb.Append(":");
                    sb.Append(line_num.ToString());
                    sb.Append(")");
                }
                sb.Append("\n");
            }
        }

        private static void _AppendLvlTag(ref ValueStringBuilder sb, ELogLvl log, string tag)
        {
            switch (log)
            {
                case ELogLvl.Debug:
                    sb.Append("D: ");
                    break;
                case ELogLvl.Info:
                    sb.Append("I: ");
                    break;
                case ELogLvl.Warning:
                    sb.Append("W: ");
                    break;

                case ELogLvl.Assert:
                case ELogLvl.Error:
                case ELogLvl.Exception:
                    sb.Append("E: ");
                    break;
                default:
                    sb.Append("U: ");
                    break;
            }

            if (tag != null)
            {
                sb.Append('<');
                sb.Append(tag);
                sb.Append('>');
                sb.Append(' ');
            }
        }
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
        public static void E<T1, T2>(UnityEngine.Object content, string format, T1 arg0, T2 arg1)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print2(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, content, format, arg0, arg1);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1, T2, T3>(string format, T1 arg0, T2 arg1, T3 arg2)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print3(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, null, format, arg0, arg1, arg2);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1, T2, T3>(UnityEngine.Object content, string format, T1 arg0, T2 arg1, T3 arg2)
        {
#if ENABLE_LOG_Error
            LogPrinter.Print3(ELogLvl.Error, null, AllowMask, TraceMask, UnityMask, content, format, arg0, arg1, arg2);
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
    }

    public abstract class TagLoggerT<T> where T : new()
    {
        public static string Tag = nameof(T);
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
    }
}
