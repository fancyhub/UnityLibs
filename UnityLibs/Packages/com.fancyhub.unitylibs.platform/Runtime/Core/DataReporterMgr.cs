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
        void ReportEvent(string event_name, Dictionary<string, string> event_data);

        /// <summary>
        /// 第三方账号的登录
        /// </summary>
        public void OnAuthLogin();
        public void OnAuthLogout();

        /// <summary>
        /// 游戏账号的登录
        /// </summary>
        public void OnGameLogin();
        public void OnGameLogout();

    }

    public static class DataReporterMgr
    {
        private static List<(EDataReporterChannel channel, IDataReporter reporter)> _Reporter = new();

        public static void Reg(EDataReporterChannel channel, IDataReporter reporter)
        {
            if (reporter == null)
                return;

            foreach (var p in _Reporter)
            {
                if (p.channel == channel)
                    return;
            }

            _Reporter.Add((channel, reporter));
        }

        public static void OnGameLogin()
        {
            foreach (var p in _Reporter)
            {
                p.reporter.OnGameLogin();
            }
        }

        public static void OnGameLogout()
        {
            foreach (var p in _Reporter)
            {
                p.reporter.OnGameLogout();
            }
        }
        public static void OnAuthLogin()
        {
            foreach (var p in _Reporter)
            {
                p.reporter.OnAuthLogin();
            }
        }
        public static void OnAuthLogout()
        {
            foreach (var p in _Reporter)
            {
                p.reporter.OnAuthLogout();
            }
        }


        public static bool UnReg(EDataReporterChannel channel)
        {
            for (int i = 0; i < _Reporter.Count; i++)
            {
                if (_Reporter[i].channel == channel)
                {
                    _Reporter.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public static void ReportEvent(BitEnum32<EDataReporterChannel> channels, string event_name, Dictionary<string, string> event_data)
        {
            foreach (var p in _Reporter)
            {
                if (channels[p.channel])
                    p.reporter.ReportEvent(event_name, event_data);
            }
        }

        public static void ReportEvent(EDataReporterChannel channel, string event_name, Dictionary<string, string> event_data)
        {
            BitEnum32<EDataReporterChannel> mask = BitEnum32<EDataReporterChannel>.Zero;
            mask.SetBit(channel, true);
            ReportEvent(mask, event_name, event_data);
        }

        public static void ReportEvent(string event_name, Dictionary<string, string> event_data)
        {
            ReportEvent(BitEnum32<EDataReporterChannel>.All, event_name, event_data);
        }
    }
}
