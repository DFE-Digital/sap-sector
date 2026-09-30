using System.Text.RegularExpressions;

namespace SAPSec.Data.Common.Catalogue.Validation.Rules;

/// <summary>
/// A time_period filter is for the property's year, e.g. 202425 for 2024-2025.
/// Properties without an academic year (e.g. school email) are skipped.
/// </summary>
public sealed partial class TimePeriodMatchesYear : IValidationRule
{
    public const string RuleName = "time-period-filter";

    public string Name => RuleName;

    public string Description => "A time_period filter is for the property's year.";

    [GeneratedRegex(@"^(?<start>\d{4})-(?<end>\d{4})$")]
    private static partial Regex AcademicYearLabel();

    public IEnumerable<ValidationIssue> Check(ValidationContext context)
    {
        foreach (var r in context.Rows)
        {
            var year = AcademicYearLabel().Match(r.Year ?? "");
            if (!year.Success)
                continue;

            var code = new AcademicYear(int.Parse(year.Groups["start"].Value)).Code;
            foreach (var (column, values) in DataMapFilters.Of(r))
            {
                if (ColumnNames.Normalise(column) == "time_period" && values.Any(v => v.Trim() != code))
                    yield return this.Issue(r, $"time_period filter '{string.Join('+', values)}' doesn't match the row's year {r.Year} ({code})");
            }
        }
    }
}
