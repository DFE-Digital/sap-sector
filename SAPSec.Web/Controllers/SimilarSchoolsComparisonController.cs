using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Web.Formatters;
using SAPSec.Web.Constants;
using SAPSec.Web.Helpers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Route("school/{urn}/view-similar-schools/{similarSchoolUrn}")]
public class SimilarSchoolsComparisonController : Controller
{
    private readonly GetSimilarSchoolDetails _getSimilarSchoolDetails;
    private readonly GetCharacteristicsComparison _getCharacteristicsComparison;
    private readonly ICharacteristicsComparisonFormatter _characteristicsFormatter;
    private readonly ILogger<SimilarSchoolsComparisonController> _logger;

    public SimilarSchoolsComparisonController(
        GetSimilarSchoolDetails getSimilarSchoolDetails,
        GetCharacteristicsComparison getCharacteristicsComparison,
        ICharacteristicsComparisonFormatter characteristicsFormatter,
        ILogger<SimilarSchoolsComparisonController> logger)
    {
        _getSimilarSchoolDetails = getSimilarSchoolDetails ?? throw new ArgumentNullException(nameof(getSimilarSchoolDetails));
        _getCharacteristicsComparison = getCharacteristicsComparison ?? throw new ArgumentNullException(nameof(getCharacteristicsComparison));
        _characteristicsFormatter = characteristicsFormatter ?? throw new ArgumentNullException(nameof(characteristicsFormatter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public Task<IActionResult> Index(string urn, string similarSchoolUrn) =>
        Similarity(urn, similarSchoolUrn);

    [HttpGet]
    [Route("Similarity")]
    public async Task<IActionResult> Similarity(string urn, string similarSchoolUrn)
    {
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);

        var modelResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn);
        if (modelResult.Result != null)
            return modelResult.Result;

        GetCharacteristicsComparisonResponse? response;
        try
        {
            response = await _getCharacteristicsComparison.Execute(
                new GetCharacteristicsComparisonRequest(urn, similarSchoolUrn));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error calling GetCharacteristicsComparison for urn='{Urn}', similarSchoolUrn='{SimilarUrn}'",
                urn, similarSchoolUrn);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        if (response is null)
        {
            _logger.LogWarning(
                "GetCharacteristicsComparison returned no data for urn='{Urn}', similarSchoolUrn='{SimilarUrn}'",
                urn, similarSchoolUrn);

            return NotFound();
        }

        modelResult.Model!.CharacteristicsRows = _characteristicsFormatter.BuildRows(
            response.CurrentSchool,
            response.SimilarSchool);

        SetComparisonSchoolViewData(modelResult.Model!);
        return View("Similarity", modelResult.Model);
    }

    [HttpGet]
    [Route("Ks4HeadlineMeasures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(string urn, string similarSchoolUrn)
    {
        var modelResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn);
        if (modelResult.Result != null)
            return modelResult.Result;

        SetComparisonSchoolViewData(modelResult.Model!);
        return View(modelResult.Model);
    }

    [HttpGet]
    [Route("Ks4CoreSubjects")]
    public async Task<IActionResult> Ks4CoreSubjects(string urn, string similarSchoolUrn)
    {
        var modelResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn);
        if (modelResult.Result != null)
            return modelResult.Result;

        SetComparisonSchoolViewData(modelResult.Model!);
        return View(modelResult.Model);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn, string similarSchoolUrn)
    {
        var modelResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn);
        if (modelResult.Result != null)
            return modelResult.Result;

        SetComparisonSchoolViewData(modelResult.Model!);
        return View(modelResult.Model);
    }

    [HttpGet]
    [Route("SchoolDetails")]
    public async Task<IActionResult> SchoolDetails(string urn, string similarSchoolUrn)
    {
        var modelResult = await TryBuildFullSchoolDetailsModelAsync(urn, similarSchoolUrn);
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
        TryBuildBaseModelAsync(string urn, string similarSchoolUrn)
    {
        if (string.IsNullOrWhiteSpace(urn) || string.IsNullOrWhiteSpace(similarSchoolUrn))
        {
            _logger.LogWarning(
                "SimilarSchoolsComparison requested with invalid route params. urn='{Urn}', similarSchoolUrn='{SimilarUrn}'",
                urn, similarSchoolUrn);

            return (null, BadRequest());
        }

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
                "Error calling GetSimilarSchoolDetails for urn='{Urn}', similarSchoolUrn='{SimilarUrn}'",
                urn, similarSchoolUrn);

            return (null, StatusCode(StatusCodes.Status500InternalServerError));
        }

        if (response is null)
        {
            _logger.LogWarning(
                "GetSimilarSchoolDetails returned null for urn='{Urn}', similarSchoolUrn='{SimilarUrn}'",
                urn, similarSchoolUrn);

            return (null, NotFound());
        }

        if (response.SimilarSchoolDetails is null)
        {
            _logger.LogWarning(
                "GetSimilarSchoolDetails returned null SimilarSchoolDetails for urn='{Urn}', similarSchoolUrn='{SimilarUrn}'",
                urn, similarSchoolUrn);

            return (null, NotFound());
        }

        // Name is a "DataWithAvailability", can be null or not available depending on your model.
        // Display() should handle "not available", but we still guard against null ref.
        var similarName = response.SimilarSchoolDetails.Name is null
            ? null
            : response.SimilarSchoolDetails.Name.Display();

        if (string.IsNullOrWhiteSpace(similarName))
        {
            _logger.LogWarning(
                "SimilarSchoolDetails.Name is missing for urn='{Urn}', similarSchoolUrn='{SimilarUrn}'",
                urn, similarSchoolUrn);
        }

        var model = new SimilarSchoolsComparisonViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = response.SchoolName,
            SimilarSchoolName = similarName ?? string.Empty
        };

        return (model, null);
    }

    /// <summary>
    /// Builds the model for SchoolDetails page (includes coordinates/distance/details).
    /// </summary>
    private async Task<(SimilarSchoolsComparisonViewModel? Model, IActionResult? Result)>
        TryBuildFullSchoolDetailsModelAsync(string urn, string similarSchoolUrn)
    {
        var baseResult = await TryBuildBaseModelAsync(urn, similarSchoolUrn);
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
}
