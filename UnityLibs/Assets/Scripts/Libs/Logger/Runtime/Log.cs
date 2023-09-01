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
        private const string TRUE = "NET_STANDARD"; //这个名字只要保证 编译的时候指定就行了
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
    public enum ELogMask : uint
    {
        None = 0,
        Debug = 1 << (int)ELogLvl.Debug,
        Info = 1 << (int)ELogLvl.Info,
        Warning = 1 << (int)ELogLvl.Warning,
        Assert = 1 << (int)ELogLvl.Assert,
        Error = 1 << (int)ELogLvl.Error,
        Exception = 1 << (int)ELogLvl.Exception,
    }


    public static class Log
    {
        private const int C_TEMP_SIZE = 1024;

        // allow mask, 对应的位,如果是true,就可以显示
        public static ELogMask AllowMask = (ELogMask)uint.MaxValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _IsEnable(ELogMask mask, ELogLvl lvl)
        {
            return (mask & (ELogMask)(1 << (int)lvl)) != ELogMask.None;
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D(string format, params object[] args)
        {
#if ENABLE_LOG_Debug
            ELogLvl log_lvl = ELogLvl.Debug;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            _AppendFormat(ref sb, format, args);
            UnityEngine.Debug.Log(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Debug;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            sb.Append(ZString.Format(format, arg0));
            UnityEngine.Debug.Log(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Debug;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            sb.Append(ZString.Format(format, arg0, arg1));
            UnityEngine.Debug.Log(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Debug;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            sb.Append(ZString.Format(format, arg0, arg1, arg2));
            UnityEngine.Debug.Log(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Debug;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            _AppendFormat(ref sb, format, args);
            UnityEngine.Debug.Log(sb.ToString(), content);
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
            ELogLvl log_lvl = ELogLvl.Info;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.Log(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Info;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.Log(sb.ToString(), content);
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
            ELogLvl log_lvl = ELogLvl.Warning;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.LogWarning(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Warning;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.LogWarning(sb.ToString(), content);
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
            ELogLvl log_lvl = ELogLvl.Assert;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            if (cond)
                return;
            if (format == null)
            {
                format = "Asset Error";
                args = null;
            }

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.LogError(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Assert;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            if (cond)
                return;
            if (format == null)
            {
                format = "Asset Error";
                args = null;
            }

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            _AppendFormat(ref sb, format, args);


            UnityEngine.Debug.LogError(sb.ToString(), content);
#endif
        }

        [Conditional(LogConditional.COND_EXCEPTION)]
        [HideInCallstack]
        public static void E(Exception e)
        {
#if ENABLE_LOG_Exception
            ELogLvl log_lvl = ELogLvl.Exception;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            UnityEngine.Debug.LogException(e);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(string format, params object[] args)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.LogError(sb.ToString());
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.LogError(sb.ToString(), content);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1>(string format, T1 arg0)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            sb.Append(ZString.Format(format, arg0));
            UnityEngine.Debug.LogError(sb.ToString());
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1>(UnityEngine.Object content, string format, T1 arg0)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            sb.Append(ZString.Format(format, arg0));
            UnityEngine.Debug.LogError(sb.ToString(), content);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1, T2>(string format, T1 arg0, T2 arg1)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            sb.Append(ZString.Format(format, arg0, arg1));

            UnityEngine.Debug.LogError(sb.ToString());
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1, T2>(UnityEngine.Object content, string format, T1 arg0, T2 arg1)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            sb.Append(ZString.Format(format, arg0, arg1));
            UnityEngine.Debug.LogError(sb.ToString(), content);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1, T2, T3>(string format, T1 arg0, T2 arg1, T3 arg2)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            sb.Append(ZString.Format(format, arg0, arg1));

            UnityEngine.Debug.LogError(sb.ToString());
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1, T2, T3>(UnityEngine.Object content, string format, T1 arg0, T2 arg1, T3 arg2)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, null);
            sb.Append(ZString.Format(format, arg0, arg1));
            UnityEngine.Debug.LogError(sb.ToString(), content);
#endif
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

        private static void _Append_Lvl_Tag(ref ValueStringBuilder sb, ELogLvl log, string tag)
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

    public struct TagLogger
    {
        private const int C_TEMP_SIZE = 1024;

        public readonly string Tag;
        // allow mask, 对应的位,如果是true,就可以显示
        public ELogMask AllowMask;

        public TagLogger(string tag, ELogMask allow_mask)
        {
            Tag = tag;
            AllowMask = allow_mask;
        }

        public static TagLogger Create(string tag_name, ELogLvl log_lvl = ELogLvl.Info)
        {
            uint m = 1U << (int)log_lvl;
            ELogMask mask = (ELogMask)(~(m - 1));
            return new TagLogger(tag_name, mask);
        }


        public bool this[ELogLvl lvl]
        {
            get
            {
                return _IsEnable(AllowMask, lvl);
            }
            set
            {
                if (value)
                    AllowMask = AllowMask | (ELogMask)(1 << (int)lvl);
                else
                    AllowMask = AllowMask & (ELogMask)(~(1 << (int)lvl));
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool _IsEnable(ELogMask mask, ELogLvl lvl)
        {
            return (mask & (ELogMask)(1 << (int)lvl)) != ELogMask.None;
        }

        /// <summary>
        /// Debug Info
        /// </summary>
        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public void D(string format, params object[] args)
        {
#if ENABLE_LOG_Debug
            ELogLvl log_lvl = ELogLvl.Debug;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            _AppendFormat(ref sb, format, args);
            UnityEngine.Debug.Log(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Debug;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            sb.Append(ZString.Format(format, arg0));
            UnityEngine.Debug.Log(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Debug;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            sb.Append(ZString.Format(format, arg0, arg1));
            UnityEngine.Debug.Log(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Debug;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            sb.Append(ZString.Format(format, arg0, arg1, arg2));
            UnityEngine.Debug.Log(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Debug;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            _AppendFormat(ref sb, format, args);
            UnityEngine.Debug.Log(sb.ToString(), content);
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
            ELogLvl log_lvl = ELogLvl.Info;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.Log(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Info;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.Log(sb.ToString(), content);
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
            ELogLvl log_lvl = ELogLvl.Warning;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.LogWarning(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Warning;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.LogWarning(sb.ToString(), content);
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
            ELogLvl log_lvl = ELogLvl.Assert;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            if (cond)
                return;
            if (format == null)
            {
                format = "Asset Error";
                args = null;
            }

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.LogError(sb.ToString());
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
            ELogLvl log_lvl = ELogLvl.Assert;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            if (cond)
                return;
            if (format == null)
            {
                format = "Asset Error";
                args = null;
            }

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            _AppendFormat(ref sb, format, args);


            UnityEngine.Debug.LogError(sb.ToString(), content);
#endif
        }

        [Conditional(LogConditional.COND_EXCEPTION)]
        [HideInCallstack]
        public void E(Exception e)
        {
#if ENABLE_LOG_Exception
            ELogLvl log_lvl = ELogLvl.Exception;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            UnityEngine.Debug.LogException(e);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E(string format, params object[] args)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.LogError(sb.ToString());
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E(UnityEngine.Object content, string format, params object[] args)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            _AppendFormat(ref sb, format, args);

            UnityEngine.Debug.LogError(sb.ToString(), content);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T1>(string format, T1 arg0)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            sb.Append(ZString.Format(format, arg0));
            UnityEngine.Debug.LogError(sb.ToString());
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T1>(UnityEngine.Object content, string format, T1 arg0)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            sb.Append(ZString.Format(format, arg0));
            UnityEngine.Debug.LogError(sb.ToString(), content);
#endif
        }
        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T1, T2>(string format, T1 arg0, T2 arg1)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            sb.Append(ZString.Format(format, arg0, arg1));

            UnityEngine.Debug.LogError(sb.ToString());
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T1, T2>(UnityEngine.Object content, string format, T1 arg0, T2 arg1)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            sb.Append(ZString.Format(format, arg0, arg1));
            UnityEngine.Debug.LogError(sb.ToString(), content);
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T1, T2, T3>(string format, T1 arg0, T2 arg1, T3 arg2)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            sb.Append(ZString.Format(format, arg0, arg1));

            UnityEngine.Debug.LogError(sb.ToString());
#endif
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public void E<T1, T2, T3>(UnityEngine.Object content, string format, T1 arg0, T2 arg1, T3 arg2)
        {
#if ENABLE_LOG_Error
            ELogLvl log_lvl = ELogLvl.Error;
            if (!_IsEnable(AllowMask, log_lvl))
                return;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[C_TEMP_SIZE]);
            _Append_Lvl_Tag(ref sb, log_lvl, Tag);
            sb.Append(ZString.Format(format, arg0, arg1));
            UnityEngine.Debug.LogError(sb.ToString(), content);
#endif
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
        private static void _Append_Lvl_Tag(ref ValueStringBuilder sb, ELogLvl log, string tag)
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


    public abstract class TagLoggerT<T> where T : new()
    {
        public static TagLogger Log = TagLogger.Create(typeof(T).Name, ELogLvl.Debug);

        static TagLoggerT()
        {
            new T();
        }

        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D(string format, params object[] args)
        {
            Log.D(format, args);
        }

        [Conditional(LogConditional.COND_DEBUG)]
        [HideInCallstack]
        public static void D(UnityEngine.Object content, string format, params object[] args)
        {
            Log.D(content, format, args);
        }

        [Conditional(LogConditional.COND_INFO)]
        [HideInCallstack]
        public static void I(string format, params object[] args)
        {
            Log.I(format, args);
        }

        [Conditional(LogConditional.COND_INFO)]
        [HideInCallstack]
        public static void I(UnityEngine.Object content, string format, params object[] args)
        {
            Log.I(format, args);
        }

        [Conditional(LogConditional.COND_WARNING)]
        [HideInCallstack]
        public static void W(string format, params object[] args)
        {
            Log.W(format, args);
        }

        [Conditional(LogConditional.COND_WARNING)]
        [HideInCallstack]
        public static void W(UnityEngine.Object content, string format, params object[] args)
        {
            Log.W(content, format, args);
        }


        [Conditional(LogConditional.COND_ASSERT)]
        [HideInCallstack]
        public static void Assert(bool cond, string format = null, params object[] args)
        {
            Log.Assert(cond, format, args);
        }

        [Conditional(LogConditional.COND_ASSERT)]
        [HideInCallstack]
        public static void Assert(UnityEngine.Object content, bool cond, string format = null, params object[] args)
        {
            Log.Assert(content, cond, format, args);
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(Exception e)
        {
            Log.E(e);
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(string format, params object[] args)
        {
            Log.E(format, args);
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E(UnityEngine.Object content, string format, params object[] args)
        {
            Log.E(content, format, args);
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1>(string format, T1 arg0)
        {
            Log.E(format, arg0);
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1>(UnityEngine.Object content, string format, T1 arg0)
        {
            Log.E(format, arg0);
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1, T2>(string format, T1 arg0, T2 arg1)
        {
            Log.E(format, arg0, arg1);
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1, T2>(UnityEngine.Object content, string format, T1 arg0, T2 arg1)
        {
            Log.E(content, format, arg0, arg1);
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1, T2, T3>(string format, T1 arg0, T2 arg1, T3 arg2)
        {
            Log.E(format, arg0, arg1, arg2);
        }

        [Conditional(LogConditional.COND_ERROR)]
        [HideInCallstack]
        public static void E<T1, T2, T3>(UnityEngine.Object content, string format, T1 arg0, T2 arg1, T3 arg2)
        {
            Log.E(content, format, arg0, arg1, arg2);
        }
    }
#pragma warning disable CS8632
    public ref struct ValueStringBuilder
    {
        private char[]? _arrayToReturnToPool;
        private bool _growable;
        private Span<char> _chars;
        private int _pos;

        public ValueStringBuilder(Span<char> initialBuffer, bool growable = false)
        {
            _arrayToReturnToPool = null;
            _chars = initialBuffer;
            _pos = 0;
            _growable = growable;
        }

        public ValueStringBuilder(int initialCapacity)
        {
            _arrayToReturnToPool = System.Buffers.ArrayPool<char>.Shared.Rent(initialCapacity);
            _chars = _arrayToReturnToPool;
            _pos = 0;
            _growable = true;
        }

        public bool Append(string s)
        {
            if (s == null)
                return true;
            return Append(s.AsSpan());
        }
        public bool Append(char value)
        {
            int pos = _pos;
            if (pos > _chars.Length - 1)
            {
                if (!_growable)
                    return false;
                Grow(1);
            }
            _chars[_pos] = value;
            _pos += 1;
            return true;
        }

        public bool Append(ReadOnlySpan<char> value)
        {
            int pos = _pos;
            if (pos > _chars.Length - value.Length)
            {
                if (!_growable)
                    return false;
                Grow(value.Length);
            }
            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
            return true;
        }

        private void Grow(int additionalCapacityBeyondPos)
        {
            // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative
            int count = (int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), (uint)_chars.Length * 2);
            char[] poolArray = System.Buffers.ArrayPool<char>.Shared.Rent(count);

            _chars.Slice(0, _pos).CopyTo(poolArray);

            char[]? toReturn = _arrayToReturnToPool;
            _chars = _arrayToReturnToPool = poolArray;
            if (toReturn != null)
            {
                System.Buffers.ArrayPool<char>.Shared.Return(toReturn);
            }
        }

        public override string ToString()
        {
            string s = _chars.Slice(0, _pos).ToString();
            Dispose();
            return s;
        }

        public void Dispose()
        {
            char[]? toReturn = _arrayToReturnToPool;
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
            if (toReturn != null)
            {
                System.Buffers.ArrayPool<char>.Shared.Return(toReturn);
            }
        }
    }

}
