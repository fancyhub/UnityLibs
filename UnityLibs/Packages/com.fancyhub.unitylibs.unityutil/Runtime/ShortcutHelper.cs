/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/8/8
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace FH
{
#if UNITY_EDITOR
    [System.Serializable]
#endif
    public class KeyCodeDetector
    {
#if UNITY_EDITOR
        public bool _Enable = false;
        private bool _LastEnable = false;
        private DateTime _StartTime;

        public List<KeyCode> _Result = new();
#endif
        [Conditional("UNITY_EDITOR")]
        public void Update()
        {
#if UNITY_EDITOR
            if (!_Enable)
                return;

            if (!_LastEnable)
            {
                _LastEnable = true;
                _StartTime = DateTime.Now;
                _Result.Clear();
            }

            var dt = DateTime.Now - _StartTime;
            if (dt.TotalMilliseconds > 5000)
            {
                _Enable = false;
                _LastEnable = false;
                return;
            }

            foreach (var p in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown((KeyCode)p))
                {
                    _Result.Add((KeyCode)p);
                }
            }
#endif
        }
    }

    public class ShortcutHelper : MonoBehaviour
    {
        public KeyCodeDetector KeyCodeDetector;

        [System.Serializable]
        public class ShortcutKeyCode
        {
            public KeyCode[] KeyCodes;
            public UnityEngine.Events.UnityEvent Events;

            public void Update()
            {
                if (KeyCodes == null || KeyCodes.Length == 0)
                    return;

                foreach (var p in KeyCodes)
                {
                    if (!Input.GetKeyDown(p))
                        return;
                }

                Events.Invoke();
            }
        }
        [System.Serializable]
        public class ShortcutTouchCount
        {
            public int TouchCount;
            public UnityEngine.Events.UnityEvent Events;
            public void Update()
            {
                if (TouchCount <= 0)
                    return;
                if(Input.touchCount==TouchCount)
                    Events.Invoke();
            }
        }

        public List<ShortcutKeyCode> ShortcutKeyCodes = new();
        public List<ShortcutTouchCount> ShortcutTouchCounts = new();

        public void Update()
        {
            KeyCodeDetector?.Update();

            foreach (var p in ShortcutKeyCodes)
            {
                p.Update();
            }

            foreach (var p in ShortcutTouchCounts)
            {
                p.Update();
            }
        }
    }
}
