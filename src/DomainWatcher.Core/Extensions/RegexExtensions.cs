using System.Text.RegularExpressions;

namespace DomainWatcher.Core.Extensions;

public static class RegexExtensions
{
    public static IEnumerable<string> GroupMatches(this Regex regex, string input, int groupIndex = 1)
    {
        var index = 0;

        while (true)
        {
            var match = regex.Match(input, index);

            if (!match.Success) break;

            yield return match.Groups[groupIndex].Value;
            index = match.Index + match.Length;
        }
    }
}
