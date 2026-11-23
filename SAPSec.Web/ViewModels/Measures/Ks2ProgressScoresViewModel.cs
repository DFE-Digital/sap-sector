using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.SchoolInfo;

namespace SAPSec.Web.ViewModels.Measures;

/// <summary>
/// Adds the 'Number of pupils' tables to the KS2 progress scores measure.
/// </summary>
public static class Ks2ProgressScoresViewModel
{
    /// <summary>
    /// School page: the user's school under the charts and table, and the top performing similar schools
    /// (in rank order) under the top performers table.
    /// </summary>
    public static MeasureViewModel WithNumberOfPupils(
        MeasureViewModel measureViewModel,
        Measure measure,
        SchoolInfo school,
        IReadOnlyDictionary<string, SchoolPupilNumbers> pupilNumbers)
    {
        var topPerformers = measureViewModel.TopPerformers is null || measure.TopPerformers is null
            ? null
            : measure.TopPerformers
                .OrderBy(t => t.Rank)
                .Select(t => (t.Name, pupilNumbers.GetValueOrDefault(t.Urn)))
                .ToList();

        return measureViewModel with
        {
            NumberOfPupils = MeasureNumberOfPupilsViewModel.Create(
                measureViewModel.MeasureInfo.HtmlPrefix,
                measure.Year,
                [(school.Name, pupilNumbers.GetValueOrDefault(school.Urn))],
                topPerformers)
        };
    }

    /// <summary>
    /// Comparing to page: the user's school and the selected similar school. There is no top performers tab.
    /// </summary>
    public static MeasureViewModel WithNumberOfPupils(
        MeasureViewModel measureViewModel,
        Measure measure,
        SchoolInfo currentSchool,
        SchoolPupilNumbers currentSchoolNumbers,
        SchoolInfo comparatorSchool,
        SchoolPupilNumbers comparatorSchoolNumbers) =>
        measureViewModel with
        {
            NumberOfPupils = MeasureNumberOfPupilsViewModel.Create(
                measureViewModel.MeasureInfo.HtmlPrefix,
                measure.Year,
                [(currentSchool.Name, currentSchoolNumbers), (comparatorSchool.Name, comparatorSchoolNumbers)],
                null)
        };
}
