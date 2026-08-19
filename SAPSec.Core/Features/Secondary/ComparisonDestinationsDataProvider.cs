using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Secondary;

public class ComparisonDestinationsDataProvider(
    IEstablishmentRepository establishmentRepository,
    IKs4DestinationsRepository destinationsRepository)
{
    public async Task<(SchoolData<Ks4DestinationsData> CurrentSchool, SchoolData<Ks4DestinationsData> SimilarSchool)> GetData(
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

        var destinations = (await destinationsRepository.GetByUrnsAsync(urns))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchoolData = new SchoolData<Ks4DestinationsData>(
            schools[currentSchoolUrn],
            destinations.TryGetValue(currentSchoolUrn, out var currentPerformance) ? currentPerformance : null);

        var similarSchoolData = new SchoolData<Ks4DestinationsData>(
            schools[similarSchoolUrn],
            destinations.TryGetValue(similarSchoolUrn, out var similarPerformance) ? similarPerformance : null);

        return (currentSchoolData, similarSchoolData);
    }
}
