namespace SAPSec.Data.Common.Catalogue.Validation.Rules;

/// <summary>The value column, key column and every filter column are columns in the file.</summary>
public sealed class ColumnsExist : IValidationRule
{
    public const string RuleName = "unknown-column";

    public string Name => RuleName;

    public string Description => "The value column, key column and every filter column are columns in the file.";

    public IEnumerable<ValidationIssue> Check(ValidationContext context)
    {
        if (context.Profiles is null)
            yield break;

        foreach (var r in context.Rows)
        {
            // A missing file is reported by FileExists.
            if (!context.Profiles.Files.TryGetValue(RuleText.File(r), out var profile))
                continue;

            var columns = new[] { ("field", r.Field), ("key column", r.RecordFilterBy) }
                .Concat(DataMapFilters.Of(r).Select(f => ("filter column", f.Column)));

            foreach (var (role, column) in columns)
            {
                if (!profile.Columns.Contains(ColumnNames.Normalise(column)))
                    yield return this.Issue(r, $"{role} '{column}' is not a column in {RuleText.File(r)}");
            }
        }
    }
}
