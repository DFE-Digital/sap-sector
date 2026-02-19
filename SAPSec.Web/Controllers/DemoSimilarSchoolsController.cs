using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Web.Constants;
using SAPSec.Web.Helpers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Authorize]
[Route("school/{urn}/demo-similar-schools")]
public class DemoSimilarSchoolsController : Controller
{
    private readonly FindSimilarSchools _findSimilarSchools;

    public DemoSimilarSchoolsController(FindSimilarSchools findSimilarSchools)
    {
        _findSimilarSchools = findSimilarSchools;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);

        var filters = Request.Query
            .Where(kvp => kvp.Key != "page" && kvp.Key != "sort")
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value
                .Select(v => v ?? "")
                .Where(v => v.Length > 0)
                .ToList()
                .AsEnumerable());

        var currentPage = int.TryParse(Request.Query["page"], out int p) ? p : 1;
        var sort = Request.Query["sort"].ToString();

        var response = await _findSimilarSchools.Execute(new FindSimilarSchoolsRequest(urn, filters, sort, currentPage));

        // TODO: Handle properly with 404 response
        if (response == null)
        {
            return RedirectToAction("Error");
        }

        var path = Request.GetEncodedPathAndQuery();

        return View(new DemoSimilarSchoolsViewModel
        {
            Name = response.SchoolName,
            CurrentUrl = path.Contains("?") ? path : path + "?",
            SortOptions = response.SortOptions,
            FilterOptions = response.FilterOptions,
            ResultsPage = response.ResultsPage.Map(r => new DemoSimilarSchoolResultViewModel
            {
                URN = r.SimilarSchool.URN,
                Name = r.SimilarSchool.Name,
                Street = r.SimilarSchool.Address.Street,
                Locality = r.SimilarSchool.Address.Locality ?? "",
                Address3 = r.SimilarSchool.Address.Address3 ?? "",
                Town = r.SimilarSchool.Address.Town,
                Postcode = r.SimilarSchool.Address.Postcode,
                LAName = r.SimilarSchool.LocalAuthority.Name,
                Latitude = r.Coordinates.Latitude.ToString(),
                Longitude = r.Coordinates.Longitude.ToString(),
                UrbanRuralId = r.SimilarSchool.UrbanRural.Id,
                UrbanRuralName = r.SimilarSchool.UrbanRural.Name,
                SortValue = $"{r.SortValue.Name}: {r.SortValue.Value.Display("0.00")}"
            }),
            AllResults = response.AllResults.Select(r => new DemoSimilarSchoolResultViewModel
            {
                URN = r.SimilarSchool.URN,
                Name = r.SimilarSchool.Name,
                Street = r.SimilarSchool.Address.Street,
                Locality = r.SimilarSchool.Address.Locality ?? "",
                Address3 = r.SimilarSchool.Address.Address3 ?? "",
                Town = r.SimilarSchool.Address.Town,
                Postcode = r.SimilarSchool.Address.Postcode,
                LAName = r.SimilarSchool.LocalAuthority.Name,
                Latitude = r.Coordinates.Latitude.ToString(),
                Longitude = r.Coordinates.Longitude.ToString(),
                UrbanRuralId = r.SimilarSchool.UrbanRural.Id,
                UrbanRuralName = r.SimilarSchool.UrbanRural.Name,
                SortValue = $"{r.SortValue.Name}: {r.SortValue.Value.Display("0.00")}"
            })
        });
    }
}
