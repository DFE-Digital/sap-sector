using System.Globalization;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;

public class Ks4CoreSubjectsPageViewModel
{
    public record TopPerformerRow(int Rank, string Urn, string Name, decimal? Value, string DisplayValue);

    public required SchoolDetails SchoolDetails { get; set; }
    public required int SimilarSchoolsCount { get; set; }

    public required decimal? SchoolEnglishLanguageThreeYearAverage { get; set; }
    public required decimal? SimilarSchoolsEnglishLanguageThreeYearAverage { get; set; }
    public required decimal? LocalAuthorityEnglishLanguageThreeYearAverage { get; set; }
    public required decimal? EnglandEnglishLanguageThreeYearAverage { get; set; }
    public required IReadOnlyList<TopPerformerRow> EnglishLanguageTopPerformers { get; set; }
    public required Ks4HeadlineMeasureSeries SchoolEnglishLanguageYearByYear { get; set; }
    public required Ks4HeadlineMeasureSeries SimilarSchoolsEnglishLanguageYearByYear { get; set; }
    public required Ks4HeadlineMeasureSeries LocalAuthorityEnglishLanguageYearByYear { get; set; }
    public required Ks4HeadlineMeasureSeries EnglandEnglishLanguageYearByYear { get; set; }

    public string SchoolEnglishLanguageDisplay => DisplayPercent(SchoolEnglishLanguageThreeYearAverage);
    public string SimilarSchoolsEnglishLanguageDisplay => DisplayPercent(SimilarSchoolsEnglishLanguageThreeYearAverage);
    public string LocalAuthorityEnglishLanguageDisplay => DisplayPercent(LocalAuthorityEnglishLanguageThreeYearAverage);
    public string EnglandEnglishLanguageDisplay => DisplayPercent(EnglandEnglishLanguageThreeYearAverage);

    public string SchoolLabel => SchoolDetails.Name;
    public string SimilarSchoolsLabel => "Similar schools average";
    public string LocalAuthorityLabel => "Local authority schools average";
    public string EnglandLabel => "Schools in England average";

    public static string DisplayPercent(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%" : "No available data";
}
