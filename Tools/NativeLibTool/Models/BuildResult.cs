using System.Collections.Generic;

namespace NativeLibTool.Models
{
    internal sealed class BuildResult
    {
        public BuildResult()
        {
            Warnings = new List<string>();
        }

        public string PrimaryArtifactPath { get; set; }
        public string MetadataPath { get; set; }
        public string ResolvedSourceDirectory { get; set; }
        public string DependencyNotation { get; set; }
        public string RepositorySnippet { get; set; }
        public string DependencySnippet { get; set; }
        public List<string> Warnings { get; private set; }
    }
}
