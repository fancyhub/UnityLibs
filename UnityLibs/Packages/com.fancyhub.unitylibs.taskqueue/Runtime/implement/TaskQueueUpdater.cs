/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/12 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Xml.Schema;
using UnityEngine;

namespace FH
{
    internal sealed class TaskQueueUpdater : MonoBehaviour
    {
        internal static TaskQueueUpdater _Inst;
        internal static bool _ManualUpdate;
        internal static CPtr<ITaskQueue> _TaskQueue;

        public static TaskQueueUpdater CreateInst()
        {
            if (_Inst == null)
            {
                GameObject obj = new GameObject(nameof(TaskQueueUpdater));
                _Inst = obj.AddComponent<TaskQueueUpdater>();
                GameObject.DontDestroyOnLoad(obj);
                obj.hideFlags = HideFlags.HideAndDontSave;
            }
            return _Inst;
        }

        public static void CreateInst(ITaskQueue task_queue, bool manual_update)
        {
            CreateInst();
            _ManualUpdate = manual_update;
            _TaskQueue = new CPtr<ITaskQueue>(task_queue);
        }     

        public void Awake()
        {
            _Inst = this;
        }

        public void Update()
        {
            if (!_ManualUpdate)
                _TaskQueue.Val?.Update();
        }

        public void OnDestroy()
        {
            _TaskQueue.Destroy();
        }

        public void OnApplicationQuit()
        {
            _TaskQueue.Destroy();
        }
    }
}
