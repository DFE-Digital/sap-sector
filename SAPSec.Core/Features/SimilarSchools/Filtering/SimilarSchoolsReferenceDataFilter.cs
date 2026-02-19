using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public abstract class SimilarSchoolsReferenceDataFilter(
    SimilarSchool currentSchool,
    Func<SimilarSchool, ReferenceData> field) : ISimilarSchoolsMultiValueFilter
{
    public abstract string Name { get; }
    public FilterType Type => FilterType.MultipleValue;

    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i => values.Contains(field(i).Id));
    }

    public SimilarSchoolsAvailableFilter AsAvailableFilter(string key, IEnumerable<SimilarSchool> items, IEnumerable<string?> values) => new(
        key,
        Name,
        Type,
        GetPossibleOptions(items, values).ToList().AsReadOnly(),
        DataWithAvailability.FromStringWithCodes(field(currentSchool).Id, field(currentSchool).Name));

    private IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, IEnumerable<string?> values) =>
        items.GroupBy(field)
            .Select(g => new FilterOption(
                g.Key.Id,
                g.Key.Name,
                g.Count(),
                values.Contains(g.Key.Id)));
}
