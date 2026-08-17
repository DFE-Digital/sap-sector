using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.Secondary.Ks4CoreSubjects.UseCases;
using SAPSec.Core.Features.Secondary.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.Formatters;
using SAPSec.Web.ViewModels;
using System.Globalization;

namespace SAPSec.Web.Areas.Secondary.Controllers;

[Area("Secondary")]
[Route("school/secondary/{urn}/view-similar-schools/{similarSchoolUrn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Secondary, "urn", "similarSchoolUrn")]
public class SimilarSchoolsComparisonController : Controller
{
    private readonly GetSimilarSchoolDetails _getSimilarSchoolDetails;
    private readonly GetAttendanceMeasures _getAttendanceMeasures;
    private readonly GetSchoolKs4CoreSubjects _getSchoolKs4CoreSubjects;
    private readonly GetFilteredSchoolKs4CoreSubject _getFilteredSchoolKs4CoreSubject;
    private readonly GetKs4HeadlineMeasures _getKs4HeadlineMeasures;
    private readonly GetCharacteristicsComparison _getCharacteristicsComparison;
    private readonly ILogger<SimilarSchoolsComparisonController> _logger;
    private readonly ICharacteristicsComparisonFormatter _characteristicsFormatter;

    public SimilarSchoolsComparisonController(
        GetSimilarSchoolDetails getSimilarSchoolDetails,
        GetAttendanceMeasures getAttendanceMeasures,
        GetSchoolKs4CoreSubjects getSchoolKs4CoreSubjects,
        GetFilteredSchoolKs4CoreSubject getFilteredSchoolKs4CoreSubject,
        GetKs4HeadlineMeasures getKs4HeadlineMeasures,
        GetCharacteristicsComparison getCharacteristicsComparison,
        ICharacteristicsComparisonFormatter characteristicsFormatter,
        ILogger<SimilarSchoolsComparisonController> logger)
    {
        _getSimilarSchoolDetails =
            getSimilarSchoolDetails ?? throw new ArgumentNullException(nameof(getSimilarSchoolDetails));
        _getAttendanceMeasures = getAttendanceMeasures ?? throw new ArgumentNullException(nameof(getAttendanceMeasures));
        _getSchoolKs4CoreSubjects = getSchoolKs4CoreSubjects ?? throw new ArgumentNullException(nameof(getSchoolKs4CoreSubjects));
        _getFilteredSchoolKs4CoreSubject = getFilteredSchoolKs4CoreSubject ?? throw new ArgumentNullException(nameof(getFilteredSchoolKs4CoreSubject));
        _getKs4HeadlineMeasures = getKs4HeadlineMeasures ?? throw new ArgumentNullException(nameof(getKs4HeadlineMeasures));
        _getCharacteristicsComparison = getCharacteristicsComparison ??
                                        throw new ArgumentNullException(nameof(getCharacteristicsComparison));
        _characteristicsFormatter = characteristicsFormatter ??
                                    throw new ArgumentNullException(nameof(characteristicsFormatter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public Task<IActionResult> Index(
        string urn,
        string similarSchoolUrn,
        [FromQuery(Name = "similarityCalculation")] string? similarityCalculation = null) =>
        Similarity(urn, similarSchoolUrn, similarityCalculation);

    [HttpGet]
    [Route("compare-similarity")]
    public async Task<IActionResult> Similarity(
        string urn,
        string similarSchoolUrn,
        [FromQuery(Name = "similarityCalculation")] string? similarityCalculation = null)
    {
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);

        var modelResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn, similarityCalculation);
        if (modelResult.Result != null)
            return modelResult.Result;

        SetComparisonSchoolViewData(modelResult.Model!);
        return View("Similarity", modelResult.Model);
    }

    [HttpGet]
    [Route("compare-ks4-headline-measures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(
        string urn,
        string similarSchoolUrn,
        [FromQuery(Name = "similarityCalculation")] string? similarityCalculation = null)
    {
        var modelResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn, similarityCalculation);
        if (modelResult.Result != null)
            return modelResult.Result;

        var thisSchoolKs4 = await _getKs4HeadlineMeasures.Execute(new GetKs4HeadlineMeasuresRequest(urn));
        var selectedSchoolKs4 = await _getKs4HeadlineMeasures.Execute(new GetKs4HeadlineMeasuresRequest(similarSchoolUrn));

        var model = modelResult.Model!;
        model.ThisSchoolAttainment8ThreeYearAverage = thisSchoolKs4?.Attainment8ThreeYearAverage.SchoolValue;
        model.SelectedSchoolAttainment8ThreeYearAverage = selectedSchoolKs4?.Attainment8ThreeYearAverage.SchoolValue;
        model.EnglandAttainment8ThreeYearAverage =
            thisSchoolKs4?.Attainment8ThreeYearAverage.EnglandValue
            ?? selectedSchoolKs4?.Attainment8ThreeYearAverage.EnglandValue;
        model.ThisSchoolAttainment8YearByYear = thisSchoolKs4?.Attainment8YearByYear.School;
        model.SelectedSchoolAttainment8YearByYear = selectedSchoolKs4?.Attainment8YearByYear.School;
        model.EnglandAttainment8YearByYear =
            thisSchoolKs4?.Attainment8YearByYear.England
            ?? selectedSchoolKs4?.Attainment8YearByYear.England;
        model.ThisSchoolEngMaths49ThreeYearAverage = thisSchoolKs4?.EngMaths49ThreeYearAverage.SchoolValue;
        model.SelectedSchoolEngMaths49ThreeYearAverage = selectedSchoolKs4?.EngMaths49ThreeYearAverage.SchoolValue;
        model.EnglandEngMaths49ThreeYearAverage =
            thisSchoolKs4?.EngMaths49ThreeYearAverage.EnglandValue
            ?? selectedSchoolKs4?.EngMaths49ThreeYearAverage.EnglandValue;
        model.ThisSchoolEngMaths49YearByYear = thisSchoolKs4?.EngMaths49YearByYear.School;
        model.SelectedSchoolEngMaths49YearByYear = selectedSchoolKs4?.EngMaths49YearByYear.School;
        model.EnglandEngMaths49YearByYear =
            thisSchoolKs4?.EngMaths49YearByYear.England
            ?? selectedSchoolKs4?.EngMaths49YearByYear.England;
        model.ThisSchoolEngMaths59ThreeYearAverage = thisSchoolKs4?.EngMaths59ThreeYearAverage.SchoolValue;
        model.SelectedSchoolEngMaths59ThreeYearAverage = selectedSchoolKs4?.EngMaths59ThreeYearAverage.SchoolValue;
        model.EnglandEngMaths59ThreeYearAverage =
            thisSchoolKs4?.EngMaths59ThreeYearAverage.EnglandValue
            ?? selectedSchoolKs4?.EngMaths59ThreeYearAverage.EnglandValue;
        model.ThisSchoolEngMaths59YearByYear = thisSchoolKs4?.EngMaths59YearByYear.School;
        model.SelectedSchoolEngMaths59YearByYear = selectedSchoolKs4?.EngMaths59YearByYear.School;
        model.EnglandEngMaths59YearByYear =
            thisSchoolKs4?.EngMaths59YearByYear.England
            ?? selectedSchoolKs4?.EngMaths59YearByYear.England;
        model.ThisSchoolDestinationsThreeYearAverage = thisSchoolKs4?.DestinationsThreeYearAverage.SchoolValue;
        model.SelectedSchoolDestinationsThreeYearAverage = selectedSchoolKs4?.DestinationsThreeYearAverage.SchoolValue;
        model.EnglandDestinationsThreeYearAverage =
            thisSchoolKs4?.DestinationsThreeYearAverage.EnglandValue
            ?? selectedSchoolKs4?.DestinationsThreeYearAverage.EnglandValue;
        model.ThisSchoolDestinationsYearByYear = thisSchoolKs4?.DestinationsYearByYear.School;
        model.SelectedSchoolDestinationsYearByYear = selectedSchoolKs4?.DestinationsYearByYear.School;
        model.EnglandDestinationsYearByYear =
            thisSchoolKs4?.DestinationsYearByYear.England
            ?? selectedSchoolKs4?.DestinationsYearByYear.England;

        SetComparisonSchoolViewData(model);
        return View(model);
    }

    [HttpGet]
    [Route("ks4-headline-measures/data")]
    public async Task<IActionResult> Ks4HeadlineMeasuresData(string urn, string similarSchoolUrn, string grade = "4")
    {
        if (string.IsNullOrWhiteSpace(urn) || string.IsNullOrWhiteSpace(similarSchoolUrn))
        {
            return BadRequest(new { error = "Missing route parameters." });
        }

        var normalizedGrade = grade == "5" ? "5" : "4";
        var thisSchoolKs4 = await _getKs4HeadlineMeasures.Execute(new GetKs4HeadlineMeasuresRequest(urn));
        var selectedSchoolKs4 = await _getKs4HeadlineMeasures.Execute(new GetKs4HeadlineMeasuresRequest(similarSchoolUrn));

        var isGrade5 = normalizedGrade == "5";

        var thisSchoolSeries = isGrade5
            ? thisSchoolKs4?.EngMaths59YearByYear.School
            : thisSchoolKs4?.EngMaths49YearByYear.School;
        var selectedSchoolSeries = isGrade5
            ? selectedSchoolKs4?.EngMaths59YearByYear.School
            : selectedSchoolKs4?.EngMaths49YearByYear.School;
        var englandSeries = isGrade5
            ? (thisSchoolKs4?.EngMaths59YearByYear.England ?? selectedSchoolKs4?.EngMaths59YearByYear.England)
            : (thisSchoolKs4?.EngMaths49YearByYear.England ?? selectedSchoolKs4?.EngMaths49YearByYear.England);

        return Json(new
        {
            grade = normalizedGrade,
            bar = new decimal?[]
            {
                thisSchoolSeries?.Current,
                selectedSchoolSeries?.Current,
                englandSeries?.Current
            },
            line = new
            {
                thisSchool = new decimal?[]
                {
                    thisSchoolSeries?.Previous2,
                    thisSchoolSeries?.Previous,
                    thisSchoolSeries?.Current
                },
                similarSchool = new decimal?[]
                {
                    selectedSchoolSeries?.Previous2,
                    selectedSchoolSeries?.Previous,
                    selectedSchoolSeries?.Current
                },
                england = new decimal?[]
                {
                    englandSeries?.Previous2,
                    englandSeries?.Previous,
                    englandSeries?.Current
                }
            },
            table = new
            {
                thisSchool = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(thisSchoolSeries?.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(thisSchoolSeries?.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(thisSchoolSeries?.Current)
                },
                similarSchool = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolSeries?.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolSeries?.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolSeries?.Current)
                },
                england = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandSeries?.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandSeries?.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandSeries?.Current)
                }
            }
        });
    }

    [HttpGet]
    [Route("ks4-destinations/data")]
    public async Task<IActionResult> Ks4DestinationsData(string urn, string similarSchoolUrn, string destination = "all")
    {
        if (string.IsNullOrWhiteSpace(urn) || string.IsNullOrWhiteSpace(similarSchoolUrn))
        {
            return BadRequest(new { error = "Missing route parameters." });
        }

        var normalizedDestination = NormalizeDestinationFilter(destination);
        var thisSchoolKs4 = await _getKs4HeadlineMeasures.Execute(new GetKs4HeadlineMeasuresRequest(urn));
        var selectedSchoolKs4 = await _getKs4HeadlineMeasures.Execute(new GetKs4HeadlineMeasuresRequest(similarSchoolUrn));

        var thisSchoolSeries = SelectDestinationsYearByYear(thisSchoolKs4, normalizedDestination)?.School;
        var selectedSchoolSeries = SelectDestinationsYearByYear(selectedSchoolKs4, normalizedDestination)?.School;
        var englandSeries =
            SelectDestinationsYearByYear(thisSchoolKs4, normalizedDestination)?.England
            ?? SelectDestinationsYearByYear(selectedSchoolKs4, normalizedDestination)?.England;

        return Json(new
        {
            destination = normalizedDestination,
            bar = new decimal?[]
            {
                thisSchoolSeries?.Current,
                selectedSchoolSeries?.Current,
                englandSeries?.Current
            },
            line = new
            {
                thisSchool = new decimal?[]
                {
                    thisSchoolSeries?.Previous2,
                    thisSchoolSeries?.Previous,
                    thisSchoolSeries?.Current
                },
                similarSchool = new decimal?[]
                {
                    selectedSchoolSeries?.Previous2,
                    selectedSchoolSeries?.Previous,
                    selectedSchoolSeries?.Current
                },
                england = new decimal?[]
                {
                    englandSeries?.Previous2,
                    englandSeries?.Previous,
                    englandSeries?.Current
                }
            },
            table = new
            {
                thisSchool = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(thisSchoolSeries?.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(thisSchoolSeries?.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(thisSchoolSeries?.Current)
                },
                similarSchool = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolSeries?.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolSeries?.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolSeries?.Current)
                },
                england = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandSeries?.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandSeries?.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandSeries?.Current)
                }
            }
        });
    }

    [HttpGet]
    [Route("compare-ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(
        string urn,
        string similarSchoolUrn,
        [FromQuery(Name = "similarityCalculation")] string? similarityCalculation = null)
    {
        var modelResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn, similarityCalculation);
        if (modelResult.Result != null)
            return modelResult.Result;

        var thisSchoolKs4 = await _getSchoolKs4CoreSubjects.Execute(new GetSchoolKs4CoreSubjectsRequest(urn));
        var selectedSchoolKs4 = await _getSchoolKs4CoreSubjects.Execute(new GetSchoolKs4CoreSubjectsRequest(similarSchoolUrn));

        var model = modelResult.Model!;
        model.EnglishLanguage = BuildComparisonCoreSubjectSection(thisSchoolKs4, selectedSchoolKs4, SchoolKs4CoreSubject.EnglishLanguage);
        model.EnglishLiterature = BuildComparisonCoreSubjectSection(thisSchoolKs4, selectedSchoolKs4, SchoolKs4CoreSubject.EnglishLiterature);
        model.Biology = BuildComparisonCoreSubjectSection(thisSchoolKs4, selectedSchoolKs4, SchoolKs4CoreSubject.Biology);
        model.Chemistry = BuildComparisonCoreSubjectSection(thisSchoolKs4, selectedSchoolKs4, SchoolKs4CoreSubject.Chemistry);
        model.Physics = BuildComparisonCoreSubjectSection(thisSchoolKs4, selectedSchoolKs4, SchoolKs4CoreSubject.Physics);
        model.Maths = BuildComparisonCoreSubjectSection(thisSchoolKs4, selectedSchoolKs4, SchoolKs4CoreSubject.Maths);
        model.CombinedScienceDoubleAward = BuildComparisonCoreSubjectSection(thisSchoolKs4, selectedSchoolKs4, SchoolKs4CoreSubject.CombinedScienceDoubleAward);

        SetComparisonSchoolViewData(model);
        return View(model);
    }

    [HttpGet]
    [Route("ks4-core-subjects/data")]
    public async Task<IActionResult> Ks4CoreSubjectsData(string urn, string similarSchoolUrn, string subject = "english-language", string grade = "4")
    {
        if (string.IsNullOrWhiteSpace(urn) || string.IsNullOrWhiteSpace(similarSchoolUrn))
        {
            return BadRequest(new { error = "Missing route parameters." });
        }

        GetFilteredSchoolKs4CoreSubjectResponse thisSchoolFilteredSubject;
        GetFilteredSchoolKs4CoreSubjectResponse selectedSchoolFilteredSubject;
        try
        {
            thisSchoolFilteredSubject = await _getFilteredSchoolKs4CoreSubject.Execute(new GetFilteredSchoolKs4CoreSubjectRequest(urn, subject, grade));
            selectedSchoolFilteredSubject = await _getFilteredSchoolKs4CoreSubject.Execute(new GetFilteredSchoolKs4CoreSubjectRequest(similarSchoolUrn, subject, grade));
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest(new { error = "Invalid KS4 core subjects filter." });
        }

        var thisSchoolSubject = thisSchoolFilteredSubject.Selection;
        var selectedSchoolSubject = selectedSchoolFilteredSubject.Selection;

        return Json(new
        {
            subject = thisSchoolFilteredSubject.Subject.ToSubjectValue(),
            grade = thisSchoolFilteredSubject.Grade.ToFilterValue(),
            bar = new decimal?[]
            {
                RoundWholePercentValue(thisSchoolSubject.YearByYear.School.Current),
                RoundWholePercentValue(selectedSchoolSubject.YearByYear.School.Current),
                RoundWholePercentValue((thisSchoolSubject.YearByYear.England ?? selectedSchoolSubject.YearByYear.England)?.Current)
            },
            line = new
            {
                thisSchool = new decimal?[]
                {
                    thisSchoolSubject.YearByYear.School.Previous2,
                    thisSchoolSubject.YearByYear.School.Previous,
                    thisSchoolSubject.YearByYear.School.Current
                },
                similarSchool = new decimal?[]
                {
                    selectedSchoolSubject.YearByYear.School.Previous2,
                    selectedSchoolSubject.YearByYear.School.Previous,
                    selectedSchoolSubject.YearByYear.School.Current
                },
                england = new decimal?[]
                {
                    (thisSchoolSubject.YearByYear.England ?? selectedSchoolSubject.YearByYear.England)?.Previous2,
                    (thisSchoolSubject.YearByYear.England ?? selectedSchoolSubject.YearByYear.England)?.Previous,
                    (thisSchoolSubject.YearByYear.England ?? selectedSchoolSubject.YearByYear.England)?.Current
                }
            },
            table = new
            {
                thisSchool = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(thisSchoolSubject.YearByYear.School.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(thisSchoolSubject.YearByYear.School.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(thisSchoolSubject.YearByYear.School.Current)
                },
                similarSchool = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolSubject.YearByYear.School.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolSubject.YearByYear.School.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolSubject.YearByYear.School.Current)
                },
                england = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent((thisSchoolSubject.YearByYear.England ?? selectedSchoolSubject.YearByYear.England)?.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent((thisSchoolSubject.YearByYear.England ?? selectedSchoolSubject.YearByYear.England)?.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent((thisSchoolSubject.YearByYear.England ?? selectedSchoolSubject.YearByYear.England)?.Current)
                }
            }
        });
    }

    private static decimal? RoundWholePercentValue(decimal? value) =>
        value.HasValue
            ? Math.Round(value.Value, 0, MidpointRounding.AwayFromZero)
            : null;

    [HttpGet]
    [Route("compare-attendance")]
    public async Task<IActionResult> Attendance(
        string urn,
        string similarSchoolUrn,
        [FromQuery(Name = "similarityCalculation")] string? similarityCalculation = null)
    {
        var modelResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn, similarityCalculation);
        if (modelResult.Result != null)
            return modelResult.Result;

        SetComparisonSchoolViewData(modelResult.Model!);
        return View(modelResult.Model);
    }

    [HttpGet]
    [Route("attendance-data")]
    public async Task<IActionResult> AttendanceData(
        string urn,
        string similarSchoolUrn,
        string absenceType = "overall")
    {
        if (string.IsNullOrWhiteSpace(urn) || string.IsNullOrWhiteSpace(similarSchoolUrn))
        {
            return BadRequest(new { error = "Missing route parameters." });
        }

        var normalizedAbsenceType = NormalizeAttendanceOption(absenceType, "overall", "persistent");

        var thisSchoolAttendance = await _getAttendanceMeasures.Execute(new GetSchoolAttendanceMeasuresRequest(urn));
        var similarSchoolAttendance = await _getAttendanceMeasures.Execute(new GetSchoolAttendanceMeasuresRequest(similarSchoolUrn));

        var isPersistentAbsence = normalizedAbsenceType == "persistent";
        var yearLabels = AcademicYearLabelConfig.AttendanceYearByYear;

        var thisSchoolSeries = isPersistentAbsence
            ? thisSchoolAttendance.PersistentAbsenceYearByYear.School
            : thisSchoolAttendance.OverallAbsenceYearByYear.School;
        var similarSchoolSeries = isPersistentAbsence
            ? similarSchoolAttendance.PersistentAbsenceYearByYear.School
            : similarSchoolAttendance.OverallAbsenceYearByYear.School;
        var englandSeries = isPersistentAbsence
            ? (thisSchoolAttendance.PersistentAbsenceYearByYear.England ?? similarSchoolAttendance.PersistentAbsenceYearByYear.England)
            : (thisSchoolAttendance.OverallAbsenceYearByYear.England ?? similarSchoolAttendance.OverallAbsenceYearByYear.England);

        return Json(new
        {
            absenceType = normalizedAbsenceType,
            years = yearLabels,
            bar = new decimal?[]
            {
                thisSchoolSeries.Current,
                similarSchoolSeries.Current,
                englandSeries?.Current
            },
            line = new
            {
                thisSchool = new decimal?[] { thisSchoolSeries.Previous2, thisSchoolSeries.Previous, thisSchoolSeries.Current },
                similarSchool = new decimal?[] { similarSchoolSeries.Previous2, similarSchoolSeries.Previous, similarSchoolSeries.Current },
                england = new decimal?[] { englandSeries?.Previous2, englandSeries?.Previous, englandSeries?.Current }
            },
            table = new
            {
                thisSchool = new[]
                {
                    DisplayPercentNullable(thisSchoolSeries.Previous2),
                    DisplayPercentNullable(thisSchoolSeries.Previous),
                    DisplayPercentNullable(thisSchoolSeries.Current)
                },
                similarSchool = new[]
                {
                    DisplayPercentNullable(similarSchoolSeries.Previous2),
                    DisplayPercentNullable(similarSchoolSeries.Previous),
                    DisplayPercentNullable(similarSchoolSeries.Current)
                },
                england = new[]
                {
                    DisplayPercentNullable(englandSeries?.Previous2),
                    DisplayPercentNullable(englandSeries?.Previous),
                    DisplayPercentNullable(englandSeries?.Current)
                }
            }
        });
    }

    [HttpGet]
    [Route("compare-school-details")]
    public async Task<IActionResult> SchoolDetails(
        string urn,
        string similarSchoolUrn,
        [FromQuery(Name = "similarityCalculation")] string? similarityCalculation = null)
    {
        var modelResult = await TryBuildFullSchoolDetailsModelAsync(urn, similarSchoolUrn, similarityCalculation);
        if (modelResult.Result != null)
            return modelResult.Result;

        var comparisonModel = modelResult.Model!;
        SetComparisonSchoolViewData(comparisonModel);

        var schoolDetailsModel = new SimilarSchoolDetailsViewModel
        {
            Urn = comparisonModel.Urn,
            SimilarSchoolUrn = comparisonModel.SimilarSchoolUrn,
            Name = comparisonModel.Name,
            SimilarSchoolName = comparisonModel.SimilarSchoolName,
            CurrentSchoolLatitude = comparisonModel.CurrentSchoolLatitude,
            CurrentSchoolLongitude = comparisonModel.CurrentSchoolLongitude,
            SimilarSchoolLatitude = comparisonModel.SimilarSchoolLatitude,
            SimilarSchoolLongitude = comparisonModel.SimilarSchoolLongitude,
            Distance = comparisonModel.Distance,
            SimilarSchoolDetails = comparisonModel.SimilarSchoolDetails
        };

        return View("~/Views/Shared/SimilarSchoolsComparison/SchoolDetails.cshtml", schoolDetailsModel);
    }

    /// <summary>
    /// Builds the "base" view model used by Similarity/KS4/Attendance pages.
    /// Handles: invalid params, null response, missing SimilarSchoolDetails, exceptions.
    /// </summary>
    private async Task<(SimilarSchoolsComparisonViewModel? Model, IActionResult? Result)>
        TryBuildBaseModelAsync(string urn, string similarSchoolUrn, string? similarityCalculation)
    {
        if (string.IsNullOrWhiteSpace(urn) || string.IsNullOrWhiteSpace(similarSchoolUrn))
        {
            _logger.LogWarning(
                "SimilarSchoolsComparison requested with invalid route params. urn='{Urn}', similarSchoolUrn='{SimilarUrn}'",
                urn, similarSchoolUrn);

            return (null, BadRequest());
        }

        var response = await _getSimilarSchoolDetails.Execute(
            new GetSimilarSchoolDetailsRequest(urn, similarSchoolUrn));

        var model = new SimilarSchoolsComparisonViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = response.SchoolName,
            SimilarSchoolName = response.SimilarSchoolDetails.Name
        };

        model.CharacteristicsRows = await BuildCharacteristicRowsAsync(urn, similarSchoolUrn, similarityCalculation);
        return (model, null);
    }

    /// <summary>
    /// Builds the model for SchoolDetails page (includes coordinates/distance/details).
    /// </summary>
    private async Task<(SimilarSchoolsComparisonViewModel? Model, IActionResult? Result)>
        TryBuildFullSchoolDetailsModelAsync(string urn, string similarSchoolUrn, string? similarityCalculation)
    {
        var baseResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn, similarityCalculation);
        if (baseResult.Result != null)
            return baseResult;

        // We need the full response again to map coords etc.
        // If you want to avoid calling Execute twice, we can refactor to return response as well.
        GetSimilarSchoolDetailsResponse? response;
        try
        {
            response = await _getSimilarSchoolDetails.Execute(
                new GetSimilarSchoolDetailsRequest(urn, similarSchoolUrn));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error calling GetSimilarSchoolDetails (SchoolDetails) for urn='{Urn}', similarSchoolUrn='{SimilarUrn}'",
                urn, similarSchoolUrn);

            return (null, StatusCode(StatusCodes.Status500InternalServerError));
        }

        if (response is null)
        {
            _logger.LogWarning(
                "GetSimilarSchoolDetails returned null (SchoolDetails) for urn='{Urn}', similarSchoolUrn='{SimilarUrn}'",
                urn, similarSchoolUrn);

            return (null, NotFound());
        }

        var model = baseResult.Model!;

        model.CurrentSchoolLatitude = response.CurrentSchoolCoordinates?.Latitude;
        model.CurrentSchoolLongitude = response.CurrentSchoolCoordinates?.Longitude;
        model.SimilarSchoolLatitude = response.SimilarSchoolCoordinates?.Latitude;
        model.SimilarSchoolLongitude = response.SimilarSchoolCoordinates?.Longitude;
        model.Distance = response.DistanceMiles;
        model.SimilarSchoolDetails = response.SimilarSchoolDetails;

        return (model, null);
    }

    private void SetComparisonSchoolViewData(SimilarSchoolsComparisonViewModel data)
    {
        ViewData["ComparisonSchool"] = data;
    }

    private async Task<IReadOnlyList<SimilarSchoolsComparisonViewModel.CharacteristicRow>>
        BuildCharacteristicRowsAsync(string urn, string similarSchoolUrn, string? similarityCalculation)
    {
        var calculationMethod = ParseSimilarityCalculation(similarityCalculation);
        var response = await _getCharacteristicsComparison.Execute(
            new GetCharacteristicsComparisonRequest(urn, similarSchoolUrn, calculationMethod));

        return _characteristicsFormatter.BuildRows(response);
    }

    private static SimilarityCalculationMethod ParseSimilarityCalculation(string? value)
    {
        return Enum.TryParse<SimilarityCalculationMethod>(value, true, out var similarityCalculationMethod)
            ? similarityCalculationMethod
            : SimilarityCalculationMethod.National;
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

    private static string NormalizeDestinationFilter(string? destination) =>
        destination?.ToLowerInvariant() switch
        {
            "education" => "education",
            "employment" => "employment",
            _ => "all"
        };

    private static SimilarSchoolsComparisonViewModel.CoreSubjectSection BuildComparisonCoreSubjectSection(
        GetSchoolKs4CoreSubjectsResponse thisSchoolResponse,
        GetSchoolKs4CoreSubjectsResponse selectedSchoolResponse,
        SchoolKs4CoreSubject subject)
    {
        var thisSchoolSelection = SchoolKs4CoreSubjectSelection.From(
            thisSchoolResponse,
            subject,
            SchoolKs4CoreSubjectGradeFilter.Grade4);
        var selectedSchoolSelection = SchoolKs4CoreSubjectSelection.From(
            selectedSchoolResponse,
            subject,
            SchoolKs4CoreSubjectGradeFilter.Grade4);

        return new SimilarSchoolsComparisonViewModel.CoreSubjectSection(
            thisSchoolSelection.ThreeYearAverage.SchoolValue,
            selectedSchoolSelection.ThreeYearAverage.SchoolValue,
            thisSchoolSelection.ThreeYearAverage.EnglandValue ?? selectedSchoolSelection.ThreeYearAverage.EnglandValue,
            thisSchoolSelection.YearByYear.School,
            selectedSchoolSelection.YearByYear.School,
            thisSchoolSelection.YearByYear.England ?? selectedSchoolSelection.YearByYear.England);
    }

    private static Ks4HeadlineMeasureAverage? SelectDestinationsAverage(GetKs4HeadlineMeasuresResponse? response, string destination) =>
        destination switch
        {
            "education" => response?.DestinationsEducationThreeYearAverage,
            "employment" => response?.DestinationsEmploymentThreeYearAverage,
            _ => response?.DestinationsThreeYearAverage
        };

    private static Ks4HeadlineMeasureYearByYear? SelectDestinationsYearByYear(GetKs4HeadlineMeasuresResponse? response, string destination) =>
        destination switch
        {
            "education" => response?.DestinationsEducationYearByYear,
            "employment" => response?.DestinationsEmploymentYearByYear,
            _ => response?.DestinationsYearByYear
        };

}
