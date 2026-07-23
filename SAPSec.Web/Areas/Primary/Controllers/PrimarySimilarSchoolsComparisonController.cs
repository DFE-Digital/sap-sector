using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Primary.ViewModels;
using SAPSec.Web.Filters;

namespace SAPSec.Web.Areas.Primary.Controllers;

[Area("Primary")]
[Route("school/primary/{urn}/view-similar-schools/{similarSchoolUrn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Primary, "urn", "similarSchoolUrn")]
[RequireFeatureFlag(FeatureFlags.EnablePrimarySchools)]
public class PrimarySimilarSchoolsComparisonController(
    IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse> getSchoolInfoUseCase)
    : Controller
{
    [HttpGet]
    public Task<IActionResult> Index(string urn, string similarSchoolUrn) =>
        Similarity(urn, similarSchoolUrn);

    [HttpGet]
    [Route("Similarity")]
    public async Task<IActionResult> Similarity(string urn, string similarSchoolUrn)
    {
        var model = await BuildBaseModelAsync(urn, similarSchoolUrn);
        return View("Similarity", model);
    }

    [HttpGet]
    [Route("ks2")]
    public async Task<IActionResult> Ks2(string urn, string similarSchoolUrn)
    {
        var model = await BuildBaseModelAsync(urn, similarSchoolUrn);
        return View(model);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn, string similarSchoolUrn)
    {
        var model = await BuildBaseModelAsync(urn, similarSchoolUrn);
        return View(model);
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn, string similarSchoolUrn)
    {
        var model = await BuildBaseModelAsync(urn, similarSchoolUrn);
        return View(model);
    }

    private async Task<PrimarySimilarSchoolsComparisonViewModel> BuildBaseModelAsync(string urn, string similarSchoolUrn)
    {
        var currentSchool = (await getSchoolInfoUseCase.Execute(new GetSchoolInfoRequest(urn))).School;
        var similarSchool = (await getSchoolInfoUseCase.Execute(new GetSchoolInfoRequest(similarSchoolUrn))).School;

        return new PrimarySimilarSchoolsComparisonViewModel
        {
            Urn = urn,
            SimilarSchoolUrn = similarSchoolUrn,
            Name = currentSchool.Name,
            SimilarSchoolName = similarSchool.Name
        };
    }
}
