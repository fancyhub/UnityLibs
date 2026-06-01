/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/6/1
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace FH
{
    internal static class DebugConnectionEditorAdbTransport
    {
        public const int DefaultLocalPortStart = 56040;
        public const int DefaultLocalPortEnd = 56060;
        private const int ProcessTimeoutMilliseconds = 10000;

        private static readonly List<ForwardRecord> _ForwardRecords = new List<ForwardRecord>();

        public static string LastError { get; private set; }
        public static string LastAdbPath { get; private set; }

        public static bool ForwardAnyLocalPort(
            string deviceSerial,
            int localStartPort,
            int localEndPort,
            int remotePort,
            out int localPort)
        {
            localPort = 0;

            if (!IsValidPort(localStartPort) || !IsValidPort(localEndPort) || localStartPort > localEndPort)
            {
                LastError = "Invalid adb local port range: " + localStartPort + " - " + localEndPort;
                return false;
            }

            for (int port = localStartPort; port <= localEndPort; port++)
            {
                if (!Forward(deviceSerial, port, remotePort))
                    continue;

                localPort = port;
                return true;
            }

            LastError = "No free adb local port found from " + localStartPort + " to " + localEndPort;
            return false;
        }

        public static bool Forward(string deviceSerial, int localPort, int remotePort)
        {
            if (!IsValidPort(localPort) || !IsValidPort(remotePort))
            {
                LastError = "Invalid adb forward port: " + localPort + " -> " + remotePort;
                return false;
            }

            string arguments = BuildDeviceArguments(deviceSerial)
                + " forward tcp:" + localPort + " tcp:" + remotePort;
            if (!RunAdb(arguments, out string output, out string error))
            {
                LastError = string.IsNullOrEmpty(error) ? output : error;
                return false;
            }

            AddForwardRecord(deviceSerial, localPort);
            LastError = string.Empty;
            return true;
        }

        public static bool RemoveForward(string deviceSerial, int localPort)
        {
            if (!IsValidPort(localPort))
            {
                LastError = "Invalid adb forward port: " + localPort;
                return false;
            }

            string arguments = BuildDeviceArguments(deviceSerial) + " forward --remove tcp:" + localPort;
            if (!RunAdb(arguments, out string output, out string error))
            {
                LastError = string.IsNullOrEmpty(error) ? output : error;
                return false;
            }

            RemoveForwardRecord(deviceSerial, localPort);
            LastError = string.Empty;
            return true;
        }

        public static void RemoveForwards()
        {
            ForwardRecord[] records = _ForwardRecords.ToArray();
            for (int i = 0; i < records.Length; i++)
                RemoveForward(records[i].DeviceSerial, records[i].LocalPort);
        }

        public static string[] GetDeviceSerials()
        {
            if (!RunAdb("devices", out string output, out string error))
            {
                LastError = string.IsNullOrEmpty(error) ? output : error;
                return Array.Empty<string>();
            }

            List<string> serials = new List<string>();
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || !line.EndsWith("\tdevice", StringComparison.Ordinal))
                    continue;

                int tabIndex = line.IndexOf('\t');
                if (tabIndex > 0)
                    serials.Add(line.Substring(0, tabIndex));
            }

            LastError = string.Empty;
            return serials.ToArray();
        }

        private static bool RunAdb(string arguments, out string output, out string error)
        {
            output = string.Empty;
            error = string.Empty;

            string adbPath = FindAdbPath();
            if (string.IsNullOrEmpty(adbPath))
            {
                LastError = "adb not found. Install Android Build Support or add adb to PATH.";
                error = LastError;
                return false;
            }

            LastAdbPath = adbPath;

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        error = "Failed to start adb.";
                        return false;
                    }

                    if (!process.WaitForExit(ProcessTimeoutMilliseconds))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                        }

                        error = "adb command timeout: " + arguments;
                        return false;
                    }

                    output = process.StandardOutput.ReadToEnd().Trim();
                    error = process.StandardError.ReadToEnd().Trim();
                    return process.ExitCode == 0;
                }
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        private static string FindAdbPath()
        {
            string androidSdkRoot = EditorPrefs.GetString("AndroidSdkRoot", string.Empty);
            string sdkAdbPath = FindAdbInSdk(androidSdkRoot);
            if (!string.IsNullOrEmpty(sdkAdbPath))
                return sdkAdbPath;

            string environmentSdkRoot = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
            sdkAdbPath = FindAdbInSdk(environmentSdkRoot);
            if (!string.IsNullOrEmpty(sdkAdbPath))
                return sdkAdbPath;

            string environmentHome = Environment.GetEnvironmentVariable("ANDROID_HOME");
            sdkAdbPath = FindAdbInSdk(environmentHome);
            if (!string.IsNullOrEmpty(sdkAdbPath))
                return sdkAdbPath;

            return "adb";
        }

        private static string FindAdbInSdk(string sdkRoot)
        {
            if (string.IsNullOrEmpty(sdkRoot))
                return string.Empty;

            string adbFileName = Environment.OSVersion.Platform == PlatformID.Win32NT ? "adb.exe" : "adb";
            string adbPath = Path.Combine(sdkRoot, "platform-tools", adbFileName);
            return File.Exists(adbPath) ? adbPath : string.Empty;
        }

        private static string BuildDeviceArguments(string deviceSerial)
        {
            if (string.IsNullOrWhiteSpace(deviceSerial))
                return string.Empty;

            return "-s " + Quote(deviceSerial);
        }

        private static string Quote(string text)
        {
            return "\"" + text.Replace("\"", "\\\"") + "\"";
        }

        private static bool IsValidPort(int port)
        {
            return port > 0 && port <= 65535;
        }

        private static void AddForwardRecord(string deviceSerial, int localPort)
        {
            RemoveForwardRecord(deviceSerial, localPort);
            _ForwardRecords.Add(new ForwardRecord
            {
                DeviceSerial = deviceSerial ?? string.Empty,
                LocalPort = localPort,
            });
        }

        private static void RemoveForwardRecord(string deviceSerial, int localPort)
        {
            string serial = deviceSerial ?? string.Empty;
            for (int i = _ForwardRecords.Count - 1; i >= 0; i--)
            {
                ForwardRecord record = _ForwardRecords[i];
                if (record.LocalPort == localPort
                    && string.Equals(record.DeviceSerial, serial, StringComparison.Ordinal))
                    _ForwardRecords.RemoveAt(i);
            }
        }

        private struct ForwardRecord
        {
            public string DeviceSerial;
            public int LocalPort;
        }
    }
}
