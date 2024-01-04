using System.Text;
using DomainWatcher.Infrastructure.HttpServer.Contracts;

namespace DomainWatcher.Infrastructure.HttpServer.IntegrationTests.TestInfrastructure;

[TestFixture]
public class HttpServerIntegrationTestFixture
{
    public HttpClient HttpClient { get; private set; }

    [OneTimeSetUp]
    public void OneTimeSetUpExposed()
    {
        HttpClient = new HttpClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDownExposed()
    {
        HttpClient.Dispose();
    }

    protected static HttpServerTestsWrapper StartServerWithEndpoints(params Type[] endpoints) => new(endpoints);

    protected async Task<HttpResponseMessage> GetResponseFrom<T>(HttpServerTestsWrapper server) where T : IHttpEndpoint
    {
        using var requestMessage = new HttpRequestMessage(T.Method, server.Url + T.Path);

        return await HttpClient.SendAsync(requestMessage);
    }

    protected async Task<HttpResponseMessage> GetResponseFrom<T>(HttpServerTestsWrapper server, string body) where T : IHttpEndpoint
    {
        using var requestMessage = new HttpRequestMessage(T.Method, server.Url + T.Path)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(body))
        };

        return await HttpClient.SendAsync(requestMessage);
    }

    protected async Task AssertResponseContent(HttpResponseMessage response, string expectedContent)
    {
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo(expectedContent));
    }
}
