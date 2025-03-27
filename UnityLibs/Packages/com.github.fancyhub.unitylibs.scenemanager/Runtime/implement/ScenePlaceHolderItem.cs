using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH.SceneManagement
{
    internal class ScenePlaceHolderItem
    {
        private const string SceneName = "__PlaceHolder";

        private Scene _EmptyScene;

        public void CreateForUnload()
        {
            _CreateForUnload();
            CheckForActive();
        }

        public void CheckForActive()
        {
            if (!_EmptyScene.IsValid())
                return;

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene() != _EmptyScene)
                return;

            if (UnityEngine.SceneManagement.SceneManager.sceneCount == 1)
                return;
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (s == _EmptyScene || !s.isLoaded)
                    continue;
                
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(s);
                return;
            }
        }

        private void _CreateForUnload()
        {
            if (_EmptyScene.IsValid())
                return;
            _EmptyScene = SceneManager.GetSceneByName(SceneName);
            if (_EmptyScene.IsValid())
                return;

            if (UnityEngine.SceneManagement.SceneManager.sceneCount > 1)
                return;

            _EmptyScene = SceneManager.CreateScene(SceneName, new CreateSceneParameters(LocalPhysicsMode.None));
        }
    }
}
