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
    internal class ResInstGc
    {
        //下面的变量都是传入的
        public ResPool _res_pool;
        public GameObjectInstPool _gobj_pool;
        public GameObjectPreInstData _gobj_pre_data;
        public GameObjectStat _gobj_stat;

        public IResMgr.Config.GCConfig _config;
        
        public List<KeyValuePair<ResId, int>> _temp_list = new List<KeyValuePair<ResId, int>>();

        public ResInstGc(IResMgr.Config.GCConfig config)
        {
            _config = config;            
        }
        public void Update()
        {
            GcInst();
            GcRes();
        }

        public void GcRes()
        {
            int now_time = UnityEngine.Time.frameCount;
            //1.1 获取列表
            List<KeyValuePair<ResId, int>> res_list = _temp_list;
            _res_pool.GetLruFreeList(res_list, true, _config.MaxResCountProcess);
            int expire_time = now_time - _config.ResWaitFrameCount;

            //1.2 检查数据是否到了
            for (int i = 0; i < res_list.Count; ++i)
            {
                int time = res_list[i].Value;
                if (time > expire_time) //还没有到时间                    
                    break;

                //1.3 销毁
                ResId id = res_list[i].Key;
                EResError err = _res_pool.RemoveRes(id.Id, out ResPath path, out var asset_ref);
                if (err != EResError.OK)
                {
                    ResLog._.Assert(false, "释放资源错误 Code: {0}", err);
                    continue;
                }
                ResLog._.D("{0} remove res {1}", id.Id, path);
                asset_ref.Destroy();
            }
        }

        public void GcInst()
        {
            //2.1 获取列表            
            int now_time = UnityEngine.Time.frameCount;
            List<KeyValuePair<ResId, int>> inst_list = _temp_list;
            _gobj_pool.GetLruFreeList(inst_list, true, _config.MaxInstCountProcess);
            int expire_time = now_time - _config.InstWaitFrameCount;

            //2.2 检查数据是否到了
            for (int i = 0; i < inst_list.Count; ++i)
            {
                int time = inst_list[i].Value;
                if (time > expire_time) //还没有到时间
                    break;

                //2.3 获取路径
                ResId res_id = inst_list[i].Key;
                bool suc1 = _gobj_pool.GetInstResUser(
                    res_id
                    , out string path
                    , out UnityEngine.GameObject inst
                    , out int user_count
                    , out System.Object user);
                if (!suc1)
                {
                    ResLog._.Assert(false, "释放资源错误");
                    continue;
                }

                //2.4 检查 预加载的策略
                if (inst != null) //如果该对象已经为null了，不检查预加载策略
                {
                    int count_need = _gobj_pre_data.GetCount(path);
                    int count_now = _gobj_stat.GetCount(path);
                    if (count_need >= count_now)
                    {
                        //不要释放
                        _gobj_pool.RefreshLru(res_id);
                        continue;
                    }
                }

                //2.5 移除对资源的引用
                EResError err = _res_pool.RemoveUser(ResPath.CreateRes(path), user, out ResId _);
                ResLog._.ErrCode(err, "释放资源出错 {0}", path);

                //2.6 开始释放
                bool suc2 = _gobj_pool.RemoveInst(res_id, out string _);
                if (!suc2)
                {
                    ResLog._.Assert(false, "释放资源错误");
                    continue;
                }
                GoUtil.Destroy(inst);
                ResLog._.D("{0} destroy inst {1}", res_id.Id, path);
                //Do nothing
                //Resources.UnloadAsset(res);
            }
        }
    }
}
