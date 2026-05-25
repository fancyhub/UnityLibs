namespace NativeLibTool.Models
{
    public sealed class ToolCache
    {
        public string UnityProjectRoot { get; set; }
        public string AndroidSourceDirectory { get; set; }
        public bool? AndroidAutoDetectSourceRoot { get; set; }
        public string AndroidGroupId { get; set; }
        public string AndroidArtifactId { get; set; }
        public string AndroidVersion { get; set; }
        public string IosSourceDirectory { get; set; }
        public bool? IosAutoDetectSourceRoot { get; set; }
        public string IosPodName { get; set; }
        public string IosVersion { get; set; }
        public string IosMinimumVersion { get; set; }
        public string IosSystemFrameworks { get; set; }
        public string IosSystemLibraries { get; set; }
    }
}
