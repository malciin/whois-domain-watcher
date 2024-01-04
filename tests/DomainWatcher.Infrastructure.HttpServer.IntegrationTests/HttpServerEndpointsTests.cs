using System.Net;
using DomainWatcher.Infrastructure.HttpServer.IntegrationTests.TestInfrastructure;
using DomainWatcher.Infrastructure.HttpServer.IntegrationTests.TestInfrastructure.Endpoints;

namespace DomainWatcher.Infrastructure.HttpServer.IntegrationTests;

[Parallelizable(ParallelScope.All)]
public class HttpServerEndpointsTests : HttpServerIntegrationTestFixture
{
    [Test]
    public async Task SendingRequestToUknownEndpoint_Returns404()
    {
        await using var serverTask = StartServerWithEndpoints(typeof(EchoEndpoint));

        var result = await HttpClient.GetAsync($"{serverTask.Url}/unknown");

        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [TestCase("")]
    [TestCase("a")]
    [TestCase("abcde")]
    public async Task SendingRequestToEchoEndpoint_ReturnsSameBody(string body)
    {
        await using var serverTask = StartServerWithEndpoints(typeof(EchoEndpoint));

        var echoResponse = await GetResponseFrom<EchoEndpoint>(serverTask, body);

        Assert.That(echoResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await AssertResponseContent(echoResponse, body);
    }

    [Test]
    public async Task WhenSendingRequestsToDifferentEndpoints_CorrectResponseIsPresent()
    {
        await using var serverTask = StartServerWithEndpoints(typeof(EchoEndpoint), typeof(OkEndpoint));

        var echoResponse = await GetResponseFrom<EchoEndpoint>(serverTask, "hello world");
        var okResponse = await GetResponseFrom<OkEndpoint>(serverTask);

        Assert.Multiple(async () =>
        {
            Assert.That(echoResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(okResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            await AssertResponseContent(echoResponse, "hello world");
            await AssertResponseContent(okResponse, OkEndpoint.StringResponse);
        });
    }
}
