using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.Secondary;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.Services;
using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;
using System.Globalization;

namespace SAPSec.Web.Areas.Secondary.Controllers;

/// <summary>
/// Controller for school details pages.
/// Single Responsibility: HTTP handling and view selection only.
/// </summary>
[Area("Secondary")]
[Route("school/secondary/{urn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Secondary)]
public class SchoolController : Controller
{
    private readonly IUseCase<GetSchoolKs4HeadlineMeasuresRequest, GetSchoolKs4HeadlineMeasuresResponse> _getSchoolKs4HeadlineMeasuresUseCase;
    private readonly IUseCase<GetSchoolKs4CoreSubjectsRequest, GetSchoolKs4CoreSubjectsResponse> _getSchoolKs4CoreSubjectsUseCase;
    private readonly GetAttendanceMeasures _getAttendanceMeasures;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly IRequestSchoolAccessor _requestSchoolAccessor;
    private readonly ILogger<SchoolController> _logger;

    public SchoolController(
        IUseCase<GetSchoolKs4HeadlineMeasuresRequest, GetSchoolKs4HeadlineMeasuresResponse> getSchoolKs4HeadlineMeasuresUseCase,
        IUseCase<GetSchoolKs4CoreSubjectsRequest, GetSchoolKs4CoreSubjectsResponse> getSchoolKs4CoreSubjectsUseCase,
        GetAttendanceMeasures getAttendanceMeasures,
        IFeatureFlagService featureFlagService,
        IRequestSchoolAccessor requestSchoolAccessor,
        ILogger<SchoolController> logger)
    {
        _getSchoolKs4HeadlineMeasuresUseCase = getSchoolKs4HeadlineMeasuresUseCase;
        _getSchoolKs4CoreSubjectsUseCase = getSchoolKs4CoreSubjectsUseCase;
        _getAttendanceMeasures = getAttendanceMeasures;
        _featureFlagService = featureFlagService;
        _requestSchoolAccessor = requestSchoolAccessor;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        var school = await _requestSchoolAccessor.GetAsync(HttpContext, urn);

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewDataAsync(school);
        return View(school);
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn)
    {
        var school = await _requestSchoolAccessor.GetAsync(HttpContext, urn);
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewDataAsync(school);
        return View(school);
    }

    [HttpGet]
    [Route("what-is-a-similar-school")]
    public async Task<IActionResult> WhatIsASimilarSchool(string urn)
    {
        var school = await _requestSchoolAccessor.GetAsync(HttpContext, urn);
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewDataAsync(school);
        return View(school);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn)
    {
        var school = await _requestSchoolAccessor.GetAsync(HttpContext, urn);
        var attendanceMeasures = await _getAttendanceMeasures.Execute(new(urn));
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewDataAsync(school);
        return View(new SchoolAttendancePageViewModel
        {
            SchoolDetails = school,
            AttendanceMeasures = attendanceMeasures
        });
    }

    [HttpGet]
    [Route("attendance-data")]
    public async Task<IActionResult> AttendanceData(string urn, string absenceType = "overall")
    {
        if (string.IsNullOrWhiteSpace(urn))
        {
            return BadRequest(new { error = "Missing route parameters." });
        }

        var normalizedAbsenceType = NormalizeAttendanceOption(absenceType, "overall", "persistent");
        var response = await _getAttendanceMeasures.Execute(new(urn));
        var yearLabels = AcademicYearLabelConfig.AttendanceYearByYear;
        var isPersistentAbsence = normalizedAbsenceType == "persistent";

        var selectedSchoolSeries = isPersistentAbsence
            ? response.PersistentAbsenceYearByYear.School
            : response.OverallAbsenceYearByYear.School;
        var localAuthoritySeries = isPersistentAbsence
            ? response.PersistentAbsenceYearByYear.LocalAuthority
            : response.OverallAbsenceYearByYear.LocalAuthority;
        var englandSeries = isPersistentAbsence
            ? response.PersistentAbsenceYearByYear.England
            : response.OverallAbsenceYearByYear.England;
        var selectedSchoolCurrentValue = selectedSchoolSeries.Current;
        var localAuthorityCurrentValue = localAuthoritySeries.Current;
        var englandCurrentValue = englandSeries.Current;
        var topPerformers = isPersistentAbsence
            ? response.PersistentAbsenceTopPerformers
            : response.OverallAbsenceTopPerformers;

        return Json(new
        {
            absenceType = normalizedAbsenceType,
            years = yearLabels,
            bar = new decimal?[]
            {
                selectedSchoolCurrentValue,
                localAuthorityCurrentValue,
                englandCurrentValue
            },
            line = new
            {
                school = new decimal?[] { selectedSchoolSeries.Previous2, selectedSchoolSeries.Previous, selectedSchoolSeries.Current },
                localAuthority = new decimal?[] { localAuthoritySeries.Previous2, localAuthoritySeries.Previous, localAuthoritySeries.Current },
                england = new decimal?[] { englandSeries.Previous2, englandSeries.Previous, englandSeries.Current }
            },
            table = new
            {
                school = new[]
                {
                    DisplayPercentNullable(selectedSchoolSeries.Previous2),
                    DisplayPercentNullable(selectedSchoolSeries.Previous),
                    DisplayPercentNullable(selectedSchoolSeries.Current)
                },
                localAuthority = new[]
                {
                    DisplayPercentNullable(localAuthoritySeries.Previous2),
                    DisplayPercentNullable(localAuthoritySeries.Previous),
                    DisplayPercentNullable(localAuthoritySeries.Current)
                },
                england = new[]
                {
                    DisplayPercentNullable(englandSeries.Previous2),
                    DisplayPercentNullable(englandSeries.Previous),
                    DisplayPercentNullable(englandSeries.Current)
                }
            },
            topPerformers = topPerformers.Select(x => new
            {
                x.Rank,
                x.Urn,
                x.Name,
                x.IsCurrentSchool,
                DisplayValue = SchoolAttendancePageViewModel.DisplayPercentNullable(x.Value)
            })
        });
    }

    [HttpGet]
    [Route("ks4-headline-measures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await _getSchoolKs4HeadlineMeasuresUseCase.Execute(new(urn, filters));

        PopulateViewData(response.School);

        var model = new ViewModels.Ks4HeadlineMeasuresPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            Attainment8 = MeasureViewModel.FromSecondaryMeasure(response.Attainment8, response.School),
            EnglishMaths = MeasureViewModel.FromSecondaryMeasure(response.EnglishMaths, response.School),
            Destinations = MeasureViewModel.FromSecondaryMeasure(response.Destinations, response.School)
        };

        return View(model);
    }

    [HttpGet]
    [Route("ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await _getSchoolKs4CoreSubjectsUseCase.Execute(new(urn, filters));

        PopulateViewData(response.School);

        var model = new ViewModels.Ks4CoreSubjectsPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            Measures = [
                MeasureViewModel.FromSecondaryMeasure(response.EnglishLanguage, response.School),
                MeasureViewModel.FromSecondaryMeasure(response.EnglishLiterature, response.School),
                MeasureViewModel.FromSecondaryMeasure(response.Maths, response.School),
                MeasureViewModel.FromSecondaryMeasure(response.CombinedScience, response.School),
                MeasureViewModel.FromSecondaryMeasure(response.Biology, response.School),
                MeasureViewModel.FromSecondaryMeasure(response.Chemistry, response.School),
                MeasureViewModel.FromSecondaryMeasure(response.Physics, response.School)
            ]
        };

        return View(model);
    }

    private void PopulateViewData(SchoolInfo currentSchool)
    {
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(currentSchool.Urn);
        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchoolInfo(currentSchool);
        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreateSecondary(
            Url,
            currentSchool.Urn,
            ControllerContext.ActionDescriptor.ActionName);
    }

    private void SetSchoolViewDataAsync(Core.Model.SchoolDetails school)
    {
        ViewData[ViewDataKeys.SchoolDetails] = school;

        if (Url is null)
        {
            return;
        }

        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreateSecondary(
            Url,
            school.Urn,
            ControllerContext.ActionDescriptor.ActionName);
    }

    private static string NormalizeAttendanceOption(string? requested, params string[] allowedValues)
    {
        if (string.IsNullOrWhiteSpace(requested))
        {
            return allowedValues[0];
        }

        return allowedValues.Contains(requested, StringComparer.OrdinalIgnoreCase)
            ? requested.ToLowerInvariant()
            : allowedValues[0];
    }

    private static string DisplayPercentNullable(decimal? value) =>
        value.HasValue
            ? value.Value.ToString("0.00", CultureInfo.InvariantCulture) + "%"
            : "No available data";
}
