using System.Text.RegularExpressions;

namespace DomainWatcher.Core.Values;

public class Domain
{
    public string Name { get; init; }

    public string Tld { get; init; }

    public string FullName => $"{Name}.{Tld}";

    public Domain(string url)
    {
        var dotSplitted = url.Split(".");

        Name = dotSplitted[0];
        Tld = string.Join(".", dotSplitted[1..]);
    }

    public override string ToString() => FullName;

    public override bool Equals(object? obj) => obj is Domain domain && FullName == domain.FullName;

    public override int GetHashCode() => FullName.GetHashCode();

    public static bool TryParse(string url, out Domain? domain)
    {
        if (Regex.IsMatch(url, @"^([\w\-]+\.)+\w+$"))
        {
            domain = new Domain(url);
            return true;
        }

        domain = null;
        return false;
    }
}
