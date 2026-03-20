using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public class GetSchoolKs4HeadlineMeasures(
    GetKs4HeadlineMeasures getKs4HeadlineMeasures,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository)
{
    public async Task<GetSchoolKs4HeadlineMeasuresResponse> Execute(GetSchoolKs4HeadlineMeasuresRequest request)
    {
        var schoolResponse = await getKs4HeadlineMeasures.Execute(new GetKs4HeadlineMeasuresRequest(request.Urn));
        var similarSchoolUrns = await similarSchoolsRepository.GetSimilarSchoolUrnsAsync(request.Urn);

        var similarSchoolResponses = (await Task.WhenAll(
                similarSchoolUrns.Select(async urn =>
                {
                    try
                    {
                        return await getKs4HeadlineMeasures.Execute(new GetKs4HeadlineMeasuresRequest(urn));
                    }
                    catch (NotFoundException)
                    {
                        return null;
                    }
                })))
            .Where(response => response is not null)
            .Cast<GetKs4HeadlineMeasuresResponse>()
            .ToArray();

        return new(
            schoolResponse.SchoolDetails,
            similarSchoolResponses.Length,
            BuildComparisonAverage(
                schoolResponse.Attainment8ThreeYearAverage,
                similarSchoolResponses.Select(x => x.Attainment8ThreeYearAverage.SchoolValue)),
            BuildComparisonYearByYear(
                schoolResponse.Attainment8YearByYear,
                similarSchoolResponses.Select(x => x.Attainment8YearByYear.School)),
            BuildComparisonAverage(
                schoolResponse.EngMaths49ThreeYearAverage,
                similarSchoolResponses.Select(x => x.EngMaths49ThreeYearAverage.SchoolValue)),
            BuildComparisonYearByYear(
                schoolResponse.EngMaths49YearByYear,
                similarSchoolResponses.Select(x => x.EngMaths49YearByYear.School)),
            BuildComparisonAverage(
                schoolResponse.EngMaths59ThreeYearAverage,
                similarSchoolResponses.Select(x => x.EngMaths59ThreeYearAverage.SchoolValue)),
            BuildComparisonYearByYear(
                schoolResponse.EngMaths59YearByYear,
                similarSchoolResponses.Select(x => x.EngMaths59YearByYear.School)),
            BuildComparisonAverage(
                schoolResponse.DestinationsThreeYearAverage,
                similarSchoolResponses.Select(x => x.DestinationsThreeYearAverage.SchoolValue)),
            BuildComparisonYearByYear(
                schoolResponse.DestinationsYearByYear,
                similarSchoolResponses.Select(x => x.DestinationsYearByYear.School)),
            BuildComparisonAverage(
                schoolResponse.DestinationsEducationThreeYearAverage,
                similarSchoolResponses.Select(x => x.DestinationsEducationThreeYearAverage.SchoolValue)),
            BuildComparisonYearByYear(
                schoolResponse.DestinationsEducationYearByYear,
                similarSchoolResponses.Select(x => x.DestinationsEducationYearByYear.School)),
            BuildComparisonAverage(
                schoolResponse.DestinationsEmploymentThreeYearAverage,
                similarSchoolResponses.Select(x => x.DestinationsEmploymentThreeYearAverage.SchoolValue)),
            BuildComparisonYearByYear(
                schoolResponse.DestinationsEmploymentYearByYear,
                similarSchoolResponses.Select(x => x.DestinationsEmploymentYearByYear.School)));
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
}

public record GetSchoolKs4HeadlineMeasuresRequest(string Urn);

public record SchoolKs4ComparisonAverage(
    decimal? SchoolValue,
    decimal? SimilarSchoolsValue,
    decimal? LocalAuthorityValue,
    decimal? EnglandValue);

public record SchoolKs4ComparisonYearByYear(
    Ks4HeadlineMeasureSeries School,
    Ks4HeadlineMeasureSeries SimilarSchools,
    Ks4HeadlineMeasureSeries LocalAuthority,
    Ks4HeadlineMeasureSeries England);

public record GetSchoolKs4HeadlineMeasuresResponse(
    SchoolDetails SchoolDetails,
    int SimilarSchoolsCount,
    SchoolKs4ComparisonAverage Attainment8ThreeYearAverage,
    SchoolKs4ComparisonYearByYear Attainment8YearByYear,
    SchoolKs4ComparisonAverage EngMaths49ThreeYearAverage,
    SchoolKs4ComparisonYearByYear EngMaths49YearByYear,
    SchoolKs4ComparisonAverage EngMaths59ThreeYearAverage,
    SchoolKs4ComparisonYearByYear EngMaths59YearByYear,
    SchoolKs4ComparisonAverage DestinationsThreeYearAverage,
    SchoolKs4ComparisonYearByYear DestinationsYearByYear,
    SchoolKs4ComparisonAverage DestinationsEducationThreeYearAverage,
    SchoolKs4ComparisonYearByYear DestinationsEducationYearByYear,
    SchoolKs4ComparisonAverage DestinationsEmploymentThreeYearAverage,
    SchoolKs4ComparisonYearByYear DestinationsEmploymentYearByYear);
