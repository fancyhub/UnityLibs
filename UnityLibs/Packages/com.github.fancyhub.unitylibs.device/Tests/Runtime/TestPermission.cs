using System.Collections;
using System.Collections.Generic;
using FH;
using UnityEngine;

namespace FH
{
    public class TestPermission : MonoBehaviour
    {
        public UnityEngine.UI.Text _Text;
        public UnityEngine.UI.Button _BtnUpdate;
        public UnityEngine.UI.Button _BtnRequest;
        public UnityEngine.UI.InputField _Input;

        // Start is called before the first frame update
        void Start()
        {
            _BtnRequest.onClick.AddListener(_OnRequestClick);
            _BtnUpdate.onClick.AddListener(_OnUpdateClick);
        }

        private void _OnUpdateClick()
        {
            string permission = _Input.text;
            permission = permission.Trim();
            string msg = $"Check {permission} : {AndroidPermissionUtil.HasPermission(permission)}";
            _Text.text = msg;
            Debug.Log(permission);
        }

        private void _OnRequestClick()
        {
            string permission = _Input.text;
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
            AndroidPermissionUtil.RequestPermission(permission,
            _OnPermissionCallBack,
            dialog1, dialog2);
        }

        private void _OnPermissionCallBack(bool allGranted, string[] grantedList, string[] deniedList)
        {
            Debug.Log($"allGranted: {allGranted} grantedList:{grantedList.Length} deniedList:{deniedList.Length}");

            foreach (var p in grantedList)
            {
                Debug.Log($"grantedList: {p} ");
            }

            foreach (var p in deniedList)
            {
                Debug.Log($"deniedList: {p} ");
            }
        }
    }
}
