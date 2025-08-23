# Memory Pool

## 概述
有5个功能
1. ICPtr 和 CPtr<IPtr>
2. ISPtr 和 SPtr<ISPtr>
3. IPoolItem , IPool, GPool
4. SimplePool
5. Boxing<Struct>


以及一些为了方便使用的类
1. CPtrBase 
2. CPoolItemBase
3. SPoolItemBase
4. PtrList

PtrList 的使用方法  
```cs
//
public class SomeClass
{
    public PtrList PtrList;

    public void CreateSomeThing()
    {
        PtrList +=new ICPtr1();
        PtrList +=new ICPtr2();
        PtrList +=new ICPtr3();
    }

    public void Destroy()
    {
        //按照添加顺序, 反过来销毁
        PtrList?.Destroy();
        PtrList=null;
    }
}
```
