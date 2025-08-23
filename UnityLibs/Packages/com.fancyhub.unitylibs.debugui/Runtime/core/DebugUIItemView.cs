/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/07/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace FH.DebugUI
{
    public interface IDebugUIItemView
    {
        public void OnDebugUIItemEnable(VisualElement content);
    }
}