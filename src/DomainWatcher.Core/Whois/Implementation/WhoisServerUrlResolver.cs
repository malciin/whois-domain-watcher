using System.Text.RegularExpressions;
using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Core.Whois.Exceptions;

namespace DomainWatcher.Core.Whois.Implementation;

public class WhoisServerUrlResolver(IWhoisRawResponseProvider whoisRawResponseProvider) : IWhoisServerUrlResolver
{
    private static readonly Regex WhoisRegex = new(@"whois:\s+([^ ]+)$", RegexOptions.Multiline);

    public async Task<string?> Resolve(string tld)
    {
        var whoisResponse = await whoisRawResponseProvider.GetResponse("whois.iana.org", tld);
        
        if (string.IsNullOrWhiteSpace(whoisResponse))
        {
            throw new UnexpectedWhoisResponseException("Empty whois response", whoisResponse);
        }

        if (whoisResponse.Contains("This query returned 0 objects."))
        {
            return null;
        }
        
        var matches = WhoisRegex.GroupMatches(whoisResponse).Select(x => x.Trim());

        if (!matches.Any())
        {
            throw new UnexpectedWhoisResponseException($"Cannot resolve whois server given response", whoisResponse);
        }

        return matches.Single();
    }
}
