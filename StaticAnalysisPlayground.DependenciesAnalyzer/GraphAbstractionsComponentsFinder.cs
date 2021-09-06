using StaticAnalysisPlayground.DependenciesAnalyzer.Models;
using System.Collections.Generic;
using System.Linq;

namespace StaticAnalysisPlayground.DependenciesAnalyzer
{
    public class GraphAbstractionsComponentsFinder
    {
        public IEnumerable<GraphComponent<TNode>> Find<TNode>(Graph<TNode, TypesLink> graph)
            where TNode : INode
        {
            var components = FindComponents(graph).ToList();

            return components;

            //var nodesComponentsMap = graph.Nodes.ToDictionary(x => x.Key, x => components.Where(c => c.Nodes.Contains(x.Value)).ToList());
            //var nodesInMultipleComponents = nodesComponentsMap.Where(x => x.Value.Count > 1).Select(x => graph.Nodes[x.Key]).ToList();

            //return components.Select(x => x with { Nodes = new HashSet<TNode>(x.Nodes.Except(nodesInMultipleComponents)) }).Where(x => x.Nodes.Any()).ToList();
        }

        private IEnumerable<GraphComponent<TNode>> FindComponents<TNode>(Graph<TNode, TypesLink> graph)
            where TNode : INode
        {
            var notGroupedNodes = new HashSet<TNode>(graph.Nodes.Values);
            var notAnalyzedImplementations = new HashSet<TypesLink>(graph.Links.Where(x => x.Type == LinkType.Implements));

            var visitor = new GraphVisitor();

            while (notAnalyzedImplementations.Count > 0)
            {
                var componentNodes = new HashSet<TNode>();

                var start = notAnalyzedImplementations.First();

                var implementor = graph.Nodes[start.SourceId];
                componentNodes.Add(implementor);
                componentNodes.Add(graph.Nodes[start.TargetId]);

                if (notGroupedNodes.Contains(implementor))
                {
                    foreach(var call in visitor.DepthFirstSearch(
                            implementor, 
                            graph, 
                            x => x.Type != LinkType.Implements)
                        .Where(x => x.Backtrack == false))
                    {
                        componentNodes.Add(graph.Nodes[call.Link.TargetId]);
                    }
                }

                yield return new GraphComponent<TNode>(componentNodes);

                componentNodes.ToList().ForEach(x => notGroupedNodes.Remove(x));

                notAnalyzedImplementations.Remove(start);
            }
        }
    }
}
