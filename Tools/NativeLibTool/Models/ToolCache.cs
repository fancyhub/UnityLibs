namespace NativeLibTool.Models
{
    public sealed class ToolCache
    {
        public string UnityProjectRoot { get; set; }
        public string AndroidAarPath { get; set; }
        public string AndroidSourceDirectory { get; set; }
        public bool? AndroidAutoDetectSourceRoot { get; set; }
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
        public bool? AndroidSourceProjectAutoDetectSourceRoot { get; set; }
        public bool? AndroidSourceKeepGradleProject { get; set; }
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
        public string IosPodDependencies { get; set; }
    }
}
