using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using FH.UI;

namespace Game
{
    public class UITestDeviceInfoPage : UIPageBase<UITestDeviceInfoView>
    { 
        protected override void OnUI2Init()
        {
            base.OnUI2Init();
            BaseView._BtnClose.OnClick = this.UIClose;            
        }
         
    }
}