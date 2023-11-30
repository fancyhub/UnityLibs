/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.Res
{
    internal sealed class EmptyGameObjectItem : UserRefCounter, IPoolItem, IDestroyable
    {
        public static IPool<EmptyGameObjectItem> S_Pool = GPool.CreatePool<EmptyGameObjectItem>();
        private IPool ___pool = null;
        private bool ___in_pool = false;
        IPool IPoolItem.Pool { get => ___pool; set => ___pool = value; }
        bool IPoolItem.InPool { get => ___in_pool; set => ___in_pool = value; }

        public UnityEngine.GameObject Res;

        public EmptyGameObjectItem()
        {
        }
        public static EmptyGameObjectItem Create(GameObject obj)
        {
            var ret = S_Pool.New();
            ret.Res = obj;
            return ret;
        }

        public override void Destroy()
        {
            GoUtil.Destroy(ref Res);
            base.Destroy();
            S_Pool.Del(this);
        }
    }

    //只有一个节点的gameobject的对象池
    internal class EmptyGameObjectPool
    {
        public const int C_CAP = 200; 

        public LinkedList<int> _free;
        public Dictionary<int, EmptyGameObjectItem> _all;

        public EmptyGameObjectPool()
        {
            _free = new LinkedList<int>();
            _all = new Dictionary<int, EmptyGameObjectItem>();
        }

        public void Destroy()
        {
            _all.ExtFreeMembers();
            _free.ExtClear();
        }

        public bool Contain(GameObject obj)
        {
            if (null == obj)
                return false;
            int id = obj.GetInstanceID();
            return _all.ContainsKey(id);
        }

        public bool Contain(int id)
        {
            return _all.ContainsKey(id);
        }

        public EResError Get(int id, out GameObject obj)
        {
            bool has = _all.TryGetValue(id, out var item);
            if (!has)
            {
                obj = null;
                return (EResError)EResError.EmptyGameObjectPool_id_not_exist;
            }

            if (item.GetUserCount() == 0)
            {
                obj = null;
                return (EResError)EResError.EmptyGameObjectPool_id_is_not_in_using;
            }

            obj = item.Res;
            if (obj == null)
            {
                ResLog._.Assert(false, "Empty 对象被不正常的销毁了");
                item.Destroy();
                _all.Remove(id);
                return (EResError)EResError.EmptyGameObjectPool_obj_destroy_outer;
            }
            return EResError.OK;
        }

        public EResError Create(System.Object user, out ResId res_id)
        {
            if (user == null)
            {
                res_id = default;
                return (EResError)EResError.EmptyGameObjectPool_user_null;
            }

            //1. 从free 里面获取
            for (; ; )
            {
                //1.1 如果为空，break
                bool found = _free.ExtPopFirst(out int inst_id);
                if (!found)
                    break;

                //1.2 获取free的id，以及对应的gameobject
                bool has = _all.TryGetValue(inst_id, out var item);
                ResLog._.Assert(has);

                //1.3 检查gameobject 是否已经被销毁了
                if (null == item.Res)
                {
                    //1.4 如果已经被销毁了，要处理一下
                    item.Destroy();
                    _all.Remove(inst_id);
                }
                GameObject obj = item.Res;
                item.AddUser(user);
                obj.SetActive(true);
                obj.ExtResetTran();

                res_id = new ResId(obj, EResType.EmptyInst);
                return EResError.OK;
            }

            //2. 如果没有，直接创建一个
            {
                GameObject obj = new GameObject(string.Empty);
                obj.transform.SetParent(GoPoolUtil.GetDummyInactive(), false);
                var item = EmptyGameObjectItem.Create(obj);

                int inst_id = obj.GetInstanceID();
                _all.Add(inst_id, item);

                res_id = new ResId(obj, EResType.EmptyInst);
                return EResError.OK;
            }
        }

        public EResError AddUser(int id, System.Object user)
        {
            if (user == null)
                return (EResError)EResError.EmptyGameObjectPool_user_null;

            _all.TryGetValue(id, out var item);
            if (item == null)
                return (EResError)EResError.EmptyGameObjectPool_id_not_exist;

            if (item.GetUserCount() == 0)
                return (EResError)EResError.EmptyGameObjectPool_id_is_not_in_using;

            if (item.AddUser(user))
                return EResError.OK;
            return (EResError)EResError.EmptyGameObjectPool_user_already_exist;
        }

        public EResError RemoveUser(int id, System.Object user)
        {
            if (user == null)
                return (EResError)EResError.EmptyGameObjectPool_user_null;

            _all.TryGetValue(id, out var item);
            if (item == null)
                return (EResError)EResError.EmptyGameObjectPool_id_not_exist;

            if (item.GetUserCount() == 0)
                return (EResError)EResError.EmptyGameObjectPool_id_is_not_in_using;

            if (!item.RemoveUser(user))
                return (EResError)EResError.EmptyGameObjectPool_user_not_exist;

            if (item.GetUserCount() > 0)
                return EResError.OK;

            //4. 加到free 队列里面
            GoPoolUtil.Push2Pool(item.Res);
            _free.ExtAddLast(id);
#if DEBUG
            if(item.Res!=null)
                item.Res.name = string.Empty;
#endif
            return EResError.OK;
        }       
    }
}
