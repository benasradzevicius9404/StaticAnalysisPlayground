namespace StaticAnalysisPlayground.DependenciesAnalyzer.Models
{
    public record ComponentLink(string SourceId, string TargetId) : ILink { }
}
