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
    public interface IDataReporter
    {
        void ReportEvent(EDataReporterChannel channel, string event_name, Dictionary<string, string> event_data);
    }

    public static class DataReporterMgr
    {
        private static bool _Inited = false;
        private static List<IDataReporter> _Reporter = new List<IDataReporter>();
        public static void Init(params IDataReporter[] reporters)
        {
            if (_Inited)
            {
                PlatformLog._.E("DataReporterMgr Is Inited, can't init twice");
                return;
            }
            _Inited = true;
            _Reporter.AddRange(reporters);
        }

        public static void ReportEvent(EDataReporterChannel channel, string event_anme, Dictionary<string, string> event_data)
        {
            PlatformLog._.Assert(_Inited, "DataReporterMgr Is not inited");

            foreach (var r in _Reporter)
            {
                r.ReportEvent(channel, event_anme, event_data);
            }
        }
    }
}
