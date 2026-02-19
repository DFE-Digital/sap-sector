using SAPSec.Core.Features.Geography;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetSimilarSchoolDetails(
    ISimilarSchoolsSecondaryRepository repository,
    ISchoolDetailsService schoolDetailsService)
{
    public async Task<GetSimilarSchoolDetailsResponse> Execute(GetSimilarSchoolDetailsRequest request)
    {
        // TODO: Validate SimilarSchoolUrn actually belongs in similar schools group for current school
        var (currentSchool, similarSchools) = await repository.GetSimilarSchoolsGroupAsync(request.CurrentSchoolUrn);
        var similarSchoolDetails = await schoolDetailsService.GetByUrnAsync(request.SimilarSchoolUrn);

        var similarSchool = similarSchools.Single(s => s.URN == request.SimilarSchoolUrn);

        return new(
            currentSchool.Name,
            // TODO: Validate coordinates exist
            CoordinateConverter.Convert(currentSchool.Coordinates!),
            CoordinateConverter.Convert(similarSchool.Coordinates!),
            currentSchool.Coordinates!.DistanceMiles(similarSchool.Coordinates!),
            similarSchoolDetails
        );
    }
}

public record GetSimilarSchoolDetailsRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn);

public record GetSimilarSchoolDetailsResponse(
    string SchoolName,
    GeographicCoordinates CurrentSchoolCoordinates,
    GeographicCoordinates SimilarSchoolCoordinates,
    double DistanceMiles,
    SchoolDetails SimilarSchoolDetails);
