/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2019/8/7
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace FH
{
    public interface IPBMessage
    {
        void Serialize(PBWriter writer);
        bool Unserialize(PBReader reader);        
    }
}
