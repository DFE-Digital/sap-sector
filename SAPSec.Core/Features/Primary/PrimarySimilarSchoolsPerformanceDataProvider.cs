using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Store;

namespace SAPSec.Core.Features.Primary;

public class PrimarySimilarSchoolsPerformanceDataProvider(
    IEstablishmentStore establishmentStore,
    ISimilarSchoolsPrimaryStore similarSchoolsStore,
    IKs2PerformanceStore performanceStore)
{
    public async Task<SimilarSchoolsData<Ks2PerformanceData>> GetSimilarSchoolsPerformance(string currentSchoolUrn)
    {
        var similarSchoolUrns = (await similarSchoolsStore.GetGroupAsync(currentSchoolUrn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var schools = (await establishmentStore.GetEstablishmentsAsync([currentSchoolUrn, .. similarSchoolUrns]))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(currentSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        var currentSchool = schools[currentSchoolUrn];

        var performances = (await performanceStore.GetByUrnsAsync(schools.Keys))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchoolData = new SchoolData<Ks2PerformanceData>(
            currentSchool,
            performances[currentSchoolUrn]);

        var similarSchoolsData = similarSchoolUrns
            .Where(schools.ContainsKey)
            .Select(urn => new SchoolData<Ks2PerformanceData>(
                schools[urn],
                performances.TryGetValue(urn, out var p) ? p : null))
            .ToList();

        return new SimilarSchoolsData<Ks2PerformanceData>(
            currentSchoolData,
            similarSchoolsData);
    }
}
