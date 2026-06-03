namespace SAPSec.Web.ViewModels.Measures;

public record TopPerformersSubMeasureViewModel(
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<TopPerformersSubMeasureItemViewModel> TopPerformers,
    string SimilarSchoolsLink)
    : SubMeasureViewModel(MeasureInfo)
{
    public override string Id => "top-performers";
    public override string Name => "Top performers";
}

public record TopPerformersSubMeasureItemViewModel(
    int Rank,
    string Urn,
    string Name,
    string Link,
    decimal? Value,
    bool IsCurrentSchool);
