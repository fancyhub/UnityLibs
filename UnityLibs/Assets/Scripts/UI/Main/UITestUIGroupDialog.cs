using System;
using System.Collections.Generic;

namespace Game
{
    public class UITestUIGroupDialog : FH.UI.UIPageBase<UITestUIGroupDialogView>
    {
        protected override void OnUI2Init()
        {
            base.OnUI2Init();
            BaseView._BtnClose.OnClick = this.UIClose;
            BaseView._Title.text = Id.ToString();

            BaseView._BtnOpenFree.OnClick = () =>
                {
                    FH.UI.UISceneMgr.OpenUI<UITestUIGroupDialog>(new FH.UI.PageOpenInfo()
                    {
                        GroupChannel = FH.UI.EUIPageGroupChannel.Free,
                        AddToScene = true,
                        GroupUniquePage = false,
                    });
                };

            BaseView._BtnOpenFreeUnique.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<UITestUIGroupDialog>(new FH.UI.PageOpenInfo()
                {
                    GroupChannel = FH.UI.EUIPageGroupChannel.Free,
                    AddToScene = true,
                    GroupUniquePage = true,
                });
            };

            BaseView._BtnOpenStack.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<UITestUIGroupDialog>(new FH.UI.PageOpenInfo()
                {
                    GroupChannel = FH.UI.EUIPageGroupChannel.Stack,
                    AddToScene = true,
                    GroupUniquePage = false,
                });
            };

            BaseView._BtnOpenStackUnique.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<UITestUIGroupDialog>(new FH.UI.PageOpenInfo()
                {
                    GroupChannel = FH.UI.EUIPageGroupChannel.Stack,
                    AddToScene = true,
                    GroupUniquePage = true,
                });
            };

            BaseView._BtnOpenQueue.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<UITestUIGroupDialog>(new FH.UI.PageOpenInfo()
                {
                    GroupChannel = FH.UI.EUIPageGroupChannel.Queue,
                    AddToScene = true,
                    GroupUniquePage = false,
                });
            };

            BaseView._BtnOpenQueueUnique.OnClick = () =>
            {
                FH.UI.UISceneMgr.OpenUI<UITestUIGroupDialog>(new FH.UI.PageOpenInfo()
                {
                    GroupChannel = FH.UI.EUIPageGroupChannel.Queue,
                    AddToScene = true,
                    GroupUniquePage = true,
                });
            };
        }
        
        protected override void OnUI5Close()
        {
        }
    }
}
