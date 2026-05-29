namespace NativeLibTool.Models
{
    public sealed class ToolConfig
    {
        public string UnityProjectRoot { get; set; }
        public string AndroidAarPath { get; set; }
        public string AndroidSourceDirectory { get; set; }
        public string AndroidAarOutputDirectory { get; set; }
        public string AndroidSourceProjectDirectory { get; set; }
        public string AndroidSourceGradleProjectDirectory { get; set; }
        public string AndroidSourceAarOutputDirectory { get; set; }
        public string AndroidGradleCommand { get; set; }
        public string AndroidGradlePluginVersion { get; set; }
        public string AndroidCompileSdk { get; set; }
        public string AndroidMinSdk { get; set; }
        public string AndroidNamespace { get; set; }
        public string AndroidUnityDataDirectory { get; set; }
        public string AndroidGroupId { get; set; }
        public string AndroidArtifactId { get; set; }
        public string AndroidVersion { get; set; }
        public string IosSourceDirectory { get; set; }
        public string IosPodName { get; set; }
        public string IosVersion { get; set; }
        public string IosMinimumVersion { get; set; }
        public string IosSystemFrameworks { get; set; }
        public string IosSystemLibraries { get; set; }
        public string IosPodDependencies { get; set; }
        public string IosSummaryFormat { get; set; }
        public string IosHomepageFormat { get; set; }
        public string IosLicenseType { get; set; }
        public string IosAuthorName { get; set; }
        public string IosAuthorEmail { get; set; }
        public bool IosStaticFramework { get; set; }
        public bool IosGenerateVersionDirectory { get; set; }
    }
}
