namespace NativeLibTool.Models
{
    internal sealed class AndroidMavenPublishOptions
    {
        public string AarPath { get; set; }
        public string OutputRepositoryDirectory { get; set; }
        public string GroupId { get; set; }
        public string ArtifactId { get; set; }
        public string Version { get; set; }
        public string DependencyConfiguration { get; set; }
        public bool GenerateChecksums { get; set; }
    }
}
