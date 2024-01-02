namespace DomainWatcher.Core.Whois.Exceptions;

internal class NoParserForWhoisServerException : Exception
{
    public NoParserForWhoisServerException(string whoisServerUrl)
    {
        WhoisServerUrl = whoisServerUrl;
    }

    public string WhoisServerUrl { get; }
}
