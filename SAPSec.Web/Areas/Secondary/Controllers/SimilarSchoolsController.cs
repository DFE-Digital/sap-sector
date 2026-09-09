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

namespace SAPSec.Web.Areas.Secondary.Controllers;

[Area("Secondary")]
[Route("school/secondary/{urn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Secondary)]
public class SimilarSchoolsController(
        IUseCase<FindSecondarySimilarSchoolsRequest, FindSecondarySimilarSchoolsResponse> findSimilarSchoolsUseCase,
        IFeatureFlagService featureFlagService) : Controller
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
            "secondary",
            Request.Query,
            response.CurrentSchool,
            response.SortOptions,
            response.FilterOptions,
            response.ResultsPage,
            response.AllResults,
            response.ValidationErrors,
            Routes.SecondarySchool(urn).ViewSimilarSchools,
            Routes.SecondarySchool(urn).WhatIsASimilarSchool,
            comparatorUrn => Routes.SecondarySchool(urn).Comparison(comparatorUrn).Similarity
        );

        return View(viewModel);
    }

    private async Task PopulateViewData(SchoolInfo currentSchool)
    {
        var includeRise = featureFlagService is not null
            && await featureFlagService.IsEnabledAsync(FeatureFlags.EnableRiseResources);

        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreateSecondary(
            Url,
            currentSchool.Urn,
            ControllerContext.ActionDescriptor.ActionName,
            includeRise);
    }
}
