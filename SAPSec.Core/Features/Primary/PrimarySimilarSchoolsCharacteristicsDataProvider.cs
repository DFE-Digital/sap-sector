using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

public class PrimarySimilarSchoolsCharacteristicsDataProvider(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsRepository)
{
    public async Task<PrimarySimilarSchoolsData> GetSimilarSchoolsCharacteristics(string currentSchoolUrn)
    {
        var groups = (await similarSchoolsRepository.GetGroupAsync(currentSchoolUrn))
            .Where(group => !string.IsNullOrWhiteSpace(group.NeighbourURN))
            .ToList();

        var similarSchoolUrns = groups
            .Select(group => group.NeighbourURN)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var urns = similarSchoolUrns
            .Concat([currentSchoolUrn])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var schools = (await establishmentRepository.GetEstablishmentsAsync(urns))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(school => school.Urn, StringComparer.Ordinal);

        if (!schools.TryGetValue(currentSchoolUrn, out var currentSchool))
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        var characteristics = SimilarSchoolsPrimaryValues.FromData(
                await similarSchoolsRepository.GetValuesByUrnsAsync(urns))
            .ToDictionary(values => values.Urn, StringComparer.Ordinal);

        if (!characteristics.TryGetValue(currentSchoolUrn, out var currentCharacteristics))
        {
            throw new NotFoundException($"No similar schools characteristics found for URN {currentSchoolUrn}");
        }

        var similarSchools = groups
            .Select(group =>
            {
                if (!schools.TryGetValue(group.NeighbourURN, out var school))
                {
                    return null;
                }

                if (!characteristics.TryGetValue(group.NeighbourURN, out var values))
                {
                    return null;
                }

                return new RankedSchoolData<SimilarSchoolsPrimaryValues>(
                    group.Rank,
                    group.Dist,
                    new SchoolData<SimilarSchoolsPrimaryValues>(school, values));
            })
            .Where(school => school is not null)
            .Select(school => school!)
            .ToList()
            .AsReadOnly();

        return new PrimarySimilarSchoolsData(
            new SchoolData<SimilarSchoolsPrimaryValues>(currentSchool, currentCharacteristics),
            similarSchools);
    }
}

public record PrimarySimilarSchoolsData(
    SchoolData<SimilarSchoolsPrimaryValues> CurrentSchool,
    IReadOnlyCollection<RankedSchoolData<SimilarSchoolsPrimaryValues>> SimilarSchools);

public record RankedSchoolData<T>(
    string Rank,
    string Distance,
    SchoolData<T> School);
