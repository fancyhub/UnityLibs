using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using FH.UI;
using System;
using System.Globalization;

namespace Game
{
    public enum EPermissionItem
    {
        AndroidPermission,
        AndroidUnity,
        IOS,
    }

    public class PerissmionItem
    {
        public string Name;
        public EPermissionItem Type;
        public PerissmionItem(string name, EPermissionItem type)
        {
            this.Type = type;
            this.Name = name;
        }
    }

    public class UITestPermissionPage : UIPageBase<UITestPermissionView>, IUIUpdater
    {
        public List<PerissmionItem> _Data = new List<PerissmionItem>();
        public UIListViewSync<PerissmionItem, UITestPermissionItemPage> _ViewSyncer;

        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnClose.OnClick = _OnBtnCloseClick;
            UISceneMgr.AddUpdate(this);

            _Data.Clear();
            _Data.Add(new(UnityEngine.Android.Permission.Microphone, EPermissionItem.AndroidPermission));
            _Data.Add(new(UnityEngine.Android.Permission.Microphone, EPermissionItem.AndroidUnity));


            _ViewSyncer = new UIListViewSync<PerissmionItem, UITestPermissionItemPage>(this, BaseView._ScrollView.content);
            _ViewSyncer.SetData(_Data);
        }

        void IUIUpdater.OnUIUpdate()
        {
        }

        private void _OnBtnCloseClick()
        {
            this.UIClose();
        }
    }

    public class UITestPermissionItemPage : UIPageBase<UIPermissionItemView>, IDataSetter<PerissmionItem>
    {
        private PerissmionItem _Data;

        protected override void OnUI2Open()
        {
            base.OnUI2Open();

            BaseView._BtnRequest.OnClick = _OnRequestClick;
        }

        public void SetData(PerissmionItem data)
        {
            _Data = data;
            _Apply();
        }

        protected override void OnUI3Show()
        {
            base.OnUI3Show();
            _Apply();
        }

        private void _OnRequestClick()
        {
            switch (_Data.Type)
            {
                case EPermissionItem.AndroidUnity:
                    _RequestWithUnity();
                    break;

                case EPermissionItem.AndroidPermission:
                    _RequestWithPermission();
                    break;
            }
        }

        private UnityEngine.Android.PermissionCallbacks _UnityCallBacks;
        private void _RequestWithUnity()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            if (_UnityCallBacks == null)
            {
                _UnityCallBacks = new UnityEngine.Android.PermissionCallbacks();
                _UnityCallBacks.PermissionGranted += _OnUnityPermissionGranted;
                _UnityCallBacks.PermissionDenied += _OnUnityPermissionDenied;
                _UnityCallBacks.PermissionDeniedAndDontAskAgain += _OnUnityPermissionDeniedAndDontAskAgain;
            }

            Debug.Log($"<Permission>: _RequestWithUnity: {_Data.Name} ");
            UnityEngine.Android.Permission.RequestUserPermission(_Data.Name, _UnityCallBacks);
        }

        private void _OnUnityPermissionGranted(string permission)
        {
            Debug.Log($"<Permission>: _OnUnityPermissionGranted: {permission} ");
        }

        private void _OnUnityPermissionDenied(string permission)
        {
            Debug.Log($"<Permission>: _OnUnityPermissionDenied: {permission} ");
        }

        private void _OnUnityPermissionDeniedAndDontAskAgain(string permission)
        {
            Debug.Log($"<Permission>: _OnUnityPermissionDeniedAndDontAskAgain: {permission} ");
        }

        private void _RequestWithPermission()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            string permission = _Data.Name;
            permission = permission.Trim();

            var dialog1 = new AndroidPermissionUtil.AlertDialog()
            {
                Title = "权限1",
                Message = "需要权限1",
                ButtonOK = "OK",
                ButtonCancel = "Cancle",
            };

            var dialog2 = new AndroidPermissionUtil.AlertDialog()
            {
                Title = "权限2",
                Message = "需要权限2",
                ButtonOK = "OK2",
                ButtonCancel = "Cancle2",
            };

            Debug.Log($"<Permission>: _RequestWithPermission: {_Data.Name} ");
            AndroidPermissionUtil.RequestPermission(permission,
            _OnPermissionCallBack,
            dialog1, dialog2);
        }

        private void _OnPermissionCallBack(bool allGranted, string[] grantedList, string[] deniedList)
        {
            Debug.Log($"<Permission>: allGranted: {allGranted} grantedList:{grantedList.Length} deniedList:{deniedList.Length}");

            foreach (var p in grantedList)
            {
                Debug.Log($"<Permission>: grantedList: {p} ");
            }

            foreach (var p in deniedList)
            {
                Debug.Log($"<Permission>: deniedList: {p} ");
            }
        }

        private void _Apply()
        {
            if (BaseView == null || _Data == null)
                return;

            BaseView._Name.text = $"{_Data.Type}: {_Data.Name}";
            switch (_Data.Type)
            {
                case EPermissionItem.AndroidUnity:
                    {
                        bool has = UnityEngine.Android.Permission.HasUserAuthorizedPermission(_Data.Name);
                        BaseView._Status.text = $"{has}";
                    }
                    break;

                case EPermissionItem.AndroidPermission:
                    {
                        bool has = FH.AndroidPermissionUtil.HasPermission(_Data.Name);
                        BaseView._Status.text = $"{has}";
                    }
                    break;
                default:
                    {
                        BaseView._Status.text = "Unkown";
                    }
                    break;
            }
        }
    }
}