using System;
using System.Collections.Generic;
using UnityEngine;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace FH.Res
{
    //资源和实例的 Holder,方便一起卸载
    internal sealed class ResHolder : CPoolItemBase, IResHolder
    {
        public struct InnerItem
        {
            public int JobId;
            public ResRef ResRef;
        }

        public MyDict<ResPath, InnerItem> _all;
        public ResEvent _res_event;
        public HolderStat _stat;
        public bool _sync_load_enable;
        public CPtr<IResMgr> _ResMgr;
        public ResHolder()
        {
            _all = new MyDict<ResPath, InnerItem>();
            _res_event = _OnResLoaded;
        }

        internal static ResHolder Create(IResMgr res_mgr, bool sync_load_enable)
        {
            var ret = GPool.New<ResHolder>();
            ret._ResMgr = new CPtr<IResMgr>(res_mgr);
            ret._sync_load_enable = sync_load_enable;
            return ret;
        }

        public UnityEngine.Object Load(string path, bool sprite)
        {
            var mgr = _ResMgr.Val;
            if (mgr == null)
                return null;

            ResPath res_path = ResPath.Create(path, sprite);

            //1. 先检查缓存
            if (_all.TryGetValue(res_path, out var item))
            {
                if (item.ResRef.IsSelfValid())
                    return item.ResRef.Get();

                if (item.JobId == 0) //加载失败                                    
                    return null;
            }

            //2. 同步加载
            EResError err = mgr.Load(path, sprite, _sync_load_enable, out var res_ref);
            ResLog._.ErrCode(err, path);
            if (err != EResError.OK)
            {
                _all[res_path] = default; //加载失败的
                return null;
            }

            //3. 添加
            res_ref.AddUser(this);
            _all[res_path] = new InnerItem()
            {
                JobId = 0,
                ResRef = res_ref,
            };
            return res_ref.Get();
        }

        public void PreLoad(string path, bool sprite = false, int priority = 0)
        {
            IResMgr mgr = _ResMgr.Val;
            if (mgr == null)
                return;

            //已经存在了
            ResPath res_path = ResPath.Create(path, sprite);
            if (_all.TryGetValue(res_path, out var item))
                return;

            EResError err = mgr.AsyncLoad(path,sprite, priority, _res_event, out int job_id);
            ResLog._.ErrCode(err);
            if (err == EResError.OK)
            {
                _all.Add(res_path, new InnerItem()
                {
                    JobId = job_id,
                });
            }
        }

        public void UnloadAll()
        {
            foreach (var p in _all)
            {
                p.Value.ResRef.RemoveUser(this);
            }
            _all.Clear();
            _stat = new HolderStat();
        }

        private void _OnResLoaded(EResError err, string path, EResType resType, int job_id)
        {
            //1. 查找, 如果找不到,说明已经被销毁了
            ResPath res_path = ResPath.Create(path, resType== EResType.Sprite? true:false);
            
            if (!_all.TryGetValue(res_path, out var item) || item.JobId != job_id)
                return;

            //2. 检查错误
            if (err != EResError.OK)
            {
                ResLog._.ErrCode(err, path);
                _all[res_path] = default;
                return;
            }

            IResMgr res_mgr = _ResMgr.Val;
            if (res_mgr == null)
                return;


            //3. 添加
            err = res_mgr.Load(path,res_path.Sprite, false, out var res_ref);
            ResLog._.ErrCode(err, path);
            if (err != EResError.OK)
            {
                _all[res_path] = default;
                return;
            }

            res_ref.AddUser(this);
            _all[res_path] = new InnerItem() { ResRef = res_ref };
        }

        protected override void OnPoolRelease()
        {
            UnloadAll();
        }

        public void GetAll(List<ResRef> out_list)
        {
            foreach (var p in _all)
            {
                if (p.Value.ResRef.IsSelfValid())
                    out_list.Add(p.Value.ResRef);
            }
        }

        public HolderStat GetStat()
        {
            return _stat;
        }
    }
}
