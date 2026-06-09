using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features;
using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Features.Ks4CoreSubjects;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;
using System.Globalization;

namespace SAPSec.Web.Controllers;

/// <summary>
/// Controller for school details pages.
/// Single Responsibility: HTTP handling and view selection only.
/// </summary>
[Route("school/{urn}")]
[Authorize]
public class SchoolController : Controller
{
    private readonly GetSchoolInfoUseCase _getSchoolInfoUseCase;
    private readonly GetSchoolDetailsUseCase _getSchoolDetailsUseCase;
    private readonly GetSchoolKs4HeadlineMeasuresUseCase _getSchoolKs4HeadlineMeasuresUseCase;
    private readonly GetSchoolKs4CoreSubjectsUseCase _getSchoolKs4CoreSubjectsUseCase;
    private readonly GetAttendanceMeasuresUseCase _getAttendanceMeasuresUseCase;
    private readonly ILogger<SchoolController> _logger;

    public SchoolController(
        GetSchoolInfoUseCase getSchoolInfoUseCase,
        GetSchoolDetailsUseCase getSchoolDetailsUseCase,
        GetSchoolKs4HeadlineMeasuresUseCase getSchoolKs4HeadlineMeasuresUseCase,
        GetSchoolKs4CoreSubjectsUseCase getSchoolKs4CoreSubjectsUseCase,
        GetAttendanceMeasuresUseCase getAttendanceMeasuresUseCase,
        ILogger<SchoolController> logger)
    {
        _getSchoolInfoUseCase = getSchoolInfoUseCase;
        _getSchoolDetailsUseCase = getSchoolDetailsUseCase;
        _getSchoolKs4HeadlineMeasuresUseCase = getSchoolKs4HeadlineMeasuresUseCase;
        _getSchoolKs4CoreSubjectsUseCase = getSchoolKs4CoreSubjectsUseCase;
        _getAttendanceMeasuresUseCase = getAttendanceMeasuresUseCase;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        var response = await _getSchoolInfoUseCase.Execute(new(urn));

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(response.School);

        return View(SchoolInfoViewModel.FromSchoolInfo(response.School));
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn)
    {
        var response = await _getSchoolDetailsUseCase.Execute(new(urn));

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(response.SchoolDetails);

        return View(SchoolDetailsViewModel.FromSchoolDetails(response.SchoolDetails));
    }

