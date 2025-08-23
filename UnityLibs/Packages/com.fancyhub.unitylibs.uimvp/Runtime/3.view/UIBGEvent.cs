/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
    public static class UIBGEvent
    {
        public static Action GlobalBGClick;
        public static Action<GameObject> GlobalEventClick;
        public static Action<GameObject> GlobalEventDown;
    }
}
