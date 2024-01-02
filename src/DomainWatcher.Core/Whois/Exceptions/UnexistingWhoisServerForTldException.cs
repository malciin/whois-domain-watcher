namespace DomainWatcher.Core.Whois.Exceptions;

public class UnexistingWhoisServerForTldException : Exception
{
    public UnexistingWhoisServerForTldException(string tld, string whoisResponse) : base()
    {
        Tld = tld;
        WhoisResponse = whoisResponse;
    }

    public string Tld { get; }

    public string WhoisResponse { get; }
}
