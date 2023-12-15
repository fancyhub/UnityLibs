/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2019/4/20
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FH.AssetBundleBuilder.Ed
{
    public class BuilderTimer : IDisposable
    {
        private System.DateTime _start_time;
        private string _msg;
        public static int Intend = 0;

        public BuilderTimer(string msg)
        {
            _msg = msg;
            _start_time = DateTime.Now;
            BuilderLog.Log($"{_GetIntendStr(Intend)}Start: {msg} ...............");
            Intend++;
        }

        public void Dispose()
        {
            var end_time = DateTime.Now;
            var elapsed = end_time - _start_time;
            Intend--;
            BuilderLog.Log($"{_GetIntendStr(Intend)}End: {_msg} ..............., {elapsed.TotalSeconds}s");
        }

        private string _GetIntendStr(int intend)
        {
            if (intend <= 0)
                return string.Empty;
            if (intend == 1)
                return "\t";
            if (intend == 2)
                return "\t\t";
            if (intend == 3)
                return "\t\t\t";
            if (intend == 4)
                return "\t\t\t\t";

            return "\t\t\t\t\t";
        }
    }
}
