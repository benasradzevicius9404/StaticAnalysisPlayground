using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using StaticAnalysisPlayground.DependenciesAnalyzer.Models;

namespace StaticAnalysisPlayground.DependenciesAnalyzer
{
    public class TypesGraphBuilder
    {
        private HashSet<TypeNode> declaredNodes = new HashSet<TypeNode>();
        private HashSet<TypeNode> nodes = new HashSet<TypeNode>();
        private HashSet<TypesLink> links = new HashSet<TypesLink>();

        public TypesGraphBuilder AddLink(INamedTypeSymbol source, INamedTypeSymbol target, LinkType type)
        {
            var sourceNode = new TypeNode(source);
            var targetNode = new TypeNode(target);

            nodes.Add(sourceNode);
            nodes.Add(targetNode);
            links.Add(new TypesLink(sourceNode, targetNode, type));

            return this;
        }

        public TypesGraphBuilder AddDeclaration(INamedTypeSymbol declaration)
        {
            declaredNodes.Add(new TypeNode(declaration));
            return this;
        }

        public Graph<TypeNode, TypesLink> Build()
        {
            var localNodes = nodes.Where(x => declaredNodes.Contains(x)).ToDictionary(x => x.Id);
            var localLinks = new HashSet<TypesLink>(links.Where(x => localNodes.ContainsKey(x.SourceId) && localNodes.ContainsKey(x.TargetId)));

            return new Graph<TypeNode, TypesLink>(localNodes, localLinks);
        }
    }
}
