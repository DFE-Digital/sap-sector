using SAPSec.Core.Features.Filtering;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsUrbanRuralFilter() : ISimilarSchoolsFilter, ISimilarSchoolsMultiValueFilter
{
    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i => values.Contains(i.UrbanRuralId));
    }

    public IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, IEnumerable<string?> values)
    {
        return items.GroupBy(i => new { i.UrbanRuralId, i.UrbanRuralName })
            .Select(g => new FilterOption(g.Key.UrbanRuralId, g.Key.UrbanRuralName, g.Count(), values.Contains(g.Key.UrbanRuralId)));
    }
}
