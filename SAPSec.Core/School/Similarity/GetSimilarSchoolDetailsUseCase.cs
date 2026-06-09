using SAPSec.Core.Exceptions;
using SAPSec.Core.Geography;
using SAPSec.Core.School.Details;
using SAPSec.Core.UseCases;
using SAPSec.Data.Store;

namespace SAPSec.Core.School.Similarity;

public class GetSimilarSchoolDetailsUseCase(
    IEstablishmentStore establishmentStore,
    ISimilarSchoolsSecondaryStore similarSchoolsStore,
    ISchoolDetailsService schoolDetailsService,
    IKs4PerformanceStore performanceStore,
    IAbsenceStore absenceStore)
    : IUseCase<GetSimilarSchoolDetailsRequest, GetSimilarSchoolDetailsResponse>
{
    public async Task<GetSimilarSchoolDetailsResponse> Execute(GetSimilarSchoolDetailsRequest request)
    {
        // TODO: Validate SimilarSchoolUrn actually belongs in similar schools group for current school
        var groups = await similarSchoolsStore.GetSimilarSchoolsGroupAsync(request.CurrentSchoolUrn);
        var urns = groups.Select(g => g.NeighbourURN).Concat([request.CurrentSchoolUrn]);
        var establishments = await establishmentStore.GetEstablishmentsAsync(urns);
        var performance = await performanceStore.GetByUrnsAsync(urns);
        var absence = await absenceStore.GetByUrnsAsync(urns);

        var schools =
            from e in establishments
            join p in performance on e.URN equals p.URN into perf
            join a in absence on e.URN equals a.URN into abs
            select SimilarSchool.FromData(e, perf.FirstOrDefault()?.EstablishmentPerformance, abs.FirstOrDefault()?.EstablishmentAbsence);

        var currentSchool = schools.FirstOrDefault(s => s.URN == request.CurrentSchoolUrn);
        var similarSchool = schools.FirstOrDefault(s => s.URN == request.SimilarSchoolUrn);

        if (currentSchool is null)
        {
            throw new NotFoundException($"School not found with URN: {request.CurrentSchoolUrn}");
        }

        if (similarSchool is null)
        {
            throw new NotFoundException($"School not found with URN: {request.SimilarSchoolUrn}");
        }

        var similarSchoolDetails = await schoolDetailsService.GetByUrnAsync(request.SimilarSchoolUrn);

        return new(
            currentSchool.Name,
            // TODO: Validate coordinates exist
            currentSchool.Coordinates is null ? null : CoordinateConverter.Convert(currentSchool.Coordinates),
            similarSchool.Coordinates is null ? null : CoordinateConverter.Convert(similarSchool.Coordinates),
            currentSchool.Coordinates is null || similarSchool.Coordinates is null
                ? null
                : currentSchool.Coordinates.DistanceMiles(similarSchool.Coordinates),
            similarSchoolDetails
        );
    }
}

public record GetSimilarSchoolDetailsRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn);

public record GetSimilarSchoolDetailsResponse(
    string SchoolName,
    GeographicCoordinates? CurrentSchoolCoordinates,
    GeographicCoordinates? SimilarSchoolCoordinates,
    double? DistanceMiles,
    SchoolDetails SimilarSchoolDetails);
