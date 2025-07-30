using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{

    public class UIMainPage : FH.UI.UIPageBase<Game.UIMainPanelView>
    {
        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnTestUIGroupDialog.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<Game.UITestUIGroupDialogPage>();
            };

            BaseView._BtnTestLoadScene.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<Game.UITestScenePage>();
            };

            BaseView._BtnTestPageAsync.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<Game.UITestUIResPage>();
            };

            BaseView._BtnTestDeviceInfo.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<Game.UITestDeviceInfoPage>();
            };

            BaseView._BtnLocalization.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<Game.UITestLocalizationPage>();
            };

            BaseView._BtnUpgrade.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<Game.UITestUpgradePage>();
            };

            BaseView._BtnReloadUIScene.OnClick = () =>
            {
                FH.UI.UISceneMgr.ChangeScene<UISceneMain>();
            };
            
        }

        protected override void OnUI5Close()
        {
        }
    }
}