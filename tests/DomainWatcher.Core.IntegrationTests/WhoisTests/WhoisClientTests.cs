using DomainWatcher.Core.Enums;
using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Core.Whois.Implementation;
using DomainWatcher.Infrastructure.Sqlite;
using DomainWatcher.Infrastructure.Sqlite.Abstract;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DomainWatcher.Core.IntegrationTests.WhoisTests;

[TestFixture]
public class WhoisClientTests
{
    private WhoisClient client;
    private ServiceProvider serviceProvider;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Service provider to create database for storage for cache TcpWhoisRawResponseProvider and provide real logging
        serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole();
            })
            .AddSqlite($"Data Source=whois_responses_cache.db")
            .AddScoped<IWhoisRawResponseProvider>(ctx => new WhoisResponseCache(
                ctx.GetRequiredService<ILogger<WhoisResponseCache>>(),
                ctx.GetRequiredService<SqliteConnection>(),
                new TcpWhoisRawResponseProvider()))
            .BuildServiceProvider();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        serviceProvider.Dispose();
    }

    [SetUp]
    public async Task SetUp()
    {
        await serviceProvider.GetRequiredService<SqliteDbMigrator>().MigrateIfNecessary();
        var tcpWhoisRawResponseProvider = serviceProvider.GetRequiredService<IWhoisRawResponseProvider>();

        client = new WhoisClient(
            serviceProvider.GetRequiredService<ILogger<WhoisClient>>(),
            new WhoisServerUrlResolver(tcpWhoisRawResponseProvider),
            tcpWhoisRawResponseProvider,
            new WhoisResponseParser());
    }

    [TestCase("google.dev")]
    [TestCase("google.com")]
    [TestCase("google.pl")]
    [TestCase("google.net")]
    [TestCase("google.io")]
    [TestCase("google.tv")]
    [TestCase("google.gg")]
    [TestCase("google.is")]
    [TestCase("google.biz")]
    [TestCase("google.co")]
    [TestCase("google.fr")]
    [TestCase("google.info")]
    [TestCase("google.me")]
    [TestCase("google.tech")]
    [TestCase("google.uk")]
    [TestCase("google.us")]
    [TestCase("google.xyz")]
    [TestCase("google.in")]
    public async Task WhenQueryingDomain_ReturnsDomainTakenWithTimestamps(string domain)
    {
        var result = await client.QueryAsync(new Domain(domain));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsAvailable, Is.False);
            Assert.That(result.Expiration, Is.GreaterThan(DateTime.UtcNow));
            Assert.That(result.Registration, Is.LessThan(DateTime.UtcNow));
        });
    }

    [TestCase("google.de")]
    [TestCase("google.to")]
    [TestCase("google.eu")]
    public async Task WhenQueryingDomainWithWhoisThatHidesTimestamps_ReturnsDomainTakenWithHiddenTimestamps(string domain)
    {
        var result = await client.QueryAsync(new Domain(domain));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsAvailable, Is.False);
            Assert.That(result.Status, Is.EqualTo(WhoisResponseStatus.TakenButTimestampsHidden));
            Assert.That(result.Registration, Is.Null);
            Assert.That(result.Expiration, Is.Null);
        });
    }

    private class WhoisResponseCache(
        ILogger<WhoisResponseCache> logger,
        SqliteConnection connection,
        IWhoisRawResponseProvider implementation) : SqliteCacheService(logger, connection), IWhoisRawResponseProvider
    {
        public Task<string?> GetResponse(string whoisServerUrl, string query)
        {
            return GetFromCachedOrImpl(
                $"{whoisServerUrl} {query}",
                whoisServerUrl == "whois.iana.org" ? TimeSpan.FromDays(7) : TimeSpan.FromHours(1),
                () => implementation.GetResponse(whoisServerUrl, query),
                _ => true);
        }
    }
}
