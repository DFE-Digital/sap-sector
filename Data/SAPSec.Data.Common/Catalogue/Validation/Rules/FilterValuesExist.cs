namespace SAPSec.Data.Common.Catalogue.Validation.Rules;

/// <summary>
/// Every filter value occurs in the file. Catches DfE renaming a pupil group or a file not having a year yet.
/// (datamap.csv filtered grade = 7/8/9 in a file that only has grade bands, so every value was blank.)
/// </summary>
public sealed class FilterValuesExist : IValidationRule
{
    public const string RuleName = "unknown-value";

    public string Name => RuleName;

    public string Description => "Every filter value occurs in the file.";

    public IEnumerable<ValidationIssue> Check(ValidationContext context)
    {
        if (context.Profiles is null)
            yield break;

        foreach (var r in context.Rows)
        {
            if (!context.Profiles.Files.TryGetValue(RuleText.File(r), out var profile))
                continue;

            foreach (var (column, values) in DataMapFilters.Of(r))
            {
                // A filter column missing from the file has no values; ColumnsExist reports it.
                if (!profile.Values.TryGetValue(ColumnNames.Normalise(column), out var known))
                    continue;

                var missing = values.Select(v => v.Trim()).Where(v => !known.Contains(v)).ToList();
                if (missing.Count > 0)
                    yield return this.Issue(r, $"filter {column} = '{string.Join("', '", missing)}' matches no rows in {RuleText.File(r)}");
            }
        }
    }
}
