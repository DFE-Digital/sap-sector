using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.Ks4CoreSubjects.UseCases;
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
    private readonly GetSchoolKs4CoreSubjects _getSchoolKs4CoreSubjects;
    private readonly GetFilteredSchoolKs4CoreSubject _getFilteredSchoolKs4CoreSubject;
    private readonly GetAttendanceMeasures _getAttendanceMeasures;
    private readonly ILogger<SchoolController> _logger;

    public SchoolController(
        ISchoolDetailsService schoolDetailsService,
        GetSchoolKs4HeadlineMeasures getSchoolKs4HeadlineMeasures,
        GetSchoolKs4CoreSubjects getSchoolKs4CoreSubjects,
        GetFilteredSchoolKs4CoreSubject getFilteredSchoolKs4CoreSubject,
        GetAttendanceMeasures getAttendanceMeasures,
        ILogger<SchoolController> logger)
    {
        _schoolDetailsService = schoolDetailsService;
        _getSchoolKs4HeadlineMeasures = getSchoolKs4HeadlineMeasures;
        _getSchoolKs4CoreSubjects = getSchoolKs4CoreSubjects;
        _getFilteredSchoolKs4CoreSubject = getFilteredSchoolKs4CoreSubject;
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
        var response = await _getSchoolKs4CoreSubjects.Execute(new GetSchoolKs4CoreSubjectsRequest(urn));
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(response.SchoolDetails);
        return View(BuildKs4CoreSubjectsViewModel(response));
    }

    [HttpGet]
    [Route("ks4-core-subjects/data")]
    public async Task<IActionResult> Ks4CoreSubjectsData(string urn, string subject = "english-language", string grade = "4")
    {
        GetFilteredSchoolKs4CoreSubjectResponse filteredSubject;
        try
        {
            filteredSubject = await _getFilteredSchoolKs4CoreSubject.Execute(new GetFilteredSchoolKs4CoreSubjectRequest(urn, subject, grade));
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest(new { error = "Invalid KS4 core subjects filter." });
        }

        var selectedSubject = filteredSubject.Selection;

        return Json(new
        {
            subject = filteredSubject.Subject.ToSubjectValue(),
            grade = filteredSubject.Grade.ToFilterValue(),
            bar = new decimal?[]
            {
                selectedSubject.ThreeYearAverage.SchoolValue,
                selectedSubject.ThreeYearAverage.SimilarSchoolsValue,
                selectedSubject.ThreeYearAverage.LocalAuthorityValue,
                selectedSubject.ThreeYearAverage.EnglandValue
            },
            line = new
            {
                thisSchool = SeriesToArray(selectedSubject.YearByYear.School),
                similarSchools = SeriesToArray(selectedSubject.YearByYear.SimilarSchools),
                localAuthority = SeriesToArray(selectedSubject.YearByYear.LocalAuthority),
                england = SeriesToArray(selectedSubject.YearByYear.England)
            },
            table = new
            {
                thisSchool = new[]
                {
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.School.Previous2),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.School.Previous),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.School.Current),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.ThreeYearAverage.SchoolValue)
                },
                similarSchools = new[]
                {
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.SimilarSchools.Previous2),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.SimilarSchools.Previous),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.SimilarSchools.Current),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.ThreeYearAverage.SimilarSchoolsValue)
                },
                localAuthority = new[]
                {
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.LocalAuthority.Previous2),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.LocalAuthority.Previous),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.LocalAuthority.Current),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.ThreeYearAverage.LocalAuthorityValue)
                },
                england = new[]
                {
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.England.Previous2),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.England.Previous),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.England.Current),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.ThreeYearAverage.EnglandValue)
                }
            },
            topPerformers = selectedSubject.TopPerformers
                .Select(x => new
                {
                    x.Rank,
                    x.Urn,
                    x.Name,
                    DisplayValue = Ks4CoreSubjectsPageViewModel.DisplayWholePercent(x.Value)
                })
        });
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

    private static Ks4CoreSubjectsPageViewModel BuildKs4CoreSubjectsViewModel(
        GetSchoolKs4CoreSubjectsResponse response)
    {
        var defaultEnglishLanguage = SchoolKs4CoreSubjectSelection.From(
            response,
            SchoolKs4CoreSubject.EnglishLanguage,
            SchoolKs4CoreSubjectGradeFilter.Grade4);
        var defaultEnglishLiterature = SchoolKs4CoreSubjectSelection.From(
            response,
            SchoolKs4CoreSubject.EnglishLiterature,
            SchoolKs4CoreSubjectGradeFilter.Grade4);
        var defaultBiology = SchoolKs4CoreSubjectSelection.From(
            response,
            SchoolKs4CoreSubject.Biology,
            SchoolKs4CoreSubjectGradeFilter.Grade4);
        var defaultChemistry = SchoolKs4CoreSubjectSelection.From(
            response,
            SchoolKs4CoreSubject.Chemistry,
            SchoolKs4CoreSubjectGradeFilter.Grade4);
        var defaultPhysics = SchoolKs4CoreSubjectSelection.From(
            response,
            SchoolKs4CoreSubject.Physics,
            SchoolKs4CoreSubjectGradeFilter.Grade4);
        var defaultMaths = SchoolKs4CoreSubjectSelection.From(
            response,
            SchoolKs4CoreSubject.Maths,
            SchoolKs4CoreSubjectGradeFilter.Grade4);
        var defaultCombinedScienceDoubleAward = SchoolKs4CoreSubjectSelection.From(
            response,
            SchoolKs4CoreSubject.CombinedScienceDoubleAward,
            SchoolKs4CoreSubjectGradeFilter.Grade4);

        return new()
        {
            SchoolDetails = response.SchoolDetails,
            SimilarSchoolsCount = response.SimilarSchoolsCount,
            EnglishLanguage = MapCoreSubjectSection(defaultEnglishLanguage),
            EnglishLiterature = MapCoreSubjectSection(defaultEnglishLiterature),
            Biology = MapCoreSubjectSection(defaultBiology),
            Chemistry = MapCoreSubjectSection(defaultChemistry),
            Physics = MapCoreSubjectSection(defaultPhysics),
            Maths = MapCoreSubjectSection(defaultMaths),
            CombinedScienceDoubleAward = MapCoreSubjectSection(defaultCombinedScienceDoubleAward)
        };
    }

    private static Ks4CoreSubjectsPageViewModel.SubjectSection MapCoreSubjectSection(
        SchoolKs4CoreSubjectSelection selection) =>
        new(
            selection.ThreeYearAverage.SchoolValue,
            selection.ThreeYearAverage.SimilarSchoolsValue,
            selection.ThreeYearAverage.LocalAuthorityValue,
            selection.ThreeYearAverage.EnglandValue,
            selection.TopPerformers
                .Select(x => new Ks4CoreSubjectsPageViewModel.TopPerformerRow(
                    x.Rank,
                    x.Urn,
                    x.Name,
                    x.Value,
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(x.Value)))
                .ToList()
                .AsReadOnly(),
            selection.YearByYear.School,
            selection.YearByYear.SimilarSchools,
            selection.YearByYear.LocalAuthority,
            selection.YearByYear.England);

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
