/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/20
 * Title   : 
 * Desc    : 
*************************************************************************************/

using FH.SceneManagement;
using System.Collections.Generic;

namespace FH
{
    public partial interface ISceneMgr : ICPtr
    {
        public SceneRef LoadScene(string scene_path, UnityEngine.SceneManagement.LoadSceneMode loadMode);
        public void Update();
        public void GetAllScenes(List<SceneRef> out_list);
    }

    public static class SceneMgr
    {
        private static CPtr<ISceneMgr> _Inst;

        public static void InitMgr(ISceneMgr.Config config, ISceneMgr.IExternalLoader external_loader)
        {
            if (config == null)
            {
                SceneLog._.E("SceneMgrConfig Is Null");
                return;
            }

            if (!_Inst.Null)
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
            _Inst = new CPtr<ISceneMgr>(mgr);
        }

        public static SceneRef LoadScene(string scene_path, UnityEngine.SceneManagement.LoadSceneMode loadMode)
        {
            ISceneMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                SceneLog._.E("SceneMgr is Null");
                return SceneRef.Empty;
            }
            return mgr.LoadScene(scene_path, loadMode);
        }

        public static void GetAllScenes(List<SceneRef> out_list)
        {
            ISceneMgr mgr = _Inst.Val;
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
            ISceneMgr mgr = _Inst.Val;
            mgr?.Update();
        }

        public static void Destroy()
        {
            _Inst.Destroy();
        }
    }
}
