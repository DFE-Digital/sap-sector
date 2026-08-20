using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Json;

public class JsonSimilarSchoolsSecondaryRepository : ISimilarSchoolsSecondaryRepository
{
    private readonly IJsonFile<SimilarSchoolsSecondaryGroupsEntry> _groupsFile;
    private readonly IJsonFile<SimilarSchoolsSecondaryValuesEntry> _valuesFile;

    public JsonSimilarSchoolsSecondaryRepository(
        IJsonFile<SimilarSchoolsSecondaryGroupsEntry> groupsFile,
        IJsonFile<SimilarSchoolsSecondaryValuesEntry> valuesFile)
    {
        _groupsFile = groupsFile;
        _valuesFile = valuesFile;
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetGroupAsync(string urn)
    {
        var rows = await _groupsFile.ReadAllAsync();
        var groupRows = rows.Where(r => r.URN == urn).ToList();
        return groupRows.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>> GetValuesByUrnsAsync(
        IEnumerable<string> urns)
    {
        if (urns is null)
        {
            return Array.Empty<SimilarSchoolsSecondaryValuesEntry>();
        }

        var urnList = urns as IList<string> ?? urns.ToList();
        if (urnList.Count == 0)
        {
            return Array.Empty<SimilarSchoolsSecondaryValuesEntry>();
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
