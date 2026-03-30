using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public class GetSchoolKs4HeadlineMeasures(
    IKs4PerformanceRepository repository,
    ISchoolDetailsService schoolDetailsService,
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository)
{
    public async Task<GetSchoolKs4HeadlineMeasuresResponse> Execute(GetSchoolKs4HeadlineMeasuresRequest request)
    {
        var schoolDetails = await schoolDetailsService.GetByUrnAsync(request.Urn);
        var schoolResponse = Ks4HeadlineMeasuresResponseFactory.Create(
            schoolDetails,
            await repository.GetByUrnAsync(request.Urn));
        var similarSchoolUrns = await similarSchoolsRepository.GetSimilarSchoolUrnsAsync(request.Urn);
        var similarSchoolData = await repository.GetByUrnsAsync(similarSchoolUrns)
            ?? new Dictionary<string, Ks4HeadlineMeasuresData?>(StringComparer.Ordinal);
        var similarSchoolDetails = ((await establishmentRepository.GetEstablishmentsAsync(similarSchoolUrns))
                ?? Array.Empty<SAPSec.Core.Model.Generated.Establishment>())
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);

        var similarSchools = similarSchoolUrns
            .Where(similarSchoolDetails.ContainsKey)
            .Select(urn => new SimilarSchoolMeasure(
                urn,
                similarSchoolDetails[urn].EstablishmentName,
                similarSchoolData.GetValueOrDefault(urn)))
            .ToArray();

        return new(
            schoolResponse.SchoolDetails,
            similarSchools.Length,
            BuildComparisonAverage(
                schoolResponse.Attainment8ThreeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    x.Data?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                    x.Data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                    x.Data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num))),
            BuildTopPerformers(
                similarSchools,
                x => MeasureValue(
                    x.Data?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                    x.Data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                    x.Data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num)),
            BuildComparisonYearByYear(
                schoolResponse.Attainment8YearByYear,
                similarSchools.Select(x => SeriesFrom(
                    x.Data?.EstablishmentPerformance?.Attainment8_Tot_Est_Current_Num,
                    x.Data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous_Num,
                    x.Data?.EstablishmentPerformance?.Attainment8_Tot_Est_Previous2_Num))),
            BuildComparisonAverage(
                schoolResponse.EngMaths49ThreeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    x.Data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct))),
            BuildTopPerformers(
                similarSchools,
                x => MeasureValue(
                    x.Data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct)),
            BuildComparisonYearByYear(
                schoolResponse.EngMaths49YearByYear,
                similarSchools.Select(x => SeriesFrom(
                    x.Data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentPerformance?.EngMaths49_Tot_Est_Previous2_Pct))),
            BuildComparisonAverage(
                schoolResponse.EngMaths59ThreeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    x.Data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct))),
            BuildTopPerformers(
                similarSchools,
                x => MeasureValue(
                    x.Data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct)),
            BuildComparisonYearByYear(
                schoolResponse.EngMaths59YearByYear,
                similarSchools.Select(x => SeriesFrom(
                    x.Data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentPerformance?.EngMaths59_Tot_Est_Previous2_Pct))),
            BuildComparisonAverage(
                schoolResponse.DestinationsThreeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    x.Data?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct))),
            BuildTopPerformers(
                similarSchools,
                x => MeasureValue(
                    x.Data?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct)),
            BuildComparisonYearByYear(
                schoolResponse.DestinationsYearByYear,
                similarSchools.Select(x => SeriesFrom(
                    x.Data?.EstablishmentDestinations?.AllDest_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentDestinations?.AllDest_Tot_Est_Previous2_Pct))),
            BuildComparisonAverage(
                schoolResponse.DestinationsEducationThreeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    x.Data?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct))),
            BuildTopPerformers(
                similarSchools,
                x => MeasureValue(
                    x.Data?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct)),
            BuildComparisonYearByYear(
                schoolResponse.DestinationsEducationYearByYear,
                similarSchools.Select(x => SeriesFrom(
                    x.Data?.EstablishmentDestinations?.Education_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentDestinations?.Education_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentDestinations?.Education_Tot_Est_Previous2_Pct))),
            BuildComparisonAverage(
                schoolResponse.DestinationsEmploymentThreeYearAverage,
                similarSchools.Select(x => MeasureValue(
                    x.Data?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct))),
            BuildTopPerformers(
                similarSchools,
                x => MeasureValue(
                    x.Data?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct)),
            BuildComparisonYearByYear(
                schoolResponse.DestinationsEmploymentYearByYear,
                similarSchools.Select(x => SeriesFrom(
                    x.Data?.EstablishmentDestinations?.Employment_Tot_Est_Current_Pct,
                    x.Data?.EstablishmentDestinations?.Employment_Tot_Est_Previous_Pct,
                    x.Data?.EstablishmentDestinations?.Employment_Tot_Est_Previous2_Pct))));
    }

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
        Ks4HeadlineMeasuresResponseFactory.Average(
            Ks4HeadlineMeasuresResponseFactory.ParseNullableDecimal(current),
            Ks4HeadlineMeasuresResponseFactory.ParseNullableDecimal(previous),
            Ks4HeadlineMeasuresResponseFactory.ParseNullableDecimal(previous2));

    private static Ks4HeadlineMeasureSeries SeriesFrom(string? current, string? previous, string? previous2) =>
        new(
            Ks4HeadlineMeasuresResponseFactory.ParseNullableDecimal(current),
            Ks4HeadlineMeasuresResponseFactory.ParseNullableDecimal(previous),
            Ks4HeadlineMeasuresResponseFactory.ParseNullableDecimal(previous2));

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
        IEnumerable<SimilarSchoolMeasure> similarSchoolResponses,
        Func<SimilarSchoolMeasure, decimal?> selector) =>
        similarSchoolResponses
            .Select(response => new
            {
                response.Urn,
                response.Name,
                Value = selector(response)
            })
            .Where(x => x.Value.HasValue)
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select((x, index) => new Ks4TopPerformer(index + 1, x.Urn, x.Name, x.Value))
            .ToList()
            .AsReadOnly();
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
    decimal? Value);

public record SchoolKs4ComparisonYearByYear(
    Ks4HeadlineMeasureSeries School,
    Ks4HeadlineMeasureSeries SimilarSchools,
    Ks4HeadlineMeasureSeries LocalAuthority,
    Ks4HeadlineMeasureSeries England);

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

internal sealed record SimilarSchoolMeasure(string Urn, string Name, Ks4HeadlineMeasuresData? Data);
