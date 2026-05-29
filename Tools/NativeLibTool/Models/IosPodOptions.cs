namespace NativeLibTool.Models
{
    internal sealed class IosPodOptions
    {
        public string SourceDirectory { get; set; }
        public string OutputPodsDirectory { get; set; }
        public string PodName { get; set; }
        public string Version { get; set; }
        public string Summary { get; set; }
        public string Homepage { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public string LicenseType { get; set; }
        public string MinimumIosVersion { get; set; }
        public string SystemFrameworks { get; set; }
        public string SystemLibraries { get; set; }
        public string PodDependencies { get; set; }
        public bool StaticFramework { get; set; }
        public bool GenerateVersionDirectory { get; set; }
        public bool AutoDetectSourceRoot { get; set; }
    }
}
