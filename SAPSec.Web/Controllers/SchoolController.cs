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
        var model = BuildKs4HeadlineMeasuresViewModel(
            await _getSchoolKs4HeadlineMeasures.Execute(new GetSchoolKs4HeadlineMeasuresRequest(urn)));

        var isGrade5 = grade == "5";

        return Json(new
        {
            grade = isGrade5 ? "5" : "4",
            bar = new decimal?[]
            {
                isGrade5 ? model.SchoolEngMaths59ThreeYearAverage : model.SchoolEngMaths49ThreeYearAverage,
                isGrade5 ? model.SimilarSchoolsEngMaths59ThreeYearAverage : model.SimilarSchoolsEngMaths49ThreeYearAverage,
                isGrade5 ? model.LocalAuthorityEngMaths59ThreeYearAverage : model.LocalAuthorityEngMaths49ThreeYearAverage,
                isGrade5 ? model.EnglandEngMaths59ThreeYearAverage : model.EnglandEngMaths49ThreeYearAverage
            },
            line = new
            {
                thisSchool = SeriesToArray(isGrade5 ? model.SchoolEngMaths59YearByYear : model.SchoolEngMaths49YearByYear),
                similarSchools = SeriesToArray(isGrade5 ? model.SimilarSchoolsEngMaths59YearByYear : model.SimilarSchoolsEngMaths49YearByYear),
                localAuthority = SeriesToArray(isGrade5 ? model.LocalAuthorityEngMaths59YearByYear : model.LocalAuthorityEngMaths49YearByYear),
                england = SeriesToArray(isGrade5 ? model.EnglandEngMaths59YearByYear : model.EnglandEngMaths49YearByYear)
            },
            table = new
            {
                thisSchool = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent((isGrade5 ? model.SchoolEngMaths59YearByYear : model.SchoolEngMaths49YearByYear).Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent((isGrade5 ? model.SchoolEngMaths59YearByYear : model.SchoolEngMaths49YearByYear).Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent((isGrade5 ? model.SchoolEngMaths59YearByYear : model.SchoolEngMaths49YearByYear).Current),
                    isGrade5 ? model.SchoolEngMaths59Display : model.SchoolEngMaths49Display
                },
                similarSchools = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent((isGrade5 ? model.SimilarSchoolsEngMaths59YearByYear : model.SimilarSchoolsEngMaths49YearByYear).Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent((isGrade5 ? model.SimilarSchoolsEngMaths59YearByYear : model.SimilarSchoolsEngMaths49YearByYear).Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent((isGrade5 ? model.SimilarSchoolsEngMaths59YearByYear : model.SimilarSchoolsEngMaths49YearByYear).Current),
                    isGrade5 ? model.SimilarSchoolsEngMaths59Display : model.SimilarSchoolsEngMaths49Display
                },
                localAuthority = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent((isGrade5 ? model.LocalAuthorityEngMaths59YearByYear : model.LocalAuthorityEngMaths49YearByYear).Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent((isGrade5 ? model.LocalAuthorityEngMaths59YearByYear : model.LocalAuthorityEngMaths49YearByYear).Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent((isGrade5 ? model.LocalAuthorityEngMaths59YearByYear : model.LocalAuthorityEngMaths49YearByYear).Current),
                    isGrade5 ? model.LocalAuthorityEngMaths59Display : model.LocalAuthorityEngMaths49Display
                },
                england = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent((isGrade5 ? model.EnglandEngMaths59YearByYear : model.EnglandEngMaths49YearByYear).Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent((isGrade5 ? model.EnglandEngMaths59YearByYear : model.EnglandEngMaths49YearByYear).Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayPercent((isGrade5 ? model.EnglandEngMaths59YearByYear : model.EnglandEngMaths49YearByYear).Current),
                    isGrade5 ? model.EnglandEngMaths59Display : model.EnglandEngMaths49Display
                }
            },
            topPerformers = (isGrade5 ? model.EngMaths59TopPerformers : model.EngMaths49TopPerformers)
                .Select(x => new { x.Rank, x.Urn, x.Name, x.DisplayValue })
        });
    }

    [HttpGet]
    [Route("ks4-destinations-data")]
    public async Task<IActionResult> Ks4DestinationsData(string urn, string destination = "all")
    {
        var model = BuildKs4HeadlineMeasuresViewModel(
            await _getSchoolKs4HeadlineMeasures.Execute(new GetSchoolKs4HeadlineMeasuresRequest(urn)));

        var normalizedDestination = destination?.ToLowerInvariant() switch
        {
            "education" => "education",
            "employment" => "employment",
            _ => "all"
        };

        var schoolSeries = normalizedDestination switch
        {
            "education" => model.SchoolDestinationsEducationYearByYear,
            "employment" => model.SchoolDestinationsEmploymentYearByYear,
            _ => model.SchoolDestinationsYearByYear
        };
        var similarSeries = normalizedDestination switch
        {
            "education" => model.SimilarSchoolsDestinationsEducationYearByYear,
            "employment" => model.SimilarSchoolsDestinationsEmploymentYearByYear,
            _ => model.SimilarSchoolsDestinationsYearByYear
        };
        var localAuthoritySeries = normalizedDestination switch
        {
            "education" => model.LocalAuthorityDestinationsEducationYearByYear,
            "employment" => model.LocalAuthorityDestinationsEmploymentYearByYear,
            _ => model.LocalAuthorityDestinationsYearByYear
        };
        var englandSeries = normalizedDestination switch
        {
            "education" => model.EnglandDestinationsEducationYearByYear,
            "employment" => model.EnglandDestinationsEmploymentYearByYear,
            _ => model.EnglandDestinationsYearByYear
        };

        var schoolAverage = normalizedDestination switch
        {
            "education" => model.SchoolDestinationsEducationThreeYearAverage,
            "employment" => model.SchoolDestinationsEmploymentThreeYearAverage,
            _ => model.SchoolDestinationsThreeYearAverage
        };
        var similarAverage = normalizedDestination switch
        {
            "education" => model.SimilarSchoolsDestinationsEducationThreeYearAverage,
            "employment" => model.SimilarSchoolsDestinationsEmploymentThreeYearAverage,
            _ => model.SimilarSchoolsDestinationsThreeYearAverage
        };
        var localAuthorityAverage = normalizedDestination switch
        {
            "education" => model.LocalAuthorityDestinationsEducationThreeYearAverage,
            "employment" => model.LocalAuthorityDestinationsEmploymentThreeYearAverage,
            _ => model.LocalAuthorityDestinationsThreeYearAverage
        };
        var englandAverage = normalizedDestination switch
        {
            "education" => model.EnglandDestinationsEducationThreeYearAverage,
            "employment" => model.EnglandDestinationsEmploymentThreeYearAverage,
            _ => model.EnglandDestinationsThreeYearAverage
        };

        return Json(new
        {
            destination = normalizedDestination,
            bar = new decimal?[] { schoolAverage, similarAverage, localAuthorityAverage, englandAverage },
            line = new
            {
                thisSchool = SeriesToArray(schoolSeries),
                similarSchools = SeriesToArray(similarSeries),
                localAuthority = SeriesToArray(localAuthoritySeries),
                england = SeriesToArray(englandSeries)
            },
            table = new
            {
                thisSchool = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(schoolSeries.Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(schoolSeries.Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(schoolSeries.Current),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(schoolAverage)
                },
                similarSchools = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(similarSeries.Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(similarSeries.Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(similarSeries.Current),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(similarAverage)
                },
                localAuthority = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(localAuthoritySeries.Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(localAuthoritySeries.Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(localAuthoritySeries.Current),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(localAuthorityAverage)
                },
                england = new[]
                {
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(englandSeries.Previous2),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(englandSeries.Previous),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(englandSeries.Current),
                    Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent(englandAverage)
                }
            },
            topPerformers = (normalizedDestination switch
            {
                "education" => model.DestinationsEducationTopPerformers,
                "employment" => model.DestinationsEmploymentTopPerformers,
                _ => model.DestinationsTopPerformers
            }).Select(x => new { x.Rank, x.Urn, x.Name, x.DisplayValue })
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
        GetSchoolKs4HeadlineMeasuresResponse response) =>
        new()
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
            SchoolEngMaths49ThreeYearAverage = response.EngMaths49ThreeYearAverage.SchoolValue,
            SimilarSchoolsEngMaths49ThreeYearAverage = response.EngMaths49ThreeYearAverage.SimilarSchoolsValue,
            LocalAuthorityEngMaths49ThreeYearAverage = response.EngMaths49ThreeYearAverage.LocalAuthorityValue,
            EnglandEngMaths49ThreeYearAverage = response.EngMaths49ThreeYearAverage.EnglandValue,
            EngMaths49TopPerformers = MapTopPerformers(response.EngMaths49TopPerformers, Ks4HeadlineMeasuresPageViewModel.DisplayPercent),
            SchoolEngMaths49YearByYear = response.EngMaths49YearByYear.School,
            SimilarSchoolsEngMaths49YearByYear = response.EngMaths49YearByYear.SimilarSchools,
            LocalAuthorityEngMaths49YearByYear = response.EngMaths49YearByYear.LocalAuthority,
            EnglandEngMaths49YearByYear = response.EngMaths49YearByYear.England,
            SchoolEngMaths59ThreeYearAverage = response.EngMaths59ThreeYearAverage.SchoolValue,
            SimilarSchoolsEngMaths59ThreeYearAverage = response.EngMaths59ThreeYearAverage.SimilarSchoolsValue,
            LocalAuthorityEngMaths59ThreeYearAverage = response.EngMaths59ThreeYearAverage.LocalAuthorityValue,
            EnglandEngMaths59ThreeYearAverage = response.EngMaths59ThreeYearAverage.EnglandValue,
            EngMaths59TopPerformers = MapTopPerformers(response.EngMaths59TopPerformers, Ks4HeadlineMeasuresPageViewModel.DisplayPercent),
            SchoolEngMaths59YearByYear = response.EngMaths59YearByYear.School,
            SimilarSchoolsEngMaths59YearByYear = response.EngMaths59YearByYear.SimilarSchools,
            LocalAuthorityEngMaths59YearByYear = response.EngMaths59YearByYear.LocalAuthority,
            EnglandEngMaths59YearByYear = response.EngMaths59YearByYear.England,
            SchoolDestinationsThreeYearAverage = response.DestinationsThreeYearAverage.SchoolValue,
            SimilarSchoolsDestinationsThreeYearAverage = response.DestinationsThreeYearAverage.SimilarSchoolsValue,
            LocalAuthorityDestinationsThreeYearAverage = response.DestinationsThreeYearAverage.LocalAuthorityValue,
            EnglandDestinationsThreeYearAverage = response.DestinationsThreeYearAverage.EnglandValue,
            DestinationsTopPerformers = MapTopPerformers(response.DestinationsTopPerformers, Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent),
            SchoolDestinationsYearByYear = response.DestinationsYearByYear.School,
            SimilarSchoolsDestinationsYearByYear = response.DestinationsYearByYear.SimilarSchools,
            LocalAuthorityDestinationsYearByYear = response.DestinationsYearByYear.LocalAuthority,
            EnglandDestinationsYearByYear = response.DestinationsYearByYear.England,
            SchoolDestinationsEducationThreeYearAverage = response.DestinationsEducationThreeYearAverage.SchoolValue,
            SimilarSchoolsDestinationsEducationThreeYearAverage = response.DestinationsEducationThreeYearAverage.SimilarSchoolsValue,
            LocalAuthorityDestinationsEducationThreeYearAverage = response.DestinationsEducationThreeYearAverage.LocalAuthorityValue,
            EnglandDestinationsEducationThreeYearAverage = response.DestinationsEducationThreeYearAverage.EnglandValue,
            DestinationsEducationTopPerformers = MapTopPerformers(response.DestinationsEducationTopPerformers, Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent),
            SchoolDestinationsEducationYearByYear = response.DestinationsEducationYearByYear.School,
            SimilarSchoolsDestinationsEducationYearByYear = response.DestinationsEducationYearByYear.SimilarSchools,
            LocalAuthorityDestinationsEducationYearByYear = response.DestinationsEducationYearByYear.LocalAuthority,
            EnglandDestinationsEducationYearByYear = response.DestinationsEducationYearByYear.England,
            SchoolDestinationsEmploymentThreeYearAverage = response.DestinationsEmploymentThreeYearAverage.SchoolValue,
            SimilarSchoolsDestinationsEmploymentThreeYearAverage = response.DestinationsEmploymentThreeYearAverage.SimilarSchoolsValue,
            LocalAuthorityDestinationsEmploymentThreeYearAverage = response.DestinationsEmploymentThreeYearAverage.LocalAuthorityValue,
            EnglandDestinationsEmploymentThreeYearAverage = response.DestinationsEmploymentThreeYearAverage.EnglandValue,
            DestinationsEmploymentTopPerformers = MapTopPerformers(response.DestinationsEmploymentTopPerformers, Ks4HeadlineMeasuresPageViewModel.DisplayWholePercent),
            SchoolDestinationsEmploymentYearByYear = response.DestinationsEmploymentYearByYear.School,
            SimilarSchoolsDestinationsEmploymentYearByYear = response.DestinationsEmploymentYearByYear.SimilarSchools,
            LocalAuthorityDestinationsEmploymentYearByYear = response.DestinationsEmploymentYearByYear.LocalAuthority,
            EnglandDestinationsEmploymentYearByYear = response.DestinationsEmploymentYearByYear.England
        };

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
