/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/20
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    public partial interface ISceneMgr
    {
        public interface IExternalRef : ICPtr
        {
            UnityEngine.AsyncOperation Load(UnityEngine.SceneManagement.LoadSceneParameters load_param);
        }

        public interface IExternalLoader : ICPtr
        {
            /// <summary>
            /// dont load scene
            /// </summary>
            IExternalRef CreateSceneRef(string scene_path);
        }
    }
}
