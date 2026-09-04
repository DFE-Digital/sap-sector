using SAPSec.Core.Features.RiseResources;

namespace SAPSec.Web.Areas.Shared.ViewModels;

internal static class RiseResourceSlug
{
    public static string From(string value)
    {
        var slug = new string([.. value.Trim().ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')]);

        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }
}

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

    /// <summary>In-page anchor id for the "Contents" jump links.</summary>
    public string Slug => RiseResourceSlug.From(Name);
}

public sealed class RiseResourceCategoryViewModel
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<RiseResourceSubCategoryViewModel> SubCategories { get; init; } = [];

    public string Slug => RiseResourceSlug.From(Name);
}

public sealed class RiseResourcesPageViewModel
{
    public required string SchoolUrn { get; init; }
    public required string SchoolName { get; init; }

    /// <summary>
    /// Categories and sub-categories in first-appearance order from the content file's
    /// <c>resourceEntries</c>. Resource links within a sub-category are ordered alphabetically by title.
    /// </summary>
    public IReadOnlyList<RiseResourceCategoryViewModel> Categories { get; init; } = [];

    /// <summary>Every sub-category across all categories, in display order — the "Contents" entries.</summary>
    public IEnumerable<RiseResourceSubCategoryViewModel> ContentsEntries =>
        Categories.SelectMany(category => category.SubCategories)
            .Where(subCategory => !string.IsNullOrWhiteSpace(subCategory.Name));

    public bool HasResources => Categories.Count > 0;

    public static RiseResourcesPageViewModel FromResponse(GetRiseResourcesResponse response) =>
        new()
        {
            SchoolUrn = response.Urn,
            SchoolName = response.SchoolName,
            Categories = [.. response.Categories
                .Select(category => new RiseResourceCategoryViewModel
                {
                    Name = category.Name,
                    Description = category.Description,
                    SubCategories = [.. category.Resources
                        .GroupBy(resource => resource.SubCategory ?? string.Empty)
                        .Select(subCategoryGroup => new RiseResourceSubCategoryViewModel
                        {
                            Name = subCategoryGroup.Key,
                            Resources = [.. subCategoryGroup
                                .OrderBy(resource => resource.Title, StringComparer.CurrentCultureIgnoreCase)
                                .Select(resource => new RiseResourceItemViewModel
                                {
                                    Title = resource.Title,
                                    Description = resource.Description,
                                    Url = resource.Url
                                })]
                        })]
                })]
        };
}
