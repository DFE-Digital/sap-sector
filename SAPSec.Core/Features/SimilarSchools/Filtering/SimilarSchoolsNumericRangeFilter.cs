using SAPSec.Core.Features.SimilarSchools.UseCases;
using System.Text.RegularExpressions;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public abstract class SimilarSchoolsNumericRangeFilter(
    string key,
    string name,
    IDictionary<string, IEnumerable<string>> filterValues,
    SimilarSchool currentSchool)
    : SimilarSchoolsFilter(key, name, filterValues, currentSchool)
{
    const int MinValue = 0;
    const int MaxValue = 999;
    static readonly Regex NumericValueRegex = new Regex(@"^-?\d+(\.\d+)?$", RegexOptions.Compiled);

    public override bool IsApplied => HasFilterValues(Key + "_f") || HasFilterValues(Key + "_t");

    public IEnumerable<ValidationError> Validate()
    {
        (var fromError, var from) = ParseAndValidateFieldValue("_f", MinValue);
        (var toError, var to) = ParseAndValidateFieldValue("_t", MaxValue);

        if (fromError is not null)
        {
            yield return new(Key + "_f", fromError);
        }

        if (toError is not null)
        {
            yield return new(Key + "_t", toError);
        }

        if (fromError is null && toError is null && to <= from)
        {
            yield return new(Key, "The From value must be lower than the To value");
        }
    }

    public override IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items)
    {
        (var fromError, var from) = ParseAndValidateFieldValue("_f", MinValue);
        (var toError, var to) = ParseAndValidateFieldValue("_t", MaxValue);

        if (fromError is not null || toError is not null)
        {
            return items;
        }

        return Filter(items, from, to);
    }

    public override SimilarSchoolsAvailableFilter? AsAvailableFilter(IEnumerable<SimilarSchool> items)
    {
        (var fromError, var from) = ParseAndValidateFieldValue("_f", MinValue);
        (var toError, var to) = ParseAndValidateFieldValue("_t", MaxValue);

        var fromFieldValue = !string.IsNullOrWhiteSpace(GetFieldValue("_f")) && fromError is null
            ? Math.Round(from, 2).ToString()
            : GetFieldValue("_f") ?? "";
        var toFieldValue = !string.IsNullOrWhiteSpace(GetFieldValue("_t")) && toError is null
            ? Math.Round(to, 2).ToString()
            : GetFieldValue("_t") ?? "";

        return new SimilarSchoolsNumericRangeAvailableFilter(
            Key,
            Name,
            new(Key + "_f", fromFieldValue),
            new(Key + "_t", toFieldValue),
            CurrentSchoolValue,
            Validate().ToList());
    }

    private string? GetFieldValue(string fieldSuffix) =>
        (FilterValues.ContainsKey(Key + fieldSuffix) ? FilterValues[Key + fieldSuffix] : []).LastOrDefault();

    private (string?, decimal) ParseAndValidateFieldValue(string fieldSuffix, decimal defaultValue)
    {
        var value = defaultValue;
        string? error = null;

        var fieldValue = GetFieldValue(fieldSuffix);
        if (!string.IsNullOrWhiteSpace(fieldValue) && !NumericValueRegex.IsMatch(fieldValue))
        {
            error = "Enter a numeric value";
        }
        else if (!string.IsNullOrWhiteSpace(fieldValue) && (!decimal.TryParse(fieldValue, out value) || value < MinValue || value > MaxValue))
        {
            error = $"Enter a value between {MinValue} and {MaxValue}";
        }

        return (error, value);
    }

    protected abstract IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, decimal from, decimal to);
}