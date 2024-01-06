using System;
using System.Linq;
using DomainWatcher.CodeGenerator.Common.Extensions;
using DomainWatcher.CodeGenerator.Common.Receivers;
using Microsoft.CodeAnalysis;

namespace DomainWatcher.Core.SourceGenerator;

[Generator]
public class WhoisResponseParserSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new DerivedClassReceiver("WhoisServerResponseParser"));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var derivedClassReceiver = (DerivedClassReceiver)context.SyntaxContextReceiver;
        var migrations = derivedClassReceiver
            .Get()
            .Where(x => !x.Symbol.IsAbstract);

        var methodCode = "";

        foreach (var migrationName in migrations)
        {
            methodCode += $$"""
                yield return new {{ migrationName.Symbol.Name }}();

                """;
        }

        context.AddSource("WhoisResponseParser.g.cs", $$"""
            // {{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}} UTC

            using DomainWatcher.Core.Whois.Parsers;

            namespace DomainWatcher.Core.Whois.Implementation;

            public partial class WhoisResponseParser
            {
                private static partial IEnumerable<WhoisServerResponseParser> GetParsers()
                {
            {{methodCode.PadEachLine(8).TrimEnd()}}
                }
            }
            """);
    }
}