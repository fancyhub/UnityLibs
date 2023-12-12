# Resource Manager

## 概述
不涉及
1. AssetBundle的加载
2. 打包
3. 资源更新

是一个资源管理类, 包括如下几个部分
1. 资源的加载
2. 实例的对象的管理
3. 预实例化
4. 空GameObject的实例化对象管理
5. ResInstHolder

注意:
1. 可以使用 Addressable , 或者其他的Asset打包相关的部分, 需要自己实现 IAssetLoader接口  
2. 不支持一个 Texture 多个Sprite的资源, 只能加载第一个sprite, 如果确实需要多个Sprite, 自己写一个ScriptableObject 持有多个引用
3. 理论上支持 异步Atlas加载, 但是内存释放的时机不确定, 所以会出现不可控的问题, 最好是Atlas 勾选include in build
4. 最好是使用ResInstHolder,可以查看最外层代码 FH.UI.UIBaseView.CreateView, 比如 一个角色用一个 ResInstHolder, 一个UI页面用一个ResInstHolder, 该页面的相关的资源加载都通过该ResInstHolder, UI关闭之后, 直接 ResInstHolder.Destroy()
5. 回收 GameObjectInst的时候, 如果一个组件实现了FH.IDynamicComponent, 会被调用 OnDynamicRelease 方法

## 入口类 FH.ResMgr

```cs
public static class ResMgr
{
	public static bool InitMgr(IAssetLoader asset_loader, ResMgrConfig conf);	
	public static void Update();
	public static void Destroy();
	
	public static EAssetStatus GetAssetStatus(string path)
	
	public static IResInstHolder CreateHolder(bool sync_load_enable)
	 
 
	#region Res
	public static EResError Load(string path, bool sprite, bool aync_load_enable, out ResRef res_ref);
	public static EResError AsyncLoad(string path, bool sprite, int priority, ResEvent cb, out int job_id);
	public static void ResSnapshot(ref List<ResSnapShotItem> out_snapshot);	
	#endregion

	#region GameObject Inst
	public static EResError Create(string path, System.Object user, bool aync_load_enable, out ResRef res_ref);
	public static EResError AsyncCreate(string path, int priority, ResEvent cb, out int job_id);
	#endregion

	#region 预实例化
	public static EResError ReqPreInst(string path, int count, out int req_id);
	public static EResError CancelPreInst(int req_id);
	#endregion;

	#region Empty
	public static EResError CreateEmpty(System.Object user, out ResRef res_ref);
	#endregion

	public static void CancelJob(int job_id);	
}
```



## 初始化, Update,Destroy
### 初始化
```cs
//FH.ResMgr.InitMgr(new FH.Res.SampleAssetLoader.AssetLoader_Resource(), new FH.ResMgrConfig());
FH.ResMgr.InitMgr(new FH.Res.SampleAssetLoader.AssetLoader_AssetDatabase(), new FH.ResMgrConfig());
```
需要实现的接口

```cs
public enum EAssetStatus
{
    Exist,
    NotExist,
    NotDownloaded,
}

public interface IAssetRef : ICPtr
{
    bool IsDone { get; }
    UnityEngine.Object Asset { get; }
}

public interface IAssetLoader : ICPtr
{
    IAssetRef Load(string path, bool sprite);
    IAssetRef LoadAsync(string path, bool sprite);

    string AtlasTag2Path(string atlasName);
    EAssetStatus GetAssetStatus(string path);
}
```

### Update

```cs
FH.ResMgr.Update();
```

### Destroy

```cs
FH.ResMgr.Destroy();
```


## 资源引用
Res, GameObjectInst, EmptyGameObjectInst 这3种资产 都需要引用  
GameObjectInst 和 EmptyGameObjectInst 的同步方法, 必须一开始就给使用者  
Res 和 其他异步方法都是等到 调用者获得 该ResRef之后才添加使用者的  

