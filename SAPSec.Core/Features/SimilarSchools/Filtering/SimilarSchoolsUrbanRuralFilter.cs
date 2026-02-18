using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsUrbanRuralFilter(SimilarSchool currentSchool) : ISimilarSchoolsMultiValueFilter
{
    public string Key => "ur";
    public string Name => "Urban or rural";
    public FilterType Type => FilterType.SingleValue;

    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i => values.Contains(i.UrbanRuralId));
    }

    public SimilarSchoolsAvailableFilter AsAvailableFilter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values) => new(
        Key,
        Name,
        Type,
        GetPossibleOptions(items, values).ToList().AsReadOnly(),
        currentSchool.UrbanRuralName);

    private IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, IEnumerable<string?> values) =>
        items.GroupBy(i => new { i.UrbanRuralId, i.UrbanRuralName })
            .Select(g => new FilterOption(
                g.Key.UrbanRuralId,
                g.Key.UrbanRuralName,
                g.Count(),
                values.Contains(g.Key.UrbanRuralId)));
}
