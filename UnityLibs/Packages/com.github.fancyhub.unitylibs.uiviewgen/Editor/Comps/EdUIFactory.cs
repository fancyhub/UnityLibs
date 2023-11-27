/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FH.UI.View.Gen.ED
{
    public partial class EdUIFactory
    {
        public UIViewGenConfig Config;
        public EdUIViewData _data;
        public EdUIFactory(EdUIViewData data)
        {
            _data = data;
            Config = data.Config;
        } 
    }
}
