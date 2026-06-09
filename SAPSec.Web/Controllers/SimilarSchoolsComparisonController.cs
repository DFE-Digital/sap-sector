using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features;
using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Features.Ks4CoreSubjects;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Web.Constants;
using SAPSec.Web.Formatters;
using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;
using System.Globalization;

namespace SAPSec.Web.Controllers;

[Route("school/{urn}/view-similar-schools/{similarSchoolUrn}")]
[Authorize]
public class SimilarSchoolsComparisonController : Controller
{
    private readonly GetSimilarSchoolDetailsUseCase _getSimilarSchoolDetails;
    private readonly GetAttendanceMeasuresUseCase _getAttendanceMeasures;
    private readonly GetSchoolComparisonKs4CoreSubjectsUseCase _getSchoolComparisonKs4CoreSubjects;
    private readonly GetSchoolComparisonKs4HeadlineMeasuresUseCase _getSchoolComparisonKs4HeadlineMeasures;
    private readonly GetCharacteristicsComparisonUseCase _getCharacteristicsComparison;
    private readonly ILogger<SimilarSchoolsComparisonController> _logger;
    private readonly ICharacteristicsComparisonFormatter _characteristicsFormatter;

    public SimilarSchoolsComparisonController(
        GetSimilarSchoolDetailsUseCase getSimilarSchoolDetails,
        GetAttendanceMeasuresUseCase getAttendanceMeasures,
        GetSchoolComparisonKs4CoreSubjectsUseCase getSchoolComparisonKs4CoreSubjects,
        GetSchoolComparisonKs4HeadlineMeasuresUseCase getSchoolComparisonKs4HeadlineMeasures,
        GetCharacteristicsComparisonUseCase getCharacteristicsComparison,
        ICharacteristicsComparisonFormatter characteristicsFormatter,
        ILogger<SimilarSchoolsComparisonController> logger)
    {
        _getSimilarSchoolDetails = getSimilarSchoolDetails
            ?? throw new ArgumentNullException(nameof(getSimilarSchoolDetails));
        _getAttendanceMeasures = getAttendanceMeasures
            ?? throw new ArgumentNullException(nameof(getAttendanceMeasures));
        _getSchoolComparisonKs4CoreSubjects = getSchoolComparisonKs4CoreSubjects
            ?? throw new ArgumentNullException(nameof(getSchoolComparisonKs4CoreSubjects));
        _getSchoolComparisonKs4HeadlineMeasures = getSchoolComparisonKs4HeadlineMeasures
            ?? throw new ArgumentNullException(nameof(getSchoolComparisonKs4HeadlineMeasures));
        _getCharacteristicsComparison = getCharacteristicsComparison
            ?? throw new ArgumentNullException(nameof(getCharacteristicsComparison));
        _characteristicsFormatter = characteristicsFormatter
            ?? throw new ArgumentNullException(nameof(characteristicsFormatter));
        _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public Task<IActionResult> Index(
        string urn,
        string similarSchoolUrn,
        [FromQuery(Name = "similarityCalculation")] string? similarityCalculation = null) =>
        Similarity(urn, similarSchoolUrn, similarityCalculation);

    [HttpGet]
    [Route("similarity")]
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
    [Route("ks4-headline-measures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(
        string urn,
        string similarSchoolUrn,
        [FromQuery(Name = "similarityCalculation")] string? similarityCalculation = null)
    {
        var modelResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn, similarityCalculation);
        if (modelResult.Result != null)
            return modelResult.Result;

        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await _getSchoolComparisonKs4HeadlineMeasures.Execute(new GetSchoolComparisonKs4HeadlineMeasuresRequest(urn, similarSchoolUrn, filters));
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);

        var model = modelResult.Model!;
        model.Attainment8 = MapMeasure(response.Attainment8, response.CurrentSchool, response.SimilarSchool);
        model.EnglishAndMaths = MapMeasure(response.EnglishAndMaths, response.CurrentSchool, response.SimilarSchool);
        model.Destinations = MapMeasure(response.Destinations, response.CurrentSchool, response.SimilarSchool);
        SetComparisonSchoolViewData(model);

        return View(model);
    }

    [HttpGet]
    [Route("ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(
        string urn,
        string similarSchoolUrn,
        [FromQuery(Name = "similarityCalculation")] string? similarityCalculation = null)
    {
        var modelResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn, similarityCalculation);
        if (modelResult.Result != null)
            return modelResult.Result;

        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await _getSchoolComparisonKs4CoreSubjects.Execute(new GetSchoolComparisonKs4CoreSubjectsRequest(urn, similarSchoolUrn, filters));
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);

        var model = modelResult.Model!;
        model.Measures = response.Measures.Select(m => MapMeasure(m, response.CurrentSchool, response.SimilarSchool));
        SetComparisonSchoolViewData(model);

