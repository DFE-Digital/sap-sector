using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.SchoolDetails.Comparison;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Primary.ViewModels.Comparison;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels.Comparison;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.Formatters;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Primary.Controllers;

[Area("Primary")]
[Route("school/primary/{urn}/view-similar-schools/{comparatorSchoolUrn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Primary, "urn", "comparatorSchoolUrn")]
public class ComparisonController(
    IUseCase<GetPrimaryComparisonSimilarityCharacteristicsRequest, GetPrimaryComparisonSimilarityCharacteristicsResponse> getSimilarityCharacteristicsUseCase,
    IUseCase<GetComparisonKs2PerformanceMeasuresRequest, GetComparisonKs2PerformanceMeasuresResponse> getKs2PerformanceMeasuresUseCase,
    IUseCase<GetPrimaryComparisonAttendanceMeasuresRequest, GetComparisonAttendanceMeasuresResponse> getAttendanceMeasuresUseCase,
    [FromKeyedServices(ServiceKeys.Primary)]
    IUseCase<GetComparisonSchoolDetailsRequest, GetComparisonSchoolDetailsResponse> getSchoolDetailsUseCase,
    IPrimaryCharacteristicsComparisonFormatter characteristicsComparisonFormatter)
    : Controller
{
    [HttpGet]
    [Route("compare-similarity")]
    public async Task<IActionResult> Similarity(string urn, string comparatorSchoolUrn)
    {
        var response = await getSimilarityCharacteristicsUseCase.Execute(new(urn, comparatorSchoolUrn));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(
            response.CurrentSchool,
            response.ComparatorSchool);

        var model = new SimilarityPageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            ComparatorSchool = SchoolInfoViewModel.FromSchoolInfo(response.ComparatorSchool),
            CharacteristicsRows = characteristicsComparisonFormatter.BuildRows(response.SimilarityCharacteristics)
        };

        return View(model);
    }

    [HttpGet]
    [Route("compare-ks2")]
    public async Task<IActionResult> Ks2PerformanceMeasures(string urn, string comparatorSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getKs2PerformanceMeasuresUseCase.Execute(new(urn, comparatorSchoolUrn, filters));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(
            response.CurrentSchool,
            response.ComparatorSchool);

        var model = new Ks2PerformanceMeasuresPageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            ComparatorSchool = SchoolInfoViewModel.FromSchoolInfo(response.ComparatorSchool),
            MeetingExpectedStandardRwm = MeasureViewModel.FromPrimaryComparisonMeasure(response.MeetingExpectedStandardRwm, response.CurrentSchool, response.ComparatorSchool),
            AchievedHigherStandardRwm = MeasureViewModel.FromPrimaryComparisonMeasure(response.AchievedHigherStandardRwm, response.CurrentSchool, response.ComparatorSchool),
            AverageScaledScoreReading = MeasureViewModel.FromPrimaryComparisonMeasure(response.AverageScaledScoreReading, response.CurrentSchool, response.ComparatorSchool),
            AverageScaledScoreMaths = MeasureViewModel.FromPrimaryComparisonMeasure(response.AverageScaledScoreMaths, response.CurrentSchool, response.ComparatorSchool),
            MeetingExpectedStandardGps = MeasureViewModel.FromPrimaryComparisonMeasure(response.MeetingExpectedStandardGps, response.CurrentSchool, response.ComparatorSchool),
            AchievedHigherStandardGps = MeasureViewModel.FromPrimaryComparisonMeasure(response.AchievedHigherStandardGps, response.CurrentSchool, response.ComparatorSchool),
        };

        return View(model);
    }

    [HttpGet]
    [Route("compare-attendance")]
    public async Task<IActionResult> Attendance(string urn, string comparatorSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getAttendanceMeasuresUseCase.Execute(new(urn, comparatorSchoolUrn, filters));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(
            response.CurrentSchool,
            response.ComparatorSchool);

        var model = new AttendancePageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            ComparatorSchool = SchoolInfoViewModel.FromSchoolInfo(response.ComparatorSchool),
            Absence = MeasureViewModel.FromPrimaryComparisonMeasure(response.Absence, response.CurrentSchool, response.ComparatorSchool)
        };

        return View(model);
    }

    [HttpGet]
    [Route("compare-school-details")]
    public async Task<IActionResult> SchoolDetails(string urn, string comparatorSchoolUrn)
    {
        var response = await getSchoolDetailsUseCase.Execute(new(urn, comparatorSchoolUrn));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(
            response.CurrentSchool.School,
            response.ComparatorSchool.School);

        var model = new SchoolDetailsPageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool.School),
            ComparatorSchool = SchoolInfoViewModel.FromSchoolInfo(response.ComparatorSchool.School),
            CurrentSchoolLatitude = response.CurrentSchool.Coordinates?.Latitude,
            CurrentSchoolLongitude = response.CurrentSchool.Coordinates?.Longitude,
            ComparatorSchoolLatitude = response.ComparatorSchool.Coordinates?.Latitude,
            ComparatorSchoolLongitude = response.ComparatorSchool.Coordinates?.Longitude,
            Distance = response.DistanceMiles,
            ComparatorSchoolDetails = SchoolDetailsViewModel.FromSchoolDetails(response.ComparatorSchoolDetails)
        };

        return View(model);
    }
}
