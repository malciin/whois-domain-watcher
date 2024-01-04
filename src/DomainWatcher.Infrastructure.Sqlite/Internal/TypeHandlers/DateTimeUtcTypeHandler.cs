using System.Data;
using System.Globalization;
using Dapper;

namespace DomainWatcher.Infrastructure.Sqlite.Internal.TypeHandlers;

internal class DateTimeUtcTypeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override DateTime Parse(object value)
    {
        if (value is string valueString)
        {
            if (TryParse(valueString, out var parsed))
            {
                return parsed;
            }

            throw new FormatException($"Unrecognized date time format: {value}");
        }
        else
        {
            throw new NotImplementedException($"Parsing {value} of type {value.GetType().FullName} is not yet implemented.");
        }
    }

    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.Value = value;
    }

    private static bool TryParse(string value, out DateTime parsed)
    {
        parsed = DateTime.MinValue;

        foreach (var dateFormat in ValidDateTimeFormats)
        {
            var success = DateTime.TryParseExact(
                value,
                dateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out DateTime dateTime);

            if (success)
            {
                parsed = dateTime.ToUniversalTime();
                return true;
            }
        }

        return false;
    }

    private static readonly IEnumerable<string> ValidDateTimeFormats = new[]
    {
        "yyyy-MM-dd HH:mm:ss.fffffff",
        "yyyy-MM-dd HH:mm:ss.ffffff",
        "yyyy-MM-dd HH:mm:ss.fffff",
        "yyyy-MM-dd HH:mm:ss.ffff",
        "yyyy-MM-dd HH:mm:ss.fff",
        "yyyy-MM-dd HH:mm:ss.ff",
        "yyyy-MM-dd HH:mm:ss.f",
        "yyyy-MM-dd HH:mm:ss"
    };
}
