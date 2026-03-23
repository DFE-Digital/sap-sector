using System.Globalization;
using SAPSec.Core.Model;
using SAPSec.Web.Helpers;

namespace SAPSec.Web.ViewModels;

public class Ks4HeadlineMeasuresPageViewModel
{
    public required SchoolDetails SchoolDetails { get; set; }
    public required int SimilarSchoolsCount { get; set; }

    public required decimal? SchoolAttainment8ThreeYearAverage { get; set; }
    public required decimal? SimilarSchoolsAttainment8ThreeYearAverage { get; set; }
    public required decimal? LocalAuthorityAttainment8ThreeYearAverage { get; set; }
    public required decimal? EnglandAttainment8ThreeYearAverage { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SchoolAttainment8YearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SimilarSchoolsAttainment8YearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries LocalAuthorityAttainment8YearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries EnglandAttainment8YearByYear { get; set; }

    public required decimal? SchoolEngMaths49ThreeYearAverage { get; set; }
    public required decimal? SimilarSchoolsEngMaths49ThreeYearAverage { get; set; }
    public required decimal? LocalAuthorityEngMaths49ThreeYearAverage { get; set; }
    public required decimal? EnglandEngMaths49ThreeYearAverage { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SchoolEngMaths49YearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SimilarSchoolsEngMaths49YearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries LocalAuthorityEngMaths49YearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries EnglandEngMaths49YearByYear { get; set; }

    public required decimal? SchoolEngMaths59ThreeYearAverage { get; set; }
    public required decimal? SimilarSchoolsEngMaths59ThreeYearAverage { get; set; }
    public required decimal? LocalAuthorityEngMaths59ThreeYearAverage { get; set; }
    public required decimal? EnglandEngMaths59ThreeYearAverage { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SchoolEngMaths59YearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SimilarSchoolsEngMaths59YearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries LocalAuthorityEngMaths59YearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries EnglandEngMaths59YearByYear { get; set; }

    public required decimal? SchoolDestinationsThreeYearAverage { get; set; }
    public required decimal? SimilarSchoolsDestinationsThreeYearAverage { get; set; }
    public required decimal? LocalAuthorityDestinationsThreeYearAverage { get; set; }
    public required decimal? EnglandDestinationsThreeYearAverage { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SchoolDestinationsYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SimilarSchoolsDestinationsYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries LocalAuthorityDestinationsYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries EnglandDestinationsYearByYear { get; set; }

    public required decimal? SchoolDestinationsEducationThreeYearAverage { get; set; }
    public required decimal? SimilarSchoolsDestinationsEducationThreeYearAverage { get; set; }
    public required decimal? LocalAuthorityDestinationsEducationThreeYearAverage { get; set; }
    public required decimal? EnglandDestinationsEducationThreeYearAverage { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SchoolDestinationsEducationYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SimilarSchoolsDestinationsEducationYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries LocalAuthorityDestinationsEducationYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries EnglandDestinationsEducationYearByYear { get; set; }

    public required decimal? SchoolDestinationsEmploymentThreeYearAverage { get; set; }
    public required decimal? SimilarSchoolsDestinationsEmploymentThreeYearAverage { get; set; }
    public required decimal? LocalAuthorityDestinationsEmploymentThreeYearAverage { get; set; }
    public required decimal? EnglandDestinationsEmploymentThreeYearAverage { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SchoolDestinationsEmploymentYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries SimilarSchoolsDestinationsEmploymentYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries LocalAuthorityDestinationsEmploymentYearByYear { get; set; }
    public required SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4HeadlineMeasureSeries EnglandDestinationsEmploymentYearByYear { get; set; }

    public string SchoolAttainment8Display => DisplayValue(SchoolAttainment8ThreeYearAverage);
    public string SimilarSchoolsAttainment8Display => DisplayValue(SimilarSchoolsAttainment8ThreeYearAverage);
    public string LocalAuthorityAttainment8Display => DisplayValue(LocalAuthorityAttainment8ThreeYearAverage);
    public string EnglandAttainment8Display => DisplayValue(EnglandAttainment8ThreeYearAverage);

    public string SchoolEngMaths49Display => DisplayPercent(SchoolEngMaths49ThreeYearAverage);
    public string SimilarSchoolsEngMaths49Display => DisplayPercent(SimilarSchoolsEngMaths49ThreeYearAverage);
    public string LocalAuthorityEngMaths49Display => DisplayPercent(LocalAuthorityEngMaths49ThreeYearAverage);
    public string EnglandEngMaths49Display => DisplayPercent(EnglandEngMaths49ThreeYearAverage);

    public string SchoolEngMaths59Display => DisplayPercent(SchoolEngMaths59ThreeYearAverage);
    public string SimilarSchoolsEngMaths59Display => DisplayPercent(SimilarSchoolsEngMaths59ThreeYearAverage);
    public string LocalAuthorityEngMaths59Display => DisplayPercent(LocalAuthorityEngMaths59ThreeYearAverage);
    public string EnglandEngMaths59Display => DisplayPercent(EnglandEngMaths59ThreeYearAverage);

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

    public static string DisplayPercent(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.0", CultureInfo.InvariantCulture) + "%" : "No available data";

    public static string DisplayWholePercent(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture) + "%"
            : "No available data";
}
