

## Editor 模式下

AsyncOperation op = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("a.unity", new LoadSceneParameters(LoadSceneMode.Additive));
op.allowSceneActivation = false; //无效


## 普通模式下
AsyncOperation op = SceneManager.LoadSceneAsync("Assets/Scenes/Scenes/a.unity", new LoadSceneParameters(LoadSceneMode.Additive));
op.allowSceneActivation = false;

//直接把之前的 AsyncOperation 的 allowSceneActivation 无效化
// https://docs.unity.cn/cn/current/ScriptReference/SceneManagement.SceneManager.LoadScene.html
SceneManager.LoadScene("b.unity", new LoadSceneParameters(LoadSceneMode.Additive));

## 概述
1. 全部使用异步方法加载
2. 按照队列的方式加载, 同时只能加载一个, 这样才能找到对应的