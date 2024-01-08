using System.Text;

namespace DomainWatcher.Core.Extensions;

public static class StringExtensions
{
    public static string Repeated(this string @string, int repeatCount)
    {
        if (repeatCount <= 0) return string.Empty;

        var stringBuilder = new StringBuilder(@string.Length * repeatCount);

        for (var i = 0; i < repeatCount; i++)
        {
            stringBuilder.Append(@string);
        }

        return stringBuilder.ToString();
    }

    public static int FindIndex(this string @string, Func<char, bool> predicate)
    {
        for (var i = 0; i < @string.Length; i++)
        {
            if (predicate(@string[i])) return i;
        }

        return -1;
    }

    public static string SubstringToFirst(this string @string, char @char)
    {
        var charIndex = @string.IndexOf(@char);

        return charIndex < 0
            ? @string
            : @string[..charIndex];
    }

    public static string? GetLineThatContains(this string @string, string substring)
    {
        var index = @string.IndexOf(substring);

        if (index == -1) return null;

        var endLineIndex = @string.IndexOfAny(['\r', '\n'], index);

        if (endLineIndex == -1) return @string[index..];

        return @string[index..endLineIndex];
    }
}
