/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/07/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace FH.DebugUI
{
    public class SingleButtonView : IDebugUIItemView
    {
        public Button _Button;
        public SingleButtonView(string name, System.Action action)
        {
            _Button = new Button(action);
            _Button.text = name;
        }

        public void OnDebugUIItemEnable(VisualElement content)
        {
            content.Add(_Button);
        }
    }

}
