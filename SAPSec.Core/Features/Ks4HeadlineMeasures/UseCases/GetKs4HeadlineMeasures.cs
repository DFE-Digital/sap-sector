using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public class GetKs4HeadlineMeasures(
    IKs4PerformanceRepository repository,
    ISchoolDetailsService schoolDetailsService)
{
    public async Task<GetKs4HeadlineMeasuresResponse> Execute(GetKs4HeadlineMeasuresRequest request)
    {
        var schoolDetails = await schoolDetailsService.GetByUrnAsync(request.Urn);
        var data = await repository.GetByUrnAsync(request.Urn);
        return Ks4HeadlineMeasuresResponseFactory.Create(schoolDetails, data);
    }
}

public record GetKs4HeadlineMeasuresRequest(string Urn);

public record Ks4HeadlineMeasureAverage(
    decimal? SchoolValue,
    decimal? LocalAuthorityValue,
    decimal? EnglandValue);

public record Ks4HeadlineMeasureSeries(
    decimal? Current,
    decimal? Previous,
    decimal? Previous2);

public record Ks4HeadlineMeasureYearByYear(
    Ks4HeadlineMeasureSeries School,
    Ks4HeadlineMeasureSeries LocalAuthority,
    Ks4HeadlineMeasureSeries England);

public record GetKs4HeadlineMeasuresResponse(
    SchoolDetails SchoolDetails,
    Ks4HeadlineMeasureAverage Attainment8ThreeYearAverage,
    Ks4HeadlineMeasureYearByYear Attainment8YearByYear,
    Ks4HeadlineMeasureAverage EngMaths49ThreeYearAverage,
    Ks4HeadlineMeasureYearByYear EngMaths49YearByYear,
    Ks4HeadlineMeasureAverage EngMaths59ThreeYearAverage,
    Ks4HeadlineMeasureYearByYear EngMaths59YearByYear,
    Ks4HeadlineMeasureAverage DestinationsThreeYearAverage,
    Ks4HeadlineMeasureYearByYear DestinationsYearByYear,
    Ks4HeadlineMeasureAverage DestinationsEducationThreeYearAverage,
    Ks4HeadlineMeasureYearByYear DestinationsEducationYearByYear,
    Ks4HeadlineMeasureAverage DestinationsEmploymentThreeYearAverage,
    Ks4HeadlineMeasureYearByYear DestinationsEmploymentYearByYear);


