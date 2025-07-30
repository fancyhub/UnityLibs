using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using FH.UI;

namespace Game
{
    public class UITestDeviceInfoPage : UIPageBase<UITestDeviceInfoView>
    { 
        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnClose.OnClick = this.UIClose;            
        }
         
    }
}