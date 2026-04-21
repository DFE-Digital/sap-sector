using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public class GetSchoolKs4HeadlineMeasures(
    IKs4PerformanceRepository performanceRepository,
    IKs4DestinationsRepository destinationsRepository,
    ISchoolDetailsService schoolDetailsService,
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository)
{
    public async Task<GetSchoolKs4HeadlineMeasuresResponse> Execute(GetSchoolKs4HeadlineMeasuresRequest request)
    {
        var schoolDetails = await schoolDetailsService.GetByUrnAsync(request.Urn);
        var schoolResponse = BuildSchoolResponse(
            schoolDetails,
            await performanceRepository.GetByUrnAsync(request.Urn),
            await destinationsRepository.GetByUrnAsync(request.Urn));

        var similarSchoolUrns = await similarSchoolsRepository.GetSimilarSchoolUrnsAsync(request.Urn);
        var similarSchoolPerformanceData = ((await performanceRepository.GetByUrnsAsync(similarSchoolUrns)) ?? [])
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);
        var similarSchoolDestinationsData = ((await destinationsRepository.GetByUrnsAsync(similarSchoolUrns)) ?? [])
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);
        var similarSchoolDetails = ((await establishmentRepository.GetEstablishmentsAsync(similarSchoolUrns))
                ?? Array.Empty<SAPSec.Core.Model.Generated.Establishment>())
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);

        var similarSchools = similarSchoolUrns
            .Where(similarSchoolDetails.ContainsKey)
            .Select(urn => new SimilarSchoolMeasure(
                urn,
                similarSchoolDetails[urn].EstablishmentName,
                similarSchoolPerformanceData.GetValueOrDefault(urn),
                similarSchoolDestinationsData.GetValueOrDefault(urn)))
            .ToArray();

        return new(
            schoolResponse.SchoolDetails,
            similarSchools.Length,
            BuildComparisonAverage(
                schoolResponse.Attainment8ThreeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    x.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                    x.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                    x.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num))),
            BuildTopPerformers(
                schoolResponse.SchoolDetails,
                schoolResponse.Attainment8ThreeYearAverage.SchoolValue,
                similarSchools,
                x => MeasureValue(
                    x.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                    x.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                    x.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num)),
            BuildComparisonYearByYear(
                schoolResponse.Attainment8YearByYear,
                similarSchools.Select(x => SeriesFrom(
                    x.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                    x.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                    x.PerformanceData?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num))),
            BuildComparisonAverage(
                schoolResponse.EngMaths49ThreeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct,
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct,
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct))),
            BuildTopPerformers(
                schoolResponse.SchoolDetails,
                schoolResponse.EngMaths49ThreeYearAverage.SchoolValue,
                similarSchools,
                x => MeasureValue(
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct,
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct,
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct)),
            BuildComparisonYearByYear(
                schoolResponse.EngMaths49YearByYear,
                similarSchools.Select(x => SeriesFrom(
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct,
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct,
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct))),
            BuildComparisonAverage(
                schoolResponse.EngMaths59ThreeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct,
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct,
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct))),
            BuildTopPerformers(
                schoolResponse.SchoolDetails,
                schoolResponse.EngMaths59ThreeYearAverage.SchoolValue,
                similarSchools,
                x => MeasureValue(
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct,
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct,
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct)),
            BuildComparisonYearByYear(
                schoolResponse.EngMaths59YearByYear,
                similarSchools.Select(x => SeriesFrom(
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct,
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct,
                    x.PerformanceData?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct))),
            BuildComparisonAverage(
                schoolResponse.DestinationsThreeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    x.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct))),
            BuildTopPerformers(
                schoolResponse.SchoolDetails,
                schoolResponse.DestinationsThreeYearAverage.SchoolValue,
                similarSchools,
                x => MeasureValue(
                    x.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct)),
            BuildComparisonYearByYear(
                schoolResponse.DestinationsYearByYear,
                similarSchools.Select(x => SeriesFrom(
                    x.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct))),
            BuildComparisonAverage(
                schoolResponse.DestinationsEducationThreeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    x.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct))),
            BuildTopPerformers(
                schoolResponse.SchoolDetails,
                schoolResponse.DestinationsEducationThreeYearAverage.SchoolValue,
                similarSchools,
                x => MeasureValue(
                    x.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct)),
            BuildComparisonYearByYear(
                schoolResponse.DestinationsEducationYearByYear,
                similarSchools.Select(x => SeriesFrom(
                    x.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct))),
            BuildComparisonAverage(
                schoolResponse.DestinationsEmploymentThreeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    x.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct))),
            BuildTopPerformers(
                schoolResponse.SchoolDetails,
                schoolResponse.DestinationsEmploymentThreeYearAverage.SchoolValue,
                similarSchools,
                x => MeasureValue(
                    x.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct)),
            BuildComparisonYearByYear(
                schoolResponse.DestinationsEmploymentYearByYear,
                similarSchools.Select(x => SeriesFrom(
                    x.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct,
                    x.DestinationsData?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct))));
    }

    private static GetKs4HeadlineMeasuresResponse BuildSchoolResponse(
        SchoolDetails schoolDetails,
        Ks4PerformanceData? performance,
        Ks4DestinationsData? destinations) =>
        new(
            schoolDetails,
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.Attainment8_Tot_LA_Previous2_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.Attainment8_Tot_Eng_Current_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.Attainment8_Tot_Eng_Previous_Num),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.Attainment8_Tot_Eng_Previous2_Num)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.EngMaths49_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.EngMaths49_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.EngMaths49_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.LocalAuthorityPerformance?.EngMaths59_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.EngMaths59_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(performance?.EnglandPerformance?.EngMaths59_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.AllDest_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.AllDest_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.AllDest_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.AllDest_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.AllDest_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.Education_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.Education_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.Education_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.Education_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.Education_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.Education_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.Education_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildAverage(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct)),
            Ks4HeadlineMeasuresCalculator.BuildYearByYear(
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.Employment_Tot_LA_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.LocalAuthorityDestinations?.Employment_Tot_LA_Previous2_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.Employment_Tot_Eng_Current_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.Employment_Tot_Eng_Previous_Pct),
                Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(destinations?.EnglandDestinations?.Employment_Tot_Eng_Previous2_Pct)));

    private static SchoolKs4ComparisonAverage BuildComparisonAverage(
        Ks4HeadlineMeasureAverage current,
        IEnumerable<decimal?> similarSchoolValues) =>
        new(
            current.SchoolValue,
            Average(similarSchoolValues),
            current.LocalAuthorityValue,
            current.EnglandValue);

    private static SchoolKs4ComparisonYearByYear BuildComparisonYearByYear(
        Ks4HeadlineMeasureYearByYear current,
        IEnumerable<Ks4HeadlineMeasureSeries> similarSchoolSeries)
    {
        var similarSeries = similarSchoolSeries.ToArray();

        return new(
            current.School,
            new Ks4HeadlineMeasureSeries(
                Average(similarSeries.Select(x => x.Current)),
                Average(similarSeries.Select(x => x.Previous)),
                Average(similarSeries.Select(x => x.Previous2))),
            current.LocalAuthority,
            current.England);
    }

    private static decimal? MeasureValue(string? current, string? previous, string? previous2) =>
        Ks4HeadlineMeasuresCalculator.Average(
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(current),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(previous),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(previous2));

    private static Ks4HeadlineMeasureSeries SeriesFrom(string? current, string? previous, string? previous2) =>
        new(
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(current),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(previous),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(previous2));

    private static decimal? Average(IEnumerable<decimal?> values)
    {
        var availableValues = values
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        return availableValues.Count == 0
            ? null
            : Math.Round(availableValues.Average(), 1, MidpointRounding.AwayFromZero);
    }

    private static IReadOnlyList<Ks4TopPerformer> BuildTopPerformers(
        SchoolDetails currentSchool,
        decimal? currentSchoolValue,
        IEnumerable<SimilarSchoolMeasure> similarSchoolResponses,
        Func<SimilarSchoolMeasure, decimal?> selector)
    {
        var currentSchoolCandidate = new TopPerformerCandidate(
            currentSchool.Urn,
            currentSchool.Name,
            currentSchoolValue,
            IsCurrentSchool: true);

        return similarSchoolResponses
            .Select(response => new TopPerformerCandidate(
                response.Urn,
                response.Name,
                selector(response),
                IsCurrentSchool: false))
            .Append(currentSchoolCandidate)
            .Where(x => x.Value.HasValue)
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select((x, index) => new Ks4TopPerformer(index + 1, x.Urn, x.Name, x.Value, x.IsCurrentSchool))
            .ToList()
            .AsReadOnly();
    }

    private sealed record TopPerformerCandidate(
        string Urn,
        string Name,
        decimal? Value,
        bool IsCurrentSchool);
}

