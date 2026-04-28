using System.Globalization;
using SAPSec.Core.Model;
using SAPSec.Web.Helpers;

namespace SAPSec.Web.ViewModels;

public class Ks4HeadlineMeasuresPageViewModel
{
    public record TopPerformerRow(int Rank, string Urn, string Name, decimal? Value, string DisplayValue, bool IsCurrentSchool);

    public required SchoolDetails SchoolDetails { get; set; }
    public required int SimilarSchoolsCount { get; set; }

    public required decimal? SchoolAttainment8ThreeYearAverage { get; set; }
    public required decimal? SimilarSchoolsAttainment8ThreeYearAverage { get; set; }
    public required decimal? LocalAuthorityAttainment8ThreeYearAverage { get; set; }
    public required decimal? EnglandAttainment8ThreeYearAverage { get; set; }
    public required IReadOnlyList<TopPerformerRow> Attainment8TopPerformers { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SchoolAttainment8YearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SimilarSchoolsAttainment8YearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries LocalAuthorityAttainment8YearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries EnglandAttainment8YearByYear { get; set; }

    public required decimal? SchoolEngMathsThreeYearAverage { get; set; }
    public required decimal? SimilarSchoolsEngMathsThreeYearAverage { get; set; }
    public required decimal? LocalAuthorityEngMathsThreeYearAverage { get; set; }
    public required decimal? EnglandEngMathsThreeYearAverage { get; set; }
    public required IReadOnlyList<TopPerformerRow> EngMathsTopPerformers { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SchoolEngMathsYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SimilarSchoolsEngMathsYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries LocalAuthorityEngMathsYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries EnglandEngMathsYearByYear { get; set; }

    public required decimal? SchoolDestinationsThreeYearAverage { get; set; }
    public required decimal? SimilarSchoolsDestinationsThreeYearAverage { get; set; }
    public required decimal? LocalAuthorityDestinationsThreeYearAverage { get; set; }
    public required decimal? EnglandDestinationsThreeYearAverage { get; set; }
    public required IReadOnlyList<TopPerformerRow> DestinationsTopPerformers { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SchoolDestinationsYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SimilarSchoolsDestinationsYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries LocalAuthorityDestinationsYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries EnglandDestinationsYearByYear { get; set; }

    public string SchoolAttainment8Display => DisplayValue(SchoolAttainment8ThreeYearAverage);
    public string SimilarSchoolsAttainment8Display => DisplayValue(SimilarSchoolsAttainment8ThreeYearAverage);
    public string LocalAuthorityAttainment8Display => DisplayValue(LocalAuthorityAttainment8ThreeYearAverage);
    public string EnglandAttainment8Display => DisplayValue(EnglandAttainment8ThreeYearAverage);

    public string SchoolEngMathsDisplay => DisplayWholePercent(SchoolEngMathsThreeYearAverage);
    public string SimilarSchoolsEngMathsDisplay => DisplayWholePercent(SimilarSchoolsEngMathsThreeYearAverage);
    public string LocalAuthorityEngMathsDisplay => DisplayWholePercent(LocalAuthorityEngMathsThreeYearAverage);
    public string EnglandEngMathsDisplay => DisplayWholePercent(EnglandEngMathsThreeYearAverage);

    public string SchoolDestinationsDisplay => DisplayWholePercent(SchoolDestinationsThreeYearAverage);
    public string SimilarSchoolsDestinationsDisplay => DisplayWholePercent(SimilarSchoolsDestinationsThreeYearAverage);
    public string LocalAuthorityDestinationsDisplay => DisplayWholePercent(LocalAuthorityDestinationsThreeYearAverage);
    public string EnglandDestinationsDisplay => DisplayWholePercent(EnglandDestinationsThreeYearAverage);

    public string SchoolLabel => SchoolDetails.Name;
    public string SimilarSchoolsLabel => $"Similar schools average";
    public string LocalAuthorityLabel => $"Local authority schools average";
    public string EnglandLabel => "Schools in England average";

    public static string DisplayValue(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) : "No available data";

    public static string DisplayWholeValue(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture)
            : "No available data";

    public static string DisplayPercent(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%" : "No available data";

    public static string DisplayWholePercent(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) + "%"
            : "No available data";
}
