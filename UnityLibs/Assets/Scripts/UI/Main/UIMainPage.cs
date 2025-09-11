using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH.UI;

namespace Game
{

    public class UIMainPage : FH.UI.UIPageBase<Game.UIMainPanelView>
    {
        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnTestUIGroupDialog.OnClick = () =>
            {
                FH.UI.UIMgr.OpenUI<Game.UITestUIGroupDialogPage>();
            };

            BaseView._BtnTestLoadScene.OnClick = () =>
            {
                FH.UI.UIMgr.OpenUI<Game.UITestScenePage>();
            };

            BaseView._BtnTestPageAsync.OnClick = () =>
            {
                FH.UI.UIMgr.OpenUI<Game.UITestUIResPage>();
            };

            BaseView._BtnTestDeviceInfo.OnClick = () =>
            {
                FH.UI.UIMgr.OpenUI<Game.UITestDeviceInfoPage>();
            };

            BaseView._BtnLocalization.OnClick = () =>
            {
                FH.UI.UIMgr.OpenUI<Game.UITestLocalizationPage>();
            };

            BaseView._BtnUpgrade.OnClick = () =>
            {
                FH.UI.UIMgr.OpenUI<Game.UITestUpgradePage>();
            };

            BaseView._BtnReloadUIScene.OnClick = () =>
            {
                FH.UI.UIMgr.ChangeScene<UISceneMain>();
            };

            BaseView._BtnTime.OnClick = () =>
            {
                FH.UI.UIMgr.OpenUI<Game.UITestTimePage>();
            };
            
            BaseView._BtnPermission.OnClick = () =>
            {
                FH.UI.UIMgr.OpenUI<Game.UITestPermissionPage>();
            };

            BaseView._BtnScroller.OnClick = () =>
            {
                UIMgr.OpenUI<UITestScrollerPage>();
            };
        }

        protected override void OnUI5Close()
        {
        }
    }
}