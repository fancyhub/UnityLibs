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
            UnityEngine.AsyncOperation LoadScene(UnityEngine.SceneManagement.LoadSceneParameters load_param);
        }

        public interface IExternalLoader : ICPtr
        {
            /// <summary>
            /// 不是真的Load Scene
            /// </summary>            
            IExternalRef Load(string scene_path);
        }
    }
}
