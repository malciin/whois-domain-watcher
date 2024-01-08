using System.Linq;
using DomainWatcher.CodeGenerator.Common.Extensions;
using DomainWatcher.CodeGenerator.Common.Receivers;
using Microsoft.CodeAnalysis;

namespace DomainWatcher.Cli.SourceGenerator;

[Generator]
public class ServiceCollectionExtensionsSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ClassInterfaceImplementationReceiver("IHttpEndpoint"));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var endpointImplementationsReceiver = (ClassInterfaceImplementationReceiver)context.SyntaxContextReceiver;
        var endpointsRegisterCode = string.Join("\r\n", endpointImplementationsReceiver.Get().Select(x => $"builder.AddEndpoint<{x.Namespace}.{x.Symbol.Name}>();"));

        context.AddSource("ServiceCollectionExtensions.g.cs", $$"""
            using DomainWatcher.Infrastructure.HttpServer;
            using Microsoft.Extensions.DependencyInjection;

            namespace DomainWatcher.Cli.Extensions;
            
            public static partial class ServiceCollectionExtensions
            {
                private static partial HttpServerBuilder AddSourceGeneratedEndpoints(this HttpServerBuilder builder)
                {
                    {{ endpointsRegisterCode.PadEachLine(8).Trim() }}

                    return builder;
                }
            }
            """);
    }
}
