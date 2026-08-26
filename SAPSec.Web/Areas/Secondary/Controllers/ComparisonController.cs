using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels.Comparison;
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
public class ComparisonController(
    GetSimilarSchoolDetails getSimilarSchoolDetails,
    GetAttendanceMeasures getAttendanceMeasures,
    IUseCase<GetComparisonKs4HeadlineMeasuresRequest, GetComparisonKs4HeadlineMeasuresResponse> getKs4HeadlineMeasuresUseCase,
    IUseCase<GetComparisonKs4CoreSubjectsRequest, GetComparisonKs4CoreSubjectsResponse> getKs4CoreSubjectsUseCase,
    IUseCase<GetComparisonAttendanceMeasuresRequest, GetComparisonAttendanceMeasuresResponse> getAttendanceMeasuresUseCase,
    GetCharacteristicsComparison getCharacteristicsComparison,
    ICharacteristicsComparisonFormatter characteristicsFormatter,
    ILogger<ComparisonController> logger) : Controller
{

    [HttpGet]
    [Route("compare-similarity")]
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
    [Route("compare-ks4-headline-measures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(
        string urn,
        string similarSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getKs4HeadlineMeasuresUseCase.Execute(new(urn, similarSchoolUrn, filters));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(response.CurrentSchool, response.SimilarSchool);

        var model = new ViewModels.Comparison.Ks4HeadlineMeasuresPageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            SimilarSchool = SchoolInfoViewModel.FromSchoolInfo(response.SimilarSchool),
            Attainment8 = MeasureViewModel.FromSecondaryComparisonMeasure(response.Attainment8, response.CurrentSchool, response.SimilarSchool),
            EnglishMaths = MeasureViewModel.FromSecondaryComparisonMeasure(response.EnglishMaths, response.CurrentSchool, response.SimilarSchool),
            Destinations = MeasureViewModel.FromSecondaryComparisonMeasure(response.Destinations, response.CurrentSchool, response.SimilarSchool)
        };

        return View(model);
    }

    [HttpGet]
    [Route("compare-ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(
    string urn,
    string similarSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getKs4CoreSubjectsUseCase.Execute(new(urn, similarSchoolUrn, filters));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(response.CurrentSchool, response.SimilarSchool);

        var model = new ViewModels.Comparison.Ks4CoreSubjectsPageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            SimilarSchool = SchoolInfoViewModel.FromSchoolInfo(response.SimilarSchool),
            Measures = [
                MeasureViewModel.FromSecondaryComparisonMeasure(response.EnglishLanguage, response.CurrentSchool, response.SimilarSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.EnglishLiterature, response.CurrentSchool, response.SimilarSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Maths, response.CurrentSchool, response.SimilarSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.CombinedScience, response.CurrentSchool, response.SimilarSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Biology, response.CurrentSchool, response.SimilarSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Chemistry, response.CurrentSchool, response.SimilarSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Physics, response.CurrentSchool, response.SimilarSchool)
            ]
        };

        return View(model);
    }

    [HttpGet]
    [Route("compare-attendance")]
    public async Task<IActionResult> Attendance(string urn, string similarSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getAttendanceMeasuresUseCase.Execute(new(MeasurePhase.Secondary, urn, similarSchoolUrn, filters));

        ViewData[ViewDataKeys.ComparisonLayout] = new ComparisonLayoutModel(
            response.CurrentSchool.Urn,
            response.CurrentSchool.Name,
            response.SimilarSchool.Urn,
            response.SimilarSchool.Name
        );

        var model = new AttendancePageViewModel
        {
            Urn = response.CurrentSchool.Urn,
            Name = response.CurrentSchool.Name,
            SimilarSchoolUrn = response.SimilarSchool.Urn,
            SimilarSchoolName = response.SimilarSchool.Name,
            Absence = MeasureViewModel.FromPrimaryComparisonMeasure(response.Absence, response.CurrentSchool, response.SimilarSchool)
        };

        return View(model);
    }

    [HttpGet]
    [Route("compare-attendance-old")]
    public async Task<IActionResult> AttendanceOld(
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

        var thisSchoolAttendance = await getAttendanceMeasures.Execute(new GetAttendanceMeasuresRequest(urn));
        var similarSchoolAttendance = await getAttendanceMeasures.Execute(new GetAttendanceMeasuresRequest(similarSchoolUrn));

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
            logger.LogWarning(
                "SimilarSchoolsComparison requested with invalid route params. urn='{Urn}', similarSchoolUrn='{SimilarUrn}'",
                urn, similarSchoolUrn);

            return (null, BadRequest());
        }

        var response = await getSimilarSchoolDetails.Execute(
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
            response = await getSimilarSchoolDetails.Execute(
                new GetSimilarSchoolDetailsRequest(urn, similarSchoolUrn));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error calling GetSimilarSchoolDetails (SchoolDetails) for urn='{Urn}', similarSchoolUrn='{SimilarUrn}'",
                urn, similarSchoolUrn);

            return (null, StatusCode(StatusCodes.Status500InternalServerError));
        }

        if (response is null)
        {
            logger.LogWarning(
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
        var response = await getCharacteristicsComparison.Execute(
            new GetCharacteristicsComparisonRequest(urn, similarSchoolUrn));

        return characteristicsFormatter.BuildRows(response);
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
