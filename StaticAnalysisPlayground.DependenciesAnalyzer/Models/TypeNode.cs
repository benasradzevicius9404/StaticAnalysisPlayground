using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace StaticAnalysisPlayground.DependenciesAnalyzer.Models
{
    public class TypeNode : INode, IEquatable<TypeNode>
    {
        public string Id { get; }
        public INamedTypeSymbol Type { get; }

        public TypeNode(INamedTypeSymbol typeSymbol)
        {
            this.Type = typeSymbol;
            Id = $"{typeSymbol.ContainingNamespace}.{typeSymbol.Name}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TypeNode);
        }

        public bool Equals(TypeNode other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(TypeNode left, TypeNode right)
        {
            return EqualityComparer<TypeNode>.Default.Equals(left, right);
        }

        public static bool operator !=(TypeNode left, TypeNode right)
        {
            return !(left == right);
        }
    }
}
