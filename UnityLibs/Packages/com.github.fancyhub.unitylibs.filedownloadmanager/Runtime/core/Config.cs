/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;

namespace FH
{
    public partial interface IFileDownloadMgr
    {
        [Serializable]
        public sealed class Config
        {
            public ELogLvl LogLvl = ELogLvl.Info;

            [Min(1)]
            public int WorkerCount = 4;
            public string ServerUrl;

            [Range(0,5)]
            public int MaxRetryCount = 3;
        }
    }
}
