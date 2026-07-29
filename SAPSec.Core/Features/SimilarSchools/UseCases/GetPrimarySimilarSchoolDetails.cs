using SAPSec.Core.Features.Geography;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetPrimarySimilarSchoolDetailsUseCase(
    IEstablishmentRepository establishmentRepository,
    ISchoolDetailsService schoolDetailsService)
    : IUseCase<GetPrimarySimilarSchoolDetailsRequest, GetPrimarySimilarSchoolDetailsResponse>
{
    public async Task<GetPrimarySimilarSchoolDetailsResponse> Execute(GetPrimarySimilarSchoolDetailsRequest request)
    {
        var dataProvider = new PrimarySimilarSchoolDetailsDataProvider(establishmentRepository);

        var coordinates = await dataProvider.GetCoordinates(request.CurrentSchoolUrn, request.SimilarSchoolUrn);

        var similarSchoolDetails = await schoolDetailsService.GetByUrnAsync(request.SimilarSchoolUrn);

        return new(
            coordinates.CurrentSchoolName,
            coordinates.CurrentSchoolCoordinates,
            coordinates.SimilarSchoolCoordinates,
            coordinates.DistanceMiles,
            similarSchoolDetails);
    }
}

public record GetPrimarySimilarSchoolDetailsRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn);

public record GetPrimarySimilarSchoolDetailsResponse(
    string SchoolName,
    GeographicCoordinates? CurrentSchoolCoordinates,
    GeographicCoordinates? SimilarSchoolCoordinates,
    double? DistanceMiles,
    SchoolDetails SimilarSchoolDetails);
