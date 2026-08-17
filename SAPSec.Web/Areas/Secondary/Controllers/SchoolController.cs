using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.Secondary;
using SAPSec.Core.Features.Secondary.Ks4CoreSubjects_Old.UseCases;
using SAPSec.Core.Features.Secondary.Ks4HeadlineMeasures_Old.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.UseCases;
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
    private readonly IUseCase<Core.Features.Secondary.GetSchoolKs4CoreSubjectsRequest, Core.Features.Secondary.GetSchoolKs4CoreSubjectsResponse> _getSchoolKs4CoreSubjectsUseCase;
    private readonly GetSchoolKs4CoreSubjects _getSchoolKs4CoreSubjects;
    private readonly GetFilteredSchoolKs4CoreSubject _getFilteredSchoolKs4CoreSubject;
    private readonly GetAttendanceMeasures _getAttendanceMeasures;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly IRequestSchoolAccessor _requestSchoolAccessor;
    private readonly ILogger<SchoolController> _logger;

    public SchoolController(
        IUseCase<GetSchoolKs4HeadlineMeasuresRequest, GetSchoolKs4HeadlineMeasuresResponse> getSchoolKs4HeadlineMeasuresUseCase,
        IUseCase<Core.Features.Secondary.GetSchoolKs4CoreSubjectsRequest, Core.Features.Secondary.GetSchoolKs4CoreSubjectsResponse> getSchoolKs4CoreSubjectsUseCase,
        GetSchoolKs4CoreSubjects getSchoolKs4CoreSubjects,
        GetFilteredSchoolKs4CoreSubject getFilteredSchoolKs4CoreSubject,
        GetAttendanceMeasures getAttendanceMeasures,
        IFeatureFlagService featureFlagService,
        IRequestSchoolAccessor requestSchoolAccessor,
        ILogger<SchoolController> logger)
    {
        _getSchoolKs4HeadlineMeasuresUseCase = getSchoolKs4HeadlineMeasuresUseCase;
        _getSchoolKs4CoreSubjectsUseCase = getSchoolKs4CoreSubjectsUseCase;
        _getSchoolKs4CoreSubjects = getSchoolKs4CoreSubjects;
        _getFilteredSchoolKs4CoreSubject = getFilteredSchoolKs4CoreSubject;
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
            Measures = response.Measures.Select(m => MeasureViewModel.FromSecondaryMeasure(m, response.School)).ToArray()
        };

        return View(model);
    }

    [HttpGet]
    [Route("ks4-core-subjects-old")]
    public async Task<IActionResult> Ks4CoreSubjectsOld(string urn)
    {
        var response = await _getSchoolKs4CoreSubjects.Execute(new Core.Features.Secondary.Ks4CoreSubjects_Old.UseCases.GetSchoolKs4CoreSubjectsRequest(urn));
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewDataAsync(response.SchoolDetails);
        return View(BuildKs4CoreSubjectsViewModel(response));
    }

    [HttpGet]
    [Route("ks4-core-subjects-old/data")]
    public async Task<IActionResult> Ks4CoreSubjectsOldData(string urn, string subject = "english-language", string grade = "4")
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
                selectedSubject.YearByYear.School.Current,
                selectedSubject.YearByYear.SimilarSchools.Current,
                selectedSubject.YearByYear.LocalAuthority.Current,
                selectedSubject.YearByYear.England.Current
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
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.School.Current)
                },
                similarSchools = new[]
                {
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.SimilarSchools.Previous2),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.SimilarSchools.Previous),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.SimilarSchools.Current)
                },
                localAuthority = new[]
                {
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.LocalAuthority.Previous2),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.LocalAuthority.Previous),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.LocalAuthority.Current)
                },
                england = new[]
                {
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.England.Previous2),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.England.Previous),
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(selectedSubject.YearByYear.England.Current)
                }
            },
            topPerformers = selectedSubject.TopPerformers
                .Select(x => new
                {
                    x.Rank,
                    x.Urn,
                    x.Name,
                    x.IsCurrentSchool,
                    DisplayValue = Ks4CoreSubjectsPageViewModel.DisplayWholePercent(x.Value)
                })
        });
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
        ViewData["SchoolDetails"] = school;

        if (Url is null)
        {
            return;
        }

        ViewData["SchoolNavigation"] = SchoolSideNavigationViewModel.CreateSecondary(
            Url,
            school.Urn,
            ControllerContext.ActionDescriptor.ActionName);
    }

    private static decimal?[] SeriesToArray(Ks4HeadlineMeasureSeries series) =>
        [series.Previous2, series.Previous, series.Current];

    private static Ks4CoreSubjectsPageViewModel BuildKs4CoreSubjectsViewModel(
        Core.Features.Secondary.Ks4CoreSubjects_Old.UseCases.GetSchoolKs4CoreSubjectsResponse response)
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
                    Ks4CoreSubjectsPageViewModel.DisplayWholePercent(x.Value),
                    x.IsCurrentSchool))
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
