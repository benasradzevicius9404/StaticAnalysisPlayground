namespace StaticAnalysisPlayground.DependenciesAnalyzer.Models
{
    public record TypesLink : ILink
    {
        public string SourceId { get; }
        public string TargetId { get; }
        public LinkType Type { get; }

        public TypesLink(TypeNode source, TypeNode target, LinkType type)
        {
            Type = type;
            SourceId = source.Id;
            TargetId = target.Id;
        }
    }
}
