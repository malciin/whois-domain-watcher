﻿using System.Net;
using DomainWatcher.Core.Enums;
using DomainWatcher.Core.Repositories;
using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.E2ETests.TestInfrastructure;
using DomainWatcher.Infrastructure.HttpServer.Contracts;

namespace DomainWatcher.E2ETests;

public class WhoisEndpointTests : E2ETestFixture
{
    [TestCase("google.com")]
    [TestCase("google.net")]
    [TestCase("google.dev")]
    [TestCase("google.zip")]
    public async Task WhoisQuerying_Works(string domainName)
    {
        var url = $"http://localhost:{Resolve<IHttpServerInfo>().AssignedPort}/{domainName}";
        var domain = new Domain(domainName);

        using var response = await HttpClient.GetAsync(url);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var responseString = await response.Content.ReadAsStringAsync();
        
        var whoisResponsesRepository = Resolve<IWhoisResponsesRepository>();
        var storedResponseInDb = await whoisResponsesRepository.GetLatestFor(domain);
        var domainResponsesIds = await whoisResponsesRepository.GetWhoisResponsesIdsFor(domain).ToListAsync();

        Assert.That(await Resolve<IDomainsRepository>().IsWatched(domain), Is.False, "Querying whois response for a domain should not make it watched.");
        Assert.That(storedResponseInDb, Is.Not.Null, "Response was not stored in database.");
        Assert.That(storedResponseInDb.Id, Is.Not.EqualTo(0), "Stored response has invalid id.");
        Assert.That(storedResponseInDb.Domain, Is.EqualTo(domain), "Stored response has invalid domain.");
        Assert.That(storedResponseInDb.IsAvailable, Is.False, "Stored response has invalid availability status.");
        Assert.That(storedResponseInDb.Status, Is.EqualTo(WhoisResponseStatus.OK), "Stored response has invalid status");
        Assert.That(storedResponseInDb.SourceServer, Is.EqualTo(await Resolve<IWhoisServerUrlResolver>().ResolveFor(domain)), "Stored response has invalid server");
        Assert.That(domainResponsesIds, Has.Count.EqualTo(1), "Unexpected amount of domain responses stored");
        Assert.That(domainResponsesIds, Is.EquivalentTo(new[] { storedResponseInDb.Id }), "Unexpected id stored");
        Assert.That(responseString, Is.EqualTo(storedResponseInDb!.RawResponse.TrimEnd()), "Invalid whois response returned");
    }
}
