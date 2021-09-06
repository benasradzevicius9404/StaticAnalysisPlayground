using System.Collections.Generic;

namespace StaticAnalysisPlayground.DependenciesAnalyzer.Models
{
    public record GraphComponent<TNode>(IReadOnlySet<TNode> Nodes) : IGraphComponent<TNode>
        where TNode : INode
    {
    }
}
