using System.Text;
using UnityEngine;

namespace FH
{
    internal static class DebugConnectionHello
    {
        public const string RoleEditor = "Editor";
        public const string RolePlayer = "Player";

        public static byte[] CreatePayload(string role)
        {
            string text = role + " " + Application.platform + " " + SystemInfo.deviceName;
            return Encoding.UTF8.GetBytes(text);
        }

        public static string ParsePayload(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return string.Empty;

            return Encoding.UTF8.GetString(payload);
        }

        public static bool IsRole(byte[] payload, string role)
        {
            if (string.IsNullOrEmpty(role))
                return true;

            string text = ParsePayload(payload);
            return string.Equals(text, role, System.StringComparison.Ordinal)
                || text.StartsWith(role + " ", System.StringComparison.Ordinal);
        }
    }
}
