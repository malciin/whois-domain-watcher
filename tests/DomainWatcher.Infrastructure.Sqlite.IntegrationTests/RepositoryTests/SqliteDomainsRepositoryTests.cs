using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Values;
using DomainWatcher.Infrastructure.Sqlite.IntegrationTests.TestInfrastructure;

namespace DomainWatcher.Infrastructure.Sqlite.IntegrationTests.RepositoryTests;

[TestFixture]
public class SqliteDomainsRepositoryTests : SqliteIntegrationTestFixture
{
    private IDomainsRepository repository;

    protected override void AdditionalSetupSteps()
    {
        MigrateDb();
        repository = ResolveService<IDomainsRepository>();
    }

    [Test]
    public async Task WatchingDomain_AddsIt()
    {
        var domain = new Domain("google.com");

        await repository.Watch(domain);

        var watchedDomains = await repository.GetWatchedDomains().ToListAsync();
        Assert.That(watchedDomains, Has.Count.EqualTo(1));
        Assert.That(watchedDomains.Contains(domain));
    }

    [Test]
    public async Task WatchingMultipleDomains_Works()
    {
        var domains = new[]
        {
            new Domain("google.com"),
            new Domain("google.eu"),
            new Domain("google.net"),
            new Domain("google.dev"),
        };

        foreach (var domain in domains) await repository.Watch(domain);

        var watchedDomains = await repository.GetWatchedDomains().ToListAsync();
        Assert.That(watchedDomains, Has.Count.EqualTo(domains.Length));
        foreach (var domain in domains) Assert.That(watchedDomains.Contains(domain));
    }

    [Test]
    public async Task IsWatchingWorks()
    {
        var notWatchedNotStored = new Domain("notwatchednotstored.com");
        var storedOnly = new Domain("storedonly.com");
        var watched = new Domain("watched.com");

        await repository.Watch(watched);
        await repository.Store(storedOnly);

        Assert.That(await repository.IsWatched(watched), Is.True);
        Assert.That(await repository.IsWatched(storedOnly), Is.False);
        Assert.That(await repository.IsWatched(notWatchedNotStored), Is.False);
    }
}
