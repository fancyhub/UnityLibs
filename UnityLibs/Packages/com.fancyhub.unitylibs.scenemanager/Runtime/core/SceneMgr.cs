/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/20
 * Title   : 
 * Desc    : 
*************************************************************************************/

using FH.SceneManagement;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

namespace FH
{
    public sealed class SceneMgrUpgradeOperation : UnityEngine.CustomYieldInstruction
    {
        private int _total_count;
        internal System.Func<(int remain_count, bool all_done)> FuncGetStat;
        public SceneMgrUpgradeOperation(int total_count)
        {
            _total_count = total_count;
        }

        public override bool keepWaiting => !FuncGetStat().all_done;

        public bool IsDone
        {
            get
            {
                return FuncGetStat().all_done;
            }
        }

        public float Progress
        {
            get
            {
                var (remain_count, all_done) = FuncGetStat();
                if (all_done)
                    return 1.0f;
                if (_total_count <= 0)
                    return 0.9f;
                return System.Math.Clamp((float)((_total_count - remain_count) / (double)_total_count), 0, 0.99f);
            }
        }
    }

    public partial interface ISceneMgr : ICPtr
    {
        public SceneRef LoadScene(string scene_path, UnityEngine.SceneManagement.LoadSceneMode loadMode);
        public void Update();
        public void GetAllScenes(List<SceneRef> out_list);

        public void UnloadAll();


        /// <summary>
        /// 会阻止新的加载, 返回的operation 指示是否所有的异步加载都结束了
        /// </summary>
        public SceneMgrUpgradeOperation BeginUpgrade();
        public void EndUpgrade(bool result);
    }

    public static class SceneMgr
    {
        private static CPtr<ISceneMgr> _;
        public static ISceneMgr Inst => _.Val;

        public static void InitMgr(ISceneMgr.Config config, ISceneMgr.IExternalLoader external_loader)
        {
            if (config == null)
            {
                SceneLog._.E("SceneMgrConfig Is Null");
                return;
            }

            if (!_.Null)
            {
                SceneLog._.E("SceneMgr 已经创建了");
                return;
            }
            if (external_loader == null)
            {
                SceneLog._.E("SceneLoader Is Null");
                return;
            }

            SceneLog._ = TagLog.Create(SceneLog._.Tag, config.LogLvl);
            SceneMgrImplement mgr = new SceneMgrImplement(external_loader);
            _ = new CPtr<ISceneMgr>(mgr);
        }

        public static SceneRef LoadScene(string scene_path, UnityEngine.SceneManagement.LoadSceneMode loadMode)
        {
            ISceneMgr mgr = _.Val;
            if (mgr == null)
            {
                SceneLog._.E("SceneMgr is Null");
                return SceneRef.Empty;
            }
            return mgr.LoadScene(scene_path, loadMode);
        }

        public static void GetAllScenes(List<SceneRef> out_list)
        {
            ISceneMgr mgr = _.Val;
            if (mgr == null)
            {
                SceneLog._.E("SceneMgr is Null");
                return;
            }
            if (out_list == null)
            {
                SceneLog._.E("out_list is null");
                return;
            }
            out_list.Clear();
            mgr.GetAllScenes(out_list);
        }

        public static void Update()
        {
            ISceneMgr mgr = _.Val;
            mgr?.Update();
        }

        public static void Destroy()
        {
            _.Destroy();
        }
    }
}
