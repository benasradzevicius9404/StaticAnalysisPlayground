using System.Collections.Generic;

namespace StaticAnalysisPlayground.DependenciesAnalyzer.Models
{
    public record NamedComponent<TNode>(string Name, IReadOnlySet<TNode> Nodes) : INode, IGraphComponent<TNode>
        where TNode : INode
    {
        public string Id => Name;
    }
}
