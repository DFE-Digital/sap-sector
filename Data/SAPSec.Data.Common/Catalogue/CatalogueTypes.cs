namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// The level a measure is reported at. Maps to the DataMap "Range" column.
/// </summary>
public enum Scope
{
    Establishment,
    LA,
    England
}

/// <summary>
/// Relative reporting period. Maps to the DataMap "YearDesc" column.
/// </summary>
public enum Period
{
    Current,
    Previous,
    Previous2
}

public enum Unit
{
    Num,
    Pct
}

public enum DataType
{
    Double,
    String,
    StringArray
}

public static class CatalogueTypeExtensions
{
    /// <summary>Short scope code used in property names, e.g. Attainment8_Tot_<b>Est</b>_Current_Num.</summary>
    public static string NameCode(this Scope scope) => scope switch
    {
        Scope.Establishment => "Est",
        Scope.LA => "LA",
        Scope.England => "Eng",
        _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
    };

    /// <summary>Value written to the DataMap "DataType" column.</summary>
    public static string DataMapValue(this DataType dataType) => dataType switch
    {
        DataType.Double => "double",
        DataType.String => "string",
        DataType.StringArray => "string array",
        _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
    };
}

/// <summary>
/// An academic year, e.g. 2024 → Label "2024-2025", Code "202425" (the EES time_period format).
/// </summary>
public sealed record AcademicYear(int StartYear)
{
    public int EndYear => StartYear + 1;

    public string Label => $"{StartYear}-{EndYear}";

    public string Code => $"{StartYear}{EndYear % 100:D2}";

    public override string ToString() => Label;
}

/// <summary>
/// A column filter applied to a source file. Multiple values are ORed together.
/// </summary>
public sealed record Filter
{
    public Filter(string column, params string[] values)
    {
        if (string.IsNullOrWhiteSpace(column))
            throw new CatalogueException("Filter column must not be empty.");

        if (values.Length == 0 || values.Any(string.IsNullOrWhiteSpace))
            throw new CatalogueException($"Filter '{column}' must have at least one non-empty value.");

        // The generator splits filter values on '+' to build OR conditions, so a literal '+' cannot be represented.
        if (values.Any(v => v.Contains('+')))
            throw new CatalogueException($"Filter '{column}' value must not contain '+'. Pass multiple values instead.");

        Column = column;
        Values = values;
    }

    public string Column { get; }

    public IReadOnlyList<string> Values { get; }

    /// <summary>DataMap encoding: values joined with '+'.</summary>
    public string EncodedValue => string.Join('+', Values);
}

/// <summary>
/// A pupil group a measure is broken down by. <see cref="Code"/> is used in property names.
/// </summary>
public sealed record Breakdown(string Code, string Description);

public static class Breakdowns
{
    public static readonly Breakdown Total = new("Tot", "Total");
    public static readonly Breakdown Sum = new("Sum", "Sum");
    public static readonly Breakdown Boys = new("Boy", "Boys");
    public static readonly Breakdown Girls = new("Grl", "Girls");
    public static readonly Breakdown Disadvantaged = new("Dis", "Disadvantaged");
    public static readonly Breakdown NotDisadvantaged = new("NDi", "Not disadvantaged");
    public static readonly Breakdown Eal = new("EAL", "English as an additional language");
    public static readonly Breakdown FirstLanguageEnglish = new("EFL", "First language English");
    public static readonly Breakdown Mobile = new("Mob", "Mobile");
    public static readonly Breakdown NonMobile = new("NMo", "Non-mobile");

    public static readonly IReadOnlyList<Breakdown> Standard =
    [
        Total, Boys, Girls, Disadvantaged, NotDisadvantaged, Eal, NonMobile
    ];
}

public sealed class CatalogueException(string message) : Exception(message);
