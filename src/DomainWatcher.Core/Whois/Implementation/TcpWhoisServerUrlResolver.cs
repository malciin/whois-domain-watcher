using System.Net.Sockets;
using System.Text;
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

        using var bufferedStreamWhois = new BufferedStream(tcpClient.GetStream());
        using var writter = new StreamWriter(bufferedStreamWhois, leaveOpen: true);
        using var receiver = new StreamReader(bufferedStreamWhois, leaveOpen: true);

        await writter.WriteLineAsync(tld);
        await writter.FlushAsync();

        var stringBuilderResult = new StringBuilder();
        while (!receiver!.EndOfStream) stringBuilderResult.Append(await receiver.ReadToEndAsync());

        var whoisResponse = stringBuilderResult.ToString();
        var matches = WhoisRegex.GroupMatches(whoisResponse).Select(x => x.Trim());

        if (!matches.Any())
        {
            throw new UnexistingWhoisServerForTldException(tld, whoisResponse);
        }

        return matches.Single();
    }
}
