using System.Diagnostics.CodeAnalysis;
using DomainWatcher.Core.Whois.Values;

namespace DomainWatcher.Core.Whois.Parsers;

/// <summary>
/// Parser that handles common responses where:
/// 
/// 1. Domain availability status is easibly determined
/// 
/// 2. If domain is taken then creation/expiration timestamp are accessible by specific constant string.
///
/// 3. Date format is constant
/// 
/// Parser that will not fullfil one or more of these requirements should be implemented in separate class.
/// </summary>
internal class GeneralWhoisServerResponseParser : WhoisServerResponseParser
{
    private static readonly IEnumerable<Handler> Handlers = new[]
    {
        new Handler { Url = "whois.nic.io", IsFree = x => x.StartsWith("Domain not found.") },
        new Handler { Url = "whois.nic.tv", IsFree = x => x.StartsWith("No Data Found") },
        new Handler { Url = "tvwhois.verisign-grs.com", IsFree = x => x.StartsWith("No match for ") },
        new Handler { Url = "whois.verisign-grs.com", IsFree = x => x.StartsWith("No match for ") },
        new Handler { Url = "whois.registry.in", IsFree = x => x.StartsWith("No Data Found") },
        new Handler
        {
            Url = "whois.nic.xyz",
            DateFormat = @"yyyy-MM-dd\THH:mm:ss.f\Z",
            IsFree = x => x.StartsWith("The queried object does not exist")
        },
        new Handler { Url = "whois.nic.us", IsFree = x => x.StartsWith("No Data Found") },
        new Handler
        {
            Url = "whois.nic.tech",
            DateFormat = @"yyyy-MM-dd\THH:mm:ss.f\Z",
            IsFree = x => x.StartsWith("The queried object does not exist")
        },
        new Handler { Url = "whois.nic.me", IsFree = x => x.StartsWith("Domain not found.") },
        new Handler { Url = "whois.nic.info", IsFree = x => x.StartsWith("Domain not found.") },
        new Handler
        {
            Url = "whois.nic.fr",
            CreationMark = "created:",
            ExpirationMark = "Expiry Date:",
            IsFree = x => x.Contains("%% NOT FOUND")
        },
        new Handler { Url = "whois.nic.co", IsFree = x => x.StartsWith("No Data Found") },
        new Handler { Url = "whois.nic.biz", IsFree = x => x.StartsWith("No Data Found") },
        new Handler { Url = "whois.nic.google", IsFree = x => x.StartsWith("Domain not found.") },
        new Handler
        {
            Url = "whois.nic.uk",
            DateFormat = "dd-MMM-yyyy",
            CreationMark = "Registered on:",
            ExpirationMark = "Expiry date:",
            IsFree = x => x.Trim().StartsWith("No match for")
        },
        new Handler
        {
            Url = "whois.isnic.is",
            DateFormat = "MMM dd yyyy",
            CreationMark = "created:",
            ExpirationMark = "expires:",
            IsFree = x => x.Contains("% No entries found for query")
        },
        new Handler
        {
            Url = "whois.educause.edu",
            DateFormat = "dd-MMM-yyyy",
            CreationMark = "Domain record activated:",
            ExpirationMark = "Domain expires:",
            IsFree = x => x.StartsWith("NO MATCH:")
        }
    };

    internal override IEnumerable<string> GetSupportedWhoisServers()
    {
        return Handlers.Select(x => x.Url);
    }

    internal override WhoisServerResponseParsed Parse(string whoisServerUrl, string rawResponse)
    {
        var handler = Handlers.Single(x => x.Url == whoisServerUrl);

        return Handle(handler, rawResponse);
    }

    /// <summary>
    /// Intended to use when no parser was found for whoisServerUrl
    /// </summary>
    internal WhoisServerResponseParsed? FallbackParse(string rawResponse, out string? handledBy)
    {
        handledBy = null;

        foreach (var handler in Handlers)
        {
            try
            {
                var response = Handle(handler, rawResponse);

                handledBy = handler.Url;

                return response;
            }
            catch (Exception)
            {
            }
        }

        return null;
    }

    private static WhoisServerResponseParsed Handle(Handler handler, string rawResponse)
    {
        if (handler.IsFree(rawResponse))
        {
            return DomainAvailable;
        }

        return new WhoisServerResponseParsed
        {
            Registration = ParseDate(rawResponse, handler.CreationMark, handler.DateFormat),
            Expiration = ParseDate(rawResponse, handler.ExpirationMark, handler.DateFormat)
        };
    }

    private class Handler
    {
        public required string Url { get; init; }

        public string CreationMark { get; init; } = "Creation Date:";

        public string ExpirationMark { get; init; } = "Registry Expiry Date:";

        [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
        public string DateFormat { get; init; } = @"yyyy-MM-dd\THH:mm:ss\Z";

        public required Func<string, bool> IsFree { get; init; }
    }
}
