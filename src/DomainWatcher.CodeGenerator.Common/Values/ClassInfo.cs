using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainWatcher.CodeGenerator.Common.Values;

public class ClassInfo
{
    public ClassInfo(ClassDeclarationSyntax declaration, INamedTypeSymbol symbol)
    {
        Symbol = symbol;
        Declaration = declaration;
    }

    public string Namespace => Symbol.ContainingNamespace.Name;

    public INamedTypeSymbol Symbol { get; }

    public ClassDeclarationSyntax Declaration { get; }
}
