using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Store;

namespace SAPSec.Infrastructure.Json;

public class JsonSimilarSchoolsPrimaryStore : ISimilarSchoolsPrimaryStore
{
    private readonly IJsonFile<SimilarSchoolsPrimaryGroupsEntry> _groupsFile;
    private readonly IJsonFile<SimilarSchoolsPrimaryValuesEntry> _valuesFile;

    public JsonSimilarSchoolsPrimaryStore(
        IJsonFile<SimilarSchoolsPrimaryGroupsEntry> groupsFile,
        IJsonFile<SimilarSchoolsPrimaryValuesEntry> valuesFile)
    {
        _groupsFile = groupsFile;
        _valuesFile = valuesFile;
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>> GetGroupAsync(string urn)
    {
        var rows = await _groupsFile.ReadAllAsync();
        var groupRows = rows.Where(r => r.URN == urn).ToList();
        return groupRows.AsReadOnly();
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
        var matched = rows.Where(r => urnList.Contains(r.URN)).ToList();

        return matched
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet()
    {
        var rows = await _valuesFile.ReadAllAsync();
        return rows.Select(r => r.URN).ToList();
    }
}
