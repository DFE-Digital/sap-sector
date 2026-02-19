using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Constants;
using SAPSec.Web.Helpers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Route("school/{urn}/view-similar-schools/{similarSchoolUrn}")]
public class SimilarSchoolsComparisonController : Controller
{
    private readonly GetSimilarSchoolDetails _getSimilarSchoolDetails;

    public SimilarSchoolsComparisonController(GetSimilarSchoolDetails getSimilarSchoolDetails)
    {
        _getSimilarSchoolDetails = getSimilarSchoolDetails;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index(string urn, string similarSchoolUrn)
    {
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        
        var response = await _getSimilarSchoolDetails.Execute(new GetSimilarSchoolDetailsRequest(urn, similarSchoolUrn));
        
        var model = new SimilarSchoolsComparisonViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = response.SchoolName,
            SimilarSchoolName = response.SimilarSchoolDetails.Name.Display(),
            CurrentSchoolLatitude = response.CurrentSchoolCoordinates.Latitude,
            CurrentSchoolLongitude = response.CurrentSchoolCoordinates.Longitude,
            SimilarSchoolLatitude = response.SimilarSchoolCoordinates.Latitude,
            SimilarSchoolLongitude = response.SimilarSchoolCoordinates.Longitude,
            Distance = response.DistanceMiles,
            SimilarSchoolDetails = response.SimilarSchoolDetails
        };

        SetComparisonSchoolViewData(model);

        return View(model);
    }

    
    
    [HttpGet]
    [Route("Ks4HeadlineMeasures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(string urn, string similarSchoolUrn)
    {
        var response = await _getSimilarSchoolDetails.Execute(new GetSimilarSchoolDetailsRequest(urn, similarSchoolUrn));
        
        var model = new SimilarSchoolsComparisonViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = response.SchoolName,
            SimilarSchoolName = response.SimilarSchoolDetails.Name.Display(),
            CurrentSchoolLatitude = response.CurrentSchoolCoordinates.Latitude,
            CurrentSchoolLongitude = response.CurrentSchoolCoordinates.Longitude,
            SimilarSchoolLatitude = response.SimilarSchoolCoordinates.Latitude,
            SimilarSchoolLongitude = response.SimilarSchoolCoordinates.Longitude,
            Distance = response.DistanceMiles,
            SimilarSchoolDetails = response.SimilarSchoolDetails
        };

        SetComparisonSchoolViewData(model);
        return View(model);
    }

    [HttpGet]
    [Route("Ks4CoreSubjects")]
    public async Task<IActionResult> Ks4CoreSubjects(string urn, string similarSchoolUrn)
    {
        var response = await _getSimilarSchoolDetails.Execute(new GetSimilarSchoolDetailsRequest(urn, similarSchoolUrn));
        
        var model = new SimilarSchoolsComparisonViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = response.SchoolName,
            SimilarSchoolName = response.SimilarSchoolDetails.Name.Display(),
            CurrentSchoolLatitude = response.CurrentSchoolCoordinates.Latitude,
            CurrentSchoolLongitude = response.CurrentSchoolCoordinates.Longitude,
            SimilarSchoolLatitude = response.SimilarSchoolCoordinates.Latitude,
            SimilarSchoolLongitude = response.SimilarSchoolCoordinates.Longitude,
            Distance = response.DistanceMiles,
            SimilarSchoolDetails = response.SimilarSchoolDetails
        };

        SetComparisonSchoolViewData(model);
        return View(model);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn, string similarSchoolUrn)
    {
        var response = await _getSimilarSchoolDetails.Execute(new GetSimilarSchoolDetailsRequest(urn, similarSchoolUrn));
        
        var model = new SimilarSchoolsComparisonViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = response.SchoolName,
            SimilarSchoolName = response.SimilarSchoolDetails.Name.Display(),
            CurrentSchoolLatitude = response.CurrentSchoolCoordinates.Latitude,
            CurrentSchoolLongitude = response.CurrentSchoolCoordinates.Longitude,
            SimilarSchoolLatitude = response.SimilarSchoolCoordinates.Latitude,
            SimilarSchoolLongitude = response.SimilarSchoolCoordinates.Longitude,
            Distance = response.DistanceMiles,
            SimilarSchoolDetails = response.SimilarSchoolDetails
        };
        
        SetComparisonSchoolViewData(model);

        return View(model);
    }

    [HttpGet]
    [Route("SchoolDetails")]
    public async Task<IActionResult> SchoolDetails(string urn, string similarSchoolUrn)
    {
        var response = await _getSimilarSchoolDetails.Execute(new GetSimilarSchoolDetailsRequest(urn, similarSchoolUrn));
        
        var model = new SimilarSchoolsComparisonViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = response.SchoolName,
            SimilarSchoolName = response.SimilarSchoolDetails.Name.Display(),
            CurrentSchoolLatitude = response.CurrentSchoolCoordinates.Latitude,
            CurrentSchoolLongitude = response.CurrentSchoolCoordinates.Longitude,
            SimilarSchoolLatitude = response.SimilarSchoolCoordinates.Latitude,
            SimilarSchoolLongitude = response.SimilarSchoolCoordinates.Longitude,
            Distance = response.DistanceMiles,
            SimilarSchoolDetails = response.SimilarSchoolDetails
        };
        
        SetComparisonSchoolViewData(model);

        return View(model);
    }

    
    private void SetComparisonSchoolViewData(SimilarSchoolsComparisonViewModel data)
    {
        ViewData["ComparisonSchool"] = data;
    }
    
}