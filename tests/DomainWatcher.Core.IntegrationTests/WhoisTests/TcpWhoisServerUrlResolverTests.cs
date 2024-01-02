using DomainWatcher.Core.Whois.Implementation;

namespace DomainWatcher.Core.IntegrationTests.WhoisTests;

[TestFixture]
public class TcpWhoisServerUrlResolverTests
{
    private TcpWhoisServerUrlResolver resolver;

    [OneTimeSetUp]
    public void SetUp()
    {
        resolver = new TcpWhoisServerUrlResolver();
    }

    [TestCase("com", ExpectedResult = "whois.verisign-grs.com")]
    [TestCase("dev", ExpectedResult = "whois.nic.google")]
    [TestCase("pl", ExpectedResult = "whois.dns.pl")]
    public async Task<string> Resolving_Works(string tld)
    {
        var response = await resolver.Resolve(tld);

        return response!;
    }
}
