using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Data.Repositories;

namespace SAPSec.Test.Common.InMemory;

public class InMemorySimilarSchoolsSecondaryRepository : ISimilarSchoolsSecondaryRepository
{
    private List<SimilarSchoolsSecondaryGroupsEntry> _groups = new();
    private List<SimilarSchoolsSecondaryValuesEntry> _values = new();

    public void SetupGroups(params SimilarSchoolsSecondaryGroupsEntry[] groups)
    {
        _groups = groups.ToList();
    }

    public void SetupValues(params SimilarSchoolsSecondaryValuesEntry[] values)
    {
        _values = values.ToList();
    }

    public void ClearDown()
    {
        _groups = [];
        _values = [];
    }

    public Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetGroupAsync(string urn)
        => Task.FromResult((IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>)_groups.Where(x => x.URN == urn).ToList());

    public Task<IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>> GetValuesByUrnsAsync(IEnumerable<string> urns)
        => Task.FromResult((IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>)_values.Where(x => urns.Contains(x.URN)).ToList());

    public Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet()
        => Task.FromResult((IReadOnlyCollection<string>)_values.Select(v => v.URN).ToList());
}
