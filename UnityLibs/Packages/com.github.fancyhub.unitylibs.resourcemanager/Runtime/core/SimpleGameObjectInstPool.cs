/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/17
 * Title   : 
 * Desc    : 
*************************************************************************************/


using FH.ResManagement;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public sealed class SimpleGameObjectInstPool
    {
        private struct InnerValue
        {
            public GameObject Obj;
            public bool Using;
        }

        private LinkedList<int> _Free = new LinkedList<int>();
        private MyDict<int, InnerValue> _All = new MyDict<int, InnerValue>();

        public GameObject PopFree()
        {
            for (; ; )
            {
                if (!_Free.ExtPopFirst(out var id))
                    return null;

                if (!_All.TryGetValue(id, out var item))
                {
                    //Error
                    continue;
                }

                if (item.Obj != null)
                {
                    item.Using = true;
                    _All[id] = item;
                    ResLog._.D("SimpleGameObjectInstPool, PopFree: {0}", id);
                    return item.Obj;
                }
                _All.Remove(id);
            }
        }

        public void Recycle(GameObject obj)
        {
            if (obj == null)
                return;

            GameObjectPoolUtil.Push2Pool(obj);

            int id = obj.GetInstanceID();
            if (!_All.TryGetValue(id, out var item))
            {
                item.Obj = obj;
                item.Using = false;
                _All.Add(id, item);
                _Free.ExtAddLast(id);

                ResLog._.D("SimpleGameObjectInstPool, Recycle: {0}", id);
                return;
            }

            if (!item.Using)
            {
                ResLog._.E("SimpleGameObjectInstPool, GameObject: {0} 已经是Free, 不能 回收多次", id);
                return;
            }

            item.Using = false;
            _All[id] = item;
            _Free.ExtAddLast(id);
            ResLog._.D("SimpleGameObjectInstPool, Recycle: {0}", id);
        }
    }
}
