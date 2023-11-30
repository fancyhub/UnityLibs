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

namespace FH.Res
{
    internal class AtlasLoader : IDestroyable, IMsgProc<ResJob>
    {
        public IAssetLoader _asset_loader;
        public ResMsgQueue _msg_queue;        
        public ResJobDB _job_db;
        public ResPool _res_pool;

        public Dictionary<string, InnerData> _dict;

        public struct InnerData
        {
            public string _path;
            public string _atlas_tag;
            public Action<SpriteAtlas> _call_back;
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
            
            bool succ = _dict.TryGetValue(job.Path.Path, out InnerData cb);
            if (!succ)
                return;
            _dict.Remove(job.Path.Path);


            var action = cb._call_back;
            EResError error_code = _res_pool.GetIdByPath(job.Path, out ResId id);

            ResLog._.ErrCode(error_code, "加载 失败 {0}", job.Path.Path);
            if (!id.IsValid())
                return;


            SpriteAtlas atlas = _res_pool.GetRes(id.Id) as SpriteAtlas;
            if (atlas == null)
            {
                ResLog._.Assert(false, "加载Atlas 失败{0}", job.Path.Path);
                return;
            }
            action(atlas);
        }


        private void _OnAtlasRequest(string atlas_tag, Action<SpriteAtlas> arg2)
        {
            string path = _asset_loader.AtlasTag2Path(atlas_tag);
            if (string.IsNullOrEmpty(path))
            {
                ResLog._.Assert(false, "Atlas Tag {0} -> Path Null", atlas_tag);
                return;
            }
            ResLog._.D("Atlas Request {0} {1}", atlas_tag, path);

            if (_dict.ContainsKey(path))
                return;

            ResJob job = _job_db.CreateJob(new ResPath(path, false), 0, null);
            job.AddWorker(EResWoker.async_load_res);
            job.AddWorker(EResWoker.call_res_event);
            job.AddWorker(EResWoker.call_set_atlas);         
            _msg_queue.BeginJob(job,false);

            _dict.Add(path, new InnerData()
            {
                _atlas_tag = atlas_tag,
                _call_back = arg2,
                _path = path,
            });
        }
    }
}
