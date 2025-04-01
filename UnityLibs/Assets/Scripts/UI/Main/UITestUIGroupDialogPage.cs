using System;
using System.Collections.Generic;

namespace Game
{
    public class UITestUIGroupDialogPage : FH.UI.UIPageBase<UITestUIGroupDialogView>
    {
        protected override void OnUI2Init()
        {
            base.OnUI2Init();
            BaseView._BtnClose.OnClick = this.UIClose;
            BaseView._Title.text = $"{GroupPageInfo.GroupChannel}_{Id}";

            BaseView._BtnOpenFree.OnClick = () =>
                {
                    FH.UI.UISceneMgr.OpenUI<UITestUIGroupDialogPage>(new FH.UI.PageOpenInfo()
                    {
                        GroupChannel = FH.UI.EUIPageGroupChannel.Free,
                        AddToScene = true,
                        GroupUniquePage = false,
                        ViewLayer = FH.UI.EUIViewLayer.Dialog,
                        Tag = FH.UI.EUITagIndex.Dialog,
                    });
                };

            BaseView._BtnOpenFreeUnique.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<UITestUIGroupDialogPage>(new FH.UI.PageOpenInfo()
                {
                    GroupChannel = FH.UI.EUIPageGroupChannel.Free,
                    AddToScene = true,
                    GroupUniquePage = true,
                    ViewLayer = FH.UI.EUIViewLayer.Dialog,
                    Tag = FH.UI.EUITagIndex.Dialog,
                });
            };

            BaseView._BtnOpenStack.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<UITestUIGroupDialogPage>(new FH.UI.PageOpenInfo()
                {
                    GroupChannel = FH.UI.EUIPageGroupChannel.Stack,
                    AddToScene = true,
                    GroupUniquePage = false,
                    ViewLayer = FH.UI.EUIViewLayer.Dialog,

                    Tag = FH.UI.EUITagIndex.FullScreenDialog,
                });
            };

            BaseView._BtnOpenStackUnique.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<UITestUIGroupDialogPage>(new FH.UI.PageOpenInfo()
                {
                    GroupChannel = FH.UI.EUIPageGroupChannel.Stack,
                    AddToScene = true,
                    GroupUniquePage = true,
                    ViewLayer = FH.UI.EUIViewLayer.Dialog,

                    Tag = FH.UI.EUITagIndex.FullScreenDialog,
                });
            };

            BaseView._BtnOpenQueue.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<UITestUIGroupDialogPage>(new FH.UI.PageOpenInfo()
                {
                    GroupChannel = FH.UI.EUIPageGroupChannel.Queue,
                    AddToScene = true,
                    GroupUniquePage = false,
                    ViewLayer = FH.UI.EUIViewLayer.Dialog,
                    Tag = FH.UI.EUITagIndex.FullScreenDialog,
                });
            };

            BaseView._BtnOpenQueueUnique.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<UITestUIGroupDialogPage>(new FH.UI.PageOpenInfo()
                {
                    GroupChannel = FH.UI.EUIPageGroupChannel.Queue,
                    AddToScene = true,
                    GroupUniquePage = true,
                    ViewLayer = FH.UI.EUIViewLayer.Dialog,
                    Tag = FH.UI.EUITagIndex.FullScreenDialog,
                });
            };
        }

        protected override void OnUI5Close()
        {
        }
    }
}
