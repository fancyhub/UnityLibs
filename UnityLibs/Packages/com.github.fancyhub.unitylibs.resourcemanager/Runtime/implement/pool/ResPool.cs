/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.ResManagement
{
    internal sealed class ResPoolItem : UserRefCounter, IPoolItem, IDestroyable
    {
        public static IPool<ResPoolItem> S_Pool = GPool.CreatePool<ResPoolItem>();
        private IPool ___pool = null;
        private bool ___in_pool = false;
        IPool IPoolItem.Pool { get => ___pool; set => ___pool = value; }
        bool IPoolItem.InPool { get => ___in_pool; set => ___in_pool = value; }

        public string Path;
        public BitEnum32<EResPathType> PathTypeMask;
        public ResId ResId;
        public UnityEngine.Object Asset;
        public CPtr<IResMgr.IExternalRef> AssetRef;

        public ResPoolItem()
        {
        }

        public static ResPoolItem Create(ResPath path, IResMgr.IExternalRef asset_ref)
        {
            var ret = S_Pool.New();
            ret.Path = path.Path;
            ret.PathTypeMask.Clear(false);
            ret.PathTypeMask[path.PathType] = true;
            ret.AssetRef = new CPtr<IResMgr.IExternalRef>(asset_ref);
            ret.Asset = asset_ref.Asset;
            ret.ResId = new ResId(asset_ref.Asset, EResType.Res);

            return ret;
        }

        public bool HasPathType(EResPathType type)
        {
            return PathTypeMask[type];
        }

        public void AddPathType(EResPathType type)
        {
            PathTypeMask[type] = true;
        }

        public bool GetPath(EResPathType type, out ResPath path)
        {
            if (PathTypeMask[type])
            {
                path = ResPath.Create(Path, type);
                return true;
            }
            path = default;
            return false;
        }

        public ResPath GetPath()
        {
            for (var i = EResPathType.Default; i < EResPathType.Max; i++)
            {
                if (PathTypeMask[i])
                    return ResPath.Create(Path, i);
            }
            ResLog._.E("严重错误");
            return ResPath.Empty;
        }

        public override void Destroy()
        {
            Path = default;
            Asset = null;
            base.Destroy();
            S_Pool.Del(this);
        }
    }

    internal class ResPool
    {
        public const int C_POOL_CAP = 1000;

        //Key: Inst Id 
        public Dictionary<int, ResPoolItem> _pool;

        //Path -> Inst Id,  多对一
        public Dictionary<ResPath, ResId> _path_2_index_dict;

        //Key: ResId, Value: TimeFrameCount
        public LruList<ResId, int> _lru_free_list;

        public ResPool()
        {
            _pool = new Dictionary<int, ResPoolItem>(C_POOL_CAP);
            _lru_free_list = new LruList<ResId, int>();
            _path_2_index_dict = new Dictionary<ResPath, ResId>(MyEqualityComparer<ResPath>.Default);
        }

        public void Destroy()
        {
            _pool.ExtFreeMembers();
            _lru_free_list.Clear();
            _path_2_index_dict.Clear();
            _pool = null;
            _lru_free_list = null;
            _path_2_index_dict = null;
        }

        // 资源的部分,key: path
        public void Snapshot(ref List<ResSnapShotItem> out_snapshot)
        {
            //1. check
            if (out_snapshot == null)
            {
                ResLog._.E("参数为空");
                return;
            }

            //2. clear
            out_snapshot.Clear();

            //3.foreach
            foreach (var p in _pool)
            {
                //获取数据
                ResPoolItem pool_val = p.Value;

                //填充数据                
                ResSnapShotItem data = new ResSnapShotItem();
                data.Path = pool_val.Path;
                data.PathType = pool_val.PathTypeMask;
                data.Id = pool_val.ResId;
                data.UserCount = pool_val._user_count;
                pool_val.CopyUserList(ref data.Users);
                out_snapshot.Add(data);
            }
        }

        public EResError GetPathById(int inst_id, out ResPath path)
        {
            bool found = _pool.TryGetValue(inst_id, out ResPoolItem val);
            if (val == null)
            {
                path = default;
                return EResError.ResPool_res_not_exist_6;
            }

            path = val.GetPath();
            return EResError.OK;
        }

        public EResError GetIdByPath(ResPath path, out ResId resId)
        {
            if (!path.IsValid())
            {
                resId = ResId.Null;
                return EResError.ResPool_path_null_1;
            }

            bool succ = _path_2_index_dict.TryGetValue(path, out resId);
            if (succ)
                return EResError.OK;
            return EResError.ResPool_res_not_exist_5;
        }

        public EResError AddRes(ResPath path, IResMgr.IExternalRef asset_ref, out ResId id)
        {
            //1. 检查
            if (!path.IsValid())
            {
                id = ResId.Null;
                return EResError.ResPool_path_null_2;
            }
            if (asset_ref == null || asset_ref.Asset == null)
            {
                id = ResId.Null;
                return EResError.ResPool_res_null;
            }

            if (path.PathType != EResPathType.Default && asset_ref.Asset.GetType() != path.PathType.ExtResPathType2UnityType())
            {
                id = ResId.Null;
                return EResError.ResPool_res_is_not_spec_type;
            }

            id = new ResId(asset_ref.Asset, EResType.Res);

            //2. 根据id 和 路径找对象
            bool result_in_id = _pool.TryGetValue(id.Id, out var exist_item);
            bool result_in_path = _path_2_index_dict.TryGetValue(path, out var exist_res_id);

            //3. 都存在
            if (result_in_id && result_in_path)
            {
                //这个地方的发生: AB模式下, 加载成功之后, 该AB包被卸载了, 然后重新加载该AB包,并加载该资源,会导致 同一个路径,加载出的资源不一样
                if (exist_item.Asset != asset_ref.Asset)
                {
                    ResLog._.Assert(false, "添加资源的时候，资源不一致 {0}", path);
                    id = ResId.Null;
                    return EResError.ResPool_same_path_diff_res;
                }

                if (exist_res_id != id)
                {
                    ResLog._.Assert(false, "资源id 不一致 {0}", path);
                    _path_2_index_dict[path] = id;
                }

                ResLog._.D("res is already in pool {0}", path);
                asset_ref.Destroy(); //不需要两个, 销毁一个
                _lru_free_list.Set(id, UnityEngine.Time.frameCount);
                return EResError.OK;
            }

            //4. id存在, path 不存在
            if (result_in_id && !result_in_path)
            {
                if (exist_item.Path != path.Path) //路径不相同
                {
                    //因为两个路径加载出了同一个资源导致的,可能的原因,比如
                    // 1) : 路径大小写
                    // 2) : AssetLoader 可以通过别名加载资源
                    return EResError.ResPool_multi_path_to_one_res;
                }

                if (exist_item.HasPathType(path.PathType)) //类型不同
                {
                    //严重错误
                    ResLog._.Assert(false, "严重错误 {0}", path.Path);
                    return EResError.ResPool_e3;
                }

                ResLog._.D(" {0} res is already in pool, add an other res type {1},{2}", id, exist_item.GetPath(), path.PathType);
                //类型不一样, 添加一下类型就行了
                _path_2_index_dict[path] = id;
                exist_item.AddPathType(path.PathType);
                asset_ref.Destroy(); //不需要两个, 销毁一个
                _lru_free_list.Set(id, UnityEngine.Time.frameCount);
                return EResError.OK;
            }

            //5. path 存在,id 不存在
            if (!result_in_id && result_in_path)
            {
                ResLog._.Assert(false, "严重错误, 同一个路径加载出来两个不同的 asset,{0}", path.Path);
                return EResError.ResPool_e4;
            }

            //6. 都不存在, 就添加
            ResPoolItem pool_val = ResPoolItem.Create(path, asset_ref);
            ResLog._.D("{0} add res {1}", id.Id, path);
            _pool.Add(id.Id, pool_val);
            _path_2_index_dict.Add(path, id);
            _lru_free_list.Set(id, UnityEngine.Time.frameCount);
            if (path.PathType == EResPathType.Default)
            {
                System.Type assetType = asset_ref.Asset.GetType();
                for (var i = EResPathType.Default + 1; i < EResPathType.Max; i++)
                {
                    if(assetType == i.ExtResPathType2UnityType())
                    {
                        var path_2 = ResPath.Create(path.Path, i);
                        ResLog._.D("{0} add res {1}", id.Id, path_2);
                        _path_2_index_dict.Add(path_2, id);
                    }
                }                 
            }
            return EResError.OK;
        }

        public EResError RemoveRes(int inst_id, out ResPath path, out IResMgr.IExternalRef asset_ref)
        {
            //1. 先从pool里面移除
            bool succ = _pool.ExtRemove(inst_id, out ResPoolItem pool_val);
            if (!succ)
            {
                path = default;
                asset_ref = null;
                return EResError.ResPool_res_not_exist_2;
            }

            //2. 检查 user 列表是否为空       
            path = pool_val.GetPath();
            if (pool_val.GetUserCount() > 0)
            {
                _pool.Add(inst_id, pool_val);
                path = default;
                asset_ref = null;
                ResLog._.Assert(false, "有人正在使用，不能移除 {0}", pool_val.Path);
                return EResError.ResPool_user_count_not_zero;
            }

            //4. 获取返回参数
            asset_ref = pool_val.AssetRef.Val;

            //3. 从lru 以及 index里面删除
            _lru_free_list.Remove(pool_val.ResId, out int _);
            for (var i = EResPathType.Default; i < EResPathType.Max; i++)
            {
                if (pool_val.GetPath(i, out var p1))
                    _path_2_index_dict.Remove(p1);
            }
            pool_val.Destroy();
            return EResError.OK;
        }

        public UnityEngine.Object GetRes(int inst_id)
        {
            bool suc = _pool.TryGetValue(inst_id, out ResPoolItem pool_val);
            if (!suc)
            {
                ResLog._.Assert(false, "找不到资源 {0}", inst_id);
                return null;
            }
            ResLog._.Assert(pool_val.Asset != null, "资源为空 {0}", inst_id);
            return pool_val.Asset;
        }

        public void GetLruFreeList(List<KeyValuePair<ResId, int>> out_list, bool asc, int max_count)
        {
            _lru_free_list.GetSortedList(out_list, asc, max_count);
        }


        public EResError AddUser(ResPath path, System.Object user, out ResId out_res_id)
        {
            EResError err = GetIdByPath(path, out out_res_id);
            ResLog._.ErrCode(err, path.Path);
            if (err != EResError.OK)
                return err;

            return AddUser(out_res_id.Id, user);
        }

        public EResError AddUser(int inst_id, System.Object user)
        {
            //1. 检查
            if (user == null)
            {
                return EResError.ResPool_user_null_3;
            }

            //2. 根据id 找到poolval            
            bool suc = _pool.TryGetValue(inst_id, out ResPoolItem pool_val);
            if (!suc)
                return EResError.ResPool_res_not_exist_3;


            //3. 获取user list并添加
            if (!pool_val.AddUser(user))
            {
                return EResError.ResPool_addusage_user_exist;
            }
            int user_count = pool_val.GetUserCount();
            ResLog._.D("{0} IncResUseCount: {1} -> {2} {3}", inst_id, user_count - 1, user_count, pool_val.Path);
            //4. 刷新lru，从里面删除
            _lru_free_list.Remove(pool_val.ResId, out int _);
            return EResError.OK;
        }

        public EResError RemoveUser(ResPath path, System.Object user, out ResId out_res_id)
        {
            if (user == null)
            {
                ResLog._.Assert(false, "RemoveUser User is null {0}", path);
                out_res_id = ResId.Null;
                return EResError.ResPool_user_null_1;
            }

            EResError err = GetIdByPath(path, out out_res_id);
            if (err != 0)
                return EResError.ResPool_res_not_exist;

            return RemoveUser(out_res_id.Id, user);
        }

        public EResError RemoveUser(int inst_id, System.Object user)
        {
            //1. 检查
            if (user == null)
            {
                ResLog._.Assert(false, "RemoveUser User is null ");
                return EResError.ResPool_user_null_2;
            }

            //2. 根据id 找到poolval            
            bool suc = _pool.TryGetValue(inst_id, out ResPoolItem pool_val);
            if (!suc)
            {
                ResLog._.Assert(false, "严重错误，该对象找不到，可能被释放了");
                return EResError.ResPool_res_not_exist_4;
            }

            //3. 移除user
            if (!pool_val.RemoveUser(user))
            {
                return EResError.ResPool_user_not_exist;
            }

            int user_count = pool_val.GetUserCount();

            ResLog._.D("{0} DecResUseCount: {1} -> {2} {3}", inst_id, user_count + 1, user_count, pool_val.Path);

            //4. 更新lru
            if (user_count == 0)
                _lru_free_list.Set(pool_val.ResId, UnityEngine.Time.frameCount);
            return EResError.OK;
        }

        public void RefreshLru(ResId res_id)
        {
            if (_lru_free_list.TryGetVal(res_id, out _))
                _lru_free_list.Set(res_id, UnityEngine.Time.frameCount);
        }
    }
}
