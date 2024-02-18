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
    /// 这个用设备号来登陆
    /// </summary>
    public class LoginPlatformDeviceGuid : ILoginPlatform
    {
        public const string CParamKeyNickName = "nick_name";

        private const ELoginPlatform CPlatform = ELoginPlatform.Device;
        private const ELoginPlatformAccount CAccount = ELoginPlatformAccount.Device_GUID;
        private const string CSavedKeyLastLoginID = "PF_DEVICE_ID";
        private const int CWaitFrameCount = 5;
        private const long CExpire = long.MaxValue;

        private Action<LoginPlatformResult> _EventHandler;
        private Dictionary<string, System.Object> _ExtraData = new Dictionary<string, System.Object>();
        private bool _IsLogined = false;
        private LoginPlatformData _LoginData;

        public LoginPlatformDeviceGuid()
        {
            _LoginData = new LoginPlatformData();
            _LoginData.PlatformType = CPlatform;
            _LoginData.AccountType = CAccount;
            _LoginData.ExpireTime = CExpire;
            _LoginData.Extra = _ExtraData;
        }

        public void Destroy()
        {
            _EventHandler = null;
        }

        public void AutoLogin()
        {
            _Login(ELoginPlatformOperation.AutoLogin);
        }

        public void GetSupportedAccount(List<ELoginPlatformAccount> out_account_types)
        {
            out_account_types.Add(CAccount);
        }

        public void Bind()
        {
            //Do nothing
        }

        public bool IsSupported(ELoginPlatformAccount account_type)
        {
            return account_type == CAccount;
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

        public void Login(ELoginPlatformAccount account_type, Dictionary<string, System.Object> param = null)
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

            string cust_id = _GetLastLoginId();
            if (string.IsNullOrEmpty(cust_id))
            {
                LoginPlatformResult result = new LoginPlatformResult();
                result.OperationType = op;
                result.PlatformType = CPlatform;
                result.AccountType = CAccount;
                result.Result = ELoginPlatformResult.Error;
                result.ErrorMsg = "账号错误";
                result.Data = _LoginData;
                _FireEvent(result, CWaitFrameCount);
                return;
            }

            _LoginData.OpenId = cust_id;
            _LoginData.Token = cust_id;
            _ExtraData[CParamKeyNickName] = cust_id;
            _IsLogined = true;

            {
                LoginPlatformResult result = new LoginPlatformResult();
                result.OperationType = op;
                result.PlatformType = CPlatform;
                result.AccountType = CAccount;
                result.Result = ELoginPlatformResult.OK;
                result.ErrorMsg = "成功";
                result.Data = _LoginData;
                _FireEvent(result, CWaitFrameCount);
            }
        }

        private string _GetLastLoginId()
        {
            string ret = PlayerPrefs.GetString(CSavedKeyLastLoginID, null);

            if (string.IsNullOrEmpty(ret))
            {
                //ret = SystemInfo.deviceUniqueIdentifier;
                ret = Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString(CSavedKeyLastLoginID, ret);
            }

            return ret;
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
