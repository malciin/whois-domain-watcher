using System.Net.Sockets;
using System.Text.RegularExpressions;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Core.Whois.Exceptions;

namespace DomainWatcher.Core.Whois.Implementation;

public class TcpWhoisServerUrlResolver : IWhoisServerUrlResolver
{
    private static readonly Regex WhoisRegex = new(@"whois:\s+([^ ]+)$", RegexOptions.Multiline);

    public async Task<string> Resolve(string tld)
    {
        using var tcpClient = new TcpClient();

        await tcpClient.ConnectAsync("whois.iana.org", 43);

        using var networkStream = tcpClient.GetStream();
        using var writter = new StreamWriter(networkStream, leaveOpen: true);
        using var receiver = new StreamReader(networkStream, leaveOpen: true);

        await writter.WriteLineAsync(tld);
        await writter.FlushAsync();

        var whoisResponse = await receiver.ReadToEndAsync();
        var matches = WhoisRegex.GroupMatches(whoisResponse).Select(x => x.Trim());

        if (!matches.Any())
        {
            throw new UnexistingWhoisServerForTldException(tld, whoisResponse);
        }

        return matches.Single();
    }
}
