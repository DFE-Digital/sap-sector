namespace SAPSec.Data.Common.Catalogue.Validation.Rules;

/// <summary>
/// Every property reading a file in one view uses the same key column: the SQL generator keys each file by its first
/// row only.
/// </summary>
public sealed class OneKeyColumnPerFile : IValidationRule
{
    public const string RuleName = "key-column";

    public string Name => RuleName;

    public string Description => "Every property reading a file in one view uses the same key column.";

    public IEnumerable<ValidationIssue> Check(ValidationContext context) =>
        context.Rows
            .GroupBy(r => (View: RuleText.View(r), File: RuleText.File(r)))
            .Where(g => g.Select(r => ColumnNames.Normalise(r.RecordFilterBy)).Distinct().Count() > 1)
            .Select(g => new ValidationIssue(
                Name, g.Key.View, g.Key.File,
                $"rows use different key columns ({string.Join(", ", g.Select(r => $"'{r.RecordFilterBy}'").Distinct())}); only the first is used"));
}
