using System.Collections.Generic;

namespace StaticAnalysisPlayground.DependenciesAnalyzer.Models
{
    public interface IGraphComponent<TNode>
        where TNode : INode
    {
        public IReadOnlySet<TNode> Nodes { get; }
    }
}
