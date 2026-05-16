/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/12 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;

namespace FH
{
    internal sealed class TaskQueueUpdater : MonoBehaviour
    {
        private static TaskQueueUpdater _Inst;
        private static bool _ManualUpdate;
        private static ITaskQueue _TaskQueue;

        private static TaskQueueUpdater CreateInst()
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
            _TaskQueue = task_queue;
        }     

        public void Awake()
        {
            _Inst = this;
        }

        public void Update()
        {
            if (!_ManualUpdate)
                _TaskQueue?.Update();
        }
    }
}
