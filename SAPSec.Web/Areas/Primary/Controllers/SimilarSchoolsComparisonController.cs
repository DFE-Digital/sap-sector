using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.UseCases;
using SAPSec.Web.Filters;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Areas.Primary.Controllers;

[Area("Primary")]
[Route("school/primary/{urn}/view-similar-schools/{similarSchoolUrn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Primary, "urn", "similarSchoolUrn")]
[RequireFeatureFlag(FeatureFlags.EnablePrimarySchools)]
public class SimilarSchoolsComparisonController(
    IUseCase<GetPrimarySimilarSchoolDetailsRequest, GetPrimarySimilarSchoolDetailsResponse> getPrimarySimilarSchoolDetailsUseCase)
    : Controller
{
    [HttpGet]
    public Task<IActionResult> Index(string urn, string similarSchoolUrn) =>
        SchoolDetails(urn, similarSchoolUrn);

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn, string similarSchoolUrn)
    {
        var response = await getPrimarySimilarSchoolDetailsUseCase.Execute(
            new GetPrimarySimilarSchoolDetailsRequest(urn, similarSchoolUrn));

        var model = new SimilarSchoolDetailsViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = response.SchoolName,
            SimilarSchoolName = response.SimilarSchoolDetails.Name,
            CurrentSchoolLatitude = response.CurrentSchoolCoordinates?.Latitude,
            CurrentSchoolLongitude = response.CurrentSchoolCoordinates?.Longitude,
            SimilarSchoolLatitude = response.SimilarSchoolCoordinates?.Latitude,
            SimilarSchoolLongitude = response.SimilarSchoolCoordinates?.Longitude,
            Distance = response.DistanceMiles,
            SimilarSchoolDetails = response.SimilarSchoolDetails
        };

        ViewData["ComparisonSchool"] = model;

        return View("~/Views/Shared/SimilarSchoolsComparison/SchoolDetails.cshtml", model);
    }
}
