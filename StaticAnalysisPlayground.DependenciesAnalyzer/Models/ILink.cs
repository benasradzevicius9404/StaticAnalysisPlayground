namespace StaticAnalysisPlayground.DependenciesAnalyzer.Models
{
    public interface ILink
    {
        public string SourceId { get; }
        public string TargetId { get; }
    }
}
