# Task Queue

## 概述
推荐使用 [UniTask](https://github.com/Cysharp/UniTask)

TaskQueue 是为了解决多线程的问题用的,  IOS版本的Unity 对于ThreadPool支持不太好, 只能自己写, 而且也需要在主线程回调

```cs
TaskQueue.Start(10, false);
CPtr<ITask> task= TaskQueue.AddTask(
	()=>
	{
		//Do Some thing
		System.Threading.Thread.Sleep(1000);
	}, 
	()=>
	{
		UnityEngine.Debug.Log("Job Done");
	});
	
//task.Cancel();
```


```cs 
async UniTask DemoAsync()
{
	await UniTask.SwitchToThreadPool();
	
	//Do Something
	System.Threading.Thread.Sleep(1000);
	
	
	await UniTask.SwitchToMainThread();
	
	
	UnityEngine.Debug.Log("Job Done");
}
    
```