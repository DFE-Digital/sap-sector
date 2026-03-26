using SAPSec.Core.Features.Geography;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Data;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetSimilarSchoolDetails(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository,
    ISchoolDetailsService schoolDetailsService,
    IKs4PerformanceRepository performanceRepository)
{
    public async Task<GetSimilarSchoolDetailsResponse> Execute(GetSimilarSchoolDetailsRequest request)
    {
        // TODO: Validate SimilarSchoolUrn actually belongs in similar schools group for current school
        var groups = await similarSchoolsRepository.GetSimilarSchoolsGroupAsync(request.CurrentSchoolUrn);
        var urns = groups.Select(g => g.NeighbourURN).Concat([request.CurrentSchoolUrn]);
        var establishments = await establishmentRepository.GetEstablishmentsAsync(urns);
        var performances = await performanceRepository.GetEstablishmentPerformanceAsync(urns);
        var schools = establishments.GroupJoin(performances, e => e.URN, p => p.Id, SimilarSchool.FromData).ToList();
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
