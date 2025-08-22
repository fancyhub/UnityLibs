/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/1 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    public enum EUIPageGroupChannel
    {
        None,
        Free,
        Stack,
        Queue,
    }

    public enum EUIViewLayer
    {
        Normal,
        Dialog,
        Top,
    }

    public enum EUITag : byte
    {
        None = byte.MaxValue,
        BG = 0,
        FullScreenDialog,
        Dialog,
    }

    public enum EUITagIndex
    {
        None,
        BG,
        FullScreenDialog,
        Dialog,
    }
}
