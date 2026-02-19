using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsNurseryProvisionFilter(SimilarSchool currentSchool) : ISimilarSchoolsMultiValueFilter
{
    public string Name => "Nursery provision";
    public FilterType Type => FilterType.MultipleValue;

    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i => values.Contains(i.NurseryProvisionName));
    }

    public SimilarSchoolsAvailableFilter AsAvailableFilter(string key, IEnumerable<SimilarSchool> items, IEnumerable<string?> values) => new(
        key,
        Name,
        Type,
        GetPossibleOptions(items, values).ToList().AsReadOnly(),
        DataWithAvailability.FromStringWithoutCodes(currentSchool.NurseryProvisionName));

    private IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, IEnumerable<string?> values) =>
        items.GroupBy(i => new { i.NurseryProvisionName })
            .Select(g => new FilterOption(
                g.Key.NurseryProvisionName,
                g.Key.NurseryProvisionName,
                g.Count(),
                values.Contains(g.Key.NurseryProvisionName)));
}
