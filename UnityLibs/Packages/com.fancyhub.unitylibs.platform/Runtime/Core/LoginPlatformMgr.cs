/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/19 
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;

namespace FH
{
    public enum ELoginPlatformResult
    {
        OK,
        Cancel, //用户取消登陆        
        NetworkError,//网络错误
        NeedInstallApp, //没有对应的app
        AutoLoginNoRecord,    //自动登录的时候发现没有历史记录
        TimeOut, //超时
        Error, //其他错误统统称为 error
    }

    public enum ELoginPlatformOperation
    {
        AutoLogin,
        Login,
        Logout,
        Bind
    }

    public sealed class LoginPlatformData
    {
        //平台类型
        public ELoginPlatform PlatformType;

        //有些平台会有多个 登陆渠道
        public ELoginPlatformAccount AccountType;

        public string OpenId;
        public string Token;

        //过期时间
        public long ExpireTime;


        public Dictionary<string, System.Object> Extra;
    }

    public struct LoginPlatformResult
    {
        public ELoginPlatformResult Result;

        //平台类型
        public ELoginPlatform PlatformType;

        //有些平台会有多个 登陆渠道
        public ELoginPlatformAccount AccountType;

        //操作的类型
        public ELoginPlatformOperation OperationType;

        //错误消息
        public string ErrorMsg;
        public System.Object ExtraErrorData;

        public LoginPlatformData Data;
    }

    public interface ILoginPlatform
    {
        ELoginPlatform GetPlatformType();

        void AutoLogin();

        void Login(ELoginPlatformAccount account_type, Dictionary<string, System.Object> param);

        void Logout();

        bool IsLogined();

        void Bind();

        bool GetData(out LoginPlatformData data);

        void SetEventHandler(Action<LoginPlatformResult> handler);

        public bool IsSupported(ELoginPlatformAccount account_type);

        public void GetSupportedAccount(List<ELoginPlatformAccount> out_account_types);

        void Destroy();
    }


    public interface ILoginPlatformMgr
    {
        public void AutoLogin();

        public void Login(ELoginPlatform platform_type, ELoginPlatformAccount account_type, Dictionary<string, System.Object> param = null);

        public void Logout();

        public ILoginPlatform GetLoginedPlatform();

        //根据类型获取登陆的平台，如果不支持，就返回null
        public ILoginPlatform GetPlatform(ELoginPlatform platform_type);

        public bool IsSupported(ELoginPlatform platform_type, ELoginPlatformAccount account_type);


        public void AddEventHandler(Action<LoginPlatformResult> handler);

        public void RemoveEventHandler(Action<LoginPlatformResult> handler);

        public void Destroy();
    }


    public static class LoginPlatformMgr
    {
        private static ILoginPlatformMgr _Mgr;
        public static void Init(params ILoginPlatform[] platforms)
        {
            if (_Mgr != null)
            {
                PlatformLog._.E("LoginPlatformMgr Inited, can't init twice");
                return;
            }

            PlatformLog._.D("LoginPlatformMgr Init Start");
            _Mgr = new LoginPlatformMgrImpelment(platforms);
            PlatformLog._.D("LoginPlatformMgr Init Done");
        }

        public static bool IsSupport(ELoginPlatform platform_type, ELoginPlatformAccount account_type)
        {
            if (_Mgr == null)
            {
                PlatformLog._.E("LoginPlatformMgr Is Null");
                return false;
            }
            return _Mgr.IsSupported(platform_type, account_type);
        }

        public static void Destroy()
        {
            if (_Mgr == null)
            {
                PlatformLog._.D("LoginPlatformMgr Is Null");
                return;
            }
            var mgr = _Mgr;
            _Mgr = null;
            mgr.Destroy();
        }

        public static void AutoLogin()
        {
            if (_Mgr == null)
            {
                PlatformLog._.E("LoginPlatformMgr Is Null");
                return;
            }
            _Mgr.AutoLogin();
        }

        public static void Login(ELoginPlatform platform_type, ELoginPlatformAccount account_type, Dictionary<string, System.Object> param = null)
        {
            if (_Mgr == null)
            {
                PlatformLog._.E("LoginPlatformMgr Is Null");
                return;
            }
            _Mgr.Login(platform_type, account_type, param);
        }

        public static void Logout()
        {
            if (_Mgr == null)
            {
                PlatformLog._.E("LoginPlatformMgr Is Null");
                return;
            }
            _Mgr.Logout();
        }

        public static ILoginPlatform GetLoginedPlatform()
        {
            if (_Mgr == null)
            {
                PlatformLog._.E("LoginPlatformMgr Is Null");
                return null;
            }
            return _Mgr.GetLoginedPlatform();
        }

        public static ILoginPlatform GetPlatform(ELoginPlatform platform_type)
        {
            if (_Mgr == null)
            {
                PlatformLog._.E("LoginPlatformMgr Is Null");
                return null;
            }
            return _Mgr.GetPlatform(platform_type);
        }

        public static bool IsSupported(ELoginPlatform platform_type, ELoginPlatformAccount account_type)
        {
            if (_Mgr == null)
            {
                PlatformLog._.E("LoginPlatformMgr Is Null");
                return false;
            }
            return _Mgr.IsSupported(platform_type, account_type);
        }

        public static LoginPlatformData GetLoginedData()
        {
            if (_Mgr == null)
            {
                PlatformLog._.E("LoginPlatformMgr Is Null");
                return null;
            }

            ILoginPlatform platform = GetLoginedPlatform();
            if (null == platform)
                return null;

            if (platform.GetData(out var ret))
                return ret;
            return null;
        }

        public static void AddEventHandler(Action<LoginPlatformResult> handler)
        {
            if (_Mgr == null)
            {
                PlatformLog._.E("LoginPlatformMgr Is Null");
                return;
            }
            _Mgr.AddEventHandler(handler);
        }

        public static void RemoveEventHandler(Action<LoginPlatformResult> handler)
        {
            if (_Mgr == null)
            {
                PlatformLog._.D("LoginPlatformMgr Is Null");
                return;
            }
            _Mgr.RemoveEventHandler(handler);
        }
    }
}
