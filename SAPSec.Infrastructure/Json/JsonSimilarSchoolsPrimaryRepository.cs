using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Json;

public class JsonSimilarSchoolsPrimaryRepository : ISimilarSchoolsPrimaryRepository
{
    private readonly IJsonFile<SimilarSchoolsPrimaryGroupsEntry> _similarSchoolsGroups;
    private readonly IJsonFile<SimilarSchoolsPrimaryValuesEntry> _similarSchoolsValues;

    public JsonSimilarSchoolsPrimaryRepository(
        IJsonFile<SimilarSchoolsPrimaryGroupsEntry> similarSchoolsGroupsRepository,
        IJsonFile<SimilarSchoolsPrimaryValuesEntry> similarSchoolsValuesRepository)
    {
        _similarSchoolsGroups = similarSchoolsGroupsRepository;
        _similarSchoolsValues = similarSchoolsValuesRepository;
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>> GetSimilarSchoolsGroupAsync(string urn)
    {
        var rows = await _similarSchoolsGroups.ReadAllAsync();
        return rows
            .Where(r => r.URN == urn)
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsPrimaryValuesEntry>> GetPrimaryValuesByUrnsAsync(IEnumerable<string> urns)
    {
        if (urns is null)
        {
            return Array.Empty<SimilarSchoolsPrimaryValuesEntry>();
        }

        var urnList = urns as IList<string> ?? urns.ToList();
        if (urnList.Count == 0)
        {
            return Array.Empty<SimilarSchoolsPrimaryValuesEntry>();
        }

        var rows = await _similarSchoolsValues.ReadAllAsync();
        return rows
            .Where(r => urnList.Contains(r.URN))
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet()
    {
        var rows = await _similarSchoolsValues.ReadAllAsync();
        return rows.Select(r => r.URN).ToList();
    }
}
