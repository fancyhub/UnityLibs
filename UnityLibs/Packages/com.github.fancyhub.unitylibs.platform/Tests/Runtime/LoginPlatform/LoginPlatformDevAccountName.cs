/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/19 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;

namespace FH
{
    /// <summary>
    /// 这个用户自己输入用户名
    /// </summary>
    public class LoginPlatformDevAccountName : ILoginPlatform
    {
        public const string CParamKeyNickName = "nick_name";
        public delegate void OpenUIInput(string input_val, Action<string> callback);

        private const ELoginPlatform CPlatform = ELoginPlatform.Dev;
        private const ELoginPlatformAccount CAccount = ELoginPlatformAccount.Dev_AccountName;
        private const long CExpire = long.MaxValue;
        private const string CSavedKeyLastLoginName = "PF_DEV_ACCOUNT_NAME";

        private Action<LoginPlatformResult> _EventHandler;
        private bool _IsLogined = false;
        private LoginPlatformData _LoginData;
        private Dictionary<string, System.Object> _ExtraData = new Dictionary<string, System.Object>();
        private OpenUIInput _InputBox;

        public LoginPlatformDevAccountName(OpenUIInput input_box)
        {
            _LoginData = new LoginPlatformData();
            _LoginData.PlatformType = CPlatform;
            _LoginData.AccountType = CAccount;
            _LoginData.ExpireTime = CExpire;
            _LoginData.Extra = _ExtraData;
            _InputBox = input_box;
        }
        public void GetSupportedAccount(List<ELoginPlatformAccount> out_account_types)
        {
            out_account_types.Add(CAccount);
        }
        public void Destroy()
        {
            _EventHandler = null;
        }

        public void AutoLogin()
        {
            _Login(ELoginPlatformOperation.AutoLogin);
        }

        public void Bind()
        {
            //Do nothing
        }

        public bool GetData(out LoginPlatformData data)
        {
            data = _LoginData;
            if (IsLogined())
                return true;
            return false;
        }

        public ELoginPlatform GetPlatformType()
        {
            return CPlatform;
        }

        public bool IsLogined()
        {
            return _IsLogined;
        }

        public void Login(ELoginPlatformAccount account_type, Dictionary<string, System.Object> param)
        {
            _Login(ELoginPlatformOperation.Login);
        }

        public void Logout()
        {
            if (_IsLogined)
            {
                _IsLogined = false;

                LoginPlatformResult result = new LoginPlatformResult();
                result.OperationType = ELoginPlatformOperation.Logout;
                result.PlatformType = CPlatform;
                result.AccountType = CAccount;
                result.Result = ELoginPlatformResult.OK;
                result.ErrorMsg = "成功Logout";
                _FireEvent(result, 0);
                return;
            }


            {
                LoginPlatformResult result = new LoginPlatformResult();
                result.OperationType = ELoginPlatformOperation.Logout;
                result.PlatformType = CPlatform;
                result.AccountType = CAccount;
                result.Result = ELoginPlatformResult.OK;
                result.ErrorMsg = "已经是 logout 状态了";
                _FireEvent(result, 0);
                return;
            }
        }

        public void SetEventHandler(Action<LoginPlatformResult> handler)
        {
            _EventHandler = handler;
        }

        private void _Login(ELoginPlatformOperation op)
        {
            if (IsLogined())
            {
                LoginPlatformResult result = new LoginPlatformResult();
                result.OperationType = op;
                result.PlatformType = CPlatform;
                result.AccountType = CAccount;
                result.Result = ELoginPlatformResult.OK;
                result.ErrorMsg = "已经登陆了";
                result.Data = _LoginData;
                _FireEvent(result, 0);
                return;
            }

            switch (op)
            {
                case ELoginPlatformOperation.AutoLogin:
                    {
                        var last_name = _GetLastLoginName();
                        if (!string.IsNullOrEmpty(last_name))
                        {
                            LoginPlatformResult result = new LoginPlatformResult();
                            result.OperationType = op;
                            result.PlatformType = CPlatform;
                            result.AccountType = CAccount;
                            result.Result = ELoginPlatformResult.OK;
                            result.ErrorMsg = "";
                            result.Data = _LoginData;
                            _LoginData.OpenId = last_name;
                            _LoginData.Token = "dev_account_name" + last_name + "_" + TimeUtil.Unix.ToString();
                            _ExtraData[CParamKeyNickName] = last_name;
                            _IsLogined = true;
                            _FireEvent(result, 0);
                        }
                        else
                        {
                            LoginPlatformResult result = new LoginPlatformResult();
                            result.OperationType = op;
                            result.PlatformType = CPlatform;
                            result.AccountType = CAccount;
                            result.Result = ELoginPlatformResult.Error;
                            result.ErrorMsg = "";
                            _FireEvent(result, 0);
                        }
                    }
                    break;

                case ELoginPlatformOperation.Login:
                    {
                        string last_name = _GetLastLoginName();
                        Action<string> call_back = _OnUIResult;
                        _InputBox(last_name, call_back);
                    }
                    break;
                default:
                    break;
            }
        }

        public bool IsSupported(ELoginPlatformAccount account_type)
        {
            return account_type == CAccount;
        }

        private static string _GetLastLoginName()
        {
            return PlayerPrefs.GetString(CSavedKeyLastLoginName, null);
        }

        private void _OnUIResult(string user_name)
        {
            LoginPlatformResult result = new LoginPlatformResult();
            result.OperationType = ELoginPlatformOperation.Login;
            result.PlatformType = CPlatform;
            result.AccountType = CAccount;

            if (!string.IsNullOrEmpty(user_name))
            {
                result.Result = ELoginPlatformResult.OK;
                result.Data = _LoginData;
                _LoginData.OpenId = user_name;
                _LoginData.Token = "nick_name_" + user_name + "_" + TimeUtil.Unix;
                _ExtraData[CParamKeyNickName] = user_name;
                PlayerPrefs.SetString(CSavedKeyLastLoginName, user_name);
                _IsLogined = true;
            }
            else
            {
                result.Result = ELoginPlatformResult.Cancel;
                _IsLogined = false;
            }
            _EventHandler?.Invoke(result);
        }

        private void _FireEvent(LoginPlatformResult result, int wait_count)
        {
            if (wait_count <= 0)
            {
                _EventHandler?.Invoke(result);
                return;
            }

            TaskQueue.StartCoroutine(_wait_fire(result, wait_count));
        }

        private System.Collections.IEnumerator _wait_fire(LoginPlatformResult result, int wait_count)
        {
            for (int i = 0; i < wait_count; i++)
                yield return null;

            _EventHandler?.Invoke(result);
        }
    }
}
