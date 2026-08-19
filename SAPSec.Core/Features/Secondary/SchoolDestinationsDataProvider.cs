using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Secondary;

public class SchoolDestinationsDataProvider(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository,
    IKs4DestinationsRepository destinationsRepository)
{
    public async Task<SimilarSchoolsData<Ks4DestinationsData>> GetData(string currentSchoolUrn)
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

        var performances = (await destinationsRepository.GetByUrnsAsync(schools.Keys))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchoolData = new SchoolData<Ks4DestinationsData>(
            currentSchool,
            performances[currentSchoolUrn]);

        var similarSchoolsData = similarSchoolUrns
            .Where(schools.ContainsKey)
            .Select(urn => new SchoolData<Ks4DestinationsData>(
                schools[urn],
                performances.TryGetValue(urn, out var p) ? p : null))
            .ToList();

        return new SimilarSchoolsData<Ks4DestinationsData>(
            currentSchoolData,
            similarSchoolsData);
    }
}
