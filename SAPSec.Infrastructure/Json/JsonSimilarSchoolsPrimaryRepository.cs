using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Json;

public class JsonSimilarSchoolsPrimaryRepository : ISimilarSchoolsPrimaryRepository
{
    private readonly IJsonFile<SimilarSchoolsPrimaryGroupsEntry> _groupsFile;
    private readonly IJsonFile<SimilarSchoolsPrimaryValuesEntry> _valuesFile;

    public JsonSimilarSchoolsPrimaryRepository(
        IJsonFile<SimilarSchoolsPrimaryGroupsEntry> groupsFile,
        IJsonFile<SimilarSchoolsPrimaryValuesEntry> valuesFile)
    {
        _groupsFile = groupsFile;
        _valuesFile = valuesFile;
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>> GetGroupAsync(string urn)
    {
        var rows = await _groupsFile.ReadAllAsync();
        return rows
            .Where(r => r.URN == urn)
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsPrimaryValuesEntry>> GetValuesByUrnsAsync(
        IEnumerable<string> urns)
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

        var rows = await _valuesFile.ReadAllAsync();
        return rows
            .Where(r => urnList.Contains(r.URN))
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet()
    {
        var rows = await _valuesFile.ReadAllAsync();
        return rows.Select(r => r.URN).ToList();
    }
}
