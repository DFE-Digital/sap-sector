using System;
using System.Data;
using System.Globalization;
using Dapper;

namespace SAPSec.Infrastructure.TypeHandlers;

public sealed class NullableDoubleHandler : SqlMapper.TypeHandler<double?>
{
    public override void SetValue(IDbDataParameter parameter, double? value)
    {
        parameter.Value = (object?)value ?? DBNull.Value;
    }

    public override double? Parse(object value)
    {
        if (value is null || value is DBNull) return null;

        if (value is double d) return d;
        if (value is float f) return f;
        if (value is decimal m) return (double)m;
        if (value is int i) return i;
        if (value is long l) return l;

        if (value is string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var trimmed = s.Trim();
            if (trimmed.Equals("c", StringComparison.OrdinalIgnoreCase)) return null;
            if (trimmed.Equals("s", StringComparison.OrdinalIgnoreCase)) return null;
            if (trimmed.Equals("x", StringComparison.OrdinalIgnoreCase)) return null;

            if (double.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            if (double.TryParse(trimmed, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
                return parsed;
        }

        return null;
    }
}
