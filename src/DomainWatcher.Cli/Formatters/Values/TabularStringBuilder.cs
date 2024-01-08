using System.Text;
using DomainWatcher.Core.Extensions;

namespace DomainWatcher.Cli.Formatters.Values;

public class TabularStringBuilder
{
    public int CurrentRow { get; set; }

    private readonly Dictionary<int, Dictionary<int, string>> rowColMap;
    private readonly Dictionary<int, TabularColumnSpec> columnsSpec;
    private readonly bool hasHeader;

    public int rows;
    public int columns;

    public TabularStringBuilder()
    {
        rows = 0;
        columns = 0;
        CurrentRow = 0;
        hasHeader = false;
        rowColMap = [];
        columnsSpec = [];
    }

    public TabularStringBuilder(params TabularColumnSpec[] specs) : this()
    {
        for (var col = 0; col < specs.Length; col++)
        {
            columnsSpec[col] = specs[col];
            if (!hasHeader && specs[col].Header != null) hasHeader = true;
        }
    }

    public string this[int column]
    {
        set => SetString(CurrentRow, column, value);
    }

    public string this[int row, int column]
    {
        set => SetString(row, column, value);
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        if (hasHeader)
        {
            WriteHeader(stringBuilder);
        }

        for (var row = 0; row <= rows; row++)
        {
            if (hasHeader) stringBuilder.Append('|');
            for (var col = 0; col <= columns; col++)
            {
                stringBuilder.Append(GetTransformedString(row, col));
                if (hasHeader) stringBuilder.Append('|');
            }

            if (row < rows)
            {
                stringBuilder.AppendLine();
            }
        }

        return stringBuilder.ToString();
    }

    private void SetString(int row, int column, string @string)
    {
        if (!rowColMap.TryGetValue(row, out var columnMap))
        {
            columnMap = [];
            rowColMap[row] = columnMap;
        }

        columnMap[column] = @string;
        rows = Math.Max(row, rows);
        columns = Math.Max(column, columns);

        if (columnsSpec.TryGetValue(column, out var columnSpec))
        {
            columnSpec.UpdateWidthIfBigger(@string.Length);
        }
        else
        {
            columnsSpec[column] = new TabularColumnSpec { Width = @string.Length };
        }

        CurrentRow = row;
    }

    private string GetString(int row, int column)
    {
        if (!rowColMap.TryGetValue(row, out var columnMap))
        {
            return string.Empty;
        }

        if (!columnMap.TryGetValue(column, out var value))
        {
            return string.Empty;
        }

        return value;
    }

    private string GetTransformedString(int row, int column)
    {
        var @string = GetString(row, column);

        return TransformColumnString(@string, column);
    }

    private string TransformColumnString(string @string, int column)
    {
        var spec = columnsSpec[column];
        var resultString = spec.Align switch
        {
            0 => @string.PadRight(spec.Width),
            _ => @string.PadLeft(spec.Width)
        };

        return " ".Repeated(spec.PaddingLeft) + resultString + " ".Repeated(spec.PaddingRight);
    }

    private void WriteHeader(StringBuilder stringBuilder)
    {
        if (!columnsSpec.Values.All(x => x.Header != null))
        {
            throw new ArgumentException($"Not all columns have specified {nameof(TabularColumnSpec.Header)}!");
        }

        stringBuilder.Append('|');

        for (var col = 0; col <= columns; col++)
        {
            stringBuilder.Append(TransformColumnString(columnsSpec[col].Header, col));
            stringBuilder.Append('|');
        }

        var headerLength = stringBuilder.Length;

        stringBuilder.AppendLine();
        stringBuilder.Append('-', headerLength);
        stringBuilder.AppendLine();
    }
}
