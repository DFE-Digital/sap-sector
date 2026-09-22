using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Features.SchoolDetails.Comparison;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels.Comparison;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.Formatters;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.AllThrough.Controllers;

[Area("AllThrough")]
[Route("school/all-through/{urn}/view-similar-schools")]
[Authorize]
[RequireFeatureFlag(FeatureFlags.EnableAllThroughSchools)]
[RequireSchoolPhase(ExpectedSchoolPhase.AllThrough, "urn")]
public class ComparisonController(
    IUseCase<GetPrimaryComparisonSimilarityCharacteristicsRequest, GetPrimaryComparisonSimilarityCharacteristicsResponse> getPrimarySimilarityCharacteristicsUseCase,
    IUseCase<GetSecondaryComparisonSimilarityCharacteristicsRequest, GetSecondaryComparisonSimilarityCharacteristicsResponse> getSecondarySimilarityCharacteristicsUseCase,
    IUseCase<GetComparisonKs2PerformanceMeasuresRequest, GetComparisonKs2PerformanceMeasuresResponse> getKs2PerformanceMeasuresUseCase,
    IUseCase<GetComparisonKs4HeadlineMeasuresRequest, GetComparisonKs4HeadlineMeasuresResponse> getKs4HeadlineMeasuresUseCase,
    IUseCase<GetComparisonKs4CoreSubjectsMeasuresRequest, GetComparisonKs4CoreSubjectsMeasuresResponse> getKs4CoreSubjectsUseCase,
    IUseCase<GetPrimaryComparisonAttendanceMeasuresRequest, GetComparisonAttendanceMeasuresResponse> getPrimaryAttendanceMeasuresUseCase,
    IUseCase<GetSecondaryComparisonAttendanceMeasuresRequest, GetComparisonAttendanceMeasuresResponse> getSecondaryAttendanceMeasuresUseCase,
    [FromKeyedServices(ServiceKeys.Primary)]
    IUseCase<GetComparisonSchoolDetailsRequest, GetComparisonSchoolDetailsResponse> getPrimarySchoolDetailsUseCase,
    [FromKeyedServices(ServiceKeys.Secondary)]
    IUseCase<GetComparisonSchoolDetailsRequest, GetComparisonSchoolDetailsResponse> getSecondarySchoolDetailsUseCase,
    IPrimaryCharacteristicsComparisonFormatter primaryCharacteristicsComparisonFormatter,
    ISecondaryCharacteristicsComparisonFormatter secondaryCharacteristicsComparisonFormatter)
    : Controller
{
    private const string SimilarityView = "~/Areas/Shared/Views/Comparison/Similarity.cshtml";
    private const string PrimaryKs2View = "~/Areas/Primary/Views/Comparison/Ks2PerformanceMeasures.cshtml";
    private const string AttendanceView = "~/Areas/Shared/Views/Comparison/Attendance.cshtml";
    private const string SecondaryKs4HeadlineMeasuresView = "~/Areas/Secondary/Views/Comparison/Ks4HeadlineMeasures.cshtml";
    private const string SecondaryKs4CoreSubjectsView = "~/Areas/Secondary/Views/Comparison/Ks4CoreSubjects.cshtml";
    private const string SchoolDetailsView = "~/Areas/Shared/Views/Comparison/SchoolDetails.cshtml";

    [HttpGet]
    [Route("primary/{comparatorSchoolUrn}/compare-similarity")]
    [RequireSchoolPhase(ExpectedSchoolPhase.PrimaryComparisonParticipant, "comparatorSchoolUrn")]
    public async Task<IActionResult> PrimarySimilarity(string urn, string comparatorSchoolUrn)
    {
        var response = await getPrimarySimilarityCharacteristicsUseCase.Execute(new(urn, comparatorSchoolUrn));

        SetPrimaryComparisonLayout(response.CurrentSchool, response.ComparatorSchool);

        var model = new SimilarityPageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            ComparatorSchool = SchoolInfoViewModel.FromSchoolInfo(response.ComparatorSchool),
            CharacteristicsRows = primaryCharacteristicsComparisonFormatter.BuildRows(response.SimilarityCharacteristics)
        };

        return View(SimilarityView, model);
    }

    [HttpGet]
    [Route("primary/{comparatorSchoolUrn}/compare-ks2")]
    [RequireSchoolPhase(ExpectedSchoolPhase.PrimaryComparisonParticipant, "comparatorSchoolUrn")]
    public async Task<IActionResult> PrimaryKs2PerformanceMeasures(string urn, string comparatorSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getKs2PerformanceMeasuresUseCase.Execute(new(urn, comparatorSchoolUrn, filters));

        SetPrimaryComparisonLayout(response.CurrentSchool, response.ComparatorSchool);

        var model = new Primary.ViewModels.Comparison.Ks2PerformanceMeasuresPageViewModel
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

        return View(PrimaryKs2View, model);
    }

    [HttpGet]
    [Route("primary/{comparatorSchoolUrn}/compare-attendance")]
    [RequireSchoolPhase(ExpectedSchoolPhase.PrimaryComparisonParticipant, "comparatorSchoolUrn")]
    public async Task<IActionResult> PrimaryAttendance(string urn, string comparatorSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getPrimaryAttendanceMeasuresUseCase.Execute(new(urn, comparatorSchoolUrn, filters));

        SetPrimaryComparisonLayout(response.CurrentSchool, response.ComparatorSchool);

        var model = new AttendancePageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            ComparatorSchool = SchoolInfoViewModel.FromSchoolInfo(response.ComparatorSchool),
            Absence = MeasureViewModel.FromPrimaryComparisonMeasure(response.Absence, response.CurrentSchool, response.ComparatorSchool)
        };

        return View(AttendanceView, model);
    }

    [HttpGet]
    [Route("primary/{comparatorSchoolUrn}/compare-school-details")]
    [RequireSchoolPhase(ExpectedSchoolPhase.PrimaryComparisonParticipant, "comparatorSchoolUrn")]
    public Task<IActionResult> PrimarySchoolDetails(string urn, string comparatorSchoolUrn) =>
        SchoolDetails(urn, comparatorSchoolUrn, getPrimarySchoolDetailsUseCase, isPrimaryComparison: true);

    [HttpGet]
    [Route("secondary/{comparatorSchoolUrn}/compare-similarity")]
    [RequireSchoolPhase(ExpectedSchoolPhase.SecondaryComparisonParticipant, "comparatorSchoolUrn")]
    public async Task<IActionResult> SecondarySimilarity(string urn, string comparatorSchoolUrn)
    {
        var response = await getSecondarySimilarityCharacteristicsUseCase.Execute(new(urn, comparatorSchoolUrn));

        SetSecondaryComparisonLayout(response.CurrentSchool, response.ComparatorSchool);

        var model = new SimilarityPageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            ComparatorSchool = SchoolInfoViewModel.FromSchoolInfo(response.ComparatorSchool),
            CharacteristicsRows = secondaryCharacteristicsComparisonFormatter.BuildRows(response.SimilarityCharacteristics)
        };

        return View(SimilarityView, model);
    }

    [HttpGet]
    [Route("secondary/{comparatorSchoolUrn}/compare-ks4-headline-measures")]
    [RequireSchoolPhase(ExpectedSchoolPhase.SecondaryComparisonParticipant, "comparatorSchoolUrn")]
    public async Task<IActionResult> SecondaryKs4HeadlineMeasures(string urn, string comparatorSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getKs4HeadlineMeasuresUseCase.Execute(new(urn, comparatorSchoolUrn, filters));

        SetSecondaryComparisonLayout(response.CurrentSchool, response.ComparatorSchool);

        var model = new Secondary.ViewModels.Comparison.Ks4HeadlineMeasuresPageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            ComparatorSchool = SchoolInfoViewModel.FromSchoolInfo(response.ComparatorSchool),
            Attainment8 = MeasureViewModel.FromSecondaryComparisonMeasure(response.Attainment8, response.CurrentSchool, response.ComparatorSchool),
            EnglishMaths = MeasureViewModel.FromSecondaryComparisonMeasure(response.EnglishMaths, response.CurrentSchool, response.ComparatorSchool),
            Destinations = MeasureViewModel.FromSecondaryComparisonMeasure(response.Destinations, response.CurrentSchool, response.ComparatorSchool)
        };

        return View(SecondaryKs4HeadlineMeasuresView, model);
    }

    [HttpGet]
    [Route("secondary/{comparatorSchoolUrn}/compare-ks4-core-subjects")]
    [RequireSchoolPhase(ExpectedSchoolPhase.SecondaryComparisonParticipant, "comparatorSchoolUrn")]
    public async Task<IActionResult> SecondaryKs4CoreSubjects(string urn, string comparatorSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getKs4CoreSubjectsUseCase.Execute(new(urn, comparatorSchoolUrn, filters));

        SetSecondaryComparisonLayout(response.CurrentSchool, response.ComparatorSchool);

        var model = new Secondary.ViewModels.Comparison.Ks4CoreSubjectsPageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            ComparatorSchool = SchoolInfoViewModel.FromSchoolInfo(response.ComparatorSchool),
            Measures = [
                MeasureViewModel.FromSecondaryComparisonMeasure(response.EnglishLanguage, response.CurrentSchool, response.ComparatorSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.EnglishLiterature, response.CurrentSchool, response.ComparatorSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Maths, response.CurrentSchool, response.ComparatorSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.CombinedScience, response.CurrentSchool, response.ComparatorSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Biology, response.CurrentSchool, response.ComparatorSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Chemistry, response.CurrentSchool, response.ComparatorSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Physics, response.CurrentSchool, response.ComparatorSchool)
            ]
        };

        return View(SecondaryKs4CoreSubjectsView, model);
    }

    [HttpGet]
    [Route("secondary/{comparatorSchoolUrn}/compare-attendance")]
    [RequireSchoolPhase(ExpectedSchoolPhase.SecondaryComparisonParticipant, "comparatorSchoolUrn")]
    public async Task<IActionResult> SecondaryAttendance(string urn, string comparatorSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getSecondaryAttendanceMeasuresUseCase.Execute(new(urn, comparatorSchoolUrn, filters));

        SetSecondaryComparisonLayout(response.CurrentSchool, response.ComparatorSchool);

        var model = new AttendancePageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            ComparatorSchool = SchoolInfoViewModel.FromSchoolInfo(response.ComparatorSchool),
            Absence = MeasureViewModel.FromSecondaryComparisonMeasure(response.Absence, response.CurrentSchool, response.ComparatorSchool)
        };

        return View(AttendanceView, model);
    }

    [HttpGet]
    [Route("secondary/{comparatorSchoolUrn}/compare-school-details")]
    [RequireSchoolPhase(ExpectedSchoolPhase.SecondaryComparisonParticipant, "comparatorSchoolUrn")]
    public Task<IActionResult> SecondarySchoolDetails(string urn, string comparatorSchoolUrn) =>
        SchoolDetails(urn, comparatorSchoolUrn, getSecondarySchoolDetailsUseCase, isPrimaryComparison: false);

    private async Task<IActionResult> SchoolDetails(
        string urn,
        string comparatorSchoolUrn,
        IUseCase<GetComparisonSchoolDetailsRequest, GetComparisonSchoolDetailsResponse> getSchoolDetailsUseCase,
        bool isPrimaryComparison)
    {
        var response = await getSchoolDetailsUseCase.Execute(new(urn, comparatorSchoolUrn));

        if (isPrimaryComparison)
        {
            SetPrimaryComparisonLayout(response.CurrentSchool.School, response.ComparatorSchool.School);
        }
        else
        {
            SetSecondaryComparisonLayout(response.CurrentSchool.School, response.ComparatorSchool.School);
        }

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

        return View(SchoolDetailsView, model);
    }

    private void SetPrimaryComparisonLayout(
        SAPSec.Core.Features.SchoolInfo.SchoolInfo currentSchool,
        SAPSec.Core.Features.SchoolInfo.SchoolInfo comparatorSchool) =>
        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.AllThroughPrimary(
            currentSchool,
            comparatorSchool);

    private void SetSecondaryComparisonLayout(
        SAPSec.Core.Features.SchoolInfo.SchoolInfo currentSchool,
        SAPSec.Core.Features.SchoolInfo.SchoolInfo comparatorSchool) =>
        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.AllThroughSecondary(
            currentSchool,
            comparatorSchool);

}
