using SAPSec.Core.Exceptions;
using SAPSec.Core.School.Info;
using SAPSec.Core.School.Similarity;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.School.Primary;

public class PrimarySimilarSchoolsPerformanceDataProvider(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsRepository,
    IKs2PerformanceRepository performanceRepository)
{
    public async Task<SimilarSchoolsData<Ks2PerformanceData>> GetSimilarSchoolsPerformance(string currentSchoolUrn)
    {
        var similarSchoolUrns = (await similarSchoolsRepository.GetGroupAsync(currentSchoolUrn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var schools = (await establishmentRepository.GetEstablishmentsAsync([currentSchoolUrn, .. similarSchoolUrns]))
            .Select(SchoolInfo.FromEstablishment)
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
