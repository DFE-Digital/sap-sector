using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Features.SchoolDetails.Comparison;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels.Comparison;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.Formatters;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Secondary.Controllers;

[Area("Secondary")]
[Route("school/secondary/{urn}/view-similar-schools/{comparatorSchoolUrn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Secondary, "urn", "comparatorSchoolUrn")]
public class ComparisonController(
    IUseCase<GetSecondaryComparisonSimilarityCharacteristicsRequest, GetSecondaryComparisonSimilarityCharacteristicsResponse> getSimilarityCharacteristicsUseCase,
    IUseCase<GetComparisonKs4HeadlineMeasuresRequest, GetComparisonKs4HeadlineMeasuresResponse> getKs4HeadlineMeasuresUseCase,
    IUseCase<GetComparisonKs4CoreSubjectsMeasuresRequest, GetComparisonKs4CoreSubjectsMeasuresResponse> getKs4CoreSubjectsUseCase,
    IUseCase<GetSecondaryComparisonAttendanceMeasuresRequest, GetComparisonAttendanceMeasuresResponse> getAttendanceMeasuresUseCase,
    [FromKeyedServices(ServiceKeys.Secondary)]
    IUseCase<GetComparisonSchoolDetailsRequest, GetComparisonSchoolDetailsResponse> getSchoolDetailsUseCase,
    ISecondaryCharacteristicsComparisonFormatter characteristicsFormatter) : Controller
{
    [HttpGet]
    [Route("compare-similarity")]
    public async Task<IActionResult> Similarity(
        string urn,
        string comparatorSchoolUrn)
    {
        var response = await getSimilarityCharacteristicsUseCase.Execute(new(urn, comparatorSchoolUrn));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(
            response.CurrentSchool,
            response.ComparatorSchool);

        var model = new SimilarityPageViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = comparatorSchoolUrn,
            Name = response.CurrentSchool.Name,
            SimilarSchoolName = response.ComparatorSchool.Name,
            CharacteristicsRows = characteristicsFormatter.BuildRows(response.SimilarityCharacteristics)
        };

        return View(model);
    }

    [HttpGet]
    [Route("compare-ks4-headline-measures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(
        string urn,
        string comparatorSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getKs4HeadlineMeasuresUseCase.Execute(new(urn, comparatorSchoolUrn, filters));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(response.CurrentSchool, response.ComparatorSchool);

        var model = new ViewModels.Comparison.Ks4HeadlineMeasuresPageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            ComparatorSchool = SchoolInfoViewModel.FromSchoolInfo(response.ComparatorSchool),
            Attainment8 = MeasureViewModel.FromSecondaryComparisonMeasure(response.Attainment8, response.CurrentSchool, response.ComparatorSchool),
            EnglishMaths = MeasureViewModel.FromSecondaryComparisonMeasure(response.EnglishMaths, response.CurrentSchool, response.ComparatorSchool),
            Destinations = MeasureViewModel.FromSecondaryComparisonMeasure(response.Destinations, response.CurrentSchool, response.ComparatorSchool)
        };

        return View(model);
    }

    [HttpGet]
    [Route("compare-ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(
    string urn,
    string comparatorSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getKs4CoreSubjectsUseCase.Execute(new(urn, comparatorSchoolUrn, filters));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(response.CurrentSchool, response.ComparatorSchool);

        var model = new ViewModels.Comparison.Ks4CoreSubjectsPageViewModel
        {
            CurrentSchool = SchoolInfoViewModel.FromSchoolInfo(response.CurrentSchool),
            ComparatorSchool = SchoolInfoViewModel.FromSchoolInfo(response.ComparatorSchool),
            Measures = [
                MeasureViewModel.FromSecondaryComparisonMeasure(response.EnglishLanguage, response.CurrentSchool, response.ComparatorSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.EnglishLiterature, response.CurrentSchool, response.ComparatorSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Maths, response.CurrentSchool, response.ComparatorSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.CombinedScience, response.CurrentSchool, response.ComparatorSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Biology, response.CurrentSchool, response.ComparatorSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Chemistry, response.CurrentSchool, response.ComparatorSchool),
                MeasureViewModel.FromSecondaryComparisonMeasure(response.Physics, response.CurrentSchool, response.ComparatorSchool)
            ]
        };

        return View(model);
    }

    [HttpGet]
    [Route("compare-attendance")]
    public async Task<IActionResult> Attendance(string urn, string comparatorSchoolUrn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getAttendanceMeasuresUseCase.Execute(new(urn, comparatorSchoolUrn, filters));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(
            response.CurrentSchool,
            response.ComparatorSchool);

        var model = new AttendancePageViewModel
        {
            Urn = response.CurrentSchool.Urn,
            Name = response.CurrentSchool.Name,
            SimilarSchoolUrn = response.ComparatorSchool.Urn,
            SimilarSchoolName = response.ComparatorSchool.Name,
            Absence = MeasureViewModel.FromPrimaryComparisonMeasure(response.Absence, response.CurrentSchool, response.ComparatorSchool)
        };

        return View(model);
    }

    [HttpGet]
    [Route("compare-school-details")]
    public async Task<IActionResult> SchoolDetails(
        string urn,
        string comparatorSchoolUrn)
    {
        var response = await getSchoolDetailsUseCase.Execute(new(urn, comparatorSchoolUrn));

        ViewData[ViewDataKeys.ComparisonLayout] = ComparisonLayoutModel.FromSchoolInfo(
            response.CurrentSchool.School,
            response.ComparatorSchool.School);

        var schoolDetailsModel = new SchoolDetailsPageViewModel
        {
            CurrentSchoolUrn = urn,
            ComparatorSchoolUrn = comparatorSchoolUrn,
            CurrentSchoolName = response.CurrentSchool.School.Name,
            ComparatorSchoolName = response.ComparatorSchoolDetails.Name,
            CurrentSchoolLatitude = response.CurrentSchool.Coordinates?.Latitude,
            CurrentSchoolLongitude = response.CurrentSchool.Coordinates?.Longitude,
            ComparatorSchoolLatitude = response.ComparatorSchool.Coordinates?.Latitude,
            ComparatorSchoolLongitude = response.ComparatorSchool.Coordinates?.Longitude,
            Distance = response.DistanceMiles,
            ComparatorSchoolDetails = SchoolDetailsViewModel.FromSchoolDetails(response.ComparatorSchoolDetails)
        };

        return View(schoolDetailsModel);
    }
}
