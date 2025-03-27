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
        
        public enum EPreloadStatus
        {
            None,
            Succ,
            Failed,
        }

        public struct PreloadData
        {
            public ResPath Path;
            public EPreloadStatus Status;
            public int JobId;
        }

        private MyDict<ResPath, PreloadData> _PreloadPathDict = new MyDict<ResPath, PreloadData>();
        //key: job id
        private MyDict<int, ResPath> _PreLoadJobDict = new MyDict<int, ResPath>();

        private HashSet<ResRef> _AllRes;
        public HolderStat _Stat = new HolderStat();
        public bool _SyncLoadEnable;
        private IHolderCallBack _HolderCb;
        public CPtr<IResMgr> _ResMgr;

        public ResHolder()
        {
            _AllRes = new HashSet<ResRef>(MyEqualityComparer<ResRef>.Default);
        }

        internal static ResHolder Create(IResMgr res_mgr, bool sync_load_enable)
        {
            var ret = GPool.New<ResHolder>();
            ret._ResMgr = new CPtr<IResMgr>(res_mgr);
            ret._SyncLoadEnable = sync_load_enable;
            return ret;
        }

        public UnityEngine.Object Load(string path, EResPathType pathType)
        {
            var mgr = _ResMgr.Val;
            if (mgr == null)
                return null;

            ResPath res_path = ResPath.Create(path, pathType);

            //1. 同步加载
            EResError err = mgr.Load(path, pathType, _SyncLoadEnable, out var res_ref);
            ResLog._.ErrCode(err, path);
            if (err != EResError.OK)
                return null;

            //2. 添加
            if (_AllRes.Add(res_ref))
                res_ref.AddUser(this);

            //3. 从 预加载的 dict里面移除, 并修改统计
            if (_PreloadPathDict.TryGetValue(res_path, out var item))
            {
                _PreLoadJobDict.Remove(item.JobId);

                item.Status = EPreloadStatus.Succ;
                item.JobId = 0;
                _PreloadPathDict[res_path] = item;
                _Stat.Succ++;
            }
            return res_ref.Get();
        }

        public void PreLoad(string path, EResPathType pathType, int priority = 0)
        {
            //1. 检查ResMgr
            IResMgr mgr = _ResMgr.Val;
            if (mgr == null)
                return;

            //2. 已经存在了
            ResPath res_path = ResPath.Create(path, pathType);
            if (_PreloadPathDict.TryGetValue(res_path, out var item))
                return;

            //3. 预加载
            _Stat.Total++;
            EResError err = mgr.AsyncLoad(path, pathType, priority, _OnResLoaded, out int job_id);
            ResLog._.ErrCode(err);
            if (err == EResError.OK)
            {
                _PreLoadJobDict[job_id] = res_path;
                _PreloadPathDict[res_path] = new PreloadData()
                {
                    JobId = job_id,
                    Path = res_path,
                    Status = EPreloadStatus.None,
                };
            }
            else
            {
                _PreloadPathDict[res_path] = new PreloadData()
                {
                    JobId = 0,
                    Path = res_path,
                    Status = EPreloadStatus.Failed,
                };
                _Stat.Fail++;
            }
        }

        private void _OnResLoaded(int job_id, EResError error, ResRef res_ref)
        {
            //1. 查找, 如果找不到,说明已经被销毁了            
            if (!_PreLoadJobDict.Remove(job_id, out var res_path))
                return;
            if (!_PreloadPathDict.TryGetValue(res_path, out var item))
                return;

            //2. 检查错误
            UnityEngine.Object res = null;
            if (res_ref.IsValid())
                res = res_ref.Get();

            //3. 添加
            item.JobId = 0;
            if (res == null)
            {
                _Stat.Fail++;
                item.Status = EPreloadStatus.Failed;
                _PreloadPathDict[res_path] = item;
                _HolderCb?.OnHolderCallBack();
            }
            else
            {
                _Stat.Succ++;
                if (_AllRes.Add(res_ref))
                    res_ref.AddUser(this);
                item.Status = EPreloadStatus.Succ;
                _PreloadPathDict[res_path] = item;
                _HolderCb?.OnHolderCallBack();
            }
        }

        protected override void OnPoolRelease()
        {
            var res_mgr = _ResMgr.Val;
            if (res_mgr != null)
            {
                foreach (var p in _AllRes)
                {
                    p.RemoveUser(this);
                }

                foreach (var p in _PreLoadJobDict)
                {
                    res_mgr.CancelJob(p.Key);
                }
            }

            _PreloadPathDict.Clear();
            _PreLoadJobDict.Clear();
            _AllRes.Clear();
            _Stat = new HolderStat();
        }

        public void GetAllRes(List<ResRef> out_list)
        {
            foreach (var p in _AllRes)
            {
                if (p.IsValid())
                    out_list.Add(p);
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
