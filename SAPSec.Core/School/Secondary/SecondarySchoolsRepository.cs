using SAPSec.Core.Exceptions;
using SAPSec.Core.School.Info;
using SAPSec.Data.Store;

namespace SAPSec.Core.School.Secondary;

public class SecondarySchoolsRepository(
    IEstablishmentStore establishmentStore,
    ISimilarSchoolsSecondaryStore similarSchoolsStore,
    IKs4PerformanceStore performanceStore,
    IKs4DestinationsStore destinationsStore) : ISecondarySchoolsRepository
{
    public async Task<SecondarySimilarSchoolsData<Ks4PerformanceData>> GetSimilarSchoolsPerformance(string urn)
    {
        var similarSchoolUrns = (await similarSchoolsStore.GetSimilarSchoolsGroupAsync(urn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var schools = (await establishmentStore.GetEstablishmentsAsync([urn, .. similarSchoolUrns]))
            .Select(e => new SchoolInfo(e.URN, e.EstablishmentName))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(urn))
        {
            throw new NotFoundException($"School not found with URN: {urn}");
        }

        var performances = (await performanceStore.GetByUrnsAsync([urn, .. similarSchoolUrns]))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);

        var currentSchool = new SecondarySchoolData<Ks4PerformanceData>(
            schools[urn],
            performances[urn]);

        var similarSchools = similarSchoolUrns
            .Where(schools.ContainsKey)
            .Select(urn => new SecondarySchoolData<Ks4PerformanceData>(
                schools[urn],
                performances.TryGetValue(urn, out var p) ? p : null))
            .ToList();

        return new SecondarySimilarSchoolsData<Ks4PerformanceData>(
            currentSchool,
            similarSchools);
    }

    public async Task<SecondarySimilarSchoolsData<Ks4DestinationsData>> GetSimilarSchoolsDestinations(string urn)
    {
        var similarSchoolUrns = (await similarSchoolsStore.GetSimilarSchoolsGroupAsync(urn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var schools = (await establishmentStore.GetEstablishmentsAsync([urn, .. similarSchoolUrns]))
            .Select(e => new SchoolInfo(e.URN, e.EstablishmentName))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(urn))
        {
            throw new NotFoundException($"School not found with URN: {urn}");
        }

        var destinations = (await destinationsStore.GetByUrnsAsync([urn, .. similarSchoolUrns]))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchool = new SecondarySchoolData<Ks4DestinationsData>(
            schools[urn],
            destinations[urn]);

        var similarSchools = similarSchoolUrns
            .Where(schools.ContainsKey)
            .Select(urn => new SecondarySchoolData<Ks4DestinationsData>(
                schools[urn],
                destinations.TryGetValue(urn, out var p) ? p : null))
            .ToList();

        return new SecondarySimilarSchoolsData<Ks4DestinationsData>(
            currentSchool,
            similarSchools);
    }
}