public record GetSchoolKs4HeadlineMeasuresRequest(string Urn);

public record SchoolKs4ComparisonAverage(
    decimal? SchoolValue,
    decimal? SimilarSchoolsValue,
    decimal? LocalAuthorityValue,
    decimal? EnglandValue);

public record Ks4TopPerformer(
    int Rank,
    string Urn,
    string Name,
    decimal? Value,
    bool IsCurrentSchool = false);

public record SchoolKs4ComparisonYearByYear(
    Ks4HeadlineMeasureSeries School,
    Ks4HeadlineMeasureSeries SimilarSchools,
    Ks4HeadlineMeasureSeries LocalAuthority,
    Ks4HeadlineMeasureSeries England);

public enum SchoolKs4DestinationFilter
{
    All,
    Education,
    Employment
}

public enum SchoolKs4GradeFilter
{
    Grade4,
    Grade5
}

public record SchoolKs4DestinationsSelection(
    SchoolKs4ComparisonAverage ThreeYearAverage,
    IReadOnlyList<Ks4TopPerformer> TopPerformers,
    SchoolKs4ComparisonYearByYear YearByYear)
{
    public static SchoolKs4DestinationFilter ParseFilter(string? destination) =>
        destination?.ToLowerInvariant() switch
        {
            "education" => SchoolKs4DestinationFilter.Education,
            "employment" => SchoolKs4DestinationFilter.Employment,
            _ => SchoolKs4DestinationFilter.All
        };

    public static string ToFilterValue(SchoolKs4DestinationFilter filter) =>
        filter switch
        {
            SchoolKs4DestinationFilter.Education => "education",
            SchoolKs4DestinationFilter.Employment => "employment",
            _ => "all"
        };

    public static SchoolKs4DestinationsSelection From(
        GetSchoolKs4HeadlineMeasuresResponse response,
        SchoolKs4DestinationFilter filter) =>
        filter switch
        {
            SchoolKs4DestinationFilter.Education => new(
                response.DestinationsEducationThreeYearAverage,
                response.DestinationsEducationTopPerformers,
                response.DestinationsEducationYearByYear),
            SchoolKs4DestinationFilter.Employment => new(
                response.DestinationsEmploymentThreeYearAverage,
                response.DestinationsEmploymentTopPerformers,
                response.DestinationsEmploymentYearByYear),
            _ => new(
                response.DestinationsThreeYearAverage,
                response.DestinationsTopPerformers,
                response.DestinationsYearByYear)
        };
}

