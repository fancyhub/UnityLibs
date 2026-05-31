/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;

namespace FH
{
    public static class DebugConnectionHistory
    {
        private const string CPrefsKey = "FH.DebugConnection.History";

        [Serializable]
        private sealed class Data
        {
            public List<DebugConnectionHistoryRecord> Records = new List<DebugConnectionHistoryRecord>();
        }

        public static List<DebugConnectionHistoryRecord> GetRecords()
        {
            Data data = Load();
            data.Records.Sort(CompareByUpdatedTime);
            return data.Records;
        }

        public static DebugConnectionHistoryRecord SaveOrUpdate(
            string name,
            string host,
            int port,
            bool autoPort,
            int portCount)
        {
            host = NormalizeHost(host);
            if (string.IsNullOrEmpty(host))
                return null;

            Data data = Load();
            DebugConnectionHistoryRecord record = Find(data.Records, host);
            if (record == null)
            {
                record = new DebugConnectionHistoryRecord();
                data.Records.Add(record);
            }

            record.Name = NormalizeName(name, host);
            record.Host = host;
            record.Port = port;
            record.AutoPort = autoPort;
            record.PortCount = Math.Max(1, portCount);
            record.UpdatedTicks = DateTime.UtcNow.Ticks;

            Save(data);
            return record.Clone();
        }

        public static bool Remove(string host)
        {
            host = NormalizeHost(host);
            if (string.IsNullOrEmpty(host))
                return false;

            Data data = Load();
            for (int i = data.Records.Count - 1; i >= 0; i--)
            {
                if (!IsSameHost(data.Records[i].Host, host))
                    continue;

                data.Records.RemoveAt(i);
                Save(data);
                return true;
            }

            return false;
        }

        public static void Clear()
        {
            EditorPrefs.DeleteKey(CPrefsKey);
        }

        public static bool TryGet(string host, out DebugConnectionHistoryRecord record)
        {
            host = NormalizeHost(host);
            Data data = Load();
            DebugConnectionHistoryRecord found = Find(data.Records, host);
            record = found == null ? null : found.Clone();
            return record != null;
        }

        private static Data Load()
        {
            string json = EditorPrefs.GetString(CPrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json))
                return new Data();

            try
            {
                Data data = new Data();
                EditorJsonUtility.FromJsonOverwrite(json, data);
                return data;
            }
            catch
            {
                return new Data();
            }
        }

        private static void Save(Data data)
        {
            data.Records.RemoveAll(record => record == null || string.IsNullOrEmpty(NormalizeHost(record.Host)));
            data.Records.Sort(CompareByUpdatedTime);
            EditorPrefs.SetString(CPrefsKey, EditorJsonUtility.ToJson(data));
        }

        private static DebugConnectionHistoryRecord Find(List<DebugConnectionHistoryRecord> records, string host)
        {
            foreach (DebugConnectionHistoryRecord record in records)
            {
                if (record == null)
                    continue;

                if (IsSameHost(record.Host, host))
                    return record;
            }

            return null;
        }

        private static string NormalizeHost(string host)
        {
            return string.IsNullOrWhiteSpace(host) ? string.Empty : host.Trim();
        }

        private static string NormalizeName(string name, string host)
        {
            return string.IsNullOrWhiteSpace(name) ? host : name.Trim();
        }

        private static bool IsSameHost(string a, string b)
        {
            return string.Equals(NormalizeHost(a), NormalizeHost(b), StringComparison.OrdinalIgnoreCase);
        }

        private static int CompareByUpdatedTime(DebugConnectionHistoryRecord a, DebugConnectionHistoryRecord b)
        {
            long aTicks = a == null ? 0 : a.UpdatedTicks;
            long bTicks = b == null ? 0 : b.UpdatedTicks;
            return bTicks.CompareTo(aTicks);
        }
    }
}
