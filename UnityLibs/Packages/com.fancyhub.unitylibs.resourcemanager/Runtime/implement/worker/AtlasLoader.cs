/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/12 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace FH.ResManagement
{
    internal class AtlasLoader : IDestroyable, IMsgProc<ResJob>
    {
        public IResMgr.IExternalLoader _external_loader;
        public ResMsgQueue _msg_queue;
        public ResJobDB _job_db;
        public AssetPool _asset_pool;

        public Dictionary<string, InnerData> _dict;

        public class InnerData
        {
            public string _path;
            public string _atlas_tag;
            public LinkedList<Action<SpriteAtlas>> _call_back = new LinkedList<Action<SpriteAtlas>>();

            public void Call(SpriteAtlas atlas)
            {
                foreach (var p in _call_back)
                    p(atlas);

                _call_back.Clear();
            }
        }

        public AtlasLoader()
        {
            _dict = new Dictionary<string, InnerData>();
        }

        public void Init()
        {
            _msg_queue.Reg(EResWoker.call_set_atlas, this);
            SpriteAtlasManager.atlasRequested += _OnAtlasRequest;
        }
        public void Destroy()
        {
            _msg_queue.UnReg(EResWoker.call_set_atlas);
            SpriteAtlasManager.atlasRequested -= _OnAtlasRequest;
        }


        public void OnMsgProc(ref ResJob job)
        {
            _ProcessJob(job);
            _msg_queue.SendJobNext(job);
        }

        private void _ProcessJob(ResJob job)
        {
            bool succ = _dict.TryGetValue(job.Path.Path, out InnerData inner_data);
            if (!succ)
                return;
            _dict.Remove(job.Path.Path);


            EResError error_code = _asset_pool.GetIdByPath(job.Path, out ResId res_id);

            ResLog._.ErrCode(error_code, "加载 失败 {0}", job.Path.Path);
            if (!res_id.IsValid())
            {
                inner_data.Call(null);
                return;
            }


            SpriteAtlas atlas = _asset_pool.Get<SpriteAtlas>(res_id);
            if (atlas == null)
            {
                ResLog._.Assert(false, "加载Atlas 失败{0}", job.Path.Path);
                inner_data.Call(null);
                return;
            }
            inner_data.Call(atlas);
        }

        private void _OnAtlasRequest(string atlas_tag, Action<SpriteAtlas> action)
        {
            string path = _external_loader.AtlasTag2Path(atlas_tag);
            if (string.IsNullOrEmpty(path))
            {
                ResLog._.Assert(false, "Atlas Tag {0} -> Path Null", atlas_tag);
                action(null);
                return;
            }
            ResLog._.D("Atlas Request {0} {1}", atlas_tag, path);

            if (_dict.TryGetValue(path,out var data))
            {
                data._call_back.ExtAddLast(action);
                return;
            }

            data = new InnerData()
            {
                _atlas_tag = atlas_tag,
                _path = path,
            };
            _dict.Add(path, data);
            data._call_back.ExtAddLast(action);

            ResJob job = _job_db.CreateJob(AssetPath.Create(path), 0);
            job.AddWorker(EResWoker.async_load_asset);
            job.AddWorker(EResWoker.call_asset_event);
            job.AddWorker(EResWoker.call_set_atlas);
            _msg_queue.BeginJob(job, false);
        }
    }
}
