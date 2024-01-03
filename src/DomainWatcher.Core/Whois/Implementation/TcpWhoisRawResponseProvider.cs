using System.Net.Sockets;
using DomainWatcher.Core.Whois.Contracts;

namespace DomainWatcher.Core.Whois.Implementation;

public class TcpWhoisRawResponseProvider : IWhoisRawResponseProvider
{
    public async Task<string?> GetResponse(string whoisServerUrl, string queryLine)
    {
        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(whoisServerUrl, 43);

        using var networkStream = tcpClient.GetStream();
        using var writter = new StreamWriter(networkStream, leaveOpen: true);
        using var receiver = new StreamReader(networkStream, leaveOpen: true);

        await writter.WriteLineAsync(queryLine);
        await writter.FlushAsync();

        return await receiver.ReadToEndAsync();
    }
}
