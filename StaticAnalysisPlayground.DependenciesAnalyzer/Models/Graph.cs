using System;
using System.Collections.Generic;
using System.Linq;

namespace StaticAnalysisPlayground.DependenciesAnalyzer.Models
{
    public record Graph<TNode, TLink>(IReadOnlyDictionary<string, TNode> Nodes, IReadOnlySet<TLink> Links)
        where TLink : ILink
        where TNode : INode
    {
        public IEnumerable<TLink> GetAllOutgoingLinks(TNode start) => Links.Where(x => x.SourceId == start.Id);
        public IEnumerable<TLink> GetAllIncommingLinks(TNode start) => Links.Where(x => x.TargetId == start.Id);

        public Graph<TNode, TLink> Merge(TNode merge1, TNode merge2, TNode newNode, Func<TLink, TLink> linkUpdator) 
        {
            return new Graph<TNode, TLink>(
                Nodes.Values.Except(new[] { merge1, merge2 }).Concat(new [] { newNode }).ToDictionary(x => x.Id),
                new HashSet<TLink>(Links.Where(x => !(x.SourceId == merge1.Id && x.TargetId == merge2.Id)).Select(x =>
                {
                    if(x.SourceId == merge1.Id || x.SourceId == merge2.Id || x.TargetId == merge1.Id || x.TargetId == merge2.Id)
                    {
                        return linkUpdator(x);
                    }
                    return x;
                })));
        }

        public Graph<TNode, TLink> Remove(TNode node)
        {
            return new Graph<TNode, TLink>(
                Nodes.Values.Where(n => n.Id != node.Id).ToDictionary(x => x.Id),
                new HashSet<TLink>(Links.Where(x => x.SourceId != node.Id && x.TargetId != node.Id))
            );
        }
    }
}
