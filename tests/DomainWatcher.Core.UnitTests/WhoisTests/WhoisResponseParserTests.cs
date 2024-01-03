using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois.Implementation;
using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.UnitTests.WhoisTests;

[TestFixture]
public class WhoisResponseParserTests
{
    private WhoisResponseParser parser;

    [SetUp]
    public void SetUp()
    {
        parser = new WhoisResponseParser();
    }

    [TestCaseSource(nameof(TestCases))]
    public void GivenWhoisResponse_ReturnsCorrectResult(Domain domain, WhoisServerResponseParsed expectedResponse)
    {
        var whoisServerUrl = GetWhoisServerUrl(domain);
        var whoisResponse = File.ReadAllText($"WhoisTests/Resources/{whoisServerUrl}@{domain.FullName}.domain.response");

        var response = parser.Parse(whoisServerUrl, whoisResponse);

        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Expiration, Is.EqualTo(expectedResponse.Expiration));
            Assert.That(response.Registration, Is.EqualTo(expectedResponse.Registration));
        });
    }

    private static string GetWhoisServerUrl(Domain domain) => TldToWhoisServerUrl.First(x => domain.Tld.EndsWith(x.Key)).Value;

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
            yield return CreateTestCaseFor(new Domain("google.com"), new WhoisServerResponseParsed
            {
                Expiration = new DateTime(2028, 09, 14, 04, 00, 00, DateTimeKind.Utc),
                Registration = new DateTime(1997, 09, 15, 04, 00, 00, DateTimeKind.Utc)
            });
            yield return CreateTestCaseFor(new Domain("get.dev"), new WhoisServerResponseParsed
            {
                Registration = new DateTime(2018, 10, 10, 19, 35, 58, DateTimeKind.Utc),
                Expiration = new DateTime(2023, 10, 10, 19, 35, 58, DateTimeKind.Utc)
            });
            yield return CreateTestCaseFor(new Domain("google.pl"), new WhoisServerResponseParsed
            {
                Registration = new DateTime(2002, 09, 19, 13, 00, 00, DateTimeKind.Utc),
                Expiration = new DateTime(2023, 10, 14, 09, 30, 46, DateTimeKind.Utc)
            });
            yield return CreateTestCaseFor(new Domain("google.com.pl"), new WhoisServerResponseParsed
            {
                Registration = new DateTime(2001, 10, 23, 13, 00, 00, DateTimeKind.Utc),
                Expiration = new DateTime(2025, 09, 15, 16, 34, 31, DateTimeKind.Utc)
            });
            yield return CreateTestCaseFor(new Domain("twitch.tv"), new WhoisServerResponseParsed
            {
                Registration = new DateTime(2009, 06, 08, 22, 31, 23, DateTimeKind.Utc),
                Expiration = new DateTime(2024, 06, 08, 22, 31, 23, DateTimeKind.Utc)
            });
            yield return CreateTestCaseFor(new Domain("nutki.pl"), new WhoisServerResponseParsed
            {
                Registration = new DateTime(2008, 07, 05, 16, 36, 16, DateTimeKind.Utc),
                Expiration = new DateTime(2023, 07, 05, 16, 36, 16, DateTimeKind.Utc)
            });
            yield return CreateTestCaseFor(new Domain("google.io"), new WhoisServerResponseParsed
            {
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
                yield return CreateTestCaseFor(new Domain(tld), new WhoisServerResponseParsed
                {
                    Registration = null,
                    Expiration = null
                });
            }
        }
    }

    private static TestCaseData CreateTestCaseFor(Domain domain, WhoisServerResponseParsed response)
    {
        return new TestCaseData([domain, response]).SetArgDisplayNames(domain.FullName);
    }
}
