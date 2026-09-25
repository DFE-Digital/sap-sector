using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Shared.ViewModels.School;

public class Ks4CoreSubjectsPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }
    public required string WhatIsASimilarSchoolUrl { get; set; }
    public required string SimilarSchoolDefinitionLinkText { get; set; }

    public required MeasureViewModel[] Measures { get; set; }
}
