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
            UnityEngine.AsyncOperation LoadScene();
        }

        public interface IExternalLoader : ICPtr
        {
            IExternalRef Load(string scene_path, UnityEngine.SceneManagement.LoadSceneMode mode);
        }
    }   
}