public record SchoolKs4EngMathsSelection(
    SchoolKs4ComparisonAverage ThreeYearAverage,
    IReadOnlyList<Ks4TopPerformer> TopPerformers,
    SchoolKs4ComparisonYearByYear YearByYear)
{
    public static SchoolKs4GradeFilter ParseFilter(string? grade) =>
        grade == "5"
            ? SchoolKs4GradeFilter.Grade5
            : SchoolKs4GradeFilter.Grade4;

    public static string ToFilterValue(SchoolKs4GradeFilter filter) =>
        filter == SchoolKs4GradeFilter.Grade5 ? "5" : "4";

    public static SchoolKs4EngMathsSelection From(
        GetSchoolKs4HeadlineMeasuresResponse response,
        SchoolKs4GradeFilter filter) =>
        filter switch
        {
            SchoolKs4GradeFilter.Grade5 => new(
                response.EngMaths59ThreeYearAverage,
                response.EngMaths59TopPerformers,
                response.EngMaths59YearByYear),
            _ => new(
                response.EngMaths49ThreeYearAverage,
                response.EngMaths49TopPerformers,
                response.EngMaths49YearByYear)
        };
}

public record GetSchoolKs4HeadlineMeasuresResponse(
    SchoolDetails SchoolDetails,
    int SimilarSchoolsCount,
    SchoolKs4ComparisonAverage Attainment8ThreeYearAverage,
    IReadOnlyList<Ks4TopPerformer> Attainment8TopPerformers,
    SchoolKs4ComparisonYearByYear Attainment8YearByYear,
    SchoolKs4ComparisonAverage EngMaths49ThreeYearAverage,
    IReadOnlyList<Ks4TopPerformer> EngMaths49TopPerformers,
    SchoolKs4ComparisonYearByYear EngMaths49YearByYear,
    SchoolKs4ComparisonAverage EngMaths59ThreeYearAverage,
    IReadOnlyList<Ks4TopPerformer> EngMaths59TopPerformers,
    SchoolKs4ComparisonYearByYear EngMaths59YearByYear,
    SchoolKs4ComparisonAverage DestinationsThreeYearAverage,
    IReadOnlyList<Ks4TopPerformer> DestinationsTopPerformers,
    SchoolKs4ComparisonYearByYear DestinationsYearByYear,
    SchoolKs4ComparisonAverage DestinationsEducationThreeYearAverage,
    IReadOnlyList<Ks4TopPerformer> DestinationsEducationTopPerformers,
    SchoolKs4ComparisonYearByYear DestinationsEducationYearByYear,
    SchoolKs4ComparisonAverage DestinationsEmploymentThreeYearAverage,
    IReadOnlyList<Ks4TopPerformer> DestinationsEmploymentTopPerformers,
    SchoolKs4ComparisonYearByYear DestinationsEmploymentYearByYear);

internal sealed record SimilarSchoolMeasure(string Urn, string Name, Ks4PerformanceData? PerformanceData, Ks4DestinationsData? DestinationsData);
