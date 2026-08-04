using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Secondary.ViewModels;

public class Ks4HeadlineMeasuresPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }

    public required MeasureViewModel Attainment8 { get; set; }
    public required MeasureViewModel EnglishMaths { get; set; }
    public required MeasureViewModel Destinations { get; set; }
}
