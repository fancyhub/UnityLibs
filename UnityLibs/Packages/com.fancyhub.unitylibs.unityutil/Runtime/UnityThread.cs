/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/9/5
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public class UnityThread
    {
        private const string Name = "UnityThread";

        public static void RunInUpdate(Action action)
        {
            var inst = _GetInst();
            if (inst == null)
                return;

            inst.AddToUpdate(action);
        }


        [UnityEngine.ExecuteAlways]
        private class UnityThreadBehaviour : MonoBehaviour
        {
            private static List<Action> _UpdateActionsPending = new List<Action>();
            private static volatile bool _HasUpdateActions = false;

            private List<Action> _UpdateActionsExecuting = new List<Action>();

            public void AddToUpdate(Action action)
            {
                if (action == null)
                    return;
                lock (_UpdateActionsPending)
                {
                    _UpdateActionsPending.Add(action);
                    _HasUpdateActions = true;
                }
            }

            public void Update()
            {
                if (!_HasUpdateActions)
                    return;

                _UpdateActionsExecuting.Clear();

                lock (_UpdateActionsPending)
                {
                    _UpdateActionsExecuting.AddRange(_UpdateActionsPending);
                    _UpdateActionsPending.Clear();
                    _HasUpdateActions = false;
                }

                foreach (Action a in _UpdateActionsExecuting)
                {
                    a.Invoke();
                }
                _UpdateActionsExecuting.Clear();
            }
        }
        private static UnityThreadBehaviour _Inst;
        private static UnityThreadBehaviour _GetInst()
        {
            if (_Inst != null)
                return _Inst;


            if (UnityEngine.Application.isPlaying)
            {
                GameObject obj = new GameObject(Name);
                _Inst = obj.AddComponent<UnityThreadBehaviour>();
                obj.hideFlags = HideFlags.HideAndDontSave;
            }
            else
            {
                GameObject obj = new GameObject(Name);
                _Inst = obj.AddComponent<UnityThreadBehaviour>();
                obj.hideFlags = HideFlags.HideAndDontSave;
                GameObject.DontDestroyOnLoad(obj);
            }
            return _Inst;
        }
                
    }
}
