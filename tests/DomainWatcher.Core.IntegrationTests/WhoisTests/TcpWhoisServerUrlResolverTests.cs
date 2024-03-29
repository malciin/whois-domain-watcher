﻿using DomainWatcher.Core.Whois.Implementation;

namespace DomainWatcher.Core.IntegrationTests.WhoisTests;

[TestFixture]
public class TcpWhoisServerUrlResolverTests
{
    private WhoisServerUrlResolver resolver;

    [SetUp]
    public void SetUp()
    {
        resolver = new WhoisServerUrlResolver(new TcpWhoisRawResponseProvider());
    }

    [TestCase("com", ExpectedResult = "whois.verisign-grs.com")]
    [TestCase("dev", ExpectedResult = "whois.nic.google")]
    [TestCase("pl", ExpectedResult = "whois.dns.pl")]
    [TestCase("com.pl", ExpectedResult = "whois.dns.pl")]
    public async Task<string> Resolving_Works(string tld)
    {
        var response = await resolver.Resolve(tld);

        return response!;
    }
}
