using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Shared.ViewModels.School;

public class Ks4HeadlineMeasuresPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }
    public required string WhatIsASimilarSchoolUrl { get; set; }
    public required string SimilarSchoolDefinitionLinkText { get; set; }

    public required MeasureViewModel Attainment8 { get; set; }
    public required MeasureViewModel EnglishMaths { get; set; }
    public required MeasureViewModel Destinations { get; set; }
}
