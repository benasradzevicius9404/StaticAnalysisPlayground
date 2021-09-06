using Microsoft.CodeAnalysis;
using StaticAnalysisPlayground.DependenciesAnalyzer.Models;
using System.Collections.Generic;
using System.Linq;

namespace StaticAnalysisPlayground.DependenciesAnalyzer
{
    public class GraphComponentLinksFinder
    {
        public Graph<NamedComponent<TNode>, ComponentLink> Find<TNode>(ICollection<NamedComponent<TNode>> components, Graph<TNode, TypesLink> graph)
            where TNode : INode
        {
            var links = FindLinks(components, graph);

            return new Graph<NamedComponent<TNode>, ComponentLink>(components.ToDictionary(x => x.Name), new HashSet<ComponentLink>(links));
        }

        private IEnumerable<ComponentLink> FindLinks<TNode>(ICollection<NamedComponent<TNode>> components, Graph<TNode, TypesLink> graph)
            where TNode: INode
        {
            foreach(var component in components)
            {
                foreach(var node in component.Nodes)
                {
                    var implements = graph.GetAllOutgoingLinks(node).Where(x => x.Type == LinkType.Implements).Select(x => graph.Nodes[x.TargetId]);
                    var implementsExternal = implements.Where(x => !component.Nodes.Contains(x));

                    var implementsExternalComponents = components.Where(x => implementsExternal.Any(i => x.Nodes.Contains(i)));

                    foreach(var implementedComponent in implementsExternalComponents)
                    {
                        yield return new ComponentLink(component.Name, implementedComponent.Name);
                    }
                }
            }
        }
    }
}
