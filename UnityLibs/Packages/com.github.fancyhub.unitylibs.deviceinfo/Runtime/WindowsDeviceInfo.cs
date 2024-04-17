/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/2/2
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Runtime.InteropServices;
using UnityEngine;

namespace FH
{
    public static class WindowsDeviceInfo
    {
        private const bool ReturnExcpetion = false;
        private static void _PrintException(System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }

        public static long PhysicallyMemorySize
        {
            get
            {
                try
                {
                    GetPhysicallyInstalledSystemMemory(out var ret);
                    return ret*1024;
                }
                catch (System.Exception ex)
                {
                    if (ReturnExcpetion)
                        throw ex;
                    _PrintException(ex);
                    return default;
                }
            }
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);
    }
}