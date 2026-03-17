using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Web.Constants;
using SAPSec.Web.Formatters;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Route("school/{urn}/view-similar-schools/{similarSchoolUrn}")]
public class SimilarSchoolsComparisonController : Controller
{
    private readonly GetSimilarSchoolDetails _getSimilarSchoolDetails;
    private readonly GetKs4HeadlineMeasures _getKs4HeadlineMeasures;
    private readonly GetCharacteristicsComparison _getCharacteristicsComparison;
    private readonly ILogger<SimilarSchoolsComparisonController> _logger;
    private readonly ISimilarSchoolsSecondaryRepository _similarSchoolsSecondaryRepository;
    private readonly ICharacteristicsComparisonFormatter _characteristicsFormatter;

    public SimilarSchoolsComparisonController(
        GetSimilarSchoolDetails getSimilarSchoolDetails,
        GetKs4HeadlineMeasures getKs4HeadlineMeasures,
        GetCharacteristicsComparison getCharacteristicsComparison,
        ICharacteristicsComparisonFormatter characteristicsFormatter,
        ILogger<SimilarSchoolsComparisonController> logger,
        ISimilarSchoolsSecondaryRepository similarSchoolsSecondaryRepository)
    {
        _getSimilarSchoolDetails =
            getSimilarSchoolDetails ?? throw new ArgumentNullException(nameof(getSimilarSchoolDetails));
        _getKs4HeadlineMeasures = getKs4HeadlineMeasures ?? throw new ArgumentNullException(nameof(getKs4HeadlineMeasures));
        _getCharacteristicsComparison = getCharacteristicsComparison ??
                                        throw new ArgumentNullException(nameof(getCharacteristicsComparison));
        _characteristicsFormatter = characteristicsFormatter ??
                                    throw new ArgumentNullException(nameof(characteristicsFormatter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _similarSchoolsSecondaryRepository = similarSchoolsSecondaryRepository
            ?? throw new ArgumentNullException(nameof(similarSchoolsSecondaryRepository));
    }

    [HttpGet]
    public Task<IActionResult> Index(
        string urn,
        string similarSchoolUrn,
        [FromQuery(Name = "similarityCalculation")] string? similarityCalculation = null) =>
        Similarity(urn, similarSchoolUrn, similarityCalculation);

    [HttpGet]
    [Route("Similarity")]
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
    [Route("Throw")]
    public async Task<IActionResult> Throw(
        string urn,
        string similarSchoolUrn,
        [FromQuery(Name = "similarityCalculation")] string? similarityCalculation = null)
    {
        throw new InvalidOperationException("STU");
    }

    [HttpGet]
    [Route("Ks4HeadlineMeasures")]
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
    [Route("Ks4HeadlineMeasuresData")]
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

        var thisSchoolThreeYear = isGrade5
            ? thisSchoolKs4?.EngMaths59ThreeYearAverage.SchoolValue
            : thisSchoolKs4?.EngMaths49ThreeYearAverage.SchoolValue;
        var selectedSchoolThreeYear = isGrade5
            ? selectedSchoolKs4?.EngMaths59ThreeYearAverage.SchoolValue
            : selectedSchoolKs4?.EngMaths49ThreeYearAverage.SchoolValue;
        var englandThreeYear = isGrade5
            ? (thisSchoolKs4?.EngMaths59ThreeYearAverage.EnglandValue ?? selectedSchoolKs4?.EngMaths59ThreeYearAverage.EnglandValue)
            : (thisSchoolKs4?.EngMaths49ThreeYearAverage.EnglandValue ?? selectedSchoolKs4?.EngMaths49ThreeYearAverage.EnglandValue);

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
                thisSchoolThreeYear,
                selectedSchoolThreeYear,
                englandThreeYear
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
                    SimilarSchoolsComparisonViewModel.DisplayPercent(thisSchoolSeries?.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayPercent(thisSchoolSeries?.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayPercent(thisSchoolSeries?.Current),
                    SimilarSchoolsComparisonViewModel.DisplayPercent(thisSchoolThreeYear)
                },
                similarSchool = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayPercent(selectedSchoolSeries?.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayPercent(selectedSchoolSeries?.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayPercent(selectedSchoolSeries?.Current),
                    SimilarSchoolsComparisonViewModel.DisplayPercent(selectedSchoolThreeYear)
                },
                england = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayPercent(englandSeries?.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayPercent(englandSeries?.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayPercent(englandSeries?.Current),
                    SimilarSchoolsComparisonViewModel.DisplayPercent(englandThreeYear)
                }
            }
        });
    }

    [HttpGet]
    [Route("Ks4DestinationsData")]
    public async Task<IActionResult> Ks4DestinationsData(string urn, string similarSchoolUrn, string destination = "all")
    {
        if (string.IsNullOrWhiteSpace(urn) || string.IsNullOrWhiteSpace(similarSchoolUrn))
        {
            return BadRequest(new { error = "Missing route parameters." });
        }

        var normalizedDestination = NormalizeDestinationFilter(destination);
        var thisSchoolKs4 = await _getKs4HeadlineMeasures.Execute(new GetKs4HeadlineMeasuresRequest(urn));
        var selectedSchoolKs4 = await _getKs4HeadlineMeasures.Execute(new GetKs4HeadlineMeasuresRequest(similarSchoolUrn));

        var thisSchoolThreeYear = SelectDestinationsAverage(thisSchoolKs4, normalizedDestination)?.SchoolValue;
        var selectedSchoolThreeYear = SelectDestinationsAverage(selectedSchoolKs4, normalizedDestination)?.SchoolValue;
        var englandThreeYear =
            SelectDestinationsAverage(thisSchoolKs4, normalizedDestination)?.EnglandValue
            ?? SelectDestinationsAverage(selectedSchoolKs4, normalizedDestination)?.EnglandValue;

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
                thisSchoolThreeYear,
                selectedSchoolThreeYear,
                englandThreeYear
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
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(thisSchoolSeries?.Current),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(thisSchoolThreeYear)
                },
                similarSchool = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolSeries?.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolSeries?.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolSeries?.Current),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(selectedSchoolThreeYear)
                },
                england = new[]
                {
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandSeries?.Previous2),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandSeries?.Previous),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandSeries?.Current),
                    SimilarSchoolsComparisonViewModel.DisplayWholePercent(englandThreeYear)
                }
            }
        });
    }

    [HttpGet]
    [Route("Ks4CoreSubjects")]
    public async Task<IActionResult> Ks4CoreSubjects(
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
    [Route("SchoolDetails")]
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

        model.CurrentSchoolLatitude = response.CurrentSchoolCoordinates.Latitude;
        model.CurrentSchoolLongitude = response.CurrentSchoolCoordinates.Longitude;
        model.SimilarSchoolLatitude = response.SimilarSchoolCoordinates.Latitude;
        model.SimilarSchoolLongitude = response.SimilarSchoolCoordinates.Longitude;
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

    private static string NormalizeDestinationFilter(string? destination) =>
        destination?.ToLowerInvariant() switch
        {
            "education" => "education",
            "employment" => "employment",
            _ => "all"
        };

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
