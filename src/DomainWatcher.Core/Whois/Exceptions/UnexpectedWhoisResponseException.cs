namespace DomainWatcher.Core.Whois.Exceptions;

public class UnexpectedWhoisResponseException : Exception
{
    public UnexpectedWhoisResponseException(string? whoisResponse) : base()
    {
        WhoisResponse = whoisResponse;
    }

    public UnexpectedWhoisResponseException(string message, string? whoisResponse) : base(message)
    {
        WhoisResponse = whoisResponse;
    }

    public UnexpectedWhoisResponseException(string message, string? whoisResponse, Exception innerException) : base(message, innerException)
    {
        WhoisResponse = whoisResponse;
    }

    public string? WhoisResponse { get; }
}
