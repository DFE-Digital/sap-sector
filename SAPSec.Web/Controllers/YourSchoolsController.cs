using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Authentication;
using SAPSec.Core.Features.AssociatedSchools;
using SAPSec.Web.Authorization;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

/// <summary>
/// Lists the schools associated with a trust or local authority user's organisation.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.TrustOrLocalAuthorityUser)]
[Route("your-schools")]
public class YourSchoolsController(
    IUserService userService,
    IAssociatedSchoolsService associatedSchoolsService) : Controller
{
    private const int PageSize = 10;

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] int? page = null)
    {
        var organisation = await userService.GetCurrentOrganisationAsync(User);
        if (organisation is null)
        {
            return Forbid();
        }

        var schools = await associatedSchoolsService.GetAssociatedSchoolsAsync(organisation);

        var totalPages = Math.Max(1, (int)Math.Ceiling((double)schools.Count / PageSize));
        var currentPage = Math.Clamp(page ?? 1, 1, totalPages);

        return View(new YourSchoolsViewModel
        {
            OrganisationName = organisation.Name,
            OrganisationTypeDescription = organisation.IsTrust ? "trust" : "local authority",
            CurrentPage = currentPage,
            PageSize = PageSize,
            TotalResults = schools.Count,
            PaginationBasePath = Routes.YourSchools,
            Results = schools
                .Skip((currentPage - 1) * PageSize)
                .Take(PageSize)
                .Select(SchoolSearchResultViewModel.From)
                .ToArray(),
            AllResults = schools.Select(SchoolSearchResultViewModel.From).ToArray()
        });
    }
}
