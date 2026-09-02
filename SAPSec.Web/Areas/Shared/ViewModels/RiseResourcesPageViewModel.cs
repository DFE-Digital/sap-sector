using SAPSec.Core.Features.RiseResources;

namespace SAPSec.Web.Areas.Shared.ViewModels;

public sealed class RiseResourceItemViewModel
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public string? Url { get; init; }

    public bool HasLink => !string.IsNullOrWhiteSpace(Url);
}

public sealed class RiseResourceSubCategoryViewModel
{
    public required string Name { get; init; }
    public IReadOnlyList<RiseResourceItemViewModel> Resources { get; init; } = [];
}

public sealed class RiseResourceCategoryViewModel
{
    public required string Name { get; init; }
    public IReadOnlyList<RiseResourceSubCategoryViewModel> SubCategories { get; init; } = [];
}

public sealed class RiseResourcesPageViewModel
{
    public required string SchoolUrn { get; init; }
    public required string SchoolName { get; init; }

    /// <summary>Resources grouped by category, then by sub-category, preserving content-file order.</summary>
    public IReadOnlyList<RiseResourceCategoryViewModel> Categories { get; init; } = [];

    public bool HasResources => Categories.Count > 0;

    public static RiseResourcesPageViewModel FromResponse(GetRiseResourcesResponse response) =>
        new()
        {
            SchoolUrn = response.Urn,
            SchoolName = response.SchoolName,
            Categories = response.Resources
                .GroupBy(resource => resource.Category ?? string.Empty)
                .Select(categoryGroup => new RiseResourceCategoryViewModel
                {
                    Name = categoryGroup.Key,
                    SubCategories = categoryGroup
                        .GroupBy(resource => resource.SubCategory ?? string.Empty)
                        .Select(subCategoryGroup => new RiseResourceSubCategoryViewModel
                        {
                            Name = subCategoryGroup.Key,
                            Resources = subCategoryGroup
                                .Select(resource => new RiseResourceItemViewModel
                                {
                                    Title = resource.Title,
                                    Description = resource.Description,
                                    Url = resource.Url
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList()
        };
}
