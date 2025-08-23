/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/21
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public partial interface IFileMgr
    {
        [Serializable]
        public sealed class Config
        {
            public ELogLvl LogLvl = ELogLvl.Info;

            public List<string> BaseTags = new List<string>() { "base" };

            [Tooltip("Android 平台的时候, 需要 从streaming assets 移动到Cache目录的tag")]
            public string AndroidExtractTag = "android";
        }
    }
}