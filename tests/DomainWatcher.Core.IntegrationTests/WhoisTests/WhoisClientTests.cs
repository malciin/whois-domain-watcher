using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Core.Whois.Implementation;
using Moq;

namespace DomainWatcher.Core.IntegrationTests.WhoisTests;

[TestFixture]
public class WhoisClientTests
{
    private Mock<IWhoisServerUrlResolver> whoisServerUrlResolver;
    private Mock<IWhoisRawClient> whoisRawClient;
    private WhoisClient client;

    [SetUp]
    public void SetUp()
    {
        whoisServerUrlResolver = new Mock<IWhoisServerUrlResolver>();
        whoisRawClient = new Mock<IWhoisRawClient>();

        client = new WhoisClient(whoisServerUrlResolver.Object, whoisRawClient.Object);
    }

    [TestCaseSource(nameof(TestCases))]
    public async Task GivenWhoisResponse_ReturnsCorrectResult(Domain domain, WhoisResponse expectedResponse)
    {
        var whoisServerUrl = TldToWhoisServerUrl.First(x => domain.Tld.EndsWith(x.Key)).Value;
        var whoisResponse = File.ReadAllText($"WhoisTests/Resources/{whoisServerUrl}@{domain.FullName}.domain.response");
        whoisServerUrlResolver.Setup(x => x.Resolve(domain.Tld)).ReturnsAsync(whoisServerUrl);
        whoisRawClient.Setup(x => x.QueryAsync(whoisServerUrl, domain)).ReturnsAsync(whoisResponse);

        var response = await client.QueryAsync(domain);

        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Domain, Is.EqualTo(expectedResponse.Domain));
            Assert.That(response.Expiration, Is.EqualTo(expectedResponse.Expiration));
            Assert.That(response.Registration, Is.EqualTo(expectedResponse.Registration));
            Assert.That(response.IsAvailable, Is.EqualTo(expectedResponse.IsAvailable));
        });
    }

    private static readonly IReadOnlyDictionary<string, string> TldToWhoisServerUrl = new Dictionary<string, string>
    {
        ["dev"] = "whois.nic.google",
        ["pl"] = "whois.dns.pl",
        ["tv"] = "tvwhois.verisign-grs.com",
        ["net"] = "whois.verisign-grs.com",
        ["com"] = "whois.verisign-grs.com",
        ["io"] = "whois.nic.io",
    };

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return CreateTestCaseFor(new WhoisResponse
            {
                Domain = new Domain("google.com"),
                Expiration = new DateTime(2028, 09, 14, 04, 00, 00, DateTimeKind.Utc),
                Registration = new DateTime(1997, 09, 15, 04, 00, 00, DateTimeKind.Utc)
            });
            yield return CreateTestCaseFor(new WhoisResponse
            {
                Domain = new Domain("get.dev"),
                Registration = new DateTime(2018, 10, 10, 19, 35, 58, DateTimeKind.Utc),
                Expiration = new DateTime(2023, 10, 10, 19, 35, 58, DateTimeKind.Utc)
            });
            yield return CreateTestCaseFor(new WhoisResponse
            {
                Domain = new Domain("google.pl"),
                Registration = new DateTime(2002, 09, 19, 13, 00, 00, DateTimeKind.Utc),
                Expiration = new DateTime(2023, 10, 14, 09, 30, 46, DateTimeKind.Utc)
            });
            yield return CreateTestCaseFor(new WhoisResponse
            {
                Domain = new Domain("google.com.pl"),
                Registration = new DateTime(2001, 10, 23, 13, 00, 00, DateTimeKind.Utc),
                Expiration = new DateTime(2025, 09, 15, 16, 34, 31, DateTimeKind.Utc)
            });
            yield return CreateTestCaseFor(new WhoisResponse
            {
                Domain = new Domain("twitch.tv"),
                Registration = new DateTime(2009, 06, 08, 22, 31, 23, DateTimeKind.Utc),
                Expiration = new DateTime(2024, 06, 08, 22, 31, 23, DateTimeKind.Utc)
            });
            yield return CreateTestCaseFor(new WhoisResponse
            {
                Domain = new Domain("nutki.pl"),
                Registration = new DateTime(2008, 07, 05, 16, 36, 16, DateTimeKind.Utc),
                Expiration = new DateTime(2023, 07, 05, 16, 36, 16, DateTimeKind.Utc)
            });
            yield return CreateTestCaseFor(new WhoisResponse
            {
                Domain = new Domain("google.io"),
                Registration = new DateTime(2002, 10, 01, 01, 00, 00, DateTimeKind.Utc),
                Expiration = new DateTime(2023, 09, 30, 01, 00, 00, DateTimeKind.Utc)
            });

            var unexistingDomains = new[]
            {
                "some-untaken-domain.dev",
                "some-untaken-domain.com",
                "some-untaken-domain.pl",
                "some-untaken-domain.net",
                "some-untaken-domain.io",
                "some-untaken-domain-but-different-response.net"
            };
            foreach (var tld in unexistingDomains)
            {
                yield return CreateTestCaseFor(new WhoisResponse
                {
                    Domain = new Domain(tld),
                    Registration = null,
                    Expiration = null
                });
            }
        }
    }

    private static TestCaseData CreateTestCaseFor(WhoisResponse response)
    {
        return new TestCaseData([response.Domain, response]).SetArgDisplayNames(response.Domain.FullName);
    }
}
