using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public abstract class SimilarSchoolsIdAndNameFieldFilter(
    SimilarSchool currentSchool,
    Func<SimilarSchool, string> idField,
    Func<SimilarSchool, string> nameField) : ISimilarSchoolsMultiValueFilter
{
    public abstract string Key { get; }
    public abstract string Name { get; }
    public FilterType Type => FilterType.MultipleValue;

    public IEnumerable<SimilarSchool> Filter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values)
    {
        if (!values.Any())
        {
            return items;
        }

        return items.Where(i => values.Contains(idField(i)));
    }

    public SimilarSchoolsAvailableFilter AsAvailableFilter(IEnumerable<SimilarSchool> items, IEnumerable<string?> values) => new(
        Key,
        Name,
        Type,
        GetPossibleOptions(items, values).ToList().AsReadOnly(),
        DataWithAvailability.FromStringWithCodes(idField(currentSchool), nameField(currentSchool)));

    private IEnumerable<FilterOption> GetPossibleOptions(IEnumerable<SimilarSchool> items, IEnumerable<string?> values) =>
        items.GroupBy(i => new { Id = idField(i), Name = nameField(i) })
            .Select(g => new FilterOption(
                g.Key.Id,
                g.Key.Name,
                g.Count(),
                values.Contains(g.Key.Id)));
}
