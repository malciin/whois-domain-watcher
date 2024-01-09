namespace DomainWatcher.Core.Enums;

public enum WhoisResponseStatus
{
    OK = 0,
    /// <summary>
    /// Whois server parser not implemented but parsed by fallback parser
    /// which can sometimes produce wrong result.
    /// </summary>
    OKParsedByFallback = 3,
    /// <summary>
    /// Valid response received but parser is missing
    /// </summary>
    ParserMissing = 1,
    /// <summary>
    /// Some whois servers like whois.eu hides expiration dates
    /// </summary>
    TakenButTimestampsHidden = 2
}
