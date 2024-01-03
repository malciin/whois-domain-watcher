using System.Net.Sockets;
using DomainWatcher.Core.Values;
using DomainWatcher.Core.Whois.Contracts;

namespace DomainWatcher.Core.Whois.Implementation;

public class WhoisRawClient : IWhoisRawClient
{
    public async Task<string> QueryAsync(string whoisServerUrl, Domain domain)
    {
        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(whoisServerUrl, 43);

        using var networkStream = tcpClient.GetStream();
        using var writter = new StreamWriter(networkStream, leaveOpen: true);
        using var receiver = new StreamReader(networkStream, leaveOpen: true);

        await writter.WriteLineAsync(domain.ToString());
        await writter.FlushAsync();

        var whoisResponse = await receiver.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(whoisResponse))
        {
            throw new NullReferenceException($"{whoisServerUrl} returned empty response for {domain}");
        }

        return whoisResponse;
    }
}
