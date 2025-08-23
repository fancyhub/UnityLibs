/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/12 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.ResManagement
{
    //同步创建实例
    internal class GameObjectCreatorSync : IMsgProc<ResJob>
    {
        #region 传入
        public ResPool _res_pool;
        public GameObjectInstPool _gobj_pool;        
        public ResMsgQueue _msg_queue;
        #endregion

 

        public void Init()
        {
            _msg_queue.Reg(EResWoker.sync_obj_inst, this);
        }

        public void Destroy()
        {
            _msg_queue.UnReg(EResWoker.sync_obj_inst);
        }

        public void OnMsgProc(ref ResJob job)
        {
            EResWoker job_type = job.GetCurrentWorker();
            if (job_type != EResWoker.sync_obj_inst)
            {
                ResLog._.Assert(false, "job 不是 类型 {0}!={1}, {2}", job_type, EResWoker.sync_obj_inst, job.Path);
                _msg_queue.SendJobNext(job);
                return;
            }

            //3. 判断job是否已经出错了
            if (job.ErrorCode != EResError.OK)
            {
                _msg_queue.SendJobNext(job);
                return;
            }

            //5. 获取资源
            UnityEngine.Object res = _res_pool.Get(job.ResId);
            if (null == res)
            {
                ResLog._.Assert(false, "实例化的时候，资源不正常的销毁了 {0}", job.Path);
                job.ErrorCode = (EResError)EResError.GameObjectCreatorSync_inst_error_res_null;
                _msg_queue.SendJobNext(job);
                return;
            }

            //6. 判断是否为Prefab
            GameObject prefab = res as GameObject;
            if (null == prefab)
            {
                ResLog._.Assert(false, "实例化的时候， 资源不是gameobject {0}", job.Path);
                job.ErrorCode = (EResError)EResError.GameObjectCreatorSync_inst_error_not_gameobj;
                _msg_queue.SendJobNext(job);
                return;
            }

            //6. 找一个free 的对象
            EResError err = _gobj_pool.PopInst(job.Path.Path, out job.InstId);
            if (err == EResError.OK)
            {
                //说明找到了
                _msg_queue.SendJobNext(job);
                return;
            }

            //7. 没有找到空闲的，创建一个                      
            GameObject inst = GameObjectPoolUtil.InstNew(job.Path.Path, prefab);
            if (null == inst)
            {
                job.ErrorCode = (EResError)EResError.GameObjectCreatorSync_inst_error_unkown;
                _msg_queue.SendJobNext(job);
                return;
            }
            GameObjectPoolUtil.InstActive(inst);
            bool succ = _gobj_pool.AddInst(new ResRef(job.ResId, job.Path.Path, _res_pool), inst, out job.InstId);
            ResLog._.Assert(succ, "严重错误 {0}", job.Path);

            _msg_queue.SendJobNext(job);
        }
    }
}
