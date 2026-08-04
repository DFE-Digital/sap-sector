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
    [Route("Similarity")]
    public async Task<IActionResult> Similarity(string urn, string similarSchoolUrn)
    {
        var (model, _, _) = await BuildBaseModelAsync(urn, similarSchoolUrn);

        var characteristicsResponse = await getPrimaryCharacteristicsComparison.Execute(
            new GetPrimaryCharacteristicsComparisonRequest(urn, similarSchoolUrn));
        model.CharacteristicsRows = primaryCharacteristicsComparisonFormatter.BuildRows(characteristicsResponse);

        ViewData["ComparisonSchool"] = model;
        return View("Similarity", model);
    }

    [HttpGet]
    [Route("ks2")]
    public async Task<IActionResult> Ks2(string urn, string similarSchoolUrn)
    {
        var (model, currentSchool, similarSchool) = await BuildBaseModelAsync(urn, similarSchoolUrn);

        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var comparisonResponse = await getSchoolKs2PerformanceComparisonUseCase.Execute(
            new GetSchoolKs2PerformanceComparisonRequest(urn, similarSchoolUrn, filters));

        Func<string, string> viewSimilarSchools = urn => Routes.PrimarySchool(urn).ViewSimilarSchools;
        Func<string, string, string> similarSchoolComparison = (currentSchoolUrn, similarSchoolUrn) => Routes.PrimarySchool(currentSchoolUrn).SimilarSchoolComparison(similarSchoolUrn);

        model.MeetingExpectedStandardRwm = MeasureViewModel.FromMeasure(comparisonResponse.MeetingExpectedStandardRwm, currentSchool, similarSchool,
            viewSimilarSchools, similarSchoolComparison);
        model.AchievedHigherStandardRwm = MeasureViewModel.FromMeasure(comparisonResponse.AchievedHigherStandardRwm, currentSchool, similarSchool,
            viewSimilarSchools, similarSchoolComparison);
        model.AverageScaledScoreReading = MeasureViewModel.FromMeasure(comparisonResponse.AverageScaledScoreReading, currentSchool, similarSchool,
            viewSimilarSchools, similarSchoolComparison);
        model.AverageScaledScoreMaths = MeasureViewModel.FromMeasure(comparisonResponse.AverageScaledScoreMaths, currentSchool, similarSchool,
            viewSimilarSchools, similarSchoolComparison);
        model.MeetingExpectedStandardGps = MeasureViewModel.FromMeasure(comparisonResponse.MeetingExpectedStandardGps, currentSchool, similarSchool,
            viewSimilarSchools, similarSchoolComparison);
        model.AchievedHigherStandardGps = MeasureViewModel.FromMeasure(comparisonResponse.AchievedHigherStandardGps, currentSchool, similarSchool,
            viewSimilarSchools, similarSchoolComparison);

        ViewData["ComparisonSchool"] = model;
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

        model.Absence = MeasureViewModel.FromMeasure(
            comparisonResponse.Absence, currentSchool, similarSchool);

        ViewData["ComparisonSchool"] = model;
        return View(model);
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn, string similarSchoolUrn)
    {
        var response = await getPrimarySimilarSchoolDetailsUseCase.Execute(
            new GetPrimarySimilarSchoolDetailsRequest(urn, similarSchoolUrn));

        ViewData["ComparisonSchool"] = new PrimarySimilarSchoolsComparisonViewModel
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
