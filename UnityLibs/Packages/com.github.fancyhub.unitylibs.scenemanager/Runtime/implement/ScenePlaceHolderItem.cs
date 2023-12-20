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
        public void Update()
        {
            _CreateScene();

        }

        private void _CreateScene()
        {
            if (_EmptyScene.IsValid())
                return;

            _EmptyScene = SceneManager.GetSceneByName(SceneName);
            if (_EmptyScene.IsValid())
                return;

            _EmptyScene = SceneManager.CreateScene(SceneName, new CreateSceneParameters(LocalPhysicsMode.None));
        }
    }
}
