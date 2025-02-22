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
    //资源和实例的 Holder,方便一起卸载
    internal sealed class ResHolder : CPoolItemBase, IResHolder
    {
        public struct InnerItem
        {
            public int JobId;
            public ResRef ResRef;
        }

        private MyDict<ResPath, ResRef> _All = new MyDict<ResPath, ResRef>();
        private MyDict<int, ResPath> _PreLoadDict = new MyDict<int, ResPath>();
        public HolderStat _Stat = new HolderStat();
        public bool _SyncLoadEnable;
        private IHolderCallBack _HolderCb;
        public CPtr<IResMgr> _ResMgr;

        public ResHolder()
        {
        }

        internal static ResHolder Create(IResMgr res_mgr, bool sync_load_enable)
        {
            var ret = GPool.New<ResHolder>();
            ret._ResMgr = new CPtr<IResMgr>(res_mgr);
            ret._SyncLoadEnable = sync_load_enable;
            return ret;
        }

        public UnityEngine.Object Load(string path, bool sprite)
        {
            var mgr = _ResMgr.Val;
            if (mgr == null)
                return null;

            ResPath res_path = ResPath.Create(path, sprite);

            //1. 先检查缓存
            if (_All.TryGetValue(res_path, out var item))
            {
                return item.Get();
            }

            //2. 同步加载
            EResError err = mgr.Load(path, sprite, _SyncLoadEnable, out var res_ref);
            ResLog._.ErrCode(err, path);
            if (err != EResError.OK)
                return null;

            //3. 添加
            res_ref.AddUser(this);
            _All[res_path] = res_ref;

            //4. 从 预加载的 dict里面移除, 并修改统计
            foreach (var p in _PreLoadDict)
            {
                if (p.Value == res_path)
                {
                    _Stat.Succ++;
                    _PreLoadDict.Remove(p.Key);
                    break;
                }
            }
            return res_ref.Get();
        }

        public void PreLoad(string path, bool sprite = false, int priority = 0)
        {
            //1. 检查ResMgr
            IResMgr mgr = _ResMgr.Val;
            if (mgr == null)
                return;

            //2. 已经存在了
            ResPath res_path = ResPath.Create(path, sprite);
            if (_All.TryGetValue(res_path, out var item))
                return;
            foreach (var p in _PreLoadDict)
            {
                if (p.Value == res_path)
                    return;
            }

            //3. 预加载
            _Stat.Total++;
            EResError err = mgr.AsyncLoad(path, sprite, priority, _OnResLoaded, out int job_id);
            ResLog._.ErrCode(err);
            if (err == EResError.OK)
            {
                _PreLoadDict.Add(job_id, res_path);
            }
            else
            {
                _All[res_path] = default;
                _Stat.Fail++;
            }
        }

        private void _OnResLoaded(int job_id, EResError error, ResRef res_ref)
        {
            //1. 查找, 如果找不到,说明已经被销毁了            
            if (!_PreLoadDict.Remove(job_id, out var res_path))
            {
                return;
            }

            //2. 检查错误
            UnityEngine.Object res = null;
            if (res_ref.IsValid())
                res = res_ref.Get();

            //3. 添加
            if (res == null)
            {
                _Stat.Fail++;
                if (!_All.ContainsKey(res_path))
                    _All[res_path] = default;
                _HolderCb?.OnHolderCallBack();
            }
            else
            {
                _Stat.Succ++;
                res_ref.AddUser(this);
                _All[res_path] = res_ref;
                _HolderCb?.OnHolderCallBack();
            }
        }

        protected override void OnPoolRelease()
        {
            var res_mgr = _ResMgr.Val;

            if (res_mgr != null)
            {
                foreach (var p in _All)
                {
                    p.Value.RemoveUser(this);
                }

                foreach (var p in _PreLoadDict)
                {
                    res_mgr.CancelJob(p.Key);
                }
            }

            _All.Clear();
            _PreLoadDict.Clear();
            _Stat = new HolderStat();
        }

        public void GetAllRes(List<ResRef> out_list)
        {
            foreach (var p in _All)
            {
                if (p.Value.IsValid())
                    out_list.Add(p.Value);
            }
        }

        public HolderStat GetResStat()
        {
            return _Stat;
        }

        public void SetCallBack(IHolderCallBack callback)
        {
            _HolderCb = callback;
        }
    }
}
