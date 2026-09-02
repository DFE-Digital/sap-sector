using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Repositories;

namespace SAPSec.Test.Common.InMemory;

public class InMemorySimilarSchoolsPrimaryRepository : ISimilarSchoolsPrimaryRepository
{
    private List<SimilarSchoolsPrimaryGroupsEntry> _groups = new();
    private List<SimilarSchoolsPrimaryValuesEntry> _values = new();

    public InMemorySimilarSchoolsPrimaryRepository SetupGroups(params SimilarSchoolsPrimaryGroupsEntry[] groups)
    {
        _groups = groups.ToList();

        return this;
    }

    public InMemorySimilarSchoolsPrimaryRepository SetupValues(params SimilarSchoolsPrimaryValuesEntry[] values)
    {
        _values = values.ToList();

        return this;
    }

    public InMemorySimilarSchoolsPrimaryRepository ClearDown()
    {
        _groups = [];
        _values = [];

        return this;
    }

    public Task<IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>> GetGroupAsync(string urn)
        => Task.FromResult((IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>)_groups.Where(x => x.URN == urn).ToList());

    public Task<IReadOnlyCollection<SimilarSchoolsPrimaryValuesEntry>> GetValuesByUrnsAsync(IEnumerable<string> urns)
        => Task.FromResult((IReadOnlyCollection<SimilarSchoolsPrimaryValuesEntry>)_values.Where(x => urns.Contains(x.URN)).ToList());

    public Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet()
        => Task.FromResult((IReadOnlyCollection<string>)_values.Select(v => v.URN).ToList());
}
