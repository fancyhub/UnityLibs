using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH.UI;
using FH;

namespace Game
{

    public class UI3DSceneMainPage : FH.UI.UIPageBase<Game.UI3DSceneMainPanelView>
    {
        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnClose.OnClick = () =>
            {
                UIMgr.ChangeScene<UISceneMain>();
            }; 
        }

        protected override void OnUI5Close()
        {
        }
    }
}