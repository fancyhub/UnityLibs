/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/19 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic; 

namespace FH
{

    [Flags]
    public enum EDataReporterChannel
    {
        //svr = 1 << 0,
        beacon = 1 << 1, //灯塔
        //tdm = 1 << 2,
    }


    public static class DataReporterConst
    {        
        //点击进入游戏
        public const string CEnterName = "enter_game";
    }
}
