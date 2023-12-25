/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/21
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;

namespace FH
{
    public partial interface IBundleMgr
    {
        [Serializable]
        public class Config
        {
            public ELogLvl LogLvl = ELogLvl.Info;
        }
    }    
}