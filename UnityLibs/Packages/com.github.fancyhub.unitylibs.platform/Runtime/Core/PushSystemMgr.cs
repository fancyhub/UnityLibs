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
    public class PushLocalData
    {
        public string Title;
        public string Content;
        public int FireTimeStamp; //触发时间,秒，1970 年
    }

    public interface IPushSystem
    {
        void RegUser(EPushSystemChannel channel, string open_id);
        void UnRegUser(EPushSystemChannel channel);

        void AddTag(EPushSystemChannel channel, string tag);
        void RemoveTag(EPushSystemChannel channel, string tag);

        void ClearLocal(EPushSystemChannel channel);
        void AddLocal(EPushSystemChannel channel, PushLocalData data);        
    }


    public static class PushSystemMgr
    {
        private static bool _Inited = false;
        private static List<IPushSystem> _PushSystemList = new List<IPushSystem>();

        public static void Init(params IPushSystem[] push_systems)
        {
            if (_Inited)
            {
                PlatformLog._.E("PushSystemMgr Is Inited, can't init twice");
                return;
            }
            _Inited = true;
            _PushSystemList.AddRange(push_systems);
        }

        public static void RegUser(string open_id, EPushSystemChannel channel = EPushSystemChannel.All)
        {
            PlatformLog._.Assert(_Inited, "PushSystemMgr Is not inited");

            foreach (var a in _PushSystemList)
            {
                a.RegUser(channel, open_id);
            }
        }

        public static void UnRegUser(EPushSystemChannel channel = EPushSystemChannel.All)
        {
            PlatformLog._.Assert(_Inited, "PushSystemMgr Is not inited");
            foreach (var a in _PushSystemList)
            {
                a.UnRegUser(channel);
            }
        }

        public static void AddTag(string tag, EPushSystemChannel channel = EPushSystemChannel.All)
        {
            PlatformLog._.Assert(_Inited, "PushSystemMgr Is not inited");
            foreach (var a in _PushSystemList)
            {
                a.AddTag(channel, tag);
            }
        }

        public static void RemoveTag(string tag, EPushSystemChannel channel = EPushSystemChannel.All)
        {
            PlatformLog._.Assert(_Inited, "PushSystemMgr Is not inited");
            foreach (var a in _PushSystemList)
            {
                a.RemoveTag(channel, tag);
            }
        }

        public static void PushLocal(PushLocalData data, EPushSystemChannel channel = EPushSystemChannel.All)
        {
            PlatformLog._.Assert(_Inited, "PushSystemMgr Is not inited");
            foreach (var a in _PushSystemList)
            {
                a.AddLocal(channel, data);
            }
        }
        public static void ClearLocal(EPushSystemChannel channel = EPushSystemChannel.All)
        {
            PlatformLog._.Assert(_Inited, "PushSystemMgr Is not inited");
            foreach (var a in _PushSystemList)
            {
                a.ClearLocal(channel);
            }
        }
    }
}
