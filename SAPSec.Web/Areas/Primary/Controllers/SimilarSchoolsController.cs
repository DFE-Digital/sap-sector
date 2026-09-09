using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Primary.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels.School;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Areas.Primary.Controllers;

[Area("Primary")]
[Route("school/primary/{urn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Primary)]
[RequireFeatureFlag(FeatureFlags.EnablePrimarySchools)]
public class SimilarSchoolsController(
        IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse> getSchoolInfoUseCase,
        IUseCase<FindPrimarySimilarSchoolsRequest, FindPrimarySimilarSchoolsResponse> findSimilarSchoolsUseCase,
        IFeatureFlagService featureFlagService)
    : Controller
{
    [HttpGet]
    [Route("view-similar-schools")]
    public async Task<IActionResult> ViewSimilarSchools(
        string urn,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? page = null)
    {
        var schoolInfoResponse = await getSchoolInfoUseCase.Execute(new(urn));
        var filterBy = PrimarySimilarSchoolsPageViewModel.ExtractCurrentFilters(Request.Query)
            .ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<string>)kvp.Value, StringComparer.InvariantCultureIgnoreCase);
        var response = await findSimilarSchoolsUseCase.Execute(new(
            urn,
            filterBy,
            sortBy,
            page));

        await PopulateViewData(schoolInfoResponse.School);

        return View(PrimarySimilarSchoolsPageViewModel.FromResponse(response, Request.Query));
    }

    private async Task PopulateViewData(SchoolInfo currentSchool)
    {
        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchoolInfo(currentSchool);

        var includeRise = featureFlagService is not null
            && await featureFlagService.IsEnabledAsync(FeatureFlags.EnableRiseResources);

        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreatePrimary(
            Url,
            currentSchool.Urn,
            ControllerContext.ActionDescriptor.ActionName,
            includeRise);
    }
}