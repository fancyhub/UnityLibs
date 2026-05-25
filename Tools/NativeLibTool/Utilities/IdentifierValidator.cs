using System;
using System.Text.RegularExpressions;

namespace NativeLibTool.Utilities
{
    internal static class IdentifierValidator
    {
        private static readonly Regex MavenPartRegex = new Regex("^[A-Za-z0-9_.-]+$", RegexOptions.Compiled);
        private static readonly Regex PodNameRegex = new Regex("^[A-Za-z0-9_][A-Za-z0-9_.-]*$", RegexOptions.Compiled);

        public static void ValidateMavenCoordinate(string groupId, string artifactId, string version)
        {
            ValidateRequired("GroupId", groupId);
            ValidateRequired("ArtifactId", artifactId);
            ValidateRequired("Version", version);

            if (!MavenPartRegex.IsMatch(groupId))
            {
                throw new InvalidOperationException("GroupId can only contain letters, digits, dot, underscore, and hyphen.");
            }

            if (!MavenPartRegex.IsMatch(artifactId))
            {
                throw new InvalidOperationException("ArtifactId can only contain letters, digits, dot, underscore, and hyphen.");
            }

            if (!MavenPartRegex.IsMatch(version))
            {
                throw new InvalidOperationException("Version can only contain letters, digits, dot, underscore, and hyphen.");
            }
        }

        public static void ValidatePodName(string podName)
        {
            ValidateRequired("PodName", podName);
            if (!PodNameRegex.IsMatch(podName))
            {
                throw new InvalidOperationException("PodName can only contain letters, digits, dot, underscore, and hyphen, and cannot start with dot or hyphen.");
            }
        }

        public static void ValidateRequired(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(name + " is required.");
            }
        }
    }
}
