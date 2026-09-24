using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.AllThrough.Services;
using SAPSec.Web.Areas.AllThrough.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels.SimilarSchools;
using SAPSec.Web.Areas.Shared.ViewModels.School;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Areas.AllThrough.Controllers;

[Area("AllThrough")]
[Route("school/all-through/{urn}")]
[Authorize]
[RequireFeatureFlag(FeatureFlags.EnableAllThroughSchools)]
[RequireSchoolPhase(ExpectedSchoolPhase.AllThrough)]
public class SimilarSchoolsController(
        IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse> getSchoolInfoUseCase,
        IUseCase<FindPrimarySimilarSchoolsRequest, FindPrimarySimilarSchoolsResponse> findPrimarySimilarSchoolsUseCase,
        IUseCase<FindSecondarySimilarSchoolsRequest, FindSecondarySimilarSchoolsResponse> findSecondarySimilarSchoolsUseCase,
        IUseCase<GetAllThroughSimilarSchoolPhasesRequest, GetAllThroughSimilarSchoolPhasesResponse> getAllThroughSimilarSchoolPhasesUseCase,
        IAllThroughSimilarSchoolsTabUrlBuilder allThroughSimilarSchoolsTabUrlBuilder,
        IFeatureFlagService featureFlagService)
    : Controller
{
    [HttpGet]
    [Route("view-similar-schools")]
    public async Task<IActionResult> ViewSimilarSchools(
        string urn,
        [FromQuery] string? phase = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? page = null)
    {
        var schoolResponse = await getSchoolInfoUseCase.Execute(new(urn));
        await PopulateViewData(schoolResponse.School);

        var similarSchoolPhases = await GetSimilarSchoolPhasesAsync(urn);
        var selectedPhase = phase?.Equals("secondary", StringComparison.OrdinalIgnoreCase) == true
            ? "secondary"
            : "primary";
        var tabUrls = allThroughSimilarSchoolsTabUrlBuilder.Build(urn, selectedPhase, Request.Query);
        var selectedPhaseViewSimilarSchoolsUrl = BuildSelectedPhaseViewSimilarSchoolsUrl(urn, selectedPhase, Request.Query);

        SimilarSchoolsPageViewModel? primarySimilarSchools = null;
        SimilarSchoolsPageViewModel? secondarySimilarSchools = null;
        if (selectedPhase == "primary" && similarSchoolPhases.HasPrimary)
        {
            var filterBy = SimilarSchoolsPageViewModel.BuildCoreFilters(Request.Query);
            var response = await findPrimarySimilarSchoolsUseCase.Execute(new(
                urn,
                filterBy,
                sortBy,
                page));

            primarySimilarSchools = SimilarSchoolsPageViewModel.Build(
                "primary",
                Request.Query,
                response.CurrentSchool,
                response.SortOptions,
                response.FilterOptions,
                response.ResultsPage,
                response.AllResults,
                response.ValidationErrors,
                selectedPhaseViewSimilarSchoolsUrl,
                Routes.AllThroughSchool(urn).WhatIsASimilarSchool,
                comparatorUrn => Routes.AllThroughSchool(urn).PrimaryComparison(comparatorUrn).Similarity);
        }
        else if (selectedPhase == "secondary" && similarSchoolPhases.HasSecondary)
        {
            var filterBy = SimilarSchoolsPageViewModel.BuildCoreFilters(Request.Query);
            var response = await findSecondarySimilarSchoolsUseCase.Execute(new(
                urn,
                filterBy,
                sortBy,
                page));

            secondarySimilarSchools = SimilarSchoolsPageViewModel.Build(
                "secondary",
                Request.Query,
                response.CurrentSchool,
                response.SortOptions,
                response.FilterOptions,
                response.ResultsPage,
                response.AllResults,
                response.ValidationErrors,
                selectedPhaseViewSimilarSchoolsUrl,
                Routes.AllThroughSchool(urn).WhatIsASimilarSchool,
                comparatorUrn => Routes.AllThroughSchool(urn).SecondaryComparison(comparatorUrn).Similarity);
        }

        return View(AllThroughSimilarSchoolsPageViewModel.FromSchoolInfo(
            schoolResponse.School,
            selectedPhase,
            tabUrls.PrimaryTabUrl,
            tabUrls.SecondaryTabUrl,
            primarySimilarSchools,
            secondarySimilarSchools,
            similarSchoolPhases.HasPrimary,
            similarSchoolPhases.HasSecondary));
    }

    private async Task PopulateViewData(SchoolInfo currentSchool)
    {
        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchoolInfo(currentSchool);
        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreateAllThrough(
            Url,
            currentSchool.Urn,
            ControllerContext.ActionDescriptor.ActionName,
            await GetSimilarSchoolPhasesAsync(currentSchool.Urn),
            await IsRiseResourcesEnabledAsync());
    }

    private async Task<AllThroughSimilarSchoolPhases> GetSimilarSchoolPhasesAsync(string urn)
    {
        var response = await getAllThroughSimilarSchoolPhasesUseCase.Execute(new(urn));

        return new(response.HasPrimary, response.HasSecondary);
    }

    private async Task<bool> IsRiseResourcesEnabledAsync() =>
        featureFlagService is not null
        && await featureFlagService.IsEnabledAsync(FeatureFlags.EnableRiseResources);

    private static string BuildSelectedPhaseViewSimilarSchoolsUrl(
        string urn,
        string selectedPhase,
        IQueryCollection query)
    {
        var url = Routes.AllThroughSchool(urn).ViewSimilarSchools;
        var preservedQueryParts = new List<string>();

        if (selectedPhase == "secondary")
        {
            preservedQueryParts.Add("phase=secondary");
            AddStoredQueryParameter(query, preservedQueryParts, "primaryQuery");
        }
        else
        {
            AddStoredQueryParameter(query, preservedQueryParts, "secondaryQuery");
        }

        return preservedQueryParts.Count == 0
            ? url
            : $"{url}?{string.Join("&", preservedQueryParts)}";
    }

    private static void AddStoredQueryParameter(
        IQueryCollection query,
        List<string> queryParts,
        string key)
    {
        if (!query.TryGetValue(key, out var values))
        {
            return;
        }

        var value = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        queryParts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
    }
}
