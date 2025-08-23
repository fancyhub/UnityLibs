/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/21
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    public partial interface ISceneMgr
    {
        [Serializable]
        public class Config
        {
            public ELogLvl LogLvl = ELogLvl.Info;
        }
    }
   
}