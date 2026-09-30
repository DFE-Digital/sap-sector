using System.Text.RegularExpressions;

namespace SAPSec.Data.Common.Catalogue.Validation.Rules;

/// <summary>
/// A property named "…_Previous2_…" reads the Previous2 year.
/// (datamap.csv mapped Prog8_Avg_LA_Previous2_Num to the current year.)
/// </summary>
public sealed partial class NameMatchesYear : IValidationRule
{
    public const string RuleName = "period-in-name";

    public string Name => RuleName;

    public string Description => "A property named \"…_Previous2_…\" reads the Previous2 year.";

    [GeneratedRegex(@"_(?<scope>Est|LA|Eng)_(?<period>Current|Previous2|Previous)_(?:Num|Pct)$")]
    private static partial Regex NameSuffix();

    public IEnumerable<ValidationIssue> Check(ValidationContext context)
    {
        foreach (var r in context.Rows)
        {
            var suffix = NameSuffix().Match(r.PropertyName);
            if (suffix.Success && suffix.Groups["period"].Value != r.YearDesc)
                yield return this.Issue(r, $"name says {suffix.Groups["period"].Value} but the row is YearDesc '{r.YearDesc}'");
        }
    }
}
