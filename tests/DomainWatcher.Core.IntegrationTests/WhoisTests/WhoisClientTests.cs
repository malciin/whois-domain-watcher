using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Core.Whois.Implementation;
using Moq;

namespace DomainWatcher.Core.IntegrationTests.WhoisTests;

[TestFixture]
public class WhoisClientTests
{
    private Mock<IWhoisServerUrlResolver> whoisServerUrlResolver;
    private WhoisClient client;

    [SetUp]
    public void SetUp()
    {
        whoisServerUrlResolver = new Mock<IWhoisServerUrlResolver>();
        client = new WhoisClient(whoisServerUrlResolver.Object, new WhoisRawClient());
    }

    [Test]
    public async Task WhenQueryingGoogleCom_ItReturnsDomainIsTaken()
    {
        whoisServerUrlResolver.Setup(x => x.Resolve("com")).ReturnsAsync("whois.verisign-grs.com");

        var result = await client.QueryAsync(new Domain("google.com"));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsAvailable, Is.False);
            Assert.That(result.Expiration, Is.GreaterThan(DateTime.UtcNow));
        });
    }
}