        return View(model);
    }

    [HttpGet]
    [Route("attendance")]
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

        var thisSchoolAttendance = await _getAttendanceMeasures.Execute(new GetAttendanceMeasuresRequest(urn));
        var similarSchoolAttendance = await _getAttendanceMeasures.Execute(new GetAttendanceMeasuresRequest(similarSchoolUrn));

        var isPersistentAbsence = normalizedAbsenceType == "persistent";
        var yearLabels = Ks4YearLabelConfig.YearByYear;

        var thisSchoolSeries = isPersistentAbsence
            ? thisSchoolAttendance.PersistentAbsenceYearByYear.School
            : thisSchoolAttendance.OverallAbsenceYearByYear.School;
        var similarSchoolSeries = isPersistentAbsence
            ? similarSchoolAttendance.PersistentAbsenceYearByYear.School
            : similarSchoolAttendance.OverallAbsenceYearByYear.School;
        var englandSeries = isPersistentAbsence
            ? (thisSchoolAttendance.PersistentAbsenceYearByYear.England ?? similarSchoolAttendance.PersistentAbsenceYearByYear.England)
            : (thisSchoolAttendance.OverallAbsenceYearByYear.England ?? similarSchoolAttendance.OverallAbsenceYearByYear.England);

        var thisSchoolThreeYearAverage = isPersistentAbsence
            ? thisSchoolAttendance.PersistentAbsenceThreeYearAverage.SchoolValue
            : thisSchoolAttendance.OverallAbsenceThreeYearAverage.SchoolValue;
        var similarSchoolThreeYearAverage = isPersistentAbsence
            ? similarSchoolAttendance.PersistentAbsenceThreeYearAverage.SchoolValue
            : similarSchoolAttendance.OverallAbsenceThreeYearAverage.SchoolValue;
        var englandThreeYearAverage = isPersistentAbsence
            ? (thisSchoolAttendance.PersistentAbsenceThreeYearAverage.EnglandValue ?? similarSchoolAttendance.PersistentAbsenceThreeYearAverage.EnglandValue)
            : (thisSchoolAttendance.OverallAbsenceThreeYearAverage.EnglandValue ?? similarSchoolAttendance.OverallAbsenceThreeYearAverage.EnglandValue);

        return Json(new
        {
            absenceType = normalizedAbsenceType,
            years = yearLabels,
            bar = new decimal?[]
            {
                thisSchoolThreeYearAverage,
                similarSchoolThreeYearAverage,
                englandThreeYearAverage
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
                    DisplayPercentNullable(thisSchoolSeries.Current),
                    DisplayPercentNullable(thisSchoolThreeYearAverage)
                },
                similarSchool = new[]
                {
                    DisplayPercentNullable(similarSchoolSeries.Previous2),
                    DisplayPercentNullable(similarSchoolSeries.Previous),
                    DisplayPercentNullable(similarSchoolSeries.Current),
                    DisplayPercentNullable(similarSchoolThreeYearAverage)
                },
                england = new[]
                {
                    DisplayPercentNullable(englandSeries?.Previous2),
                    DisplayPercentNullable(englandSeries?.Previous),
                    DisplayPercentNullable(englandSeries?.Current),
                    DisplayPercentNullable(englandThreeYearAverage)
                }
            }
        });
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(
        string urn,
        string similarSchoolUrn,
        [FromQuery(Name = "similarityCalculation")] string? similarityCalculation = null)
    {
        var modelResult = await TryBuildFullSchoolDetailsModelAsync(urn, similarSchoolUrn, similarityCalculation);
        if (modelResult.Result != null)
            return modelResult.Result;

        SetComparisonSchoolViewData(modelResult.Model!);
        return View(modelResult.Model);
    }

    private static MeasureViewModel MapMeasure(Measure measure, SchoolInfo currentSchool, SchoolInfo similarSchool)
    {
        // TODO: Should colours be in the view? Not sure how to do that yet.
        var chartColors = new[] { "#ca357c", "#2a1950", "#2a1950" };
        var yearByYearColors = new[] { "#ca357c", "#2a1950", "#4b9b7d" };

        return MeasureViewModel.FromMeasure(measure, currentSchool,
            // TODO: Should labels be in the view? Not sure how to do that yet.
            [
                currentSchool.Name,
                similarSchool.Name,
                "Schools in England average",
            ],
            chartColors,
            yearByYearColors);
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
        model.SimilarSchoolDetails = SchoolDetailsViewModel.FromSchoolDetails(response.SimilarSchoolDetails);

        return (model, null);
    }

    private void SetComparisonSchoolViewData(SimilarSchoolsComparisonViewModel data)
    {
        var layoutModel = new SimilarSchoolsComparisonLayoutModel
        {
            Urn = data.Urn,
            Name = data.Name,
            SimilarSchoolUrn = data.SimilarSchoolUrn,
            SimilarSchoolName = data.SimilarSchoolName
        };

        ViewData["LayoutModel"] = layoutModel;
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
}
