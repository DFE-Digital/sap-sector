using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;
using System.Globalization;
using static SAPSec.Web.ViewModels.Ks4HeadlineMeasuresPageViewModel;

namespace SAPSec.Web.Controllers;

/// <summary>
/// Controller for school details pages.
/// Single Responsibility: HTTP handling and view selection only.
/// </summary>
[Route("school/{urn}")]
public class SchoolController : Controller
{
    private readonly ISchoolDetailsService _schoolDetailsService;
    private readonly GetSchoolKs4HeadlineMeasures _getSchoolKs4HeadlineMeasures;
    private readonly GetAttendanceMeasures _getAttendanceMeasures;
    private readonly ILogger<SchoolController> _logger;

    public SchoolController(
        ISchoolDetailsService schoolDetailsService,
        GetSchoolKs4HeadlineMeasures getSchoolKs4HeadlineMeasures,
        GetAttendanceMeasures getAttendanceMeasures,
        ILogger<SchoolController> logger)
    {
        _schoolDetailsService = schoolDetailsService;
        _getSchoolKs4HeadlineMeasures = getSchoolKs4HeadlineMeasures;
        _getAttendanceMeasures = getAttendanceMeasures;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        var school = await _schoolDetailsService.GetByUrnAsync(urn);

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(school);
        return View(school);
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn)
    {
        var school = await _schoolDetailsService.GetByUrnAsync(urn);
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(school);
        return View(school);
    }

    [HttpGet]
    [Route("what-is-a-similar-school")]
    public async Task<IActionResult> WhatIsASimilarSchool(string urn)
    {
        var school = await _schoolDetailsService.GetByUrnAsync(urn);
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(school);
        return View(school);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn)
    {
        var school = await _schoolDetailsService.GetByUrnAsync(urn);
        var attendanceMeasures = await _getAttendanceMeasures.Execute(new GetAttendanceMeasuresRequest(urn));
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(school);
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
        var response = await _getAttendanceMeasures.Execute(new GetAttendanceMeasuresRequest(urn));
        var yearLabels = Ks4YearLabelConfig.YearByYear;
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

        var selectedSchoolThreeYearAverage = isPersistentAbsence
            ? response.PersistentAbsenceThreeYearAverage.SchoolValue
            : response.OverallAbsenceThreeYearAverage.SchoolValue;
        var localAuthorityThreeYearAverage = isPersistentAbsence
            ? response.PersistentAbsenceThreeYearAverage.LocalAuthorityValue
            : response.OverallAbsenceThreeYearAverage.LocalAuthorityValue;
        var englandThreeYearAverage = isPersistentAbsence
            ? response.PersistentAbsenceThreeYearAverage.EnglandValue
            : response.OverallAbsenceThreeYearAverage.EnglandValue;

        return Json(new
        {
            absenceType = normalizedAbsenceType,
            years = yearLabels,
            bar = new decimal?[]
            {
                selectedSchoolThreeYearAverage,
                localAuthorityThreeYearAverage,
                englandThreeYearAverage
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
                    DisplayPercentNullable(selectedSchoolSeries.Current),
                    DisplayPercentNullable(selectedSchoolThreeYearAverage)
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
            }
        });
    }

