/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/19 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public class LoginPlatformMgrImpelment : ILoginPlatformMgr
    {
        private const string CKeyLastLoginType = "platform_last_login_type";

        public event Action<LoginPlatformResult> EventHandler;
        public ILoginPlatform[] _PlatformList = new ILoginPlatform[(int)ELoginPlatform.Max];
        public static List<ELoginPlatformAccount> _TempAccountList = new List<ELoginPlatformAccount>();

        public LoginPlatformMgrImpelment(ILoginPlatform[] platforms)
        {
            if (platforms == null)
            {
                PlatformLog._.Assert(false, "param Platforms is null");
                return;
            }

            foreach (var p in platforms)
            {
                if (p == null)
                {
                    PlatformLog._.Assert(false, "Platform is null");
                    continue;
                }
                ELoginPlatform platform_type = p.GetPlatformType();

                int index = (int)platform_type;
                if (index < 0 || index >= _PlatformList.Length)
                {
                    PlatformLog._.Assert(false, "PlatformType:{0} is out of range", platform_type);
                    continue;
                }

                if (_PlatformList[index] != null)
                {
                    PlatformLog._.E("PlatformType:{0} Register Twice", platform_type);
                    continue;
                }

                _PlatformList[index] = p;
                p.SetEventHandler(_OnPlatformEvent);

                PlatformLog._.D("Added Platform: {0}", platform_type);
                _TempAccountList.Clear();
                p.GetSupportedAccount(_TempAccountList);
                foreach (var p2 in _TempAccountList)
                {
                    PlatformLog._.D("Added Platform: {0}:{1}", platform_type, p2);
                }
            }
        }

        public bool IsSupported(ELoginPlatform platform_type, ELoginPlatformAccount account_type)
        {
            int index = (int)platform_type;
            if (index < 0 || index >= _PlatformList.Length)
                return false;

            var p = _PlatformList[index];
            if (p == null)
                return false;
            return p.IsSupported(account_type);
        }

        public void Destroy()
        {
            EventHandler = null;
            foreach (var p in _PlatformList)
            {
                if (p == null)
                    continue;
                p.Destroy();
            }
            _PlatformList = System.Array.Empty<ILoginPlatform>();
        }

        public void AutoLogin()
        {
            PlatformLog._.D("AutoLogin");
            ILoginPlatform logined_platform = GetLoginedPlatform();

            if (null != GetLoginedPlatform())
            {
                PlatformLog._.I("AutoLogin,已经登陆了");

                LoginPlatformResult result = new LoginPlatformResult();
                logined_platform.GetData(out result.Data);

                result.PlatformType = logined_platform.GetPlatformType();
                result.AccountType = result.Data == null ? ELoginPlatformAccount.None : result.Data.AccountType;
                result.OperationType = ELoginPlatformOperation.AutoLogin;
                result.Result = ELoginPlatformResult.OK;
                result.ErrorMsg = "已经自动登陆了";

                _CallEventHandle(result);
                return;
            }

            if (!_GetLastLoginPlatformType(out var last_login_type))
            {
                LoginPlatformResult result = new LoginPlatformResult();
                result.PlatformType = ELoginPlatform.None;
                result.AccountType = ELoginPlatformAccount.None;
                result.OperationType = ELoginPlatformOperation.AutoLogin;
                result.Result = ELoginPlatformResult.AutoLoginNoRecord;
                result.ErrorMsg = "没有上次的登陆信息,自动登陆失败";
                PlatformLog._.D("AutoLogin, {0}", result.ErrorMsg);
                _CallEventHandle(result);
                return;
            }

            ILoginPlatform platform = GetPlatform(last_login_type);
            if (null == platform)
            {
                LoginPlatformResult result = new LoginPlatformResult();
                result.PlatformType = ELoginPlatform.None;
                result.AccountType = ELoginPlatformAccount.None;
                result.OperationType = ELoginPlatformOperation.AutoLogin;
                result.Result = ELoginPlatformResult.Error;
                result.ErrorMsg = "没有找到上次登陆的信息，自动登陆失败 " + last_login_type;

                PlatformLog._.D("AutoLogin, {0}", result.ErrorMsg);
                _CallEventHandle(result);
                return;
            }

            PlatformLog._.D("AutoLogin, {0}", platform.GetPlatformType());
            platform.AutoLogin();
        }

        public ILoginPlatform GetLoginedPlatform()
        {
            foreach (var a in _PlatformList)
            {
                if (a == null)
                    continue;

                if (a.IsLogined())
                    return a;
            }
            return null;
        }

        public ILoginPlatform GetPlatform(ELoginPlatform platform_type)
        {
            if (platform_type <= ELoginPlatform.None || platform_type >= ELoginPlatform.Max)
            {
                PlatformLog._.Assert(false, "Can't find Platform: {0}", platform_type);
                return null;
            }

            var ret = _PlatformList[(int)platform_type];
            PlatformLog._.Assert(ret != null, "Can't find Platform: {0}", platform_type);
            return ret;
        }

        public void Login(ELoginPlatform platform_type, ELoginPlatformAccount account_type, Dictionary<string, System.Object> param)
        {
            PlatformLog._.D("Login,Platform:{0},AccountType:{1}", platform_type, account_type);

            ILoginPlatform platform = GetLoginedPlatform();
            if (null != platform)
            {
                //同一个Platform, 可以内部切换Account
                if (platform.GetPlatformType() == platform_type)
                {
                    PlatformLog._.Assert(platform.IsSupported(account_type), "Platform:{0}, Unsupport: {1}", platform_type, account_type);
                    platform.Login(account_type, param);
                }
                else //切换Platform必须要先logout
                {
                    LoginPlatformResult result = new LoginPlatformResult();
                    result.PlatformType = platform_type;
                    result.AccountType = account_type;
                    result.OperationType = ELoginPlatformOperation.Login;
                    result.Result = ELoginPlatformResult.Error;
                    result.ErrorMsg = "已经登陆了，现在登陆的平台是 " + platform.GetPlatformType();
                    _CallEventHandle(result);

                    PlatformLog._.D("Login,{0}", result.ErrorMsg);
                }
                return;
            }

            platform = GetPlatform(platform_type);
            if (null == platform || !platform.IsSupported(account_type))
            {
                PlatformLog._.Assert(false, "Can't find,Platform:{0},Account:{1}", platform_type, account_type);

                LoginPlatformResult result = new LoginPlatformResult();
                result.PlatformType = platform_type;
                result.AccountType = account_type;
                result.OperationType = ELoginPlatformOperation.Login;
                result.Result = ELoginPlatformResult.Error;
                result.ErrorMsg = "找不到该平台";
                _CallEventHandle(result);
                return;
            }

            PlatformLog._.D("Login,StartLogin, Platform:{0},AccountType:{1}", platform_type, account_type);
            platform.Login(account_type, param);
        }


        public void Logout()
        {
            PlatformLog._.D("Logout");

            ILoginPlatform platform = GetLoginedPlatform();
            if (null == platform)
            {
                LoginPlatformResult result = new LoginPlatformResult();
                result.PlatformType = ELoginPlatform.None;
                result.AccountType = ELoginPlatformAccount.None;
                result.OperationType = ELoginPlatformOperation.Logout;
                result.Result = ELoginPlatformResult.OK;
                result.ErrorMsg = "当前没有任何登陆的Platform";
                _CallEventHandle(result);
                PlatformLog._.D("Logout,{0}", result.ErrorMsg);
                return;
            }
            PlatformLog._.D("Logout, {0}", platform.GetPlatformType());
            platform.Logout();
        }

        public void AddEventHandler(Action<LoginPlatformResult> handler)
        {
            EventHandler += handler;
        }

        public void RemoveEventHandler(Action<LoginPlatformResult> handler)
        {
            EventHandler -= handler;
        }

        private bool _GetLastLoginPlatformType(out ELoginPlatform last_type)
        {
            if (!PlayerPrefs.HasKey(CKeyLastLoginType))
            {
                last_type = ELoginPlatform.None;
                return false;
            }

            last_type = (ELoginPlatform)PlayerPrefs.GetInt(CKeyLastLoginType);
            PlatformLog._.Assert(!int.TryParse(last_type.ToString(), out var _), "储存的Platform类型有问题:{0}", last_type);
            return true;
        }

        private void _CallEventHandle(LoginPlatformResult data)
        {
            PlatformLog._.D("LoginResult, Platform: {0}, Account: {1}, Operation: {2}, Result: {3}, Msg: {4}", data.PlatformType, data.AccountType, data.OperationType, data.Result, data.ErrorMsg);

            EventHandler?.Invoke(data);
        }

        private void _OnPlatformEvent(LoginPlatformResult data)
        {
            if (data.Result == ELoginPlatformResult.OK)
            {
                switch (data.OperationType)
                {
                    case ELoginPlatformOperation.AutoLogin:
                        //自动登陆不算
                        break;

                    case ELoginPlatformOperation.Login:
                        PlayerPrefs.SetInt(CKeyLastLoginType, (int)data.PlatformType);
                        break;

                    case ELoginPlatformOperation.Logout:
                        PlayerPrefs.DeleteKey(CKeyLastLoginType);
                        break;

                    default:
                        //Do nothing
                        break;
                }
            }
            PlayerPrefs.Save();
            _CallEventHandle(data);
        }
    }
}