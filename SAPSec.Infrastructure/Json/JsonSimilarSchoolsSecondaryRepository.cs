using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Json;

public class JsonSimilarSchoolsSecondaryRepository : ISimilarSchoolsSecondaryRepository
{
    private readonly IJsonFile<SimilarSchoolsSecondaryGroupsEntry> _similarSchoolsGroups;
    private readonly IJsonFile<SimilarSchoolsSecondaryValuesEntry> _similarSchoolsValues;
    private readonly IJsonFile<Establishment> _establishments;
    private readonly IJsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry> _standardDeviations;

    public JsonSimilarSchoolsSecondaryRepository(
        IJsonFile<SimilarSchoolsSecondaryGroupsEntry> similarSchoolsGroupsRepository,
        IJsonFile<SimilarSchoolsSecondaryValuesEntry> similarSchoolsValuesRepository,
        IJsonFile<Establishment> establishmentRepository,
        IJsonFile<EstablishmentPerformance> establishmentPerformanceRepository,
        IJsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry> standardDeviationsRepository)
    {
        _similarSchoolsGroups = similarSchoolsGroupsRepository;
        _similarSchoolsValues = similarSchoolsValuesRepository;
        _establishments = establishmentRepository;
        _standardDeviations = standardDeviationsRepository;
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetSimilarSchoolsGroupAsync(string urn)
    {
        var allEstabs = await _establishments.ReadAllAsync();
        var currentEstab = allEstabs.Single(e => e.URN == urn);

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
}
