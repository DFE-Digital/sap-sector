using System.Globalization;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;

public class Ks4CoreSubjectsPageViewModel
{
    public record TopPerformerRow(int Rank, string Urn, string Name, decimal? Value, string DisplayValue);
    public record SubjectSection(
        decimal? SchoolThreeYearAverage,
        decimal? SimilarSchoolsThreeYearAverage,
        decimal? LocalAuthorityThreeYearAverage,
        decimal? EnglandThreeYearAverage,
        IReadOnlyList<TopPerformerRow> TopPerformers,
        Ks4HeadlineMeasureSeries SchoolYearByYear,
        Ks4HeadlineMeasureSeries SimilarSchoolsYearByYear,
        Ks4HeadlineMeasureSeries LocalAuthorityYearByYear,
        Ks4HeadlineMeasureSeries EnglandYearByYear)
    {
        public string SchoolDisplay => DisplayPercent(SchoolThreeYearAverage);
        public string SimilarSchoolsDisplay => DisplayPercent(SimilarSchoolsThreeYearAverage);
        public string LocalAuthorityDisplay => DisplayPercent(LocalAuthorityThreeYearAverage);
        public string EnglandDisplay => DisplayPercent(EnglandThreeYearAverage);
    }

    public required SchoolDetails SchoolDetails { get; set; }
    public required int SimilarSchoolsCount { get; set; }
    public required SubjectSection EnglishLanguage { get; set; }
    public required SubjectSection EnglishLiterature { get; set; }
    public required SubjectSection Maths { get; set; }
    public required SubjectSection CombinedScienceDoubleAward { get; set; }

    public string SchoolLabel => SchoolDetails.Name;
    public string SimilarSchoolsLabel => "Similar schools average";
    public string LocalAuthorityLabel => "Local authority schools average";
    public string EnglandLabel => "Schools in England average";

    public static string DisplayPercent(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%" : "No available data";
}
