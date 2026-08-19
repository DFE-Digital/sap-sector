using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Secondary.ViewModels;

public class ComparisonKs4HeadlineMeasuresPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }
    public required SchoolInfoViewModel SimilarSchool { get; set; }

    public required MeasureViewModel Attainment8 { get; set; }
    public required MeasureViewModel EnglishMaths { get; set; }
    public required MeasureViewModel Destinations { get; set; }
}
