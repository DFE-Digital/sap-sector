using SAPSec.Core.Exceptions;
using SAPSec.Core.Features.SimilarSchools.Sorting;
using SAPSec.Data.Dto.KS4.Performance;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools;

internal class SecondarySimilarSchoolsDataProvider(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository,
    IKs4PerformanceRepository performanceRepository,
    IAbsenceRepository absenceRepository)
{
    public async Task<SecondarySimilarSchoolsSourceData> GetData(string currentSchoolUrn)
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

        var absences = (await absenceRepository.GetByUrnsAsync(urns))
            .ToDictionary(absence => absence.Urn, StringComparer.Ordinal);
        var performances = (await performanceRepository.GetByUrnsAsync(urns))
            .ToDictionary(performance => performance.Urn, StringComparer.Ordinal);

        var currentSimilarSchool = SimilarSchool.Create(currentEstablishment, absences.GetValueOrDefault(currentSchoolUrn)?.EstablishmentAbsence);

        var similarSchools = groups
            .Select(group =>
            {
                if (!establishments.TryGetValue(group.NeighbourURN, out var establishment))
                {
                    return null;
                }

                var schoolInfo = SchoolInfo.SchoolInfo.Create(establishment);
                if (!schoolInfo.HasSecondaryEducationPhase)
                {
                    return null;
                }

                return new SimilarSchoolSortItem<EstablishmentPerformance>(
                    SimilarSchool.Create(establishment, absences.GetValueOrDefault(group.NeighbourURN)?.EstablishmentAbsence),
                    performances.GetValueOrDefault(group.NeighbourURN)?.EstablishmentPerformance);
            })
            .Where(school => school is not null)
            .Select(school => school!)
            .ToList()
            .AsReadOnly();

        return new SecondarySimilarSchoolsSourceData(
            currentSimilarSchool,
            similarSchools);
    }
}

internal record SecondarySimilarSchoolsSourceData(
    SimilarSchool CurrentSimilarSchool,
    IReadOnlyCollection<SimilarSchoolSortItem<EstablishmentPerformance>> SimilarSchools);