    [HttpGet]
    [Route("ks4-headline-measures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await _getSchoolKs4HeadlineMeasuresUseCase.Execute(new GetSchoolKs4HeadlineMeasuresRequest(urn, filters));
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(response.School);

        return View(BuildKs4HeadlineMeasuresViewModel(response));
    }

    [HttpGet]
    [Route("ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await _getSchoolKs4CoreSubjectsUseCase.Execute(new GetSchoolKs4CoreSubjectsRequest(urn, filters));
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(response.School);

        return View(BuildKs4CoreSubjectsViewModel(response));
    }

    [HttpGet]
    [Route("what-is-a-similar-school")]
    public async Task<IActionResult> WhatIsASimilarSchool(string urn)
    {
        var response = await _getSchoolInfoUseCase.Execute(new(urn));

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(response.School);

        return View(SchoolInfoViewModel.FromSchoolInfo(response.School));
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn)
    {
        var response = await _getSchoolInfoUseCase.Execute(new(urn));

        var attendanceMeasures = await _getAttendanceMeasuresUseCase.Execute(new GetAttendanceMeasuresRequest(urn));
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(response.School);

        return View(new SchoolAttendancePageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
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
        var response = await _getAttendanceMeasuresUseCase.Execute(new GetAttendanceMeasuresRequest(urn));
        var yearLabels = Ks4YearLabelConfig.YearByYear;
        var isPersistentAbsence = normalizedAbsenceType == "persistent";

        var selectedSchoolSeries = isPersistentAbsence
            ? response.PersistentAbsenceYearByYear.School
            : response.OverallAbsenceYearByYear.School;
        var similarSchoolsSeries = isPersistentAbsence
            ? response.PersistentAbsenceYearByYear.SimilarSchools
            : response.OverallAbsenceYearByYear.SimilarSchools;
        var localAuthoritySeries = isPersistentAbsence
            ? response.PersistentAbsenceYearByYear.LocalAuthority
            : response.OverallAbsenceYearByYear.LocalAuthority;
        var englandSeries = isPersistentAbsence
            ? response.PersistentAbsenceYearByYear.England
            : response.OverallAbsenceYearByYear.England;

        var selectedSchoolThreeYearAverage = isPersistentAbsence
            ? response.PersistentAbsenceThreeYearAverage.SchoolValue
            : response.OverallAbsenceThreeYearAverage.SchoolValue;
        var similarSchoolsThreeYearAverage = isPersistentAbsence
            ? response.PersistentAbsenceThreeYearAverage.SimilarSchoolsValue
            : response.OverallAbsenceThreeYearAverage.SimilarSchoolsValue;
        var localAuthorityThreeYearAverage = isPersistentAbsence
            ? response.PersistentAbsenceThreeYearAverage.LocalAuthorityValue
            : response.OverallAbsenceThreeYearAverage.LocalAuthorityValue;
        var englandThreeYearAverage = isPersistentAbsence
            ? response.PersistentAbsenceThreeYearAverage.EnglandValue
            : response.OverallAbsenceThreeYearAverage.EnglandValue;
        var topPerformers = isPersistentAbsence
            ? response.PersistentAbsenceTopPerformers
            : response.OverallAbsenceTopPerformers;

        return Json(new
        {
            absenceType = normalizedAbsenceType,
            years = yearLabels,
            bar = new decimal?[]
            {
                selectedSchoolThreeYearAverage,
                similarSchoolsThreeYearAverage,
                localAuthorityThreeYearAverage,
                englandThreeYearAverage
            },
            line = new
            {
                school = new decimal?[] { selectedSchoolSeries.Previous2, selectedSchoolSeries.Previous, selectedSchoolSeries.Current },
                similarSchools = new decimal?[] { similarSchoolsSeries.Previous2, similarSchoolsSeries.Previous, similarSchoolsSeries.Current },
                localAuthority = new decimal?[] { localAuthoritySeries.Previous2, localAuthoritySeries.Previous, localAuthoritySeries.Current },
                england = new decimal?[] { englandSeries.Previous2, englandSeries.Previous, englandSeries.Current }
            },
            table = new
            {
                school = new[]
                {
                    DisplayPercentNullable(selectedSchoolSeries.Previous2),
                    DisplayPercentNullable(selectedSchoolSeries.Previous),
                    DisplayPercentNullable(selectedSchoolSeries.Current),
                    DisplayPercentNullable(selectedSchoolThreeYearAverage)
                },
                similarSchools = new[]
                {
                    DisplayPercentNullable(similarSchoolsSeries.Previous2),
                    DisplayPercentNullable(similarSchoolsSeries.Previous),
                    DisplayPercentNullable(similarSchoolsSeries.Current),
                    DisplayPercentNullable(similarSchoolsThreeYearAverage)
                },
                localAuthority = new[]
                {
                    DisplayPercentNullable(localAuthoritySeries.Previous2),
                    DisplayPercentNullable(localAuthoritySeries.Previous),
                    DisplayPercentNullable(localAuthoritySeries.Current),
                    DisplayPercentNullable(localAuthorityThreeYearAverage)
                },
                england = new[]
                {
                    DisplayPercentNullable(englandSeries.Previous2),
                    DisplayPercentNullable(englandSeries.Previous),
                    DisplayPercentNullable(englandSeries.Current),
                    DisplayPercentNullable(englandThreeYearAverage)
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

    private void SetSchoolViewData(SchoolDetails school)
    {
        var layoutModel = new SchoolLayoutModel
        {
            Urn = school.Urn,
            Name = school.Name
        };

        ViewData["SchoolLayout"] = layoutModel;
    }

    private void SetSchoolViewData(SchoolInfo school)
    {
        var layoutModel = new SchoolLayoutModel
        {
            Urn = school.Urn,
            Name = school.Name
        };

        ViewData["SchoolLayout"] = layoutModel;
    }

    private static Ks4HeadlineMeasuresPageViewModel BuildKs4HeadlineMeasuresViewModel(
        GetSchoolKs4HeadlineMeasuresResponse response) => new()
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            SimilarSchoolsCount = response.SimilarSchoolsCount,
            Attainment8 = MapMeasure(response.Attainment8, response.School),
            EnglishAndMaths = MapMeasure(response.EnglishAndMaths, response.School),
            Destinations = MapMeasure(response.Destinations, response.School)
        };

    private static Ks4CoreSubjectsPageViewModel BuildKs4CoreSubjectsViewModel(
        GetSchoolKs4CoreSubjectsResponse response) => new()
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            SimilarSchoolsCount = response.SimilarSchoolsCount,
            Measures = response.Measures.Select(m => MapMeasure(m, response.School))
        };

    private static MeasureViewModel MapMeasure(Measure measure, SchoolInfo school) =>
        MeasureViewModel.FromMeasure(measure, school, [
            school.Name,
            "Similar schools average",
            "Local authority schools average",
            "Schools in England average",
        ]);

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
