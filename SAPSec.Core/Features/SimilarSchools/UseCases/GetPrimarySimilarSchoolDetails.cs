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
        var establishments = (await establishmentRepository.GetEstablishmentsAsync(
                [request.CurrentSchoolUrn, request.SimilarSchoolUrn]))
            .ToDictionary(e => e.URN, StringComparer.Ordinal);

        if (!establishments.TryGetValue(request.CurrentSchoolUrn, out var currentEstablishment))
        {
            throw new NotFoundException($"School not found with URN: {request.CurrentSchoolUrn}");
        }

        if (!establishments.TryGetValue(request.SimilarSchoolUrn, out var similarEstablishment))
        {
            throw new NotFoundException($"School not found with URN: {request.SimilarSchoolUrn}");
        }

        var currentCoordinates = BNGCoordinates.TryParse(
            currentEstablishment.Easting, currentEstablishment.Northing, out var parsedCurrent)
            ? parsedCurrent
            : null;
        var similarCoordinates = BNGCoordinates.TryParse(
            similarEstablishment.Easting, similarEstablishment.Northing, out var parsedSimilar)
            ? parsedSimilar
            : null;

        var similarSchoolDetails = await schoolDetailsService.GetByUrnAsync(request.SimilarSchoolUrn);

        return new(
            currentEstablishment.EstablishmentName,
            currentCoordinates is null ? null : CoordinateConverter.Convert(currentCoordinates),
            similarCoordinates is null ? null : CoordinateConverter.Convert(similarCoordinates),
            currentCoordinates is null || similarCoordinates is null
                ? null
                : currentCoordinates.DistanceMiles(similarCoordinates),
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
