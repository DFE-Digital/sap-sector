using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.Ks4CoreSubjects.UseCases;

public class GetSchoolKs4EnglishLanguage(
    IKs4PerformanceRepository repository,
    ISchoolDetailsService schoolDetailsService,
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository)
{
    public async Task<GetSchoolKs4EnglishLanguageResponse> Execute(GetSchoolKs4EnglishLanguageRequest request)
    {
        var schoolDetails = await schoolDetailsService.GetByUrnAsync(request.Urn);
        var schoolData = await repository.GetByUrnAsync(request.Urn);
        var similarSchoolUrns = await similarSchoolsRepository.GetSimilarSchoolUrnsAsync(request.Urn);
        var similarSchoolData = ((await repository.GetByUrnsAsync(similarSchoolUrns))
                ?? Array.Empty<Ks4HeadlineMeasuresByUrn>())
            .ToDictionary(x => x.Urn, x => x.Data, StringComparer.Ordinal);
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
            schoolDetails,
            similarSchools.Length,
            BuildSelection(
                schoolData,
                similarSchools,
                x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Previous2_Pct),
            BuildSelection(
                schoolData,
                similarSchools,
                x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Previous2_Pct),
            BuildSelection(
                schoolData,
                similarSchools,
                x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Current_Pct,
                x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous_Pct,
                x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous2_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Current_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous_Pct,
                x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous2_Pct,
                x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Current_Pct,
                x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Previous_Pct,
                x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Previous2_Pct));
    }

    private static SchoolKs4EnglishLanguageSelection BuildSelection(
        Ks4HeadlineMeasuresData? schoolData,
        IEnumerable<SimilarSchoolMeasure> similarSchools,
        Func<Ks4HeadlineMeasuresData?, string?> schoolCurrent,
        Func<Ks4HeadlineMeasuresData?, string?> schoolPrevious,
        Func<Ks4HeadlineMeasuresData?, string?> schoolPrevious2,
        Func<Ks4HeadlineMeasuresData?, string?> localAuthorityCurrent,
        Func<Ks4HeadlineMeasuresData?, string?> localAuthorityPrevious,
        Func<Ks4HeadlineMeasuresData?, string?> localAuthorityPrevious2,
        Func<Ks4HeadlineMeasuresData?, string?> englandCurrent,
        Func<Ks4HeadlineMeasuresData?, string?> englandPrevious,
        Func<Ks4HeadlineMeasuresData?, string?> englandPrevious2)
    {
        var schoolAverage = BuildAverage(
            schoolCurrent(schoolData),
            schoolPrevious(schoolData),
            schoolPrevious2(schoolData),
            localAuthorityCurrent(schoolData),
            localAuthorityPrevious(schoolData),
            localAuthorityPrevious2(schoolData),
            englandCurrent(schoolData),
            englandPrevious(schoolData),
            englandPrevious2(schoolData));
        var schoolYearByYear = BuildYearByYear(
            schoolCurrent(schoolData),
            schoolPrevious(schoolData),
            schoolPrevious2(schoolData),
            localAuthorityCurrent(schoolData),
            localAuthorityPrevious(schoolData),
            localAuthorityPrevious2(schoolData),
            englandCurrent(schoolData),
            englandPrevious(schoolData),
            englandPrevious2(schoolData));

        return new(
            new SchoolKs4ComparisonAverage(
                schoolAverage.SchoolValue,
                Average(similarSchools.Select(x => MeasureValue(
                    schoolCurrent(x.Data),
                    schoolPrevious(x.Data),
                    schoolPrevious2(x.Data)))),
                schoolAverage.LocalAuthorityValue,
                schoolAverage.EnglandValue),
            BuildTopPerformers(
                similarSchools,
                x => MeasureValue(
                    schoolCurrent(x.Data),
                    schoolPrevious(x.Data),
                    schoolPrevious2(x.Data))),
            BuildComparisonYearByYear(
                schoolYearByYear,
                similarSchools.Select(x => SeriesFrom(
                    schoolCurrent(x.Data),
                    schoolPrevious(x.Data),
                    schoolPrevious2(x.Data)))));
    }

    private static Ks4HeadlineMeasureAverage BuildAverage(
        string? schoolCurrent,
        string? schoolPrevious,
        string? schoolPrevious2,
        string? localAuthorityCurrent,
        string? localAuthorityPrevious,
        string? localAuthorityPrevious2,
        string? englandCurrent,
        string? englandPrevious,
        string? englandPrevious2) =>
        Ks4HeadlineMeasuresCalculator.BuildAverage(
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(schoolCurrent),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(schoolPrevious),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(schoolPrevious2),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(localAuthorityCurrent),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(localAuthorityPrevious),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(localAuthorityPrevious2),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(englandCurrent),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(englandPrevious),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(englandPrevious2));

    private static Ks4HeadlineMeasureYearByYear BuildYearByYear(
        string? schoolCurrent,
        string? schoolPrevious,
        string? schoolPrevious2,
        string? localAuthorityCurrent,
        string? localAuthorityPrevious,
        string? localAuthorityPrevious2,
        string? englandCurrent,
        string? englandPrevious,
        string? englandPrevious2) =>
        Ks4HeadlineMeasuresCalculator.BuildYearByYear(
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(schoolCurrent),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(schoolPrevious),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(schoolPrevious2),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(localAuthorityCurrent),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(localAuthorityPrevious),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(localAuthorityPrevious2),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(englandCurrent),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(englandPrevious),
            Ks4HeadlineMeasuresCalculator.ParseNullableDecimal(englandPrevious2));

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

public record GetSchoolKs4EnglishLanguageRequest(string Urn);

public enum SchoolKs4EnglishLanguageGradeFilter
{
    Grade4,
    Grade5,
    Grade7
}

public record SchoolKs4EnglishLanguageSelection(
    SchoolKs4ComparisonAverage ThreeYearAverage,
    IReadOnlyList<Ks4TopPerformer> TopPerformers,
    SchoolKs4ComparisonYearByYear YearByYear)
{
    public static SchoolKs4EnglishLanguageGradeFilter ParseFilter(string? grade) =>
        grade switch
        {
            "5" => SchoolKs4EnglishLanguageGradeFilter.Grade5,
            "7" => SchoolKs4EnglishLanguageGradeFilter.Grade7,
            _ => SchoolKs4EnglishLanguageGradeFilter.Grade4
        };

    public static string ToFilterValue(SchoolKs4EnglishLanguageGradeFilter filter) =>
        filter switch
        {
            SchoolKs4EnglishLanguageGradeFilter.Grade5 => "5",
            SchoolKs4EnglishLanguageGradeFilter.Grade7 => "7",
            _ => "4"
        };

    public static SchoolKs4EnglishLanguageSelection From(
        GetSchoolKs4EnglishLanguageResponse response,
        SchoolKs4EnglishLanguageGradeFilter filter) =>
        filter switch
        {
            SchoolKs4EnglishLanguageGradeFilter.Grade5 => response.Grade5AndAbove,
            SchoolKs4EnglishLanguageGradeFilter.Grade7 => response.Grade7AndAbove,
            _ => response.Grade4AndAbove
        };
}

public record GetSchoolKs4EnglishLanguageResponse(
    SchoolDetails SchoolDetails,
    int SimilarSchoolsCount,
    SchoolKs4EnglishLanguageSelection Grade4AndAbove,
    SchoolKs4EnglishLanguageSelection Grade5AndAbove,
    SchoolKs4EnglishLanguageSelection Grade7AndAbove);
