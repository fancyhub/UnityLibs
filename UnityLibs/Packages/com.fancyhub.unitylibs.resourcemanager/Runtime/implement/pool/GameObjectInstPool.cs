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
    internal struct GameObjInstUser
    {
#if UNITY_EDITOR
        public System.Object _user;
#endif
        public bool _in_use;

        public bool AddUser(System.Object user)
        {
            if (user == null)
                return false;
            if (_in_use)
                return false;
            _in_use = true;

#if UNITY_EDITOR
            _user = user;
#endif
            return true;
        }

        public bool RemoveUser(System.Object user)
        {
            if (user == null)
                return false;
            if (!_in_use)
                return false;
#if UNITY_EDITOR
            if (_user != user)
            {
                ResLog._.Assert(false, "移除的user和当前user 不相等");
                return false;
            }
#endif
            _in_use = false;
            return true;
        }
    }



    internal sealed class GameObjectInstItem : CPoolItemBase
    {
        public const int C_WAIT_FOR_USE_FRAME = 2;

        public ResRef _res_ref;
        public ResId _inst_id;
        public UnityEngine.GameObject _unity_inst;
        public GameObjInstUser _user;
        public bool _wait_for_use;
        public int _wait_for_use_frame_expire;
        public bool _upgrade_flag; //更新之前的标记位

        public GameObjectInstItem()
        {
        }
        public static GameObjectInstItem Create(
            ResRef res_ref,
            UnityEngine.GameObject inst)
        {
            var ret = GPool.New<GameObjectInstItem>();
            ret._res_ref = res_ref;
            ret._unity_inst = inst;
            ret._user = default;
            ret._inst_id = new ResId(inst, EResType.Inst);
            ret._wait_for_use_frame_expire = UnityEngine.Time.frameCount + C_WAIT_FOR_USE_FRAME;
            ret._wait_for_use = true;
            ret._upgrade_flag = false;
            return ret;
        }

        public bool TransferUser(System.Object old_user, System.Object new_user)
        {
            var old_status = Status;
            if (old_status != EGameObjInstStatus.InUse)
            {
                ResLog._.E("{0},当前不是Use状态,{1}", _inst_id.Id, _res_ref.Path);
                return false;
            }

            if (this._user.RemoveUser(old_user) && this._user.AddUser(new_user))
                return true;
            return false;
        }

        public bool MoveWait2Free()
        {
            var old_status = Status;
            if (old_status != EGameObjInstStatus.WaitForUse)
            {
                ResLog._.E("{0},当前不是Wait状态,{1}", _inst_id.Id, _res_ref.Path);
                return false;
            }

            this._wait_for_use = false;
            this._wait_for_use_frame_expire = int.MaxValue;
            this._user = default;

            ResLog._.D("Inst: {0} Inst StatusChange, {1}->{2}, {3}", _inst_id.Id, old_status, Status, _res_ref.Path);
            return true;
        }

        public bool MoveWait2Use(System.Object user)
        {
            var old_status = Status;
            if (old_status != EGameObjInstStatus.WaitForUse)
            {
                ResLog._.E("{0},当前不是Wait状态,{1}, {2}", _inst_id.Id, old_status, _res_ref.Path);
                return false;
            }

            if (!this._user.AddUser(user))
            {
                ResLog._.E("{0}, add user failed, {1},{2}", _inst_id.Id, old_status, _res_ref.Path);
                return false;
            }

            this._wait_for_use = false;
            this._wait_for_use_frame_expire = int.MaxValue;


            ResLog._.D("Inst: {0} Inst StatusChange, {1}->{2}, {3}", _inst_id.Id, old_status, Status, _res_ref.Path);
            return true;
        }

        public bool MoveUse2Free(System.Object user)
        {
            var old_status = Status;
            if (old_status != EGameObjInstStatus.InUse)
            {
                ResLog._.E("{0},当前不是Use状态,{1}, {2}", _inst_id.Id, old_status, _res_ref.Path);
                return false;
            }

            if (!this._user.RemoveUser(user))
            {
                ResLog._.E("{0},remove user failed, {1},{2}", _inst_id.Id, old_status, _res_ref.Path);
                return false;
            }

            this._wait_for_use = false;
            this._wait_for_use_frame_expire = int.MaxValue;

            ResLog._.D("Inst: {0} Inst StatusChange, {1}->{2}, {3}", _inst_id.Id, old_status, Status, _res_ref.Path);
            return true;
        }

        public bool MoveFree2Wait()
        {
            var old_status = Status;
            if (old_status != EGameObjInstStatus.Free)
            {
                ResLog._.E("{0},当前不是free 状态,{1}, {2}", _inst_id.Id, old_status, _res_ref.Path);
                return false;
            }

            this._wait_for_use = true;
            this._wait_for_use_frame_expire = UnityEngine.Time.frameCount + C_WAIT_FOR_USE_FRAME;

            ResLog._.D("Inst: {0} Inst StatusChange, {1}->{2}, {3}", _inst_id.Id, old_status, Status, _res_ref.Path);
            return true;
        }

        public bool IsInUse => _user._in_use;
        public EGameObjInstStatus Status
        {
            get
            {
                if (_user._in_use)
                    return EGameObjInstStatus.InUse;
                if (_wait_for_use)
                    return EGameObjInstStatus.WaitForUse;
                return EGameObjInstStatus.Free;
            }
        }

        protected override void OnPoolRelease()
        {
            _res_ref.RemoveUser(this);
            _res_ref = default;
            _user = default;
            _wait_for_use = false;
            _inst_id = default;

            GoUtil.Destroy(ref _unity_inst);
        }
    }

    internal class GameObjectInstPool : IResInstPool
    {
        public const int C_POOL_CAP = 1000;
        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;


        public Dictionary<int, GameObjectInstItem> _all;
        public LinkedList<CPtr<GameObjectInstItem>> _wait_for_use;
        public LruList<ResId, int> _lru_free_list;
        public One2MultiMap<string, ResId> _free_index_by_path; //free pool
        public GameObjectStat _stat;

        public GameObjectInstPool(GameObjectStat stat)
        {
            _all = new Dictionary<int, GameObjectInstItem>(C_POOL_CAP);
            _wait_for_use = new();
            _lru_free_list = new LruList<ResId, int>();
            _free_index_by_path = new One2MultiMap<string, ResId>();
            _stat = stat;
        }

        public bool AddInst(ResRef res_ref, UnityEngine.GameObject obj, out ResId inst_id)
        {
            //1. 检查
            if (!res_ref.IsValid() || obj == null)
            {
                ResLog._.Assert(res_ref.IsValid(), "res ref is invalid {0}", res_ref);
                ResLog._.Assert(obj != null, "实例不能为空 {0}", res_ref);
                inst_id = ResId.Null;
                return false;
            }

            inst_id = new ResId(obj, EResType.Inst);

            //3. 创建poolval

            GameObjectInstItem pool_val = GameObjectInstItem.Create(res_ref, obj);

            //4. 向各个pool里面添加
            int count = _stat.AddOne(res_ref.Path);
            _all.Add(inst_id.Id, pool_val);
            _wait_for_use.ExtAddLast(pool_val);
            res_ref.AddUser(pool_val);
            ResLog._.D("Inst: {0} Inst Create {1},Count:{2} {3}", inst_id.Id, pool_val.Status, count, res_ref.Path);
            return true;
        }

        public void Update()
        {
            var now = UnityEngine.Time.frameCount;
            var node = _wait_for_use.First;
            for (; ; )
            {
                if (node == null)
                    break;
                var cur_node = node;
                node = node.Next;

                var item = cur_node.Value.Val;
                if (item == null || item.Status != EGameObjInstStatus.WaitForUse)
                {
                    cur_node.ExtRemoveFromList();
                    continue;
                }

                if (now <= item._wait_for_use_frame_expire)
                    break;

                if (!item.MoveWait2Free())
                    continue;

                _lru_free_list.Set(item._inst_id, now);
                cur_node.ExtRemoveFromList();

                if (item._upgrade_flag)
                {
                    RemoveInst(item._inst_id, out _);
                }
                else
                {
                    _free_index_by_path.Add(item._res_ref.Path, item._inst_id);
                }
            }
        }

        public bool RemoveInst(ResId inst_id, out string path)
        {
            //1. 先移除            
            bool succ = _all.ExtRemove(inst_id.Id, out GameObjectInstItem pool_val);
            if (!succ)
            {
                ResLog._.Assert(false, "移除实例失败 ");
                path = null;
                return false;
            }

            //2. 检查user
            path = pool_val._res_ref.Path;
            var status = pool_val.Status;
            if (status != EGameObjInstStatus.Free)
            {
                _all.Add(inst_id.Id, pool_val);
                path = null;
                ResLog._.Assert(false, "有人正在使用，不能移除 {0} {1}", inst_id.Id, path);
                return false;
            }


            //3. 从 lru里面移除           
            succ = _lru_free_list.Remove(inst_id, out int _);
            ResLog._.Assert(succ, "从lru list 移除失败 {0} {1}", inst_id.Id, path);
            succ = _free_index_by_path.RemoveVal(inst_id);
            ResLog._.Assert(succ, "从 index_by_path 移除失败 {0} {1}", inst_id.Id, path);

            //4. 获取参数
            pool_val.Destroy();
            int count = _stat.RemoveOne(path);
            ResLog._.D("Inst: {0} Inst Destroy, {1},Count:{2} {3}", inst_id.Id, status, count, path);
            return true;
        }

        public EResError TransferUser(ResId inst_id, System.Object old_user, System.Object new_user)
        {
            bool succ = _all.TryGetValue(inst_id.Id, out GameObjectInstItem pool_val);
            if (!succ)
                return (EResError)EResError.GameObjectInstPool_pool_val_not_found_4;

            if (old_user == null || new_user == null || old_user == new_user)
                return EResError.GameObjectInstPool_user_null_4;

            if (!pool_val.TransferUser(old_user, new_user))
                return EResError.GameObjectInstPool_user_null_5;
            return EResError.OK;
        }

        public void Destroy()
        {
            ___obj_ver++;
            _all.ExtFreeMembers();
            _lru_free_list.Clear();
            _free_index_by_path.Clear();

            _all = null;
            _lru_free_list = null;
            _free_index_by_path = null;
        }

        public int GetFreeCount(string path)
        {
            return _free_index_by_path.GetCount(path);
        }

        public EResError GetInstPath(ResId inst_id, out string path)
        {
            bool succ = _all.TryGetValue(inst_id.Id, out GameObjectInstItem pool_val);
            if (!succ)
            {
                path = null;
                return (EResError)EResError.GameObjectInstPool_pool_val_not_found_3;
            }
            path = pool_val._res_ref.Path;
            return EResError.OK;
        }

        public bool GetInstResUser(
            ResId inst_id
            , out string path
            , out UnityEngine.GameObject inst)
        {
            bool succ = _all.TryGetValue(inst_id.Id, out GameObjectInstItem pool_val);
            if (!succ)
            {
                path = null;
                inst = null;
                return false;
            }

            inst = pool_val._unity_inst;
            path = pool_val._res_ref.Path;
            return true;
        }

        public UnityEngine.Object Get(ResId inst_id)
        {
            return GetInst(inst_id, true);
        }

        public T Get<T>(ResId inst_id) where T : UnityEngine.Object
        {
            var obj = GetInst(inst_id, true);
            if (obj == null)
                return null;
            T ret = obj as T;
            ResLog._.Assert(ret != null, "类型不对，当前类型 {0}, 你要的 {1}", obj.GetType(), typeof(T));
            return ret;
        }

        public UnityEngine.GameObject GetInst(ResId inst_id, bool check_user)
        {
            bool suc = _all.TryGetValue(inst_id.Id, out GameObjectInstItem pool_val);
            if (!suc)
            {
                ResLog._.Assert(false, "该对象已经被释放了，需要外面持有引用 {0}", inst_id.Id);
                return null;
            }

            if (check_user)
            {
                if (pool_val.Status != EGameObjInstStatus.InUse)
                {
                    ResLog._.Assert(false, "该对象没有user，需要外面持有引用 {0} {1} {2}", inst_id.Id, pool_val.Status, pool_val._res_ref.Path);
                    return null;
                }
            }

            return pool_val._unity_inst;
        }

        public EResError PopInst(string path, out ResId inst_id)
        {
            //1. 检查参数
            if (string.IsNullOrEmpty(path))
            {
                ResLog._.Assert(!string.IsNullOrEmpty(path), "路径不能为空");
                inst_id = ResId.Null;
                return (EResError)EResError.GameObjectInstPool_path_null_1;
            }

            for (; ; )
            {
                //2. 从free的pool里面弹出一个
                bool suc = _free_index_by_path.ExtPop(path, out inst_id);
                if (!suc)
                {
                    //不报错误了，因为是可能的
                    return (EResError)EResError.GameObjectInstPool_no_free_inst;
                }

                //3. 设置user                
                suc = _all.TryGetValue(inst_id.Id, out GameObjectInstItem pool_val);
                ResLog._.Assert(suc, "获取失败 {0}", path);
                ResLog._.Assert(pool_val.Status == EGameObjInstStatus.Free, "错误，有对象的使用情况不一致 {0},{1}", path, pool_val.Status);
                if (pool_val._unity_inst == null)
                {
                    //等待GC 来销毁
                    ResLog._.Assert(false, "有对象不正常的被销毁了 {0} {1}", inst_id.Id, path);
                    continue;
                }

                if (!pool_val.MoveFree2Wait())
                    continue;

                //4. 从lru里面移除
                _wait_for_use.ExtAddLast(pool_val);
                _lru_free_list.Remove(inst_id, out int _);

                break;
            }
            return EResError.OK;
        }

        public EResError AddUser(ResId inst_id, System.Object user)
        {
            //1. 检查参数
            if (user == null)
            {
                ResLog._.Assert(user != null, "user 不能为空 ");
                return (EResError)EResError.GameObjectInstPool_user_null_1;
            }

            //2. 找到            
            bool suc = _all.TryGetValue(inst_id.Id, out GameObjectInstItem pool_val);
            if (!suc)
            {
                ResLog._.Assert(false, "找不到对应的实例");
                return (EResError)EResError.GameObjectInstPool_pool_val_not_found_1;
            }

            //3. 判断user list 是否合法
            if (!pool_val.MoveWait2Use(user))
            {
                return (EResError)EResError.GameObjectInstPool_canot_add_user_2_nowait_inst;
            }


            pool_val._unity_inst.ExtResetTran();

            return EResError.OK;
        }

        public EResError RemoveUser(ResId inst_id, System.Object user)
        {
            //1. 检查参数
            if (user == null)
            {
                ResLog._.Assert(user != null, "user 不能为空 ");
                return (EResError)EResError.GameObjectInstPool_user_null_3;
            }

            //2. 找到            
            bool suc = _all.TryGetValue(inst_id.Id, out GameObjectInstItem pool_val);
            if (!suc)
            {
                ResLog._.Assert(false, "找不到对应的实例");
                return (EResError)EResError.GameObjectInstPool_pool_val_not_found_2;
            }

            //3.  移到free
            if (!pool_val.MoveUse2Free(user))
            {
                return (EResError)EResError.GameObjectInstPool_user_remove_twice;
            }
            GameObjectPoolUtil.Push2Pool(pool_val._unity_inst);
            _free_index_by_path.Add(pool_val._res_ref.Path, inst_id);
            _lru_free_list.Set(inst_id, UnityEngine.Time.frameCount);

            if (pool_val._upgrade_flag) // 资源更新前的对象, 立刻清除
                RemoveInst(pool_val._inst_id, out var _);
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

        public void OnUpgradeSucc()
        {
            List<ResId> list = new List<ResId>(_lru_free_list.Count);

            foreach (var p in _all)
            {
                p.Value._upgrade_flag = true;
                if (p.Value.Status == EGameObjInstStatus.Free)
                    list.Add(p.Value._inst_id);
            }

            foreach (var p in list)
            {
                RemoveInst(p, out _);
            }

            ResLog._.Assert(_lru_free_list.Count == 0, "inst lru list is not 0, {0}", _lru_free_list.Count);
            ResLog._.Assert(_free_index_by_path.Count == 0, "inst index_by_path is not 0,{0}", _free_index_by_path.Count);
        }


        public void Snapshot(ref List<ResSnapShotItem> out_snapshot)
        {
            foreach (var p in _all)
            {
                var item = p.Value;
                ResSnapShotItem data = new ResSnapShotItem();
                data.Id = item._inst_id.Id;
                data.ResType = EResType.Inst;
                data.UpdateFlag = item._upgrade_flag;
                data.Path = item._res_ref.Path;
                data.PathTypeMask = new BitEnum32<EResPathType>(EResPathType.Default);
                data.UserCount = item.Status == EGameObjInstStatus.InUse ? 1 : 0;
                data.InstStatus = item.Status;

                out_snapshot.Add(data);
            }
        }
    }
}
