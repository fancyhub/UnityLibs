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
        public BitEnum32<EAssetPathType> PathTypeMask;
        public ResId ResId;
        public UnityEngine.Object Asset;
        public CPtr<IResMgr.IExternalAssetRef> AssetRef;

        public bool _update_flag;

        public ResPoolItem()
        {
        }

        public static ResPoolItem Create(AssetPath path, IResMgr.IExternalAssetRef asset_ref)
        {
            var ret = S_Pool.New();
            ret.Path = path.Path;
            ret.PathTypeMask.Clear(false);
            ret.PathTypeMask[path.PathType] = true;
            ret.AssetRef = new CPtr<IResMgr.IExternalAssetRef>(asset_ref);
            ret.Asset = asset_ref.Asset;
            ret.ResId = new ResId(asset_ref.Asset, EResType.Asset);
            ret._update_flag = false;

            return ret;
        }

        public bool HasPathType(EAssetPathType type)
        {
            return PathTypeMask[type];
        }

        public void AddPathType(EAssetPathType type)
        {
            PathTypeMask[type] = true;
        }

        public bool GetPath(EAssetPathType type, out AssetPath path)
        {
            if (PathTypeMask[type])
            {
                path = AssetPath.Create(Path, type);
                return true;
            }
            path = default;
            return false;
        }

        public AssetPath GetPath()
        {
            for (var i = EAssetPathType.Default; i < EAssetPathType.Max; i++)
            {
                if (PathTypeMask[i])
                    return AssetPath.Create(Path, i);
            }
            ResLog._.E("严重错误");
            return AssetPath.Empty;
        }

        public override void Destroy()
        {
            var asset = Asset;
            Path = default;
            Asset = null;
            AssetRef.Destroy();
            base.Destroy();

            S_Pool.Del(this);

            if (!(asset is GameObject))
            {
                Resources.UnloadAsset(asset);
            }
        }
    }

    internal class AssetPool : IResourcePool
    {
        public const int C_POOL_CAP = 1000;

        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        //Key: Inst Id 
        public Dictionary<int, ResPoolItem> _pool;

        //Path -> Inst Id,  多对一
        public Dictionary<AssetPath, ResId> _path_2_index_dict;

        //Key: ResId, Value: TimeFrameCount
        public LruList<ResId, int> _lru_free_list;

        public AssetPool()
        {
            _pool = new Dictionary<int, ResPoolItem>(C_POOL_CAP);
            _lru_free_list = new LruList<ResId, int>();
            _path_2_index_dict = new Dictionary<AssetPath, ResId>(MyEqualityComparer<AssetPath>.Default);
        }

        public void Destroy()
        {
            ___obj_ver++;
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

            foreach (var p in _pool)
            {
                //获取数据
                ResPoolItem pool_val = p.Value;

                //填充数据                
                ResSnapShotItem data = new ResSnapShotItem();
                data.Id = pool_val.ResId.Id;
                data.ResType = EResType.Asset;
                data.UpdateFlag = pool_val._update_flag;
                data.Path = pool_val.Path;
                data.PathTypeMask = pool_val.PathTypeMask;

                data.UserCount = pool_val._user_count;
                pool_val.CopyUserList(ref data.Users);

                out_snapshot.Add(data);
            }
        }

        /*
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
        //*/

        public EResError GetIdByPath(AssetPath path, out ResId resId)
        {
            if (!path.IsValid())
            {
                resId = ResId.Null;
                return EResError.AssetPool_path_null_1;
            }

            bool succ = _path_2_index_dict.TryGetValue(path, out resId);
            if (succ)
                return EResError.OK;
            return EResError.AssetPool_asset_not_exist_5;
        }

        public EResError AddAsset(AssetPath path, IResMgr.IExternalAssetRef asset_ref, out ResId id)
        {
            //1. 检查
            if (!path.IsValid())
            {
                id = ResId.Null;
                return EResError.AssetPool_path_null_2;
            }
            if (asset_ref == null || asset_ref.Asset == null)
            {
                id = ResId.Null;
                return EResError.AssetPool_asset_null;
            }

            if (path.PathType != EAssetPathType.Default && asset_ref.Asset.GetType() != path.PathType.ExtAssetPathType2UnityType())
            {
                id = ResId.Null;
                return EResError.AssetPool_asset_is_not_spec_type;
            }

            id = new ResId(asset_ref.Asset, EResType.Asset);

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
                    return EResError.AssetPool_same_path_diff_asset;
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
                    return EResError.AssetPool_multi_path_to_one_asset;
                }

                if (exist_item.HasPathType(path.PathType)) //类型不同
                {
                    //严重错误
                    ResLog._.Assert(false, "严重错误 {0}", path.Path);
                    return EResError.AssetPool_e3;
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
                return EResError.AssetPool_e4;
            }

            //6. 都不存在, 就添加
            ResPoolItem pool_val = ResPoolItem.Create(path, asset_ref);
            ResLog._.D("Res: {0} Res Add {1}", id.Id, path);
            _pool.Add(id.Id, pool_val);
            _path_2_index_dict.Add(path, id);
            _lru_free_list.Set(id, UnityEngine.Time.frameCount);
            if (path.PathType == EAssetPathType.Default)
            {
                System.Type assetType = asset_ref.Asset.GetType();
                for (var i = EAssetPathType.Default + 1; i < EAssetPathType.Max; i++)
                {
                    if (assetType == i.ExtAssetPathType2UnityType())
                    {
                        var path_2 = AssetPath.Create(path.Path, i);
                        ResLog._.D("{0} add res {1}", id.Id, path_2);
                        _path_2_index_dict.Add(path_2, id);
                    }
                }
            }
            return EResError.OK;
        }

        public EResError RemoveAsset(int inst_id, out AssetPath path)
        {
            //1. 先从pool里面移除
            bool succ = _pool.ExtRemove(inst_id, out ResPoolItem pool_val);
            if (!succ)
            {
                path = default;
                return EResError.AssetPool_asset_not_exist_2;
            }

            //2. 检查 user 列表是否为空       
            path = pool_val.GetPath();
            if (pool_val.GetUserCount() > 0)
            {
                _pool.Add(inst_id, pool_val);
                path = default;
                ResLog._.Assert(false, "有人正在使用，不能移除 {0}", pool_val.Path);
                return EResError.AssetPool_user_count_not_zero;
            }

            //3. 从lru 以及 index里面删除
            _lru_free_list.Remove(pool_val.ResId, out int _);
            if (!pool_val._update_flag)
            {
                for (var i = EAssetPathType.Default; i < EAssetPathType.Max; i++)
                {
                    if (pool_val.GetPath(i, out var p1))
                        _path_2_index_dict.Remove(p1);
                }
            }

            ResLog._.D("Res: {0} Res Remove, {1}", pool_val.ResId.Id, pool_val.Path);
            pool_val.Destroy();
            return EResError.OK;
        }

        public UnityEngine.Object Get(ResId res_id)
        {
            bool suc = _pool.TryGetValue(res_id.Id, out ResPoolItem pool_val);
            if (!suc)
            {
                ResLog._.Assert(false, "找不到资源 {0}", res_id);
                return null;
            }
            ResLog._.Assert(pool_val.Asset != null, "资源为空 {0}", res_id);
            return pool_val.Asset;
        }

        public T Get<T>(ResId id) where T : UnityEngine.Object
        {
            var obj = Get(id);
            if (obj == null)
                return null;
            T ret = obj as T;
            ResLog._.Assert(ret != null, "类型不对，当前类型 {0}, 你要的 {1}", obj.GetType(), typeof(T));
            return ret;
        }

        public void GetLruFreeList(List<KeyValuePair<ResId, int>> out_list, bool asc, int max_count)
        {
            _lru_free_list.GetSortedList(out_list, asc, max_count);
        }

        public EResError AddUser(ResId res_id, System.Object user)
        {
            //1. 检查
            if (user == null)
            {
                return EResError.AssetPool_user_null_3;
            }

            //2. 根据id 找到poolval            
            bool suc = _pool.TryGetValue(res_id.Id, out ResPoolItem pool_val);
            if (!suc)
                return EResError.AssetPool_asset_not_exist_3;
            if (pool_val._update_flag)
                return EResError.AssetPool_item_is_old_canot_add_user;

            //3. 获取user list并添加
            if (!pool_val.AddUser(user))
            {
                return EResError.AssetPool_addusage_user_exist;
            }
            int user_count = pool_val.GetUserCount();
            ResLog._.D("Res: {0} Res UseCount Inc: {1} -> {2} {3}", res_id.Id, user_count - 1, user_count, pool_val.Path);
            //4. 刷新lru，从里面删除
            _lru_free_list.Remove(pool_val.ResId, out int _);
            return EResError.OK;
        }

        public EResError RemoveUser(ResId res_id, System.Object user)
        {
            //1. 检查
            if (user == null)
            {
                ResLog._.Assert(false, "RemoveUser User is null ");
                return EResError.AssetPool_user_null_2;
            }

            //2. 根据id 找到poolval            
            bool suc = _pool.TryGetValue(res_id.Id, out ResPoolItem pool_val);
            if (!suc)
            {
                ResLog._.Assert(false, "严重错误，该对象找不到，可能被释放了");
                return EResError.AssetPool_asset_not_exist_4;
            }

            //3. 移除user
            if (!pool_val.RemoveUser(user))
            {
                return EResError.AssetPool_user_not_exist;
            }

            int user_count = pool_val.GetUserCount();

            ResLog._.D("Res: {0} Res UseCount Dec: {1} -> {2} {3}", res_id.Id, user_count + 1, user_count, pool_val.Path);

            //4. 更新lru
            if (user_count == 0)
            {
                if (pool_val._update_flag) //旧资源, 直接移除
                    RemoveAsset(res_id.Id, out _);
                else
                    _lru_free_list.Set(pool_val.ResId, UnityEngine.Time.frameCount);
            }

            return EResError.OK;
        }

        public EResError TransferUser(ResId res_id, System.Object old_user, System.Object new_user)
        {
            return EResError.AssetPool_dont_support_transfer_user;
        }

        public void RefreshLru(ResId res_id)
        {
            if (_lru_free_list.TryGetVal(res_id, out _))
                _lru_free_list.Set(res_id, UnityEngine.Time.frameCount);
        }

        public void OnUpgradeSucc()
        {
            List<int> temp = new List<int>(_lru_free_list.Count);

            foreach (var p in _pool)
            {
                p.Value._update_flag = true;
                if (p.Value._user_count == 0)
                    temp.Add(p.Key);
            }

            foreach (var p in temp)
            {
                RemoveAsset(p, out _);
            }

            ResLog._.Assert(_lru_free_list.Count == 0, "res pool count is not 0, {0}", _lru_free_list.Count);

            _path_2_index_dict.Clear();//清除路径对inst的映射
        }
    }
}
