# Asset Bundle Manager

## 概述
Asset Bundle的 管理类



不涉及 
1. 包的下载
2. AssetBundle的打包 (可以查看 [Asset Bundle Builder](../com.fancyhub.unitylibs.assetbundlebuilder))


## 主要类
```cs
public enum EBundleFileStatus
{
    Exist,
    NoExist,
    NeedDownload,
    Downloading,
}

public interface IBundleLoader : ICPtr
{
    /// <summary>
    /// 如果返回null, 使用 AssetBundle.LoadFromFile
    /// 如果返回正确的值, 使用 AssetBundle.LoadFromStream
    /// </summary>
    public Stream LoadBundleFile(string name);
    public string GetBundleFullPath(string name);
    public EBundleFileStatus GetBundleFileStatus(string name);
}

public interface IBundle
{
    public string Name { get; }

    public void GetAllDeps(List<IBundle> deps);

    public bool IsDownloaded();

    public int IncRefCount();
    public int DecRefCount();
    public int RefCount { get; }

    public T LoadAsset<T>(string path) where T : UnityEngine.Object;

    public AssetBundleRequest LoadAssetAsync<T>(string path) where T : UnityEngine.Object;
}

public static class BundleMgr
{
    public static bool InitMgr(IBundleLoader bundle_loader, BundleMgrManifest config);
    public static void Destroy();
    public static IBundle LoadBundleByAsset(string asset);
}
```

不需要 Update

IBundleLoader 需要外部实现, 可以查看 [Resource Service](../com.fancyhub.unitylibs.resourceservice)  

上层的资源加载, 场景加载, 通过 BundleMgr.LoadBundleByAsset, 获取 IBundle对象,  持有他的引用, 如果对应的资源卸载了, 调用 IBundle.DecRefCount 就行了