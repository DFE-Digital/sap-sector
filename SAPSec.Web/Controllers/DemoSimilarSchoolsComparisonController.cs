using Lucene.Net.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;
using SAPSec.Web.Helpers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Authorize]
[Route("school/{urn}/demo-similar-schools/{similarSchoolUrn}")]
public class DemoSimilarSchoolsComparisonController : Controller
{
    private readonly GetSimilarSchoolDetails _getSimilarSchoolDetails;

    public DemoSimilarSchoolsComparisonController(GetSimilarSchoolDetails getSimilarSchoolDetails)
    {
        _getSimilarSchoolDetails = getSimilarSchoolDetails;
    }


    [HttpGet]
    public async Task<IActionResult> Index(string urn, string similarSchoolUrn)
    {
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);

        var response = await _getSimilarSchoolDetails.Execute(new GetSimilarSchoolDetailsRequest(urn, similarSchoolUrn));

        return View(new DemoSimilarSchoolsComparisonViewModel
        {
            Name = response.SchoolName,
            SimilarSchoolName = response.SimilarSchoolDetails.Name.Display(),
            CurrentSchoolLatitude = response.CurrentSchoolCoordinates.Latitude,
            CurrentSchoolLongitude = response.CurrentSchoolCoordinates.Longitude,
            SimilarSchoolLatitude = response.SimilarSchoolCoordinates.Latitude,
            SimilarSchoolLongitude = response.SimilarSchoolCoordinates.Longitude,
            Distance = response.DistanceMiles,
            SimilarSchoolDetails = response.SimilarSchoolDetails
        });
    }
}
