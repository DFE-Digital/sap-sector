using SAPSec.Data.Store;

namespace SAPSec.Core.Features.Primary;

public class PrimarySchoolsRepository(
    IEstablishmentStore establishmentStore,
    ISimilarSchoolsPrimaryStore similarSchoolsStore,
    IKs2PerformanceStore performanceStore) : IPrimarySchoolsRepository
{
    public async Task<SimilarSchoolsData<Ks2PerformanceData>> GetSimilarSchoolsPerformance(string urn)
    {
        var similarSchoolUrns = (await similarSchoolsStore.GetGroupAsync(urn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var schools = (await establishmentStore.GetEstablishmentsAsync([urn, .. similarSchoolUrns]))
            .Select(e => new SchoolInfo.SchoolInfo(e.URN, e.EstablishmentName, SchoolInfo.Address.FromEstablishment(e)))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(urn))
        {
            throw new NotFoundException($"School not found with URN: {urn}");
        }

        var performances = (await performanceStore.GetByUrnsAsync([urn, .. similarSchoolUrns]))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchool = new SchoolData<Ks2PerformanceData>(
            schools[urn],
            performances[urn]);

        var similarSchools = similarSchoolUrns
            .Where(schools.ContainsKey)
            .Select(urn => new SchoolData<Ks2PerformanceData>(
                schools[urn],
                performances.TryGetValue(urn, out var p) ? p : null))
            .ToList();

        return new SimilarSchoolsData<Ks2PerformanceData>(
            currentSchool,
            similarSchools);
    }
}
