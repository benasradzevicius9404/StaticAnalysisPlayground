using System;
using System.Collections.Generic;
using System.Linq;
using StaticAnalysisPlayground.DependenciesAnalyzer.Models;

namespace StaticAnalysisPlayground.DependenciesAnalyzer
{
    public class GraphVisitor
    {
        public IEnumerable<(TLink Link, bool Backtrack)> DepthFirstSearch<TNode, TLink>(TNode start, Graph<TNode, TLink> graph, Func<TLink, bool> linkPredicate = null)
            where TLink : ILink
            where TNode : INode
        {
            return DepthFirstVisit(start, graph, linkPredicate, null);
        }

        private IEnumerable<(TLink Link, bool Backtrack)> DepthFirstVisit<TNode, TLink>(TNode start, Graph<TNode, TLink> graph, Func<TLink, bool> linkPredicate = null, HashSet<TNode> visited = null)
            where TLink : ILink
            where TNode : INode
        {
            visited = visited ?? new HashSet<TNode>();

            if(visited.Contains(start))
            {
                yield break;
            }

            visited.Add(start);

            var links = graph.GetAllOutgoingLinks(start);
            if(linkPredicate != null)
            {
                links = links.Where(linkPredicate);
            }

            foreach (var link in links)
            {
                yield return (link, false);

                var targetNode = graph.Nodes[link.TargetId];

                foreach(var visit in DepthFirstVisit(targetNode, graph, linkPredicate, visited))
                {
                    yield return visit;
                }

                yield return (link, true);
            }
        }
    }
}
