using DomainWatcher.Core.Enums;
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
    public void ParsingWorksFor(WhoisServerResponseTestCase testCase)
    {
        if (testCase.WhoisServerUrl == null)
        {
            Assert.Fail($"Failed to resolve {nameof(testCase.WhoisServerUrl)} for {testCase.Domain}");
        }

        var whoisResponse = testCase.WhoisResponse;
        var response = parser.Parse(testCase.WhoisServerUrl!, whoisResponse);
        WhoisServerResponseParsed expectedResponse = testCase;

        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Status, Is.EqualTo(expectedResponse.Status));
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
                WhoisServerUrl = "whois.nic.tv",
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
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.eu"),
                Status = WhoisResponseStatus.TakenButTimestampsHidden,
                Registration = null,
                Expiration = null
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.gg"),
                Registration = new DateTime(2003, 4, 30, 0, 0, 0, DateTimeKind.Utc),
                Expiration = new DateTime(
                    // clunky but expiration date is relative to current year
                    DateTime.UtcNow > new DateTime(DateTime.UtcNow.Year, 4, 30, 0, 0, 0, DateTimeKind.Utc)
                        ? DateTime.UtcNow.Year + 1
                        : DateTime.UtcNow.Year,
                    4, 30, 0, 0, 0, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.to"),
                Status = WhoisResponseStatus.TakenButTimestampsHidden,
                Registration = null,
                Expiration = null
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.de"),
                Status = WhoisResponseStatus.TakenButTimestampsHidden,
                Registration = null,
                Expiration = null
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.is"),
                // created:      May 22 2002
                Registration = new DateTime(2002, 5, 22, 0, 0, 0, DateTimeKind.Utc),
                // expires:      May 22 2024
                Expiration = new DateTime(2024, 5, 22, 0, 0, 0, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.biz"),
                // Creation Date: 2002-03-27T16:03:44Z
                Registration = new DateTime(2002, 3, 27, 16, 3, 44, DateTimeKind.Utc),
                // Registry Expiry Date: 2024-03-26T23:59:59Z
                Expiration = new DateTime(2024, 3, 26, 23, 59, 59, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.co"),
                // Creation Date: 2010-02-25T01:04:59Z
                Registration = new DateTime(2010, 2, 25, 1, 4, 59, DateTimeKind.Utc),
                // Registry Expiry Date: 2024-02-24T23:59:59Z
                Expiration = new DateTime(2024, 2, 24, 23, 59, 59, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.fr"),
                // created:                       2000-07-26T22:00:00Z
                Registration = new DateTime(2000, 7, 26, 22, 0, 0, DateTimeKind.Utc),
                // Expiry Date:                   2024-12-30T17:16:48Z
                Expiration = new DateTime(2024, 12, 30, 17, 16, 48, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.info"),
                // Creation Date: 2001-07-31T23:57:50Z
                Registration = new DateTime(2001, 7, 31, 23, 57, 50, DateTimeKind.Utc),
                // Registry Expiry Date: 2024-07-31T23:57:50Z
                Expiration = new DateTime(2024, 7, 31, 23, 57, 50, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.me"),
                // Creation Date: 2008-06-13T17:17:40Z
                Registration = new DateTime(2008, 6, 13, 17, 17, 40, DateTimeKind.Utc),
                // Registry Expiry Date: 2024-06-13T17:17:40Z
                Expiration = new DateTime(2024, 6, 13, 17, 17, 40, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.tech"),
                // Creation Date: 2015-07-29T14:20:05.0Z
                Registration = new DateTime(2015, 7, 29, 14, 20, 5, DateTimeKind.Utc),
                // Registry Expiry Date: 2024-07-29T23:59:59.0Z
                Expiration = new DateTime(2024, 7, 29, 23, 59, 59, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.uk"),
                //         Registered on: 11-Jun-2014
                Registration = new DateTime(2014, 6, 11, 0, 0, 0, DateTimeKind.Utc),
                //         Expiry date:  11-Jun-2024
                Expiration = new DateTime(2024, 6, 11, 0, 0, 0, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.us"),
                // Creation Date: 2002-04-19T23:16:01Z
                Registration = new DateTime(2002, 4, 19, 23, 16, 1, DateTimeKind.Utc),
                // Registry Expiry Date: 2024-04-18T23:59:59Z
                Expiration = new DateTime(2024, 4, 18, 23, 59, 59, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.xyz"),
                // Creation Date: 2014-05-20T12:04:51.0Z
                Registration = new DateTime(2014, 5, 20, 12, 04, 51, DateTimeKind.Utc),
                // Registry Expiry Date: 2024-11-26T23:59:59.0Z
                Expiration = new DateTime(2024, 11, 26, 23, 59, 59, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("google.in"),
                // Creation Date: 2005-02-14T20:35:14Z
                Registration = new DateTime(2005, 2, 14, 20, 35, 14, DateTimeKind.Utc),
                // Registry Expiry Date: 2024-02-14T20:35:14Z
                Expiration = new DateTime(2024, 2, 14, 20, 35, 14, DateTimeKind.Utc)
            };
            yield return new WhoisServerResponseTestCase
            {
                Domain = new Domain("educause.edu"),
                // Domain record activated:    11-Mar-1998
                Registration = new DateTime(1998, 3, 11, 0, 0, 0, DateTimeKind.Utc),
                // Domain expires:             30-May-2025
                Expiration = new DateTime(2025, 5, 30, 0, 0, 0, DateTimeKind.Utc)
            };

            var unexistingDomains = new[]
            {
                "some-untaken-domain-but-different-response.net",
                "some-untaken-domain.dev",
                "some-untaken-domain.com",
                "some-untaken-domain.pl",
                "some-untaken-domain.net",
                "some-untaken-domain.io",
                "some-untaken-domain.tv",
                "some-untaken-domain.eu",
                "some-untaken-domain.gg",
                "some-untaken-domain.de",
                "some-untaken-domain.is",
                "some-untaken-domain.biz",
                "some-untaken-domain.co",
                "some-untaken-domain.fr",
                "some-untaken-domain.info",
                "some-untaken-domain.me",
                "some-untaken-domain.tech",
                "some-untaken-domain.uk",
                "some-untaken-domain.us",
                "some-untaken-domain.xyz",
                "some-untaken-domain.in",
                "some-untaken-domain.to",
                "some-untaken-domain.edu",
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

        public string WhoisResponse => File.ReadAllText($"WhoisTests/Resources/{WhoisServerUrl}@{Domain.FullName}.domain.response");

        public string? WhoisServerUrl
        {
            get => whoisServerOverride ?? ResolveFromDirectory();
            init => whoisServerOverride = value;
        }

        private string? whoisServerOverride;

        public static implicit operator TestCaseData(WhoisServerResponseTestCase testCase)
        {
            return new TestCaseData([testCase]).SetArgDisplayNames(testCase.Domain.FullName);
        }

        private string? ResolveFromDirectory()
        {
            return new DirectoryInfo("WhoisTests/Resources")
                .GetFiles()
                .SingleOrDefault(x => x.Name.EndsWith($"@{Domain.FullName}.domain.response"))
                ?.Name
                .Split('@')[0];
        }
    }
}
