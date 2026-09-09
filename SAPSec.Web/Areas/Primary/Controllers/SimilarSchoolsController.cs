using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Shared.ViewModels.SimilarSchools;
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
        var filterBy = SimilarSchoolsPageViewModel.BuildCoreFilters(Request.Query);

        var response = await findSimilarSchoolsUseCase.Execute(new(
            urn,
            filterBy,
            sortBy,
            page));

        await PopulateViewData(response.CurrentSchool);

        var viewModel = SimilarSchoolsPageViewModel.Build(
            "primary",
            Request.Query,
            response.CurrentSchool,
            response.SortOptions,
            response.FilterOptions,
            response.ResultsPage,
            response.AllResults,
            response.ValidationErrors,
            Routes.PrimarySchool(urn).ViewSimilarSchools,
            Routes.PrimarySchool(urn).WhatIsASimilarSchool,
            comparatorUrn => Routes.PrimarySchool(urn).Comparison(comparatorUrn).Similarity
        );

        return View(viewModel);
    }

    private async Task PopulateViewData(SchoolInfo currentSchool)
    {
        var includeRise = featureFlagService is not null
            && await featureFlagService.IsEnabledAsync(FeatureFlags.EnableRiseResources);

        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreatePrimary(
            Url,
            currentSchool.Urn,
            ControllerContext.ActionDescriptor.ActionName,
            includeRise);
    }
}