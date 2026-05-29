namespace NativeLibTool.Models
{
    internal sealed class AndroidSourceAarOptions
    {
        public string SourceDirectory { get; set; }
        public string GradleProjectDirectory { get; set; }
        public string OutputDirectory { get; set; }
        public string ArtifactId { get; set; }
        public string Version { get; set; }
        public string Namespace { get; set; }
        public string UnityDataDirectory { get; set; }
        public string GradleCommand { get; set; }
        public string AndroidGradlePluginVersion { get; set; }
        public string CompileSdk { get; set; }
        public string MinSdk { get; set; }
        public bool AutoDetectSourceRoot { get; set; }
        public bool KeepGradleProject { get; set; }
    }
}
