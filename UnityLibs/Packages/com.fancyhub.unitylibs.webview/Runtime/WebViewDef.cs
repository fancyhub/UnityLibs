/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/09/28
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
 
namespace FH
{
    public enum EWebViewEventType
    {
        DocumentReady = 1,
        Destroyed = 2,
    }

    internal static class WebViewDef
    {        
        public const string JsHostObjName = "FH";
    }   
}

