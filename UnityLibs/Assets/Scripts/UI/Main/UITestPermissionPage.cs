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
        Android,
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

    public class UITestPermissionPage : UIPageBase<UITestPermissionView>
    {
        public List<PerissmionItem> _Data = new List<PerissmionItem>();
        public IListDataSetter<PerissmionItem> _ViewSyncer;

        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnClose.OnClick = _OnBtnCloseClick;

            _Data.Clear();
            if(Application.platform == RuntimePlatform.Android)
            {
                string[] permissions = FH.AndroidDeviceInfo.GetSelfPackageInfoPermissions();
                foreach(var permission in permissions)
                {
                    _Data.Add(new(permission, EPermissionItem.Android));
                }
            }

            _ViewSyncer = new UIListViewSyncer<PerissmionItem>(_ItemPageCreator);
            _ViewSyncer.SetData(_Data);
        }

        public IListViewSyncerViewer<PerissmionItem> _ItemPageCreator()
        {
            var openInfo = FH.UI.PageOpenInfo.CreateDefaultSubPage(BaseView._ScrollView.content, ResHolder);
            var ret= OpenChildPage<UITestPermissionItemPage>(openInfo);
            return ret;
        }

        private void _OnBtnCloseClick()
        {
            this.UIClose();
        }
    }

    public class UITestPermissionItemPage : UIPageBase<UIPermissionItemView>, IListViewSyncerViewer<PerissmionItem>
    {
        private PerissmionItem _Data;

        protected override void OnUI2Open()
        {
            base.OnUI2Open();

            BaseView._BtnRequest.OnClick = _OnRequestClick;
            BaseView._BtnRequest2.OnClick = _OnRequestClick2;
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
                case EPermissionItem.Android:
                    _RequestAndroidPermissionWithUnity();
                    break; 
            }
        }

        private void _OnRequestClick2()
        {
            switch (_Data.Type)
            {
                case EPermissionItem.Android:
                    _RequestAndroidPermissionWithJava();
                    break;
            }            
        }

        private UnityEngine.Android.PermissionCallbacks _UnityCallBacks;
        private void _RequestAndroidPermissionWithUnity()
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

            Log.I($"<Permission>: _RequestWithUnity: {_Data.Name} ");
            UnityEngine.Android.Permission.RequestUserPermission(_Data.Name, _UnityCallBacks);
        }

        private void _OnUnityPermissionGranted(string permission)
        {
            Log.I($"<Permission>: _OnUnityPermissionGranted: {permission} ");

            _Apply();
        }

        private void _OnUnityPermissionDenied(string permission)
        {
            Log.I($"<Permission>: _OnUnityPermissionDenied: {permission} ");
            _Apply();
        }

        private void _OnUnityPermissionDeniedAndDontAskAgain(string permission)
        {
            Log.I($"<Permission>: _OnUnityPermissionDeniedAndDontAskAgain: {permission} ");
            _Apply();
        }

        private void _RequestAndroidPermissionWithJava()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            string permission = _Data.Name;
            permission = permission.Trim();

            var dialog1 = new AndroidPermissionUtil.AlertDialog()
            {
                Title = $"{_Data.Name} 1",
                Message = $"Need {_Data.Name} 1",
                ButtonOK = "OK",
                ButtonCancel = "Cancle",
            };

            var dialog2 = new AndroidPermissionUtil.AlertDialog()
            {
                Title = $"{_Data.Name} 2",
                Message = $"Need {_Data.Name} 2",
                ButtonOK = "OK2",
                ButtonCancel = "Cancle2",
            };

            Log.I($"<Permission>: _RequestWithPermission: {_Data.Name} ");
            AndroidPermissionUtil.RequestPermission(permission,
            _OnAndroidPermissionCallBack,
            dialog1, dialog2);
        }

        private void _OnAndroidPermissionCallBack(bool allGranted, string[] grantedList, string[] deniedList)
        {
            _Apply();
            Log.I($"<Permission>: allGranted: {allGranted} grantedList:{grantedList.Length} deniedList:{deniedList.Length}");

            foreach (var p in grantedList)
            {
                Log.I($"<Permission>: grantedList: {p} ");
            }

            foreach (var p in deniedList)
            {
                Log.I($"<Permission>: deniedList: {p} ");
            }
        }

        private void _Apply()
        {
            if (BaseView == null || _Data == null)
                return;

            BaseView._Name.text = $"{_Data.Type}: {_Data.Name}";
            switch (_Data.Type)
            {
                case EPermissionItem.Android:
                    {
                        bool has = UnityEngine.Android.Permission.HasUserAuthorizedPermission(_Data.Name);
                        BaseView._Status.text = $"{has}";

                        BaseView._BtnRequest2.Visible = true;
                    }
                    break;

                case EPermissionItem.IOS:
                    BaseView._BtnRequest2.Visible = false;
                    //UnityEngine.Application.HasUserAuthorization()
                    break;
                     
                default:
                    {
                        BaseView._Status.text = "Unkown";
                        BaseView._BtnRequest2.Visible = false;
                    }
                    break;
            }
        }
    }
}