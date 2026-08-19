using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Secondary;

public class ComparisonPerformanceDataProvider(
    IEstablishmentRepository establishmentRepository,
    IKs4PerformanceRepository performanceRepository)
{
    public async Task<(SchoolData<Ks4PerformanceData> CurrentSchool, SchoolData<Ks4PerformanceData> SimilarSchool)> GetData(
        string currentSchoolUrn,
        string similarSchoolUrn)
    {
        var urns = new[] { currentSchoolUrn, similarSchoolUrn };

        var schools = (await establishmentRepository.GetEstablishmentsAsync(urns))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(currentSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        if (!schools.ContainsKey(similarSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {similarSchoolUrn}");
        }

        var performances = (await performanceRepository.GetByUrnsAsync(urns))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchoolData = new SchoolData<Ks4PerformanceData>(
            schools[currentSchoolUrn],
            performances.TryGetValue(currentSchoolUrn, out var currentPerformance) ? currentPerformance : null);

        var similarSchoolData = new SchoolData<Ks4PerformanceData>(
            schools[similarSchoolUrn],
            performances.TryGetValue(similarSchoolUrn, out var similarPerformance) ? similarPerformance : null);

        return (currentSchoolData, similarSchoolData);
    }
}
