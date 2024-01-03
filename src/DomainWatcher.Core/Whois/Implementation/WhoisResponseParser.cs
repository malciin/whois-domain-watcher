using DomainWatcher.Core.Extensions;
using DomainWatcher.Core.Whois.Contracts;
using DomainWatcher.Core.Whois.Parsers;
using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Implementation;

public class WhoisResponseParser : IWhoisResponseParser
{
    private static readonly IReadOnlyDictionary<string, WhoisServerResponseParser> ParsersByWhoisServerUrl = typeof(WhoisServerResponseParser)
        .Assembly
        .GetInstantiableTypesAssignableTo<WhoisServerResponseParser>()
        .Select(Activator.CreateInstance)
        .Cast<WhoisServerResponseParser>()
        .SelectMany(x => x.GetSupportedWhoisServers().Select(tld => KeyValuePair.Create(tld, x)))
        .ToDictionary(x => x.Key, v => v.Value);

    public IEnumerable<string> SupportedWhoisServers => ParsersByWhoisServerUrl.Keys;

    public bool DoesSupport(string whoisServerUrl) => ParsersByWhoisServerUrl.ContainsKey(whoisServerUrl);

    public WhoisServerResponseParsed Parse(string whoisServerUrl, string whoisResponse)
    {
        return ParsersByWhoisServerUrl[whoisServerUrl].Parse(whoisResponse);
    }
}
