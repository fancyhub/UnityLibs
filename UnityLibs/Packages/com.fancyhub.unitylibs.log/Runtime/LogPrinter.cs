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


    internal static class LogPrinter
    {
        private const bool C_Need_File_Info = false;
        private const int C_Buffer_Size = 2048;
        private const string C_Timer_Formater = "[{0:yy_MM_dd HH:mm:ss:fff} F:{1} T:{2}]";
        private const int C_Stack_Start_Index = 3;
        private static int[] C_Stack_Depth = new int[]
        {
            0, //Debug
            0, //Info
            1000, //Warning,
            1000,//Assert,
            1000,//Error,
            1000,//Exception,
            1000,//Off,
        };

        [HideInCallstack]
        public static void Print(ELogLvl log_lvl, string tag, ELogMask trace_mask, ELogMask unity_mask, UnityEngine.Object context, string format, object[] args)
        {
            ELogMask lvl_mask = _ToMask(log_lvl);

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_Buffer_Size]);
            _AppendTime(ref sb);
            _AppendLvlTag(ref sb, log_lvl, tag);
            _AppendFormat(ref sb, format, args);

            bool with_trace = (trace_mask & lvl_mask) != ELogMask.None;
            string log_msg = null;
            if ((unity_mask & lvl_mask) != ELogMask.None)
            {
                log_msg = sb.ToString();
                LogType logType = _ToUnityLogType(log_lvl);
                LogOption logOption = with_trace ? LogOption.None : LogOption.NoStacktrace;
                //UnityEngine.Debug.LogFormat(logType, logOption, context, log_msg); // 如果log_msg 里面含有{}, 就会报错
                var unityLogger = UnityEngine.Debug.unityLogger;
                if (unityLogger != null && unityLogger.IsLogTypeAllowed(logType))
                {
                    unityLogger.Log(logType, (object)log_msg, context);
                }
            }

            if (log_msg != null)
            {
                sb.Clear();
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                if (sb.Length == 0)
                    LogRecorderMgr.Record(log_msg);
                else
                    LogRecorderMgr.Record(log_msg, sb.ToString());
            }
            else
            {
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                LogRecorderMgr.Record(sb.ToString());
            }
        }

        [HideInCallstack]
        public static void PrintE(ELogMask unity_mask, Exception e)
        {
            ELogLvl log_lvl = ELogLvl.Exception;
            ELogMask lvl_mask = _ToMask(log_lvl);


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
        public static void Print1<T0>(ELogLvl log_lvl, string tag, ELogMask trace_mask, ELogMask unity_mask, UnityEngine.Object context, string format, T0 arg0)
        {
            ELogMask lvl_mask = _ToMask(log_lvl);


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
                sb.Clear();
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                if (sb.Length == 0)
                    LogRecorderMgr.Record(log_msg);
                else
                    LogRecorderMgr.Record(log_msg, sb.ToString());
            }
            else
            {
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                LogRecorderMgr.Record(sb.ToString());
            }
        }

        [HideInCallstack]
        public static void Print2<T0, T1>(ELogLvl log_lvl, string tag, ELogMask trace_mask, ELogMask unity_mask, UnityEngine.Object context, string format, T0 arg0, T1 arg1)
        {
            ELogMask lvl_mask = _ToMask(log_lvl);



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
                sb.Clear();
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                if (sb.Length == 0)
                    LogRecorderMgr.Record(log_msg);
                else
                    LogRecorderMgr.Record(log_msg, sb.ToString());
            }
            else
            {
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                LogRecorderMgr.Record(sb.ToString());
            }
        }

        [HideInCallstack]
        public static void Print3<T0, T1, T2>(ELogLvl log_lvl, string tag, ELogMask trace_mask, ELogMask unity_mask, UnityEngine.Object context, string format, T0 arg0, T1 arg1, T2 arg2)
        {
            ELogMask lvl_mask = _ToMask(log_lvl);

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
                sb.Clear();
                _AppendTrace(ref sb, C_Stack_Depth[(int)log_lvl], C_Need_File_Info);
                if (sb.Length == 0)
                    LogRecorderMgr.Record(log_msg);
                else
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void _AppendTime(ref ValueStringBuilder sb)
        {
            sb.Append(ZString.Format(C_Timer_Formater, System.DateTime.Now, LogTimeUpdater.FrameCount, System.Threading.Thread.CurrentThread.ManagedThreadId));
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
}
