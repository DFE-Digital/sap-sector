using SAPSec.Core.Features.SimilarSchools.Sorting;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.KS2.Performance;
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

        var absences = (await absenceRepository.GetByUrnsAsync(urns))
            .ToDictionary(absence => absence.Urn, StringComparer.Ordinal);
        var performances = (await performanceRepository.GetByUrnsAsync(urns))
            .ToDictionary(performance => performance.Urn, StringComparer.Ordinal);

        var currentSimilarSchool = SimilarSchool.FromData(currentEstablishment, absences.GetValueOrDefault(currentSchoolUrn)?.EstablishmentAbsence);

        var similarSchools = groups
            .Select(group =>
            {
                if (!establishments.TryGetValue(group.NeighbourURN, out var establishment))
                {
                    return null;
                }

                return new SimilarSchoolSortItem<EstablishmentPerformance>(
                    SimilarSchool.FromData(establishment, absences.GetValueOrDefault(group.NeighbourURN)?.EstablishmentAbsence),
                    performances.GetValueOrDefault(group.NeighbourURN)?.EstablishmentPerformance);

                //return new PrimaryRankedSimilarSchoolData(
                //    group.Rank,
                //    group.Dist,
                //    similarSchool,
                //    performances.GetValueOrDefault(group.NeighbourURN));
            })
            .Where(school => school is not null)
            .Select(school => school!)
            .ToList()
            .AsReadOnly();

        return new PrimarySimilarSchoolsSourceData(
            currentEstablishment,
            currentSimilarSchool,
            similarSchools);
    }
}

internal record PrimarySimilarSchoolsSourceData(
    Establishment CurrentEstablishment,
    SimilarSchool CurrentSimilarSchool,
    IReadOnlyCollection<SimilarSchoolSortItem<EstablishmentPerformance>> SimilarSchools);
