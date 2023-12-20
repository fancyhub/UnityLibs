# Scene Manager

## 概述
本身不依赖 AssetBundleManager子类的包, 需要外部把他们Link起来, 可以查看 [Resource Service](../com.github.fancyhub.unitylibs.resourceservice)

## 主要类
```cs
public static class SceneMgr
{
    public static void InitMgr(ISceneLoader scene_loader);
    public static SceneRef LoadScene(string scene_path, bool additive);
    public static void Update();
    public static void Destroy();
}
```