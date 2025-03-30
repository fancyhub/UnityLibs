using System;
using System.Collections.Generic;
using UnityEngine;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18 
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace FH.ResManagement
{
    internal sealed class InstHolder : CPoolItemBase, IInstHolder
    {
        private bool _OnlyFromCache;
        private bool _Share;

        private CPtr<IResMgr> _ResMgr;
        private MyDict<int, ResRef> _Dict = new MyDict<int, ResRef>();
        private MyDict<int, ResRef> _FreeDict = new MyDict<int, ResRef>();
        private MyDict<int, int> _PreCreateDict = new MyDict<int, int>();
        private HolderStat _Stat = new HolderStat();
        private IHolderCallBack _HolderCb;

        /// <summary>
        /// Share 描述的是, 实例对象被还回来之后, 是否放回到大池子里面, 还是留在 Holder里面, 也就是多个InstHolder在存续期间, 实例对象是否共享
        /// </summary>
        internal static InstHolder Create(IResMgr res_mgr, bool only_from_cache, bool share)
        {
            var ret = GPool.New<InstHolder>();
            ret._OnlyFromCache = only_from_cache;
            ret._ResMgr = new CPtr<IResMgr>(res_mgr);
            ret._Share = share;
            return ret;
        }

        public bool IsSharedMode()
        {
            return _Share;
        }

        public GameObject Create(string path)
        {
            //1. 检查
            IResMgr res_mgr = _ResMgr.Val;
            if (res_mgr == null)
            {
                ResLog._.E("ResMgr 已经被销毁了");
                return null;
            }

            //2. 非share模式下, 先从FreeDict里面查找
            if (!_Share)
            {
                foreach (var p in _FreeDict)
                {
                    if (p.Value.Path != path)
                        continue;

                    GameObject tempObj = p.Value.Get<GameObject>();
                    if (tempObj == null)
                    {
                        ResLog._.Assert(false, "被缓存的对象已经被销毁了 {0}", p.Value.Path);
                        p.Value.RemoveUser(this);//移除自己的引用, ResMgr会通过GC销毁
                        _FreeDict.Remove(p.Key);
                        continue;
                    }

                    _Dict.Add(p.Key, p.Value);
                    _FreeDict.Remove(p.Key);
                    return tempObj;
                }
            }

            //3. 从ResMgr里面获取
            EResError err = res_mgr.Create(path, this, _OnlyFromCache, out ResRef res_ref);

            if (err != EResError.OK)
            {
                if (_OnlyFromCache)
                {
                    ResLog._.ErrCode(err, $"创建实例失败,因为只能从缓存里面创建, 可能资源未提前加载 {path}");
                }
                else 
                {
                    ResLog._.ErrCode(err, $"创建实例失败,资源不存在 {path}");
                }
            }

            if (err != EResError.OK)
                return null;
            ResLog._.D("{0}", res_ref);

            //4. 获取对象
            GameObject obj = res_ref.Get<GameObject>();
            if (obj == null)
            {
                ResLog._.Assert(false, "Error");
                res_ref.RemoveUser(this);
                return null;
            }

            _Dict.Add(obj.GetInstanceID(), res_ref);
            return obj;
        }


        public void PreCreate(string path, int count, int priority = 0)
        {
            //Share 模式, 不需要
            if (_Share)
                return;
            if (count <= 0)
                return;

            IResMgr res_mgr = _ResMgr.Val;
            if (res_mgr == null)
            {
                ResLog._.E("ResMgr 已经被销毁了");
                return;
            }

            _Stat.Total += count;
            for (int i = 0; i < count; i++)
            {
                EResError err = res_mgr.CreateAsync(path, priority, _OnInstCB, out int job_id);
                ResLog._.ErrCode(err);
                if (err == EResError.OK)
                {
                    _PreCreateDict.Add(job_id, priority);
                }
                else
                {
                    _Stat.Fail++;
                }
            }
        }

        private void _OnInstCB(int job_id, EResError error, ResRef res_ref)
        {
            if (!_PreCreateDict.Remove(job_id, out int priority))
                return;

            if (error != EResError.OK)
            {
                _Stat.Fail++;
                _HolderCb?.OnHolderCallBack();
                return;
            }

            IResMgr res_mgr = _ResMgr.Val;
            if (res_mgr == null)
            {
                _Stat.Fail++;
                _HolderCb?.OnHolderCallBack();
                return;
            }
            
            if(!res_ref.AddUser(this))
            {
                _Stat.Fail++;
                _HolderCb?.OnHolderCallBack();
                return;
            }

            _Stat.Succ++;
            _FreeDict.Add(res_ref.Id.Id, res_ref);
            _HolderCb?.OnHolderCallBack();
        }

        /// <summary>
        /// 回收一个实例对象
        /// </summary>        
        public bool Release(GameObject obj)
        {
            if (obj == null)
            {
                ResLog._.E("ResMgr 已经被销毁了");
                return false;
            }
            int inst_id = obj.GetInstanceID();

            if (!_Dict.Remove(inst_id, out ResRef res_ref))
            {
                GameObjectPoolUtil.Push2Pool(obj);
                ResLog._.Assert(false, "对象不是从自身的Holder创建的");
                return true;
            }

            if (_Share)
                res_ref.RemoveUser(this);
            else
            {
                GameObjectPoolUtil.Push2Pool(obj);
                _FreeDict.Add(obj.GetInstanceID(), res_ref);
            }

            return true;
        }

        protected override void OnPoolRelease()
        {
            foreach (var p in _Dict)
            {
                p.Value.RemoveUser(this);
            }
            _Dict.Clear();

            foreach (var p in _FreeDict)
            {
                p.Value.RemoveUser(this);
            }
            _FreeDict.Clear();

            IResMgr res_mgr = this._ResMgr.Val;
            if (res_mgr != null)
            {
                foreach (var p in _PreCreateDict)
                {
                    res_mgr.CancelJob(p.Key);
                }
            }
            _PreCreateDict.Clear();
            _ResMgr = default;
            _Stat = new HolderStat();
        }

        public void GetAllInst(List<ResRef> out_list)
        {
            foreach (var p in _Dict)
                out_list.Add(p.Value);

            foreach (var p in _FreeDict)
                out_list.Add(p.Value);
        }

        public HolderStat GetInstStat()
        {
            return _Stat;
        }

        public void SetCallBack(IHolderCallBack callback)
        {
            _HolderCb = callback;
        }
    }
}
