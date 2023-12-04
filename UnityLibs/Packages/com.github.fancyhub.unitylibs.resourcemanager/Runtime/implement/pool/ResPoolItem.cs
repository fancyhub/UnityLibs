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
    internal sealed class ResPoolItem : UserRefCounter, IPoolItem, IDestroyable
    {
        public static IPool<ResPoolItem> S_Pool = GPool.CreatePool<ResPoolItem>();
        private IPool ___pool = null;
        private bool ___in_pool = false;
        IPool IPoolItem.Pool { get => ___pool; set => ___pool = value; }
        bool IPoolItem.InPool { get => ___in_pool; set => ___in_pool = value; }

        public ResPath Path;
        public ResId ResId;
        public UnityEngine.Object Asset;
        public CPtr<IAssetRef> AssetRef;

        public ResPoolItem()
        {
        }
        public static ResPoolItem Create(ResPath path, IAssetRef asset_ref)
        {
            var ret = S_Pool.New();
            ret.Path = path;
            ret.AssetRef = new CPtr<IAssetRef>(asset_ref);
            ret.Asset = asset_ref.Asset;

            if (path.Sprite)
                ret.ResId = new ResId(asset_ref.Asset.GetInstanceID(), EResType.Sprite);
            else
                ret.ResId = new ResId(asset_ref.Asset.GetInstanceID(), EResType.Res);

            return ret;
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
        //Path-> Inst Id
        public Dictionary<ResPath, ResId> _index_by_path;

        //Key: ResId, Value: TimeFrameCount
        public LruList<ResId, int> _lru_free_list;

        public ResPool()
        {
            _pool = new Dictionary<int, ResPoolItem>(C_POOL_CAP);
            _lru_free_list = new LruList<ResId, int>();
            _index_by_path = new Dictionary<ResPath, ResId>(MyEqualityComparer<ResPath>.Default);
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
                data.Path= pool_val.Path.Path;
                data.Sprite = pool_val.Path.Sprite;
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

            path = val.Path;
            return EResError.OK;
        }

        public EResError GetIdByPath(ResPath path, out ResId id)
        {
            if (!path.IsValid())
            {
                id = ResId.Null;
                return EResError.ResPool_path_null_1;
            }

            bool succ = _index_by_path.TryGetValue(path, out id);
            if (succ)
                return EResError.OK;
            return EResError.ResPool_res_not_exist_5;
        }

        public EResError AddRes(ResPath path, IAssetRef asset_ref, out ResId id)
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

            bool asset_is_sprite = asset_ref.Asset is Sprite;
            //路径指定的Sprite类型 和 资源类型不一致
            //主要是打包之后 可能会发生该情况
            // 比如: a.png 对应一个sprite, 如果没有其他资源引用该 Texture, 并且该Sprite 打包进了atlas
            // 在Editor模式下, 如果要加载该Sprite, 必须是 ResPath{path,Sprite:true) 来加载
            // 但是在打包之后, 两种方法都会加载该对象, ResPath(path,Sprite:false) 和 ResPath(path, Sprite:true) 都会加载出Sprite, 这个地方杜绝
            // 还有就是 AssetLoader的实现问题, 如果出错, 会导致 ResPath(path,Sprite:true) 加载出Texture出来, 也要杜绝
            if (path.Sprite != asset_is_sprite)
            {
                id = ResId.Null;
                return EResError.ResPool_res_is_sprite;
            }

            //2. 根据路径找到 ResId
            ResPoolItem pool_val;
            bool succ = _index_by_path.TryGetValue(path, out id);
            if (succ)
            {
                //1.1. 检查资源是否一致
                succ = _pool.TryGetValue(id.Id, out pool_val);
                if (!succ)
                {
                    ResLog._.Assert(false, "严重错误");
                    id = ResId.Null;
                    return EResError.ResPool_e2;
                }

                //这个地方的发生: AB模式下, 加载成功之后, 该AB包被卸载了, 然后重新加载该AB包,并加载该资源,会导致 同一个路径,加载出的资源不一样
                if (pool_val.Asset != asset_ref.Asset)
                {
                    ResLog._.Assert(false, "添加资源的时候，资源不一致 {0}", path);
                    id = ResId.Null;
                    return EResError.ResPool_same_path_diff_res;
                }

                //新的 asset ref 销毁, 只要保存一份就行了
                asset_ref.Destroy();

                //刷新lru
                _lru_free_list.Set(id, UnityEngine.Time.frameCount);
                return EResError.OK;
            }

            //3. 创建poolval
            pool_val = ResPoolItem.Create(path, asset_ref);
            id = pool_val.ResId;

            //4. 再次检查, 这里会发生, 是因为两个路径加载出了同一个资源导致的,可能的原因,比如
            // 1) : 路径大小写
            // 2) : AssetLoader 可以通过别名加载资源
            if (_pool.TryGetValue(id.Id, out var _))
            {
                pool_val.Destroy();
                return EResError.ResPool_multi_path_to_one_res;
            }

            //4. 向各个pool里面添加
            ResLog._.D("{0} add res {1}", id.Id, path);
            _pool.Add(id.Id, pool_val);
            _index_by_path.Add(path, id);
            _lru_free_list.Set(id, UnityEngine.Time.frameCount);

            return EResError.OK;
        }

        public EResError RemoveRes(int inst_id, out ResPath path, out IAssetRef asset_ref)
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
            path = pool_val.Path;
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
            _index_by_path.Remove(pool_val.Path);
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
            ResLog.ErrCode(err, path.Path);
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
    }
}
