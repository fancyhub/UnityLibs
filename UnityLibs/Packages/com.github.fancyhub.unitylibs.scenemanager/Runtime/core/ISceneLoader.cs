/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/20
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    public interface ISceneRef : ICPtr
    {        
        UnityEngine.AsyncOperation LoadScene();
    }

    public interface ISceneLoader : ICPtr
    {
        ISceneRef Load(string scene_path, UnityEngine.SceneManagement.LoadSceneMode mode);
    }
}
