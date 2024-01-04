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
    public void GivenWhoisResponse_ReturnsCorrectResult(WhoisServerResponseTestCase testCase)
    {
        var whoisResponse = File.ReadAllText($"WhoisTests/Resources/{testCase.WhoisServerUrl}@{testCase.Domain.FullName}.domain.response");

        var response = parser.Parse(testCase.WhoisServerUrl, whoisResponse);
        WhoisServerResponseParsed expectedResponse = testCase;

        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Expiration, Is.EqualTo(expectedResponse.Expiration));
            Assert.That(response.Registration, Is.EqualTo(expectedResponse.Registration));
        });
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.com"),
                Expiration = new DateTime(2028, 09, 14, 04, 00, 00, DateTimeKind.Utc),
                Registration = new DateTime(1997, 09, 15, 04, 00, 00, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("get.dev"),
                Registration = new DateTime(2018, 10, 10, 19, 35, 58, DateTimeKind.Utc),
                Expiration = new DateTime(2023, 10, 10, 19, 35, 58, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.pl"),
                Registration = new DateTime(2002, 09, 19, 13, 00, 00, DateTimeKind.Utc),
                Expiration = new DateTime(2023, 10, 14, 09, 30, 46, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.com.pl"),
                Registration = new DateTime(2001, 10, 23, 13, 00, 00, DateTimeKind.Utc),
                Expiration = new DateTime(2025, 09, 15, 16, 34, 31, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("twitch.tv"),
                WhoisServerUrl = "tvwhois.verisign-grs.com",
                Registration = new DateTime(2009, 06, 08, 22, 31, 23, DateTimeKind.Utc),
                Expiration = new DateTime(2024, 06, 08, 22, 31, 23, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("twitch.tv"),
                Registration = new DateTime(2009, 06, 08, 22, 31, 23, DateTimeKind.Utc),
                Expiration = new DateTime(2024, 06, 08, 22, 31, 23, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("nutki.pl"),
                Registration = new DateTime(2008, 07, 05, 16, 36, 16, DateTimeKind.Utc),
                Expiration = new DateTime(2023, 07, 05, 16, 36, 16, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.io"),
                Registration = new DateTime(2002, 10, 01, 01, 00, 00, DateTimeKind.Utc),
                Expiration = new DateTime(2023, 09, 30, 01, 00, 00, DateTimeKind.Utc)
            };

            var unexistingDomains = new[]
            {
                "some-untaken-domain.dev",
                "some-untaken-domain.com",
                "some-untaken-domain.pl",
                "some-untaken-domain.net",
                "some-untaken-domain.io",
                "some-untaken-domain.tv",
                "some-untaken-domain-but-different-response.net"
            };
            foreach (var domain in unexistingDomains)
            {
                yield return new WhoisServerResponseTestCase
                {
                    Domain = new Domain(domain),
                    Registration = null,
                    Expiration = null
                };
            }
        }
    }

    public class WhoisServerResponseTestCase : WhoisServerResponseParsed
    {
        public required Domain Domain { get; init; }
    
        public string WhoisServerUrl
        {
            get => whoisServerOverride ?? DefaultTldToWhoisServerUrl.First(x => Domain.Tld.EndsWith(x.Key)).Value;
            init => whoisServerOverride = value;
        }

        private string? whoisServerOverride;

        public static implicit operator TestCaseData(WhoisServerResponseTestCase testCase)
        {
            return new TestCaseData([testCase]).SetArgDisplayNames(testCase.WhoisServerUrl, testCase.Domain.FullName);
        }

        private static readonly IReadOnlyDictionary<string, string> DefaultTldToWhoisServerUrl = new Dictionary<string, string>
        {
            ["dev"] = "whois.nic.google",
            ["pl"] = "whois.dns.pl",
            ["tv"] = "whois.nic.tv",
            ["net"] = "whois.verisign-grs.com",
            ["com"] = "whois.verisign-grs.com",
            ["io"] = "whois.nic.io",
        };
    }
}
