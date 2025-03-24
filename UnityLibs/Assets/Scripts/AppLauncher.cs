using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppLauncher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        new FH.UI.UISceneMgr().Init().SetSceneCreator(_CreateScene);
        FH.UI.UISceneMgr.ChangeScene<UISceneUpgrader>();

        FH.TaskQueue.Init(10);
    }

    private static FH.UI.IUIScene _CreateScene(System.Type sceneType)
    {
        if (sceneType == typeof(UISceneUpgrader))
            return new UISceneUpgrader();
        else if (sceneType == typeof(UISceneMain))
            return new UISceneMain();

        return null;
    }
}


public class UISceneUpgrader : FH.UI.UISceneBase
{ 

    public override void OnSceneEnter(Type lastSceneType)
    {
        base.OnSceneEnter(lastSceneType);
        this.OpenPage<UIUpgraderPage>();
    }
}

public class UISceneMain : FH.UI.UISceneBase
{
    public override void OnSceneEnter(Type lastSceneType)
    {
        base.OnSceneEnter(lastSceneType);
        this.OpenPage<UIMainPage>();
    }
}