using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Shared.ViewModels.School;

public class Ks2PerformanceMeasuresPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }
    public required string WhatIsASimilarSchoolUrl { get; set; }
    public required string SimilarSchoolDefinitionLinkText { get; set; }

    /// <summary>
    /// KS2 progress scores. Null when <c>EnableKs2ProgressMeasures</c> is off, in which case the page shows
    /// the 'no progress scores' message instead.
    /// </summary>
    public MeasureViewModel? ProgressScores { get; set; }

    public required MeasureViewModel MeetingExpectedStandardRwm { get; set; }
    public required MeasureViewModel AchievedHigherStandardRwm { get; set; }
    public required MeasureViewModel AverageScaledScoreReading { get; set; }
    public required MeasureViewModel AverageScaledScoreMaths { get; set; }
    public required MeasureViewModel MeetingExpectedStandardGps { get; set; }
    public required MeasureViewModel AchievedHigherStandardGps { get; set; }
}
