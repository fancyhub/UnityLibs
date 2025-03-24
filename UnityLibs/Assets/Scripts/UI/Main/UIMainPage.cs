using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainPage : FH.UI.UIPageBase<Game.UIMainPanelView>
{
    protected override void OnUI2Init()
    {
        base.OnUI2Init();
        BaseView._BtnTestUIDialog.OnClick = () =>
        {
            FH.UI.UISceneMgr.OpenUI<Game.UITestUIGroupDialog>();
        };
    }     

    protected override void OnUI5Close()
    {
    }     
}
