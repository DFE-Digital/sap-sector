namespace SAPSec.Web.ViewModels.Measures;

public record TopPerformersViewModel(
    MeasureInfoViewModel MeasureInfo,
    IEnumerable<TopPerformerViewModel> TopPerformers,
    string SimilarSchoolsLink)
    : MeasureBreakdownViewModel(MeasureInfo);

public record TopPerformerViewModel(
    int Rank,
    string Urn,
    string Name,
    string Link,
    decimal? Value,
    bool IsCurrentSchool);