    [HttpGet]
    [Route("ks4-headline-measures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(string urn)
    {
        var response = await _getSchoolKs4HeadlineMeasures.Execute(new GetSchoolKs4HeadlineMeasuresRequest(urn));
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(response.SchoolDetails);

        return View(BuildKs4HeadlineMeasuresViewModel(response));
    }

    [HttpGet]
    [Route("ks4-headline-measures/data")]
    public async Task<IActionResult> Ks4HeadlineMeasuresData(string urn, string grade = "4")
    {
        var response = await _getSchoolKs4HeadlineMeasures.Execute(new GetSchoolKs4HeadlineMeasuresRequest(urn));
        var model = BuildKs4HeadlineMeasuresViewModel(response);
        var gradeFilter = SchoolKs4EngMathsSelection.ParseFilter(grade);
        var selectedEngMaths = SchoolKs4EngMathsSelection.From(response, gradeFilter);

        return Json(new
        {
            grade = SchoolKs4EngMathsSelection.ToFilterValue(gradeFilter),
            bar = new decimal?[]
            {
                selectedEngMaths.ThreeYearAverage.SchoolValue,
                selectedEngMaths.ThreeYearAverage.SimilarSchoolsValue,
                selectedEngMaths.ThreeYearAverage.LocalAuthorityValue,
                selectedEngMaths.ThreeYearAverage.EnglandValue
            },
            line = new
            {
                thisSchool = SeriesToArray(selectedEngMaths.YearByYear.School),
                similarSchools = SeriesToArray(selectedEngMaths.YearByYear.SimilarSchools),
                localAuthority = SeriesToArray(selectedEngMaths.YearByYear.LocalAuthority),
                england = SeriesToArray(selectedEngMaths.YearByYear.England)
            },
            table = new
            {
                thisSchool = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.YearByYear.School.Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.YearByYear.School.Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.YearByYear.School.Current),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.ThreeYearAverage.SchoolValue)
                },
                similarSchools = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.YearByYear.SimilarSchools.Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.YearByYear.SimilarSchools.Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.YearByYear.SimilarSchools.Current),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.ThreeYearAverage.SimilarSchoolsValue)
                },
                localAuthority = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.YearByYear.LocalAuthority.Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.YearByYear.LocalAuthority.Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.YearByYear.LocalAuthority.Current),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.ThreeYearAverage.LocalAuthorityValue)
                },
                england = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.YearByYear.England.Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.YearByYear.England.Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.YearByYear.England.Current),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent(selectedEngMaths.ThreeYearAverage.EnglandValue)
                }
            },
            topPerformers = selectedEngMaths.TopPerformers
                .Select(x => new
                {
                    x.Rank,
                    x.Urn,
                    x.Name,
                    DisplayValue = Ks4HeadlineMeasuresPageViewModel.DisplayPercent(x.Value)
                })
        });
    }

    [HttpGet]
    [Route("ks4-destinations/data")]
    public async Task<IActionResult> Ks4DestinationsData(string urn, string destination = "all")
    {
        var response = await _getSchoolKs4HeadlineMeasures.Execute(new GetSchoolKs4HeadlineMeasuresRequest(urn));
        var model = BuildKs4HeadlineMeasuresViewModel(response);
        var destinationFilter = SchoolKs4DestinationsSelection.ParseFilter(destination);
        var selectedDestinations = SchoolKs4DestinationsSelection.From(response, destinationFilter);

        return Json(new
        {
            destination = SchoolKs4DestinationsSelection.ToFilterValue(destinationFilter),
            bar = new decimal?[]
            {
                selectedDestinations.ThreeYearAverage.SchoolValue,
                selectedDestinations.ThreeYearAverage.SimilarSchoolsValue,
                selectedDestinations.ThreeYearAverage.LocalAuthorityValue,
                selectedDestinations.ThreeYearAverage.EnglandValue
            },
            line = new
            {
                thisSchool = SeriesToArray(selectedDestinations.YearByYear.School),
                similarSchools = SeriesToArray(selectedDestinations.YearByYear.SimilarSchools),
                localAuthority = SeriesToArray(selectedDestinations.YearByYear.LocalAuthority),
                england = SeriesToArray(selectedDestinations.YearByYear.England)
            },
            table = new
            {
                thisSchool = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.YearByYear.School.Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.YearByYear.School.Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.YearByYear.School.Current),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.ThreeYearAverage.SchoolValue)
                },
                similarSchools = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.YearByYear.SimilarSchools.Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.YearByYear.SimilarSchools.Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.YearByYear.SimilarSchools.Current),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.ThreeYearAverage.SimilarSchoolsValue)
                },
                localAuthority = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.YearByYear.LocalAuthority.Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.YearByYear.LocalAuthority.Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.YearByYear.LocalAuthority.Current),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.ThreeYearAverage.LocalAuthorityValue)
                },
                england = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.YearByYear.England.Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.YearByYear.England.Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.YearByYear.England.Current),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(selectedDestinations.ThreeYearAverage.EnglandValue)
                }
            },
            topPerformers = selectedDestinations.TopPerformers
                .Select(x => new
                {
                    x.Rank,
                    x.Urn,
                    x.Name,
                    DisplayValue = Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(x.Value)
                })
        });
    }

    [HttpGet]
    [Route("ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(string urn)
    {
        var school = await _schoolDetailsService.GetByUrnAsync(urn);
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(school);
        return View(school);
    }

    private void SetSchoolViewData(Core.Model.SchoolDetails school)
    {
        ViewData["SchoolDetails"] = school;
    }

    private static decimal?[] SeriesToArray(Ks4HeadlineMeasureSeries series) =>
        [series.Previous2, series.Previous, series.Current];

    private static Ks4HeadlineMeasuresPageViewModel BuildKs4HeadlineMeasuresViewModel(
        GetSchoolKs4HeadlineMeasuresResponse response)
    {
        var defaultEngMaths = SchoolKs4EngMathsSelection.From(response, SchoolKs4GradeFilter.Grade4);
        var defaultDestinations = SchoolKs4DestinationsSelection.From(response, SchoolKs4DestinationFilter.All);

        return new()
        {
            SchoolDetails = response.SchoolDetails,
            SimilarSchoolsCount = response.SimilarSchoolsCount,
            SchoolAttainment8ThreeYearAverage = response.Attainment8ThreeYearAverage.SchoolValue,
            SimilarSchoolsAttainment8ThreeYearAverage = response.Attainment8ThreeYearAverage.SimilarSchoolsValue,
            LocalAuthorityAttainment8ThreeYearAverage = response.Attainment8ThreeYearAverage.LocalAuthorityValue,
            EnglandAttainment8ThreeYearAverage = response.Attainment8ThreeYearAverage.EnglandValue,
            Attainment8TopPerformers = MapTopPerformers(response.Attainment8TopPerformers, Ks4HeadlineMeasuresPageViewModel.DisplayValue),
            SchoolAttainment8YearByYear = response.Attainment8YearByYear.School,
            SimilarSchoolsAttainment8YearByYear = response.Attainment8YearByYear.SimilarSchools,
            LocalAuthorityAttainment8YearByYear = response.Attainment8YearByYear.LocalAuthority,
            EnglandAttainment8YearByYear = response.Attainment8YearByYear.England,
            SchoolEngMathsThreeYearAverage = defaultEngMaths.ThreeYearAverage.SchoolValue,
            SimilarSchoolsEngMathsThreeYearAverage = defaultEngMaths.ThreeYearAverage.SimilarSchoolsValue,
            LocalAuthorityEngMathsThreeYearAverage = defaultEngMaths.ThreeYearAverage.LocalAuthorityValue,
            EnglandEngMathsThreeYearAverage = defaultEngMaths.ThreeYearAverage.EnglandValue,
            EngMathsTopPerformers = MapTopPerformers(defaultEngMaths.TopPerformers, Ks4HeadlineMeasuresPageViewModel.DisplayPercent),
            SchoolEngMathsYearByYear = defaultEngMaths.YearByYear.School,
            SimilarSchoolsEngMathsYearByYear = defaultEngMaths.YearByYear.SimilarSchools,
            LocalAuthorityEngMathsYearByYear = defaultEngMaths.YearByYear.LocalAuthority,
            EnglandEngMathsYearByYear = defaultEngMaths.YearByYear.England,
            SchoolDestinationsThreeYearAverage = defaultDestinations.ThreeYearAverage.SchoolValue,
            SimilarSchoolsDestinationsThreeYearAverage = defaultDestinations.ThreeYearAverage.SimilarSchoolsValue,
            LocalAuthorityDestinationsThreeYearAverage = defaultDestinations.ThreeYearAverage.LocalAuthorityValue,
            EnglandDestinationsThreeYearAverage = defaultDestinations.ThreeYearAverage.EnglandValue,
            DestinationsTopPerformers = MapTopPerformers(defaultDestinations.TopPerformers, Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent),
            SchoolDestinationsYearByYear = defaultDestinations.YearByYear.School,
            SimilarSchoolsDestinationsYearByYear = defaultDestinations.YearByYear.SimilarSchools,
            LocalAuthorityDestinationsYearByYear = defaultDestinations.YearByYear.LocalAuthority,
            EnglandDestinationsYearByYear = defaultDestinations.YearByYear.England
        };
    }

    private static IReadOnlyList<TopPerformerRow> MapTopPerformers(
        IReadOnlyList<SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases.Ks4TopPerformer> topPerformers,
        Func<decimal?, string> formatter) =>
        topPerformers
            .Select(x => new TopPerformerRow(x.Rank, x.Urn, x.Name, x.Value, formatter(x.Value)))
            .ToList()
            .AsReadOnly();

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
