using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Model;
using SAPSec.Core.Rules;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsNurseryProvisionFilter(string key,
    string name,
    IDictionary<string, IEnumerable<string>> filterValues,
    SimilarSchool currentSchool)
    : SimilarSchoolsMultiValueFilter(
        key,
        name,
        filterValues,
        currentSchool)
{
    private readonly NurseryProvisionRule _nurseryProvisionRule = new();
    protected override DataWithAvailability<string>? CurrentSchoolValue
        => DataWithAvailability.FromStringWithoutCodes(CurrentSchool.NurseryProvisionName);

    protected override IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i => values.Contains(i.NurseryProvisionName, StringComparer.OrdinalIgnoreCase));
    }

    protected override IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, IEnumerable<string?> values) =>
        items.GroupBy(FindGroup)
            .Select(g => new FilterOption(
                g.Key!.SortId,
                g.Key.Type,
                g.Count(),
                values.Contains(g.Key.SortId, StringComparer.OrdinalIgnoreCase)))
            .OrderBy(fo => fo.Key switch
            {
                "H" => 0,
                "N" => 1,
                _ => 3
            });

    private NurseryProvision FindGroup(SimilarSchool similarSchool)
    {
        var nurseryProvision = _nurseryProvisionRule.Evaluate(similarSchool).Value;

        return nurseryProvision;

    }
}
