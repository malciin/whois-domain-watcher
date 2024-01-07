using System.Text;
using Microsoft.Extensions.Configuration;

namespace DomainWatcher.Cli.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddYamlString(this IConfigurationBuilder configurationBuilder, string yaml)
    {
        var bytes = Encoding.UTF8.GetBytes(yaml);

        return configurationBuilder.AddYamlStream(new MemoryStream(bytes));
    }
}
