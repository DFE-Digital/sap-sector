using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Secondary;

public class SecondarySchoolsDataProvider(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository,
    IKs4PerformanceRepository performanceRepository,
    IKs4DestinationsRepository destinationsRepository) : ISecondarySchoolsDataProvider
{
    public async Task<SimilarSchoolsData<Ks4PerformanceData>> GetSimilarSchoolsPerformance(string urn)
    {
        var similarSchoolUrns = (await similarSchoolsRepository.GetGroupAsync(urn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var schools = (await establishmentRepository.GetEstablishmentsAsync([urn, .. similarSchoolUrns]))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(urn))
        {
            throw new NotFoundException($"School not found with URN: {urn}");
        }

        var performances = (await performanceRepository.GetByUrnsAsync([urn, .. similarSchoolUrns]))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchool = new SchoolData<Ks4PerformanceData>(
            schools[urn],
            performances[urn]);

        var similarSchools = similarSchoolUrns
            .Where(schools.ContainsKey)
            .Select(urn => new SchoolData<Ks4PerformanceData>(
                schools[urn],
                performances.TryGetValue(urn, out var p) ? p : null))
            .ToList();

        return new SimilarSchoolsData<Ks4PerformanceData>(
            currentSchool,
            similarSchools);
    }

    public async Task<SimilarSchoolsData<Ks4DestinationsData>> GetSimilarSchoolsDestinations(string urn)
    {
        var similarSchoolUrns = (await similarSchoolsRepository.GetGroupAsync(urn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var schools = (await establishmentRepository.GetEstablishmentsAsync([urn, .. similarSchoolUrns]))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(urn))
        {
            throw new NotFoundException($"School not found with URN: {urn}");
        }

        var destinations = (await destinationsRepository.GetByUrnsAsync([urn, .. similarSchoolUrns]))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchool = new SchoolData<Ks4DestinationsData>(
            schools[urn],
            destinations[urn]);

        var similarSchools = similarSchoolUrns
            .Where(schools.ContainsKey)
            .Select(urn => new SchoolData<Ks4DestinationsData>(
                schools[urn],
                destinations.TryGetValue(urn, out var p) ? p : null))
            .ToList();

        return new SimilarSchoolsData<Ks4DestinationsData>(
            currentSchool,
            similarSchools);
    }
}
