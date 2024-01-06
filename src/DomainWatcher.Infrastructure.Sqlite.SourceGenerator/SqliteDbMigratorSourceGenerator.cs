using System;
using System.Linq;
using DomainWatcher.CodeGenerator.Common.Extensions;
using DomainWatcher.CodeGenerator.Common.Receivers;
using DomainWatcher.CodeGenerator.Common.Values;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainWatcher.Infrastructure.Sqlite.SourceGenerator;

[Generator]
internal class SqliteDbMigratorSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new DerivedClassReceiver("Migration"));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        const string MigrationEntry = "DomainWatcher.Infrastructure.Sqlite.Internal.Values.MigrationEntry";
        var methodCode = "";

        var derivedClassReceiver = (DerivedClassReceiver)context.SyntaxContextReceiver;
        var migrations = derivedClassReceiver
            .Get()
            .Where(x => !x.Symbol.IsAbstract)
            .Select(x => new MigrationName(x))
            .OrderBy(x => x.Number);

        foreach (var migrationName in migrations)
        {
            methodCode += $$"""
                yield return new {{ MigrationEntry }}
                {
                    Number = {{ migrationName.Number }},
                    Name = "{{ migrationName.Name }}",
                    Factory = () => new {{ migrationName.Symbol.ContainingNamespace }}.{{ migrationName.TypeName }}()
                };


                """;
        }

        context.AddSource("SqliteDbMigrator.g.cs", $$"""
            // {{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}} UTC
            // TypeContext:
            {{string.Join("\r\n", context.Compilation.SyntaxTrees
                .SelectMany(x => x.GetRoot().DescendantNodes())
                .Where(x => x is ClassDeclarationSyntax)
                .Cast<ClassDeclarationSyntax>()
                .Select(x => "// " + x.Identifier.ValueText)
                .ToList())}}

            namespace DomainWatcher.Infrastructure.Sqlite;

            public partial class SqliteDbMigrator
            {
                private static partial IEnumerable<{{MigrationEntry}}> GetMigrations()
                {
            {{methodCode.PadEachLine(8).TrimEnd()}}
                }
            }
            """);
    }

    private class MigrationName : ClassInfo
    {
        public MigrationName(ClassInfo classInfo)
            : base(classInfo.Declaration, classInfo.Symbol)
        {
            TypeName = Symbol.Name;

            var values = TypeName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            Number = int.Parse(values[0]);
            Name = values[1];
        }

        public int Number { get; }
        
        public string Name { get; }
        
        public string TypeName { get; }
    }
}
