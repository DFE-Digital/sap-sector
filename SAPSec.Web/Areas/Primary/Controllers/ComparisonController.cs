using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Primary.ViewModels.Comparison;
using SAPSec.Web.Areas.Shared.ViewModels.Comparison;
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
public class ComparisonController(
    IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse> getSchoolInfoUseCase,
    IUseCase<GetPrimarySimilarSchoolDetailsRequest, GetPrimarySimilarSchoolDetailsResponse> getPrimarySimilarSchoolDetailsUseCase,
    IUseCase<GetComparisonKs2PerformanceMeasuresRequest, GetComparisonKs2PerformanceMeasuresResponse> getKs2PerformanceMeasuresUseCase,
    GetPrimaryCharacteristicsComparison getPrimaryCharacteristicsComparison,
    IPrimaryCharacteristicsComparisonFormatter primaryCharacteristicsComparisonFormatter,
    IUseCase<GetComparisonAttendanceMeasuresRequest, GetComparisonAttendanceMeasuresResponse> getAttendanceMeasuresUseCase)
    : Controller
{
    [HttpGet]
    [Route("compare-similarity")]
    public async Task<IActionResult> Similarity(string urn, string similarSchoolUrn)
    {
        var currentSchool = (await getSchoolInfoUseCase.Execute(new GetSchoolInfoRequest(urn))).School;
        var similarSchool = (await getSchoolInfoUseCase.Execute(new GetSchoolInfoRequest(similarSchoolUrn))).School;
        var characteristicsResponse = await getPrimaryCharacteristicsComparison.Execute(
            new GetPrimaryCharacteristicsComparisonRequest(urn, similarSchoolUrn));

        ViewData[ViewDataKeys.ComparisonLayout] = new ComparisonLayoutModel(
            urn,
            currentSchool.Name,
            similarSchoolUrn,
            similarSchool.Name
        );

        var model = new SimilarityPageViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = currentSchool.Name,
            SimilarSchoolName = similarSchool.Name,
            CharacteristicsRows = primaryCharacteristicsComparisonFormatter.BuildRows(characteristicsResponse)
        };

        return View("Similarity", model);
    }

    [HttpGet]
    [Route("compare-ks2")]
    public async Task<IActionResult> Ks2PerformanceMeasures(string urn, string similarSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getKs2PerformanceMeasuresUseCase.Execute(
            new GetComparisonKs2PerformanceMeasuresRequest(urn, similarSchoolUrn, filters));

        ViewData[ViewDataKeys.ComparisonLayout] = new ComparisonLayoutModel(
            response.CurrentSchool.Urn,
            response.CurrentSchool.Name,
            response.SimilarSchool.Urn,
            response.SimilarSchool.Name
        );

        var model = new Ks2PerformanceMeasuresPageViewModel
        {
            Urn = response.CurrentSchool.Urn,
            Name = response.CurrentSchool.Name,
            SimilarSchoolUrn = response.SimilarSchool.Urn,
            SimilarSchoolName = response.SimilarSchool.Name,
            MeetingExpectedStandardRwm = MeasureViewModel.FromPrimaryComparisonMeasure(response.MeetingExpectedStandardRwm, response.CurrentSchool, response.SimilarSchool),
            AchievedHigherStandardRwm = MeasureViewModel.FromPrimaryComparisonMeasure(response.AchievedHigherStandardRwm, response.CurrentSchool, response.SimilarSchool),
            AverageScaledScoreReading = MeasureViewModel.FromPrimaryComparisonMeasure(response.AverageScaledScoreReading, response.CurrentSchool, response.SimilarSchool),
            AverageScaledScoreMaths = MeasureViewModel.FromPrimaryComparisonMeasure(response.AverageScaledScoreMaths, response.CurrentSchool, response.SimilarSchool),
            MeetingExpectedStandardGps = MeasureViewModel.FromPrimaryComparisonMeasure(response.MeetingExpectedStandardGps, response.CurrentSchool, response.SimilarSchool),
            AchievedHigherStandardGps = MeasureViewModel.FromPrimaryComparisonMeasure(response.AchievedHigherStandardGps, response.CurrentSchool, response.SimilarSchool),
        };

        return View(model);
    }

    [HttpGet]
    [Route("compare-attendance")]
    public async Task<IActionResult> Attendance(string urn, string similarSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getAttendanceMeasuresUseCase.Execute(new(MeasurePhase.Primary, urn, similarSchoolUrn, filters));

        ViewData[ViewDataKeys.ComparisonLayout] = new ComparisonLayoutModel(
            response.CurrentSchool.Urn,
            response.CurrentSchool.Name,
            response.SimilarSchool.Urn,
            response.SimilarSchool.Name
        );

        var model = new AttendancePageViewModel
        {
            Urn = response.CurrentSchool.Urn,
            Name = response.CurrentSchool.Name,
            SimilarSchoolUrn = response.SimilarSchool.Urn,
            SimilarSchoolName = response.SimilarSchool.Name,
            Absence = MeasureViewModel.FromPrimaryComparisonMeasure(response.Absence, response.CurrentSchool, response.SimilarSchool)
        };

        return View(model);
    }

    [HttpGet]
    [Route("compare-school-details")]
    public async Task<IActionResult> SchoolDetails(string urn, string similarSchoolUrn)
    {
        var response = await getPrimarySimilarSchoolDetailsUseCase.Execute(
            new GetPrimarySimilarSchoolDetailsRequest(urn, similarSchoolUrn));

        ViewData[ViewDataKeys.ComparisonLayout] = new ComparisonLayoutModel(
            urn,
            response.SchoolName,
            similarSchoolUrn,
            response.SimilarSchoolDetails.Name
        );

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

        return View(schoolDetailsModel);
    }
}
