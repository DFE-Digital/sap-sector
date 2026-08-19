using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.Primary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Primary.ViewModels;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.Formatters;
using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Primary.Controllers;

[Area("Primary")]
[Route("school/primary/{urn}/view-similar-schools/{similarSchoolUrn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Primary, "urn", "similarSchoolUrn")]
[RequireFeatureFlag(FeatureFlags.EnablePrimarySchools)]
public class SimilarSchoolsComparisonController(
    IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse> getSchoolInfoUseCase,
    IUseCase<GetPrimarySimilarSchoolDetailsRequest, GetPrimarySimilarSchoolDetailsResponse> getPrimarySimilarSchoolDetailsUseCase,
    IUseCase<GetSchoolKs2PerformanceComparisonRequest, GetSchoolKs2PerformanceComparisonResponse> getSchoolKs2PerformanceComparisonUseCase,
    GetPrimaryCharacteristicsComparison getPrimaryCharacteristicsComparison,
    IPrimaryCharacteristicsComparisonFormatter primaryCharacteristicsComparisonFormatter,
    IUseCase<GetSchoolAttendanceComparisonRequest, GetSchoolAttendanceComparisonResponse> getSchoolAttendanceComparisonUseCase)
    : Controller
{
    [HttpGet]
    public Task<IActionResult> Index(string urn, string similarSchoolUrn) =>
        Similarity(urn, similarSchoolUrn);

    [HttpGet]
    [Route("similarity")]
    public async Task<IActionResult> Similarity(string urn, string similarSchoolUrn)
    {
        var (model, _, _) = await BuildBaseModelAsync(urn, similarSchoolUrn);

        var characteristicsResponse = await getPrimaryCharacteristicsComparison.Execute(
            new GetPrimaryCharacteristicsComparisonRequest(urn, similarSchoolUrn));
        model.CharacteristicsRows = primaryCharacteristicsComparisonFormatter.BuildRows(characteristicsResponse);

        ViewData[ViewDataKeys.ComparisonSchool] = model;
        return View("Similarity", model);
    }

    [HttpGet]
    [Route("ks2")]
    public async Task<IActionResult> Ks2PerformanceMeasures(string urn, string similarSchoolUrn)
    {
        var (model, currentSchool, similarSchool) = await BuildBaseModelAsync(urn, similarSchoolUrn);

        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var comparisonResponse = await getSchoolKs2PerformanceComparisonUseCase.Execute(
            new GetSchoolKs2PerformanceComparisonRequest(urn, similarSchoolUrn, filters));

        model.MeetingExpectedStandardRwm = MeasureViewModel.FromPrimaryComparisonMeasure(comparisonResponse.MeetingExpectedStandardRwm, currentSchool, similarSchool);
        model.AchievedHigherStandardRwm = MeasureViewModel.FromPrimaryComparisonMeasure(comparisonResponse.AchievedHigherStandardRwm, currentSchool, similarSchool);
        model.AverageScaledScoreReading = MeasureViewModel.FromPrimaryComparisonMeasure(comparisonResponse.AverageScaledScoreReading, currentSchool, similarSchool);
        model.AverageScaledScoreMaths = MeasureViewModel.FromPrimaryComparisonMeasure(comparisonResponse.AverageScaledScoreMaths, currentSchool, similarSchool);
        model.MeetingExpectedStandardGps = MeasureViewModel.FromPrimaryComparisonMeasure(comparisonResponse.MeetingExpectedStandardGps, currentSchool, similarSchool);
        model.AchievedHigherStandardGps = MeasureViewModel.FromPrimaryComparisonMeasure(comparisonResponse.AchievedHigherStandardGps, currentSchool, similarSchool);

        ViewData[ViewDataKeys.ComparisonSchool] = model;
        return View(model);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn, string similarSchoolUrn)
    {
        var (model, currentSchool, similarSchool) = await BuildBaseModelAsync(urn, similarSchoolUrn);

        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var comparisonResponse = await getSchoolAttendanceComparisonUseCase.Execute(
            new GetSchoolAttendanceComparisonRequest(urn, similarSchoolUrn, filters));

        model.Absence = MeasureViewModel.FromPrimaryComparisonMeasure(comparisonResponse.Absence, currentSchool, similarSchool);

        ViewData[ViewDataKeys.ComparisonSchool] = model;
        return View(model);
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn, string similarSchoolUrn)
    {
        var response = await getPrimarySimilarSchoolDetailsUseCase.Execute(
            new GetPrimarySimilarSchoolDetailsRequest(urn, similarSchoolUrn));

        ViewData[ViewDataKeys.ComparisonSchool] = new PrimarySimilarSchoolsComparisonViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = response.SchoolName,
            SimilarSchoolName = response.SimilarSchoolDetails.Name
        };

        var schoolDetailsModel = new SimilarSchoolDetailsViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = response.SchoolName,
            SimilarSchoolName = response.SimilarSchoolDetails.Name,
            CurrentSchoolLatitude = response.CurrentSchoolCoordinates?.Latitude,
            CurrentSchoolLongitude = response.CurrentSchoolCoordinates?.Longitude,
            SimilarSchoolLatitude = response.SimilarSchoolCoordinates?.Latitude,
            SimilarSchoolLongitude = response.SimilarSchoolCoordinates?.Longitude,
            Distance = response.DistanceMiles,
            SimilarSchoolDetails = response.SimilarSchoolDetails
        };

        return View("~/Views/Shared/SimilarSchoolsComparison/SchoolDetails.cshtml", schoolDetailsModel);
    }

    private async Task<(PrimarySimilarSchoolsComparisonViewModel Model, SchoolInfo CurrentSchool, SchoolInfo SimilarSchool)>
        BuildBaseModelAsync(string urn, string similarSchoolUrn)
    {
        var currentSchool = (await getSchoolInfoUseCase.Execute(new GetSchoolInfoRequest(urn))).School;
        var similarSchool = (await getSchoolInfoUseCase.Execute(new GetSchoolInfoRequest(similarSchoolUrn))).School;

        var model = new PrimarySimilarSchoolsComparisonViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = currentSchool.Name,
            SimilarSchoolName = similarSchool.Name
        };

        return (model, currentSchool, similarSchool);
    }
}
