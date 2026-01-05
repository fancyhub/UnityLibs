using FH;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FH.Device
{
    public class DevceInfoPanel : MonoBehaviour
    {
        public RectTransform ViewRoot;

        public UnityEngine.UI.Text _Content;
        public UnityEngine.UI.ToggleGroup _ToggleGroup;
        private List<UnityEngine.UI.Toggle> _ToggleList;
        private int _Index = -1;

        // Start is called before the first frame update
        void Start()
        {
            _ToggleList = new List<UnityEngine.UI.Toggle>();
            _ToggleGroup.GetComponentsInChildren(true, _ToggleList);
            foreach (var p in _ToggleList)
            {
                p.onValueChanged.AddListener(_OnToggleClick);
            }

            _Index = 0;
            _OnToggleIndexChanged(_Index);
        }

        private void _OnToggleClick(bool v)
        {
            int index = -1;
            for (int i = 0; i < _ToggleList.Count; i++)
            {
                if (_ToggleList[i].isOn)
                {
                    index = i;
                    break;
                }
            }

            if (_Index == index)
                return;
            _Index = index;
            _OnToggleIndexChanged(_Index);
        }

        private void _OnToggleIndexChanged(int index)
        {
            switch (index)
            {
                case 0:
                    _ShowSystemInfo();
                    break;

                case 1:
                    _ShowCpuInfo();
                    break;
                case 2:
                    _ShowWindowsDeviceInfo();
                    break;

                case 3:
                    _ShowAndroidDeviceInfo();
                    break;
            }
        }

        private void _SetContent(string text)
        {
            _Content.text = text;

            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_Content.rectTransform);
            var content = _Content.rectTransform.parent as RectTransform;
            var size = content.sizeDelta;
            size.y = _Content.rectTransform.rect.size.y + 1000;
            content.sizeDelta = size;
        }


        private void _ShowSystemInfo()
        {
            StringBuilder sb = new StringBuilder();
            var all_props = typeof(SystemInfo).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (var p in all_props)
            {
                Debug.Log(p.Name);
                var value = p.GetValue(null, null);
                if (value != null)
                {
                    sb.Append(p.Name + ": " + value.ToString());
                    sb.Append("\n");
                }
            }

            _SetContent(sb.ToString());
        }

        private void _ShowCpuInfo()
        {
            try
            {
                _SetContent(System.IO.File.ReadAllText("/proc/cpuinfo"));
            }
            catch (Exception ex)
            {
                _SetContent(ex.Message);
            }
        }

        private void _ShowWindowsDeviceInfo()
        {
            StringBuilder sb = new StringBuilder();
            var all_props = typeof(DeviceInfoWindows).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (var p in all_props)
            {
                Debug.Log(p.Name);

                try
                {
                    var value = p.GetValue(null, null);
                    if (value != null)
                    {
                        sb.Append(p.Name + ": " + value.ToString());
                    }
                    else
                    {
                        sb.Append(p.Name + ": Null");
                    }
                    sb.Append("\n");
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            _SetContent(sb.ToString());
        }

        private void _ShowAndroidDeviceInfo()
        {
            StringBuilder sb = new StringBuilder();
            var all_props = typeof(DeviceInfoAndroid).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (var p in all_props)
            {
                Debug.Log(p.Name);

                try
                {
                    var value = p.GetValue(null, null);
                    if (value != null)
                    {
                        sb.Append(p.Name + ": " + value.ToString());
                    }
                    else
                    {
                        sb.Append(p.Name + ": Null");
                    }
                    sb.Append("\n");
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            _SetContent(sb.ToString());


            string v = DeviceInfoAndroid.GetAllSystemProperties();
            foreach (var p in v.Split('\n'))
            {
                Debug.Log(p);
            }
        }
    }
}