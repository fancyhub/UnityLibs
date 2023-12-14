/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17
 * Title   : 
 * Desc    : 
*************************************************************************************/

namespace FH.AssetBundleBuilder.Ed
{
    public static class BuilderLog
    {
        public static void Log(string msg)
        {
            UnityEngine.Debug.Log(msg);
        }

        public static void Log(string msg, params System.Object[] args)
        {
            UnityEngine.Debug.LogFormat(msg, args);
        }

        public static void Warning(string msg)
        {
            UnityEngine.Debug.LogWarning(msg);
        }

        public static void Warning(string msg, params System.Object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(msg, args);
        }

        public static void Error(string msg, params System.Object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(msg, args);
        }

        public static void Error(string msg)
        {
            UnityEngine.Debug.Log(msg);
        }

        public static void Assert(bool cond)
        {
            UnityEngine.Debug.Assert(cond);
        }

        public static void AssertFormat(bool cond, string msg, params System.Object[] args)
        {
            UnityEngine.Debug.AssertFormat(cond, msg, args);
        }
    }
}
