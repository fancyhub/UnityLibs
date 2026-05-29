namespace NativeLibTool.Models
{
    internal sealed class AndroidAarPackageOptions
    {
        public string SourcePath { get; set; }
        public string OutputDirectory { get; set; }
        public string ArtifactId { get; set; }
        public string Version { get; set; }
        public bool AutoDetectSourceRoot { get; set; }
    }
}
