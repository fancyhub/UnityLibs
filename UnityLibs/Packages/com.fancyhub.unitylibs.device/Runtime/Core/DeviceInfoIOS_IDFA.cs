/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/1/4
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FH
{
    // Info.plist need NSUserTrackingUsageDescription    
    public static partial class DeviceInfoIOS
    {
        private static partial class _
        {
            [DllImport("__Internal")] public static extern string FH_GetIDFA();

            [DllImport("__Internal")] public static extern bool FH_IsIDFAReady();
        }

        //UnityEngine.iOS.Device.advertisingIdentifier
        //https://docs.unity3d.com/ScriptReference/iOS.Device.advertisingIdentifier.html
        public static string IDFA => _Call(_.FH_GetIDFA);
        public static bool IsIDFAReady => _Call(_.FH_IsIDFAReady);
    }

}