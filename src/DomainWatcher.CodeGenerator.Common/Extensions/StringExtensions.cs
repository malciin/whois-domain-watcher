using System.IO;
using System.Text;

namespace DomainWatcher.CodeGenerator.Common.Extensions;

public static class StringExtensions
{
    public static string Repeat(this string source, int count)
    {
        return new StringBuilder().Append(source, 0, count).ToString();
    }

    public static string PadEachLine(this string source, int pad)
    {
        var stringBuilder = new StringBuilder();
        using var x = new StringReader(source);

        while (true)
        {
            var line = x.ReadLine();

            if (line == null) break;

            stringBuilder.Append(' ', pad);
            stringBuilder.AppendLine(line);
        }

        return stringBuilder.ToString();
    }
}
