using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.Secondary;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.Formatters;
using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;
using System.Globalization;

namespace SAPSec.Web.Areas.Secondary.Controllers;

[Area("Secondary")]
[Route("school/secondary/{urn}/view-similar-schools/{similarSchoolUrn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Secondary, "urn", "similarSchoolUrn")]
public class ComparisonController : Controller
{
    private readonly GetSimilarSchoolDetails _getSimilarSchoolDetails;
    private readonly GetAttendanceMeasures _getAttendanceMeasures;
    private readonly IUseCase<GetComparisonKs4HeadlineMeasuresRequest, GetComparisonKs4HeadlineMeasuresResponse> _getKs4HeadlineMeasuresUseCase;
    private readonly IUseCase<GetComparisonKs4CoreSubjectsRequest, GetComparisonKs4CoreSubjectsResponse> _getKs4CoreSubjectsUseCase;
    private readonly GetCharacteristicsComparison _getCharacteristicsComparison;
    private readonly ILogger<ComparisonController> _logger;
    private readonly ICharacteristicsComparisonFormatter _characteristicsFormatter;

    public ComparisonController(
        GetSimilarSchoolDetails getSimilarSchoolDetails,
        GetAttendanceMeasures getAttendanceMeasures,
        IUseCase<GetComparisonKs4HeadlineMeasuresRequest, GetComparisonKs4HeadlineMeasuresResponse> getKs4HeadlineMeasuresUseCase,
        IUseCase<GetComparisonKs4CoreSubjectsRequest, GetComparisonKs4CoreSubjectsResponse> getKs4CoreSubjectsUseCase,
        GetCharacteristicsComparison getCharacteristicsComparison,
        ICharacteristicsComparisonFormatter characteristicsFormatter,
        ILogger<ComparisonController> logger)
    {
        _getSimilarSchoolDetails = getSimilarSchoolDetails
            ?? throw new ArgumentNullException(nameof(getSimilarSchoolDetails));
        _getAttendanceMeasures = getAttendanceMeasures
            ?? throw new ArgumentNullException(nameof(getAttendanceMeasures));
        _getKs4HeadlineMeasuresUseCase = getKs4HeadlineMeasuresUseCase
            ?? throw new ArgumentNullException(nameof(getKs4HeadlineMeasuresUseCase));
        _getKs4CoreSubjectsUseCase = getKs4CoreSubjectsUseCase
            ?? throw new ArgumentNullException(nameof(getKs4CoreSubjectsUseCase));
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
        string similarSchoolUrn) =>
        Similarity(urn, similarSchoolUrn);

    [HttpGet]
    [Route("similarity")]
    public async Task<IActionResult> Similarity(
        string urn,
        string similarSchoolUrn)
    {
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);

        var modelResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn);
        if (modelResult.Result != null)
            return modelResult.Result;

        SetComparisonSchoolViewData(modelResult.Model!);
        return View("Similarity", modelResult.Model);
    }

    [HttpGet]
    [Route("ks4-headline-measures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(
        string urn,
        string similarSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await _getKs4HeadlineMeasuresUseCase.Execute(new(urn, similarSchoolUrn, filters));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(response.School, response.SimilarSchool);

        var model = new ViewModels.Comparison.Ks4HeadlineMeasuresPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            SimilarSchool = SchoolInfoViewModel.FromSchoolInfo(response.SimilarSchool),
            Attainment8 = MeasureViewModel.FromSecondaryComparisonMeasure(response.Attainment8, response.School, response.SimilarSchool),
            EnglishMaths = MeasureViewModel.FromSecondaryComparisonMeasure(response.EnglishMaths, response.School, response.SimilarSchool),
            Destinations = MeasureViewModel.FromSecondaryComparisonMeasure(response.Destinations, response.School, response.SimilarSchool)
        };

        return View(model);
    }

    [HttpGet]
    [Route("ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(
    string urn,
    string similarSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await _getKs4CoreSubjectsUseCase.Execute(new(urn, similarSchoolUrn, filters));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(response.School, response.SimilarSchool);

        var model = new ViewModels.Comparison.Ks4CoreSubjectsPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            SimilarSchool = SchoolInfoViewModel.FromSchoolInfo(response.SimilarSchool),
            Measures = [
                MeasureViewModel.FromSecondaryComparisonMeasure(response.EnglishLanguage, response.School, response.SimilarSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.EnglishLiterature, response.School, response.SimilarSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Maths, response.School, response.SimilarSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.CombinedScience, response.School, response.SimilarSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Biology, response.School, response.SimilarSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Chemistry, response.School, response.SimilarSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Physics, response.School, response.SimilarSchool)
            ]
        };

        return View(model);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(
        string urn,
        string similarSchoolUrn)
    {
        var modelResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn);
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
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(
        string urn,
        string similarSchoolUrn)
    {
        var modelResult = await TryBuildFullSchoolDetailsModelAsync(urn, similarSchoolUrn);
        if (modelResult.Result != null)
            return modelResult.Result;

        var comparisonModel = modelResult.Model!;

        ViewData[ViewDataKeys.ComparisonLayout] = new ComparisonLayoutModel(
            comparisonModel.Urn,
            comparisonModel.Name,
            comparisonModel.SimilarSchoolUrn,
            comparisonModel.SimilarSchoolName
        );

        var schoolDetailsModel = new SimilarSchoolDetailsViewModel
        {
            Urn = comparisonModel.Urn,
            Name = comparisonModel.Name,
            SimilarSchoolUrn = comparisonModel.SimilarSchoolUrn,
            SimilarSchoolName = comparisonModel.SimilarSchoolName,
            CurrentSchoolLatitude = comparisonModel.CurrentSchoolLatitude,
            CurrentSchoolLongitude = comparisonModel.CurrentSchoolLongitude,
            SimilarSchoolLatitude = comparisonModel.SimilarSchoolLatitude,
            SimilarSchoolLongitude = comparisonModel.SimilarSchoolLongitude,
            Distance = comparisonModel.Distance,
            SimilarSchoolDetails = comparisonModel.SimilarSchoolDetails
        };

        return View(schoolDetailsModel);
    }

    private async Task<(SimilarSchoolsComparisonViewModel? Model, IActionResult? Result)>
        TryBuildBaseModelAsync(string urn, string similarSchoolUrn)
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

        model.CharacteristicsRows = await BuildCharacteristicRowsAsync(urn, similarSchoolUrn);
        return (model, null);
    }

    private async Task<(SimilarSchoolsComparisonViewModel? Model, IActionResult? Result)>
        TryBuildFullSchoolDetailsModelAsync(string urn, string similarSchoolUrn)
    {
        var baseResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn);
        if (baseResult.Result != null)
            return baseResult;

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
        ViewData[ViewDataKeys.ComparisonSchool] = data;
    }

    private async Task<IReadOnlyList<SimilarSchoolsComparisonViewModel.CharacteristicRow>>
        BuildCharacteristicRowsAsync(string urn, string similarSchoolUrn)
    {
        var response = await _getCharacteristicsComparison.Execute(
            new GetCharacteristicsComparisonRequest(urn, similarSchoolUrn));

        return _characteristicsFormatter.BuildRows(response);
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
