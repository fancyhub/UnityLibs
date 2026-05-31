/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    internal static class DebugConnectionMainThread
    {
        private static readonly List<Action> _Pending = new List<Action>();
        private static readonly List<Action> _Executing = new List<Action>();
        private static bool _Initialized;
        private static Runner _Runner;

#if UNITY_EDITOR
        private static bool _EditorUpdateRegistered;
#endif

        public static void Initialize()
        {
            if (_Initialized)
                return;

            _Initialized = true;

#if UNITY_EDITOR
            if (!_EditorUpdateRegistered)
            {
                UnityEditor.EditorApplication.update += Update;
                _EditorUpdateRegistered = true;
            }

            if (!Application.isPlaying)
                return;
#endif

            if (_Runner != null)
                return;

            GameObject obj = new GameObject(nameof(DebugConnectionMainThread));
            obj.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(obj);
            _Runner = obj.AddComponent<Runner>();
        }

        public static void Post(Action action)
        {
            if (action == null)
                return;

            lock (_Pending)
            {
                _Pending.Add(action);
            }
        }

        public static void Update()
        {
            lock (_Pending)
            {
                if (_Pending.Count == 0)
                    return;

                _Executing.AddRange(_Pending);
                _Pending.Clear();
            }

            foreach (Action action in _Executing)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            _Executing.Clear();
        }

        private sealed class Runner : MonoBehaviour
        {
            private void Update()
            {
                DebugConnectionMainThread.Update();
            }
        }
    }
}
