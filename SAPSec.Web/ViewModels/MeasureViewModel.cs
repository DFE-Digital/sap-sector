using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using System.Globalization;

namespace SAPSec.Web.ViewModels;

public record MeasureViewModel(
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<SubMeasureViewModel> SubMeasures);

public record MeasureInfoViewModel(
    string HtmlPrefix,
    string Name,
    MeasureDataType DataType,
    IEnumerable<MeasureAvailableFilterViewModel> Filters,
    IEnumerable<string> Labels);

public record MeasureAvailableFilterViewModel(
    string Key,
    string Name,
    IEnumerable<MeasureFilterOptionViewModel> Options);

public record MeasureFilterOptionViewModel(
    string Key,
    string Name,
    int Count,
    bool Selected);

public abstract record SubMeasureViewModel(
    string Id,
    string Name,
    MeasureInfoViewModel MeasureInfo)
{
    public string DisplayNumber(decimal? value) =>
        MeasureInfo.DataType == MeasureDataType.Number
            ? DisplayValue(value)
            : DisplayWholePercent(value);

    private static string DisplayValue(decimal? value) =>
        value.HasValue
            ? value.Value.ToString("0.0", CultureInfo.InvariantCulture)
            : "No available data";

    private static string DisplayWholePercent(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) + "%"
            : "No available data";
}

public record ThreeYearAverageSubMeasureViewModel(
    string Id,
    string Name,
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<decimal?> Averages,
    IEnumerable<string>? Colors = null) : SubMeasureViewModel(Id, Name, MeasureInfo)
{
}

public record TopPerformersSubMeasureViewModel(
    string Id,
    string Name,
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<TopPerformersSubMeasureItemViewModel> TopPerformers,
    string SimilarSchoolsLink)
    : SubMeasureViewModel(Id, Name, MeasureInfo)
{
}

public record TopPerformersSubMeasureItemViewModel(
    int Rank,
    string Urn,
    string Name,
    string Link,
    decimal? Value,
    bool IsCurrentSchool);

public record YearByYearSubMeasureViewModel(
    string Id,
    string Name,
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<YearByYearSeriesViewModel> Series,
    IEnumerable<string>? Colors = null) : SubMeasureViewModel(Id, Name, MeasureInfo)
{
}

public record YearByYearSeriesViewModel(
    decimal? Current,
    decimal? Previous,
    decimal? Previous2);

public record TableSubMeasureViewModel(
    string Id,
    string Name,
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<TableSubMeasureRowViewModel> Rows) : SubMeasureViewModel(Id, Name, MeasureInfo)
{
}

public record TableSubMeasureRowViewModel(
    string Key,
    YearByYearSeriesViewModel YearByYear,
    decimal? ThreeYearAverage);