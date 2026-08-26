using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Primary;

public class SchoolKs2PerformanceDataProvider(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsRepository,
    IKs2PerformanceRepository performanceRepository)
{
    public async Task<SimilarSchoolsData<Ks2PerformanceData>> GetData(string currentSchoolUrn)
    {
        var similarSchoolUrns = (await similarSchoolsRepository.GetGroupAsync(currentSchoolUrn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var schools = (await establishmentRepository.GetEstablishmentsAsync([currentSchoolUrn, .. similarSchoolUrns]))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(currentSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        var currentSchool = schools[currentSchoolUrn];

        var performances = (await performanceRepository.GetByUrnsAsync(schools.Keys))
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
