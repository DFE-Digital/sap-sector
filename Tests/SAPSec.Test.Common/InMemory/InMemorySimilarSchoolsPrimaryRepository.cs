using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Repositories;

namespace SAPSec.Test.Common.InMemory;

public class InMemorySimilarSchoolsPrimaryRepository : ISimilarSchoolsPrimaryRepository
{
    private List<SimilarSchoolsPrimaryGroupsEntry> _groups = new();
    private List<SimilarSchoolsPrimaryValuesEntry> _values = new();

    public void SetupGroups(params SimilarSchoolsPrimaryGroupsEntry[] groups)
    {
        _groups = groups.ToList();
    }

    public void SetupValues(params SimilarSchoolsPrimaryValuesEntry[] values)
    {
        _values = values.ToList();
    }

    public void ClearDown()
    {
        _groups = [];
        _values = [];
    }

    public Task<IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>> GetGroupAsync(string urn)
        => Task.FromResult((IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>)_groups.Where(x => x.URN == urn).ToList());

    public Task<IReadOnlyCollection<SimilarSchoolsPrimaryValuesEntry>> GetValuesByUrnsAsync(IEnumerable<string> urns)
        => Task.FromResult((IReadOnlyCollection<SimilarSchoolsPrimaryValuesEntry>)_values.Where(x => urns.Contains(x.URN)).ToList());

    public Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet()
        => Task.FromResult((IReadOnlyCollection<string>)_values.Select(v => v.URN).ToList());
}
