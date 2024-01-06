using System.Collections.Generic;
using System.Linq;
using DomainWatcher.CodeGenerator.Common.Values;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainWatcher.CodeGenerator.Common.Receivers;

public class ClassInterfaceImplementationReceiver : ISyntaxContextReceiver
{
    private readonly List<ClassInfo> classes = [];
    private readonly string interfaceName;

    public ClassInterfaceImplementationReceiver(string interfaceName)
    {
        this.interfaceName = interfaceName;
    }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration) return;
        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol) return;
        if (classSymbol.IsAbstract) return;
        if (!classSymbol.Interfaces.Any(x => x.Name == interfaceName)) return;

        classes.Add(new ClassInfo(classDeclaration, classSymbol));
    }

    public IEnumerable<ClassInfo> Get() => classes;
}
