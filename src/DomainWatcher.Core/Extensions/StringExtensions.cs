namespace DomainWatcher.Core.Extensions;

public static class StringExtensions
{
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
