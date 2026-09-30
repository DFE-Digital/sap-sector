using SAPData.Models;
using SAPSec.Data.Common.Catalogue.Validation.Rules;

namespace SAPSec.Data.Common.Catalogue.Validation;

/// <summary>
/// Checks the data map for mistakes before any SQL is generated, by running each rule in <see cref="Rules"/>.
/// </summary>
public static class CatalogueValidator
{
    public const string PeriodInName = NameMatchesYear.RuleName;
    public const string TimePeriodFilter = TimePeriodMatchesYear.RuleName;
    public const string DuplicateMapping = NoDuplicateMappings.RuleName;
    public const string KeyColumn = OneKeyColumnPerFile.RuleName;
    public const string UnknownFile = FileExists.RuleName;
    public const string UnknownColumn = ColumnsExist.RuleName;
    public const string UnknownValue = FilterValuesExist.RuleName;

    /// <summary>The rules that run, in order. Each is based on a real error found in the old datamap.csv.</summary>
    public static IReadOnlyList<IValidationRule> Rules { get; } =
    [
        // The data map on its own
        new NameMatchesYear(),
        new TimePeriodMatchesYear(),
        new NoDuplicateMappings(),
        new OneKeyColumnPerFile(),

        // The data map against the source files
        new FileExists(),
        new ColumnsExist(),
        new FilterValuesExist(),
    ];

    public static IReadOnlyList<ValidationIssue> Validate(IEnumerable<DataMapRow> rows, SourceProfiles? profiles = null) =>
        Validate(rows, profiles, Rules);

    public static IReadOnlyList<ValidationIssue> Validate(IEnumerable<DataMapRow> rows, SourceProfiles? profiles, IEnumerable<IValidationRule> rules)
    {
        var context = new ValidationContext(rows.Where(r => !string.IsNullOrWhiteSpace(r.PropertyName)).ToList(), profiles);
        return rules.SelectMany(rule => rule.Check(context)).ToList();
    }
}
