/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;


namespace FH.Res
{
    //gameobject 预实例化的数据
    internal class GameObjectPreInstData
    { 
        public struct InstData
        {
            public string _path;

            //所有请求加起来的最大值
            public int _count;

            //配置里面描述的最大值
            public int _conf_count;

            public int GetCountPreInst()
            {
                return Math.Min(_count, _conf_count);
            }
        }

        //key：资源路径
        //Value： 描述了 很多对该资源的预加载请求
        public Dictionary<string, InstData> _dict;

        //Key: 请求id
        //Val: key 对应的string
        //Val: Val 对应的该请求的 count
        public Dictionary<int, KeyValuePair<string, int>> _req_id_dict;

        public ResMgrConfig.GameObjectPreInstConfig _conf;
        public int _req_id_gen = 0;

        public GameObjectPreInstData(ResMgrConfig.GameObjectPreInstConfig conf)
        {
            _conf = conf;
            _dict = new Dictionary<string, InstData>();
            _req_id_dict = new Dictionary<int, KeyValuePair<string, int>>();
        }

        public int GetCount(string path)
        {
            if (string.IsNullOrEmpty(path))
                return 0;

            InstData inst_data;
            bool has = _dict.TryGetValue(path, out inst_data);
            if (!has)
                return 0;
            return inst_data.GetCountPreInst();
        }

        public EResError Req(string path, int count, out int req_id)
        {
            //1. check
            if (string.IsNullOrEmpty(path))
            {
                req_id = 0;
                return (EResError)EResError.GameObjectPreInstData_path_null_1;
            }
            if (count <= 0)
            {
                req_id = 0;
                return (EResError)EResError.GameObjectPreInstData_count_zero;
            }

            //2. 从dict 里面找到
            InstData inst_data;
            bool has = _dict.TryGetValue(path, out inst_data);

            //2. 如果没有，就创建一个
            if (!has)
            {
                int conf_count = _conf.GetMaxCount(path);

                inst_data = new InstData();
                inst_data._path = path;
                inst_data._conf_count = conf_count;
                inst_data._count = 0;

                _dict.Add(path, inst_data);
            }

            //3. 创建req id
            _req_id_gen++;
            req_id = _req_id_gen;

            //4. 添加到data里面
            inst_data._count += count;
            _dict[path] = inst_data;
            _req_id_dict.Add(req_id, new KeyValuePair<string, int>(path, count));
            return EResError.OK;
        }

        public EResError Cancel(int req_id)
        {
            //1. 从dict 里面找到
            KeyValuePair<string, int> v;
            bool has = _req_id_dict.TryGetValue(req_id, out v);
            if (!has)
                return (EResError)EResError.GameObjectPreInstData_req_id_not_exist;

            //2. 移除，并获取对应的值
            _req_id_dict.Remove(req_id);
            string path = v.Key;
            int count = v.Value;

            //3. 找到data
            InstData inst_data;
            has = _dict.TryGetValue(path, out inst_data);
            if (!has)
            {
                Res.ResLog._.Assert(false, "严重错误，数据不一致");
                return (EResError)EResError.GameObjectPreInstData_cant_find_data_with_path;
            }

            //4. 修改数据
            inst_data._count -= count;
            if (inst_data._count <= 0)
                _dict.Remove(path);
            else
                _dict[path] = inst_data;
            return EResError.OK;
        }
    }

    //预实例化的组件
    internal class GameObjectPreInst
    {
        public const int C_COUNT_PER_FRAME = 5;

        //传入的区域
        public GameObjectPreInstData _gobj_pre_data;
        public GameObjectStat _gobj_stat;
        public ResJobDB _job_db;
        public ResMsgQueue _msg_queue;

        //自己的
        public List<string> _temp_list = new List<string>();

        public void Update()
        {
            //1.如果有任务正在跑，就返回
            if (_job_db.GetCount() > 0)
                return;

            //2. 获取任务
            if (_temp_list.Count == 0)
            {
                if (_gobj_pre_data._dict.Count > 0)
                    _temp_list.AddRange(_gobj_pre_data._dict.Keys);
                else
                    return;
            }

            //3. 创建任务
            for (int i = _temp_list.Count - 1, j = 0
                ; i >= 0 && j < C_COUNT_PER_FRAME
                ; i--, j++)
            {
                string path = _temp_list[i];
                _temp_list.RemoveAt(i);

                int count_need = _gobj_pre_data.GetCount(path);
                int count_now = _gobj_stat.GetCount(path);

                int delta_count = count_need - count_now;
                if (delta_count > 0)
                {
                    ResJob job = _job_db.CreateJob(ResPath.CreateRes(path), -1000, null);
                    job.AddWorker(EResWoker.async_load_res);
                    for (int k = 0; k < delta_count; ++k)
                        job.AddWorker(EResWoker.async_obj_inst);

                    _msg_queue.BeginJob(job, false);
                }
            }
        }
    }
}
