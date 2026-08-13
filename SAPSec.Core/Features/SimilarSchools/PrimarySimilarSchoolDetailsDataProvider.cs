using SAPSec.Core.Features.Geography;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools;

internal class PrimarySimilarSchoolDetailsDataProvider(
    IEstablishmentRepository establishmentRepository)
{
    public async Task<PrimarySimilarSchoolCoordinatesData> GetCoordinates(string currentSchoolUrn, string similarSchoolUrn)
    {
        var establishments = (await establishmentRepository.GetEstablishmentsAsync(
                [currentSchoolUrn, similarSchoolUrn]))
            .ToDictionary(e => e.URN, StringComparer.Ordinal);

        if (!establishments.TryGetValue(currentSchoolUrn, out var currentEstablishment))
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        if (!establishments.TryGetValue(similarSchoolUrn, out var similarEstablishment))
        {
            throw new NotFoundException($"School not found with URN: {similarSchoolUrn}");
        }

        var currentCoordinates = BNGCoordinates.TryParse(
            currentEstablishment.Easting, currentEstablishment.Northing, out var parsedCurrent)
            ? parsedCurrent
            : null;
        var similarCoordinates = BNGCoordinates.TryParse(
            similarEstablishment.Easting, similarEstablishment.Northing, out var parsedSimilar)
            ? parsedSimilar
            : null;

        return new PrimarySimilarSchoolCoordinatesData(
            currentEstablishment.EstablishmentName,
            currentCoordinates is null ? null : CoordinateConverter.Convert(currentCoordinates),
            similarCoordinates is null ? null : CoordinateConverter.Convert(similarCoordinates),
            currentCoordinates is null || similarCoordinates is null
                ? null
                : currentCoordinates.DistanceMiles(similarCoordinates));
    }
}

internal record PrimarySimilarSchoolCoordinatesData(
    string CurrentSchoolName,
    GeographicCoordinates? CurrentSchoolCoordinates,
    GeographicCoordinates? SimilarSchoolCoordinates,
    double? DistanceMiles);
