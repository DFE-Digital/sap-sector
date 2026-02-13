using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

public class SimilarSchoolsController : Controller
{
    /// <summary>
    /// GET: /school/{urn}/similar-schools
    /// </summary>
    [HttpGet]
    [Route("school/{urn:int}/similar-schools")]
    public IActionResult Index(
        int urn,
        [FromQuery] SimilarSchoolsFilterViewModel filters,
        [FromQuery] string sortBy = "Attainment 8",
        [FromQuery] int page = 1)
    {
        // Keep this route backward-compatible by forwarding to the SchoolController view.
        var url = Url.Action("ViewSimilarSchools", "School", new { urn });
        if (string.IsNullOrEmpty(url))
        {
            return RedirectToAction("ViewSimilarSchools", "School", new { urn, sortBy, page });
        }

        return Redirect(url + Request.QueryString);
    }
}
