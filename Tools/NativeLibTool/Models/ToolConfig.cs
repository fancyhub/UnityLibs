namespace NativeLibTool.Models
{
    public sealed class ToolConfig
    {
        public string UnityProjectRoot { get; set; }
        public string AndroidSourceDirectory { get; set; }
        public string AndroidGroupId { get; set; }
        public string AndroidArtifactId { get; set; }
        public string AndroidVersion { get; set; }
        public string IosSourceDirectory { get; set; }
        public string IosPodName { get; set; }
        public string IosVersion { get; set; }
        public string IosMinimumVersion { get; set; }
        public string IosSystemFrameworks { get; set; }
        public string IosSystemLibraries { get; set; }
        public string IosSummaryFormat { get; set; }
        public string IosHomepageFormat { get; set; }
        public string IosLicenseType { get; set; }
        public string IosAuthorName { get; set; }
        public string IosAuthorEmail { get; set; }
        public bool IosStaticFramework { get; set; }
        public bool IosGenerateVersionDirectory { get; set; }
        public string PodfilePatchDefine { get; set; }
    }
}
