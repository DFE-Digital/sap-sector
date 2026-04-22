using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Test.Common.Repositories.InMemory;

public class InMemorySimilarSchoolsSecondaryRepository : ISimilarSchoolsSecondaryRepository
{
    private List<SimilarSchoolsSecondaryGroupsEntry> _groups = new();
    private List<SimilarSchoolsSecondaryValuesEntry> _values = new();
    private List<SimilarSchoolsSecondaryStandardDeviationsEntry> _standardDeviations = new();

    public void SetupGroups(params SimilarSchoolsSecondaryGroupsEntry[] groups)
    {
        _groups = groups.ToList();
    }

    public void SetupValues(params SimilarSchoolsSecondaryValuesEntry[] values)
    {
        _values = values.ToList();
    }

    public void SetupStandardDeviations(SimilarSchoolsSecondaryStandardDeviationsEntry standardDeviations)
    {
        _standardDeviations = [standardDeviations];
    }

    public Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetSimilarSchoolsGroupAsync(string urn)
        => Task.FromResult((IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>)_groups.Where(x => x.URN == urn).ToList());

    public Task<IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>> GetSecondaryValuesByUrnsAsync(IEnumerable<string> urns)
        => Task.FromResult((IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>)_values.Where(x => urns.Contains(x.URN)).ToList());

    public Task<SimilarSchoolsSecondaryStandardDeviationsEntry?> GetSimilarSchoolsSecondaryStandardDeviationsAsync()
        => Task.FromResult(_standardDeviations.FirstOrDefault());

    public Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet()
        => Task.FromResult((IReadOnlyCollection<string>)_values.Select(v => v.URN).ToList());
}