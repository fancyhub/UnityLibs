/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH.ResManagement
{
    internal sealed class GameObjectInstItem : UserRefCounter, IPoolItem, IDestroyable
    {
        public static IPool<GameObjectInstItem> S_Pool = GPool.CreatePool<GameObjectInstItem>();

        private IPool ___pool = null;
        private bool ___in_pool = false;
        IPool IPoolItem.Pool { get => ___pool; set => ___pool = value; }
        bool IPoolItem.InPool { get => ___in_pool; set => ___in_pool = value; }

        public string _path;
        public UnityEngine.GameObject _inst;

        public GameObjectInstItem()
        {
        }
        public static GameObjectInstItem Create(
            string path,
            UnityEngine.GameObject inst)
        {
            var ret = S_Pool.New();
            ret._path = path;
            ret._inst = inst;
            return ret;
        }


        public override void Destroy()
        {
            base.Destroy();
            _path = null;
            GoUtil.Destroy(ref _inst);
            S_Pool.Del(this);
        }
    }

    internal class GameObjectInstPool
    {
        public const int C_POOL_CAP = 1000;

        public Dictionary<ResId, GameObjectInstItem> _pool;
        public LruList<ResId, int> _lru_free_list;
        public One2MultiMap<string, ResId> _index_by_path; //free pool
        public GameObjectStat _stat;

        public GameObjectInstPool(GameObjectStat stat)
        {
            _pool = new Dictionary<ResId, GameObjectInstItem>(C_POOL_CAP, new ResId());
            _lru_free_list = new LruList<ResId, int>();
            _index_by_path = new One2MultiMap<string, ResId>();
            _stat = stat;
        }

        public bool AddInst(string path, UnityEngine.GameObject obj, out System.Object res_user, out ResId id)
        {
            //1. 检查
            if (string.IsNullOrEmpty(path) || obj == null)
            {
                ResLog._.Assert(!string.IsNullOrEmpty(path), "path is null {0}", path);
                ResLog._.Assert(obj != null, "实例不能为空 {0}", path);
                id = ResId.Null;
                res_user = null;
                return false;
            }

            id = new ResId(obj, EResType.Inst);

            //3. 创建poolval

            GameObjectInstItem pool_val = GameObjectInstItem.Create(path, obj);

            //4. 向各个pool里面添加
            _stat.AddOne(path);
            _pool.Add(id, pool_val);
            _index_by_path.Add(path, id);
            _lru_free_list.Set(id, UnityEngine.Time.frameCount);
            res_user = pool_val;
            ResLog._.D("{0} add inst {1}", id.Id, path);
            return true;
        }

        public void Destroy()
        {
            _pool.ExtFreeMembers();
            _lru_free_list.Clear();
            _index_by_path.Clear();

            _pool = null;
            _lru_free_list = null;
            _index_by_path = null;
        }

        public int GetFreeCount(string path)
        {
            return _index_by_path.GetCount(path);
        }

        public EResError GetInstPath(ResId id, out string path)
        {
            bool succ = _pool.TryGetValue(id, out GameObjectInstItem pool_val);
            if (!succ)
            {
                path = null;
                return (EResError)EResError.GameObjectInstPool_pool_val_not_found_3;
            }
            path = pool_val._path;
            return EResError.OK;
        }

        public bool GetInstResUser(
            ResId inst_id
            , out string path
            , out UnityEngine.GameObject inst
            , out int ref_count
            , out System.Object user)
        {
            bool succ = _pool.TryGetValue(inst_id, out GameObjectInstItem pool_val);
            if (!succ)
            {
                user = null;
                path = null;
                inst = null;
                ref_count = 0;
                return false;
            }

            user = pool_val;
            inst = pool_val._inst;
            path = pool_val._path;
            ref_count = pool_val.GetUserCount();
            return true;
        }

        public bool RemoveInst(ResId id, out string path)
        {
            //1. 先移除            
            bool succ = _pool.ExtRemove(id, out GameObjectInstItem pool_val);
            if (!succ)
            {
                ResLog._.Assert(false, "移除实例失败 ");
                path = null;
                return false;
            }

            //2. 检查user            
            path = pool_val._path;
            if (pool_val.GetUserCount() > 0)
            {
                _pool.Add(id, pool_val);
                path = null;
                ResLog._.Assert(false, "有人正在使用，不能移除 {0} {1}", id.Id, path);
                return false;
            }

            //3. 从 lru里面移除
            succ = _lru_free_list.Remove(id, out int _);
            ResLog._.Assert(succ, "从lru list 移除失败 {0} {1}", id.Id, path);
            succ = _index_by_path.RemoveVal(id);
            ResLog._.Assert(succ, "从 index_by_path 移除失败 {0} {1}", id.Id, path);

            //4. 获取参数
            pool_val.Destroy();
            _stat.RemoveOne(path);
            return true;
        }

        public UnityEngine.GameObject GetInst(ResId id, bool check_user)
        {
            bool suc = _pool.TryGetValue(id, out GameObjectInstItem pool_val);
            if (!suc)
            {
                ResLog._.Assert(false, "该对象已经被释放了，需要外面持有引用 {0}", id.Id);
                return null;
            }

            if (check_user)
            {
                if (pool_val.GetUserCount() == 0)
                {
                    ResLog._.Assert(false, "该对象已经回收了，需要外面持有引用 {0} {1}", id.Id, pool_val._path);
                    return null;
                }
            }

            return pool_val._inst;
        }

        public EResError PopInst(string path, System.Object user, out ResId id)
        {
            //1. 检查参数
            if (string.IsNullOrEmpty(path))
            {
                ResLog._.Assert(!string.IsNullOrEmpty(path), "路径不能为空");
                id = ResId.Null;
                return (EResError)EResError.GameObjectInstPool_path_null_1;
            }
            if (user == null)
            {
                ResLog._.Assert(user != null, "user 不能为空 {0}", path);
                id = ResId.Null;
                return (EResError)EResError.GameObjectInstPool_user_null_2;
            }

            for (; ; )
            {
                //2. 从free的pool里面弹出一个
                bool suc = _index_by_path.ExtPop(path, out id);
                if (!suc)
                {
                    //不报错误了，因为是可能的
                    return (EResError)EResError.GameObjectInstPool_no_free_inst;
                }

                //3. 设置user                
                suc = _pool.TryGetValue(id, out GameObjectInstItem pool_val);
                ResLog._.Assert(suc, "获取失败 {0}", path);
                ResLog._.Assert(pool_val.GetUserCount() == 0, "错误，有对象的使用情况不一致 {0}", path);
                if (pool_val._inst == null)
                {
                    //等待GC 来销毁
                    ResLog._.Assert(false, "有对象不正常的被销毁了 {0} {1}", id.Id, path);
                    continue;
                }
                pool_val._inst.transform.SetLocalPositionAndRotation(UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
                pool_val._inst.transform.localScale = UnityEngine.Vector3.one;

                pool_val.AddUser(user);
                int user_count = pool_val.GetUserCount();
                ResLog._.D("{0} PopInst: {1} -> {2} {3}", id.Id, user_count - 1, user_count, path);

                //4. 从lru里面移除
                _lru_free_list.Remove(id, out int _);
                break;
            }
            return EResError.OK;
        }

        public EResError AddUsage(ResId id, System.Object user)
        {
            //1. 检查参数
            if (user == null)
            {
                ResLog._.Assert(user != null, "user 不能为空 ");
                return (EResError)EResError.GameObjectInstPool_user_null_1;
            }

            //2. 找到            
            bool suc = _pool.TryGetValue(id, out GameObjectInstItem pool_val);
            if (!suc)
            {
                ResLog._.Assert(false, "找不到对应的实例");
                return (EResError)EResError.GameObjectInstPool_pool_val_not_found_1;
            }

            //3. 判断user list 是否合法
            string path = pool_val._path;
            if (pool_val.GetUserCount() == 0)
            {
                ResLog._.Assert(false, "不能对未pop的对象使用 add inst usage {0}", path);
                return (EResError)EResError.GameObjectInstPool_user_count_zero;
            }

            if (!pool_val.AddUser(user))
            {
                ResLog._.Assert(false, "add inst usage 不能 添加相同的user 两次{0}", path);
                return (EResError)EResError.GameObjectInstPool_user_already_exist;
            }

            int user_count = pool_val.GetUserCount();
            ResLog._.D("{0} IncInstUseCount: {1} -> {2} {3}", id.Id, user_count - 1, user_count, path);
            return EResError.OK;
        }

        public EResError RemoveUser(ResId id, System.Object user)
        {
            //1. 检查参数
            if (user == null)
            {
                ResLog._.Assert(user != null, "user 不能为空 ");
                return (EResError)EResError.GameObjectInstPool_user_null_3;
            }

            //2. 找到            
            bool suc = _pool.TryGetValue(id, out GameObjectInstItem pool_val);
            if (!suc)
            {
                ResLog._.Assert(false, "找不到对应的实例");
                return (EResError)EResError.GameObjectInstPool_pool_val_not_found_2;
            }

            //3. 判断user 是否相同            
            string path = pool_val._path;
            if (!pool_val.RemoveUser(user))
            {
                if (pool_val.GetUserCount() == 0)
                    ResLog._.Assert(false, "该对象已经被回收了，不能回收两次 {0}", path);
                else
                    ResLog._.Assert(false, "使用者不一样 {0}", path);
                return (EResError)EResError.GameObjectInstPool_user_remove_twice;
            }

            int user_count = pool_val.GetUserCount();
            ResLog._.D("{0} DecInstUseCount: {1} -> {2} {3}", id.Id, user_count + 1, user_count, path);

            //4. 如果不是最后一个使用者
            if (user_count > 0)
                return EResError.OK;

            //5. 移除
            GameObjectPoolUtil.Push2Pool(pool_val._inst);
            _index_by_path.Add(path, id);
            _lru_free_list.Set(id, UnityEngine.Time.frameCount);
            return EResError.OK;
        }

        public void GetLruFreeList(List<KeyValuePair<ResId, int>> out_list, bool asc, int max_count)
        {
            _lru_free_list.GetSortedList(out_list, asc, max_count);
        }

        public void RefreshLru(ResId inst_id)
        {
            if (_lru_free_list.TryGetVal(inst_id, out _))
                _lru_free_list.Set(inst_id, UnityEngine.Time.frameCount);
        }
    }
}
