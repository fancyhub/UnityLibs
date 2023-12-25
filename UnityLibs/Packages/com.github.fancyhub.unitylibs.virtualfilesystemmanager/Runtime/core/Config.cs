/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/15
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;

namespace FH
{
    public partial interface IVfsMgr
    {
        [Serializable]
        public sealed class Config
        {
            public ELogLvl LogLvl = ELogLvl.Info;
        }
    }    
}
