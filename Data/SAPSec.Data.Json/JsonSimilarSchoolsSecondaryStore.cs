using SAPSec.Data.Dto;
using SAPSec.Data.Store;

namespace SAPSec.Data.Json;

public class JsonSimilarSchoolsSecondaryStore : ISimilarSchoolsSecondaryStore
{
    private readonly IJsonFile<SimilarSchoolsSecondaryGroupsEntry> _similarSchoolsGroups;
    private readonly IJsonFile<SimilarSchoolsSecondaryValuesEntry> _similarSchoolsValues;
    private readonly IJsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry> _standardDeviations;

    public JsonSimilarSchoolsSecondaryStore(
        IJsonFile<SimilarSchoolsSecondaryGroupsEntry> similarSchoolsGroupsJsonFile,
        IJsonFile<SimilarSchoolsSecondaryValuesEntry> similarSchoolsValuesJsonFile,
        IJsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry> standardDeviationsJsonFile)
    {
        _similarSchoolsGroups = similarSchoolsGroupsJsonFile;
        _similarSchoolsValues = similarSchoolsValuesJsonFile;
        _standardDeviations = standardDeviationsJsonFile;
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetSimilarSchoolsGroupAsync(string urn)
    {
        var rows = await _similarSchoolsGroups.ReadAllAsync();
        var groupRows = rows.Where(r => r.URN == urn).ToList();
        return groupRows.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>> GetSecondaryValuesByUrnsAsync(
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

        var rows = await _similarSchoolsValues.ReadAllAsync();
        var matched = rows.Where(r => urnList.Contains(r.URN)).ToList();

        return matched
            .ToList()
            .AsReadOnly();
    }

    public async Task<SimilarSchoolsSecondaryStandardDeviationsEntry?> GetSimilarSchoolsSecondaryStandardDeviationsAsync()
    {
        var list = await _standardDeviations.ReadAllAsync();
        return list.FirstOrDefault();
    }

    public async Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet()
    {
        var rows = await _similarSchoolsValues.ReadAllAsync();
        return rows.Select(r => r.URN).ToList();
    }
}
