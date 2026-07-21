using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools;

internal class PrimarySimilarSchoolsDataProvider(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsRepository,
    IAbsenceRepository absenceRepository,
    IKs2PerformanceRepository performanceRepository)
{
    public async Task<PrimarySimilarSchoolsSourceData> GetSimilarSchoolsData(string currentSchoolUrn)
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

        var establishments = (await establishmentRepository.GetEstablishmentsAsync(urns))
            .ToDictionary(establishment => establishment.URN, StringComparer.Ordinal);

        if (!establishments.TryGetValue(currentSchoolUrn, out var currentEstablishment))
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        var characteristics = SimilarSchoolsPrimaryValues.FromData(
                await similarSchoolsRepository.GetValuesByUrnsAsync(urns))
            .ToDictionary(values => values.Urn, StringComparer.Ordinal);

        // A school with no similar schools group also has no row in the similar schools
        // values dataset - that's a valid "no similar schools" state, not a not-found error.
        var currentCharacteristics = characteristics.TryGetValue(currentSchoolUrn, out var foundCharacteristics)
            ? foundCharacteristics
            : new SimilarSchoolsPrimaryValues { Urn = currentSchoolUrn };

        var absences = (await absenceRepository.GetByUrnsAsync(urns))
            .ToDictionary(absence => absence.Urn, StringComparer.Ordinal);
        var performances = (await performanceRepository.GetByUrnsAsync(urns))
            .ToDictionary(performance => performance.Urn, StringComparer.Ordinal);

        var currentSimilarSchool = SimilarSchool.FromData(
            currentEstablishment,
            null,
            absences.GetValueOrDefault(currentSchoolUrn)?.EstablishmentAbsence);

        var similarSchools = groups
            .Select(group =>
            {
                if (!establishments.TryGetValue(group.NeighbourURN, out var establishment))
                {
                    return null;
                }

                if (!characteristics.TryGetValue(group.NeighbourURN, out var values))
                {
                    return null;
                }

                var similarSchool = SimilarSchool.FromData(
                    establishment,
                    null,
                    absences.GetValueOrDefault(group.NeighbourURN)?.EstablishmentAbsence);

                return new PrimaryRankedSimilarSchoolData(
                    group.Rank,
                    group.Dist,
                    similarSchool,
                    values,
                    performances.GetValueOrDefault(group.NeighbourURN));
            })
            .Where(school => school is not null)
            .Select(school => school!)
            .ToList()
            .AsReadOnly();

        return new PrimarySimilarSchoolsSourceData(
            currentEstablishment,
            currentCharacteristics,
            currentSimilarSchool,
            similarSchools);
    }
}

internal record PrimarySimilarSchoolsSourceData(
    Establishment CurrentEstablishment,
    SimilarSchoolsPrimaryValues CurrentCharacteristics,
    SimilarSchool CurrentSimilarSchool,
    IReadOnlyCollection<PrimaryRankedSimilarSchoolData> SimilarSchools);
