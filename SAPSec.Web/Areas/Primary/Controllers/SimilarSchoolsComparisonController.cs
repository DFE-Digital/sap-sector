using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Primary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Primary.ViewModels;
using SAPSec.Web.Filters;
using SAPSec.Web.ViewModels;
using static SAPSec.Core.Constants.Measures.Primary;

namespace SAPSec.Web.Areas.Primary.Controllers;

[Area("Primary")]
[Route("school/primary/{urn}/view-similar-schools/{similarSchoolUrn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Primary, "urn", "similarSchoolUrn")]
[RequireFeatureFlag(FeatureFlags.EnablePrimarySchools)]
public class SimilarSchoolsComparisonController(
    IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse> getSchoolInfoUseCase,
    IUseCase<GetPrimarySimilarSchoolDetailsRequest, GetPrimarySimilarSchoolDetailsResponse> getPrimarySimilarSchoolDetailsUseCase,
    IUseCase<GetSchoolKs2PerformanceComparisonRequest, GetSchoolKs2PerformanceComparisonResponse> getSchoolKs2PerformanceComparisonUseCase)
    : Controller
{
    [HttpGet]
    public Task<IActionResult> Index(string urn, string similarSchoolUrn) =>
        Similarity(urn, similarSchoolUrn);

    [HttpGet]
    [Route("Similarity")]
    public async Task<IActionResult> Similarity(string urn, string similarSchoolUrn)
    {
        var model = await BuildBaseModelAsync(urn, similarSchoolUrn);
        ViewData["ComparisonSchool"] = model;
        return View("Similarity", model);
    }

    [HttpGet]
    [Route("ks2")]
    public async Task<IActionResult> Ks2(string urn, string similarSchoolUrn)
    {
        var model = await BuildBaseModelAsync(urn, similarSchoolUrn);

        var comparisonResponse = await getSchoolKs2PerformanceComparisonUseCase.Execute(
            new GetSchoolKs2PerformanceComparisonRequest(urn, similarSchoolUrn));
        model.MeetingExpectedStandardRwm = comparisonResponse.MeetingExpectedStandardRwm;

        ViewData["ComparisonSchool"] = model;
        return View(model);
    }

    [HttpGet]
    [Route("ks2/data")]
    public async Task<IActionResult> Ks2Data(string urn, string similarSchoolUrn, string subject = "rwm")
    {
        if (string.IsNullOrWhiteSpace(urn) || string.IsNullOrWhiteSpace(similarSchoolUrn))
        {
            return BadRequest(new { error = "Missing route parameters." });
        }

        var normalizedSubject = NormalizeSubjectFilter(subject);

        var response = await getSchoolKs2PerformanceComparisonUseCase.Execute(
            new GetSchoolKs2PerformanceComparisonRequest(urn, similarSchoolUrn, new Dictionary<string, string>
            {
                [Ks2ExpectedRwm.Filters.Subject.Key] = normalizedSubject
            }));

        var series = response.MeetingExpectedStandardRwm.Series;
        var currentSchoolSeries = series.First(s => s.SeriesType == MeasureSeriesType.CurrentSchool);
        var similarSchoolSeries = series.First(s => s.SeriesType == MeasureSeriesType.SimilarSchool);
        var englandSeries = series.First(s => s.SeriesType == MeasureSeriesType.EnglandSchoolsAverage);

        return Json(new
        {
            subject = normalizedSubject,
            bar = new decimal?[]
            {
                currentSchoolSeries.Current,
                similarSchoolSeries.Current,
                englandSeries.Current
            },
            line = new
            {
                thisSchool = new decimal?[] { currentSchoolSeries.Previous2, currentSchoolSeries.Previous, currentSchoolSeries.Current },
                similarSchool = new decimal?[] { similarSchoolSeries.Previous2, similarSchoolSeries.Previous, similarSchoolSeries.Current },
                england = new decimal?[] { englandSeries.Previous2, englandSeries.Previous, englandSeries.Current }
            },
            table = new
            {
                thisSchool = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(currentSchoolSeries.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(currentSchoolSeries.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(currentSchoolSeries.Current)
                },
                similarSchool = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(similarSchoolSeries.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(similarSchoolSeries.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(similarSchoolSeries.Current)
                },
                england = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandSeries.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandSeries.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandSeries.Current)
                }
            }
        });
    }

    private static string NormalizeSubjectFilter(string? subject) =>
        Ks2ExpectedRwm.Filters.Subject.Values.AllValues.Any(v => v.Value == subject)
            ? subject!
            : Ks2ExpectedRwm.Filters.Subject.Values.ReadingWritingMaths;

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn, string similarSchoolUrn)
    {
        var model = await BuildBaseModelAsync(urn, similarSchoolUrn);
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

    private async Task<PrimarySimilarSchoolsComparisonViewModel> BuildBaseModelAsync(string urn, string similarSchoolUrn)
    {
        var currentSchool = (await getSchoolInfoUseCase.Execute(new GetSchoolInfoRequest(urn))).School;
        var similarSchool = (await getSchoolInfoUseCase.Execute(new GetSchoolInfoRequest(similarSchoolUrn))).School;

        return new PrimarySimilarSchoolsComparisonViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = currentSchool.Name,
            SimilarSchoolName = similarSchool.Name
        };
    }
}
