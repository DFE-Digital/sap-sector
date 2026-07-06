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
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(urn))
        {
            throw new NotFoundException($"School not found with URN: {urn}");
        }

        var performances = (await performanceStore.GetByUrnsAsync([urn, .. similarSchoolUrns]))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        //var establishmentPerformances = (await performanceStore.GetEstablishmentByUrnsAsync(schools.Keys))
        //    .ToDictionary(x => x.Id, StringComparer.Ordinal);
        //var laIds = schools.Select(s => s.Value.LocalAuthority.Id);
        //var laPerformances = (await performanceStore.GetLAByIdsAsync(laIds))
        //    .ToDictionary(x => x.Id, StringComparer.Ordinal);
        //var englandPerformance = await performanceStore.GetEnglandAsync();

        //var performances = schools
        //    .ToDictionary(
        //    x => x.Key,
        //    x => new Ks2PerformanceData(
        //        x.Key,
        //        establishmentPerformances.TryGetValue(x.Key, out var p) ? p : null,
        //        laPerformances.TryGetValue(x.Value.LocalAuthority.Id, out var la) ? la : null,
        //        englandPerformance),
        //    StringComparer.Ordinal);

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
