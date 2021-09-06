using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Newtonsoft.Json;
using StaticAnalysisPlayground.DependenciesAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StaticAnalysisPlayground.DependenciesAnalyzer
{
    public enum LinkType
    {
        Call,
        Implements,
        Extends,
        Reference
    }

    public record Link(INamedTypeSymbol Source, INamedTypeSymbol Target, LinkType Type) { }

    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="solution">Path to solution file</param>
        static void Main(string solution) 
        {
            var graphBuiler = new TypesGraphBuilder();

            MSBuildLocator.RegisterDefaults();
            var workspace = MSBuildWorkspace.Create();

            Solution solutionToAnalyze = workspace.OpenSolutionAsync(solution).Result;

            foreach (var document in solutionToAnalyze.Projects.Where(x => !x.Name.Contains("Tests")).SelectMany(x => x.Documents))
            {
                var tree = document.GetSyntaxTreeAsync().Result;
                var model = document.GetSemanticModelAsync().Result;

                foreach (var syntax in tree.GetRoot().DescendantNodes())
                {
                    switch (syntax)
                    {
                        case InvocationExpressionSyntax invocation:
                            var symbol = model.GetSymbolInfo(invocation).Symbol; 
                            if (symbol != null) // nameof(Something) for example
                            {
                                var typ = symbol.ContainingType;
                                var containedClass = model.GetDeclaredSymbol(invocation.FirstAncestorOrSelf<TypeDeclarationSyntax>());
                                graphBuiler.AddLink(containedClass, typ, LinkType.Call);
                            }
                            break;
                        case ObjectCreationExpressionSyntax creation:
                            var creationSymbol = model.GetSymbolInfo(creation).Symbol;
                            if (creationSymbol != null) // nameof(Something) for example
                            {
                                var typ = creationSymbol.ContainingType;
                                var containedClass = model.GetDeclaredSymbol(creation.FirstAncestorOrSelf<TypeDeclarationSyntax>());
                                graphBuiler.AddLink(containedClass, typ, LinkType.Call);
                            }
                            break;
                        case ConstructorDeclarationSyntax constructorSyntax:
                            foreach (var parameter in constructorSyntax.ParameterList.Parameters)
                            {
                                var parameterSymbol = model.GetSymbolInfo(parameter.Type).Symbol;
                                if (parameterSymbol is IArrayTypeSymbol arr)
                                {
                                    parameterSymbol = arr.ElementType;
                                    if (parameterSymbol is IArrayTypeSymbol arr2)
                                    {
                                        parameterSymbol = arr2.ElementType;
                                    }
                                }

                                if (parameterSymbol != null && !(parameterSymbol is ITypeParameterSymbol)) // nameof(Something) for example
                                {
                                    var containedClass = model.GetDeclaredSymbol(parameter.FirstAncestorOrSelf<TypeDeclarationSyntax>());
                                    graphBuiler.AddLink(containedClass, (INamedTypeSymbol)parameterSymbol, LinkType.Reference);
                                }
                            }
                            break;
                        case MethodDeclarationSyntax methodSyntax:
                            foreach (var parameter in methodSyntax.ParameterList.Parameters)
                            {
                                var parameterSymbol = model.GetSymbolInfo(parameter.Type).Symbol;
                                if(parameterSymbol is IArrayTypeSymbol arr)
                                {
                                    parameterSymbol = arr.ElementType; 
                                    if (parameterSymbol is IArrayTypeSymbol arr2)
                                    {
                                        parameterSymbol = arr2.ElementType;
                                    }
                                }

                                if (parameterSymbol != null && !(parameterSymbol is ITypeParameterSymbol)) // nameof(Something) for example
                                {
                                    var containedClass = model.GetDeclaredSymbol(parameter.FirstAncestorOrSelf<TypeDeclarationSyntax>());
                                    graphBuiler.AddLink(containedClass, (INamedTypeSymbol)parameterSymbol, LinkType.Reference);
                                }
                            }
                            break;
                        case TypeDeclarationSyntax typeDeclaration:
                            var type = model.GetDeclaredSymbol(typeDeclaration);
                            graphBuiler.AddDeclaration(type);

                            foreach (var implements in type.AllInterfaces)
                            {
                                graphBuiler.AddLink(type, implements, LinkType.Implements);
                            }

                            if (type.BaseType != null)
                            {
                                graphBuiler.AddLink(type, type.BaseType, LinkType.Extends);
                            }

                            type.GetTypeMembers().ToList().ForEach(x => graphBuiler.AddLink(type, x, LinkType.Reference));

                            break;
                    }
                }
            }

            var graph = graphBuiler.Build();
            while(true)
            {
                var baseClassNode = graph.Nodes.Values.Where(n =>
                    !graph.GetAllOutgoingLinks(n).Any() && graph.GetAllIncommingLinks(n).All(l => l.Type == LinkType.Extends || l.Type == LinkType.Implements))
                    .FirstOrDefault();

                if(baseClassNode == null)
                {
                    break;
                }

                graph = graph.Remove(baseClassNode);
            }

            var componentsFinder = new GraphAbstractionsComponentsFinder(); 
            var abstractions = componentsFinder.Find(graph);

            var nodesWithoutComponent = new GraphComponent<TypeNode>(new HashSet<TypeNode>(graph.Nodes.Values.Where(x => !abstractions.Any(a => a.Nodes.Contains(x)))));

            var c = 0;
            var components = new[] { nodesWithoutComponent }.Concat<IGraphComponent<TypeNode>>(abstractions).ToDictionary(x => c++);

            var i = 0;
            var nodeIds = graph.Nodes.ToDictionary(x => x.Key, x => i++);

            var json = JsonConvert.SerializeObject(new
            {
                nodes = graph.Nodes.Values.Select(x => new
                {
                    name = x.Id,
                    size = 1,
                    group = components.First(c => c.Value.Nodes.Contains(x)).Key
                }),
                links = graph.Links
                    .Select(x =>
                    {
                        return new
                        {
                            source = nodeIds[$"{x.SourceId}"],
                            target = nodeIds[$"{x.TargetId}"],
                            type = x.Type
                        };
                    })
            });

            File.AppendAllText("result.json", json);
        }
    }
}
