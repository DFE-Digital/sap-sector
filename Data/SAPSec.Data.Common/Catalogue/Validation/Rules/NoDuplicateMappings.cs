using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue.Validation.Rules;

/// <summary>
/// No two properties read the same file, column and filters: they would show the same number under different names.
/// (datamap.csv read the grade 4+ column for EngMaths59_Mob_Eng_*, duplicating EngMaths49_Mob_Eng_*.)
/// </summary>
public sealed class NoDuplicateMappings : IValidationRule
{
    public const string RuleName = "duplicate-mapping";

    public string Name => RuleName;

    public string Description => "No two properties read the same file, column and filters.";

    public IEnumerable<ValidationIssue> Check(ValidationContext context) =>
        context.Rows
            .GroupBy(r => (RuleText.View(r), Mapping: $"{RuleText.File(r)}:{ColumnNames.Normalise(r.Field)}:{FilterSignature(r)}"))
            .Where(g => g.Select(r => r.PropertyName).Distinct().Count() > 1)
            .SelectMany(g => g.Select(r => this.Issue(r,
                $"reads the same data as {string.Join(", ", g.Select(x => x.PropertyName).Where(p => p != r.PropertyName).Distinct())}")));

    private static string FilterSignature(DataMapRow r) =>
        string.Join(";", DataMapFilters.Of(r)
            .Select(f => $"{ColumnNames.Normalise(f.Column)}=({string.Join('|', f.Values.Select(v => v.Trim()).Order(StringComparer.Ordinal))})")
            .Order(StringComparer.Ordinal));
}
