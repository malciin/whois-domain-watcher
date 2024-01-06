using System.Collections.Generic;
using System.Collections.Immutable;
using DomainWatcher.CodeGenerator.Common.Values;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainWatcher.CodeGenerator.Common.Receivers;

public class DerivedClassReceiver : ISyntaxContextReceiver
{
    private readonly List<ClassInfo> classes = [];
    private readonly ImmutableHashSet<string> baseTypeNames;

    public DerivedClassReceiver(params string[] baseTypeNames)
    {
        this.baseTypeNames = baseTypeNames.ToImmutableHashSet();
    }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration) return;
        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol) return;

        var baseSymbol = classSymbol.BaseType;

        while (true)
        {
            var baseTypeName = baseSymbol?.Name;

            if (baseTypeName == null) return;
            if (baseTypeNames.Contains(baseTypeName))
            {
                classes.Add(new ClassInfo(classDeclaration, classSymbol));
                return;
            }

            baseSymbol = baseSymbol.BaseType;
        }
    }

    public IEnumerable<ClassInfo> Get() => classes;
}
