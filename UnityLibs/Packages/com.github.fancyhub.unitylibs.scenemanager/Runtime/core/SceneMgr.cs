/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/20
 * Title   : 
 * Desc    : 
*************************************************************************************/

using FH.SceneManagement;
using System;
using System.Collections.Generic;

namespace FH
{
    public interface ISceneMgr : ICPtr
    {
        public SceneRef LoadScene(string scene_path, bool additive);
        public void Update();
    }

    public static class SceneMgr
    {
        private static CPtr<ISceneMgr> _Inst;

        public static void InitMgr(ISceneLoader scene_loader)
        {
            if (!_Inst.Null)
            {
                SceneLog._.E("SceneMgr 已经创建了");
                return;
            }

            if (scene_loader == null)
            {
                SceneLog._.E("SceneLoader Is Null");
                return;
            }

            SceneMgrImplement mgr = new SceneMgrImplement(scene_loader);
            _Inst = new CPtr<ISceneMgr>(mgr);
        }

        public static SceneRef LoadScene(string scene_path, bool additive)
        {
            ISceneMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                SceneLog._.E("SceneMgr is Null");
                return SceneRef.Empty;
            }
            return mgr.LoadScene(scene_path, additive);
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