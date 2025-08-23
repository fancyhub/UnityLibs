/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace FH.UI
{
    public sealed class UISharedBGMono : MonoBehaviour
    {
        public Canvas Mask;
        public Canvas Click;
        public EventTrigger ClickTrigger;
    }
}
